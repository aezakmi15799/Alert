# Handoff — RayCity server_cs (งานที่เหลือ) สำหรับเซสชันถัดไป — 2026-11-01

คัดลอกไฟล์นี้ทั้งไฟล์ไปเป็นข้อความแรกของเซสชันใหม่ได้เลย แล้วแนบไฟล์ที่ระบุในหัวข้อ "ไฟล์ที่ต้องแนบ"

---

## 1. บริบท (อ่านก่อนทำอะไร)

- โปรเจกต์: เซิร์ฟเวอร์ MMO **RayCity** เขียนใหม่เป็น C# (.NET 8) ชื่อ `server_cs` อยู่ที่ `G:\RayCityServer_src` (เครื่องเจ้าของ)
  โปรเจกต์ย่อย: `RayCity.Core`, `RayCity.Protocol` (Packets.g.cs / Client1627.g.cs สร้างจาก client), `RayCity.Physics`, `RayCity.GameServer`
  Agent ทุกตัว (Race/Chat/Messenger/Auction/Traffic) รันในโปรเซส GameServer เดียว ใช้ **MariaDB** (ไม่ใช่ SQL Server, ไม่มี stored procedure)
- client: **PC 1627 (Raycity.exe)** และ **แอปมือถือ** ใช้ protocol 1627 **ชุดเดียวกัน** → แก้ที่เซิร์ฟเวอร์ครั้งเดียวได้ทั้งคู่ ห้ามทำทางที่ PC ได้แต่มือถือไม่ได้
- ตัวรันต้นฉบับ (GameServer.exe ฯลฯ ใน orig_servers.rar) **ไม่รัน** เพราะต้องใช้ SQL Server + SP 469 ตัว ซึ่งขาด 172 ตัว → เลิกทางนี้ ใช้เป็น**เอกสารอ้างอิง**เท่านั้น
- ทีมมี pipeline ถอด binary อยู่แล้ว: Ghidra → `tools/lift.py` → `tools/make_source.py` (ผลิตไฟล์แบบ `client_decomp/src/GoNpcActionTalk.cpp`) ใช้กับ server .exe ได้แบบเดียวกัน **ทีละฟังก์ชันตาม address** (ถอดทั้งก้อนใหญ่เกินส่ง)
- packet id = adler32(ชื่อ packet, seed 0)
- deploy: `G:\RayCityDeploy\Server\` มี `run_server.bat` (env flag ทั้งหมดอยู่ในนั้น), log ที่ `logs\`
  build: `dotnet build` ใน `G:\RayCityServer_src` (ตอนนี้ 0 error, 18 warning เดิม)
- เจ้าของสื่อสารภาษาไทย ตอบสั้น ตรงประเด็น ให้โค้ดเต็มไฟล์/คำสั่ง PowerShell ที่วางได้เลย (ไม่มี `-Recurse` ใน Select-String รุ่นเขา; `mysql` ไม่อยู่ใน PATH ใช้ `C:\Program Files\MariaDB*\bin\mariadb.exe`)

## 2. ไฟล์ที่ต้องแนบให้เซสชันใหม่

| ไฟล์ | ใช้ทำอะไร |
|---|---|
| `TODO_TEST_1101.md` | **รายการงาน 16 ข้อ + วิธีเทสในเกม T1–T8 + ผลถอด client เรื่อง NPC กลับที่เดิม** (ไฟล์หลัก) |
| `PATCH_1101.md` | โค้ดแพตช์ชุด 11-01 ที่ใส่ไปแล้ว (อ้างอิงว่าแก้อะไรไปแล้ว) |
| `GAP_SEND.md` | 220 packet ที่ต้นฉบับส่งแต่เรายังไม่ส่ง |
| `CLIENT_READ_MISSING.md` | layout ฝั่ง client ของ 109 packet ที่ Protocol ยังไม่มี Read |
| `MISSION_SYSTEM_ORIG.md`, `MISSION_NPC_GIVEUP_1101.md` | พฤติกรรมภารกิจของต้นฉบับ (state 1–6, handler address) |
| `gs_handlers.json` | สารบัญ handler ของ GameServer ต้นฉบับ (ชื่อ packet → address) |
| `docs/orig_packets/GameServer.md`, `RaceAgent.md`, `DIFF.md` | packet ที่ต้นฉบับแต่ละตัวส่ง/รับ |
| `RCRB_Missions.json`, `missionlist_db.txt`, `npc_look.txt` | ข้อมูลภารกิจ/NPC |
| source ล่าสุด: `RayCity.GameServer` (LoginHandlers*.cs, Missions.cs, ClientSession.cs, MobileRelay.cs), `RayCity.Protocol` | โค้ดจริง |
| `run_server.bat` ปัจจุบัน + log รอบเทสล่าสุด | |

## 3. สิ่งที่ทำเสร็จแล้ว (อย่าทำซ้ำ)

- รับ packet จาก client ครบ (เทียบ GameServer 291 / RaceAgent 94 handler ต้นฉบับแล้ว) ยกเว้น Facebook 2 ตัว (ไม่ใช้)
- ภารกิจเดี่ยวครบวงจรบน local (คุย → รับ → เริ่ม → ยอมแพ้ → คุยใหม่ → สำเร็จ → รางวัล)
- แพตช์ 11-01 (build ผ่าน): A1 MqAcceptCancel ไม่ทำอะไร, A2/A4 จำ key ที่ปฏิเสธ (`MissionAcceptFailKey`), A3 "McFailed anyway" ปิด (flag `RC_MCFAILED_ANYWAY=1`),
  A5 `UpdateNpcView` ไม่ลบ NPC ใน `s.MissionNpcTaken`, A7 bat แก้ `EXTRA`, B1 `PcPartyBoost` ส่งต่อปาร์ตี้ (field F10/F14/F18/F1C/F20), B2 `PcCarUpgrade` → PcChatMsg ทุกคน
- NPC ภารกิจไม่แดงทั้งแผนที่: `MissionNpcInfo` ใส่ `F34C = 1, F34D = 0xFF` (client ใช้ช่วงเลเวล (1,0xFF) = ไม่จำกัด แล้วลงสีตามส่วนต่างเลเวล >+5 เหลือง >+10 แดง)
- `gamedefinition.missionlist` import 1127 แถว
- ยืนยันแล้วว่าทุกจุดจบภารกิจ (10 จุดที่เรียก `NpcBack`) ส่ง McFailed/MrSuccess ก่อนเสมอ

## 4. ข้อสรุปสำคัญที่ต้องยึด (หลักฐานมีใน TODO_TEST_1101.md ส่วนที่ 3)

- **NPC ภารกิจกลับที่เดิมเป็นงานของ client ทั้งหมด** (GoNpcActionTaxi state 6/9→13 ลงรถ+จางหาย, GoNpcActionTalk state 7 เดินกลับจุดที่จำไว้ใน +0x94 list) เซิร์ฟเวอร์ต้นฉบับไม่ส่งอะไรเลย
  → เซิร์ฟเวอร์ **ห้าม** ส่ง `PcDelNpc` ของ NPC ในรถระหว่างภารกิจ และ **ห้าม** ส่ง `PcAddNpc` ซ้ำตอนจบ ต้องส่ง McFailed/McSuccess ทุกทางจบ
- `RC_MISSION_NPC_BACK=1` (ส่ง PcAddNpc ซ้ำ = NPC วาร์ปกลับทันที) **เป็นของเราเอง ไม่ใช่ต้นฉบับ** เป้าหมายคือใช้ `0` ให้ได้ (A6)
- ยังเป็นสมมติฐาน: A5 ครอบทุกจุดลบ NPC แล้วหรือยัง (จุดลบอื่น: `LoginHandlers.cs` ~บรรทัด 717 ลบ `SpawnedNpcs` ทั้งหมดตอนเปลี่ยน field)

## 5. งานที่ต้องทำ เรียงลำดับ (รายละเอียด+วิธีเทสอยู่ใน TODO_TEST_1101.md ส่วนที่ 1–2)

1. **รอ/อ่านผลเทส T3** ที่ `RC_MISSION_NPC_BACK=0`: ถ้า NPC กลับเอง → ตั้ง 0 ถาวร; ถ้าไม่ ใช้ตารางอาการ 3.4 หาสาเหตุ
   งานย่อยที่ทำได้เลย: เพิ่ม log `kept mission NPC {id:x} in view` ใน `UpdateNpcView`, กัน `MissionNpcTaken` ตอนเปลี่ยน field (บรรทัด ~717), คง id ใน `want` ต่ออีก ~10 วิหลังจบ
2. ภารกิจบอส/ปาร์ตี้ type 8–11: ลบ NPC ออกจาก field + `PcBossMissionNpcState` / `PcPartyMissionNpcState` + เกิดใหม่ (ต้นฉบับ GS `0x6E1A50`, `0x684350`)
3. `MqStart` ปาร์ตี้รอทุกคน, `McTeamChanged` 2 จุด, เวลาเพิ่มของทีม (+0x9E0), ค่าธรรมเนียม MqStart (GS `0x678410`, `0x678550`)
4. เควสต่อเนื่อง `QrReward` → QuestInfo ถัดไป + เติม `rc_quest_chainquestlist` (ตอนนี้ 3 แถว)
5. แข่ง C1 `PcRaceUpdateTP` (ทุก PqRaceCheckPoint/PqRaceGoalIn; layout `F20` + list `L14` มีแล้ว; ต้นฉบับ RaceAgent `0x4458B0`)
6. แข่ง C2 `PcUpdateOverralRaceResult` ตอนเลือกตัวละคร (layout 4 กลุ่ม×3 u32; ข้อมูล `gamedb00.rc_characterstate`)
7. แข่ง C3 `PcRaceRankResult` (RaceAgent `0x44D1F0`; client helper `FUN_0046e8d2`), C4 `PcRaceVictoryInfo` (`0x4426B0`; client อ่านที่ stage+0x1e0 ใน `FUN_00C0B4B8`) — ต้องถอด layout ก่อน
8. C5 `PcFieldRaceStart`
9. D field/ตัวละคร: `PcUpdateNpc`, `PcPlaySound`, `PcUpdateDisplayCar`, `PrSelectCar`, `PrGetInventory`, `PcGetCarUpgradeInfo` (เช็ก log ก่อน บางตัวอาจส่งอยู่แล้วใต้ชื่ออื่น)
10. D เควส/NPC: `PcNotifyAvailableQuests`, `PcMakeGeQuestNpcDialog`, `PcEventNpcList`, `PcNpcShopState`
11. D ปาร์ตี้: `PcUpdatePartyInfo`, `PcNotifyTermParty`, `PcPartyMemberReward`, `PcInitPartyRoom`, `PcUpdatePartyRoomMemeber`
12. D คลับ `PcUpdateGuildMessage`; สอนเล่น `PrTutorStepClear/Fail`; Xtreme `PcRUNInfo`, `PcRUNApplyCount`, `PcUpdateRUNBonusEvent`
13. ใส่ flag ปิด packet ที่เราส่งแต่ต้นฉบับไม่ส่ง: PcPlayEventLog, PcQuestAutoCompletePoint, PcShutdownEvent, PcUpdateBonusPoint, PcUpdateTimeChange, PrAskRefundFee, PrBonusCapsule, PrHitPlayAloneBoss, PrPlayAlonePickItem, PrPlayAloneStart, PrResetSocket, PrUpdatePlayEvent
14. เติม DB จาก datasub ของ client 1627: rc_quest_chainquestlist, luckyspotlist, carskill, rc_getjunctionlist/spline, dealquestlist, randomcapsulemachinelist, roadfight*
    + หาสาเหตุ 18 ภารกิจที่ JSON มีแต่ DB ไม่มี: 1220,1222,1352,1354,1383,1386,1387,1396,1435,1436,1520,1523,1524,1656,1662,1669,40013,40020 (ขอ `import_skip.txt`)
15. มือถือ: เอาข้อยกเว้น `IsMobile` ใน `UpdateNpcView` ออกเมื่อแอปเก่าหมด; ทุก packet ใหม่ข้อ 5–12 เช็กว่าแอปมี handler (log แอป "no handler") และ `MobileRelay.cs` log `... by udp from the phone - no handler`
16. ขึ้น VPS ทีละระบบที่ผ่านเทส (`PUBLIC_IP` ใน bat)

**ไม่ทำ:** รัน .exe เดิม / SQL Server / SP; ระบบที่ข้อมูลว่าง 92 packet (Match/LE, Zone Tournament, PvE, CTZ, Deal, บิงโก, Contest, กันโกง, Facebook, PcBang, AntiAddict)

## 6. สิ่งที่ยังขาด (ต้องขอจากเจ้าของ)

| ขาด | ใช้ทำข้อ | วิธีได้มา |
|---|---|---|
| log + อาการเทส T3 ที่ NPC_BACK=0, รูป NPC 3 ตัว + เลเวลตัวละคร | 1 | เทสตาม TODO_TEST_1101.md T2/T3 |
| ถอด GameServer.exe: `0x66C630` `0x66C790` `0x6E1A50` `0x684350` `0x6661D0` `0x678410` `0x678550` | 1–3 | pipeline lift.py/make_source.py ทีละฟังก์ชัน |
| ถอด RaceAgent.exe: `0x4458B0` `0x44D1F0` `0x4426B0` | 5, 7 | เหมือนกัน |
| client decomp: `FUN_0046e8d2`, `FUN_00C0B4B8` และ Read ของ PcRaceRankResult / PcRaceVictoryInfo | 7 | `client_decomp/src` |
| `import_skip.txt` | 14 | tool import ของทีม |
| datasub 1627 ของ client (ตารางใน ข้อ 14) | 14 | โฟลเดอร์ client |
| log แอปมือถือรอบเทส T8 | 15 | |

## 7. วิธีทำงานที่ใช้ได้ผล (ทำตาม)

- ทำ**ทีละระบบ**: แก้ → build → เจ้าของเทสในเกมตามตาราง T ในไฟล์ → อ่าน log → ค่อยไประบบถัดไป ห้ามอ้างว่าเสร็จก่อนมี log ยืนยัน
- ทุก handler/packet ใหม่ใส่ `[Handler, Gap(GapKind.X, "...")]` ระบุ address ต้นฉบับ และถ้าเป็นของเราเองให้มี env flag ปิด/เปิด (รูปแบบ `RC_...`)
- log ที่ต้องหาทุกรอบ: `no handler`, `handler ... failed`, `WARN`, `NPC view`, `mission over:`, `-> refused`
- เปิด `RC_PACKET_LOG=1` ตอนเทส (ลงไฟล์เท่านั้น), `RC_PACKET_HEX=ชื่อ,ชื่อ` ถ้าต้องดู byte
- layout packet ที่ Protocol ยังไม่มี: ถอดจาก client Read/Write (client_decomp) ไม่เดา; ถ้า class ซ้ำ (เช่น `PcPartyBoost`) ใช้ของ Packets.g.cs เดิม
- `PacketLog` ไม่มี `Warn` → ใช้ `log.Info($"WARN ...")`

---

## 8. สถานะจากเซสชัน Opus (11-01 14:32 local build รวมสองฝั่งแล้ว)

| ข้อ | สถานะ |
|---|---|
| A1–A5, A7, B1, B2, สี NPC | อยู่ในโค้ดครบ |
| A6 NPC_BACK=0 | bat ตั้ง 0 แล้ว รอเทส T3 ข้อ 9 → ผ่านแล้วค่อยลบโค้ด "สร้าง NPC id ใหม่" ของ Opus |
| 11 ปาร์ตี้ | agent ปาร์ตี้ทำ 31 แถว (docs/orig_port/party.md) — ต้องเช็กว่าครอบ PcUpdatePartyInfo, PcNotifyTermParty, PcPartyMemberReward, PcInitPartyRoom, PcUpdatePartyRoomMemeber ครบ |
| 12 PcUpdateGuildMessage | ทำแล้ว |
| 15 มือถือ | แอปที่ส่ง PqMoveField ได้ NPC แบบช่องทันที (ไม่รอแอปเก่าหมด) — แอปแก้แล้ว ยังไม่ออกแพตช์ |
| 2–10, 13, 14, 16 | ยังไม่ทำ — Opus จะทำ 2–4 ต่อ |

กติกาเมื่อมีสองเซสชันแก้โค้ดเดียวกัน: ไฟล์หนึ่งมีเจ้าของเดียวต่อรอบ (Missions/LoginHandlers.Missions.cs = Opus ตั้งแต่ตอนนี้), ทุกรอบ build ต้อง 0 error ก่อนเปิดเซิร์ฟ, แพตช์ที่ใช้ flag ให้คง flag ไว้จนเทสผ่านค่อยลบโค้ด

## 9. แบ่งงานสองเซสชัน (11-01 บ่าย)
- **Opus**: ภารกิจ (ข้อ 2–4) + ปาร์ตี้ `PcUpdatePartyInfo`, `PcPartyMemberReward` — เจ้าของ Missions.cs, LoginHandlers.Missions.cs, LoginHandlers.Party.cs
- **เซสชันนี้ (Fable)**: แข่ง C1–C4 (ข้อ 5–7) จาก docs/orig_dumps_1101.md — เจ้าของไฟล์ RaceAgent / LoginHandlers.RaceModes.cs / LoginHandlers.Character.cs (C2)
- ข้อ 14: import_skip.txt ไม่มีจริง → ใช้ tools/datasub.py อ่าน datasub missionlist แล้วเทียบ 18 id ตรง ๆ
- เอกสารจาก Opus: docs/orig_dumps_1101.md (ฟังก์ชันถอด 9 ตัว), docs/REPLY_1101_FOR_OTHER_SESSION.md

## 10. ภาพรวมจาก Opus (11-01 เย็น) + คำแนะนำ 3 ข้อตัดสินก่อนขึ้น VPS
handler ฝั่งเกม 402 ตัว: มีสถานะแล้ว 342 (ตรงอยู่แล้ว 135 / แก้แล้ว 105 / รอเจ้าของเลือก 76 / ของเราตั้งใจ 26) ที่เหลือ = ภารกิจ/เควส (Opus ดูแล)
**"แก้แล้ว 105" = ระดับโค้ด ยังไม่ได้เทสในเกม** → ก่อนขึ้น VPS ต้องผ่านตาราง T ใน TODO_TEST_1101.md ทีละระบบ

| เรื่อง | ทางเลือก | แนะนำ |
|---|---|---|
| 1 แข่ง: ต้นฉบับตัดผู้เล่นทันทีเมื่อข้าม checkpoint / เวลาไม่ตรง | ต้นฉบับ vs ผ่อน | **ผ่อนเป็นค่าเริ่มต้น**: ไม่นับ checkpoint นั้น + log `PcRaceHackSuspect` นับครั้ง; ตัดเมื่อเกิน N ครั้งต่อแข่ง (N=3) ; flag `RC_RACE_STRICT=1` = ต้นฉบับ เหตุผล: ต้นฉบับออกแบบสำหรับ PC สายแลน มือถือ UDP กระตุกจะโดนตัดบ่อย ใช้กฎเดียวกันทั้ง PC/มือถือ (ไม่แยกฝั่ง) |
| 2 เพื่อน: รหัสสถานะเราไม่ตรงต้นฉบับ (ต้นฉบับ 1=เพื่อน, เรา 6) | แก้ทั้งระบบ vs คงไว้ | **แก้ตอนนี้** เพราะยังเป็น DB ทดสอบ (ถูกสุดที่จะย้าย) ทำเป็น 1 migration script + แก้แอปพร้อมกัน ถ้าปล่อยไป ทุก packet ที่อ้างสถานะเพื่อนในอนาคตต้องแปลงค่าไปตลอด |
| 3 ความปลอดภัย: PcCheat*, ส่ง packet แทนคนอื่น, เตะผู้เล่น ต้นฉบับไม่ตรวจสิทธิ์ | ตามต้นฉบับ vs GM เท่านั้น | **GM เท่านั้น** (ของเรา, ใส่ `Gap(GapKind.OwnDesign)`) ไม่ลอกช่องโหว่ต้นฉบับ |

มือถือ: ระบบที่แก้ฝั่งเซิร์ฟแล้วแอปต้องตาม (แข่ง checkpoint/PcRaceHackSuspect/PcRaceUpdateTP/PqEnterRace, ของขวัญ, คลับ, โทรโข่ง) → **แพตช์แอปต้องออกวันเดียวกับเซิร์ฟขึ้น VPS** และคง fallback `IsMobile` ใน UpdateNpcView ไว้จนแอปใหม่ครบ
ข้อ ASK อีก ~70 ข้ออยู่ใน docs/orig_port/*.md — เจ้าของไล่ตอบทีละไฟล์ ไม่ต้องตอบหมดก่อนเทส
ลำดับถัดไปของ Opus: (1) PcPlayCarMove น้ำมัน/สึกหรอ/โบนัสระยะทาง, ซื้อรถ/ของ, โหวตมือใหม่ (2) ภารกิจ/เควสที่เหลือ (3) แพตช์มือถือ

## 11. แข่ง C2–C4 (Fable, 11-01 เย็น)
- C2 ทำแล้ว: `patch_1101b/` (AccountStore.RaceStats.cs + LoginHandlers.RaceStats.cs + 1 บรรทัดใน LoginHandlers.cs) ลำดับกลุ่มเป็น**สมมติฐาน** ต้องเทส T6 ข้อ 4 (ชนะเดี่ยว 1 รอบ ดูช่องไหนขยับ)
- C3/C4 ยังทำไม่ได้: dump RaceAgent ที่ได้เป็นแบบ "names only" (ไม่มี argument) และ 0x4426B0 ใช้ `FUN_0044cd10` เติม PcRaceVictoryInfo / `FUN_0044ccc0` เติม PcRaceResult / `FUN_0044d180` init PcRaceRankResult — ต้องถอด 3 ตัวนี้ + `PcRaceVictoryInfo::Write`, `PcRaceRankResult::Write` แบบเต็ม (มี argument) และฝั่ง client `PcRaceVictoryInfo::Read`, `PcRaceRankResult::Read`, handler ที่ใช้ struct 0x74 ไบต์จาก FUN_0046e8d2 (ชื่อ field), handler ของ PcUpdateOverralRaceResult (ชื่อช่องที่แสดง)
- ที่รู้จาก FUN_0046e8d2 (client): รายการหนึ่งของ PcRaceRankResult = 3×{u32, wstring, u32} + 2×{u32, u32, wstring, u32} + u32×10 + u8×4 + u32 (รวม struct 0x74) — ยังไม่รู้ว่ามีกี่รายการและชื่ออะไร
- จาก 0x4426B0: ต้นฉบับส่ง **PcRaceVictoryInfo ทุกครั้ง** ที่จบแข่ง (ถ้า +0x1dc != 1) และส่ง PcRaceResult เพิ่มเฉพาะเมื่อ +0x1d6 != 0; ของเราส่งแต่ PcRaceResult → client stage ไป state 0xc ได้เพราะเช็กแค่ typeId != VictoryInfo ต้องรู้ว่า +0x1d6/+0x14c คือโหมดอะไรก่อนแก้

## 12. C3/C4 — ผลอ่าน orig_dumps_1101b.md (Fable, 11-01 ค่ำ)
**C2**: Opus แก้ลำดับกลุ่มแล้วตามหลักฐาน GS 0x56EE1F + SP GetRaceState (0 field เดี่ยว, 1 field ทีม, 2 เดี่ยว+ไอเทมเดี่ยว, 3 ทีม+ไอเทมทีม) build ผ่าน → เหลือเทสหน้าตัวละคร

**C4 PcRaceVictoryInfo** — รู้แล้ว 90%:
- body = **โครงเดียวกับ PcRaceResult** ทุกไบต์ (0x44CCC0 สร้าง Result ด้วย ctor เดียวกัน 0x44CD10; 0x44CC00 คัดลอก body จาก VictoryInfo → Result; Read/Write ใช้ 0x44C790/0x44C7C0 ร่วมกัน) = `WriteRoomInfo` + รายการทีม + รายการผู้เล่น + u32 ท้าย ที่ `EndRace` เขียนอยู่แล้ว
- 0x4426B0 สร้าง VictoryInfo เสมอ แล้วแยกตาม race `+0x1D6` (ตั้งตอนสร้างห้อง 0x43D920 จาก param_4[0x22] = byte ที่ 0x22 ของ struct ตั้งค่าห้อง):
  `+0x1D6 == 0` → เก็บ VictoryInfo เป็นผลลัพธ์ (race+0x190), 0x443DE0 สุ่มการ์ดให้เอง, ไม่มี PcRaceResult
  `+0x1D6 != 0` → สร้าง PcRaceResult (copy body) + 0x443390 (ส่งผล+การ์ด) + timer 0x183C/0x1F40 — นี่คือทางที่เราทำอยู่
- **ยังไม่รู้**: `FUN_004400f0(race)` ที่เรียกทันทีหลังสร้าง VictoryInfo — ส่ง VictoryInfo ให้ห้องหรือไม่ และ byte 0x22 ของ struct ตั้งค่าห้องคือ option อะไร (น่าจะ "มีการ์ดรางวัล") → ถ้า 0x4400F0 ส่งจริง: เพิ่ม `ToRoom(room, Raw("PcRaceVictoryInfo", <writer เดิมของ PcRaceResult>))` ก่อน PcRaceResult 1 บรรทัด
- client: stage state 0xB เช็ก `typeId != PcRaceVictoryInfo` ถึงจะไป state 0xC (กล่องผลลัพธ์+การ์ด) → ถ้าส่ง VictoryInfo **หลัง** Result จะทับและค้าง ต้องส่ง**ก่อน**เท่านั้น

**C3 PcRaceRankResult** — layout แน่นอนแล้ว (ไฟล์ `patch_1101b/PcRaceRankResult.layout.cs`), ความหมายได้บางส่วน:
- ส่ง **คนละ 1 packet** ตอนจบ (0x443F10 loop ผู้เล่น → 0x44D1F0) ไม่ใช่ broadcast
- struct 0x74: 3×{u32, wstring, u32} (Top-3 ของสนาม: อันดับ/ชื่อ/เวลา), 2×{u32,u32,wstring,u32} (2 รายการรอบตัวผู้เล่น?), +0x48 = charId, +0x4C = เวลาแข่งนี้ (racer+0x50), +0x60/64/68 = min(เวลานี้, สถิติเดิม) 3 ค่า (racer +0x50 vs +0x5C, +0x54 vs +0x60, +0x58 vs +0x64), +0x6D/6E/6F = ทำสถิติใหม่ 3 ค่า, +0x88 = DAT_007E8254 (global)
- SP ที่เกี่ยว: `GetTrackRankingTOP1000` (RaceAgent, มี SQL ใน orig_sp_INDEX) / `ResetTrackRanking` (ไม่มี SQL) → **ต้องมีตารางสถิติสนาม** ซึ่ง server_cs ยังไม่มี (มีแค่ตัวนับ rc_characterstate) = งานใหญ่กว่า packet: ตาราง + บันทึกเวลาต่อสนาม + query top3/รอบตัว
- ยังขาด: `FUN_0044d350`, `FUN_004bff70`, `FUN_004bce40`, `FUN_0044d0e0`, `FUN_0046a0f0`, ความหมาย racer +0x50..+0x64 (เวลา 3 แบบ = รวม/รอบ/เซกเตอร์?), `DAT_007E8254`, ใครเรียก 0x443F10 (ผ่าน 0x4CF1E0) และหน้าต่าง client ที่อ่าน PcRaceRankResult (+0x14..) เพื่อตั้งชื่อ field, SQL ของ GetTrackRankingTOP1000
