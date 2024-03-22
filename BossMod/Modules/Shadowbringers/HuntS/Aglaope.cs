namespace BossMod.Shadowbringers.HuntS.Aglaope
{
    public enum OID : uint
    {
        Boss = 0x281E, // R=2.4
    };

    public enum AID : uint
    {
        AutoAttack = 870, // Boss->player, no cast, single-target
        FourfoldSuffering = 16819, // Boss->self, 5,0s cast, range 5-50 donut
        SeductiveSonata = 16824, // Boss->self, 3,0s cast, range 40 circle, applies Seduced for 6s (forced march towards boss at 1.7y/s)
        DeathlyVerse = 17074, // Boss->self, 5,0s cast, range 6 circle (right after Seductive Sonata, instant kill), 6*1.7 = 10.2 + 6 = 16.2y minimum distance to survive
        Tornado = 18040, // Boss->location, 3,0s cast, range 6 circle
        AncientAero = 16823, // Boss->self, 3,0s cast, range 40+R width 6 rect
        SongOfTorment = 16825, // Boss->self, 5,0s cast, range 50 circle, interruptible raidwide with bleed
        AncientAeroIII = 18056, // Boss->self, 3,0s cast, range 30 circle, knockback 10, away from source
    };

    public enum SID : uint
    {
        Seduced = 991, // Boss->player, extra=0x11
        Bleeding = 642, // Boss->player, extra=0x0
    };

    class SongOfTorment : Components.CastInterruptHint
    {
        public SongOfTorment() : base(ActionID.MakeSpell(AID.SongOfTorment), hint: "(Raidwide + Bleed)") { }
    }

    class SeductiveSonata : Components.SelfTargetedAOEs
    {
        public SeductiveSonata() : base(ActionID.MakeSpell(AID.SeductiveSonata), new AOEShapeCircle(16.2f)) { }
    }

    class DeathlyVerse : Components.SelfTargetedAOEs
    {
        public DeathlyVerse() : base(ActionID.MakeSpell(AID.DeathlyVerse), new AOEShapeCircle(6)) { }
    }

    class Tornado : Components.LocationTargetedAOEs
    {
        public Tornado() : base(ActionID.MakeSpell(AID.Tornado), 6) { }
    }

    class FourfoldSuffering : Components.SelfTargetedAOEs
    {
        public FourfoldSuffering() : base(ActionID.MakeSpell(AID.FourfoldSuffering), new AOEShapeDonut(5, 50)) { }
    }

    class AncientAero : Components.SelfTargetedAOEs
    {
        public AncientAero() : base(ActionID.MakeSpell(AID.AncientAero), new AOEShapeRect(42.4f, 3)) { }
    }

    class AncientAeroIII : Components.RaidwideCast
    {
        public AncientAeroIII() : base(ActionID.MakeSpell(AID.AncientAeroIII)) { }
    }

    class AncientAeroIIIKB : Components.KnockbackFromCastTarget
    {
        public AncientAeroIIIKB() : base(ActionID.MakeSpell(AID.AncientAeroIII), 10, shape: new AOEShapeCircle(30)) { }
    }

    class AglaopeStates : StateMachineBuilder
    {
        public AglaopeStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<SongOfTorment>()
                .ActivateOnEnter<SeductiveSonata>()
                .ActivateOnEnter<DeathlyVerse>()
                .ActivateOnEnter<Tornado>()
                .ActivateOnEnter<FourfoldSuffering>()
                .ActivateOnEnter<AncientAero>()
                .ActivateOnEnter<AncientAeroIII>()
                .ActivateOnEnter<AncientAeroIIIKB>();
        }
    }

    [ModuleInfo(NotoriousMonsterID = 131)]
    public class Aglaope : SimpleBossModule
    {
        public Aglaope(WorldState ws, Actor primary) : base(ws, primary) { }
    }
}
