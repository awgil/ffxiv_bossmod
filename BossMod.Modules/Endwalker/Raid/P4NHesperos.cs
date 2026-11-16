namespace BossMod.Endwalker.Raid.P4NHesperos;

public enum OID : uint
{
    Boss = 0x3602, // R5.700, x1
    Helper = 0x233C, // R0.500, x38, Helper type
    SceneSetter = 0x35FE, // R1.000, x4, one per corner, casts before setting the scene
    ExplosiveAether = 0x3603, // R1.000, x8, belone bursts orbs
    Actor3849 = 0x3849, // R1.000, x2
    Actor384A = 0x384A, // R1.000, x2
    ActorFE371 = 0xFE371, // R0.500, x1, EventNpc type
    ActorFE372 = 0xFE372, // R0.500, x1, EventNpc type
    ActorFE373 = 0xFE373, // R0.500, x1, EventNpc type
    Exit = 0x1E850B, // R0.500, x1, EventObj type
    Actor1E8536 = 0x1E8536, // R2.000, x1, EventObj type
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    Ability27082 = 27082, // Boss->self, no cast, single-target
    SettingTheScene = 27083, // Boss->self, 4.0s cast, single-target
    SceneSetterVisual = 27084, // SceneSetter->self, 2.0s cast, single-target
    SceneSetterEnd = 27085, // SceneSetter->self, no cast, single-target
    Ability27086 = 27086, // Boss->location, no cast, single-target
    Pinax = 27087, // Boss->self, 5.0s cast, single-target
    BeloneBursts = 27097, // Boss->self, 5.0s cast, single-target
    BeloneCoils = 27101, // Boss->self, 4.0s cast, single-target
    AcidMekhane = 27192, // Helper->players, 5.0s cast, range 5 circle spread
    LavaMekhane = 27193, // Helper->players, 5.0s cast, range 6 circle, not seen yet
    WellMekhane = 27194, // Helper->self, 1.0s cast, range 60 circle, knockback 13 from center
    LevinstrikeMekhane = 27195, // Helper->self, 2.0s cast, range 60 circle, damage falls off with distance from center
    AcidPinax = 27196, // Helper->self, 9.0s cast, single-target, quadrant
    LavaPinax = 27197, // Helper->self, 9.0s cast, single-target, quadrant, not seen yet
    WellPinax = 27198, // Helper->self, 9.0s cast, single-target, quadrant
    LevinstrikePinax = 27199, // Helper->self, 9.0s cast, single-target, quadrant
    Bloodrake = 27200, // Boss->self, 5.0s cast, range 40 circle
    BurstOrbTank = 27201, // ExplosiveAether->player, no cast, single-target
    BurstOrbHealer = 27202, // ExplosiveAether->player, no cast, single-target
    BurstOrbDPS = 27203, // ExplosiveAether->player, no cast, single-target
    BurstTowerNoTank = 27204, // Helper->self, 10.0s cast, range 4 circle
    BurstTowerNoHealer = 27205, // Helper->self, 10.0s cast, range 4 circle
    BurstTowerNoDPS = 27206, // Helper->self, 10.0s cast, range 4 circle
    NortherlyShiftSword = 27210, // Boss->location, 7.0s cast, single-target
    SoutherlyShiftSword = 27211, // Boss->self, 7.0s cast, single-target
    EasterlyShiftSword = 27212, // Boss->self, 7.0s cast, single-target
    WesterlyShiftSword = 27213, // Boss->self, 7.0s cast, single-target
    ShiftingStrikeSword = 27214, // Helper->self, 8.5s cast, range 60 120-degree cone
    HellSkewer = 27215, // Boss->self, 5.0s cast, range 60 width 6 rect
    ElegantEvisceration = 27216, // Boss->players, 5.0s cast, range 5 circle
    Decollation = 27217, // Boss->self, 5.0s cast, range 40 circle
    NortherlyShiftCloak = 28078, // Boss->location, 7.0s cast, single-target
    SoutherlyShiftCloak = 28079, // Boss->self, 7.0s cast, single-target
    EasterlyShiftCloak = 28080, // Boss->self, 7.0s cast, single-target
    WesterlyShiftCloak = 28081, // Boss->self, 7.0s cast, single-target
    ShiftingStrikeCloak = 28082, // Helper->self, 8.7s cast, range 60 circle, knockback
}

