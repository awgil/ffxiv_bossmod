namespace BossMod.DawnTrail.Raid.M10NDaringDevils;

sealed class CutbackBlazeBait(BossModule module) : Components.BaitAwayIcon(
    module,
    new AOEShapeCone(60f, 30f.Degrees()),                 // tune angle if needed
    (uint)IconID.CutbackBlazeBait,
    (uint)AID.CutbackBlaze1,
    activationDelay: 5.0d,
    centerAtTarget: false)
{
    public static readonly AOEShapeCone Cone = new(60f, 30f.Degrees());
}

sealed class CutbackBlazePersistent(BossModule module) : Components.GenericAOEs(module, (uint)AID.CutbackBlaze1)
{
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
        => CollectionsMarshal.AsSpan(_aoes);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.CutbackBlaze1:
                _aoes.Add(new(CutbackBlazeBait.Cone, caster.Position, caster.Rotation, WorldState.CurrentTime, Colors.AOE, true, caster.InstanceID));
                break;

            case (uint)AID.DiversDare:
            case (uint)AID.DiversDare1:
                _aoes.Clear();
                break;
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        var id = actor.InstanceID;
        _aoes.RemoveAll(a => a.ActorID == id);
    }
}
sealed class AlleyOopInfernoSpread(BossModule module)
    : Components.SpreadFromCastTargets(module, (uint)AID.AlleyOopInferno1, 5f);

sealed class AlleyOopInfernoPuddles(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _puddles = [];
    private static readonly AOEShapeCircle Shape = new(5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
        => CollectionsMarshal.AsSpan(_puddles);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.AlleyOopInferno1:
                {
                    foreach (var t in spell.Targets)
                    {
                        var target = WorldState.Actors.Find(t.ID);
                        if (target != null)
                            _puddles.Add(new(Shape, target.Position, default, WorldState.CurrentTime, Colors.AOE, true));
                    }
                    break;
                }

            case (uint)AID.DiversDare:
            case (uint)AID.DiversDare1:
                _puddles.Clear();
                break;
        }
    }
}

sealed class AlleyOopMaelstromSequential(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Tracked> _tracked = [];
    private readonly List<AOEInstance> _active = [];
    private static readonly AOEShapeCone Shape30 = new(60f, 15f.Degrees());
    private static readonly AOEShapeCone Shape15 = new(60f, 7.5f.Degrees());

    private struct Tracked(AOEInstance aoe, uint aid)
    {
        public AOEInstance AOE = aoe;
        public uint AID = aid;
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _active.Clear();

        var have30 = false;
        foreach (var t in _tracked)
        {
            if (t.AID == (uint)AID.AlleyOopMaelstrom)
            {
                have30 = true;
                break;
            }
        }

        var want = have30 ? (uint)AID.AlleyOopMaelstrom : (uint)AID.AlleyOopMaelstrom2;

        foreach (var t in _tracked)
        {
            if (t.AID == want)
                _active.Add(t.AOE);
        }

        return CollectionsMarshal.AsSpan(_active);
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.AlleyOopMaelstrom or (uint)AID.AlleyOopMaelstrom2)
        {
            _tracked.RemoveAll(t => t.AOE.ActorID == caster.InstanceID && t.AID == spell.Action.ID);
        }
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = spell.Action.ID switch
        {
            (uint)AID.AlleyOopMaelstrom => Shape30,   // 46495
            (uint)AID.AlleyOopMaelstrom2 => Shape15,  // 46496
            _ => null
        };
        if (shape == null)
            return;

        var activation = WorldState.FutureTime(spell.NPCRemainingTime);
        var aoe = new AOEInstance(shape, caster.Position, spell.Rotation, activation, Colors.AOE, true, caster.InstanceID);
        _tracked.Add(new(aoe, spell.Action.ID));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.AlleyOopMaelstrom or (uint)AID.AlleyOopMaelstrom2)
            _tracked.RemoveAll(t => t.AOE.ActorID == caster.InstanceID && t.AID == spell.Action.ID);
    }

    public override void OnActorDestroyed(Actor actor)
    {
        var id = actor.InstanceID;
        _tracked.RemoveAll(t => t.AOE.ActorID == id);
    }
}
