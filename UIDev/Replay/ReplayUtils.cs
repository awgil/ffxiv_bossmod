using BossMod;
using System.Numerics;

namespace UIDev
{
    public static class ReplayUtils
    {
        public static string PosRotString(Vector4 posRot)
        {
            return $"[{posRot.X:f2}, {posRot.Y:f2}, {posRot.Z:f2}, {Utils.RadianString(posRot.W)}]";
        }

        public static string ParticipantString(Replay.Participant? p)
        {
            return p != null ? $"{p.Type} {p.InstanceID:X} ({p.OID:X}) '{p.Name}'" : "<none>";
        }

        public static string ParticipantPosRotString(Replay.Participant? p, Vector4 posRot)
        {
            return $"{ParticipantString(p)} {PosRotString(posRot)}";
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
    }
}
