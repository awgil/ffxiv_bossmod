namespace BossMod.Endwalker.HuntA.Hulder
{
    public enum OID : uint
    {
        Boss = 0x35DD, // R5.400, x1
    };

    public enum AID : uint
    {
        AutoAttack = 872, // Boss->player, no cast, single-target
        LayOfMislaidMemory = 27073, // Boss->self, 5.0s cast, range 30 120-degree cone, dmg + vulnerability up + makes player dance for 15s
        TempestuousWrath = 27075, // Boss->location, 3.0s cast, width 8 rect charge
        RottingElegy = 27076, // Boss->self, 5.0s cast, range 5-50 donut
        OdeToLostLove = 27077, // Boss->self, 5.0s cast, range 60 circle
        StormOfColor = 27078, // Boss->player, 4.0s cast, single-target
    }

    class LayOfMislaidMemory : Components.SelfTargetedAOEs
    {
        public LayOfMislaidMemory() : base(ActionID.MakeSpell(AID.LayOfMislaidMemory), new AOEShapeCone(30, 60.Degrees())) { }
    }

    class TempestuousWrath : Components.ChargeAOEs
    {
        public TempestuousWrath() : base(ActionID.MakeSpell(AID.TempestuousWrath), 4) { }
    }

    class RottingElegy : Components.SelfTargetedAOEs
    {
        public RottingElegy() : base(ActionID.MakeSpell(AID.RottingElegy), new AOEShapeDonut(5, 50)) { }
    }

    class OdeToLostLove : Components.RaidwideCast
    {
        public OdeToLostLove() : base(ActionID.MakeSpell(AID.OdeToLostLove)) { }
    }

    class StormOfColor : Components.SingleTargetCast
    {
        public StormOfColor() : base(ActionID.MakeSpell(AID.StormOfColor)) { }
    }

    class HulderStates : StateMachineBuilder
    {
        public HulderStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<LayOfMislaidMemory>()
                .ActivateOnEnter<TempestuousWrath>()
                .ActivateOnEnter<RottingElegy>()
                .ActivateOnEnter<OdeToLostLove>()
                .ActivateOnEnter<StormOfColor>();
        }
    }

    [ModuleInfo(NotoriousMonsterID = 174)]
    public class Hulder : SimpleBossModule
    {
        public Hulder(WorldState ws, Actor primary) : base(ws, primary) { }
    }
}
