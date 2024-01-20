using System.Collections.Generic;
using System.Linq;
using BossMod.Components;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace BossMod.MaskedCarnivale.Stage06.Act1
{
    public enum OID : uint
    {
        Boss = 0x25CD, //R=2.53
        Mandragora = 0x2700, //R0.3
    };

    public enum AID : uint
    {
        TearyTwirl = 14693, // 2700->self, 3,0s cast, range 6+R circle
        DemonEye = 14691, // 25CD->self, 5,0s cast, range 50+R circle
        Attack = 6499, // 2700/25CD->player, no cast, single-target
        ColdStare = 14692, // 25CD->self, 2,5s cast, range 40+R 90-degree cone
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
        public ColdStare() : base(ActionID.MakeSpell(AID.ColdStare), new AOEShapeCone(40,45.Degrees())) { } 
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
        public TearyTwirl() : base(ActionID.MakeSpell(AID.TearyTwirl), 6) { }
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

    class Hints : BossComponent
    {
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            hints.Add("Get blinded by the Teary Twirl AOE from the mandragoras.\nBlindness makes you immune to all the gaze attacks.\nThe eyes in act 2 are weak to lightning damage.");
        } 
    }   
    class Stage06Act2States : StateMachineBuilder
    {
        public Stage06Act2States(BossModule module) : base(module)
        {
            TrivialPhase()
            .DeactivateOnEnter<Hints>()
            .ActivateOnEnter<TearyTwirl>()
            .ActivateOnEnter<DemonEye>()
            .ActivateOnEnter<ColdStare>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.Mandragora).All(e => e.IsDead);
        }
    }

    public class Stage06Act2 : BossModule
    {

        public static float BarricadeRadius = 20;
        public Stage06Act2(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 25))
        {
            ActivateComponent<Hints>();
        }
        protected override void DrawArenaForeground(int pcSlot, Actor pc)
        {
                Arena.AddQuad(new(100,107),new(107,100),new(100,93),new(93,100), ArenaColor.Border, 2);
        }
        protected override bool CheckPull() { return PrimaryActor.IsTargetable && PrimaryActor.InCombat || Enemies(OID.Mandragora).Any(e => e.InCombat); }
        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            foreach (var s in Enemies(OID.Boss))
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
                    OID.Boss => 1,
                    OID.Mandragora => 0,
                    _ => 0
                };
            }
        }
    }
}
