namespace BossMod.Shadowbringers.Dungeon.D05MtGulg.D053ForgivenWhimsy;

public enum OID : uint
{
    Boss = 0x27CC, //R=20.00
    Helper = 0x2E8, //R=0.5
    Helper2 = 0x233C,
    Brightsphere = 0x27CD, //R=1.0
    Towers = 0x1EAACF, //R=0.5
}

public enum AID : uint
{
    AutoAttack = 15624, // 27CC->player, no cast, single-target
    SacramentOfPenance = 15627, // 27CC->self, 4.0s cast, single-target
    SacramentOfPenance2 = 15628, // 233C->self, no cast, range 50 circle
    Reformation = 15620, // 27CC->self, no cast, single-target, boss changes pattern
    ExegesisA = 16989, // 27CC->self, 5.0s cast, single-target
    ExegesisB = 16987, // 27CC->self, 5.0s cast, single-target
    ExegesisC = 15622, // 27CC->self, 5.0s cast, single-target
    ExegesisD = 16988, // 27CC->self, 5.0s cast, single-target
    Exegesis = 15623, // 233C->self, no cast, range 10 width 10 rect
    Catechism = 15625, // 27CC->self, 4.0s cast, single-target
    Catechism2 = 15626, // 233C->player, no cast, single-target
    JudgmentDay = 15631, // 27CC->self, 3.0s cast, single-target, tower circle 5
    Judged = 15633, // 233C->self, no cast, range 5 circle, tower success
    FoundWanting = 15632, // 233C->self, no cast, range 40 circle, tower fail
    RiteOfTheSacrament = 15629, // 27CC->self, no cast, single-target
    PerfectContrition = 15630, // 27CD->self, 6.0s cast, range 5-15 donut
}

class PerfectContrition(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(5, 15);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Brightsphere)
            _aoes.Add(new AOEInstance(donut, actor.Position, default, Module.WorldState.FutureTime(10.6f)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.PerfectContrition)
            _aoes.Clear();
    }
}

class Catechism(BossModule module) : Components.SingleTargetCastDelay(module, ActionID.MakeSpell(AID.Catechism), ActionID.MakeSpell(AID.Catechism2), 0.5f);
class SacramentOfPenance(BossModule module) : Components.RaidwideCastDelay(module, ActionID.MakeSpell(AID.SacramentOfPenance), ActionID.MakeSpell(AID.SacramentOfPenance2), 0.5f);

class JudgmentDay(BossModule module) : Components.GenericTowers(module)
{
    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Towers)
            Towers.Add(new(actor.Position, 5, 1, 1));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Judged or AID.FoundWanting && Towers.Count > 0)
            Towers.RemoveAt(0);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Towers.Count > 0)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Towers[0].Position, 5));
        if (Towers.Count > 1)
            hints.PlannedActions.Add((ActionID.MakeSpell(WAR.AID.Sprint), actor, 1, false));
    }
}

class Exegesis(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect rect = new(5, 5, 5);
    private static readonly AOEShapeCross cross = new(15, 5);
    private static readonly WPos[] diagonalPositions = [new(-240, -50), new(-250, -40), new(-230, -40), new(-250, -60), new(-230, -60)];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var _activation = spell.NPCFinishAt.AddSeconds(0.4f);
        switch ((AID)spell.Action.ID)
        {
            case AID.ExegesisA: //diagonal
                foreach (var p in diagonalPositions)
                    _aoes.Add(new(rect, p, default, _activation));
                break;
            case AID.ExegesisB: //east+west
                _aoes.Add(new(rect, new(-250, -50), default, _activation));
                _aoes.Add(new(rect, new(-230, -50), default, _activation));
                break;
            case AID.ExegesisC: //north+south
                _aoes.Add(new(rect, new(-240, -60), default, _activation));
                _aoes.Add(new(rect, new(-240, -40), default, _activation));
                break;
            case AID.ExegesisD: //cross
                _aoes.Add(new(cross, new(-240, -50), default, _activation));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Exegesis)
            _aoes.Clear();
    }
}

class MeleeRange(BossModule module) : BossComponent(module) // force melee range for melee rotation solver users
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!Service.Config.Get<AutorotationConfig>().Enabled)
            if (!Module.FindComponent<Exegesis>()!.ActiveAOEs(slot, actor).Any() && !Module.FindComponent<PerfectContrition>()!.ActiveAOEs(slot, actor).Any() && Module.FindComponent<JudgmentDay>()!.Towers.Count == 0)
                if (actor.Role is Role.Melee or Role.Tank)
                    hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Module.PrimaryActor.Position, Module.PrimaryActor.HitboxRadius + 3));
    }
}

class D053ForgivenWhimsyStates : StateMachineBuilder
{
    public D053ForgivenWhimsyStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MeleeRange>()
            .ActivateOnEnter<Catechism>()
            .ActivateOnEnter<SacramentOfPenance>()
            .ActivateOnEnter<PerfectContrition>()
            .ActivateOnEnter<JudgmentDay>()
            .ActivateOnEnter<Exegesis>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 659, NameID = 8261)]
public class D053ForgivenWhimsy(WorldState ws, Actor primary) : BossModule(ws, primary, new(-240, -50), new ArenaBoundsSquare(15)); //actually walkable arena size is 14.5, but then the tiny safespots in the corner are no longer visible
