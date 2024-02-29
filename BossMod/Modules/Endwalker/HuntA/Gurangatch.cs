﻿namespace BossMod.Endwalker.HuntA.Gurangatch
{
    public enum OID : uint
    {
        Boss = 0x361B, // R6.000, x1
    };

    public enum AID : uint
    {
        AutoAttack = 870, // Boss->player, no cast, single-target
        LeftHammerSlammer = 27493, // Boss->self, 5.0s cast, range 30 180-degree cone
        RightHammerSlammer = 27494, // Boss->self, 5.0s cast, range 30 180-degree cone
        LeftHammerSecond = 27495, // Boss->self, 1.0s cast, range 30 180-degree cone
        RightHammerSecond = 27496, // Boss->self, 1.0s cast, range 30 180-degree cone
        OctupleSlammerLCW = 27497, // Boss->self, 9.0s cast, range 30 180-degree cone
        OctupleSlammerRCW = 27498, // Boss->self, 9.0s cast, range 30 180-degree cone
        OctupleSlammerLCCW = 27521, // Boss->self, 9.0s cast, range 30 180-degree cone
        OctupleSlammerRCCW = 27522, // Boss->self, 9.0s cast, range 30 180-degree cone
        OctupleSlammerRestL = 27499, // Boss->self, 1.0s cast, range 30 180-degree cone
        OctupleSlammerRestR = 27500, // Boss->self, 1.0s cast, range 30 180-degree cone
        WildCharge = 27511, // Boss->players, no cast, width 8 rect charge
        BoneShaker = 27512, // Boss->self, 4.0s cast, range 30 circle
    };

    class Slammer : Components.GenericRotatingAOE
    {
        private static readonly AOEShapeCone _shape = new(30, 90.Degrees());

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.OctupleSlammerLCW:
                case AID.OctupleSlammerRCW:
                    Sequences.Add(new(_shape, caster.Position, spell.Rotation, 90.Degrees(), spell.NPCFinishAt, 3.7f, 8));
                    ImminentColor = ArenaColor.Danger;
                    break;
                case AID.OctupleSlammerLCCW:
                case AID.OctupleSlammerRCCW:
                    Sequences.Add(new(_shape, caster.Position, spell.Rotation, -90.Degrees(), spell.NPCFinishAt, 3.7f, 8));
                    ImminentColor = ArenaColor.Danger;
                    break;
                case AID.LeftHammerSlammer:
                case AID.RightHammerSlammer:
                    Sequences.Add(new(_shape, caster.Position, spell.Rotation, 180.Degrees(), spell.NPCFinishAt, 3.6f, 2, 1));
                    ImminentColor = ArenaColor.AOE;
                    break;
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (Sequences.Count > 0 && caster == module.PrimaryActor && (AID)spell.Action.ID is AID.LeftHammerSlammer or AID.RightHammerSlammer or AID.LeftHammerSecond or AID.RightHammerSecond
                or AID.OctupleSlammerLCW or AID.OctupleSlammerRCW or AID.OctupleSlammerRestL or AID.OctupleSlammerRestR or AID.OctupleSlammerLCCW or AID.OctupleSlammerRCCW)
            {
                AdvanceSequence(0, module.WorldState.CurrentTime);
            }
        }
    }

    class BoneShaker : Components.RaidwideCast
    {
        public BoneShaker() : base(ActionID.MakeSpell(AID.BoneShaker)) { }
    }

    class GurangatchStates : StateMachineBuilder
    {
        public GurangatchStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<Slammer>()
                .ActivateOnEnter<BoneShaker>();
        }
    }

    [ModuleInfo(NotoriousMonsterID = 215)]
    public class Gurangatch : SimpleBossModule
    {
        public Gurangatch(WorldState ws, Actor primary) : base(ws, primary) { }
    }
}
