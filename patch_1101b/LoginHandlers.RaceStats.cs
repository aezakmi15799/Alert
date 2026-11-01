using RayCity.Protocol.GameServer;

namespace RayCity.GameServer;

public sealed partial class LoginHandlers
{
    /// <summary>C2: PcUpdateOverralRaceResult right after PrSelectCharacter, as GameServer 0x5341C0 (called from the
    /// character-select path 0x56E050). The client (Read 0x43A7C0) takes four groups of three u32 into +0x14 / +0x20 /
    /// +0x2C / +0x38; the original fills them from character +0x74..+0xA0 = 12 ints loaded from RC_CharacterState.
    /// <para>ASSUMPTION (to verify with one race, see TODO_TEST_1101.md T6 step 4): group order = solo (win, lose,
    /// retire), team (win, lose, 0), field solo (win, lose, retire), field team (win, lose, 0). If the wrong counter
    /// moves on the character screen, change only the order in <c>Values</c> below.</para>
    /// Off with RC_OVERALL_RACE_RESULT=0 (the original always sends it).</summary>
    [Gap(GapKind.MatchesOriginal, "GS 0x5341C0: PcUpdateOverralRaceResult after PrSelectCharacter; field order assumed, see summary")]
    private async Task SendOverallRaceResultAsync(ClientSession s, uint characterId)
    {
        if (Environment.GetEnvironmentVariable("RC_OVERALL_RACE_RESULT") == "0" || accounts == null) return;
        AccountStore.RaceStats? st;
        try { st = await accounts.GetRaceStatsAsync(characterId); }
        catch (Exception ex) { log.Info($"#{s.Connection.Id} PcUpdateOverralRaceResult: rc_characterstate read failed: {ex.Message}"); return; }
        st ??= new AccountStore.RaceStats(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        var pkt = new PcUpdateOverralRaceResult
        {
            Values =
            [
                (uint)st.SoloWin, (uint)st.SoloLose, (uint)st.SoloRetire,
                (uint)st.TeamWin, (uint)st.TeamLose, 0,
                (uint)st.FieldSoloWin, (uint)st.FieldSoloLose, (uint)st.FieldSoloRetire,
                (uint)st.FieldTeamWin, (uint)st.FieldTeamLose, 0,
            ],
        };
        s.Connection.Send(pkt);
        log.Info($"#{s.Connection.Id} -> PcUpdateOverralRaceResult solo {st.SoloWin}/{st.SoloLose}/{st.SoloRetire} team {st.TeamWin}/{st.TeamLose} " +
                 $"field solo {st.FieldSoloWin}/{st.FieldSoloLose}/{st.FieldSoloRetire} field team {st.FieldTeamWin}/{st.FieldTeamLose}");
    }
}
