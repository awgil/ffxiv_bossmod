using System;
using System.Linq;

namespace BossMod.Endwalker.Alliance.A2Rhalgr
{
    // this is not an official mechanic name - it refers to broken world + hand of the destroyer combo, which creates multiple small aoes
    class BrokenShards : BossComponent
    {
        private WPos[]? _targetLocations;

        private static WPos[] _eastLocations = { new(-30.0f, 266.9f), new(-46.5f, 269.6f), new(-26.2f, 292.9f), new(-2.8f, 283.5f), new(-37.4f, 283.7f), new(1.6f, 271.5f), new(-18.8f, 278.8f), new(-12.3f, 298.3f), new(-34.1f, 250.5f) };
        private static WPos[] _westLocations = { new(-6.9f, 268.0f), new(-0.2f, 285.0f), new(-25.6f, 298.5f), new(-34.2f, 283.5f), new(-11.6f, 293.5f), new(-46.1f, 270.5f), new(-18.1f, 279.0f), new(-40.3f, 290.5f), new(-2.1f, 252.0f) };
        private static AOEShapeCircle _aoe = new(20);

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_targetLocations != null && _targetLocations.Any(l => _aoe.Check(actor.Position, l)))
                hints.Add("GTFO from aoe!");
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_targetLocations != null)
                foreach (var l in _targetLocations)
                    _aoe.Draw(arena, l);
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
                if (caster != null && !Array.Exists(_eastLocations, p => p.AlmostEqual(caster.Position, 0.1f)) && Array.Exists(_westLocations, p => p.AlmostEqual(caster.Position, 0.1f)))
                {
                    module.ReportError(this, $"Unexpected shard position: {caster.Position}");
                }
            }
        }
    }
}
