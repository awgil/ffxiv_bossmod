namespace BossMod;

public enum CrucibleID : uint
{
    None = 0,
    G1BeastPotion,
    G2BeastPotion,
    G3BeastPotion,
    G4BeastPotion,
    G1CrucibleAsh,
    G2CrucibleAsh,
    G3CrucibleAsh,
    CrucibleAntidote,
    CrucibleNeedle,
    CrucibleEyeDrops,
    G1AntipoisonSoulSerum,
    G2AntipoisonSoulSerum,
    G1AntiparalysisSoulSerum,
    G2AntiparalysisSoulSerum,
    G1AntiblindSoulSerum,
    G2AntiblindSoulSerum,
    G1AntipetrificationSoulSerum,
    G2AntipetrificationSoulSerum,
    G1AntisleepSoulSerum,
    G2AntisleepSoulSerum,
    BlessedHorn,
    BeastlySmokebomb,
    G1BeastmasterReraiser,
    G2BeastmasterReraiser,
    ThiefsEye,
    MerchantsEye,
    CrucibleTannin,
    CrucibleStimulant,
    PotionOfTemperedStrength,
    PotionOfFeralStrength,
    PotionOfTemperedMagic,
    PotionOfFeralMagic,
    PotionOfTemperedIntensity,
    PotionOfFeralIntensity,
    PotionOfTemperedConstitution,
    PotionOfFeralConstitution,
    PotionOfBreathtakingSwiftness,
    CrucibleFeather,
    G1WildfireWeakener,
    G2WildfireWeakener,
    G1WildwaveWeakener,
    G2WildwaveWeakener,
    G1WildearthWeakener,
    G2WildearthWeakener,
    G1WildthunderWeakener,
    G2WildthunderWeakener,
    G1WildgaleWeakener,
    G2WildgaleWeakener,
    G1WildfreezeWeakener,
    G2WildfreezeWeakener,
    FangOfFire,
    FangOfIce,
    FangOfWater,
    FangOfLightning,
    FangOfEarth,
    FangOfWind,
    VampiricFang,
    VampiricEssence,
    TomeOfReflection,
    TomeOfTheImpervious,
    TemporalSand,
    CelestialSand,
    BeastPotionKit,
    BeastRemedyKit,

    // these two have no associated action, presumably cut content
    // SpellforgeTome,
    // SteelforgeTome,

    Count
}

public static class CrucibleItemID
{
    // magic numbers! XBMItem has no reference to Action sheet
    public static uint GetSpellID(CrucibleID id) => 46958 + (uint)id;
    public static uint GetXBMRow(CrucibleID id) => 75 + (uint)id;

    public static CrucibleID GetFromXBMRow(uint rowId) => rowId > 75 ? (CrucibleID)(rowId - 75) : CrucibleID.None;
}
