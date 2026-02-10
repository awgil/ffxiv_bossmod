namespace BossMod.Stormblood.Extreme.Ex5Rathalos;

public enum OID : uint
{
    Boss = 0x212F, // R5.460, x1
    Helper = 0x18D6, // R1.300, x1, mixed
    WyvernsTail = 0x23D9, // R3.900, x1, Part type
    SteppeSheep = 0x2131, // R0.700, x0 (spawn during fight)
    SteppeYamaa = 0x2132, // R1.920, x0 (spawn during fight)
    SteppeYamaa1 = 0x2133, // R1.920, x0 (spawn during fight)
    SteppeCoeurl = 0x2134, // R3.150, x0 (spawn during fight)
    Garula = 0x2130, // R4.000, x0 (spawn during fight)
    Fireball = 0x1E9927
}

public enum AID : uint
{
    Roar1 = 11459, // Boss->self, no cast, range 50+R circle
    MangleVisual = 10323, // Boss->self, 2.5s cast, range 10 120-degree cone
    Mangle = 10332, // Helper->self, no cast, range 10 120-degree cone
    TailSmash = 10324, // Helper->self, no cast, range 11 ?-degree cone, this gets used immediately after Mangle but i guess it only hits behind him? i don't feel like testing it fuck that
    RushVisual1 = 10326, // Boss->location, 2.0s cast, width 9 rect charge
    Rush1 = 10813, // Helper->location, no cast, width 9 rect charge
    TailSwingVisual = 10325, // Boss->self, no cast, range 11 180-degree cone
    TailSwing = 10812, // Helper->self, no cast, range 11 180-degree cone
    Roar2 = 10333, // Boss->self, no cast, range 50+R circle, applies stun
    KingOfTheSkiesVisual = 10334, // Boss->location, no cast, range 50 circle
    KingOfTheSkies = 11545, // Helper->location, no cast, range 50 circle
    SweepingFlamesVisual = 10338, // Boss->self, no cast, range 11 120-degree cone
    SweepingFlames = 11446, // Helper->self, no cast, range 11 120-degree cone
    Mangle2Visual = 10339, // Boss->self, 0.7s cast, range 9 90-degree cone
    Mangle2 = 11447, // Helper->self, no cast, range 9 90-degree cone
    FireballBossFirst = 10335, // Boss->player, 5.0s cast, range 5 circle
    FireballFirst = 10336, // Helper->player, no cast, range 5 circle
    FireballBossRest = 11530, // Boss->player, 3.0s cast, range 5 circle
    FireballRest = 11531, // Helper->player, no cast, range 5 circle
    RushVisual2 = 10337, // Boss->location, 1.0s cast, width 10 rect charge
    Rush2 = 11445, // Helper->location, no cast, width 9 rect charge
    VeniVidiVici = 21847, // Boss->location, no cast, width 10 rect charge

    GarulaRush = 10344, // Garula->Boss, 2.0s cast, width 8 rect charge, stuns boss
    CoeurlAuto = 870, // SteppeCoeurl->player/Boss, no cast, single-target
    MobAutos = 872, // SteppeYamaa1/SteppeYamaa/SteppeSheep/Garula->player/Boss, no cast, single-target
    Lanolin = 10328, // SteppeYamaa1->self, 2.5s cast, single-target
    Lullaby = 10340, // SteppeSheep->self, 3.0s cast, range 3+R circle
    HeadButt = 10341, // SteppeYamaa1->location, 2.5s cast, range 3+R width 3 rect
}

class Mangle(BossModule module) : Components.StandardAOEs(module, AID.MangleVisual, new AOEShapeCone(10f, 60.Degrees()));
class Rush2(BossModule module) : Components.ChargeAOEs(module, AID.GarulaRush, 4f);
class Lullaby(BossModule module) : Components.StandardAOEs(module, AID.Lullaby, 3.7f);
class HeadButt(BossModule module) : Components.StandardAOEs(module, AID.HeadButt, new AOEShapeRect(4.92f, 1.5f));

class Rush(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _nextAOE;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_nextAOE);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.RushVisual1 or AID.RushVisual2)
        {
            var delay = (AID)spell.Action.ID is AID.RushVisual1 ? 0.6f : 1.3f;

            var chargeDir = spell.LocXZ - caster.Position;
            _nextAOE = new(new AOEShapeRect(chargeDir.Length() + 6, 4.5f), caster.Position, chargeDir.ToAngle(), Module.CastFinishAt(spell, delay));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Rush1 or AID.Rush2)
        {
            _nextAOE = null;
        }
    }
}

