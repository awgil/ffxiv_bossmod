using System.Linq;
using BossMod.Components;

// CONTRIB: made by malediktus, not checked
namespace BossMod.MaskedCarnivale.Stage08.Act1
{
    public enum OID : uint
    {
        Boss = 0x2708, //R=0.6
        Bomb = 0x2709, //R=1.2
        Snoll = 0x270A, //R=0.9
    };

    public enum AID : uint
    {
        SelfDestruct = 14687, // 2708->self, no cast, range 10 circle
        HypothermalCombustion = 14689, // 270A->self, no cast, range 6 circle
        SelfDestruct2 = 14688, // 2709->self, no cast, range 6 circle
    };

    class Selfdetonations : GenericStackSpread
    {
        private string hint = "In bomb explosion radius!";
        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var p in module.Enemies(OID.Boss).Where(x => x.HP.Cur > 0))
                arena.AddCircle(p.Position, 10, ArenaColor.Danger);
            foreach (var p in module.Enemies(OID.Bomb).Where(x => x.HP.Cur > 0))
                arena.AddCircle(p.Position, 6, ArenaColor.Danger);
            foreach (var p in module.Enemies(OID.Snoll).Where(x => x.HP.Cur > 0))
                arena.AddCircle(p.Position, 6, ArenaColor.Danger);
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            var player = module.Raid.Player();
            if (player != null)
            {
                foreach (var p in module.Enemies(OID.Boss).Where(x => x.HP.Cur > 0))
                    if (player.Position.InCircle(p.Position, 10))
                    {
                        hints.Add(hint);
                    }
                foreach (var p in module.Enemies(OID.Bomb).Where(x => x.HP.Cur > 0))
                    if (player.Position.InCircle(p.Position, 6))
                    {
                        hints.Add(hint);
                    }
                foreach (var p in module.Enemies(OID.Snoll).Where(x => x.HP.Cur > 0))
                    if (player.Position.InCircle(p.Position, 6))
                    {
                        hints.Add(hint);
                    }
            }
        }
    }

    class Hints : BossComponent
    {
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            hints.Add("For this stage the spell Flying Sardine to interrupt the Progenitrix in Act 2\nis highly recommended. Hit the Cherry Bomb from a safe distance\nwith anything but fire damage to set of a chain reaction to win this act.");
        }
    }

    class Hints2 : BossComponent
    {
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            hints.Add("Hit the Cherry Bomb from a safe distance to win this act.");
        }
    }

    class Stage08Act1States : StateMachineBuilder
    {
        public Stage08Act1States(BossModule module) : base(module)
        {
            TrivialPhase()
                .DeactivateOnEnter<Hints>()
                .ActivateOnEnter<Hints2>()
                .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.Bomb).All(e => e.IsDead) && module.Enemies(OID.Snoll).All(e => e.IsDead);
        }
    }

    [ModuleInfo(CFCID = 618, NameID = 8140)]
    public class Stage08Act1 : BossModule
    {
        public Stage08Act1(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 25))
        {
            ActivateComponent<Hints>();
            ActivateComponent<Selfdetonations>();
        }

        protected override bool CheckPull() { return PrimaryActor.IsTargetable && PrimaryActor.InCombat || Enemies(OID.Bomb).Any(e => e.InCombat) || Enemies(OID.Snoll).Any(e => e.InCombat); }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            foreach (var s in Enemies(OID.Boss))
                Arena.Actor(s, ArenaColor.Enemy, false);
            foreach (var s in Enemies(OID.Bomb))
                Arena.Actor(s, ArenaColor.Enemy, false);
            foreach (var s in Enemies(OID.Snoll))
                Arena.Actor(s, ArenaColor.Enemy, false);
        }

        public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.CalculateAIHints(slot, actor, assignment, hints);
            foreach (var e in hints.PotentialTargets)
            {
                e.Priority = (OID)e.Actor.OID switch
                {
                    OID.Boss => 1,
                    OID.Snoll or OID.Bomb => 0,
                    _ => 0
                };
            }
        }
    }
}
