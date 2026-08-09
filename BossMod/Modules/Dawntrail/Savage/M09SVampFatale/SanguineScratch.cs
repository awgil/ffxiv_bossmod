
namespace BossMod.Dawntrail.Savage.M09SVampFatale;

// TODO: track bat players are tethered to, figure out max distance; still OK within boss hitbox (12f from center, say 10f max?)
// 3s from last scratch to bat explode; explosion only 1s cast so maybe necessary
// can AI move into donut if standing on edge of hitbox during mech?
// tethers persist through death?
// do tethers attach to players if they were dead when deathmatch resolved?
sealed class BreakdownWing(BossModule module) : Components.GenericAOEs(module)
{
    private readonly SanguineScratch _scratch = module.FindComponent<SanguineScratch>()!;
    // end pos always opposite of start; unnecessary to store end pos?
    class Vampette
    {
        public required Actor Actor;
        public WPos StartPos;
        public WPos EndPos;
    }
    private readonly List<Vampette> _vamps = [];
    private readonly List<AOEInstance> _aoes = [];
    private readonly float _vampRadius = 10f;
    private readonly Actor?[] _vampTethered = new Actor?[8];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count == 0)
            return [];

        // make it only show closer to activation? no need to show too early
        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (actor.OID != (uint)OID.VampetteFatale)
            return;

        if (status.ID == (uint)SID._Gen_2056)
        {
            // 0x426 = circle, 0x427 = donut
            // action enum says 8-15 but inner radius more like 4 in replay; based on boss stacks?
            if (status.Extra is 0x426 or 0x427)
            {
                for (var i = 0; i < _vamps.Count(); i++)
                {
                    if (_vamps[i].Actor.InstanceID == actor.InstanceID)
                    {
                        _aoes.Add(new(status.Extra == 0x426 ? new AOEShapeCircle(7f) : new AOEShapeDonut(4f, 15f), NumCasts == 0 ? _vamps[i].EndPos : _vamps[i].StartPos, activation: WorldState.FutureTime(5.1d)));
                    }
                }
            }
        }

        // gains this just as first tethers go out; could use this instead of action timeline
        if (status.ID == (uint)SID.VampetteDistance)
        {
            var start = actor.Position;
            var end = Arena.Center - (actor.Position - Arena.Center);
            _vamps.Add(new() { Actor = actor, StartPos = start, EndPos = end });
        }
    }
    /*
    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        // get vampette start pos here, end pos is opposite of starting
        // rotates CW or CCW but returns to start pos regardless
        if (actor.OID == (uint)OID.VampetteFatale)
        {
            if (id == 0x1E46)
            {
                var start = actor.Position;
                var end = Arena.Center - (actor.Position - Arena.Center);
                _vamps.Add(new() { Actor = actor, StartPos = start, EndPos = end });
            }
        }
    }
    */
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.BreakdownDrop1 or (uint)AID.BreakdownDrop2 or (uint)AID.BreakwingBeat1 or (uint)AID.BreakwingBeat2)
        {
            ++NumCasts;
            _aoes.Clear();
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        // tethers practically same time as action timeline (0.0 - 0.1s)
        if (tether.ID == (uint)TetherID.Chains)
        {
            var slot = Raid.FindSlot(source.InstanceID);

            if (slot == -1)
                return;

            var actor = WorldState.Actors.Find(tether.Target);
            if (actor == null)
                return;

            if (actor.OID != (uint)OID.VampetteFatale)
                return;

            _vampTethered[slot] = actor;
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);
        if (_vamps.Count == 0)
            return;

        var tethered = _vampTethered[pcSlot];
        if (tethered == null)
            return;

        var danger = pc.DistanceToPoint(tethered.Position) > _vampRadius;

        for (var i = 0; i < _vamps.Count; i++)
        {
            if (tethered.InstanceID == _vamps[i].Actor.InstanceID)
                Arena.ZoneCircleOutline(_vamps[i].Actor.Position, _vampRadius, danger ? Colors.Danger : Colors.Safe, 2f);
        }

        Arena.AddLine(pc.Position, tethered.Position, danger ? Colors.Danger : Colors.Safe, 1f);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_vamps.Count == 0)
            return;

        var tethered = _vampTethered[slot];
        if (tethered == null)
            return;

        if (actor.DistanceToPoint(tethered.Position) > _vampRadius)
        {
            hints.Add("Too far from bat!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_vamps.Count == 0)
            return;

        if (_vampTethered[slot] == null)
            return;

        hints.GoalZones.Add(AIHints.GoalSingleTarget(_vampTethered[slot]!.Position, 7f, 10f));

        // ignore blasts until scratches are done
        if (_scratch.ActiveAOEs(slot, actor).Length != 0)
            return;

        base.AddAIHints(slot, actor, assignment, hints);
    }
}

sealed class SanguineScratch(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [with(8)];
    private readonly AOEShapeCone cone = new(40f, 15f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SanguineScratchFirst)
        {
            var loc = spell.LocXZ;
            var rot = spell.Rotation;
            _aoes.Add(new(cone, loc, rot, Module.CastFinishAt(spell), shapeDistance: cone.Distance(loc, rot)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.SanguineScratchFirst or (uint)AID.SanguineScratchRest)
        {
            var count = _aoes.Count;
            ++NumCasts;
            if (count > 0)
            {
                if ((NumCasts & 7) == 0)
                {
                    var act = WorldState.FutureTime(2.4d);
                    var a23 = 22.5f.Degrees();
                    var aoes = CollectionsMarshal.AsSpan(_aoes);
                    for (var i = 0; i < count; ++i)
                    {
                        ref var aoe = ref aoes[i];
                        aoe.Rotation += a23;
                        aoe.ShapeDistance = aoe.Shape.Distance(aoe.Origin, aoe.Rotation);
                        aoe.Activation = act;
                    }
                }
            }
        }
    }
}
