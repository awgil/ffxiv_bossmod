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

// actually 8 units, but normalmove tends to walk into it for some reason
class P3BahamutMoon(BossModule module) : Components.Voidzone(module, 8.5f, OID.BahamutMoon)
{
    bool _knockbackHappened;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SeventhUmbralEra)
            _knockbackHappened = true;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (_knockbackHappened)
            hints.GoalZones.Add(AIHints.GoalSingleTarget(Arena.Center, Sources.Any() ? 10 : 6));
    }
}

class P3BahamutPositioning(BossModule module) : BossComponent(module)
{
    public WPos? DesiredPosition;
    public Angle? DesiredRotation;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (hints.FindEnemy(((UCOB)Module).BahamutPrime()) is { } b)
        {
            b.DesiredRotation ??= DesiredRotation;
            b.DesiredPosition ??= DesiredPosition;
        }
    }
}
