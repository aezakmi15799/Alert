# แผนรันยันเช้า 2026-11-04 → 05 (จาก cloud session, ตามที่เจ้าของมอบสิทธิ์ A10)

## คำตอบตรง
- "ครบทุกระบบ ตรง PC ภายในเช้า" ทำไม่ได้ ตัวเลขที่วัดได้ตอนนี้ (GAP_SCAN_1104): ไม่มี 181 (packet 136 → เหลือ 119 หลัง batch 3+B1) / ไม่แน่ใจ 3,524 / มีคู่ 235
- คืนนี้ทำได้เฉพาะงานที่ "ไม่ต้องรอเจ้าของ และไม่ต้องรอเซิร์ฟ/มือถือจริง" คือ อ่าน decomp → พอร์ต → คอมไพล์ → จดตาราง
- งานที่ต้องรอเจ้าของ (ทำคืนนี้ไม่ได้): เทสในเกมบน iPhone, PC+มือถือพร้อมกัน, smoke C2, merge/push/ลบ, RC build ipa, ยืนยันการย้าย RayCityDeploy
- ตอนเช้าสิ่งที่จะได้คือ: ตัวเลข GAP_SCAN ลดลง (วัดได้), โค้ดพอร์ตค้างบน branch รอเทส, ตารางครบ, รายการสิ่งที่เจ้าของต้องตัดสิน

## กติกาคืนนี้ (เหมือนเดิม ไม่ผ่อน)
1. ห้าม merge main/trunk, push, ลบไฟล์/worktree, แตะ _secret, อัป VPS
2. ห้ามใช้เซิร์ฟเวอร์/ClientPC จนกว่าเจ้าของยืนยันเรื่องย้าย RayCityDeploy ไป E:
3. ทุก function ที่พอร์ตต้องมี `// PC 0x… file:line` ไม่มี [อนุมาน] ค่าคงที่จาก exe เท่านั้น (PROMPT_DECOMP_PARITY)
4. ทุก session เขียนผลลง docs/NIGHT_1103.md ส่วนของตัวเอง ทุก 2 ชม. (บรรทัด: เวลา, ทำอะไร, commit, ตัวเลขก่อน/หลัง)
5. ถ้าติดอะไรที่ต้องตัดสิน ให้จดเป็นคำถามใน NIGHT_1103 แล้วทำงานถัดไป ไม่หยุดรอ

## งานแยกตาม session (เรียงตามลำดับที่ต้องทำ)

### -90 (server owner)
1. รายงานการย้าย RayCityDeploy ตามที่สั่ง (ตรวจอย่างเดียว) → NIGHT_1103
2. SUMMARY.md ของ smoke 2236 ถ้ายังไม่มี เขียนเองจาก log
3. GAP_SCAN: ไล่กลุ่ม "ไม่แน่ใจ 3,524" ด้วย orig_values (function ↔ constant) ให้ย้ายเป็น ตรง/ต่าง/ไม่มี ให้ได้มากที่สุด เป้าคืนนี้ = ลดไม่แน่ใจลงอย่างน้อย 1,000 แถว (วัดด้วย tools/gap_scan.py ก่อน/หลัง) เริ่มจากระบบ Login → ร้าน → ไอเทม ตามลำดับ README §6
4. Traffic round 2: พอร์ต 0x63BF40 → 0x6336E0 → 0x455290 → 0x6314E0 เทียบกับ oracle exe (รันในเครื่องได้ ไม่ใช้เซิร์ฟจริง) จด diff ต่อฟังก์ชัน
5. LE RaceAgent flow (Agents/RaceAgent.Match*.cs) พอร์ตจาก decomp คอมไพล์ + unit test 416 ต้องยังผ่าน
6. ไม่ต้องทำ: hex check, DLL switch, smoke รอบใหม่ (รอเจ้าของ)
7. 04:00 ตรวจ bundle มี e2436be4 แล้วจด

### session มือถือ (game-f8)
1. กลุ่ม B ที่เหลือ (28 ตัว ยกเว้น PqUnuseSkill ที่รอ hasSkillCancel) พอร์ตจาก decomp คอมไพล์ผ่าน ทุกตัว commit แยก ลงตาราง PACKETS_136
2. UI_PORT_BACKLOG round 1 เต็มตามสเปก (T1 ไม่มี + T3 หน้าจอ + GAP_SCAN UI + จอกลุ่ม D 51) นับยอดที่หัวตาราง
3. กลุ่ม D เริ่มตามลำดับหน้าจอ: ห้องปาร์ตี้ → จับคู่มือใหม่ → แชท 1:1 แต่ละจอ = layout จาก PC + packet คู่ของมัน คอมไพล์ผ่าน ยังไม่เทส
4. ไม่ต้องทำ: hex, เปิด ClientPC, เทสในเกม

### Xtreme / LE / aicar-perf agents (ถ้ายังเปิดอยู่)
- Xtreme: ปิดแถว [G] 37 แถว ด้วย decomp เท่านั้น (ไม่ใช้เกม) อันไหนต้องเล่นจริง จดว่า "รอ slot"
- LE: matchSetting.xml [G] → หาใน client_decomp/ไฟล์ PC ว่ามีจริงไหม จดผล
- aicar-perf: AICAR_PERF_1104.md จากค่าที่วัดแล้ว ไม่วัดเพิ่ม (ต้องใช้เซิร์ฟ) และ restore traffic.xml = 12 ถ้ายังไม่ได้ทำ

## สิ่งที่เจ้าของต้องตอบตอนเช้า (เรียงตามผลกระทบ)
1. การย้าย RayCityDeploy → E: ใช่ของคุณไหม / เสร็จไหม (ปลดล็อก hex, smoke, ทุกอย่างที่ใช้เซิร์ฟ)
2. merge: packets-136-docs 98bb90c6, charm-rain-cell (หลัง hex), และ branch โค้ดคืนนี้หลังเทส
3. push ก/ข/ค และ ลบ worktree 43 ตัว (G: เหลือ ~5 GB build อาจเต็ม)
4. OWNER_DECISIONS ชุดเดิม: app เก่า 438643/438649, วันออก app ใหม่, PC hook, level 490000000, Traffic oracle, RUNEvent/RUNBonus/matchSetting, LE fee
5. ผล RC-1104 บน iPhone S01–S10 + phone_perf log
