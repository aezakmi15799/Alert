# server_cs — สิ่งที่ต้องทำล่าสุด + วิธีเทสในเกม (สรุป 2026-11-01 เย็น)

ใช้ไฟล์นี้สั่งงานรอบถัดไปได้เลย: **ส่วนที่ 1 = งานที่ต้องทำ (เรียงลำดับ)**, **ส่วนที่ 2 = วิธีเทสเปิดเกม ทีละระบบ / ทีละ packet**,
**ส่วนที่ 3 = ผลวิเคราะห์ NPC กลับที่เดิมจาก client decomp** (GoNpcActionTalk / GoNpcActionTaxi ที่ส่งมา)

หลักเดิม: PC กับมือถือใช้ protocol 1627 ชุดเดียวกัน → แก้ที่เซิร์ฟเวอร์ครั้งเดียวได้ทั้งคู่

---

## ส่วนที่ 1 — งานที่ต้องทำ (ลำดับทำจริง)

### สถานะตอนนี้ (build ผ่านแล้ว 11-01, 0 error)
- [x] A1–A4 ภารกิจ (AcceptCancel ไม่ทำอะไร / จำ key ที่ปฏิเสธ / McFailed anyway ปิด / TalkAsync จำ key)
- [x] A5 UpdateNpcView ไม่ลบ NPC ใน `MissionNpcTaken`
- [x] A7 run_server.bat แก้ `EXTRA`
- [x] B1 PcPartyBoost ส่งต่อปาร์ตี้, B2 PcCarUpgrade ประกาศแชต
- [x] NPC ภารกิจไม่แดงทั้งแผนที่ (`F34C=1, F34D=0xFF`) — **ยังไม่ได้เทสในเกม**
- [x] missionlist import 1127 แถว
- [ ] **A6 เทส `RC_MISSION_NPC_BACK=0`** ว่า NPC กลับเองหลังแก้ A5 (ดูส่วนที่ 3 ว่าต้องดูอะไรใน log)
- [ ] import_skip.txt — 18 ภารกิจที่ JSON มีแต่ DB ไม่มี (1220,1222,1352,1354,1383,1386,1387,1396,1435,1436,1520,1523,1524,1656,1662,1669,40013,40020)

