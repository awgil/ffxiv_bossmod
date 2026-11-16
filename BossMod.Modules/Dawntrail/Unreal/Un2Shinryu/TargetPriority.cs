namespace BossMod.Dawntrail.Unreal.Un2Shinryu;

class TargetPriority : BossComponent
{
    public TargetPriority(BossModule module) : base(module)
    {
        KeepOnPhaseChange = true;
    }

    public Actor? FirstWing() => Module.Enemies(OID.LeftWingP3).Concat(Module.Enemies(OID.RightWingP3))
        .Where(w => !w.IsDead && w.IsTargetable && w.CastInfo != null && w.CastInfo.IsSpell() && (AID)w.CastInfo.Action.ID is AID.JudgmentBoltEnrage or AID.HellfireEnrage)
        .MinBy(w => w.CastInfo!.RemainingTime);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var first = FirstWing();
        foreach (var e in hints.PotentialTargets)
        {
            if (e.Priority < 0)
                continue;
            var prio = (OID)e.Actor.OID switch
            {
                OID.Tail => 3,
                OID.WormsHeart => 2,
                OID.LeftWingP3 or OID.RightWingP3 => e.Actor == first ? 2 : 1,
                _ => 0
            };
            e.Priority = Math.Max(e.Priority, prio);
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Module.Enemies(OID.Tail).Any(t => !t.IsDead && t.IsTargetable))
            hints.Add("Kill the tail");
        else if (Module.Enemies(OID.WormsHeart).Any(h => !h.IsDead && h.IsTargetable))
            hints.Add("Kill the Worm's Heart");
        else if (FirstWing() is { } wing)
            hints.Add($"Kill the {((OID)wing.OID == OID.LeftWingP3 ? "left" : "right")} wing first");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(Module.Enemies(OID.WormsHeart), ArenaColor.Enemy);
        if (FirstWing() is { } wing)
            Arena.Actor(wing, ArenaColor.Vulnerable);
    }
}
