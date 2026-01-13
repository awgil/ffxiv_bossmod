namespace BossMod.Dawntrail.Savage.RM10STheXtremes;

class HotImpact(BossModule module) : Components.CastSharedTankbuster(module, AID._Weaponskill_HotImpact, 6);

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
            SID._Gen_FirstInLine => 0,
            SID._Gen_SecondInLine => 1,
            SID._Gen_ThirdInLine => 2,
            SID._Gen_FourthInLine => 3,
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
        if ((AID)spell.Action.ID is AID._Weaponskill_FlameFloater1 or AID._Weaponskill_FlameFloater2 or AID._Weaponskill_FlameFloater3 or AID._Weaponskill_FlameFloater4)
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
class FlameFloaterPuddle(BossModule module) : FlamePuddle(module, [AID._Weaponskill_FlameFloater1, AID._Weaponskill_FlameFloater2, AID._Weaponskill_FlameFloater3, AID._Weaponskill_FlameFloater4], new AOEShapeRect(60, 4), OID.FlameFloater);

class AlleyOopInfernoSpread(BossModule module) : Components.SpreadFromCastTargets(module, AID._Weaponskill_AlleyOopInferno1, 5);
class AlleyOopInfernoPuddle(BossModule module) : FlamePuddle(module, AID._Weaponskill_AlleyOopInferno1, new AOEShapeCircle(5), OID.FlamePuddle5, originAtTarget: true);

class CutbackBlazeBait(BossModule module) : Components.CastCounter(module, AID._Weaponskill_CutbackBlaze1)
{
    private Actor? Source;
    private bool _filter;

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Source != null && (!_filter || pc.FindStatus(SID._Gen_Firesnaking) != null))
            Arena.ZoneCone(Source.Position, 0, 60, pc.AngleTo(Source), 15.Degrees(), ArenaColor.SafeFromAOE);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_Firesnaking)
            _filter = true;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_CutbackBlaze)
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
class CutbackBlazePuddle(BossModule module) : FlamePuddle(module, AID._Weaponskill_CutbackBlaze1, new AOEShapeCone(60, 165f.Degrees()), OID.CutbackBlaze);

class Pyrotation(BossModule module) : Components.StackWithIcon(module, (uint)IconID._Gen_Icon_m0982trg_g0c, AID._Weaponskill_Pyrotation1, 6, 5.3f, minStackSize: 6)
{
    public int NumCasts { get; private set; }

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
class PyrotationPuddle(BossModule module) : FlamePuddle(module, AID._Weaponskill_Pyrotation1, new AOEShapeCircle(6), OID.FlamePuddle6, originAtTarget: true);

class DiversDare1(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_DiversDare);
class DiversDare2(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_DiversDare1);

class SickestTakeOff(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_SickestTakeOff1, new AOEShapeRect(50, 7.5f));
class SickSwell(BossModule module) : Components.KnockbackFromCastTarget(module, AID._Weaponskill_SickSwell1, 10, kind: Kind.DirForward)
{
    public bool EnableHints = true;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (EnableHints)
            base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (EnableHints)
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class AwesomeSplab : Components.GenericStackSpread
{
    public int NumCasts { get; private set; }

    public AwesomeSplab(BossModule module) : base(module)
    {
        EnableHints = false;
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((OID)actor.OID == OID.DeepBlue && (SID)status.ID == SID._Gen_)
        {
            switch (status.Extra)
            {
                case 0x3ED:
                    foreach (var p in Raid.WithoutSlot().OrderByDescending(a => a.Role == Role.Healer).Take(2))
                        Stacks.Add(new(p, 6, 4, 4, WorldState.FutureTime(15.7f)));
                    break;
                case 0x3EE:
                    foreach (var p in Raid.WithoutSlot())
                        Spreads.Add(new(p, 5, WorldState.FutureTime(15.7f)));
                    break;
                case 0x3EF:
                    foreach (var p in Raid.WithoutSlot().Where(p => p.FindStatus(SID._Gen_Watersnaking) != null).OrderByDescending(a => a.Role == Role.Healer).Take(1))
                        Stacks.Add(new(p, 6, 4, 4, WorldState.FutureTime(15.7f)));
                    break;
                default:
                    ReportError($"Unrecognized status param {status.Extra:X} on Deep Blue");
                    break;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Weaponskill_AwesomeSplash:
            case AID._Weaponskill_AwesomeSplash1:
                NumCasts++;
                Spreads.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
                break;
            case AID._Weaponskill_AwesomeSlab:
            case AID._Weaponskill_AwesomeSlab1:
                NumCasts++;
                Stacks.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
                break;
        }
    }
}

class AlleyOopProteans(BossModule module) : Components.GenericBaitAway(module)
{
    private BitMask _targets = default;
    private bool _reverse;

    public static readonly AOEShapeCone Shape = new(60, 15.Degrees());

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CurrentBaits.Count > 0)
            hints.Add(_reverse ? "Proteans: split" : "Proteans: repeat");
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_AlleyOopDoubleDip or AID._Weaponskill_ReverseAlleyOop)
        {
            _reverse = (AID)spell.Action.ID == AID._Weaponskill_ReverseAlleyOop;

            var mask = _targets.NumSetBits() == 0 ? new(~0ul) : _targets;
            foreach (var (_, player) in Raid.WithSlot().IncludedInMask(mask))
                CurrentBaits.Add(new(caster, player, Shape, Module.CastFinishAt(spell)));
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (_targets[pcSlot] && _reverse && ActiveBaitsOn(pc).FirstOrNull() is { } b)
            Arena.AddCone(b.Source.Position, 60, b.Rotation, 30.Degrees(), ArenaColor.Danger);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (_targets[slot] && _reverse && ActiveBaitsOn(actor).FirstOrNull() is { } b)
        {
            if (Raid.WithSlot().ExcludedFromMask(_targets).InShape(new AOEShapeCone(60, 30.Degrees()), b.Source.Position, b.Rotation).Any())
                hints.Add("Bait away from other party!");
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_Watersnaking)
            _targets.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_Watersnaking)
            _targets.Clear(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_AlleyOopDoubleDip1 or AID._Weaponskill_ReverseAlleyOop1)
        {
            NumCasts++;
            CurrentBaits.Clear();
        }
    }
}

