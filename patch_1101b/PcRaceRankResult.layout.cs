// DRAFT (Fable 11-01) — layout only, from client PcRaceRankResult::Read 0x474B69 -> FUN_0046e8d2 (struct 0x74 at +0x14)
// + one trailing value at +0x88 (FUN_0040fa8a; the server Write 0x44D290 passes it by value -> u32).
// Meaning of the fields comes from RaceAgent 0x443F10 (sender, one packet per racer) — see HANDOFF §12; names are
// provisional until FUN_0044d350 / FUN_004bff70 and the client window that reads +0x14.. are decoded.
// Replaces the Raw class in RayCity.Protocol/GameServer/Client1627.g.cs ONLY after the names are confirmed.
namespace RayCity.Protocol.GameServer;

/// <summary>id 0x32870639 (client 1627). Per racer at race end (RA 0x443F10 -> 0x44D1F0): the track ranking view.</summary>
public sealed class PcRaceRankResult : Packet
{
    public sealed class Top { public uint Rank; public string Name = ""; public uint Time; }          // +0x00/+0x0C/+0x18 x3
    public sealed class Near { public uint A; public uint B; public string Name = ""; public uint Time; } // +0x24/+0x2C/+0x34/+0x3C x2
    public Top[] Top3 = [new(), new(), new()];      // 3 x {u32, wstring, u32}
    public Near[] Near2 = [new(), new()];           // 2 x {u32, u32, wstring, u32}
    public uint F44;                                // +0x44
    public uint CharacterId;                        // +0x48  (0x443F10: racer +4)
    public uint ThisTime;                           // +0x4C  (racer +0x50)
    public uint F50, F54, F58, F5C;                 // +0x50..+0x5C (filled by 0x44D350 - not decoded yet)
    public uint Best1, Best2, Best3;                // +0x60/+0x64/+0x68 = min(this race, stored best) for 3 metrics
    public byte F6C;                                // +0x6C
    public byte NewBest1, NewBest2, NewBest3;       // +0x6D/+0x6E/+0x6F = this race beat the stored best
    public uint F70;                                // +0x70
    public uint F88;                                // +0x88 = DAT_007E8254 (RaceAgent global; season / ranking period id?)

    public override void Read(ref PacketReader r)
    {
        foreach (var t in Top3) { t.Rank = r.U32(); t.Name = r.WString(); t.Time = r.U32(); }
        foreach (var n in Near2) { n.A = r.U32(); n.B = r.U32(); n.Name = r.WString(); n.Time = r.U32(); }
        F44 = r.U32(); CharacterId = r.U32(); ThisTime = r.U32(); F50 = r.U32(); F54 = r.U32(); F58 = r.U32(); F5C = r.U32();
        Best1 = r.U32(); Best2 = r.U32(); Best3 = r.U32();
        F6C = r.U8(); NewBest1 = r.U8(); NewBest2 = r.U8(); NewBest3 = r.U8();
        F70 = r.U32(); F88 = r.U32();
    }
    public override void Write(PacketWriter w)
    {
        foreach (var t in Top3) { w.U32(t.Rank); w.WString(t.Name); w.U32(t.Time); }
        foreach (var n in Near2) { w.U32(n.A); w.U32(n.B); w.WString(n.Name); w.U32(n.Time); }
        w.U32(F44); w.U32(CharacterId); w.U32(ThisTime); w.U32(F50); w.U32(F54); w.U32(F58); w.U32(F5C);
        w.U32(Best1); w.U32(Best2); w.U32(Best3);
        w.U8(F6C); w.U8(NewBest1); w.U8(NewBest2); w.U8(NewBest3);
        w.U32(F70); w.U32(F88);
    }
}
