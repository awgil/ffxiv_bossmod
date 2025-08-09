#pragma warning disable CA1707 // Identifiers should not contain underscores

namespace BossMod.Stormblood.Extreme.Ex5Rathalos;

public enum OID : uint
{
    Boss = 0x212F, // R5.460, x1
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
    _Weaponskill_Roar = 11459, // Boss->self, no cast, range 50+R circle
    _Weaponskill_Mangle = 10323, // Boss->self, 2.5s cast, range 10 ?-degree cone
    _Weaponskill_Mangle1 = 10332, // 18D6->self, no cast, range 10 ?-degree cone
    _Weaponskill_TailSmash = 10324, // 18D6->self, no cast, range 11 ?-degree cone
    _Weaponskill_Rush = 10326, // Boss->location, 2.0s cast, width 9 rect charge
    _Weaponskill_Rush1 = 10813, // 18D6->location, no cast, width 9 rect charge
    _Weaponskill_TailSwing = 10325, // Boss->self, no cast, range 11 ?-degree cone
    _Weaponskill_TailSwing1 = 10812, // 18D6->self, no cast, range 11 ?-degree cone
    _AutoAttack_Attack = 872, // 2133/2132/2131/2130->player/Boss, no cast, single-target
    _AutoAttack_Attack1 = 870, // 2134->player/Boss, no cast, single-target
    _Weaponskill_Roar1 = 10333, // Boss->self, no cast, range 50+R circle
    _Weaponskill_Rush2 = 10344, // 2130->Boss, 2.0s cast, width 8 rect charge
    _Ability_Lanolin = 10328, // 2133->self, 2.5s cast, single-target
    _Weaponskill_KingOfTheSkies = 10334, // Boss->location, no cast, range 50 circle
    _Weaponskill_KingOfTheSkies1 = 11545, // 18D6->location, no cast, range 50 circle
    _Weaponskill_SweepingFlames = 10338, // Boss->self, no cast, range 11 ?-degree cone
    _Weaponskill_SweepingFlames1 = 11446, // 18D6->self, no cast, range 11 ?-degree cone
    _Weaponskill_Mangle2 = 10339, // Boss->self, 0.7s cast, range 9 ?-degree cone
    _Weaponskill_Mangle3 = 11447, // 18D6->self, no cast, range 9 ?-degree cone
    _Weaponskill_Fireball = 10335, // Boss->player, 5.0s cast, range 5 circle
    _Weaponskill_Fireball1 = 10336, // 18D6->player, no cast, range 5 circle
    _Weaponskill_Fireball2 = 11530, // Boss->player, 3.0s cast, range 5 circle
    _Weaponskill_Fireball3 = 11531, // 18D6->player, no cast, range 5 circle
    _Weaponskill_Rush3 = 10337, // Boss->location, 1.0s cast, width 10 rect charge
    _Weaponskill_Rush4 = 11445, // 18D6->location, no cast, width 9 rect charge
    _Weaponskill_VeniVidiVici = 21847, // Boss->location, no cast, width 10 rect charge
    _Weaponskill_Lullaby = 10340, // 2131->self, 3.0s cast, range 3+R circle
    _Weaponskill_HeadButt = 10341, // 2133->location, 2.5s cast, range 3+R width 3 rect
}

class Mangle(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Mangle, new AOEShapeCone(10f, 60.Degrees()));
class Rush2(BossModule module) : Components.ChargeAOEs(module, AID._Weaponskill_Rush2, 4f);
class Lullaby(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Lullaby, 3.7f);
class HeadButt(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_HeadButt, new AOEShapeRect(4.92f, 1.5f));

class Rush(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _nextAOE;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_nextAOE);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_Rush or AID._Weaponskill_Rush3)
        {
            var delay = (AID)spell.Action.ID is AID._Weaponskill_Rush ? 0.6f : 1.3f;

            var chargeDir = spell.LocXZ - caster.Position;
            _nextAOE = new(new AOEShapeRect(chargeDir.Length() + 6, 4.5f), caster.Position, chargeDir.ToAngle(), Module.CastFinishAt(spell, delay));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_Rush1 or AID._Weaponskill_Rush4)
        {
            _nextAOE = null;
        }
    }
}

class TailSwing(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_TailSwing)
{
    private AOEInstance? _nextAOE;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_nextAOE);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            _nextAOE = new(new AOEShapeCone(11, 90.Degrees()), caster.Position, spell.Rotation - 90.Degrees(), WorldState.FutureTime(1.9f));
        }

        if ((AID)spell.Action.ID == AID._Weaponskill_TailSwing1)
            _nextAOE = null;
    }
}

class SweepingFlames(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_SweepingFlames)
{
    private AOEInstance? _nextAOE;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_nextAOE);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            _nextAOE = new(new AOEShapeCone(11, 60.Degrees()), caster.Position, spell.Rotation, WorldState.FutureTime(1.5f));
        }

        if ((AID)spell.Action.ID == AID._Weaponskill_SweepingFlames1)
            _nextAOE = null;
    }
}

class Mangle2(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_Mangle2)
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
        if ((AID)spell.Action.ID == AID._Weaponskill_Mangle3)
            _nextAOE = null;
    }
}

class FireballStack1(BossModule module) : Components.StackWithCastTargets(module, AID._Weaponskill_Fireball, 5);
class FireballStack2(BossModule module) : Components.StackWithCastTargets(module, AID._Weaponskill_Fireball2, 5);
class FirePuddle(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 5, AID._Weaponskill_Fireball1, m => m.Enemies(OID.Fireball).Where(e => e.EventState != 7), 0.5f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if ((AID)spell.Action.ID is AID._Weaponskill_Fireball1 or AID._Weaponskill_Fireball3)
        {
            _predictedByEvent.Add((WorldState.Actors.Find(spell.MainTargetID)?.Position ?? spell.TargetXZ, WorldState.FutureTime(CastEventToSpawn)));
        }
    }
}

class KingOfTheSkies(BossModule module) : Components.GenericLineOfSightAOE(module, AID._Weaponskill_KingOfTheSkies, 100, false)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
            Modify(new(100, 82), Module.Enemies(OID.Garula).Select(e => (e.Position, e.HitboxRadius)), WorldState.FutureTime(7));

        if ((AID)spell.Action.ID == AID._Weaponskill_KingOfTheSkies1)
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

class RathalosStates : StateMachineBuilder
{
    public RathalosStates(BossModule module) : base(module)
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

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 475, NameID = 7221)]
public class Rathalos(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(24.5f));

