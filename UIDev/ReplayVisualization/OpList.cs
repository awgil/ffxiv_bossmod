using BossMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIDev
{
    class OpList
    {
        private Replay _replay;
        private Tree _tree;

        public OpList(Replay r, Tree t)
        {
            _replay = r;
            _tree = t;
        }

        public void Draw(IEnumerable<ReplayOps.Operation> ops, DateTime reference)
        {
            foreach (var n in _tree.Node("Settings"))
            {
                DrawSettings();
            }

            foreach (var op in ops.Where(FilterOp))
            {
                foreach (var n in _tree.Node($"{(op.Timestamp - reference).TotalSeconds:f3}: {OpName(op)}", OpLeaf(op)))
                {
                    //DrawOp(op);
                }
            }
        }

        private void DrawSettings()
        {

        }

        private bool FilterOp(ReplayOps.Operation op)
        {
            return op switch
            {
                ReplayOps.OpActorMove => false,
                ReplayOps.OpActorCombat => false,
                ReplayOps.OpActorHP => false,
                ReplayOps.OpActorTarget => false,
                _ => true
            };
        }

        private string OpName(ReplayOps.Operation o)
        {
            return o switch
            {
                ReplayOps.OpActorCreate op => $"Actor create: {ActorString(op.InstanceID, op.Timestamp)}",
                ReplayOps.OpActorDestroy op => $"Actor destroy: {ActorString(op.InstanceID, op.Timestamp)}",
                //ReplayOps.OpActorStatus op => $"Status: {}",
                _ => o.ToString() ?? o.GetType().Name
            };
        }

        private bool OpLeaf(ReplayOps.Operation o)
        {
            return o switch
            {
                ReplayOps.OpEventCast op => op.Value.Targets.Count == 0,
                _ => true
            };
        }

        private string ActorString(uint instanceID, DateTime timestamp)
        {
            var p = _replay.Participants.Find(p => p.InstanceID == instanceID && p.Existence.Contains(timestamp));
            return ReplayUtils.ParticipantString(p);
        }
    }
}
