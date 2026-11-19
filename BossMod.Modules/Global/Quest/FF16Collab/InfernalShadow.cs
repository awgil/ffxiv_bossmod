namespace BossMod.Global.Quest.FF16Collab.InfernalShadow;

public enum OID : uint
{
    Boss = 0x3FE4, //R=8.5
    Helper = 0x233C,
    InfernalSword = 0x404F, //R=1.0
    Clive = 0x3FE5, //R=0.5
    DefendClive = 0x1EB947, //R=2.0, must be interacted with to start QuickTimeEvent
}

public enum AID : uint
{
    VulcanBurstVisual = 35043, // Boss->self, 5.2s cast, single-target
    VulcanBurstReal = 35044, // Helper->self, 6.0s cast, range 30 circle
    VulcanBurstStatus = 35045, // Helper->self, 2.8s cast, range 30 circle, apply invisible status effect ID 3787
    VulcanBurstStartActionCombo = 35928, // Helper->self, 5.6s cast, range 30 circle

    IncinerateVisual = 35050, // Boss->self, no cast, single-target
    IncinerateStatus = 35052, // Helper->player/Clive, 2.7s cast, range 5 circle, apply invisible status effect ID 3787
    IncinerateStartActionCombo = 35051, // Helper->player/Clive, 5.4s cast, range 5 circle
    IncinerateStartActionCombo2 = 35936, // Helper->player/Clive, 5.7s cast, range 5 circle
    IncinerateReal = 36051, // Helper->player/Clive, 5.8s cast, range 5 circle

    StartActionCombo = 36149, // Helper->player, no cast, single-target

    SpreadingFireVisual = 36246, // Boss->self, 3.5s cast, single-target
    SpreadingFireStartActionCombo1st = 36251, // Helper->self, 3.8s cast, range 10 circle
    SpreadingFire1st = 36247, // Helper->self, 4.0s cast, range 10 circle
    SpreadingFireStartActionCombo2nd = 36252, // Helper->self, 5.9s cast, range 10-20 donut
    SpreadingFire2nd = 36248, // Helper->self, 6.1s cast, range 10-20 donut
    SpreadingFireStartActionCombo3rd = 36253, // Helper->self, 8.0s cast, range 20-30 donut
    SpreadingFire3rd = 36249, // Helper->self, 8.2s cast, range 20-30 donut
    SpreadingFireStartActionCombo4th = 36254, // Helper->self, 10.1s cast, range 30-40 donut
    SpreadingFire4th = 36250, // Helper->self, 10.3s cast, range 30-40 donut

    SmolderingClawVisual = 34791, // Boss->self, 2.3s cast, single-target
    SmolderingClawStartActionCombo = 35925, // Helper->self, 4.1s cast, range 40 150 degree cone
    SmolderingClawStartActionCombo2 = 34793, // Helper->self, 0.6s cast, range 40 150-degree cone
    SmolderingClawReal = 34792, // Helper->self, 4.5s cast, range 40 150-degree cone

    FireballStartVisual = 35040, // Boss->self, 2.6s cast, single-target
    FireballStartActionCombo = 35927, // Helper->location, 3.9s cast, range 6 circle
    FireballStartActionCombo2 = 35042, // Helper->location, 0.5s cast, range 6 circle
    FireballStartActionCombo3 = 35041, // Helper->location, 3.5s cast, range 6 circle
    FireballReal = 36050, // Helper->location, 4.3s cast, range 6 circle

    CrimsonRushMovement = 35037, // Boss->location, 3.5s cast, single-target
    CrimsonRushStartActionCombo = 35926, // Helper->location, 3.9s cast, width 20 rect charge
    CrimsonRushStartActionCombo2 = 35039, // Helper->location, 0.6s cast, width 20 rect charge
    CrimsonRushReal = 35038, // Helper->location, 4.5s cast, width 20 rect charge

    TailStrikeStartActionCombo = 35047, // Boss->self, 3.5s cast, single-target
    TailStrikeStartActionCombo2 = 35049, // Helper->self, 0.6s cast, range 40 150-degree cone
    TailStrikeStartActionCombo3 = 35929, // Helper->self, 4.1s cast, range 40 150-degree cone
    TailStrikeReal = 35048, // Helper->self, 4.5s cast, range 40 150-degree cone

