using System;
using System.Numerics;

namespace BossMod.Endwalker.ARanks.Minerva
{
    public enum OID : uint
    {
        Boss = 0x3609,
    };

    public enum AID : uint
    {
        AutoAttack = 872,
        AntiPersonnelBuild = 27297,
        RingBuild = 27298,
        BallisticMissileCircle = 27299,
        BallisticMissileDonut = 27300,
        Hyperflame = 27301, // TODO: never seen one...
        SonicAmplifier = 27302, // TODO: never seen one...
        HammerKnuckles = 27304, // TODO: never seen one...
        BallisticMissileMarkTarget = 27377,
        BallisticMissileCircleWarning = 27517,
        BallisticMissileDonutWarning = 27518,
    }

    public class Mechanics : BossModule.Component
    {
        private BossModule _module;
        private AOEShapeCircle _ballisticMissileCircle = new(6);
        private AOEShapeDonut _ballisticMissileDonut = new(8, 20); // TODO: verify inner radius
        private AOEShape? _activeBallisticMissile;
        private Actor? _activeBallisticMissileTarget;
        private Vector3 _activeBallisticMissileLocation = new();

        public Mechanics(BossModule module)
        {
            _module = module;
        }

        public override void AddHints(int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
        {
            if (_activeBallisticMissile?.Check(actor.Position, _activeBallisticMissileTarget?.Position ?? _activeBallisticMissileLocation, 0) ?? false)
                hints.Add("GTFO from aoe!");
        }

        public override void AddGlobalHints(BossModule.GlobalHints hints)
        {
            if (!(_module.PrimaryActor.CastInfo?.IsSpell() ?? false))
                return;

            string hint = (AID)_module.PrimaryActor.CastInfo.Action.ID switch
            {
                AID.AntiPersonnelBuild or AID.RingBuild => "Select next AOE type",
                AID.BallisticMissileCircleWarning or AID.BallisticMissileDonutWarning => "Select next AOE target",
                AID.BallisticMissileCircle or AID.BallisticMissileDonut or AID.Hyperflame => "Avoidable AOE",
                AID.HammerKnuckles => "Tankbuster",
                AID.SonicAmplifier => "Raidwide",
                _ => "",
            };
            if (hint.Length > 0)
                hints.Add(hint);
        }

        public override void DrawArenaBackground(int pcSlot, Actor pc, MiniArena arena)
        {
            _activeBallisticMissile?.Draw(arena, _activeBallisticMissileTarget?.Position ?? _activeBallisticMissileLocation, 0);
        }

        public override void OnCastStarted(Actor actor)
        {
            if (actor != _module.PrimaryActor || !actor.CastInfo!.IsSpell())
                return;
            switch ((AID)actor.CastInfo.Action.ID)
            {
                case AID.BallisticMissileCircleWarning:
                    _activeBallisticMissile = _ballisticMissileCircle;
                    _activeBallisticMissileTarget = _module.WorldState.Actors.Find(actor.CastInfo.TargetID);
                    break;
                case AID.BallisticMissileDonutWarning:
                    _activeBallisticMissile = _ballisticMissileDonut;
                    _activeBallisticMissileTarget = _module.WorldState.Actors.Find(actor.CastInfo.TargetID);
                    break;
                case AID.BallisticMissileCircle:
                case AID.BallisticMissileDonut:
                    _activeBallisticMissileLocation = actor.CastInfo.Location;
                    break;
            }
        }

        public override void OnCastFinished(Actor actor)
        {
            if (actor != _module.PrimaryActor || !actor.CastInfo!.IsSpell())
                return;
            switch ((AID)actor.CastInfo.Action.ID)
            {
                case AID.BallisticMissileCircleWarning:
                case AID.BallisticMissileDonutWarning:
                    _activeBallisticMissileLocation = _activeBallisticMissileTarget?.Position ?? new();
                    _activeBallisticMissileTarget = null;
                    break;
                case AID.BallisticMissileCircle:
                case AID.BallisticMissileDonut:
                    _activeBallisticMissile = null;
                    break;
            }
        }
    }

    public class Minerva : SimpleBossModule
    {
        public Minerva(BossModuleManager manager, Actor primary)
            : base(manager, primary)
        {
            InitialState?.Enter.Add(() => ActivateComponent(new Mechanics(this)));
        }
    }
}
