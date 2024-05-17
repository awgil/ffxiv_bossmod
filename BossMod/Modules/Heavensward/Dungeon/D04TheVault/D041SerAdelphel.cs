namespace BossMod.Heavensward.Dungeon.D04TheVault.D041SerAdelphel;

public enum OID : uint
{
    Boss = 0x1051, // R2.200, x1
    Helper = 0xD25, // R0.500, x3, mixed types
    BrightSphere = 0x1052, // R1.000, x0 (spawn during fight)
    SerAdelphelBrightblade = 0x104E, // R0.500, x1
    VaultDeacon = 0x1050, // R0.500, x1
    VaultOstiary = 0x104F, // R0.500, x2
}

public enum AID : uint
{
    AutoAttack = 870, // VaultOstiary/Boss->player, no cast, single-target
    FastBlade = 9, // VaultOstiary->player, no cast, single-target
    FastBlade2 = 717, // SerAdelphelBrightblade->player, no cast, single-target
    Bloodstain = 1099, // SerAdelphelBrightblade->self, 2.5s cast, range 5 circle
    Fire = 966, // VaultDeacon->player, 1.0s cast, single-target

    AdventVisual1 = 4979,  // SerAdelphelBrightblade->self, no cast, single-target
    AdventVisual2 = 4122, // SerAdelphelBrightblade->self, no cast, single-target
    AdventVisual3 = 4123, // Boss->self, no cast, single-target
    Advent = 4980, // Helper->self, no cast, range 80 circle, knockback 18, away from source
    Retreat = 4257, // SerAdelphelBrightblade->self, no cast, single-target

    HoliestOfHoly = 4126, // Boss->self, 3.0s cast, range 80+R circle
    HeavenlySlash = 4125, // Boss->self, no cast, range 8+R 90-degree cone
    HolyShieldBash = 4127, // Boss->player, 4.0s cast, single-target
    SolidAscension1 = 4128, // Boss->player, no cast, single-target, on HolyShieldBash target
    SolidAscension2 = 4129, // Helper->player, no cast, single-target, on HolyShieldBash target
    ShiningBlade = 4130, // Boss->location, no cast, width 6 rect charge
    BrightFlare = 4132, // Brightsphere->self, no cast, range 5+R circle

    Execution = 4131, // Boss->location, no cast, range 5 circle

    Visual = 4121, // Boss->self, no cast, single-target
    BossPhase1Vanish = 4256, // SerAdelphelBrightblade->self, no cast, single-target
    BossPhase2Vanish = 4124, // Boss->self, no cast, single-target
}

public enum IconID : uint
{
    Stunmarker = 16, // player
    Spreadmarker = 32, // player
}

class Bloodstain(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Bloodstain), new AOEShapeCircle(5));
class HeavenlySlash(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.HeavenlySlash), new AOEShapeCone(10.2f, 45.Degrees()));
class HoliestOfHoly(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.HoliestOfHoly));
class HolyShieldBash(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.HolyShieldBash), "Stun + single target damage x2");

class BrightSphere(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(6);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
            yield return new(_aoes[0].Shape, _aoes[0].Origin, _aoes[0].Rotation, _aoes[0].Activation, ArenaColor.Danger);
        for (var i = 1; i < _aoes.Count; ++i)
            yield return new(_aoes[i].Shape, _aoes[i].Origin, _aoes[i].Rotation, _aoes[i].Activation);
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.BrightSphere)
            _aoes.Add(new AOEInstance(circle, actor.Position, default, Module.WorldState.FutureTime(4.6f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.BrightFlare && _aoes.Count > 0)
            _aoes.RemoveAt(0);
    }
}

class Execution : Components.BaitAwayIcon
{
    public Execution(BossModule module) : base(module, new AOEShapeCircle(5), (uint)IconID.Spreadmarker, ActionID.MakeSpell(AID.Execution), 4.8f) { CenterAtTarget = true; }
}