    PyrosaultVisual = 35053, // Boss->location, 4.0s cast, single-target
    PyrosaultReal = 35054, // Helper->location, 5.0s cast, range 40 circle, damage fall off AOE, seems to be fine after about range 10
    PyrosaultStatus = 35055, // Helper->location, 1.8s cast, range 40 circle, apply invisible status effect ID 3787
    PyrosaultStartActionCombo = 35930, // Helper->location, 4.8s cast, range 40 circle

    InfernalShroud = 35046, // Boss->self, 3.0s cast, single-target, applies status effect 3771
    CrimsonStreakStartActionCombo = 35061, // Helper->location, 5.8s cast, width 20 rect charge
    CrimsonStreakReal = 35060, // Helper->location, 9.7s cast, width 20 rect charge
    CrimsonStreakReal2 = 35903, // Helper->location, 9.6s cast, width 20 rect charge, not sure what the purpose is, happens once in each charge sequence at -0.1s of another cast
    CrimsonStreakVisual = 35056, // Boss->location, 9.0s cast, single-target
    CrimsonStreakVisual2 = 35057, // Boss->location, no cast, single-target
    CrimsonStreakVisual3 = 35058, // Boss->location, no cast, single-target
    CrimsonStreakVisual4 = 35059, // Boss->location, no cast, single-target

    Fetters = 35062, // Boss->Clive, 3.0s cast, single-target
    BurningStrikeVisual = 35064, // Boss->self, 3.0s cast, single-target
    BurningStrikeTeleport = 35066, // Boss->location, no cast, single-target
    BurningStrikeEnrage = 35067, // Boss->player, no cast, single-target, QTE failed

    Teleport = 35068, // Boss->location, no cast, single-target
    Knockback = 35069, // Helper->player, no cast, single-target, knockback 4, away from source

    InfernalHowlVisual = 36129, // Boss->self, no cast, single-target
    InfernalHowlReal = 36130, // Helper->self, 0.8s cast, range 40 circle
    GhastlyWind = 36117, // Boss->self, no cast, single-target
    Pull = 36118, // Helper->player, 1.4s cast, single-target, pull 30, between centers, start of QTE
    SearingStompStatus = 35073, // Boss->self, no cast, single-target, applies invisible status effect ID 2056 (extra 27B)
    SearingStompStatus2 = 35077, // Boss->self, no cast, single-target, applies invisible status effect ID 2056 (extra 284)
    SearingStomp2 = 35075, // Boss->self, no cast, single-target
    SearingStompEnrage = 35076, // Boss->self, no cast, range 40 circle, failed QTE

    HellfireVisual = 35080, // Boss->self, 3.0s cast, single-target
    HellfireRaidwide = 35082, // Helper->self, no cast, range 40 circle
    HellfireEnrage = 35081, // Helper->self, no cast, range 40 circle, fail to kill Infernal Swords in time

    FieryRampageVisual = 35083, // Boss->self, 4.5s cast, single-target
    FieryRampageCleaveStartActionCombo = 35085, // Helper->self, 2.8s cast, range 40 180-degree cone
    FieryRampageCleaveStartActionCombo2 = 35932, // Helper->self, 6.7s cast, range 40 180-degree cone
    FieryRampageCleaveStartActionCombo3 = 35087, // Helper->self, 7.0s cast, range 40 180-degree cone
    FieryRampageCleaveStartActionCombo4 = 35933, // Helper->self, 10.8s cast, range 40 180-degree cone
    FieryRampageCleaveReal = 35084, // Helper->self, 6.7s cast, range 40 180-degree cone
    FieryRampageCleaveReal2 = 35086, // Helper->self, 10.9s cast, range 40 180-degree cone
    FieryRampageCircleStartActionCombo = 35089, // Helper->self, 11.2s cast, range 16 circle
    FieryRampageCircleStartActionCombo2 = 35934, // Helper->self, 15.0s cast, range 16 circle
    FieryRampageCircleReal = 35088, // Helper->self, 15.1s cast, range 16 circle
    FieryRampageRaidwideStatus = 35091, // Helper->self, 16.1s cast, range 40 circle, apply invisible status effect ID 3787
    FieryRampageRaidwideStartActionCombo = 35935, // Helper->self, 19.2s cast, range 40 circle
    FieryRampageRaidwideReal = 35090, // Helper->self, 19.3s cast, range 40 circle