public enum SID : uint
{
    OrbRole = 2056, // none->ExplosiveAether, extra=0x13A tank, 0x13B dps, 0x13C healer
    ThriceComeRuin = 2530, // ExplosiveAether->player, extra=stacks
}

public enum IconID : uint
{
    AcidMekhane = 96, // player
    ElegantEvisceration = 218, // player
    ShiftSword = 312, // Helper
    ShiftCloak = 313, // Helper
}

public enum TetherID : uint
{
    Orb = 17, // ExplosiveAether->player
}

class ElegantEvisceration(BossModule module) : Components.BaitAwayCast(module, AID.ElegantEvisceration, new AOEShapeCircle(5), true);
class Bloodrake(BossModule module) : Components.RaidwideCast(module, AID.Bloodrake);
class Decollation(BossModule module) : Components.RaidwideCast(module, AID.Decollation);
class HellSkewer(BossModule module) : Components.StandardAOEs(module, AID.HellSkewer, new AOEShapeRect(60, 3));
class ShiftingStrikeSword(BossModule module) : Components.StandardAOEs(module, AID.ShiftingStrikeSword, new AOEShapeCone(60, 60.Degrees()));
class ShiftingStrikeCloak(BossModule module) : Components.KnockbackFromCastTarget(module, AID.ShiftingStrikeCloak, 23);
class ShiftDirection(BossModule module) : BossComponent(module)
{
    private string? _hint;

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_hint != null)
            hints.Add(_hint);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var hint = (AID)spell.Action.ID switch
        {
            AID.NortherlyShiftSword => "Sword cleave from the north",
            AID.SoutherlyShiftSword => "Sword cleave from the south",
            AID.EasterlyShiftSword => "Sword cleave from the east",
            AID.WesterlyShiftSword => "Sword cleave from the west",
            AID.NortherlyShiftCloak => "Knockback from the north",
            AID.SoutherlyShiftCloak => "Knockback from the south",
            AID.EasterlyShiftCloak => "Knockback from the east",
            AID.WesterlyShiftCloak => "Knockback from the west",
            _ => null
        };
        if (hint != null)
            _hint = hint;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ShiftingStrikeSword or AID.ShiftingStrikeCloak)
            _hint = null;
    }
}

class AcidMekhane(BossModule module) : Components.SpreadFromCastTargets(module, AID.AcidMekhane, 5);
class LavaMekhane(BossModule module) : Components.StackWithCastTargets(module, AID.LavaMekhane, 6);

class PinaxTileCenters(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle Shape = new(2.5f);
    private readonly bool[] _placed = new bool[4];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        for (int i = 0; i < _placed.Length; ++i)
            if (_placed[i])
                yield return new(Shape, TileCenter(i));
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is >= 5 and <= 20 && state is 0x00020001 or 0x02000001 or 0x08000004)
            _placed[(index - 5) & 3] = state == 0x00020001;
    }

    private WPos TileCenter(int corner) => Module.Center + 10 * (corner switch
    {
        0 => new WDir(+1, -1),
        1 => new WDir(+1, +1),
        2 => new WDir(-1, +1),
        _ => new WDir(-1, -1)
    });
}

class LevinstrikeMekhane(BossModule module) : Components.GenericAOEs(module, AID.LevinstrikeMekhane, "Go to a corner!")
{
    private static readonly AOEShapeCircle Shape = new(20);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.LevinstrikePinax)
            _aoe = new(Shape, Module.Center, default, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.LevinstrikeMekhane)
            _aoe = null;
    }
}

class WellMekhane(BossModule module) : Components.Knockback(module, AID.WellMekhane)
{
    private DateTime _activation;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_activation != default)
            yield return new(Module.Center, 13, _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.WellPinax)
            _activation = Module.CastFinishAt(spell);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.WellMekhane)
            _activation = default;
    }
}

