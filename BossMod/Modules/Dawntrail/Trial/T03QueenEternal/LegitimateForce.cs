namespace BossMod.Dawntrail.Trial.T03QueenEternal;

sealed class LegitimateForce(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [with(2)];
    private readonly AOEShapeRect rect = new(20f, 40f);
    private readonly Besiegement _aoe = module.FindComponent<Besiegement>()!;
    private readonly ShapeDistance stayInBounds = new SDIntersection([new SDInvertedRect(new(108f, 102f), new(108f, 86f), 4f),
        new SDInvertedRect(new(92f, 102f), new(92f, 86f), 4f)]);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0 || _aoe.AOEs.Count != 0)
        {
            return [];
        }
        var aoes = CollectionsMarshal.AsSpan(_aoes);

        if (count > 1 && aoes[0].Rotation != aoes[1].Rotation)
        {
            ref var aoe0 = ref aoes[0];
            aoe0.Color = Colors.Danger;
            return aoes;
        }
        return aoes[..1];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.LegitimateForceLL:
                AddAOEs(-90f, -90f);
                break;
            case (uint)AID.LegitimateForceLR:
                AddAOEs(-90f, 90f);
                break;
            case (uint)AID.LegitimateForceRR:
                AddAOEs(90f, 90f);
                break;
            case (uint)AID.LegitimateForceRL:
                AddAOEs(90f, -90f);
                break;
        }

        void AddAOEs(float first, float second)
        {
            var loc = caster.Position;
            var rot = spell.Rotation;
            AddAOE(first);
            AddAOE(second, 3.1d); // intentionally caster.Position here, since these are not the actual aoe spell casts
            void AddAOE(float offset, double delay = default) => _aoes.Add(new(rect, loc, rot + offset.Degrees(), Module.CastFinishAt(spell, delay)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0)
        {
            switch (spell.Action.ID)
            {
                case (uint)AID.LegitimateForceLL:
                case (uint)AID.LegitimateForceLR:
                case (uint)AID.LegitimateForceRR:
                case (uint)AID.LegitimateForceRL:
                case (uint)AID.LegitimateForceR:
                case (uint)AID.LegitimateForceL:
                    _aoes.RemoveAt(0);
                    break;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var count = _aoes.Count;
        var besiegeCount = _aoe.AOEs.Count;
        var gravityBounds = Arena.Bounds is ArenaBoundsCustom;
        var center = Arena.Center;
        if (count != 0 && center != new WPos(100f, 94f) || besiegeCount == 0 && count == 2 && gravityBounds)
        {
            var o = new WDir(default, 20f);
            hints.AddForbiddenZone(new SDInvertedRect(center + o, center - o, 3f), aoes[0].Activation);
        }
        else if (count != 2 && besiegeCount == 0 && gravityBounds)
        {
            hints.AddForbiddenZone(stayInBounds, count != 0 ? aoes[0].Activation : default);
        }
    }
}
