namespace BossMod.Stormblood.Ultimate.UCOB;

class P3SeventhUmbralEra(BossModule module) : Components.Knockback(module, AID.SeventhUmbralEra, true)
{
    private readonly DateTime _activation = module.WorldState.FutureTime(5.3f);

    public override IEnumerable<Source> Sources(int slot, Actor actor) => [new Source(Module.Center, 11, _activation)];

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.AddForbiddenZone(ShapeDistance.PrecisePosition(new WPos(0, 9), new(0, 1), 0.5f, actor.Position, 0.1f), _activation);
    }
}

class P3CalamitousFlame(BossModule module) : Components.CastCounter(module, AID.CalamitousFlame);
class P3CalamitousBlaze(BossModule module) : Components.CastCounter(module, AID.CalamitousBlaze);

class P3Preposition(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.GoalZones.Add(AIHints.GoalSingleTarget(Arena.Center, 7 + 0.25f * (int)assignment));
    }
}
