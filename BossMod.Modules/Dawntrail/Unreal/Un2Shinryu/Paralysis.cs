namespace BossMod.Dawntrail.Unreal.Un2Shinryu;

class Paralysis : BossComponent
{
    public Paralysis(BossModule module) : base(module)
    {
        KeepOnPhaseChange = true;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!actor.Class.CanEsuna())
            return;
        foreach (var member in Raid.WithoutSlot())
            if (!member.IsDead && member.FindStatus(SID.Paralysis) != null && member.PendingDispels.Count == 0)
                hints.Add($"Cleanse {member.Name}!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var (partySlot, member) in Raid.WithSlot())
            if (!member.IsDead && member.FindStatus(SID.Paralysis) != null)
                hints.ShouldCleanse.Set(partySlot);
    }
}
