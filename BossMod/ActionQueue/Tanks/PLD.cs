namespace BossMod.PLD;

public enum AID : uint
{
    None = 0,
    Sprint = ClassShared.AID.Sprint,

    LastBastion = 199, // LB3, instant, range 0, AOE 50 circle, targets=self, animLock=3.860s?
    FastBlade = 9, // L1, instant, GCD, range 3, single-target, targets=hostile
    FightOrFlight = 20, // L2, instant, 60.0s CD (group 10), range 0, single-target, targets=self
    RiotBlade = 15, // L4, instant, GCD, range 3, single-target, targets=hostile
    TotalEclipse = 7381, // L6, instant, GCD, range 0, AOE 5 circle, targets=self
    ReleaseIronWill = 32065, // L10, instant, 1.0s CD (group 3), range 0, single-target, targets=self
    IronWill = 28, // L10, instant, 2.0s CD (group 3), range 0, single-target, targets=self
    ShieldBash = 16, // L10, instant, GCD, range 3, single-target, targets=hostile
    ShieldLob = 24, // L15, instant, GCD, range 20, single-target, targets=hostile
    RageOfHalone = 21, // L26, instant, GCD, range 3, single-target, targets=hostile
    SpiritsWithin = 29, // L30, instant, 30.0s CD (group 5), range 3, single-target, targets=hostile
    Sheltron = 3542, // L35, instant, 5.0s CD (group 0), range 0, single-target, targets=self
    Sentinel = 17, // L38, instant, 120.0s CD (group 19), range 0, single-target, targets=self
    Prominence = 16457, // L40, instant, GCD, range 0, AOE 5 circle, targets=self
    Cover = 27, // L45, instant, 120.0s CD (group 20), range 10, single-target, targets=party
    CircleOfScorn = 23, // L50, instant, 30.0s CD (group 4), range 0, AOE 5 circle, targets=self
    HallowedGround = 30, // L50, instant, 420.0s CD (group 24), range 0, single-target, targets=self
    Bulwark = 22, // L52, instant, 90.0s CD (group 15), range 0, single-target, targets=self
    GoringBlade = 3538, // L54, instant, 60.0s CD (group 12/57), range 3, single-target, targets=hostile
    DivineVeil = 3540, // L56, instant, 90.0s CD (group 14), range 0, AOE 30 circle, targets=self
    Clemency = 3541, // L58, 1.5s cast, GCD, range 30, single-target, targets=self/party/alliance/friendly
    RoyalAuthority = 3539, // L60, instant, GCD, range 3, single-target, targets=hostile
    Intervention = 7382, // L62, instant, 10.0s CD (group 1), range 30, single-target, targets=party
    HolySpirit = 7384, // L64, 1.5s cast, GCD, range 25, single-target, targets=hostile
    Requiescat = 7383, // L68, instant, 60.0s CD (group 11), range 3, single-target, targets=hostile
    PassageOfArms = 7385, // L70, instant, 120.0s CD (group 21), range 0, ???, targets=self
    HolyCircle = 16458, // L72, 1.5s cast, GCD, range 0, AOE 5 circle, targets=self
    Intervene = 16461, // L74, instant, 30.0s CD (group 9/70) (2 charges), range 20, single-target, targets=hostile
    Atonement = 16460, // L76, instant, GCD, range 3, single-target, targets=hostile
    Supplication = 36918, // L76, instant, GCD, range 3, single-target, targets=hostile
    Sepulchre = 36919, // L76, instant, GCD, range 3, single-target, targets=hostile
    Confiteor = 16459, // L80, instant, GCD, range 25, AOE 5 circle, targets=hostile
    HolySheltron = 25746, // L82, instant, 5.0s CD (group 2), range 0, single-target, targets=self
    Expiacion = 25747, // L86, instant, 30.0s CD (group 5), range 3, AOE 5 circle, targets=hostile
    BladeOfFaith = 25748, // L90, instant, GCD, range 25, AOE 5 circle, targets=hostile
    BladeOfTruth = 25749, // L90, instant, GCD, range 25, AOE 5 circle, targets=hostile
    BladeOfValor = 25750, // L90, instant, GCD, range 25, AOE 5 circle, targets=hostile
    Guardian = 36920,
    Imperator = 36921,
    BladeOfHonor = 36922,

