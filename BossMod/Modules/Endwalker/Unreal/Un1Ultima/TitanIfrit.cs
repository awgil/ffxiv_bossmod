using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Unreal.Un1Ultima
{
    // both phases use radiant plumes
    class TitanIfrit : BossModule.Component
    {
        private List<(Actor, AOEShapeCircle)> _activeLocationTargetedAOEs = new();
        private List<Actor> _crimsonCyclone = new();

        private static AOEShapeCircle _aoeRadiantPlume = new(8);
        private static AOEShapeCircle _aoeWeightOfLand = new(6);
        private static AOEShapeCircle _aoeEruption = new(8);
        private static AOEShapeRect _aoeCrimsonCyclone = new(38, 6);

        public override void AddHints(BossModule module, int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
        {
            if (_activeLocationTargetedAOEs.Any(e => e.Item2.Check(actor.Position, e.Item1.CastInfo!.LocXZ)) || _crimsonCyclone.Any(a => _aoeCrimsonCyclone.Check(actor.Position, a)))
                hints.Add("GTFO from aoe!");
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var (a, aoe) in _activeLocationTargetedAOEs)
                aoe.Draw(arena, a.CastInfo!.LocXZ);
            foreach (var a in _crimsonCyclone)
                _aoeCrimsonCyclone.Draw(arena, a);
        }

        public override void OnCastStarted(BossModule module, Actor actor)
        {
            if (!actor.CastInfo!.IsSpell())
                return;
            switch ((AID)actor.CastInfo.Action.ID)
            {
                case AID.RadiantPlume:
                    _activeLocationTargetedAOEs.Add((actor, _aoeRadiantPlume));
                    break;
                case AID.WeightOfTheLand:
                    _activeLocationTargetedAOEs.Add((actor, _aoeWeightOfLand));
                    break;
                case AID.Eruption:
                    _activeLocationTargetedAOEs.Add((actor, _aoeEruption));
                    break;
                case AID.CrimsonCyclone:
                    _crimsonCyclone.Add(actor);
                    break;
            }
        }

        public override void OnCastFinished(BossModule module, Actor actor)
        {
            if (!actor.CastInfo!.IsSpell())
                return;
            switch ((AID)actor.CastInfo.Action.ID)
            {
                case AID.RadiantPlume:
                case AID.WeightOfTheLand:
                case AID.Eruption:
                    _activeLocationTargetedAOEs.RemoveAll(e => e.Item1 == actor);
                    break;
                case AID.CrimsonCyclone:
                    _crimsonCyclone.Remove(actor);
                    break;
            }
        }
    }
}
