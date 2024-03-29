namespace BossMod;

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

public enum ClassCategory
{
    Undefined,
    Tank,
    Healer,
    Melee,
    PhysRanged,
    Caster,
    Limited,
}

public enum Role
{
    None = 0,
    Tank = 1,
    Healer = 2,
    Melee = 3,
    Ranged = 4,
}

public static class ClassRole
{
    public static ClassCategory GetClassCategory(this Class cls)
    {
        return cls switch
        {
            Class.GLA or Class.PLD or Class.MRD or Class.WAR or Class.DRK or Class.GNB => ClassCategory.Tank,
            Class.SCH or Class.CNJ or Class.WHM or Class.AST or Class.SGE => ClassCategory.Healer,
            Class.LNC or Class.DRG or Class.PGL or Class.MNK or Class.ROG or Class.NIN or Class.SAM or Class.RPR => ClassCategory.Melee,
            Class.ARC or Class.BRD or Class.MCH or Class.DNC => ClassCategory.PhysRanged,
            Class.THM or Class.BLM or Class.ACN or Class.SMN or Class.RDM => ClassCategory.Caster,
            Class.BLU => ClassCategory.Limited,
            _ => ClassCategory.Undefined
        };
    }

    public static Role GetRole(this Class cls)
    {
        return cls.GetClassCategory() switch {
            ClassCategory.Tank => Role.Tank,
            ClassCategory.Healer => Role.Healer,
            ClassCategory.Melee => Role.Melee,
            ClassCategory.PhysRanged or ClassCategory.Caster or ClassCategory.Limited => Role.Ranged,
            _ => Role.None
        };
    }

    public static bool IsSupport(this Class cls) => cls.GetClassCategory() is ClassCategory.Tank or ClassCategory.Healer;
    public static bool IsDD(this Class cls) => cls.GetClassCategory() is ClassCategory.Melee or ClassCategory.PhysRanged or ClassCategory.Caster;
}
