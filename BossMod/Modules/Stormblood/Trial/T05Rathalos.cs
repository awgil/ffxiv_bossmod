namespace BossMod.Stormblood.Trial;

public enum OID : uint
{
    Boss = 0x2129, // R5.460, x?
    Helper = 0x18D6, // R0.500, x?, mixed types
    WyvernsTail = 0x23C0, // R3.900, x?, Part type
    SteppeSheep = 0x212B, // R0.700, x?
    SteppeYamaa = 0x212C, // R1.920, x?
    SteppeYamaa1 = 0x212D, // R1.920, x?
    SteppeCoeurl = 0x212E, // R3.150, x?
    Garula = 0x212A, // R4.000, x?
    Fireball = 0x1E9927
}

public enum AID : uint
{
    Roar = 11460, // Boss->self, no cast, range 50+R circle
    MangleVisual = 10346, // Boss->self, 2.5s cast, range 10 120-degree cone
    Mangle = 11449, // Helper->self, no cast, range 10 120-degree cone
    TailSmash = 10347, // Helper->self, no cast, range 11 ?-degree cone, dunno what this is (see ex)
    RushVisual1 = 10349, // Boss->location, 2.0s cast, width 9 rect charge
    Rush1 = 11452, // Helper->location, no cast, width 9 rect charge
    TailSwingVisual = 10348, // Boss->self, no cast, range 11 180-degree cone
    TailSwing = 11451, // Helper->self, no cast, range 11 180-degree cone
    Roar2 = 10356, // Boss->self, no cast, range 50+R circle
    KingOfTheSkiesVisual = 10357, // Boss->location, no cast, range 50 circle
    KingOfTheSkies = 11546, // Helper->location, no cast, range 50 circle
    SweepingFlamesVisual = 10361, // Boss->self, no cast, range 11 120-degree cone
    SweepingFlames = 11457, // Helper->self, no cast, range 11 120-degree cone
    Mangle2Visual = 10362, // Boss->self, 1.0s cast, range 9 90-degree cone
    Mangle2 = 11458, // Helper->self, no cast, range 9 90-degree cone
    RushVisual2 = 10360, // Boss->location, 1.5s cast, width 10 rect charge
    Rush2 = 11456, // Helper->location, no cast, width 9 rect charge
    FireballBossFirst = 10358, // Boss->player, 5.0s cast, range 5 circle
    FireballFirst = 11450, // Helper->player, no cast, range 5 circle
    VeniVidiVici = 21847, // Boss->location, no cast, width 10 rect charge

    GarulaRush = 10367, // Garula->Boss, 2.0s cast, width 8 rect charge, stuns boss
    CoeurlAuto = 870, // SteppeCoeurl->player/Boss, no cast, single-target
    MobAutos = 872, // SteppeSheep/SteppeYamaa/SteppeYamaa1/Garula->player/Boss, no cast, single-target
    Lullaby = 10363, // SteppeSheep->self, 3.0s cast, range 3+R circle
}

class Mangle(BossModule module) : Components.StandardAOEs(module, AID.MangleVisual, new AOEShapeCone(10f, 60.Degrees()));
class Rush2(BossModule module) : Components.ChargeAOEs(module, AID.GarulaRush, 4f);
class Lullaby(BossModule module) : Components.StandardAOEs(module, AID.Lullaby, 3.7f);
//class HeadButt(BossModule module) : Components.StandardAOEs(module, AID.HeadButt, new AOEShapeRect(4.92f, 1.5f));

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
class FirePuddle(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 5, AID.FireballFirst, m => m.Enemies(OID.Fireball).Where(e => e.EventState != 7), 0.5f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if ((AID)spell.Action.ID == AID.FireballFirst)
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

class T05RathalosStates : StateMachineBuilder
{
    public T05RathalosStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Mangle>()
            .ActivateOnEnter<Rush2>()
            .ActivateOnEnter<Lullaby>()
            .ActivateOnEnter<Rush>()
            .ActivateOnEnter<TailSwing>()
            .ActivateOnEnter<Mangle2>()
            .ActivateOnEnter<FireballStack1>()
            .ActivateOnEnter<FirePuddle>()
            .ActivateOnEnter<KingOfTheSkies>()
            .ActivateOnEnter<SweepingFlames>()
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<TargetHints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 474, NameID = 7221)]
public class T05Rathalos(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));

