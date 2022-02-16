using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod
{
    public static class ZoneModule
    {
        public static Type? TypeForZone(ushort zone)
        {
            return zone switch {
                993 => typeof(Zodiark),
                1003 => typeof(P1S),
                1005 => typeof(P2S),
                1007 => typeof(P3S),
                1009 => typeof(P4S),
                _ => null,
            };
        }

        public static BossModule? CreateModule(Type? type, WorldState ws)
        {
            return type != null ? (BossModule?)Activator.CreateInstance(type, ws) : null;
        }

        public static BossModule? CreateModule(ushort zone, WorldState ws)
        {
            return CreateModule(TypeForZone(zone), ws);
        }
    }
}
