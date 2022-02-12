using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod
{
    using static Class;
    public enum Class : byte
    {
        None = 0,
        GLA = 1,
        PGL = 2,
        MRD = 3,
        LNC = 4,
        ARC = 5,
        CNJ = 6,
        THM = 7,
        CRP = 8,
        BSM = 9,
        ARM = 10,
        GSM = 11,
        LTW = 12,
        WVR = 13,
        ALC = 14,
        CUL = 15,
        MIN = 16,
        BTN = 17,
        FSH = 18,
        PLD = 19,
        MNK = 20,
        WAR = 21,
        DRG = 22,
        BRD = 23,
        WHM = 24,
        BLM = 25,
        ACN = 26,
        SMN = 27,
        SCH = 28,
        ROG = 29,
        NIN = 30,
        MCH = 31,
        DRK = 32,
        AST = 33,
        SAM = 34,
        RDM = 35,
        BLU = 36,
        GNB = 37,
        DNC = 38,
        RPR = 39,
        SGE = 40,
    }

    public enum Role
    {
        None = 0,
        Tank = 1,
        Melee = 2,
        Ranged = 3,
        Healer = 4,
    }

    public static class ClassRole
    {
        public static Role GetRole(this Class cls)
        {
            return cls switch {
                GLA or PLD or MRD or WAR or DRK or GNB => Role.Tank,
                LNC or DRG or PGL or MNK or ROG or NIN or SAM or RPR => Role.Melee,
                ARC or BRD or MCH or DNC => Role.Ranged,
                THM or BLM or ACN or SMN or RDM => Role.Ranged,
                BLU => Role.Ranged,
                SCH or CNJ or WHM or AST or SGE => Role.Healer,
                _ => Role.None
            };
        }
    }
}
