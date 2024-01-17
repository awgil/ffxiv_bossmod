namespace BossMod.Endwalker.HuntA.ArchEta
{
    public enum OID : uint
    {
        Boss = 0x35C0, // R7.200, x1
    };

    public enum AID : uint
    {
        AutoAttack = 870, // Boss->player, no cast, single-target
        EnergyWave = 27269, // Boss->self, 3.0s cast, range 40 width 14 rect
        TailSwipe = 27270, // Boss->self, 4.0s cast, range 25 90-degree cone
        HeavyStomp = 27271, // Boss->self, 3.0s cast, range 17 circle
        SonicHowl = 27272, // Boss->self, 5.0s cast, range 30 circle
        SteelFang = 27273, // Boss->player, 5.0s cast, single-target
        FangedLunge = 27274, // Boss->player, no cast, single-target
    };

    class EnergyWave : Components.SelfTargetedAOEs
    {
        public EnergyWave() : base(ActionID.MakeSpell(AID.EnergyWave), new AOEShapeRect(40, 7)) { }
    }

    class TailSwipe : Components.SelfTargetedAOEs
    {
        public TailSwipe() : base(ActionID.MakeSpell(AID.TailSwipe), new AOEShapeCone(25, 45.Degrees())) { }
    }

    class HeavyStomp : Components.SelfTargetedAOEs
    {
        public HeavyStomp() : base(ActionID.MakeSpell(AID.HeavyStomp), new AOEShapeCircle(17)) { }
    }

    class SonicHowl : Components.RaidwideCast
    {
        public SonicHowl() : base(ActionID.MakeSpell(AID.SonicHowl)) { }
    }

    class SteelFang : Components.SingleTargetCast
    {
        public SteelFang() : base(ActionID.MakeSpell(AID.SteelFang)) { }
    }

    class ArchEtaStates : StateMachineBuilder
    {
        public ArchEtaStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<EnergyWave>()
                .ActivateOnEnter<TailSwipe>()
                .ActivateOnEnter<HeavyStomp>()
                .ActivateOnEnter<SonicHowl>()
                .ActivateOnEnter<SteelFang>();
        }
    }

    [ModuleInfo(NotoriousMonsterID = 224)]
    public class ArchEta : SimpleBossModule
    {
        public ArchEta(WorldState ws, Actor primary) : base(ws, primary) { }
    }
}
