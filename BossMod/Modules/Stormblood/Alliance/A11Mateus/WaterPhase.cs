namespace BossMod.Stormblood.Alliance.A11Mateus;

class Froth(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(1.4f);
    public readonly List<AOEInstance> _aoes = [with(6)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var status = actor.FindStatus((uint)SID.Breathless);
        if (status is ActorStatus breathless && breathless.Extra >= 0x5)
            return CollectionsMarshal.AsSpan(_aoes);
        return [];
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Froth)
            _aoes.Add(new(circle, actor.Position, color: Colors.SafeFromAOE, risky: false));
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.Froth)
        {
            var count = _aoes.Count;
            var pos = actor.Position;
            for (var i = 0; i < count; ++i)
            {
                if (_aoes[i].Origin == pos)
                {
                    _aoes.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = _aoes.Count;
        if (count != 0)
        {
            var status = actor.FindStatus((uint)SID.Breathless);
            if (status is ActorStatus breathless && breathless.Extra >= 0x5)
            {
                var orbs = new ShapeDistance[count];
                hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Sprint), actor, ActionQueue.Priority.High);
                for (var i = 0; i < count; ++i)
                {
                    var o = _aoes[i];
                    orbs[i] = new SDInvertedCircle(o.Origin, 1.4f);
                }
                if (orbs.Length != 0)
                    hints.AddForbiddenZone(new SDIntersection(orbs), WorldState.FutureTime(10d - breathless.Extra));
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var count = _aoes.Count;
        if (count == 0)
            return;
        var status = actor.FindStatus((uint)SID.Breathless);
        if (status is ActorStatus breathless && breathless.Extra >= 0x5)
        {
            hints.Add("Stand in an air bubble!");
        }
    }
}

class Snowpierce(BossModule module) : Components.BaitAwayChargeCast(module, (uint)AID.Snowpierce, 1.5f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        var bait = ActiveBaitsOn(actor);
        if (bait.Count != 0)
        {
            var b = bait[0];
            var froth = Module.Enemies((uint)OID.Froth);
            var count = froth.Count;
            if (count == 0)
                return;
            var forbidden = new ShapeDistance[count];
            for (var i = 0; i < count; ++i)
            {
                var a = froth[i];
                forbidden[i] = new SDCone(b.Source.Position, 100f, b.Source.AngleTo(a), Angle.Asin(2.9f / (a.Position - b.Source.Position).Length()));
            }
            hints.AddForbiddenZone(new SDUnion(forbidden), b.Activation);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (!IsBaitTarget(pc))
        {
            return;
        }
        var froth = Module.Enemies((uint)OID.Froth);
        var count = froth.Count;
        for (var i = 0; i < count; ++i)
        {
            var a = froth[i];
            Arena.ZoneCircleOutline(a.Position, a.HitboxRadius);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (IsBaitTarget(actor))
        {
            hints.Add("Bait away, avoid intersecting bubble hitboxes!");
        }
    }
}