class ShiningBlade(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly WPos West = new(-18.509f, -100.023f);
    private static readonly WPos South = new(-0.015f, -81.834f);
    private static readonly WPos North = new(-0.015f, -117.205f);
    private static readonly WPos East = new(18.387f, -100.053f);

    private const int HalfWidth = 3;
    private const float FirstActivationDelay = 0.08f;
    private const float SubsequentActivationDelay = 2.2f;
    private static readonly Angle Angle90Degrees = 90.Degrees();
    private static readonly Angle Angle180Degrees = 180.Degrees();
    private static readonly Angle Angle0Degrees = 0.Degrees();
    private static readonly Angle AngleMinus90Degrees = -90.Degrees();
    private static readonly Angle ConeAngle = 60.Degrees();

    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
            yield return new(_aoes[0].Shape, _aoes[0].Origin, _aoes[0].Rotation, _aoes[0].Activation, ArenaColor.Danger);
        for (var i = 1; i < _aoes.Count; ++i)
            yield return new(_aoes[i].Shape, _aoes[i].Origin, _aoes[i].Rotation, _aoes[i].Activation);
    }

    public override void OnActorNpcYell(Actor actor, ushort id)
    {
        if (id != 2523)
            return;

        var primary = Module.PrimaryActor.Position;
        var activationTimes = GetActivationTimes();

        if (primary.InCone(Module.Center, Angle90Degrees, ConeAngle))
            AddAoeInstances(primary, West, South, North, East, activationTimes);
        else if (primary.InCone(Module.Center, AngleMinus90Degrees, ConeAngle))
            AddAoeInstances(primary, East, North, South, West, activationTimes);
        else if (primary.InCone(Module.Center, Angle180Degrees, ConeAngle))
            AddAoeInstances(primary, South, East, West, North, activationTimes);
        else if (primary.InCone(Module.Center, Angle0Degrees, ConeAngle))
            AddAoeInstances(primary, North, West, East, South, activationTimes);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID == AID.ShiningBlade)
            _aoes.RemoveAt(0);
    }

    private List<DateTime> GetActivationTimes()
    {
        var activation = WorldState.FutureTime(FirstActivationDelay);
        return
        [
            activation,
            activation.AddSeconds(SubsequentActivationDelay),
            activation.AddSeconds(2 * SubsequentActivationDelay),
            activation.AddSeconds(3 * SubsequentActivationDelay)
        ];
    }

    private void AddAoeInstances(WPos primary, WPos first, WPos second, WPos third, WPos fourth, List<DateTime> activationTimes)
    {
        _aoes.Add(new AOEInstance(new AOEShapeRect((first - primary).Length(), HalfWidth), primary, Angle.FromDirection(first - primary), activationTimes[0]));
        _aoes.Add(new AOEInstance(new AOEShapeRect((second - first).Length(), HalfWidth), first, Angle.FromDirection(second - first), activationTimes[1]));
        _aoes.Add(new AOEInstance(new AOEShapeRect((third - second).Length(), HalfWidth), second, Angle.FromDirection(third - second), activationTimes[2]));
        _aoes.Add(new AOEInstance(new AOEShapeRect((fourth - third).Length(), HalfWidth), third, Angle.FromDirection(fourth - third), activationTimes[3]));
    }
}

class D041SerAdelphelStates : StateMachineBuilder
{
    public D041SerAdelphelStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HeavenlySlash>()
            .ActivateOnEnter<Execution>()
            .ActivateOnEnter<Bloodstain>()
            .ActivateOnEnter<BrightSphere>()
            .ActivateOnEnter<HolyShieldBash>()
            .ActivateOnEnter<ShiningBlade>()
            .ActivateOnEnter<HoliestOfHoly>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS), Xyzzy", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 34, NameID = 3634)]
public class D041SerAdelphel(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -100), arena)
{
    protected override bool CheckPull() => PrimaryActor.IsTargetable && PrimaryActor.InCombat || Enemies(OID.SerAdelphelBrightblade).Any(e => e.InCombat);

    private static readonly List<Shape> union = [new Circle(new(0, -100), 19.5f)];
    private static readonly List<Shape> difference = [new Rectangle(new(0, -120), 20, 1.75f), new Rectangle(new(-21, -100), 20, 1.75f, 90.Degrees())];
    public static readonly ArenaBounds arena = new ArenaBoundsComplex(union, difference);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.SerAdelphelBrightblade))
            Arena.Actor(s, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.VaultDeacon))
            Arena.Actor(s, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.VaultOstiary))
            Arena.Actor(s, ArenaColor.Enemy);
    }
}
