namespace BossMod.SCH;

public enum AID : uint
{
    None = 0,
    Sprint = ClassShared.AID.Sprint,

    AngelFeathers = 4247, // LB3, 2.0s cast, range 0, AOE 50 circle, targets=self, castAnimLock=8.100
    Ruin1 = 17869, // L1, 1.5s cast, GCD, range 25, single-target, targets=hostile, animLock=???
    Bio1 = 17864, // L2, instant, GCD, range 25, single-target, targets=hostile, animLock=???
    Physick = 190, // L4, 1.5s cast, GCD, range 30, single-target, targets=self/party/alliance/friendly
    SummonEos = 17215, // L4, 1.5s cast, GCD, range 0, single-target, targets=self
    WhisperingDawn = 16537, // L20, instant, 60.0s CD (group 13), range 0, single-target, targets=self
    Bio2 = 17865, // L26, instant, GCD, range 25, single-target, targets=hostile
    Adloquium = 185, // L30, 2.0s cast, GCD, range 30, single-target, targets=self/party/alliance/friendly
    Succor = 186, // L35, 2.0s cast, GCD, range 0, AOE 15 circle, targets=self
    Ruin2 = 17870, // L38, instant, GCD, range 25, single-target, targets=hostile
    FeyIllumination = 16538, // L40, instant, 120.0s CD (group 21), range 0, single-target, targets=self
    Aetherflow = 166, // L45, instant, 60.0s CD (group 12), range 0, single-target, targets=self
    EnergyDrain = 167, // L45, instant, 1.0s CD (group 3), range 25, single-target, targets=hostile
    Lustrate = 189, // L45, instant, 1.0s CD (group 0), range 30, single-target, targets=self/party/alliance/friendly
    ArtOfWar1 = 16539, // L46, instant, GCD, range 0, AOE 5 circle, targets=self
    SacredSoil = 188, // L50, instant, 30.0s CD (group 5), range 30, ???, targets=area
    Indomitability = 3583, // L52, instant, 30.0s CD (group 6), range 0, AOE 15 circle, targets=self
    Broil1 = 3584, // L54, 1.5s cast, GCD, range 25, single-target, targets=hostile, animLock=???
    DeploymentTactics = 3585, // L56, instant, 120.0s CD (group 19), range 30, AOE 30 circle, targets=self/party
    EmergencyTactics = 3586, // L58, instant, 15.0s CD (group 4), range 0, single-target, targets=self
    Dissipation = 3587, // L60, instant, 180.0s CD (group 24), range 0, single-target, targets=self
    Excogitation = 7434, // L62, instant, 45.0s CD (group 8), range 30, single-target, targets=self/party
    Broil2 = 7435, // L64, 1.5s cast, GCD, range 25, single-target, targets=hostile
    ChainStratagem = 7436, // L66, instant, 120.0s CD (group 20), range 25, single-target, targets=hostile
    Aetherpact = 7437, // L70, instant, 3.0s CD (group 2), range 30, single-target, targets=self/party
    DissolveUnion = 7869, // L70, instant, 1.0s CD (group 1), range 0, single-target, targets=self
    Broil3 = 16541, // L72, 1.5s cast, GCD, range 25, single-target, targets=hostile, animLock=???
    Biolysis = 16540, // L72, instant, GCD, range 25, single-target, targets=hostile
    Recitation = 16542, // L74, instant, 90.0s CD (group 14), range 0, single-target, targets=self
    FeyBlessing = 16543, // L76, instant, 60.0s CD (group 10), range 0, single-target, targets=self
    Consolation = 16546, // L80, instant, 30.0s CD (group 9/70) (2? charges), range 0, single-target, targets=self
    SummonSeraph = 16545, // L80, instant, 120.0s CD (group 22), range 0, single-target, targets=self
    Broil4 = 25865, // L82, 1.5s cast, GCD, range 25, single-target, targets=hostile
    ArtOfWar2 = 25866, // L82, instant, GCD, range 0, AOE 5 circle, targets=self
    Protraction = 25867, // L86, instant, 60.0s CD (group 11), range 30, single-target, targets=self/party
    Expedient = 25868, // L90, instant, 120.0s CD (group 23), range 0, AOE 30 circle, targets=self

    // pet abilities (TODO: regenerate)
    PetEmbrace = 802, // L1, instant, 3.0s CD (group 76), range 30, single-target 0/0, targets=self/party/friendly, animLock=???
    PetSeraphicVeil = 16548, // L80, instant, 3.0s CD (group 76), range 30, single-target 0/0, targets=self/party, animLock=???
    PetWhisperingDawn = 803, // L20, instant, range 0, AOE circle 15/0, targets=self, animLock=???
    PetAngelsWhisper = 16550, // L80, instant, range 0, AOE circle 15/0, targets=self, animLock=???
    PetFeyIllumination = 805, // L40, instant, range 0, AOE circle 15/0, targets=self, animLock=???
    PetSeraphicIllumination = 16551, // L80, instant, range 0, AOE circle 15/0, targets=self, animLock=???
    PetFeyUnion = 7438, // L70, instant, range 30, single-target 0/0, targets=party, animLock=???
    PetFeyBlessing = 16544, // L76, instant, range 0, AOE circle 20/0, targets=self, animLock=???
    PetConsolation = 16547, // L80, instant, range 0, AOE circle 20/0, targets=self, animLock=???

