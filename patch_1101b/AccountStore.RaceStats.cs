using MySqlConnector;

namespace RayCity.GameServer;

/// <summary>gamedb00.rc_characterstate: the per-character race counters the original kept at character +0x74.. and
/// sent at character select in PcUpdateOverralRaceResult (GameServer 0x5341C0 ← 0x56E050).</summary>
public sealed partial class AccountStore
{
    public sealed record RaceStats(
        int SoloWin, int SoloLose, int SoloRetire,
        int TeamWin, int TeamLose,
        int FieldSoloWin, int FieldSoloLose, int FieldSoloRetire,
        int FieldTeamWin, int FieldTeamLose,
        int ItemSoloWin, int ItemSoloLose, int ItemSoloRetire,
        int ItemTeamWin, int ItemTeamLose,
        int AcquirePoint, int ImprovePoint, int LegendAcquirePoint);

    /// <summary>The character's row of rc_characterstate, or null when it has none yet (a new character).</summary>
    public async Task<RaceStats?> GetRaceStatsAsync(uint characterId, CancellationToken ct = default)
    {
        await using var c = await OpenAsync(ct);
        await using var cmd = Cmd(c,
            "SELECT SoloRaceWin, SoloRaceLose, SoloRaceRetire, TeamRaceWin, TeamRaceLose, " +
            "FieldSoloRaceWin, FieldSoloRaceLose, FieldSoloRaceRetire, FieldTeamRaceWin, FieldTeamRaceLose, " +
            "ItemSoloRaceWin, ItemSoloRaceLose, ItemSoloRaceRetire, ItemTeamRaceWin, ItemTeamRaceLose, " +
            "AcquirePoint, ImprovePoint, LegendAcquirePoint FROM gamedb00.rc_characterstate WHERE CharacterID = @c LIMIT 1",
            ("@c", (int)characterId));
        await using var rd = await cmd.ExecuteReaderAsync(ct);
        if (!await rd.ReadAsync(ct)) return null;
        int I(int i) => rd.IsDBNull(i) ? 0 : rd.GetInt32(i);
        return new RaceStats(I(0), I(1), I(2), I(3), I(4), I(5), I(6), I(7), I(8), I(9), I(10), I(11), I(12), I(13), I(14), I(15), I(16), I(17));
    }
}
