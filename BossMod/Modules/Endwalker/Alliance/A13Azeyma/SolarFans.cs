namespace BossMod.Endwalker.Alliance.A13Azeyma;

sealed class SolarFans(BossModule module) : Components.ChargeAOEs(module, (uint)AID.SolarFansAOE, 5f);

sealed class RadiantRhythm(BossModule module) : Components.GenericAOEs(module, (uint)AID.RadiantFlight)
{
    private readonly AOEShapeDonutSector _shape = new(20f, 30f, 45f.Degrees());
    private readonly List<AOEInstance> _aoes = [with(10)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var max = count > 4 ? 4 : count;
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        if (count > 3)
        {
            var color = Colors.Danger;
            for (var i = 0; i < 2; ++i)
            {
                ref var aoe = ref aoes[i];
                aoe.Color = color;
            }
        }
        return aoes[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count == 0 && spell.Action.ID == (uint)AID.SolarFansCharge) // since it seems impossible to determine early enough if 4 or 5 casts happen, we draw one extra one just incase
        {
            var activation = Module.CastFinishAt(spell, 7.7d);
            var pattern1 = false;
            if ((int)spell.LocXZ.Z == -750)
            {
                pattern1 = true;
            }
            for (var i = 1; i < 6; ++i)
            {
                var act = activation.AddSeconds(1.3d * (i - 1));
                var angle = ((pattern1 ? 225f : 135f) + i * 90f).Degrees();
                AddAOE(angle, act);
                AddAOE(angle + 180f.Degrees(), act);
            }
        }
        void AddAOE(Angle rotation, DateTime activation) => _aoes.Add(new(_shape, new WPos(-750f, -750f).Quantized(), rotation, activation));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == (uint)AID.RadiantFinish)
        {
            _aoes.Clear();
        }
        else if (_aoes.Count != 0 && spell.Action.ID == WatchedAction)
        {
            _aoes.RemoveAt(0);
        }
    }
}

sealed class RadiantFlourish(BossModule module) : Components.GenericAOEs(module)
{
    private int teleportcounter;
    private readonly AOEShapeCircle circle = new(25f);
    private readonly List<AOEInstance> _aoes = [with(2)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is var id && id == (uint)AID.SolarFansAOE)
        {
            _aoes.Add(new(circle, spell.LocXZ, default, WorldState.FutureTime(16.6d)));
        }
        else if (id == (uint)AID.RadiantFlourish)
        {
            _aoes.Clear();
            teleportcounter = 0;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.TeleportFlame) // correct circle location if Flight happens 10 times instead of 8 times, ugly hack but i couldn't find a better difference in logs
        {
            if (++teleportcounter > 8)
            {
                teleportcounter = 0;
                var aoe0 = _aoes[0];
                var activation = aoe0.Activation.AddSeconds(1.4d);
                AddAOE(aoe0.Origin);
                AddAOE(_aoes[1].Origin);
                _aoes.RemoveRange(0, 2);

                void AddAOE(WPos origin) => _aoes.Add(new(circle, WPos.RotateAroundOrigin(90f, new(-750f, -750f), origin.Quantized()), default, activation));
            }
        }
    }
}