    // Shared
    HealingWind = ClassShared.AID.HealingWind, // LB1, 2.0s cast, range 0, AOE 50 circle, targets=self, castAnimLock=2.100
    BreathOfTheEarth = ClassShared.AID.BreathOfTheEarth, // LB2, 2.0s cast, range 0, AOE 50 circle, targets=self, castAnimLock=5.130
    Repose = ClassShared.AID.Repose, // L8, 2.5s cast, GCD, range 30, single-target, targets=hostile
    Esuna = ClassShared.AID.Esuna, // L10, 1.0s cast, GCD, range 30, single-target, targets=self/party/alliance/friendly
    Resurrection = ClassShared.AID.Resurrection, // L12, 8.0s cast, GCD, range 30, single-target, targets=party/alliance/friendly
    LucidDreaming = ClassShared.AID.LucidDreaming, // L14, instant, 60.0s CD (group 45), range 0, single-target, targets=self
    Swiftcast = ClassShared.AID.Swiftcast, // L18, instant, 60.0s CD (group 44), range 0, single-target, targets=self
    Surecast = ClassShared.AID.Surecast, // L44, instant, 120.0s CD (group 48), range 0, single-target, targets=self
    Rescue = ClassShared.AID.Rescue, // L48, instant, 120.0s CD (group 49), range 30, single-target, targets=party
}

public enum TraitID : uint
{
    None = 0,
    MaimAndMend1 = 66, // L20, damage & healing increase
    CorruptionMastery1 = 324, // L26, bio1 -> bio2 upgrade
    MaimAndMend2 = 69, // L40, damage & healing increase
    BroilMastery1 = 214, // L54, ruin1 -> broil1 upgrade, potency increase
    BroilMastery2 = 184, // L64, broil1 -> broil2 upgrade, potency increase
    CorruptionMastery2 = 311, // L72, bio2 -> biolysis upgrade
    BroilMastery3 = 312, // L72, broil2 -> broil3 upgrade, potency increase
    EnhancedSacredSoil = 313, // L78, adds hot
    BroilMastery4 = 491, // L82, broil3 -> broil4 upgrade, potency increase
    ArtOfWarMastery = 492, // L82, art of war 1 -> art of war 2 upgrade
    EnhancedHealingMagic = 493, // L85, potency increase
    EnhancedDeploymentTactics = 494, // L88, reduce cd
}

// TODO: regenerate
public enum SID : uint
{
    None = 0,
    Bio1 = 179, // applied by Bio1 to target, dot
    Bio2 = 189, // applied by Bio2 to target, dot
    Biolysis = 0xFFFFFF, // TODO!
    Galvanize = 297, // applied by Adloquium to target, shield
    LucidDreaming = 1204, // applied by Lucid Dreaming to self
    Swiftcast = 167, // applied by Swiftcast to self
    Sleep = 3, // applied by Repose to target
}

public sealed class Definitions : IDisposable
{
    public Definitions(ActionDefinitions d)
    {
        d.RegisterSpell(AID.AngelFeathers, castAnimLock: 8.10f); // castAnimLock=8.100
        d.RegisterSpell(AID.Ruin1); // animLock=???
        d.RegisterSpell(AID.Bio1); // animLock=???
        d.RegisterSpell(AID.Physick);
        d.RegisterSpell(AID.SummonEos);
        d.RegisterSpell(AID.WhisperingDawn);
        d.RegisterSpell(AID.Bio2);
        d.RegisterSpell(AID.Adloquium);
        d.RegisterSpell(AID.Succor);
        d.RegisterSpell(AID.Ruin2);
        d.RegisterSpell(AID.FeyIllumination);
        d.RegisterSpell(AID.Aetherflow);
        d.RegisterSpell(AID.EnergyDrain);
        d.RegisterSpell(AID.Lustrate);
        d.RegisterSpell(AID.ArtOfWar1);
        d.RegisterSpell(AID.SacredSoil);
        d.RegisterSpell(AID.Indomitability);
        d.RegisterSpell(AID.Broil1); // animLock=???
        d.RegisterSpell(AID.DeploymentTactics);
        d.RegisterSpell(AID.EmergencyTactics);
        d.RegisterSpell(AID.Dissipation);
        d.RegisterSpell(AID.Excogitation);
        d.RegisterSpell(AID.Broil2);
        d.RegisterSpell(AID.ChainStratagem);
        d.RegisterSpell(AID.Aetherpact);
        d.RegisterSpell(AID.DissolveUnion);
        d.RegisterSpell(AID.Broil3); // animLock=???
        d.RegisterSpell(AID.Biolysis);
        d.RegisterSpell(AID.Recitation);
        d.RegisterSpell(AID.FeyBlessing);
        d.RegisterSpell(AID.Consolation);
        d.RegisterSpell(AID.SummonSeraph);
        d.RegisterSpell(AID.Broil4);
        d.RegisterSpell(AID.ArtOfWar2);
        d.RegisterSpell(AID.Protraction);
        d.RegisterSpell(AID.Expedient);

        Customize(d);
    }

    public void Dispose() { }

    private void Customize(ActionDefinitions d)
    {
        // *** add any properties that can't be autogenerated here ***
    }
}
