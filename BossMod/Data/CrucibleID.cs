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
    PotionOfTemperedEvasion,
    PotionOfFeralEvasion,
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
    SpellforgeTome,
    SteelstingTome,

    Count
}

public static class CrucibleItemID
{
    public static readonly uint[] SpellID = [
        0, // None
        46959, // G1BeastPotion
        46960, // G2BeastPotion
        46961, // G3BeastPotion
        46962, // G4BeastPotion
        46963, // G1CrucibleAsh
        46964, // G2CrucibleAsh
        46965, // G3CrucibleAsh
        46966, // CrucibleAntidote
        46967, // CrucibleNeedle
        46968, // CrucibleEyeDrops
        46969, // G1AntipoisonSoulSerum
        46970, // G2AntipoisonSoulSerum
        46971, // G1AntiparalysisSoulSerum
        46972, // G2AntiparalysisSoulSerum
        46973, // G1AntiblindSoulSerum
        46974, // G2AntiblindSoulSerum
        46975, // G1AntipetrificationSoulSerum
        46976, // G2AntipetrificationSoulSerum
        46977, // G1AntisleepSoulSerum
        46978, // G2AntisleepSoulSerum
        46979, // BlessedHorn
        46980, // BeastlySmokebomb
        46981, // G1BeastmasterReraiser
        46982, // G2BeastmasterReraiser
        46983, // ThiefsEye
        46984, // MerchantsEye
        46985, // CrucibleTannin
        46986, // CrucibleStimulant
        46987, // PotionOfTemperedStrength
        48376, // PotionOfFeralStrength
        46988, // PotionOfTemperedMagic
        48377, // PotionOfFeralMagic
        46989, // PotionOfTemperedIntensity
        48378, // PotionOfFeralIntensity
        46990, // PotionOfTemperedEvasion
        48379, // PotionOfFeralEvasion
        46991, // PotionOfTemperedConstitution
        48380, // PotionOfFeralConstitution
        46992, // PotionOfBreathtakingSwiftness
        46993, // CrucibleFeather
        46994, // G1WildfireWeakener
        46995, // G2WildfireWeakener
        46996, // G1WildwaveWeakener
        46997, // G2WildwaveWeakener
        46998, // G1WildearthWeakener
        46999, // G2WildearthWeakener
        47000, // G1WildthunderWeakener
        47001, // G2WildthunderWeakener
        47002, // G1WildgaleWeakener
        47003, // G2WildgaleWeakener
        47004, // G1WildfreezeWeakener
        47005, // G2WildfreezeWeakener
        47006, // FangOfFire
        47007, // FangOfIce
        47008, // FangOfWater
        47009, // FangOfLightning
        47010, // FangOfEarth
        47011, // FangOfWind
        47012, // VampiricFang
        47013, // VampiricEssence
        47014, // TomeOfReflection
        47015, // TomeOfTheImpervious
        47016, // TemporalSand
        47017, // CelestialSand
        47018, // BeastPotionKit
        47019, // BeastRemedyKit
        48381, // SpellforgeTome
        48382, // SteelstingTome
    ];

    // magic numbers! XBMItem has no reference to Action sheet, i don't even think these are linked clientside at all
    public static uint GetSpellID(CrucibleID id) => SpellID[(int)id];
    public static uint GetXBMRow(CrucibleID id) => 75 + (uint)id;

    public static CrucibleID GetFromXBMRow(uint rowId) => rowId > 75 ? (CrucibleID)(rowId - 75) : CrucibleID.None;
}