    EruptionReal = 36052, // Helper->location, 4.2s cast, range 8 circle
    EruptionReal2 = 36257, // Helper->location, 6.7s cast, range 8 circle
    EruptionReal3 = 36260, // Helper->location, 10.9s cast, range 8 circle
    EruptionReal4 = 36263, // Helper->location, 15.1s cast, range 8 circle
    EruptionStartActionCombo = 35079, // Helper->location, 0.5s cast, range 8 circle
    EruptionStartActionCombo2 = 35931, // Helper->location, 3.6s cast, range 8 circle
    EruptionStartActionCombo3 = 35078, // Helper->location, 4.0s cast, range 8 circle
    EruptionStartActionCombo4 = 36256, // Helper->location, 6.5s cast, range 8 circle
    EruptionStartActionCombo5 = 36258, // Helper->location, 6.6s cast, range 8 circle
    EruptionStartActionCombo6 = 36259, // Helper->location, 10.7s cast, range 8 circle
    EruptionStartActionCombo7 = 36261, // Helper->location, 10.8s cast, range 8 circle
    EruptionStartActionCombo8 = 36262, // Helper->location, 14.9s cast, range 8 circle
    EruptionStartActionCombo9 = 36264, // Helper->location, 15.0s cast, range 8 circle

    ClivePhoenixShift = 35101, // Clive->Boss/InfernalSword, no cast, single-target
    CliveCombo = 35098, // Clive->Boss/InfernalSword, no cast, single-target
    CliveCombo2 = 35099, // Clive->Boss/InfernalSword, no cast, single-target
    CliveAutoAttack = 870, // Clive->Boss, no cast, single-target
    CliveMagicBurst = 35105, // Clive->Boss/InfernalSword, no cast, single-target
    CliveRisingFlames = 35103, // Clive->Boss/InfernalSword, no cast, single-target
    CliveFlamesOfRebirth = 36255, // Clive->self, 5.0s cast, range 20 circle
    CliveDodge1 = 35895, // Clive->location, no cast, single-target
    CliveDodge2 = 35092, // Clive->location, no cast, single-target
    CliveDodge3 = 35093, // Clive->location, no cast, single-target
    CliveDodge4 = 35094, // Clive->location, no cast, single-target
    CliveDodge5 = 35095, // Clive->location, no cast, single-target
    CliveDodge6 = 35096, // Clive->location, no cast, single-target
    ClivePotion = 35106, // Clive->self, no cast, single-target
    CliveAbility = 35097, // Clive->self, no cast, single-target
    CliveScarletCyclone = 35104, // Clive->self, no cast, range 6 circle
    ClivePrecisionStrike = 35100, // Clive->Boss/InfernalSword, no cast, single-target
}

public enum SID : uint
{
    DodgeSoon = 3787, // invisible, 2s before DodgeWindow
    DodgeWindow = 3711, // invisible, dodging now grants Precision
    Precision = 3710,
}

class VulcanBurst(BossModule module) : Components.RaidwideCast(module, AID.VulcanBurstReal, "Time your dodge correctly");
class Hellfire(BossModule module) : Components.RaidwideCastDelay(module, AID.HellfireVisual, AID.HellfireRaidwide, 0.6f);
class Incinerate(BossModule module) : Components.SpreadFromCastTargets(module, AID.IncinerateReal, 5);

class SpreadingFire(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(10), new AOEShapeDonut(10, 20), new AOEShapeDonut(20, 30), new AOEShapeDonut(30, 40)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SpreadingFire1st)
            AddSequence(caster.Position, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (Sequences.Count > 0)
        {
            var order = (AID)spell.Action.ID switch
            {
                AID.SpreadingFire1st => 0,
                AID.SpreadingFire2nd => 1,
                AID.SpreadingFire3rd => 2,
                AID.SpreadingFire4th => 3,
                _ => -1
            };
            AdvanceSequence(order, caster.Position, WorldState.FutureTime(2));
        }
    }
}

class SmolderingClaw(BossModule module) : Components.StandardAOEs(module, AID.SmolderingClawReal, new AOEShapeCone(40, 75.Degrees()));
class TailStrike(BossModule module) : Components.StandardAOEs(module, AID.TailStrikeReal, new AOEShapeCone(40, 75.Degrees()));