### งานถัดไป (ทำตามลำดับ ทำเสร็จข้อหนึ่ง → เทสตามส่วนที่ 2 → ค่อยข้อต่อไป)
| ลำดับ | งาน | ไฟล์ | หมายเหตุ |
|---|---|---|---|
| 1 | เทสชุด 11-01 ทั้งหมด (T1–T4 ส่วนที่ 2) + ส่ง log | — | ถ้า T3 ผ่านที่ `NPC_BACK=0` → ตั้ง 0 ถาวร |
| 2 | ภารกิจบอส/ปาร์ตี้ (type 8–11): ลบ NPC ออกจาก field + `PcBossMissionNpcState` / `PcPartyMissionNpcState` + เกิดใหม่ | LoginHandlers.Missions.cs, Missions.cs | ต้นฉบับ 0x6E1A50 / 0x684350 |
| 3 | `MqStart` ปาร์ตี้รอทุกคน, `McTeamChanged` 2 จุด, เวลาเพิ่มของทีม, ค่าธรรมเนียม MqStart | LoginHandlers.Missions.cs | จาก MISSION_SYSTEM_ORIG.md |
| 4 | เควสต่อเนื่อง: `QrReward` แล้วส่ง QuestInfo ถัดไป | LoginHandlers.Quests.cs | DB rc_quest_chainquestlist มี 3 แถว → เติมข้อมูลด้วย |
| 5 | C1 `PcRaceUpdateTP` ทุก PqRaceCheckPoint + PqRaceGoalIn | RaceAgent (ใน GameServer) | layout มีแล้ว (`F20` + list `L14`) |
| 6 | C2 `PcUpdateOverralRaceResult` ตอนเลือกตัวละคร | LoginHandlers.Character.cs | ข้อมูล `gamedb00.rc_characterstate` |
| 7 | C3 `PcRaceRankResult`, C4 `PcRaceVictoryInfo` ตอนจบแข่ง | RaceAgent | ต้องถอด layout จาก client decomp (FUN_0046e8d2 / FUN_00C0B4B8) |
| 8 | C5 `PcFieldRaceStart` แข่งใน field | — | ยังไม่ดู |
| 9 | D field/ตัวละคร: `PcUpdateNpc`, `PcPlaySound`, `PcUpdateDisplayCar`, `PrSelectCar`, `PrGetInventory`, `PcGetCarUpgradeInfo` | LoginHandlers.*.cs | เช็กก่อนว่าบางตัวส่งอยู่แล้วใต้ชื่ออื่น |
| 10 | D เควส/NPC: `PcNotifyAvailableQuests`, `PcMakeGeQuestNpcDialog`, `PcEventNpcList`, `PcNpcShopState` | LoginHandlers.Quests.cs | |
| 11 | D ปาร์ตี้: `PcUpdatePartyInfo`, `PcNotifyTermParty`, `PcPartyMemberReward`, `PcInitPartyRoom`, `PcUpdatePartyRoomMemeber` | LoginHandlers.Party.cs | |
| 12 | D คลับ `PcUpdateGuildMessage`, สอนเล่น `PrTutorStepClear/Fail`, Xtreme `PcRUNInfo/ApplyCount/UpdateRUNBonusEvent` | | |
| 13 | ที่เราส่งแต่ต้นฉบับไม่ส่ง → ใส่ flag ปิด: PcPlayEventLog, PcQuestAutoCompletePoint, PcShutdownEvent, PcUpdateBonusPoint, PcUpdateTimeChange, PrAskRefundFee, PrBonusCapsule, PrHitPlayAloneBoss, PrPlayAlonePickItem, PrPlayAloneStart, PrResetSocket, PrUpdatePlayEvent | | |
| 14 | E เติม DB: rc_quest_chainquestlist, luckyspotlist, carskill, junction/spline, dealquestlist, randomcapsulemachinelist, roadfight* | tool แปลง datasub 1627 | |
| 15 | มือถือ: เอาข้อยกเว้น `IsMobile` ใน UpdateNpcView ออก + เช็ก "no handler" ฝั่งแอปสำหรับ packet ใหม่ข้อ 5–12 | LoginHandlers.cs, MobileRelay.cs | |
| 16 | ขึ้น VPS ทีละระบบที่ผ่านเทส | run_server.bat `PUBLIC_IP` | |

ไม่ทำ: ตัวรัน .exe เดิม + SQL Server + SP (ติด SP 172 ตัว) ; Match/LE/Zone/PvE/CTZ/Deal/บิงโก/Contest/กันโกง/Facebook (92 packet ข้อมูลว่าง)

---

## ส่วนที่ 2 — วิธีเทสเปิดเกม ทีละระบบ / ทีละ packet

### 2.0 เตรียมก่อนเทสทุกครั้ง
1. `run_server.bat` ตั้ง (เฉพาะตอนเทส):
   ```bat
   set RC_PACKET_LOG=1
   set RC_MISSION_NPC_PLAIN=0
   set RC_MISSION_NPC_BACK=0
   ```
   `RC_PACKET_LOG=1` = เขียนทุก packet รับ/ส่งลงไฟล์ log (ไม่ขึ้นจอ; อยากขึ้นจอด้วยใส่ `RC_PACKET_CONSOLE=1`)
   ถ้าอยากเห็น byte ของ packet ไหน เพิ่มชื่อใน `RC_PACKET_HEX=...` (คั่นด้วย , )
