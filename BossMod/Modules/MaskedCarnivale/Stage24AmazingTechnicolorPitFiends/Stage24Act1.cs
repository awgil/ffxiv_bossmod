using System.Linq;

// CONTRIB: made by malediktus, not checked
namespace BossMod.MaskedCarnivale.Stage24.Act1
{
    public enum OID : uint
    {
        Boss = 0x2735, //R=1.0
        ArenaViking = 0x2734, //R=1.0
    };

    public enum AID : uint
    {
        AutoAttack = 6497, // ArenaViking->player, no cast, single-target
        Fire = 14266, // Boss->player, 1,0s cast, single-target
        Starstorm = 15317, // Boss->location, 3,0s cast, range 5 circle
        RagingAxe = 15316, // ArenaViking->self, 3,0s cast, range 4+R 90-degree cone
        LightningSpark = 15318, // Boss->player, 6,0s cast, single-target
    };

    class Starstorm : Components.LocationTargetedAOEs
    {
        public Starstorm() : base(ActionID.MakeSpell(AID.Starstorm), 5) { }
    }

    class RagingAxe : Components.SelfTargetedAOEs
    {
        public RagingAxe() : base(ActionID.MakeSpell(AID.RagingAxe), new AOEShapeCone(5, 45.Degrees())) { }
    }

    class LightningSpark : Components.CastHint
    {
        public LightningSpark() : base(ActionID.MakeSpell(AID.LightningSpark), "Interrupt") { }
    }

    class Hints2 : BossComponent
    {
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (!module.PrimaryActor.IsDead)
                hints.Add($"{module.PrimaryActor.Name} is immune to magical damage!");
            if (!module.Enemies(OID.ArenaViking).All(e => e.IsDead))
                hints.Add($"{module.Enemies(OID.ArenaViking).FirstOrDefault()!.Name} is immune to physical damage!");
        }
    }

    class Hints : BossComponent
    {
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            hints.Add($"The {module.PrimaryActor.Name} is immune to magic, the {module.Enemies(OID.ArenaViking).FirstOrDefault()!.Name} is immune to\nphysical attacks. For the 2nd act Diamondback is highly recommended.\nFor the 3rd act a ranged physical spell such as Fire Angon\nis highly recommended.");
        }
    }

    class Stage24Act1States : StateMachineBuilder
    {
        public Stage24Act1States(BossModule module) : base(module)
        {
            TrivialPhase()
                .DeactivateOnEnter<Hints>()
                .ActivateOnEnter<Starstorm>()
                .ActivateOnEnter<RagingAxe>()
                .ActivateOnEnter<LightningSpark>()
                .ActivateOnEnter<Hints2>()
                .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.ArenaViking).All(e => e.IsDead);
        }
    }

    [ModuleInfo(CFCID = 634, NameID = 8127)]
    public class Stage24Act1 : BossModule
    {
        public Stage24Act1(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 25))
        {
            ActivateComponent<Hints>();
        }

        protected override bool CheckPull() { return PrimaryActor.IsTargetable && PrimaryActor.InCombat || Enemies(OID.ArenaViking).Any(e => e.InCombat); }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
            foreach (var s in Enemies(OID.ArenaViking))
                Arena.Actor(s, ArenaColor.Enemy);
        }

        public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.CalculateAIHints(slot, actor, assignment, hints);
            foreach (var e in hints.PotentialTargets)
            {
                e.Priority = (OID)e.Actor.OID switch
                {
                    OID.Boss or OID.ArenaViking => 0, //TODO: ideally Viking should only be attacked with magical abilities and Magus should only be attacked with physical abilities
                    _ => 0
                };
            }
        }
    }
}
