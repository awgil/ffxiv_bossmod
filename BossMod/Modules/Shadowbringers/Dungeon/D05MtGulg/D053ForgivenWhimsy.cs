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

class Catechism(BossModule module) : Components.SingleTargetCastDelay(module, AID.Catechism, AID.Catechism2, 0.5f);
class SacramentOfPenance(BossModule module) : Components.RaidwideCastDelay(module, AID.SacramentOfPenance, AID.SacramentOfPenance2, 0.5f);
class PerfectContrition(BossModule module) : Components.StandardAOEs(module, AID.PerfectContrition, new AOEShapeDonut(5, 15));

class JudgmentDay(BossModule module) : Components.GenericTowers(module)
{
    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Towers)
            Towers.Add(new(actor.Position, 5, activation: WorldState.FutureTime(7.7f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Judged or AID.FoundWanting)
            Towers.RemoveAll(t => t.Position.AlmostEqual(caster.Position, 1));
    }
}

class Exegesis(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime _activation;
    public enum Patterns { None, Diagonal, Cross, EastWest, NorthSouth }
    public Patterns Pattern { get; private set; }
    private static readonly AOEShapeRect rect = new(5, 5, 5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Pattern == Patterns.Diagonal)
        {
            yield return new(rect, new(-240, -50), default, _activation);
            yield return new(rect, new(-250, -40), default, _activation);
            yield return new(rect, new(-230, -40), default, _activation);
            yield return new(rect, new(-250, -60), default, _activation);
            yield return new(rect, new(-230, -60), default, _activation);
        }
        if (Pattern == Patterns.EastWest)
        {
            yield return new(rect, new(-250, -50), default, _activation);
            yield return new(rect, new(-230, -50), default, _activation);
        }
        if (Pattern == Patterns.NorthSouth)
        {
            yield return new(rect, new(-240, -60), default, _activation);
            yield return new(rect, new(-240, -40), default, _activation);
        }
        if (Pattern == Patterns.Cross)
        {
            yield return new(rect, new(-230, -50), default, _activation);
            yield return new(rect, new(-240, -60), default, _activation);
            yield return new(rect, new(-240, -40), default, _activation);
            yield return new(rect, new(-250, -50), default, _activation);
            yield return new(rect, new(-240, -50), default, _activation);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ExegesisA:
                Pattern = Patterns.Diagonal;
                _activation = Module.CastFinishAt(spell);
                break;
            case AID.ExegesisB:
                Pattern = Patterns.EastWest;
                _activation = Module.CastFinishAt(spell);
                break;
            case AID.ExegesisC:
                Pattern = Patterns.NorthSouth;
                _activation = Module.CastFinishAt(spell);
                break;
            case AID.ExegesisD:
                Pattern = Patterns.Cross;
                _activation = Module.CastFinishAt(spell);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Exegesis)
            Pattern = Patterns.None;
    }
}

class D053ForgivenWhimsyStates : StateMachineBuilder
{
    public D053ForgivenWhimsyStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Catechism>()
            .ActivateOnEnter<SacramentOfPenance>()
            .ActivateOnEnter<PerfectContrition>()
            .ActivateOnEnter<JudgmentDay>()
            .ActivateOnEnter<Exegesis>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus, xan", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 659, NameID = 8261)]
public class D053ForgivenWhimsy(WorldState ws, Actor primary) : BossModule(ws, primary, new(-240, -50), new ArenaBoundsSquare(15));
