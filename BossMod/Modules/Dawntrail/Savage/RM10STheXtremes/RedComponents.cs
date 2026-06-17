namespace BossMod.Dawntrail.Savage.RM10STheXtremes;

class HotImpact(BossModule module) : Components.CastSharedTankbuster(module, AID.HotImpact1, 6);

class FlameFloater(BossModule module) : Components.GenericAOEs(module)
{
    private readonly int[] _slotToOrder = Utils.MakeArray(8, -1);
    private readonly int[] _orderToSlot = Utils.MakeArray(4, -1);

    record struct Bait(Actor Target, DateTime Activation, int Slot, int Order);
    private readonly List<Bait> Baits = [];

    private WPos _lastCastTarget;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var bait in Baits)
        {
            if (bait.Slot == slot)
                continue;

            var imminent = bait.Order == NumCasts;
            var origin = BaitOrigin(bait);

            yield return new AOEInstance(new AOEShapeRect(60, 4), origin, (bait.Target.Position - origin).ToAngle(), bait.Activation, Risky: imminent);
        }
    }

    private WPos BaitOrigin(in Bait b) => b.Order == NumCasts ? _lastCastTarget : Raid[_orderToSlot[b.Order - 1]]?.Position ?? default;

    private Bait? BaitOn(int slot) => Baits.FirstOrNull(b => b.Slot == slot);

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);

        if (BaitOn(pcSlot) is { } bait)
        {
            var origin = BaitOrigin(bait);
            Arena.AddRect(origin, (pc.Position - origin).Normalized(), 60, 0, 4, ArenaColor.Danger);
            Arena.AddCircle(origin, 16, ArenaColor.Object);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (BaitOn(slot) is { } bait)
        {
            var origin = BaitOrigin(bait);
            hints.AddForbiddenZone(ShapeContains.Circle(origin, 16), bait.Activation);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (BaitOn(slot) is { } bait && bait.Order <= NumCasts + 1)
        {
            var origin = BaitOrigin(bait);
            hints.Add("Stretch tether!", actor.Position.InCircle(origin, 16));
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var order = (SID)status.ID switch
        {
            SID.FirstInLine => 0,
            SID.SecondInLine => 1,
            SID.ThirdInLine => 2,
            SID.FourthInLine => 3,
            _ => -1
        };

        if (order >= 0)
        {
            if (order == 0)
                _lastCastTarget = Module.PrimaryActor.Position;

            if (Raid.TryFindSlot(actor, out var slot))
            {
                _slotToOrder[slot] = order;
                _orderToSlot[order] = slot;
                Baits.Add(new(actor, WorldState.FutureTime(7.3f + 1.3f * order), slot, order));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.FlameFloater1 or AID.FlameFloater2 or AID.FlameFloater3 or AID.FlameFloater4)
        {
            NumCasts++;
            _lastCastTarget = spell.TargetXZ;
            if (Baits.Count > 0)
            {
                var b = Baits[0];
                Baits.RemoveAt(0);
                _orderToSlot[b.Order] = _slotToOrder[b.Slot] = -1;
            }
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => _slotToOrder[playerSlot] >= 0 ? PlayerPriority.Interesting : PlayerPriority.Normal;
}
class FlameFloaterPuddle(BossModule module) : FlamePuddle(module, [AID.FlameFloater1, AID.FlameFloater2, AID.FlameFloater3, AID.FlameFloater4], new AOEShapeRect(60, 4), OID.FlameFloater);

class AlleyOopInfernoSpread(BossModule module) : Components.SpreadFromCastTargets(module, AID.AlleyOopInferno, 5);
class AlleyOopInfernoPuddle(BossModule module) : FlamePuddle(module, AID.AlleyOopInferno, new AOEShapeCircle(5), OID.FlamePuddle5, originAtTarget: true);

class CutbackBlazeBait(BossModule module) : Components.CastCounter(module, AID.CutbackBlaze)
{
    private Actor? Source;
    private bool _filter;

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Source != null && (!_filter || pc.FindStatus(SID.Firesnaking) != null))
            Arena.ZoneCone(Source.Position, 0, 60, pc.AngleTo(Source), 15.Degrees(), ArenaColor.SafeFromAOE);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Firesnaking)
            _filter = true;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.CutbackBlazeCast)
            Source = caster;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            Source = null;
        }
    }
}
class CutbackBlazePuddle(BossModule module) : FlamePuddle(module, AID.CutbackBlaze, new AOEShapeCone(60, 165f.Degrees()), OID.CutbackBlaze);

class Pyrotation(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Pyrotation, AID.PyrotationSpread, 6, 5.3f, minStackSize: 6)
{
    public int NumCasts { get; private set; }

    public override void Update()
    {
        if (Stacks.Count > 0)
        {
            ref var s = ref Stacks.Ref(0);
            s.ForbiddenPlayers.Reset();
            if (Module.Enemies(OID.DeepBlue).FirstOrDefault() is { } b && b.CastInfo?.IsSpell(AID.DeepImpactCast) == true)
            {
                var (slot, player) = Raid.WithSlot().Farthest(b.Position);
                if (player != null)
                    s.ForbiddenPlayers.Set(slot);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == StackAction)
        {
            NumCasts++;
            if (NumCasts >= 3)
                Stacks.Clear();
        }
    }
}
class PyrotationPuddle(BossModule module) : FlamePuddle(module, AID.PyrotationSpread, new AOEShapeCircle(6), OID.FlamePuddle6, originAtTarget: true);
