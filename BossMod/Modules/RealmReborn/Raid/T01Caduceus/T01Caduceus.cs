using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod.RealmReborn.Raid.T01Caduceus
{
    public enum OID : uint
    {
        Boss = 0x7D7,
    }

    //class XXX : Components.SelfTargetedAOEs
    //{
    //    public XXX() : base(ActionID.MakeSpell(AID.XXX), new AOEShapeRect(40, 2)) { }
    //}

    class T01CaduceusStates : StateMachineBuilder
    {
        public T01CaduceusStates(BossModule module) : base(module)
        {
            TrivialPhase();
            //.ActivateOnEnter<>()
            //;
        }
    }

    public class T01Caduceus : BossModule
    {
        public T01Caduceus(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsRect(new(-26, -407), 34, 40)) { }
    }
}
