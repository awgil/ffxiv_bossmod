namespace BossMod.Dawntrail.Savage.RM09SVampFatale;

class AetherlettingCone(BossModule module) : Components.StandardAOEs(module, AID.AetherlettingCone, new AOEShapeCone(40, 22.5f.Degrees()), maxCasts: 4, highlightImminent: true);

class AetherlettingSpread(BossModule module) : Components.SpreadFromCastTargets(module, AID.AetherlettingSpread, 6)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (Spreads.FirstOrNull(s => s.Target == actor) is { } s)
            // encourage autorot to not dash to center while spread is active
            hints.AddForbiddenZone(ShapeContains.Circle(Arena.Center, 18), s.Activation);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (Spreads.FirstOrNull(s => s.Target == actor) is { } s)
        {
            var pred = (s.Activation - WorldState.CurrentTime).TotalSeconds;
            hints.Add($"Puddle drops in {pred:f1}s", false);
        }
    }
}

class AetherlettingCross(BossModule module) : Components.StandardAOEs(module, AID.AetherlettingGround, new AOEShapeCross(40, 5), maxCasts: 4, highlightImminent: true)
{
    public bool Draw;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Draw ? base.ActiveAOEs(slot, actor) : [];
}