    // Shared
    ShieldWall = ClassShared.AID.ShieldWall, // LB1, instant, range 0, AOE 50 circle, targets=self, animLock=1.930
    Stronghold = ClassShared.AID.Stronghold, // LB2, instant, range 0, AOE 50 circle, targets=self, animLock=3.860
    Rampart = ClassShared.AID.Rampart, // L8, instant, 90.0s CD (group 46), range 0, single-target, targets=self
    LowBlow = ClassShared.AID.LowBlow, // L12, instant, 25.0s CD (group 41), range 3, single-target, targets=hostile
    Provoke = ClassShared.AID.Provoke, // L15, instant, 30.0s CD (group 42), range 25, single-target, targets=hostile
    Interject = ClassShared.AID.Interject, // L18, instant, 30.0s CD (group 43), range 3, single-target, targets=hostile
    Reprisal = ClassShared.AID.Reprisal, // L22, instant, 60.0s CD (group 44), range 0, AOE 5 circle, targets=self
    ArmsLength = ClassShared.AID.ArmsLength, // L32, instant, 120.0s CD (group 48), range 0, single-target, targets=self
    Shirk = ClassShared.AID.Shirk, // L48, instant, 120.0s CD (group 49), range 25, single-target, targets=party
}

public enum TraitID : uint
{
    None = 0,
    TankMastery = 317, // L1
    OathMastery = 209, // L35, gauge unlock
    Chivalry = 246, // L58, riot blade & spirits within restore mp
    RageOfHaloneMastery = 260, // L60, rage of halone -> royal authority upgrade
    DivineMagicMastery1 = 207, // L64, reduce mp cost and prevent interruptions
    EnhancedProminence = 261, // L72, prominence restores mp
    EnhancedSheltron = 262, // L74, duration increase
    SwordOath = 264, // L76
    EnhancedRequiescat = 263, // L80
    SheltronMastery = 412, // L82, sheltron -> holy sheltron upgrade
    EnhancedIntervention = 413, // L82
    DivineMagicMastery2 = 414, // L84, adds heal
    MeleeMastery = 504, // L84, potency increase
    SpiritsWithinMastery = 415, // L86, spirits within -> expiacion upgrade
    EnhancedDivineVeil = 416, // L88, adds heal
}

// TODO: regenerate
public enum SID : uint
{
    None = 0,
    FightOrFlight = 76, // applied by Fight or Flight to self
    IronWill = 79, // applied by Iron Will to self
    GoringBladeReady = 3847, // applied by Fight or Flight to self
    Requiescat = 1368, // applied by Requiescat to self
    ConfiteorReady = 3019, // applied by Requiescat to self
    CircleOfScorn = 248, // applied by Circle of Scorn to target
    AtonementReady = 1902, // applied by Royal Authority to self
    DivineMight = 2673, // applied by Royal Authority to self
    SupplicationReady = 3827, // applied by Atonement to self
    SepulchreReady = 3828, // applied by Supplication to self
    BladeOfHonorReady = 3831, // applied by Requiescat to self

    //Shared
    Reprisal = ClassShared.SID.Reprisal, // applied by Reprisal to target
}

