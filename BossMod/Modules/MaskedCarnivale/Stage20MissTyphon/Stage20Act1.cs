using BossMod.Components;

// CONTRIB: made by malediktus, not checked
namespace BossMod.MaskedCarnivale.Stage20.Act1
{
    public enum OID : uint
    {
        Boss = 0x272A, //R=4.5
    };

    public enum AID : uint
    {
        Fungah = 14705, // 272A->self, no cast, range 8+R ?-degree cone, knockback 20 away from source
        Fireball = 14706, // 272A->location, 3,5s cast, range 8 circle
        Snort = 14704, // 272A->self, 7,0s cast, range 50+R circle, stun, knockback 30 away from source
        Fireball2 = 14707, // 272A->player, no cast, range 8 circle, 3 casts after snort
    };

    class Fireball : LocationTargetedAOEs
    {
        public Fireball() : base(ActionID.MakeSpell(AID.Fireball), 8) { }
    }

    class Snort : CastHint
    {
        public Snort() : base(ActionID.MakeSpell(AID.Snort), "Use Diamondback!") { }
    }

    class SnortKB : KnockbackFromCastTarget
    {
        public SnortKB() : base(ActionID.MakeSpell(AID.Snort), 30, kind: Kind.AwayFromOrigin) { } //knockback actually delayed by 0.7s
        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints) { }
    }

    class Hints : BossComponent
    {
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            hints.Add("Diamondback and Flying Sardine are essential for this stage. The Final\nSting combo (Off-guard->Bristle->Moonflute->Final Sting) can make act 3\nincluding the achievement much easier. Ultros in act 2 and 3 is weak to\nfire.");
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            hints.Add("Requirement for achievement: Don't kill any tentacles in act 3", false);
        }
    }

    class Stage20Act1States : StateMachineBuilder
    {
        public Stage20Act1States(BossModule module) : base(module)
        {
            TrivialPhase()
                .DeactivateOnEnter<Hints>()
                .ActivateOnEnter<Snort>()
                .ActivateOnEnter<SnortKB>()
                .ActivateOnEnter<Fireball>();
        }
    }

    [ModuleInfo(CFCID = 630, NameID = 3046)]
    public class Stage20Act1 : BossModule
    {
        public Stage20Act1(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 25))
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
