using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.ZodiarkEx
{
    // simple component tracking raidwide cast at the end of intermission
    public class Apomnemoneumata : CommonComponents.CastCounter
    {
        public Apomnemoneumata() : base(ActionID.MakeSpell(AID.ApomnemoneumataNormal)) { }
    }

    public class ZodiarkEx : BossModule
    {
        public ZodiarkEx(BossModuleManager manager, Actor primary)
            : base(manager, primary, true)
        {
            new ZodiarkExStates(this);
        }

        protected override void ResetModule()
        {
            ActivateComponent<StyxTargetTracker>();
        }

        protected override void DrawArenaForegroundPost(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, Arena.ColorEnemy);
            Arena.Actor(pc, Arena.ColorPC);
        }
    }
}
