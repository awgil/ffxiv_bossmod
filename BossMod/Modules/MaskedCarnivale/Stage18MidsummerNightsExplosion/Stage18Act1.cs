using System.Linq;

// CONTRIB: made by malediktus, not checked
namespace BossMod.MaskedCarnivale.Stage18.Act1
{
    public enum OID : uint
    {
        Boss = 0x2724, //R=3.0
        Keg = 0x2726, //R=0.65
    };

    public enum AID : uint
    {
        WildCharge = 15055, // 2724->players, 3,5s cast, width 8 rect charge
        Explosion = 15054, // 2726->self, 2,0s cast, range 10 circle
        RipperClaw = 15050, // 2724->self, 4,0s cast, range 5+R 90-degree cone
        Fireball = 15051, // 2724->location, 4,0s cast, range 6 circle
        BoneShaker = 15053, // 2724->self, no cast, range 50 circle
        TailSmash = 15052, // 2724->self, 4,0s cast, range 12+R 90-degree cone
    };

    class Explosion : Components.SelfTargetedAOEs
    {
        public Explosion() : base(ActionID.MakeSpell(AID.Explosion), new AOEShapeCircle(10)) { }
    }

    class Fireball : Components.LocationTargetedAOEs
    {
        public Fireball() : base(ActionID.MakeSpell(AID.Fireball), 6) { }
    }

    class RipperClaw : Components.SelfTargetedAOEs
    {
        public RipperClaw() : base(ActionID.MakeSpell(AID.RipperClaw), new AOEShapeCone(8, 45.Degrees())) { }
    }

    class TailSmash : Components.SelfTargetedAOEs
    {
        public TailSmash() : base(ActionID.MakeSpell(AID.TailSmash), new AOEShapeCone(15, 45.Degrees())) { }
    }

    class WildCharge : Components.BaitAwayChargeCast
    {
        public WildCharge() : base(ActionID.MakeSpell(AID.WildCharge), 4) { }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (CurrentBaits.Count > 0 && !module.Enemies(OID.Keg).All(e => e.IsDead))
                hints.Add("Aim charge at a keg!");
        }
    }

    class WildChargeKB : Components.KnockbackFromCastTarget
    {   //knockback actually delayed by 0.5s to 1s, maybe it depends on the rectangle length of the charge
        public WildChargeKB() : base(ActionID.MakeSpell(AID.WildCharge), 10, kind: Kind.DirForward)
        {
            StopAtWall = true;
        }
    }

    class KegExplosion : Components.GenericStackSpread
    {
        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var p in module.Enemies(OID.Keg).Where(x => !x.IsDead))
            {
                if (arena.Config.ShowOutlinesAndShadows)
                    arena.AddCircle(p.Position, 10, 0xFF000000, 2);
                arena.AddCircle(p.Position, 10, ArenaColor.Danger);
            }
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            foreach (var p in module.Enemies(OID.Keg).Where(x => !x.IsDead))
                if (actor.Position.InCircle(p.Position, 10))
                    hints.Add("In keg explosion radius!");
        }
    }

    class Hints : BossComponent
    {
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            hints.Add("Make the manticores run to the kegs and their attacks will make them\nblow up. They take 2500 damage per keg explosion.\nThe Ram's Voice and Ultravibration combo can be used to kill manticores.");
        }
    }

    class Stage18Act2States : StateMachineBuilder
    {
        public Stage18Act2States(BossModule module) : base(module)
        {
            TrivialPhase()
                .DeactivateOnEnter<Hints>()
                .ActivateOnEnter<Explosion>()
                .ActivateOnEnter<Fireball>()
                .ActivateOnEnter<RipperClaw>()
                .ActivateOnEnter<TailSmash>()
                .ActivateOnEnter<WildCharge>()
                .ActivateOnEnter<WildChargeKB>()
                .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.Keg).All(e => e.IsDead);
        }
    }

    [ModuleInfo(CFCID = 628, NameID = 8116)]
    public class Stage18Act2 : BossModule
    {
        public Stage18Act2(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 25))
        {
            ActivateComponent<Hints>();
            ActivateComponent<KegExplosion>();
        }

        protected override bool CheckPull() { return PrimaryActor.IsTargetable && PrimaryActor.InCombat || Enemies(OID.Keg).Any(e => e.InCombat); }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
            foreach (var s in Enemies(OID.Keg))
                Arena.Actor(s, ArenaColor.Object);
        }

        public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.CalculateAIHints(slot, actor, assignment, hints);
            foreach (var e in hints.PotentialTargets)
            {
                e.Priority = (OID)e.Actor.OID switch
                {
                    OID.Boss => 1,
                    OID.Keg => 0,
                    _ => 0
                };
            }
        }
    }
}
