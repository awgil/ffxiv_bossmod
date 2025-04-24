namespace BossMod.Dawntrail.Savage.RM04SWickedThunder;

class SwordQuiverRaidwide(BossModule module) : Components.CastCounter(module, default)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.SwordQuiverRaidwide1 or AID.SwordQuiverRaidwide2 or AID.SwordQuiverRaidwide3 or AID.SwordQuiverRaidwide4)
            ++NumCasts;
    }
}

class SwordQuiverBurst(BossModule module) : Components.GenericAOEs(module, AID.SwordQuiverBurst)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shape = new(30, 6, 30);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.SwordQuiverN or AID.SwordQuiverC or AID.SwordQuiverS)
        {
            var activation = Module.CastFinishAt(spell, 8.9f);
            _aoes.Add(new(_shape, Module.Center, 0.Degrees(), activation));
            _aoes.Add(new(_shape, Module.Center, 118.Degrees(), activation));
            _aoes.Add(new(_shape, Module.Center, -118.Degrees(), activation));

            var zOffset = (AID)spell.Action.ID switch
            {
                AID.SwordQuiverN => -10,
                AID.SwordQuiverS => +10,
                _ => 0
            };
            _aoes.Add(new(_shape, Module.Center + new WDir(0, zOffset), 90.Degrees(), activation));
        }
    }
}

class SwordQuiverLaceration(BossModule module) : Components.CastCounter(module, AID.SwordQuiverLaceration)
{
    private static readonly AOEShapeCone _shape = new(40, 30.Degrees()); // TODO: verify angle

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var dir = Angle.FromDirection(actor.Position - Module.Center);
        var clipped = Raid.WithoutSlot().Exclude(actor).InShape(_shape, Module.Center, dir).CountByCondition(p => p.Class.IsSupport() == actor.Class.IsSupport());
        if (clipped.match != 0 || clipped.mismatch != 1)
            hints.Add("Spread by roles!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        _shape.Outline(Arena, Module.Center, Angle.FromDirection(pc.Position - Module.Center));
    }
}
