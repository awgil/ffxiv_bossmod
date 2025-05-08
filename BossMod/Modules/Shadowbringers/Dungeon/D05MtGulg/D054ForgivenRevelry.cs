namespace BossMod.Shadowbringers.Dungeon.D05MtGulg.D054ForgivenRevelry;

public enum OID : uint
{
    Boss = 0x28F3, //R=7.5
    Helper = 0x2E8, //R=0.5
    Helper2 = 0x233C,
    Brightsphere = 0x2947, //R=1.0
}

public enum AID : uint
{
    AutoAttack = 16246, // Boss->player, no cast, single-target
    LeftPalm = 16249, // Boss->self, no cast, single-target
    LeftPalm2 = 16250, // 233C->self, 4.5s cast, range 30 width 15 rect
    LightShot = 16251, // Brightsphere->self, 4.0s cast, range 40 width 4 rect
    RightPalm = 16247, // Boss->self, no cast, single-target
    RightPalm2 = 16248, // 233C->self, 4.5s cast, range 30 width 15 rect
}

class PalmAttacks(BossModule module) : Components.GenericAOEs(module) //Palm Attacks have a wrong origin, so i made a custom solution
{
    private AOEInstance? _aoe;

    private static readonly AOEShapeRect rect = new(15, 15);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.LeftPalm2:
                _aoe = new(rect, new(Module.PrimaryActor.Position.X, Module.Center.Z), -90.Degrees(), Module.CastFinishAt(spell));
                break;
            case AID.RightPalm2:
                _aoe = new(rect, new(Module.PrimaryActor.Position.X, Module.Center.Z), 90.Degrees(), Module.CastFinishAt(spell));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.LeftPalm2 or AID.RightPalm2)
            _aoe = null;
    }
}

class LightShot(BossModule module) : Components.StandardAOEs(module, AID.LightShot, new AOEShapeRect(40, 2));

class D054ForgivenRevelryStates : StateMachineBuilder
{
    public D054ForgivenRevelryStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PalmAttacks>()
            .ActivateOnEnter<LightShot>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 659, NameID = 8270)]
public class D054ForgivenRevelry(WorldState ws, Actor primary) : BossModule(ws, primary, new(-240, 176), new ArenaBoundsSquare(15));
