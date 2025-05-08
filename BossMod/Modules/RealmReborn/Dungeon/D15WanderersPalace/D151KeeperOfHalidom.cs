namespace BossMod.RealmReborn.Dungeon.D15WanderersPalace.D151KeeperOfHalidom;

public enum OID : uint
{
    Boss = 0x41C,
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    Beatdown = 575, // Boss->player, no cast, single-target, mini tankbuster
    MoldySneeze = 1090, // Boss->self, no cast, range 6+R 90-degree frontal cone
    Inhale = 950, // Boss->self, 2.5s cast, range 20+R 90-degree cone
    GoobbuesGrief = 942, // Boss->self, 0.5s cast, range 6+R circle
    MoldyPhlegm = 941, // Boss->location, 2.5s cast, range 6 circle
}

class MoldySneeze(BossModule module) : Components.Cleave(module, AID.MoldySneeze, new AOEShapeCone(8.85f, 45.Degrees()));

class InhaleGoobbuesGrief(BossModule module) : Components.GenericAOEs(module)
{
    private bool _showInhale;
    private bool _showGrief;
    private DateTime _griefActivation;

    private static readonly AOEShapeCone _shapeInhale = new(22.85f, 45.Degrees());
    private static readonly AOEShapeCircle _shapeGrief = new(8.85f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_showInhale)
            yield return new(_shapeInhale, Module.PrimaryActor.Position, Module.PrimaryActor.CastInfo!.Rotation, Module.CastFinishAt(Module.PrimaryActor.CastInfo));
        if (_showGrief)
            yield return new(_shapeGrief, Module.PrimaryActor.Position, new(), _griefActivation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Inhale:
                _showInhale = _showGrief = true;
                _griefActivation = Module.CastFinishAt(spell, 1.1f);
                break;
            case AID.GoobbuesGrief:
                _griefActivation = Module.CastFinishAt(spell);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Inhale:
                _showInhale = false;
                break;
            case AID.GoobbuesGrief:
                _showGrief = false;
                break;
        }
    }
}

class MoldyPhlegm(BossModule module) : Components.StandardAOEs(module, AID.MoldyPhlegm, 6);

class D151KeeperOfHalidomStates : StateMachineBuilder
{
    public D151KeeperOfHalidomStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MoldySneeze>()
            .ActivateOnEnter<InhaleGoobbuesGrief>()
            .ActivateOnEnter<MoldyPhlegm>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 10, NameID = 1548)]
public class D151KeeperOfHalidom(WorldState ws, Actor primary) : BossModule(ws, primary, new(125, 108), new ArenaBoundsSquare(20));
