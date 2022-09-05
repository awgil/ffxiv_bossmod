using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod.Endwalker.Extreme.Ex4Barbariccia
{
    // initial aoe + tethers
    class Tangle : Components.SelfTargetedAOEs
    {
        public int NumTethers { get; private set; }
        private Actor?[] _tethers = new Actor?[PartyState.MaxPartySize];

        public Tangle() : base(ActionID.MakeSpell(AID.Tangle), new AOEShapeCircle(6)) { }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            var tether = _tethers[pcSlot];
            if (tether != null)
            {
                arena.AddCircle(tether.Position, 8, ArenaColor.Object);
            }
        }

        public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            if (tether.ID == (uint)TetherID.Tangle)
            {
                var slot = module.Raid.FindSlot(source.InstanceID);
                var target = module.WorldState.Actors.Find(tether.Target);
                if (slot >= 0 && target != null)
                {
                    _tethers[slot] = target;
                    ++NumTethers;
                }
            }
        }

        public override void OnUntethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            if (tether.ID == (uint)TetherID.Tangle)
            {
                var slot = module.Raid.FindSlot(source.InstanceID);
                if (slot >= 0)
                {
                    _tethers[slot] = null;
                    --NumTethers;
                }
            }
        }
    }
}
