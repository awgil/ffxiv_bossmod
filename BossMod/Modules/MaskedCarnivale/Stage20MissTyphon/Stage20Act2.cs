using BossMod.Components;

// CONTRIB: made by malediktus, not checked
namespace BossMod.MaskedCarnivale.Stage20.Act2
{
    public enum OID : uint
    {
        Boss = 0x272B, //R=5.1
        Helper = 0x233C, //R=0.5
    };

    public enum AID : uint
    {
        AquaBreath = 14713, // 272B->self, 2,5s cast, range 8+R 90-degree cone
        Megavolt = 14714, // 272B->self, 3,0s cast, range 6+R circle
        ImpSong = 14712, // 272B->self, 6,0s cast, range 50+R circle
        Waterspout = 14718, // 233C->location, 2,5s cast, range 4 circle
        LightningBolt = 14717, // 233C->location, 3,0s cast, range 3 circle
    };

    class AquaBreath : SelfTargetedAOEs
    {
        public AquaBreath() : base(ActionID.MakeSpell(AID.AquaBreath), new AOEShapeCone(13.1f, 45.Degrees())) { }
    }

    class Megavolt : SelfTargetedAOEs
    {
        public Megavolt() : base(ActionID.MakeSpell(AID.Megavolt), new AOEShapeCircle(11.1f)) { }
    }

    class Waterspout : LocationTargetedAOEs
    {
        public Waterspout() : base(ActionID.MakeSpell(AID.Waterspout), 4) { }
    }

    class LightningBolt : LocationTargetedAOEs
    {
        public LightningBolt() : base(ActionID.MakeSpell(AID.LightningBolt), 3) { }
    }

    class ImpSong : CastHint
    {
        public ImpSong() : base(ActionID.MakeSpell(AID.ImpSong), "Interrupt Ultros!") { }
    }

    class Hints : BossComponent
    {
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            hints.Add("Ultros is weak to fire. Interrupt Imp Song.");
        }
    }

    class Stage20Act2States : StateMachineBuilder
    {
        public Stage20Act2States(BossModule module) : base(module)
        {
            TrivialPhase()
            .DeactivateOnEnter<Hints>()
            .ActivateOnEnter<LightningBolt>()
            .ActivateOnEnter<Waterspout>()
            .ActivateOnEnter<Megavolt>()
            .ActivateOnEnter<AquaBreath>()
            .ActivateOnEnter<LightningBolt>()
            .ActivateOnEnter<ImpSong>();
        }
    }

    [ModuleInfo(CFCID = 630, NameID = 7111)]
    public class Stage20Act2 : BossModule
    {
        public Stage20Act2(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 16))
        {
            ActivateComponent<Hints>();
        }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            foreach (var s in Enemies(OID.Boss))
                Arena.Actor(s, ArenaColor.Enemy, false);
        }
    }
}
