namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN1TwoHeadedAevis;

sealed class HypothermalCombustionShock(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> actors = [];
    private readonly List<AOEInstance> _aoes = [];
    private readonly AOEShapeCircle shape = new(15f);
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];

        var max = count > 2 ? 2 : count;
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        return aoes[..max];
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is (uint)OID.SwirlingOrb or (uint)OID.BallLightning)
        {
            actors.Add(actor);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ThunderfrostTempest)
        {
            // x2 at same time, before orbs cast their own spell
            var act = Module.CastFinishAt(spell, 2.7d);
            var count = actors.Count;
            for (var i = 0; i < count; i++)
            {
                ref var actor = ref actors.Ref(i);
                _aoes.Add(new(shape, actor.Position, activation: act));
            }
            actors.Clear();
        }
        else if (actors.Count != 0 && spell.Action.ID is (uint)AID.HypothermalCombustion or (uint)AID.Shock)
        {
            actors.Remove(caster);
        }
        else
        {
            var orbId = spell.Action.ID switch
            {
                (uint)AID.IceCluster => (uint)OID.SwirlingOrb,
                (uint)AID.LightningCluster => (uint)OID.BallLightning,
                _ => default
            };

            if (orbId == default)
            {
                return;
            }

            var pos = spell.LocXZ;
            var act = Module.CastFinishAt(spell, 2.4d);
            var count = actors.Count;

            for (var i = 0; i < count; i++)
            {
                ref var actor = ref actors.Ref(i);
                if (actor.OID == orbId && shape.Check(pos, actor))
                {
                    _aoes.Add(new(shape, actor.Position, activation: act, actorID: actor.InstanceID));
                }
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0)
        {
            switch (spell.Action.ID)
            {
                case (uint)AID.HypothermalCombustion:
                case (uint)AID.Shock:
                    _aoes.RemoveAt(0);
                    break;
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var knockbacks = Module.FindComponent<HissingReprise>();
        if (knockbacks == null || knockbacks.ActiveKnockbacks(slot, actor).Length == 0)
        {
            base.AddHints(slot, actor, hints);
        }
    }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var knockbacks = Module.FindComponent<HissingReprise>();
        if (knockbacks == null || knockbacks.ActiveKnockbacks(slot, actor).Length == 0)
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }
}
