namespace BossMod.GoldSaucer.SliceIsRight.Yojimbo //Hella work in progress
{
    public enum OID : uint
    {
        Boss = 0x25AB, //R=1.80 Yojimbo
        Dog = 0x25AC, //R=2.50 Daigoro
        Helper_Pillar = 0x25AD, //R=0.50 Pillars, based on 18327
        Unknown2 = 0x25C8, //R=0.50 Gold Cup?
    };

    public enum AID : uint
    {
        _Ability_1 = 19070, // 25AB->self, no cast, single-target
        _Ability_2 = 18331, // 25AB->self, no cast, single-tat
        _Ability_3 = 18329, // 25AB->self, no cast, single-target
        _Ability_4 = 18332, // 25AB->self, no cast, single-target
        _Ability_5 = 18338, // 2BC8->self, no cast, single-target
        _Weaponskill_2 = 18328, // 25AB->location, no cast, single-target
        _Weaponskill_1 = 18326, // 25AB->self, 3.0s cast, single-target
        _Weaponskill_3 = 18339, // 25AB->self, 3.0s cast, single-target
        _Weaponskill_4 = 18340, // 25AB->self, no cast, single-target
        _Weaponskill_5 = 19026, // 25AB->self, no cast, single-target
        PillarSplit = 18333, // 25AD->self, 0.7s cast, range 28 width 5 rect // Pillar split?
        PillarCircleFall = 18334, // 25AD->self, 0.7s cast, range 11 circle //Pillar scatter
        PillarSpawn = 18327, // 25AD->self, no cast, range 3 circle // Pillar appearing?
        FirstGilJump = 18335, // 25AC->location, 2.5s cast, width 7 rect charge //Daigoro's first gil jump
        NextGilJump = 18336, // 25AC->location, 1.5s cast, width 7 rect charge //Daigoro's consecutive gil jumps
        BadCup = 18337, // 25AC->self, 1.0s cast, range 15+R 120-degree cone //Daigoro under the cup!
    };

    public enum SID : uint
    {
        RedLight = 2159, // none->player, extra=0x0
        OutoftheAction = 1284, // none->player, extra=0x0
    };

    class PillarSplit : Components.SelfTargetedAOEs
    {
        public PillarSplit() : base(ActionID.MakeSpell(AID.PillarSplit), new AOEShapeRect(28f, 2.5f)) { }
    }

    class PillarCircle : Components.SelfTargetedAOEs
    {
        public PillarCircle() : base(ActionID.MakeSpell(AID.PillarCircleFall), new AOEShapeCircle(11)) { }
    }

    class PillarSpawning : Components.SelfTargetedAOEs
    {
        public PillarSpawning() : base(ActionID.MakeSpell(AID.PillarSpawn), new AOEShapeCircle(3)) { }
    }

    class DaigoroFirstGilJump : Components.ChargeAOEs
    {
        public DaigoroFirstGilJump() : base(ActionID.MakeSpell(AID.FirstGilJump), 3.5f) { }
    }

    class DaigoroNextGilJump : Components.ChargeAOEs
    {
        public DaigoroNextGilJump() : base(ActionID.MakeSpell(AID.NextGilJump), 3.5f) { }
    }

    class DaigoroBadCup : Components.SelfTargetedAOEs
    {
        public DaigoroBadCup() : base(ActionID.MakeSpell(AID.BadCup), new AOEShapeCone(15, 60.Degrees())) { }
    }

    class SliceIsRightStates : StateMachineBuilder
    {
        public SliceIsRightStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<PillarSplit>()
                .ActivateOnEnter<PillarCircle>()
                .ActivateOnEnter<PillarSpawning>()
                .ActivateOnEnter<DaigoroFirstGilJump>()
                .ActivateOnEnter<DaigoroNextGilJump>()
                .ActivateOnEnter<DaigoroBadCup>();
        }
    }

    [ModuleInfo(NameID = 9066)] //no clue how to get CFC or any other identifer for this
    public class SliceIsRight : BossModule
    {
        public SliceIsRight(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(70.5f, -36), 20f)) { }

        protected override bool CheckPull() { return PrimaryActor != null; }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy, true);
            foreach (var s in Enemies(OID.Dog))
                Arena.Actor(s, ArenaColor.Object, true);
        }
    }
}