class BeloneCoils(BossModule module) : Components.GenericTowers(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        Func<Actor, bool>? banned = (AID)spell.Action.ID switch
        {
            AID.BurstTowerNoTank => a => a.Role == Role.Tank,
            AID.BurstTowerNoHealer => a => a.Role == Role.Healer,
            AID.BurstTowerNoDPS => a => a.Role is Role.Melee or Role.Ranged,
            _ => null
        };
        if (banned != null)
            Towers.Add(new(caster.Position, 4, 1, 8, Raid.WithSlot().WhereActor(banned).Mask(), Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.BurstTowerNoTank or AID.BurstTowerNoHealer or AID.BurstTowerNoDPS)
        {
            ++NumCasts;
            Towers.RemoveAll(t => t.Position.AlmostEqual(caster.Position, 1));
        }
    }
}

class BeloneBursts(BossModule module) : BossComponent(module)
{
    private const float PopRadius = 2.5f;
    private readonly Dictionary<ulong, Role> _orbRoles = [];
    private readonly int[] _ruin = new int[PartyState.MaxPartySize];

    private static Role GroupRole(Role r) => r == Role.Ranged ? Role.Melee : r;
    private bool Lethal(int slot, Actor player, Role orbRole) => _ruin[slot] >= 2 || orbRole == GroupRole(player.Role);
    private IEnumerable<(Actor orb, Role role)> LiveOrbs => Module.Enemies(OID.ExplosiveAether).Where(o => !o.IsDead && _orbRoles.ContainsKey(o.InstanceID)).Select(o => (o, _orbRoles[o.InstanceID]));

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_orbRoles.Count == 0)
            return;
        if (LiveOrbs.Any(o => Lethal(slot, actor, o.role) && o.orb.Position.InCircle(actor.Position, PopRadius)))
            hints.Add("GTFO from wrong orb!");
        else if (_ruin[slot] < 2)
            hints.Add("Intercept an orb of another role", false);
        else
            hints.Add("Avoid orbs", false);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var (orb, role) in LiveOrbs)
            if (Lethal(slot, actor, role))
                hints.AddForbiddenZone(ShapeDistance.Circle(orb.Position, PopRadius + 1));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var (orb, role) in LiveOrbs)
        {
            var lethal = Lethal(pcSlot, pc, role);
            Arena.Actor(orb, lethal ? ArenaColor.Enemy : ArenaColor.Object, true);
            Arena.AddCircle(orb.Position, PopRadius, lethal ? ArenaColor.Danger : ArenaColor.Safe);
            if (WorldState.Actors.Find(orb.Tether.Target) is var target && target != null)
                Arena.AddLine(orb.Position, target.Position, ArenaColor.Danger);
        }
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.OrbRole:
                _orbRoles[actor.InstanceID] = status.Extra switch
                {
                    0x13A => Role.Tank,
                    0x13C => Role.Healer,
                    _ => Role.Melee
                };
                break;
            case SID.ThriceComeRuin:
                if (Raid.TryFindSlot(actor, out var slot))
                    _ruin[slot] = status.Extra;
                break;
        }
    }

    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.ThriceComeRuin && Raid.TryFindSlot(actor, out var slot))
            _ruin[slot] = 0;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.BurstOrbTank or AID.BurstOrbHealer or AID.BurstOrbDPS)
            _orbRoles.Remove(caster.InstanceID);
    }
}

class P4NHesperosStates : StateMachineBuilder
{
    public P4NHesperosStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ElegantEvisceration>()
            .ActivateOnEnter<Bloodrake>()
            .ActivateOnEnter<Decollation>()
            .ActivateOnEnter<HellSkewer>()
            .ActivateOnEnter<ShiftingStrikeSword>()
            .ActivateOnEnter<ShiftingStrikeCloak>()
            .ActivateOnEnter<ShiftDirection>()
            .ActivateOnEnter<AcidMekhane>()
            .ActivateOnEnter<LavaMekhane>()
            .ActivateOnEnter<PinaxTileCenters>()
            .ActivateOnEnter<LevinstrikeMekhane>()
            .ActivateOnEnter<WellMekhane>()
            .ActivateOnEnter<BeloneCoils>()
            .ActivateOnEnter<BeloneBursts>();
    }
}

[ModuleInfo(Contributors = "Nonu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 800, NameID = 10742)]
public class P4NHesperos(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsSquare(20));
