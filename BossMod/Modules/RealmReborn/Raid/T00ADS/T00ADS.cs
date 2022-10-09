namespace BossMod.RealmReborn.Raid.T00ADS
{
    public enum OID : uint
    {
        Boss = 0x887, // x1
        Helper = 0x8EC, // x1
        PatrolNode = 0x888, // spawn during fight
        AttackNode = 0x889, // spawn during fight
        DefenseNode = 0x88A, // spawn during fight
        GravityField = 0x1E8728, // EventObj type, spawn during fight
    };

    public enum AID : uint
    {
        AutoAttack = 1215, // Boss/PatrolNode/AttackNode/DefenseNode->player, no cast, range 6+R ?-degree cone cleave
        HighVoltage = 1447, // Boss->self, 2.5s cast (interruptible), raidwide
        RepellingCannons = 1448, // Boss/PatrolNode/AttackNode/DefenseNode->self, 2.5s cast, range 6+R circle aoe
        PiercingLaser = 1450, // Boss->self, 2.2s cast, range 30+R width 6 rect aoe
        DirtyCannons = 1378, // PatrolNode->self, 1.0s cast, range 4+R circle aoe
        GravityField = 1220, // AttackNode->location, 1.0s cast, range 6 circle, spawns voidzone
        ChainLightning = 1225, // DefenseNode->self, 1.5s cast, single-target, visual
        ChainLightningAOE = 1449, // Helper->player, no cast, single-target, ???
        NodeRetrieval = 1228, // Boss->PatrolNode/AttackNode/DefenseNode, no cast, single-target, happens if add is not killed in ~27s and gives boss damage up
        Object199 = 1229, // Boss->self, no cast, enrage
    };

    class HighVoltage : Components.CastHint
    {
        public HighVoltage() : base(ActionID.MakeSpell(AID.HighVoltage), "Interruptible") { }
    }

    class RepellingCannons : Components.SelfTargetedAOEs
    {
        public RepellingCannons() : base(ActionID.MakeSpell(AID.RepellingCannons), new AOEShapeCircle(8.3f)) { }
    }

    class PiercingLaser : Components.SelfTargetedAOEs
    {
        public PiercingLaser() : base(ActionID.MakeSpell(AID.PiercingLaser), new AOEShapeRect(32.3f, 3)) { }
    }

    class DirtyCannons : Components.SelfTargetedAOEs
    {
        public DirtyCannons() : base(ActionID.MakeSpell(AID.DirtyCannons), new AOEShapeCircle(5.15f)) { }
    }

    class GravityField : Components.PersistentVoidzoneAtCastTarget
    {
        public GravityField() : base(6, ActionID.MakeSpell(AID.GravityField), m => m.Enemies(OID.GravityField), 2, true) { }
    }

    // TODO: chain lightning?..

    class T00ADSStates : StateMachineBuilder
    {
        public T00ADSStates(BossModule module) : base(module)
        {
            // adds spawn: at ~40.3, ~80.3, ~120.3, ~160.3, ~200.3 (2x)
            // enrage: first cast at ~245.2, then repeat every 5s
            TrivialPhase(245)
                .ActivateOnEnter<HighVoltage>()
                .ActivateOnEnter<RepellingCannons>()
                .ActivateOnEnter<PiercingLaser>()
                .ActivateOnEnter<DirtyCannons>()
                .ActivateOnEnter<GravityField>();
        }
    }

    public class T00ADS : BossModule
    {
        public T00ADS(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsRect(new(-3, 27), 7, 28)) { }

        public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.CalculateAIHints(slot, actor, assignment, hints);
            hints.UpdatePotentialTargets(enemy =>
            {
                if (enemy.Actor == PrimaryActor)
                {
                    enemy.Priority = 1;
                    enemy.ShouldBeInterrupted = true; // only interruptible spell (high voltage) should be interrupted every time (TODO: consider physranged even/mt odd)
                }
                else if ((OID)enemy.Actor.OID is OID.PatrolNode or OID.AttackNode or OID.DefenseNode)
                {
                    // these always spawn at south entrance, let OT tank them facing away from raid
                    // even at MINE first add spawns when boss has ~25% hp left, so it makes sense just to offtank it and zerg boss
                    enemy.Priority = assignment == PartyRolesConfig.Assignment.OT ? 2 : 0;
                    enemy.TankAffinity = AIHints.TankAffinity.OT;
                    enemy.DesiredRotation = 0.Degrees();
                }
            });
        }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
            foreach (var e in Enemies(OID.PatrolNode))
                Arena.Actor(e, ArenaColor.Enemy);
            foreach (var e in Enemies(OID.AttackNode))
                Arena.Actor(e, ArenaColor.Enemy);
            foreach (var e in Enemies(OID.DefenseNode))
                Arena.Actor(e, ArenaColor.Enemy);
        }
    }
}
