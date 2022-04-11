using BossMod;
using System;
using System.Linq;
using System.Numerics;

namespace UIDev
{
    public static class ReplayUtils
    {
        public static string ParticipantString(Replay.Participant? p)
        {
            return p != null ? $"{p.Type} {p.InstanceID:X} ({p.OID:X}) '{p.Name}'" : "<none>";
        }

        public static string ParticipantPosRotString(Replay.Participant? p, DateTime t)
        {
            return p != null ? $"{ParticipantString(p)} {Utils.PosRotString(p.PosRotAt(t))}" : "<none>";
        }

        public static string ActionEffectString(ActionEffect eff)
        {
            var s = $"{eff.Type}: {eff.Param0:X2} {eff.Param1:X2} {eff.Param2:X2} {eff.Param3:X2} {eff.Param4:X2} {eff.Value:X4}";
            if ((eff.Param4 & 0x80) != 0)
                s = "(source) " + s;
            var desc = ActionEffectParser.DescribeFields(eff);
            if (desc.Length > 0)
                s += $": {desc}";
            return s;
        }

        public static int ActionDamage(Replay.ActionTarget a)
        {
            int res = 0;
            foreach (var eff in a.Effects.Where(eff => eff.Type is ActionEffectType.Damage or ActionEffectType.BlockedDamage or ActionEffectType.ParriedDamage && (eff.Param4 & 0x80) == 0))
                res += eff.Value + ((eff.Param4 & 0x40) != 0 ? eff.Param3 * 0x10000 : 0);
            return res;
        }
    }
}