class AlleyOopProteanRepeat(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _predicted = [];

    const float Delay = 2.6f;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Weaponskill_AlleyOopDoubleDip1:
                _predicted.Add(new(new AOEShapeCone(60, 15.Degrees()), caster.Position, spell.Rotation, WorldState.FutureTime(Delay)));
                break;

            case AID._Weaponskill_ReverseAlleyOop1:
                _predicted.Add(new(new AOEShapeCone(60, 7.5f.Degrees()), caster.Position, spell.Rotation + 22.5f.Degrees(), WorldState.FutureTime(Delay)));
                _predicted.Add(new(new AOEShapeCone(60, 7.5f.Degrees()), caster.Position, spell.Rotation - 22.5f.Degrees(), WorldState.FutureTime(Delay)));
                break;

            case AID._Weaponskill_AlleyOopDoubleDip2:
            case AID._Weaponskill_ReverseAlleyOop2:
                NumCasts++;
                if (_predicted.Count > 0)
                    _predicted.RemoveAt(0);
                break;
        }
    }
}

class DeepImpactBait(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    private Actor? Source;
    private BitMask _targets;

    public override void Update()
    {
        CurrentBaits.Clear();

        if (Source != null)
        {
            var target = Raid.WithSlot().IncludedInMask(_targets.NumSetBits() > 0 ? _targets : new(0xFF)).Select(p => p.Item2).Farthest(Source.Position);
            if (target != null)
                CurrentBaits.Add(new(Source, target, new AOEShapeCircle(6), Module.CastFinishAt(Source.CastInfo)));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_DeepImpact)
        {
            Source = caster;
            ForbiddenPlayers = Raid.WithSlot().WhereActor(a => a.Role != Role.Tank).Mask();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_DeepImpact1)
        {
            NumCasts++;
            Source = null;
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_Watersnaking)
            _targets.Set(Raid.FindSlot(actor.InstanceID));
    }
}

class DeepImpactKB(BossModule module) : Components.Knockback(module)
{
    private readonly DeepImpactBait _bait = module.FindComponent<DeepImpactBait>()!;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        foreach (var b in _bait.ActiveBaitsOn(actor))
            yield return new(b.Source.Position, 10, b.Activation);
    }
}

class XtremeSpectacularProximity(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_XtremeSpectacular2, new AOEShapeRect(50, 15));
class XtremeSpectacularRaidwideFirst(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_XtremeSpectacular2);
class XtremeSpectacularRaidwideLast(BossModule module) : Components.RaidwideInstant(module, AID._Weaponskill_XtremeSpectacular4, 4.9f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_XtremeSpectacular2)
            Activation = WorldState.FutureTime(Delay);

        if (spell.Action == WatchedAction)
        {
            Activation = default;
            NumCasts++;
        }
    }
}

class FiresnakingRaidwide(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_Firesnaking);
class XtremeFiresnakingRaidwide(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_XtremeFiresnaking);

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1071, NameID = 14370, PrimaryActorOID = (uint)OID.RedHot, PlanLevel = 100)]
public class RM10STheXtremes(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(20))
{
    public Actor? B1() => PrimaryActor;
    public Actor? DeepBlue;
    public Actor? B2() => DeepBlue;

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actor(DeepBlue, ArenaColor.Enemy);
    }

    protected override void UpdateModule()
    {
        DeepBlue ??= Enemies(OID.DeepBlue).FirstOrDefault();
    }
}
