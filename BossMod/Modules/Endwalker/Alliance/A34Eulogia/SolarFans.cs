namespace BossMod.Endwalker.Alliance.A34Eulogia;

sealed class SolarFans(BossModule module) : Components.ChargeAOEs(module, (uint)AID.SolarFansAOE, 5f);

sealed class RadiantRhythm(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [with(8)];
    private readonly AOEShapeDonutSector _shape = new(20f, 30f, 45f.Degrees());

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
        if (_aoes.Count == 0 && spell.Action.ID == (uint)AID.SolarFansAOE)
        {
            // assumption: flames always move CCW
            var pattern1 = false;
            if ((int)spell.LocXZ.Z == -945f)
            {
                pattern1 = true;
            }
            NumCasts = 0;
            var activation = Module.CastFinishAt(spell, 2.8d);
            for (var i = 1; i < 5; ++i)
            {
                var act = activation.AddSeconds(2.1d * (i - 1));
                var angle = ((pattern1 ? 225f : 135f) + i * 90f).Degrees();
                AddAOE(angle, act);
                AddAOE(angle + 180f.Degrees(), act);
            }
            void AddAOE(Angle rotation, DateTime activation) => _aoes.Add(new(_shape, Arena.Center.Quantized(), rotation, activation));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RadiantFlight)
        {
            ++NumCasts;
            if (_aoes.Count != 0)
            {
                _aoes.RemoveAt(0);
            }
        }
    }
}

sealed class RadiantFlourish(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [with(2)];
    private static readonly AOEShapeCircle _shape = new(25f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        // assumption: we always have 4 moves, so flames finish where they start
        if (spell.Action.ID == (uint)AID.SolarFansAOE)
        {
            _aoes.Add(new(_shape, spell.LocXZ, default, Module.CastFinishAt(spell, 13.8d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.RadiantFlourish)
        {
            ++NumCasts;
            _aoes.Clear();
        }
    }
}
