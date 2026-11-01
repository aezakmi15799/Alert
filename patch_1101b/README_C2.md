# C2 PcUpdateOverralRaceResult — ไฟล์ใหม่ 2 ไฟล์ + แทรก 1 บรรทัด (ไม่แตะไฟล์ของ Opus)

1. คัดลอก `AccountStore.RaceStats.cs` และ `LoginHandlers.RaceStats.cs` ไปที่ `server_cs\RayCity.GameServer\`
2. ใน `LoginHandlers.cs` หลังบรรทัด
   `await SendCarExtStateAsync(s, car.CarIdx);`   (ใน handler เลือกตัวละคร ~บรรทัด 530)
   เพิ่ม
   `await SendOverallRaceResultAsync(s, ch.CharacterId);`
   PowerShell:
   ```powershell
   $f="G:\RayCityServer_src\server_cs\RayCity.GameServer\LoginHandlers.cs"
   (Get-Content $f -Raw) -replace 'await SendCarExtStateAsync\(s, car\.CarIdx\);', "await SendCarExtStateAsync(s, car.CarIdx);`r`n        await SendOverallRaceResultAsync(s, ch.CharacterId);" | Set-Content $f -NoNewline
   ```
3. `dotnet build` → ถ้า `GapKind.MatchesOriginal` ใช้ไม่ได้กับเมธอดที่ไม่ใช่ handler ให้ลบบรรทัด `[Gap(...)]` ออก
4. เทส: เลือกตัวละคร → log ต้องมี `-> PcUpdateOverralRaceResult solo ...` และหน้าเลือกตัวละคร/ข้อมูลตัวละครโชว์สถิติแข่ง
   แข่งเดี่ยว 1 รอบให้ชนะ → ออกมาเลือกตัวละครใหม่ → ตัวเลขช่อง "ชนะเดี่ยว" ต้องเพิ่ม 1
   ถ้าเพิ่มผิดช่อง = ลำดับกลุ่มใน `Values` ผิด บอกว่าช่องไหนขยับ จะสลับให้
   ปิดได้ด้วย `set RC_OVERALL_RACE_RESULT=0`
