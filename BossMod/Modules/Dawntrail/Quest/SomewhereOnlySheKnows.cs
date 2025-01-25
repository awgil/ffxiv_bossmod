/*
namespace BossMod.Dawntrail.Quest.SomewhereOnlySheKnows;

public enum OID : uint
{
    _Gen_SonOfTheKingdom = 0x4295, // R0.750, x?
    _Gen_SonOfTheKingdom1 = 0x4294, // R0.750, x?
    _Gen_TheWingedSteed = 0x4293, // R1.300, x?
    _Gen_TheBirdOfPrey = 0x4297, // R1.960, x?
    _Gen_FlightOfTheGriffin = 0x4296, // R9.200, x?
    Boss = 0x4298, // R4.000, x0 (spawn during fight)
    Helper = 0x233C, // R0.500, x0 (spawn during fight), Helper type
    _Gen_AFlowerInTheSun = 0x4299, // R2.720, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_Attack = 6498, // 4295->player, no cast, single-target
    _AutoAttack_Attack1 = 6497, // 4294/4297/4296->player, no cast, single-target
    _AutoAttack_Attack2 = 6499, // 4293->player, no cast, single-target
    _Weaponskill_BurningBright = 37517, // 4293->self, 3.0s cast, range 47 width 6 rect
    _Weaponskill_SwoopingFrenzy = 37519, // 4296->location, 4.0s cast, range 12 circle
    _Weaponskill_Feathercut = 37522, // 4297->self, 3.0s cast, range 10 width 5 rect
    _Weaponskill_FrigidPulse = 37520, // 4296->self, 5.0s cast, range 4-60 donut
    _Weaponskill_EyeOfTheFierce = 37523, // 4297->self, 5.0s cast, range 40 circle
    _Weaponskill_FervidPulse = 37521, // 4296->self, 5.0s cast, range 50 width 14 cross
    _AutoAttack_ = 37542, // 4298->player, no cast, single-target
    _Weaponskill_FlowerMotif = 37524, // 4298->self, 5.0s cast, single-target
    _Weaponskill_BloodyCaress = 37527, // 4299->self, 5.0s cast, range 60 180-degree cone
    _Weaponskill_ = 37541, // 4298->location, no cast, single-target
    _Weaponskill_FloodInBlue = 37535, // 233C->self, 5.0s cast, range 50 width 10 rect
    _Weaponskill_FloodInBlue1 = 37534, // 4298->self, 5.0s cast, single-target
    _Weaponskill_FloodInBlue2 = 37536, // 233C->self, no cast, range 50 width 5 rect
    _Weaponskill_BlazeInRed = 37539, // Boss->location, 6.0s cast, range 40 circle
    _Weaponskill_ArborMotif = 37525, // Boss->self, 5.0s cast, single-target
    _Weaponskill_TornadoInGreen = 37538, // Boss->self, 5.0s cast, range -40 donut
    _Weaponskill_NineIvies = 37528, // 429A->self, 3.0s cast, single-target
    _Weaponskill_NineIvies1 = 37529, // Helper->self, 3.0s cast, range 50 20-degree cone
    _Weaponskill_1 = 39744, // 429A->self, no cast, single-target
    _Weaponskill_SculptureCast = 37537, // Boss->self, 5.0s cast, range 45 circle
    _Weaponskill_MountainMotif = 37526, // Boss->self, 5.0s cast, single-target
    _Weaponskill_Earthquake = 37531, // Helper->self, 5.0s cast, range 10 circle
    _Weaponskill_Earthquake1 = 37530, // 429B->self, 5.0s cast, single-target
    _Weaponskill_FreezeInCyan = 37540, // Boss->self, 5.0s cast, range 40 45-degree cone
    _Weaponskill_Earthquake2 = 37532, // Helper->self, 7.0s cast, range 10-20 donut
    _Weaponskill_Earthquake3 = 37533, // Helper->self, 9.0s cast, range 20-30 donut
}

class BurningBright(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_BurningBright), new AOEShapeRect(47, 3));
class SwoopingFrenzy(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_SwoopingFrenzy), 12);
class Feathercut(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Feathercut), new AOEShapeRect(10, 2.5f));
class FrigidPulse(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_FrigidPulse), new AOEShapeDonut(11.9f, 60));
class FervidPulse(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_FervidPulse), new AOEShapeCross(50, 7));
class EyeOfTheFierce(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID._Weaponskill_EyeOfTheFierce));
class BloodyCaress(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_BloodyCaress), new AOEShapeCone(60, 90.Degrees()))
{
    private DateTime? Predicted;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (ActiveCasters.Any())
        {
            foreach (var e in base.ActiveAOEs(slot, actor))
                yield return e;
        }
        else if (Module.Enemies(OID._Gen_AFlowerInTheSun).FirstOrDefault() is Actor flower && Predicted is DateTime dt)
            yield return new AOEInstance(Shape, flower.Position, flower.Rotation, dt);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action == WatchedAction)
            Predicted = null;
    }

    public override void OnActorCreated(Actor actor)
    {
        base.OnActorCreated(actor);
        if ((OID)actor.OID == OID._Gen_AFlowerInTheSun)
            Predicted = WorldState.FutureTime(10);
    }
}
class Flood(BossModule module) : Components.Exaflare(module, new AOEShapeRect(50, 2.5f, 50))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_FloodInBlue)
        {
            Lines.Add(new Line()
            {
                Next = caster.Position + new WDir(-2.5f, 0),
                Advance = new(-5, 0),
                Rotation = default,
                NextExplosion = Module.CastFinishAt(spell),
                TimeToMove = 2,
                ExplosionsLeft = 5,
                MaxShownExplosions = 1
            });
            Lines.Add(new Line()
            {
                Next = caster.Position + new WDir(2.5f, 0),
                Advance = new(5, 0),
                Rotation = default,
                NextExplosion = Module.CastFinishAt(spell),
                TimeToMove = 2,
                ExplosionsLeft = 5,
                MaxShownExplosions = 1
            });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_FloodInBlue)
        {
            AdvanceLine(Lines[0], caster.Position + new WDir(-2.5f, 0));
            AdvanceLine(Lines[1], caster.Position + new WDir(2.5f, 0));
        }

        if ((AID)spell.Action.ID == AID._Weaponskill_FloodInBlue2)
        {
            var rectCenter = caster.Position + caster.Rotation.ToDirection().OrthoR() * 2.5f;
            if (Lines.FirstOrDefault(l => l.Next.AlmostEqual(rectCenter, 0.1f)) is Line l)
            {
                AdvanceLine(l, rectCenter);
                if (l.ExplosionsLeft == 0)
                    Lines.Remove(l);
            }
        }
    }
}

class P1Bounds(BossModule module) : BossComponent(module)
{
    public override void Update()
    {
        Arena.Center = Raid.Player()?.Position ?? Arena.Center;
    }
}

class BlazeInRed(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Weaponskill_BlazeInRed));
class TornadoInGreen(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_TornadoInGreen), new AOEShapeDonut(12, 40));
class NineIvies(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_NineIvies1), new AOEShapeCone(50, 10.Degrees()), 9);
class SculptureCast(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID._Weaponskill_SculptureCast));
class Earthquake(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(10), new AOEShapeDonut(10, 20), new AOEShapeDonut(20, 30)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_Earthquake)
            AddSequence(caster.Position, Module.CastFinishAt(spell));
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var idx = (AID)spell.Action.ID switch
        {
            AID._Weaponskill_Earthquake => 0,
            AID._Weaponskill_Earthquake2 => 1,
            AID._Weaponskill_Earthquake3 => 2,
            _ => -1
        };
        AdvanceSequence(idx, caster.Position, WorldState.FutureTime(2));
    }
}
class Freeze(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_FreezeInCyan), new AOEShapeCone(40, 22.5f.Degrees()));

public class QuestStates : StateMachineBuilder
{
    public QuestStates(BossModule module) : base(module)
    {
        bool DutyEnd() => module.WorldState.CurrentCFCID != 966;
        bool P1End() => module.Enemies(OID._Gen_FlightOfTheGriffin).Any(x => x.IsTargetable) || P2End();
        bool P2End() => module.Enemies(OID.Boss).Any(x => x.IsTargetable) || DutyEnd();

        TrivialPhase()
            .ActivateOnEnter<BurningBright>()
            .OnEnter(() =>
            {
                Module.Arena.Center = new(54, -219);
                Module.Arena.Bounds = new ArenaBoundsRect(26, 9);
            })
            .Raw.Update = P1End;
        TrivialPhase(1)
            .ActivateOnEnter<SwoopingFrenzy>()
            .ActivateOnEnter<Feathercut>()
            .ActivateOnEnter<FrigidPulse>()
            .ActivateOnEnter<FervidPulse>()
            .ActivateOnEnter<EyeOfTheFierce>()
            .OnEnter(() =>
            {
                Module.Arena.Center = new(0, -250);
                Module.Arena.Bounds = new ArenaBoundsRect(20, 40);
            })
            .Raw.Update = P2End;
        TrivialPhase(2)
            .ActivateOnEnter<BloodyCaress>()
            .ActivateOnEnter<Flood>()
            .ActivateOnEnter<BlazeInRed>()
            .ActivateOnEnter<TornadoInGreen>()
            .ActivateOnEnter<NineIvies>()
            .ActivateOnEnter<SculptureCast>()
            .ActivateOnEnter<Earthquake>()
            .ActivateOnEnter<Freeze>()
            .OnEnter(() =>
            {
                Module.Arena.Center = new(0, -340);
                Module.Arena.Bounds = new ArenaBoundsSquare(25);
            })
            .Raw.Update = DutyEnd;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 966, PrimaryActorOID = BossModuleInfo.PrimaryActorNone)]
public class Quest(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 0), new ArenaBoundsCircle(20))
{
    protected override bool CheckPull() => true;

    protected override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
        Arena.Actors(WorldState.Actors.Where(x => x.IsAlly), ArenaColor.PlayerGeneric);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
            e.Priority = 0;
    }
}
*/
