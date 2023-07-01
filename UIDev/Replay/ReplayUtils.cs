using BossMod;
using System;
using System.Linq;

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
            if (eff.FromTarget)
                s = "(from target) " + s;
            if (eff.AtSource)
                s = "(at source) " + s;
            var desc = ActionEffectParser.DescribeFields(eff);
            if (desc.Length > 0)
                s += $": {desc}";
            return s;
        }

        public static string ActionTargetString(Replay.ActionTarget t, DateTime ts)
        {
            var participant = t.Target != null ? ParticipantPosRotString(t.Target, ts) : t.TargetID != 0 ? $"<unknown {t.TargetID:X}>" : "<none>";
            var confirmTarget = t.ConfirmationTarget != default ? $"confirmed at +{(t.ConfirmationTarget - ts).TotalSeconds:f3}s" : "unconfirmed";
            var confirmSource = t.ConfirmationSource != default ? $"confirmed at +{(t.ConfirmationSource - ts).TotalSeconds:f3}s" : "unconfirmed";
            return $"{participant}, target {confirmTarget}, source {confirmSource}";
        }

        public static int ActionDamage(Replay.ActionTarget a)
        {
            int res = 0;
            foreach (var eff in a.Effects.Where(eff => eff.Type is ActionEffectType.Damage or ActionEffectType.BlockedDamage or ActionEffectType.ParriedDamage && !eff.AtSource))
                res += eff.DamageHealValue;
            return res;
        }
    }
}
