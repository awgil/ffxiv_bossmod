using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod
{
    public static class CommonComponents
    {
        // generic component that counts specified casts
        public class CastCounter : BossModule.Component
        {
            public int NumCasts { get; private set; } = 0;

            private uint _watchedCastID;

            public CastCounter(uint aid)
            {
                _watchedCastID = aid;
            }

            public override void Reset() => NumCasts = 0;

            public override void OnEventCast(WorldState.CastResult info)
            {
                if (info.IsSpell() && info.ActionID == _watchedCastID)
                {
                    ++NumCasts;
                }
            }
        }
    }
}