class FireRampageCleave(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(40, 90.Degrees());
    private readonly List<(WPos position, Angle rotation, DateTime activation, uint AID)> _castersunsorted = [];
    private List<(WPos position, Angle rotation, DateTime activation)> _casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_casters.Count > 0)
            yield return new(cone, _casters[0].position, _casters[0].rotation, _casters[0].activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.FieryRampageCleaveReal or AID.FieryRampageCleaveReal2)
        {
            _castersunsorted.Add((caster.Position, spell.Rotation, Module.CastFinishAt(spell), spell.Action.ID)); //casters appear in random order in raw ops
            _casters = [.. _castersunsorted.OrderBy(x => x.AID).Select(x => (x.position, x.rotation, x.activation))];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_casters.Count > 0 && (AID)spell.Action.ID is AID.FieryRampageCleaveReal or AID.FieryRampageCleaveReal2)
        {
            _casters.RemoveAt(0);
            _castersunsorted.Clear();
        }
    }
}

class FieryRampageCircle(BossModule module) : Components.StandardAOEs(module, AID.FieryRampageCircleReal, new AOEShapeCircle(16));
class FieryRampageRaidwide(BossModule module) : Components.RaidwideCast(module, AID.FieryRampageRaidwideReal, "Time your dodge correctly");
class PyrosaultReal(BossModule module) : Components.StandardAOEs(module, AID.PyrosaultReal, new AOEShapeCircle(10));
class Fireball(BossModule module) : Components.StandardAOEs(module, AID.FireballReal, 6);
class CrimsonRush(BossModule module) : Components.ChargeAOEs(module, AID.CrimsonRushReal, 10);

class CrimsonStreak(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(WPos source, AOEShape shape, Angle direction, DateTime activation)> _casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_casters.Count > 0)
            yield return new(_casters[0].shape, _casters[0].source, _casters[0].direction, _casters[0].activation, ArenaColor.Danger);
        if (_casters.Count > 1)
            yield return new(_casters[1].shape, _casters[1].source, _casters[1].direction, _casters[1].activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.CrimsonStreakReal)
        {
            var dir = spell.LocXZ - caster.Position;
            _casters.Add((caster.Position, new AOEShapeRect(dir.Length(), 10), Angle.FromDirection(dir), Module.CastFinishAt(spell)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_casters.Count > 0 && (AID)spell.Action.ID == AID.CrimsonStreakReal)
            _casters.RemoveAt(0);
    }
}

class Eruption(BossModule module) : Components.StandardAOEs(module, AID.EruptionReal, 8);

class Eruption2(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(WPos position, DateTime activation, uint AID)> _castersunsorted = [];
    private List<(WPos position, DateTime activation)> _casters = [];
    private static readonly AOEShapeCircle circle = new(8);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (NumCasts < 10)
        {
            if (NumCasts < 6 ? _casters.Count > 2 : _casters.Count > 3)
                for (var i = 0; NumCasts < 6 ? i < 3 : i < 4; ++i)
                    yield return new(circle, _casters[i].position, default, _casters[i].activation, ArenaColor.Danger);
            if (NumCasts < 3 ? _casters.Count > 5 : _casters.Count > 6)
                for (var i = 3; NumCasts < 3 ? i < 6 : i < 7; ++i)
                    yield return new(circle, _casters[i].position, default, _casters[i].activation);
        }
        if (NumCasts >= 10)
        {
            if (_casters.Count > 3)
                for (var i = 0; _casters.Count > 6 ? i < 4 : i < 6; ++i)
                    yield return new(circle, _casters[i].position, default, _casters[i].activation, ArenaColor.Danger);
            if (_casters.Count > 7)
                for (var i = 4; _casters.Count > 10 ? i < 8 : i < 10; ++i)
                    yield return new(circle, _casters[i].position, default, _casters[i].activation);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.EruptionReal2 or AID.EruptionReal3 or AID.EruptionReal4)
        {
            _castersunsorted.Add((spell.LocXZ, Module.CastFinishAt(spell), spell.Action.ID));
            _casters = [.. _castersunsorted.OrderBy(x => x.AID).Select(x => (x.position, x.activation))];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_casters.Count > 0 && (AID)spell.Action.ID is AID.EruptionReal2 or AID.EruptionReal3 or AID.EruptionReal4)
        {
            _casters.RemoveAt(0);
            ++NumCasts;
            if (_casters.Count == 0)
                _castersunsorted.Clear();
        }
    }
}

