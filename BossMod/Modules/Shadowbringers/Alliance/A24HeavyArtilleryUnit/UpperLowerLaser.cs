namespace BossMod.Shadowbringers.Alliance.A24HeavyArtilleryUnit;

class LowerLaser(BossModule module) : Components.GenericAOEs(module, AID.LowerLaserCast)
{
    private readonly List<(Angle rotation, DateTime activation, int casts)> _casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _casters.Select(c => new AOEInstance(new AOEShapeCone(30, 30.Degrees()), Arena.Center, c.rotation, c.activation, Color: ArenaColor.Danger));

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == 0x1EA1A1 && state == 0x00100020)
        {
            var angle = (MathF.Round(Angle.FromDirection(actor.Position - Arena.Center).Deg / 60f) * 60).Degrees();
            _casters.Add((angle, WorldState.FutureTime(9.6f), 0));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.LowerLaserCast or AID.LowerLaserRepeat)
        {
            var index = _casters.FindIndex(c => c.rotation.AlmostEqual(spell.Rotation, 0.1f));
            if (index >= 0)
            {
                if (++_casters.Ref(index).casts >= 7)
                    _casters.RemoveAt(index);
            }
        }
    }
}

class UpperLaser(BossModule module) : Components.GenericAOEs(module, AID.UpperLaser1Cast)
{
    private readonly List<(Angle rotation, DateTime activation, int casts)> _casters = [];

    private static readonly AOEShape[] Shapes = [new AOEShapeCone(16, 30.Degrees()), new AOEShapeDonutSector(14, 23, 30.Degrees()), new AOEShapeDonutSector(21, 30, 30.Degrees())];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var c in _casters)
        {
            var stage = c.casts / 5;
            if (stage < 2)
                yield return new AOEInstance(Shapes[stage + 1], Arena.Center, c.rotation, c.activation.AddSeconds(3 * stage + 3), Risky: false);
            yield return new AOEInstance(Shapes[stage], Arena.Center, c.rotation, c.activation.AddSeconds(3 * stage), Color: ArenaColor.Danger);
        }
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == 0x1EA1A1 && state == 0x00800100)
        {
            var angle = (MathF.Round(Angle.FromDirection(actor.Position - Arena.Center).Deg / 60f) * 60).Degrees();
            _casters.Add((angle, WorldState.FutureTime(9.6f), 0));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.UpperLaser1Cast or AID.UpperLaser1Repeat or AID.UpperLaser2Cast or AID.UpperLaser2Repeat or AID.UpperLaser3Cast or AID.UpperLaser3Repeat)
        {
            var index = _casters.FindIndex(c => c.rotation.AlmostEqual(spell.Rotation, 0.1f));
            if (index >= 0)
            {
                if (++_casters.Ref(index).casts >= 15)
                    _casters.RemoveAt(index);
            }
        }
    }
}
