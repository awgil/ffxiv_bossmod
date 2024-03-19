using System.Linq;

// CONTRIB: made by malediktus, not checked
namespace BossMod.MaskedCarnivale.Stage17.Act1
{
    public enum OID : uint
    {
        Boss = 0x2720, //R=2.0
        RightClaw = 0x271F, //R=2.0
    };

    public enum AID : uint
    {
        AutoAttack = 6499, // 2720/271F->player, no cast, single-target
        TheHand = 14760, // 271F/2720->self, 3,0s cast, range 6+R 120-degree cone, knockback away from source, dist 10
        Shred = 14759, // 2720/271F->self, 2,5s cast, range 4+R width 4 rect, stuns player
    };

    class TheHand : Components.SelfTargetedAOEs
    {
        public TheHand() : base(ActionID.MakeSpell(AID.TheHand), new AOEShapeCone(8, 60.Degrees())) { }
    }

    class Shred : Components.SelfTargetedAOEs
    {
        public Shred() : base(ActionID.MakeSpell(AID.Shred), new AOEShapeRect(6, 2)) { }
    }

    class TheHandKB : Components.KnockbackFromCastTarget //actual knockback happens a whole 0,9s after snapshot
    {
        public TheHandKB() : base(ActionID.MakeSpell(AID.TheHand), 10, shape: new AOEShapeCone(8, 60.Degrees())) { }
    }

    class Hints2 : BossComponent
    {
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (!module.PrimaryActor.IsDead)
                hints.Add($"{module.PrimaryActor.Name} counters magical damage!");
            if (!module.Enemies(OID.RightClaw).All(e => e.IsDead))
                hints.Add($"{module.Enemies(OID.RightClaw).FirstOrDefault()!.Name} counters physical damage!");
        }
    }

    class Hints : BossComponent
    {
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            hints.Add($"The {module.PrimaryActor.Name} counters magical attacks, the {module.Enemies(OID.RightClaw).FirstOrDefault()!.Name} counters physical\nattacks. If you have healing spells you can just tank the counter damage\nand kill them however you like anyway. All opponents in this stage are\nweak to lightning.\nThe Ram's Voice and Ultravibration combo can be used in Act 2.");
        }
    }

    class Stage17Act1States : StateMachineBuilder
    {
        public Stage17Act1States(BossModule module) : base(module)
        {
            TrivialPhase()
                .DeactivateOnEnter<Hints>()
                .ActivateOnEnter<Shred>()
                .ActivateOnEnter<TheHand>()
                .ActivateOnEnter<TheHandKB>()
                .ActivateOnEnter<Hints2>()
                .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.RightClaw).All(e => e.IsDead);
        }
    }

    [ModuleInfo(CFCID = 627, NameID = 8115)]
    public class Stage17Act1 : BossModule
    {
        public Stage17Act1(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 16))
        {
            ActivateComponent<Hints>();
        }

        protected override bool CheckPull() { return PrimaryActor.IsTargetable && PrimaryActor.InCombat || Enemies(OID.RightClaw).Any(e => e.InCombat); }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
            foreach (var s in Enemies(OID.RightClaw))
                Arena.Actor(s, ArenaColor.Enemy);
        }

        public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.CalculateAIHints(slot, actor, assignment, hints);
            foreach (var e in hints.PotentialTargets)
            {
                e.Priority = (OID)e.Actor.OID switch
                {
                    OID.Boss or OID.RightClaw => 0, //TODO: ideally left claw should only be attacked with magical abilities and right claw should only be attacked with physical abilities
                    _ => 0
                };
            }
        }
    }
}