class BurningStrike(BossModule module) : BossComponent(module)
{
    private bool casting;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.BurningStrikeVisual)
            casting = true;
    }

    public override void Update()
    {
        var defendtargetable = Module.Enemies(OID.DefendClive).FirstOrDefault(x => x.IsTargetable);
        if (defendtargetable != null && casting)
            casting = false;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var defendtargetable = Module.Enemies(OID.DefendClive).FirstOrDefault(x => x.IsTargetable);
        if (casting && defendtargetable == null)
            hints.Add("Prepare to defend Clive!");
        if (defendtargetable != null)
            hints.Add($"Interact with {Module.Enemies(OID.DefendClive).FirstOrDefault()!.Name} and solve a QTE!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.InteractWithTarget ??= Module.Enemies(OID.DefendClive).FirstOrDefault(x => x.IsTargetable);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var defendtargetable = Module.Enemies(OID.DefendClive).FirstOrDefault(x => x.IsTargetable);
        if (defendtargetable != null)
            Arena.AddCircle(defendtargetable.Position, 1.4f, ArenaColor.Safe);
    }
}

class PrecisionDodge(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var window = actor.FindStatus(SID.DodgeWindow) != null;
        if (window || actor.FindStatus(SID.DodgeSoon) != null)
            hints.MaxCastTime = 0;
        if (window)
        {
            hints.ForceCancelCast = true;
            var away = Angle.FromDirection(actor.Position - Module.PrimaryActor.Position);
            var dest = actor.Position + 10 * (Module.Center - actor.Position).Normalized();
            for (var i = 0; i < 16; ++i)
            {
                var candidate = actor.Position + 10 * (away + (i % 2 == 0 ? 1 : -1) * ((i + 1) / 2 * 22.5f).Degrees()).ToDirection();
                if (ActionPredicate.IsDashSafe(actor.Position, candidate, hints))
                {
                    dest = candidate;
                    break;
                }
            }
            hints.ActionsToExecute.Push(ActionID.MakeSpell(Roleplay.AID.Dodge), WorldState.Actors.Find(actor.TargetID) ?? Module.PrimaryActor, ActionQueue.Priority.VeryHigh, targetPos: dest.ToVec3(actor.PosRot.Y));
        }
        if (!Module.PrimaryActor.IsTargetable)
            return;
        if (actor.FindStatus(SID.Precision) != null)
            hints.ActionsToExecute.Push(ActionID.MakeSpell(Roleplay.AID.PrecisionStrike), Module.PrimaryActor, ActionQueue.Priority.High);
        hints.ActionsToExecute.Push(ActionID.MakeSpell(Roleplay.AID.RisingFlames), Module.PrimaryActor, ActionQueue.Priority.Medium);
    }
}

class SearingStomp(BossModule module) : BossComponent(module)
{
    private int NumCasts;
    private bool casting;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.InfernalShroud)
        {
            ++NumCasts;
            if (NumCasts == 2)
                casting = true;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.InfernalHowlReal)
            casting = false;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (casting)
            hints.Add("Prepare to solve a QTE!");
    }
}

class InfernalShadowStates : StateMachineBuilder
{
    public InfernalShadowStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<VulcanBurst>()
            .ActivateOnEnter<Incinerate>()
            .ActivateOnEnter<SpreadingFire>()
            .ActivateOnEnter<SmolderingClaw>()
            .ActivateOnEnter<TailStrike>()
            .ActivateOnEnter<CrimsonRush>()
            .ActivateOnEnter<Fireball>()
            .ActivateOnEnter<CrimsonStreak>()
            .ActivateOnEnter<Hellfire>()
            .ActivateOnEnter<FireRampageCleave>()
            .ActivateOnEnter<FieryRampageCircle>()
            .ActivateOnEnter<FieryRampageRaidwide>()
            .ActivateOnEnter<Eruption>()
            .ActivateOnEnter<Eruption2>()
            .ActivateOnEnter<BurningStrike>()
            .ActivateOnEnter<SearingStomp>()
            .ActivateOnEnter<PrecisionDodge>()
            .ActivateOnEnter<PyrosaultReal>()
            .Raw.Update = () => Module.PrimaryActor.HPMP.CurHP == 1;
    }
}

[ModuleInfo(Contributors = "Malediktus, Kagekazu", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70334, NameID = 12564)] // also: CFC 959
public class InfernalShadow(ModuleInit init) : BossModule(init, new(0, 0), new ArenaBoundsCircle(20))
{
    // boss is untargetable during the Infernal Sword phase, so a plugin reload there would never re-activate the module
    protected override bool CheckPull() => PrimaryActor.InCombat || Enemies(OID.InfernalSword).Any(s => s.IsTargetable);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.Clive))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.InfernalSword))
            Arena.Actor(s, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.DefendClive))
            Arena.Actor(s, ArenaColor.Object);
    }
}
