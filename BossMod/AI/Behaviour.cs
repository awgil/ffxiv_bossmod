using Dalamud.Game.ClientState.Party;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod.AI
{
    // base class for AI behaviours
    abstract class Behaviour
    {
        // execute behavour; return true if it continues to be active, or false if it is to stop and revert to previous
        public abstract bool Execute(Actor master);
    }
}
