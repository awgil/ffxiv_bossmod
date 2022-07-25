using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod.Shadowbringers.Ultimate.TEA
{
    class P1Throttle : BossComponent
    {
        public bool Applied { get; private set; }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            if ((SID)status.ID == SID.Throttle)
                Applied = true;
        }
    }
}