public sealed class Definitions : IDisposable
{
    private readonly PLDConfig _config = Service.Config.Get<PLDConfig>();
    public Definitions(ActionDefinitions d)
    {
        d.RegisterSpell(AID.LastBastion, instantAnimLock: 3.86f);
        d.RegisterSpell(AID.FastBlade);
        d.RegisterSpell(AID.FightOrFlight);
        d.RegisterSpell(AID.RiotBlade);
        d.RegisterSpell(AID.TotalEclipse);
        d.RegisterSpell(AID.ReleaseIronWill);
        d.RegisterSpell(AID.IronWill);
        d.RegisterSpell(AID.ShieldBash);
        d.RegisterSpell(AID.ShieldLob);
        d.RegisterSpell(AID.RageOfHalone);
        d.RegisterSpell(AID.SpiritsWithin);
        d.RegisterSpell(AID.Sheltron);
        d.RegisterSpell(AID.Sentinel);
        d.RegisterSpell(AID.Prominence);
        d.RegisterSpell(AID.Cover);
        d.RegisterSpell(AID.CircleOfScorn);
        d.RegisterSpell(AID.HallowedGround);
        d.RegisterSpell(AID.Bulwark);
        d.RegisterSpell(AID.GoringBlade);
        d.RegisterSpell(AID.DivineVeil);
        d.RegisterSpell(AID.Clemency);
        d.RegisterSpell(AID.RoyalAuthority);
        d.RegisterSpell(AID.Intervention);
        d.RegisterSpell(AID.HolySpirit);
        d.RegisterSpell(AID.Requiescat);
        d.RegisterSpell(AID.PassageOfArms);
        d.RegisterSpell(AID.HolyCircle);
        d.RegisterSpell(AID.Intervene);
        d.RegisterSpell(AID.Atonement);
        d.RegisterSpell(AID.Supplication);
        d.RegisterSpell(AID.Sepulchre);
        d.RegisterSpell(AID.Confiteor);
        d.RegisterSpell(AID.HolySheltron);
        d.RegisterSpell(AID.Expiacion);
        d.RegisterSpell(AID.BladeOfFaith);
        d.RegisterSpell(AID.BladeOfTruth);
        d.RegisterSpell(AID.BladeOfValor);
        d.RegisterSpell(AID.Imperator);
        d.RegisterSpell(AID.Guardian);
        d.RegisterSpell(AID.BladeOfHonor);

        Customize(d);
    }

    public void Dispose() { }

    private void Customize(ActionDefinitions d)
    {
        d.Spell(AID.PassageOfArms)!.TransformAngle = (ws, player, _, _) => _config.Wings switch
        {
            PLDConfig.WingsBehavior.CharacterForward => player.Rotation + 180.Degrees(),
            PLDConfig.WingsBehavior.CameraBackward => ws.Client.CameraAzimuth + 180.Degrees(),
            PLDConfig.WingsBehavior.CameraForward => ws.Client.CameraAzimuth,
            _ => null
        };

        d.Spell(AID.Intervention)!.SmartTarget = ActionDefinitions.SmartTargetCoTank;
        d.Spell(AID.HolySpirit)!.ForbidExecute = (ws, player, _, _) => _config.ForbidEarlyHolySpirit && !player.InCombat && ws.Client.CountdownRemaining > 1.75f;
        d.Spell(AID.ShieldLob)!.ForbidExecute = (ws, player, _, _) => _config.ForbidEarlyShieldLob && !player.InCombat && ws.Client.CountdownRemaining > 0.7f;
        //d.Spell(AID.LastBastion)!.EffectDuration = 8;
        //d.Spell(AID.FightOrFlight)!.EffectDuration = 20;
        //d.Spell(AID.Sheltron)!.EffectDuration = 4; // TODO: duration increases to 6...
        //d.Spell(AID.Sentinel)!.EffectDuration = 15;
        // TODO: Cover effect duration 12?..
        //d.Spell(AID.HallowedGround)!.EffectDuration = 10;
        //d.Spell(AID.DivineVeil)!.EffectDuration = 30;
        // TODO: Intervention effect duration?

        d.Spell(AID.Intervene)!.ForbidExecute = ActionDefinitions.DashToTargetCheck;
    }
}