2. ลบ/ย้าย log เก่าใน `logs\` ออกก่อน จะได้ส่ง log เฉพาะรอบนั้น
3. รันเซิร์ฟ → เข้าเกม PC (start_client.bat หรือผ่าน launcher) → เลือกตัวละคร → เข้า field
4. ทุกเทส: จด **เวลา** ที่ทำแต่ละขั้น (ดูนาฬิกา) จะได้หาใน log ได้
5. หลังเทส ส่ง: ไฟล์ log รอบนั้น + รูปหน้าจอตอนผิด + บอกว่า "ข้อไหนผ่าน/ไม่ผ่าน"

### 2.1 วิธีอ่าน log — บรรทัดที่ต้องหาทุกรอบ
| หา (Select-String / ctrl+F) | หมายความว่า |
|---|---|
| `no handler` | client ส่ง packet ที่เราไม่รับ → **ต้องเพิ่ม handler** (จดชื่อ packet ส่งมา) |
| `handler ... failed` | handler พัง (exception) → ส่งบรรทัดนั้นมาทั้งบรรทัด |
| `WARN` | ทางที่ไม่ควรเกิด (เช่น MqGiveUp หา mission ไม่เจอ) |
| `NPC view 0x` | เพิ่ม/ลบ NPC ตามช่อง 3x3 (+N / -N) |
| `mission over:` | จบภารกิจ — บอกว่า NPC ถูก "left to the client" (NPC_BACK=0) หรือส่งกลับ (NPC_BACK=1) |
| `-> refused` | เซิร์ฟปฏิเสธคำขอ (MqStart/MqSuccess) |

PowerShell ที่ใช้ได้ (ไม่มี -Recurse):
```powershell
Select-String -Path "G:\RayCityDeploy\Server\logs\*.log" -Pattern "no handler|handler .* failed|WARN" | Select-Object -Unique Line
```

### T1 เข้าเกม / field (ทุกครั้ง)
| ขั้น | ทำในเกม | ต้องเห็นใน log | ผ่านเมื่อ |
|---|---|---|---|
| 1 | login → เลือกตัวละคร | `PqSelectCharacter` … `PrSelectCharacter`, ไม่มี `handler failed` | เข้าถึง field ได้ |
| 2 | ขับข้ามช่องแผนที่ 2–3 ช่อง | `NPC view 0x...: +N -M` | NPC รอบตัวโผล่/หาย ไม่กระพริบ |
| 3 | เปิดช่องเก็บของ / รายการรถ | `PqGetInventory`, `PqGetCarList` | ของครบ |
| 4 | ยืนเฉย 30 วิ | ไม่มี `no handler` ใหม่ | — |

### T2 สี/หัว NPC ภารกิจ (แก้ F34C/F34D)
| ขั้น | ทำในเกม | ดู | ผ่านเมื่อ |
|---|---|---|---|
| 1 | ไปหา NPC ภารกิจ 3 ตัว: เลเวลต่ำกว่าเรา / ใกล้เรา / สูงกว่าเรา >10 | จอ | ต่ำ/ใกล้ = สีปกติ, สูง >5 = เหลือง, >10 = แดง (ไม่ใช่แดงหมด) |
| 2 | ถ่ายรูป 3 ตัว + บอกเลเวลตัวละคร | — | ส่งรูปมา |
ถ้ายังแดงหมด: ส่ง log บรรทัด `NPC view` + รูป → จะเช็ก `F32` (เลเวล NPC) ที่ส่ง

### T3 ภารกิจเดี่ยว — รอบหลัก (เทส A1–A6 ทั้งหมด) ที่ `RC_MISSION_NPC_BACK=0`
ทำตามลำดับนี้ **ทีละอย่าง** และจดเวลา:
| ขั้น | ทำในเกม | packet | ต้องเห็นใน log | ผ่านเมื่อ |
|---|---|---|---|---|
| 1 | คุยกับ NPC ภารกิจ | MqTalkNpc → MrMakeMission | `MqTalkNpc mission npc ... ->`, `MqMakeMission ... -> MrMakeMission` | หน้าต่างภารกิจขึ้น |
| 2 | กด **ยกเลิก** ในหน้าต่าง | MqAcceptCancel | `MqAcceptCancel ... ignored` | ไม่ค้าง |
| 3 | คุย NPC **ตัวอื่น** ทันที | MqTalkNpc | `drops the unstarted mission` แล้ว MrMakeMission ใหม่ | **คุยได้** (บั๊กเดิม = คุยไม่ได้) |
| 4 | กดรับภารกิจ แต่ **ปฏิเสธ** (F18=0) | MqAccept 0 | `MqAccept ... declined -> MrMakeMissionFail` | ไม่ค้าง, คุยตัวอื่นได้ |
| 5 | คุย NPC เดิม → รับภารกิจ | MqAccept → MrAccept | `MqAccept ... -> MrAccept ok` | NPC ขึ้นรถ |
| 6 | ขับออกไป **ไกลหลายช่อง** (ให้ข้ามช่อง 3x3) | PqMoveField | `NPC view ... -N` **ต้องไม่มี id ของ NPC ภารกิจในรายการที่ลบ** | NPC ยังอยู่ในรถ |
| 7 | เริ่มภารกิจ | MqStart → MrStart | `MqStart ... -> MrStart` | นับเวลาเริ่ม |
| 8 | **ยอมแพ้** กลางทาง | MqGiveUp → McFailed | `MqGiveUp ... reason N`, `mission over: NPC ... left to the client` | หน้าจอกลับปกติ |
| 9 | **ดู NPC**: มันลงจากรถแล้ววิ่งกลับจุดเดิมเองไหม (รอ ~10 วิ) | ไม่มี packet | ไม่มี `PcAddNpc` ใหม่ของ NPC ตัวนั้น | **NPC กลับที่เดิมเอง = A6 ผ่าน** |
| 10 | คุย NPC เดิมอีกครั้ง → รับ → ทำจนจบ | MqSuccess → MrSuccess, MqReward | `all objectives reached -> MrSuccess`, reward | ได้รางวัล |
| 11 | ดู NPC หลังสำเร็จ | — | — | กลับที่เดิมเอง |
| 12 | ส่ง MqGiveUp ซ้ำ (กดยอมแพ้ตอนไม่มีภารกิจ ถ้าทำได้) | MqGiveUp | **ต้องไม่มี** `WARN ... no such mission` | — |

ถ้า **ข้อ 9 ไม่ผ่าน** (NPC ไม่กลับ / หายไปเลย) ให้ส่ง log + ตอบ 3 ข้อ: (ก) NPC ลงจากรถไหม (ข) ยืนนิ่งที่ที่ลง หรือหายไปเลย (ค) ตอนขับกลับไปจุดเดิมมี NPC อยู่ไหม — ดูส่วนที่ 3 ว่าแต่ละอาการแปลว่าอะไร
ถ้าผ่าน → ตั้ง `RC_MISSION_NPC_BACK=0` ถาวรใน bat (= ตรงต้นฉบับ)

### T4 ปาร์ตี้ (B1) + อัปเกรดรถ (B2) — ต้องมี 2 client
| ขั้น | ทำในเกม | ต้องเห็นใน log | ผ่านเมื่อ |
|---|---|---|---|
| 1 | ตั้งปาร์ตี้ 2 คน | `PqCreateParty` / `PqJoinParty` | ทั้งคู่เห็นปาร์ตี้ |
| 2 | คนหนึ่งใช้ booster ปาร์ตี้ | `PcPartyBoost item N '...' -> 1 party member(s)` | อีกคนเห็นข้อความ "Booster มีผลต่อปาร์ตี้" |
| 3 | อัปเกรดรถจนสำเร็จ | `PcCarUpgrade grade N '...' -> chat to N player(s)` | ทุกคนเห็นประกาศในแชต; ถ้าขึ้นผิดช่องแชต บอกช่องที่ขึ้น (จะเปลี่ยน FC 8→10) |

### T5 ภารกิจปาร์ตี้/บอส (หลังทำงานข้อ 2–3) — 2 client
| ขั้น | ทำ | ดู log | ผ่านเมื่อ |
|---|---|---|---|
| 1 | หัวหน้าคุย NPC บอส → เลือกสมาชิก | `MqMakeMission ... client chose [...]` | สมาชิกได้หน้าต่าง |
| 2 | ทั้งคู่รับ | `accepted, waiting for the party` → `MrAccept ok` | เริ่มพร้อมกัน |
| 3 | สมาชิกคนหนึ่งออก | `party mission ...: '...' ...` | อีกคนได้ McTeamChanged / ภารกิจยังไป หรือจบตามต้นฉบับ |
| 4 | จบภารกิจ | `MrWaitSuccess` → `10 s countdown over` | ทั้งคู่ได้รางวัล, NPC บอสหายแล้วเกิดใหม่ |

### T6 แข่ง (หลังทำ C1–C4) — 2 client
| ขั้น | ทำ | ดู log | ผ่านเมื่อ |
|---|---|---|---|
| 1 | สร้างห้องแข่ง → เริ่ม | `PqRaceReady` … `PcRaceStart` | ออกตัวพร้อมกัน |
| 2 | ผ่าน checkpoint | `PqRaceCheckPoint` → `PcRaceUpdateTP` | หน้าจอ TP/อันดับอัปเดต |
| 3 | เข้าเส้นชัย | `PqRaceGoalIn` → `PcRaceUpdateTP`, `PcRaceRankResult`, `PcRaceVictoryInfo` | หน้าจบแข่งขึ้นครบ (อันดับ/รางวัล) — **บอกว่าหน้าจบแข่งเป็นยังไง** |
| 4 | กลับมาเลือกตัวละครใหม่ | `PcUpdateOverralRaceResult` | สถิติแข่งรวมโชว์ถูก |

### T7 เควส (หลังทำข้อ 4/10)
| ขั้น | ทำ | ดู log | ผ่านเมื่อ |
|---|---|---|---|
| 1 | คุย NPC เควส (F30=0) | `QqTalkNpc` → QuestInfo | หน้าต่างเควสขึ้น |
| 2 | ทำเควสจนจบ → รับรางวัล | `QqReward` → `QrReward` + QuestInfo ถัดไป | เควสต่อเนื่องขึ้นทันที |
| 3 | เดินใกล้ NPC ที่มีเควสใหม่ | `PcNotifyAvailableQuests` | ไอคอนเควสบนหัว NPC |

### T8 มือถือ (ทำหลัง PC ผ่านทุกข้อ)
ทำ T1, T3, T6 ซ้ำบนแอป แล้วหาใน log: `mobile relay ...: ... by udp from the phone - no handler` → packet ที่แอปส่งแต่เราไม่รับ
ฝั่งแอป: ดู log แอป "no handler" สำหรับ packet ใหม่ (PcRaceUpdateTP, PcRaceRankResult, PcUpdateOverralRaceResult ...)

### รายการ packet ทั้งหมดที่ยังไม่ส่ง (220 ตัว) อยู่ใน `GAP_SEND.md` — เมื่อทำตัวไหนเสร็จ เทสด้วยวิธีเดียวกัน: ทำ action ในเกมที่ทำให้ต้นฉบับส่ง → หาใน log ว่า "sent <ชื่อ>" ขึ้น → ดูจอว่า client แสดงผล

---

## ส่วนที่ 3 — NPC ภารกิจกลับที่เดิม: ผลวิเคราะห์จาก client (GoNpcActionTalk.cpp / GoNpcActionTaxi.c)

สรุปสั้น: **client พา NPC กลับเอง ไม่ต้องการ packet จากเซิร์ฟเวอร์** — มีเงื่อนไขเดียวคือ object NPC ตัวนั้นต้อง**ยังอยู่ใน client** ตลอดภารกิจ
(ห้ามส่ง PcDelNpc id นั้น) และ**ไม่ต้องส่ง PcAddNpc ซ้ำ** ตรงกับที่ต้นฉบับไม่ส่งอะไรเลย (`RC_MISSION_NPC_BACK=0`) และ patch A5 คือสิ่งที่ขาด

### 3.1 ตอน NPC อยู่ในรถ — GoNpcActionTaxi (state ที่ +0x11)
| state | ทำอะไร | ไปต่อ |
|---|---|---|
| 1–3 | NPC เดินมาขึ้นรถ (ใช้ตำแหน่งรถ +0x48 = รถผู้เล่น GoPlayCar) | |
| 4 | นั่งในรถ (คัดลอกตำแหน่งรถมาที่ NPC ทุกเฟรม) | ครบ 1 วิ: ถ้าคนขับยังเป็น GoPlayCar → 5, ไม่ใช่ → 13 (จบ) |
| 5 | "ถึงแล้ว/รอจบ" (`FUN_0069638b` คืน 1 = taxi จบ) | |
| 6 / 9 | จบภารกิจ: 6 = สำเร็จ, 9 = ล้มเหลว (`FUN_0069642b(param)` เรียกจาก handler McSuccess/McFailed ของ client) | 0.5 วิ → 7 / 10 |
| 7 / 10 | NPC ลงจากรถ (ตำแหน่ง +0xb8 ข้างรถ, ค่อย ๆ ปรับความสูง 1.2–2 วิ) | ครบเวลา +0x18 → 8 / 11 |
| 8 / 11 | ยืนข้างรถ (เล่นท่า) | ครบเวลา → 12 |
| 12 | fade 2 วิ (+0x64 = alpha) | → 13 |
| 13 | จบ taxi (`FUN_00696504` คืน true) → `FUN_006963c2`: ปลด NPC ออกจากรถ (dynamic_cast รถ → GoPlayCar แล้ว `FUN_0062985b` = เอา NPC ออกจาก slot ผู้โดยสาร) | NPC กลับไปใช้ action Talk |

### 3.2 หลังลงรถ — GoNpcActionTalk (state +0x11)
| state | ทำอะไร |
|---|---|
| 3 | ถูกคุย/ถูกจอง (vfunc34: ตอนเข้า state 3 **จำตำแหน่งเดิม**ลงรายการ +0x94..+0x98 ด้วย `FUN_0065056c`) |
| 4 | รอขึ้นรถ: `FUN_00692f8a` ล้มเหลว หรือ +0xa8==0 → ไป 7 |
| 5 | `FUN_00692ae6` ล้มเหลว → 7 |
| 6 | ระยะถึงจุดบ้าน (+0x50) ≤ ค่าคงที่ → ถือว่าถึง, สั่ง "วางกลับ" ผ่าน `FUN_00693e20` |
| 7 | **เดินกลับ**ไปจุด +0x68 (vfunc7 บรรทัด 200–225: ถึงแล้ว set ตำแหน่ง = +0x68) แล้วหมุนหน้าไปทาง +0x50 (บรรทัด 234–270) |
| vfunc32/33 | ดึงตำแหน่งที่จำไว้ (+0x94 list) กลับมาเป็นจุดบ้านเมื่อ taxi จบ (`FUN_00693ed1` หา → `FUN_0068d837` ตั้ง) |

**ทุกอย่างทำบน object NPC ตัวเดิมที่อยู่ใน client** รายการตำแหน่งเดิม (+0x94) อยู่ใน object นั้น ถ้า object ถูกทำลาย (เพราะเซิร์ฟส่ง PcDelNpc) = ไม่มีใครเดินกลับ และ
ถ้าเซิร์ฟส่ง PcAddNpc ซ้ำ (NPC_BACK=1 ของเรา) client จะสร้าง object **ใหม่** ที่จุดเกิด → "กลับ" แบบวาร์ป ไม่ใช่เดินกลับ

### 3.3 สิ่งที่เซิร์ฟเวอร์ต้องทำ / ห้ามทำ (ให้ตรง client = ตรงต้นฉบับ)
1. **ห้าม** ส่ง `PcDelNpc` ของ NPC ที่อยู่ในรถ (`s.MissionNpcTaken`) ตลอด state 2–4 ของภารกิจ → ทำแล้ว (A5)
   - ตรวจใน log: ระหว่างภารกิจ บรรทัด `NPC view ... -N` ต้องไม่ทำให้ NPC ตัวนั้นหาย — ถ้าอยากชัวร์ เพิ่ม log ว่า "kept mission NPC xxx in view"
2. **ห้าม** ส่ง `PcAddNpc` ซ้ำตอนจบ (= `RC_MISSION_NPC_BACK=0`) — client ลงรถ/เดินกลับเองตาม 3.1–3.2
3. **ต้อง** ส่ง `McSuccess` / `McFailed` ให้ client ตัวนั้นทุกครั้งที่ภารกิจจบ (ไม่ว่าจบแบบไหน: ยอมแพ้ / หมดเวลา / ออกจาก field / ตัดการเชื่อมต่อแล้วกลับมา) เพราะ taxi state 6/9 เริ่มจากตรงนี้ — ถ้าไม่ส่ง NPC จะนั่งในรถค้าง
   - จุดที่ต้องเช็กในโค้ดเรา: ทุกที่ที่เรียก `NpcBack(...)` คือจุดจบภารกิจ → ทุกจุดต้องมี `Send(McFailed/McSuccess)` ก่อนหน้า (ไล่ดู LoginHandlers.Missions.cs บรรทัด 365, 386, 392, 453, 525, 537, 542, 572, 579 และ Party.cs 413)
4. หลัง `MissionNpcTaken = 0` (จบแล้ว) UpdateNpcView กลับมาลบ/เพิ่มตามช่องปกติได้ — แต่**อย่าลบทันทีในวินาทีแรก** เพราะ NPC ยังใช้เวลา ~4–5 วิ ลงรถ+fade (state 7→12) ถ้าผู้เล่นยืนอยู่นอกช่อง 3x3 ของจุดเกิด NPC จะถูกลบระหว่าง fade — อาการนี้ไม่เป็นไร (ต้นฉบับก็เป็น) แต่ถ้าอยากเนียน ให้คง id ไว้ใน `want` ต่ออีก 10 วิหลังจบ
5. ถ้าผู้เล่น**เปลี่ยน field / ออกเกม** ระหว่างภารกิจ: เซิร์ฟต้องถือว่าภารกิจล้มเหลว (McFailed) และ NPC ตัวนั้นกลับเป็น NPC ปกติบนเซิร์ฟ (ให้คนอื่น/ตัวเองคุยได้) — เช็กใน log ว่ามี `mission ... failed` ตอน `PqLeaveField`

### 3.4 อาการ vs สาเหตุ (ใช้ตอนเทส T3 ข้อ 9)
| อาการ | แปลว่า | แก้ |
|---|---|---|
| NPC ลงจากรถ ยืน 2 วิ จางหาย แล้วไปโผล่ที่จุดเดิม | **ถูกต้อง** (state 7→12→Talk 7) | — |
| NPC นั่งในรถค้าง ไม่ลง | client ไม่ได้ McSuccess/McFailed (3.3 ข้อ 3) | หาใน log ว่าส่งหรือไม่ |
| NPC หายไปเลยกลางภารกิจ (ก่อนจบ) | ถูก PcDelNpc (A5 ยังไม่ครอบจุดนั้น) | ส่งบรรทัด `NPC view -N` ตอนนั้นมา |
| ลงรถแล้วยืนนิ่งที่นั่น ไม่กลับ | Talk ไม่เข้า state 7 (จุดบ้าน +0x68 ไม่ได้ตั้ง = vfunc34 ไม่ได้จำ) → มักเพราะ NPC ถูกสร้างใหม่ (PcAddNpc ซ้ำ) ระหว่างภารกิจ | หาใน log ว่ามี PcAddNpc ของ id นั้นระหว่างภารกิจไหม |
| กลับไปจุดเดิมแล้วไม่มี NPC | เซิร์ฟลบแล้วไม่ส่งเพิ่มกลับเมื่อเข้าช่อง | ส่ง `NPC view` ตอนขับกลับไป |
