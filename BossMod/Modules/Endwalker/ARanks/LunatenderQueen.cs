using System;
using System.Numerics;

namespace BossMod.Endwalker.ARanks.LunatenderQueen
{
    public enum OID : uint
    {
        Boss = 0x35DF,
    };

    public enum AID : uint
    {
        AutoAttack = 872,
        AvertYourEyes = 27363,
        YouMayApproach = 27364,
        AwayWithYou = 27365,
        Needles = 27366,
        WickedWhim = 27367,
        AvertYourEyesInverted = 27369,
        YouMayApproachInverted = 27370,
        AwayWithYouInverted = 27371,
    }

    public class Mechanics : BossModule.Component
    {
        private BossModule _module;
        private AOEShapeCircle _needles = new(6);
        private AOEShapeCircle _circle = new(15);
        private AOEShapeDonut _donut = new(6, 40); // TODO: verify inner radius

        public Mechanics(BossModule module)
        {
            _module = module;
        }

        public override void AddHints(int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
        {
            if (ActiveAOE()?.Check(actor.Position, _module.PrimaryActor) ?? false)
                hints.Add("GTFO from aoe!");
            if ((_module.PrimaryActor.CastInfo?.IsSpell(AID.AvertYourEyes) ?? false) && Vector3.Dot(GeometryUtils.DirectionToVec3(actor.Rotation), _module.PrimaryActor.Position - actor.Position) > 0)
                hints.Add("Look away from boss!");
            if ((_module.PrimaryActor.CastInfo?.IsSpell(AID.AvertYourEyesInverted) ?? false) && Vector3.Dot(GeometryUtils.DirectionToVec3(actor.Rotation), _module.PrimaryActor.Position - actor.Position) < 0)
                hints.Add("Look at the boss!");
        }

        public override void AddGlobalHints(BossModule.GlobalHints hints)
        {
            if (!(_module.PrimaryActor.CastInfo?.IsSpell() ?? false))
                return;

            string hint = (AID)_module.PrimaryActor.CastInfo.Action.ID switch
            {
                AID.YouMayApproach or AID.AwayWithYou or AID.YouMayApproachInverted or AID.AwayWithYouInverted or AID.Needles => "Avoidable AOE",
                AID.AvertYourEyes => "Turn away",
                AID.AvertYourEyesInverted => "Face boss",
                AID.WickedWhim => "Invert next cast",
                _ => "",
            };
            if (hint.Length > 0)
                hints.Add(hint);
        }

        public override void DrawArenaBackground(int pcSlot, Actor pc, MiniArena arena)
        {
            ActiveAOE()?.Draw(arena, _module.PrimaryActor);
        }

        private AOEShape? ActiveAOE()
        {
            if (!(_module.PrimaryActor.CastInfo?.IsSpell() ?? false))
                return null;

            return (AID)_module.PrimaryActor.CastInfo.Action.ID switch
            {
                AID.AwayWithYou or AID.YouMayApproachInverted => _circle,
                AID.YouMayApproach or AID.AwayWithYouInverted => _donut,
                AID.Needles => _needles,
                _ => null
            };
        }
    }

    public class LunatenderQueen : SimpleBossModule
    {
        public LunatenderQueen(BossModuleManager manager, Actor primary)
            : base(manager, primary)
        {
            InitialState?.Enter.Add(() => ActivateComponent(new Mechanics(this)));
        }
    }
}
