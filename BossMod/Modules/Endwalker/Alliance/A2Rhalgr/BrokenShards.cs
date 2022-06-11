using System;
using System.Linq;
using System.Numerics;

namespace BossMod.Endwalker.Alliance.A2Rhalgr
{
    // this is not an official mechanic name - it refers to broken world + hand of the destroyer combo, which creates multiple small aoes
    class BrokenShards : BossModule.Component
    {
        private Vector3[]? _targetLocations;

        private static Vector3[] _eastLocations = { new(-30.0f, 475, 266.9f), new(-46.5f, 475, 269.6f), new(-26.2f, 475, 292.9f), new(-2.8f, 475, 283.5f), new(-37.4f, 475, 283.7f), new(1.6f, 475, 271.5f), new(-18.8f, 475, 278.8f), new(-12.3f, 475, 298.3f), new(-34.1f, 475, 250.5f) };
        private static Vector3[] _westLocations = { new(-6.9f, 475, 268.0f), new(-0.2f, 475, 285.0f), new(-25.6f, 475, 298.5f), new(-34.2f, 475, 283.5f), new(-11.6f, 475, 293.5f), new(-46.1f, 475, 270.5f), new(-18.1f, 475, 279.0f), new(-40.3f, 475, 290.5f), new(-2.1f, 475, 252.0f) };
        private static AOEShapeCircle _aoe = new(20);

        public override void AddHints(BossModule module, int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
        {
            if (_targetLocations != null && _targetLocations.Any(l => _aoe.Check(actor.Position, l, new())))
                hints.Add("GTFO from aoe!");
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_targetLocations != null)
                foreach (var l in _targetLocations)
                    _aoe.Draw(arena, l, new());
        }

        public override void OnCastStarted(BossModule module, Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.BrokenShardsE))
                _targetLocations = _eastLocations;
            else if (actor.CastInfo!.IsSpell(AID.BrokenShardsW))
                _targetLocations = _westLocations;
        }

        public override void OnCastFinished(BossModule module, Actor actor)
        {
            if (actor.CastInfo!.IsSpell() && (AID)actor.CastInfo.Action.ID is AID.BrokenShardsE or AID.BrokenShardsW)
                _targetLocations = null;
        }

        public override void OnEventCast(BossModule module, CastEvent info)
        {
            if (info.IsSpell(AID.BrokenShardsAOE))
            {
                var caster = module.WorldState.Actors.Find(info.CasterID);
                if (caster != null && !Array.Exists(_eastLocations, p => PositionAlmostEqual(p, caster.Position)) && Array.Exists(_westLocations, p => PositionAlmostEqual(p, caster.Position)))
                {
                    module.ReportError(this, $"Unexpected shard position: {Utils.Vec3String(caster.Position)}");
                }
            }
        }

        private bool PositionAlmostEqual(Vector3 l, Vector3 r) => Math.Abs(l.X - r.X) < 0.1f && Math.Abs(l.Z - r.Z) < 0.1f;
    }
}
