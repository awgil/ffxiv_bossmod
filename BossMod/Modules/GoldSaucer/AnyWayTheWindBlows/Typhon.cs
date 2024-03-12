namespace BossMod.GoldSaucer.AnyWayTheWindBlows.Typhon //Work in progress ugh
{
    public enum OID : uint
    {
        Boss = 0xE4F, //R=5.0
        Helper = 0xE50, //R=0.50
    };

    public enum AID : uint
    {
    TyphonKB1 = 3363, // E4F->self, 3.0s cast, range 3+R circle //Maybe smaller 4 cardianl aoes
    TyphonKB2 = 3364, // E4F->self, 3.0s cast, range 11+R circle //Edge arena donut?
    TyphonKB3 = 3365, // E4F->self, 3.0s cast, range 11+R 120-degree cone //1/3rd arena cleave
    TyphonKB4 = 3366, // E4F->self, 3.0s cast, range 26+R width 9 rect //Forward casting rectangular AOE
    TyphonKB5 = 3367, // E4F->self, 3.0s cast, single-target //Likely on boss knockback
    TyphonKB6 = 3368, // E50->self, 3.5s cast, range 4+R circle //Maybe 4 cardinal aoes
    };

    public enum SID : uint
    {
        RedLight = 663, // none->player, extra=0x0
    };

    class Snort1 : Components.SelfTargetedAOEs
    {
        public Snort1() : base(ActionID.MakeSpell(AID.TyphonKB1), new AOEShapeCircle(3f)) { }
    }
    class Snort2 : Components.SelfTargetedAOEs
    {
        public Snort2() : base(ActionID.MakeSpell(AID.TyphonKB2), new AOEShapeCircle(11f)) { }
    }
    class Snort3 : Components.SelfTargetedAOEs
    {
        public Snort3() : base(ActionID.MakeSpell(AID.TyphonKB3), new AOEShapeCone(11f, 60.Degrees())) { }
    }
    class Snort4 : Components.SelfTargetedAOEs
    {
        public Snort4() : base(ActionID.MakeSpell(AID.TyphonKB4), new AOEShapeRect(26f, 9)) { }
    }
    class Snort5 : Components.LocationTargetedAOEs
    {
        public Snort5() : base(ActionID.MakeSpell(AID.TyphonKB5), 5) { }
    }
    class Snort6 : Components.SelfTargetedAOEs
    {
        public Snort6() : base(ActionID.MakeSpell(AID.TyphonKB6), new AOEShapeCircle(4f)) { }
    }

    class AnyWayTheWindBlowsStates : StateMachineBuilder
    {
        public AnyWayTheWindBlowsStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<Snort1>()
                .ActivateOnEnter<Snort2>()
                .ActivateOnEnter<Snort3>()
                .ActivateOnEnter<Snort4>()
                .ActivateOnEnter<Snort5>()
                .ActivateOnEnter<Snort6>();
        }
    }

    [ModuleInfo(NameID = 3046)] //no clue where to get CFC at the moment for GATES
    public class AnyWayTheWindBlows : BossModule
    {
        public AnyWayTheWindBlows(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(70.5f, -36), 20f)) { }

        protected override bool CheckPull() { return PrimaryActor != null; }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy, true);
        }
    }
}
