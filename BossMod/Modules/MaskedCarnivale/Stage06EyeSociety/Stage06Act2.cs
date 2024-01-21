using System.Linq;
using BossMod.Components;

namespace BossMod.MaskedCarnivale.Stage06.Act2
{
    public enum OID : uint
    {
        Boss = 0x26FF, //R=2.53
        Eye = 0x25CE, //R1.35
        Mandragora = 0x2701, //R0.3
    };

    public enum AID : uint
    {
        TearyTwirl = 14693, // 2701->self, 3,0s cast, range 6+R circle
        DemonEye = 14691, // 26FF->self, 5,0s cast, range 50+R circle
        Attack = 6499, // /26FF/2701->player, no cast, single-target
        ColdStare = 14692, // 26FF->self, 2,5s cast, range 40+R 90-degree cone
        Stone = 14695, // 25CE->player, 1,0s cast, single-target
        DreadGaze = 14694, // 25CE->self, 3,0s cast, range 6+R ?-degree cone

    };
    public enum SID : uint
    {
        Blind = 571, // Mandragora->player, extra=0x0

    };
    class DemonEye : CastGaze
    {
        public DemonEye() : base(ActionID.MakeSpell(AID.DemonEye)) {}
        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
            {
            if (actor == module.Raid.Player())
                {if ((SID)status.ID == SID.Blind)
                    {
                        Risky = false;
                    }
                }
            }
        public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
            {
            if (actor == module.Raid.Player())
                {if ((SID)status.ID == SID.Blind)
                    {
                        Risky = true;
                    }
                }
            }
    }
    class ColdStare : SelfTargetedAOEs
    {
        public ColdStare() : base(ActionID.MakeSpell(AID.ColdStare), new AOEShapeCone(42.53f,45.Degrees())) { } 
        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
            {
            if (actor == module.Raid.Player())
                {if ((SID)status.ID == SID.Blind)
                    {
                        Risky = false;
                        Color = ArenaColor.Invisible;
                    }
                }
            }
        public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
            {
            if (actor == module.Raid.Player())
                {if ((SID)status.ID == SID.Blind)
                    {
                        Risky = true;
                    }
                }
            }
    }
    class TearyTwirl : StackWithCastTargets
    {
    private bool blinded = false;
        public TearyTwirl() : base(ActionID.MakeSpell(AID.TearyTwirl), 6.3f) { }
                public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
            {
            if (actor == module.Raid.Player())
                {if ((SID)status.ID == SID.Blind)
                    {
                        blinded = true;
                    }
                }
            }
        public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
            {
            if (actor == module.Raid.Player())
                {if ((SID)status.ID == SID.Blind)
                    {
                        blinded = false;
                    }
                }
            }
        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints) 
            {
                if (!blinded)
                    hints.Add("Stack to get blinded!", false);
            }
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
            {
                if (blinded)
                hints.Add("Kill mandragoras last incase you need to get blinded again.");
            } 
    }

    class DreadGaze : SelfTargetedAOEs
    {
        public DreadGaze() : base(ActionID.MakeSpell(AID.DreadGaze), new AOEShapeCone(7.35f,45.Degrees())) { } 
        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
            {
            if (actor == module.Raid.Player())
                {if ((SID)status.ID == SID.Blind)
                    {
                        Risky = false;
                        Color = ArenaColor.Background;
                    }
                }
            }
        public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
            {
            if (actor == module.Raid.Player())
                {if ((SID)status.ID == SID.Blind)
                    {
                        Risky = true;
                    }
                }
            }
    }
class Hints : BossComponent
    {
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            hints.Add("The eyes are weak to lightning spells.");
        } 
    }
    class Stage06Act2States : StateMachineBuilder
    {
        public Stage06Act2States(BossModule module) : base(module)
        {
            TrivialPhase()
            .ActivateOnEnter<TearyTwirl>()
            .ActivateOnEnter<ColdStare>()
            .ActivateOnEnter<DreadGaze>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.Mandragora).All(e => e.IsDead) && module.Enemies(OID.Eye).All(e => e.IsDead);
        }
    }

    public class Stage06Act2 : BossModule
    {
        public Stage06Act2(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 25))
        {
            ActivateComponent<DemonEye>();
            ActivateComponent<Hints>();
        }
        protected override bool CheckPull() { return PrimaryActor.IsTargetable && PrimaryActor.InCombat || Enemies(OID.Mandragora).Any(e => e.InCombat) || Enemies(OID.Eye).Any(e => e.InCombat); }
        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            foreach (var s in Enemies(OID.Boss))
                Arena.Actor(s, ArenaColor.Enemy, false);
            foreach (var s in Enemies(OID.Eye))
                Arena.Actor(s, ArenaColor.Enemy, false);
            foreach (var s in Enemies(OID.Mandragora))
                Arena.Actor(s, ArenaColor.Object, false);
        }
        public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
        base.CalculateAIHints(slot, actor, assignment, hints);
            foreach (var e in hints.PotentialTargets)
            {
                e.Priority = (OID)e.Actor.OID switch
                {
                    OID.Boss or OID.Eye => 1,
                    OID.Mandragora => 0,
                    _ => 0
                };
            }
        }
    }
}
