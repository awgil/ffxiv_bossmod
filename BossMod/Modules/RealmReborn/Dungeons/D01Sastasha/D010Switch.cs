using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod.RealmReborn.Dungeons.D01Sastasha
{
    class SwitchHint : BossComponent
    {
        private string _hint = "";

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (_hint.Length == 0)
            {
                foreach (var a in module.WorldState.Actors.Where(a => a.IsTargetable))
                {
                    _hint = a.OID switch
                    {
                        0x1E8554 => "blue",
                        0x1E8A8D => "green",
                        0x1E8A8C => "red",
                        _ => ""
                    };
                    if (_hint.Length > 0)
                        break;
                }
            }
            hints.Add($"Correct switch: {_hint}");
        }
    }

    class D010SwitchStates : StateMachineBuilder
    {
        public D010SwitchStates(BossModule module) : base(module)
        {
            TrivialPhase();
        }
    }

    [ModuleInfo(PrimaryActorOID = 0x1E8554)]
    public class D010Switch : BossModule
    {
        public D010Switch(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(primary.Position, 20))
        {
            ActivateComponent<SwitchHint>();
        }
    }
}
