using BossMod;
using System;
using System.Collections.Generic;

namespace UIDev
{
    public class ReplayParser
    {
        protected Replay _res = new();
        protected WorldState _ws = new();
        private Dictionary<uint, Replay.Encounter> _encounters = new();

        protected ReplayParser()
        {
            _ws.Actors.InCombatChanged += ActorCombat;
        }

        protected void AddOp(DateTime timestamp, ReplayOps.Operation op)
        {
            _ws.CurrentTime = op.Timestamp = timestamp;
            op.Redo(_ws);
            _res.Ops.Add(op);
        }

        protected Replay Finish()
        {
            foreach (var enc in _encounters.Values)
            {
                enc.End = _ws.CurrentTime;
            }
            return _res;
        }

        private void ActorCombat(object? sender, Actor actor)
        {
            if (actor.InCombat)
            {
                var m = ModuleRegistry.TypeForOID(actor.OID);
                if (m != null)
                {
                    var enc = new Replay.Encounter();
                    enc.OID = actor.OID;
                    enc.Start = _ws.CurrentTime;
                    enc.Zone = _ws.CurrentZone;
                    _encounters[actor.InstanceID] = enc;
                    _res.Encounters.Add(enc);
                }
            }
            else
            {
                var enc = _encounters.GetValueOrDefault(actor.InstanceID);
                if (enc != null)
                {
                    enc.End = _ws.CurrentTime;
                    _encounters.Remove(actor.InstanceID);
                }
            }
        }
    }
}
