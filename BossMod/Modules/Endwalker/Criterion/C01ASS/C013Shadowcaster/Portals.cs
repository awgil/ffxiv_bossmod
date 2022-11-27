using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Criterion.C01ASS.C013Shadowcaster
{
    class Portals : Components.GenericAOEs
    {
        private OID _movedOID;
        private List<Actor> _movedActors = new();
        private float _activationDelay;
        private AOEShape _shape;
        private List<(WPos pos, Angle rot, DateTime activation)> _origins = new();

        private static float _portalLength = 10;

        public Portals(AID aid, OID movedOID, float activationDelay, AOEShape shape) : base(ActionID.MakeSpell(aid))
        {
            _movedOID = movedOID;
            _activationDelay = activationDelay;
            _shape = shape;
        }

        public override void Init(BossModule module)
        {
            _movedActors = module.Enemies(_movedOID);
        }

        public override IEnumerable<(AOEShape shape, WPos origin, Angle rotation, DateTime time)> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            return _origins.Select(o => (_shape, o.pos, o.rot, o.activation));
        }

        public override void OnActorEAnim(BossModule module, Actor actor, uint state)
        {
            if ((OID)actor.OID != OID.Portal)
                return;

            int rotation = state switch
            {
                0x00400080 => -90, // CW arrows appear
                0x01000200 => +90, // CCW arrows appear
                _ => 0, // other known: 0x04000800 = CW arrows end, 0x10002000 = CCW arrows end, 0x00100020 = arrows disappear, 0x00040008 = disappear
            };
            if (rotation == 0)
                return;

            var movedActor = _movedActors.Find(a => a.Position.AlmostEqual(actor.Position, 1));
            if (movedActor != null)
                _origins.Add((actor.Position + _portalLength * (actor.Rotation + rotation.Degrees()).ToDirection(), movedActor.Rotation, module.WorldState.CurrentTime.AddSeconds(_activationDelay)));
        }
    }

    class PortalsBurn : Portals
    {
        public PortalsBurn() : base(AID.Burn, OID.BallOfFire, 11.6f, new AOEShapeCircle(12)) { }
    }

    class PortalsMirror : Portals
    {
        public PortalsMirror() : base(AID.BlazingBenifice, OID.ArcaneFont, 11.7f, new AOEShapeRect(100, 5, 100)) { }
    }
}
