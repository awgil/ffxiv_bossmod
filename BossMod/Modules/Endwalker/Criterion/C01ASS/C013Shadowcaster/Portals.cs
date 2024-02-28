using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Criterion.C01ASS.C013Shadowcaster
{
    static class Portals
    {
        private static float _portalLength = 10;

        // returns null if this is not arrow appear eanim
        public static WPos? DestinationForEAnim(Actor actor, uint state)
        {
            if ((OID)actor.OID != OID.Portal)
                return null;

            int rotation = state switch
            {
                0x00400080 => -90, // CW arrows appear
                0x01000200 => +90, // CCW arrows appear
                _ => 0, // other known: 0x04000800 = CW arrows end, 0x10002000 = CCW arrows end, 0x00100020 = arrows disappear, 0x00040008 = disappear
            };
            if (rotation == 0)
                return null;

            return actor.Position + _portalLength * (actor.Rotation + rotation.Degrees()).ToDirection();
        }
    }

    class PortalsAOE : Components.GenericAOEs
    {
        private OID _movedOID;
        private IReadOnlyList<Actor> _movedActors = ActorEnumeration.EmptyList;
        private float _activationDelay;
        private AOEShape _shape;
        private List<(WPos pos, Angle rot, DateTime activation)> _origins = new();

        public PortalsAOE(AID aid, OID movedOID, float activationDelay, AOEShape shape) : base(ActionID.MakeSpell(aid))
        {
            _movedOID = movedOID;
            _activationDelay = activationDelay;
            _shape = shape;
        }

        public override void Init(BossModule module)
        {
            _movedActors = module.Enemies(_movedOID);
        }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            return _origins.Select(o => new AOEInstance(_shape, o.pos, o.rot, o.activation));
        }

        public override void OnActorEAnim(BossModule module, Actor actor, uint state)
        {
            var dest = Portals.DestinationForEAnim(actor, state);
            if (dest == null)
                return;

            var movedActor = _movedActors.FirstOrDefault(a => a.Position.AlmostEqual(actor.Position, 1));
            if (movedActor != null)
                _origins.Add((dest.Value, movedActor.Rotation, module.WorldState.CurrentTime.AddSeconds(_activationDelay)));
        }
    }

    class NPortalsBurn : PortalsAOE { public NPortalsBurn() : base(AID.NBurn, OID.NBallOfFire, 11.6f, new AOEShapeCircle(12)) { } }
    class SPortalsBurn : PortalsAOE { public SPortalsBurn() : base(AID.SBurn, OID.SBallOfFire, 11.6f, new AOEShapeCircle(12)) { } }

    class NPortalsMirror : PortalsAOE { public NPortalsMirror() : base(AID.NBlazingBenifice, OID.NArcaneFont, 11.7f, new AOEShapeRect(100, 5, 100)) { } }
    class SPortalsMirror : PortalsAOE { public SPortalsMirror() : base(AID.SBlazingBenifice, OID.SArcaneFont, 11.7f, new AOEShapeRect(100, 5, 100)) { } }

    class PortalsWave : BossComponent
    {
        public bool Done { get; private set; }
        private List<(WPos n, WPos s)> _portals = new();
        private int[] _playerPortals = new int[PartyState.MaxPartySize]; // 0 = unassigned, otherwise 'z direction sign' (-1 if own portal points N, +1 for S)

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            var dir = _playerPortals[pcSlot];
            if (dir != 0)
            {
                foreach (var p in _portals)
                {
                    arena.AddCircle(dir > 0 ? p.s : p.n, 1, ArenaColor.Safe, 2);
                }
            }
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            if ((SID)status.ID == SID.PlayerPortal)
            {
                var slot = module.Raid.FindSlot(actor.InstanceID);
                if (slot >= 0)
                {
                    _playerPortals[slot] = status.Extra switch
                    {
                        0x1CD or 0x1CE => -1,
                        0x1D2 or 0x1D3 => +1,
                        _ => 0
                    };
                }
            }
        }

        public override void OnActorEAnim(BossModule module, Actor actor, uint state)
        {
            if ((OID)actor.OID == OID.Portal && state == 0x00100020)
            {
                Done = true;
                return;
            }

            var dest = Portals.DestinationForEAnim(actor, state);
            if (dest == null || !module.Bounds.Contains(dest.Value))
                return;

            var n = actor.Position;
            var s = dest.Value;
            if (n.Z > s.Z)
                Utils.Swap(ref n, ref s);
            _portals.Add((n, s));
        }
    }
}
