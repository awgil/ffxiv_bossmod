using System.Collections.Generic;
using System.Linq;

namespace BossMod.RealmReborn.Dungeon.D06Haukke.D063LadyAmandine
{
    public enum OID : uint
    {
        Boss = 0x38A8, // x1
        Sentry = 0x38A9, // x1
        Handmaiden = 0x38AA, // spawn during fight
    };

    public enum AID : uint
    {
        AutoAttackBoss = 28647, // Boss->player, no cast
        Teleport = 28644, // Boss->location, no cast
        VoidCall = 28640, // Boss->self, 4.0s cast, visual (summons add)
        DarkMist = 28646, // Boss->self, 4.0s cast, range 9 aoe
        BeguilingMist = 28649, // Boss->self, 3.9s cast, visual
        BeguilingMistAOE = 28643, // Boss->self, no cast, unavoidable
        VoidThunder3 = 28645, // Boss->player, 5.0s cast, interruptible tankbuster

        PetrifyingEye = 28648, // Sentry->self, 5.0s cast, gaze

        AutoAttackAdd = 870, // Handmaiden->player, no cast
        ColdCaress = 28642, // Handmaiden->player, no cast
        Stoneskin = 28641, // Handmaiden->Boss, 5.0s cast, buff target
    };

    class DarkMist : Components.SelfTargetedAOEs
    {
        public DarkMist() : base(ActionID.MakeSpell(AID.DarkMist), new AOEShapeCircle(9)) { }
    }

    class BeguilingMist : Components.CastHint
    {
        public BeguilingMist() : base(ActionID.MakeSpell(AID.BeguilingMist), "Forced movement towards boss") { }
    }

    class VoidThunder : Components.SingleTargetCast
    {
        public VoidThunder() : base(ActionID.MakeSpell(AID.VoidThunder3), "Interruptible tankbuster") { }
    }

    class PetrifyingEye : Components.CastGaze
    {
        public PetrifyingEye() : base(ActionID.MakeSpell(AID.PetrifyingEye)) { }
    }

    class D063LadyAmandineStates : StateMachineBuilder
    {
        public D063LadyAmandineStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<DarkMist>()
                .ActivateOnEnter<BeguilingMist>()
                .ActivateOnEnter<VoidThunder>()
                .ActivateOnEnter<PetrifyingEye>();
        }
    }

    [ModuleInfo(CFCID = 6, NameID = 422)]
    public class D063LadyAmandine : BossModule
    {
        public D063LadyAmandine(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(0, 4), 20)) { }

        public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.CalculateAIHints(slot, actor, assignment, hints);
            foreach (var e in hints.PotentialTargets)
            {
                e.Priority = (OID)e.Actor.OID switch
                {
                    OID.Handmaiden => 2,
                    OID.Boss => 1,
                    _ => 0
                };
            }
        }
    }
}