class TailSwing(BossModule module) : Components.GenericAOEs(module, AID.TailSwingVisual)
{
    private AOEInstance? _nextAOE;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_nextAOE);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            _nextAOE = new(new AOEShapeCone(11, 90.Degrees()), caster.Position, spell.Rotation - 90.Degrees(), WorldState.FutureTime(1.9f));
        }

        if ((AID)spell.Action.ID == AID.TailSwing)
            _nextAOE = null;
    }
}

class SweepingFlames(BossModule module) : Components.GenericAOEs(module, AID.SweepingFlamesVisual)
{
    private AOEInstance? _nextAOE;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_nextAOE);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            _nextAOE = new(new AOEShapeCone(11, 60.Degrees()), caster.Position, spell.Rotation, WorldState.FutureTime(1.5f));
        }

        if ((AID)spell.Action.ID == AID.SweepingFlames)
            _nextAOE = null;
    }
}

class Mangle2(BossModule module) : Components.GenericAOEs(module, AID.Mangle2Visual)
{
    private AOEInstance? _nextAOE;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_nextAOE);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            _nextAOE = new(new AOEShapeCone(9, 45.Degrees()), caster.Position, spell.Rotation, Module.CastFinishAt(spell, 0.6f));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Mangle2)
            _nextAOE = null;
    }
}

class FireballStack1(BossModule module) : Components.StackWithCastTargets(module, AID.FireballBossFirst, 5);
class FireballStack2(BossModule module) : Components.StackWithCastTargets(module, AID.FireballBossRest, 5);
class FirePuddle(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 5, AID.FireballFirst, m => m.Enemies(OID.Fireball).Where(e => e.EventState != 7), 0.5f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if ((AID)spell.Action.ID is AID.FireballFirst or AID.FireballRest)
        {
            _predictedByEvent.Add((WorldState.Actors.Find(spell.MainTargetID)?.Position ?? spell.TargetXZ, WorldState.FutureTime(CastEventToSpawn)));
        }
    }
}

class KingOfTheSkies(BossModule module) : Components.GenericLineOfSightAOE(module, AID.KingOfTheSkiesVisual, 100, false)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
            Modify(new(100, 82), Module.Enemies(OID.Garula).Select(e => (e.Position, e.HitboxRadius)), WorldState.FutureTime(7));

        if ((AID)spell.Action.ID == AID.KingOfTheSkies)
            Modify(null, []);
    }
}

class Adds(BossModule module) : Components.AddsMulti(module, [OID.SteppeYamaa, OID.SteppeYamaa1, OID.SteppeSheep, OID.SteppeCoeurl, OID.Garula]);

class TargetHints(BossModule module) : BossComponent(module)
{
    private Actor? Tail;

    public override void OnTargetable(Actor actor)
    {
        if (actor.OID == (uint)OID.WyvernsTail)
            Tail = actor;
    }

    public override void OnUntargetable(Actor actor)
    {
        if (actor.OID == (uint)OID.WyvernsTail)
            Tail = null;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (actor.HPRatio < 0.25f)
            hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.MegaPotion), null, ActionQueue.Priority.VeryHigh);

        if (Tail != null && !Tail.IsDead)
        {
            hints.SetPriority(Tail, 1);
            hints.SetPriority(Module.PrimaryActor, AIHints.Enemy.PriorityForbidden);
        }
    }
}

class Ex5RathalosStates : StateMachineBuilder
{
    public Ex5RathalosStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Mangle>()
            .ActivateOnEnter<Rush2>()
            .ActivateOnEnter<Lullaby>()
            .ActivateOnEnter<HeadButt>()
            .ActivateOnEnter<Rush>()
            .ActivateOnEnter<TailSwing>()
            .ActivateOnEnter<Mangle2>()
            .ActivateOnEnter<FireballStack1>()
            .ActivateOnEnter<FireballStack2>()
            .ActivateOnEnter<FirePuddle>()
            .ActivateOnEnter<KingOfTheSkies>()
            .ActivateOnEnter<SweepingFlames>()
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<TargetHints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 475, NameID = 7221)]
public class Ex5Rathalos(ModuleInitializer init) : BossModule(init, new(100, 100), new ArenaBoundsCircle(24.5f));

