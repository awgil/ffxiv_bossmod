using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS1TrinitySeeker
{
    // TODO: generalize to earthshaker component
    class DeadIron : Components.GenericAOEs
    {
        private List<(Actor target, Actor source, DateTime activation)> _earthshakers = new();

        private static AOEShapeCone _shape = new(50, 15.Degrees());

        public DeadIron() : base(ActionID.MakeSpell(AID.DeadIronAOE), "GTFO from earthshaker!") { }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            foreach (var e in _earthshakers)
                if (e.target != actor)
                    yield return new(_shape, e.source.Position, Angle.FromDirection(e.target.Position - e.source.Position), e.activation);
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            base.AddHints(module, slot, actor, hints, movementHints);

            var ownSource = _earthshakers.Find(e => e.target == actor).source;
            if (ownSource != null)
            {
                var dir = Angle.FromDirection(actor.Position - ownSource.Position);
                hints.Add("GTFO from others!", module.Raid.WithoutSlot().Any(a => a != actor && _shape.Check(a.Position, ownSource.Position, dir)));
            }
        }

        public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        {
            return _earthshakers.Any(e => e.target == player) ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            var ownSource = _earthshakers.Find(e => e.target == pc).source;
            if (ownSource != null)
                _shape.Outline(arena, ownSource.Position, Angle.FromDirection(pc.Position - ownSource.Position));
        }

        public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            // tether is from player (tether source == earthshaker target) to avatar (tether target == earthshaker source)
            if (tether.ID == (uint)TetherID.DeadIron && module.WorldState.Actors.Find(tether.Target) is var target && target != null)
                _earthshakers.Add((source, target, module.WorldState.CurrentTime.AddSeconds(4.6f)));
        }
    }
}
