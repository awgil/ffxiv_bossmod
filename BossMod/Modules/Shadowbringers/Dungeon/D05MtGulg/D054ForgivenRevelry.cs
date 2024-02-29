// CONTRIB: made by malediktus, not checked
using System;
using System.Collections.Generic;

namespace BossMod.Shadowbringers.Dungeon.D05MtGulg.D054ForgivenRevelry
{
    public enum OID : uint
    {
        Boss = 0x28F3, //R=7.5
        Helper = 0x2E8, //R=0.5
        Brightsphere = 0x2947, //R=1.0
    }

    public enum AID : uint
    {
        AutoAttack = 16246, // Boss->player, no cast, single-target
        LeftPalm = 16249, // Boss->self, no cast, single-target
        LeftPalm2 = 16250, // 233C->self, 4,5s cast, range 30 width 15 rect
        LightShot = 16251, // Brightsphere->self, 4,0s cast, range 40 width 4 rect
        RightPalm = 16247, // Boss->self, no cast, single-target
        RightPalm2 = 16248, // 233C->self, 4,5s cast, range 30 width 15 rect
    };

    class PalmAttacks : Components.GenericAOEs //Palm Attacks have a wrong origin, so i made a custom solution
    {
        private DateTime _activation;
        private bool left;
        private bool right;

        private static readonly AOEShapeRect rect = new(15, 15);
        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (left)
                yield return new(rect, new(module.PrimaryActor.Position.X, module.Bounds.Center.Z), -90.Degrees(), _activation);
            if (right)
                yield return new(rect, new(module.PrimaryActor.Position.X, module.Bounds.Center.Z), 90.Degrees(), _activation);

        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.LeftPalm2:
                    left = true;
                    _activation = spell.NPCFinishAt;
                    break;
                case AID.RightPalm2:
                    right = true;
                    _activation = spell.NPCFinishAt;
                    break;
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.LeftPalm2 or AID.RightPalm2)
            {
                left = false;
                right = false;
            }
        }
    }

    class LightShot : Components.SelfTargetedAOEs
    {
        public LightShot() : base(ActionID.MakeSpell(AID.LightShot), new AOEShapeRect(40, 2)) { }
    }

    class D054ForgivenRevelryStates : StateMachineBuilder
    {
        public D054ForgivenRevelryStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<PalmAttacks>()
                .ActivateOnEnter<LightShot>();
        }
    }

    [ModuleInfo(CFCID = 659, NameID = 8270)]
    public class D054ForgivenRevelry : BossModule
    {
        public D054ForgivenRevelry(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(-240, 176), 15)) { }
    }
}
