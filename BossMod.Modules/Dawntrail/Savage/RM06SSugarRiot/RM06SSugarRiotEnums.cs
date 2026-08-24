namespace BossMod.Dawntrail.Savage.RM06SSugarRiot;

public enum OID : uint
{
    Boss = 0x479F, // R4.000, x0-1
    Helper = 0x233C, // R0.500, x17-35, Helper type
    Canvas = 0x47B4, // R1.000, x0-1
    HeavenBomb = 0x47A1, // R0.800, x0 (spawn during fight)
    PaintBomb = 0x47A0, // R0.800, x0 (spawn during fight)
    CandiedSuccubus = 0x47A5, // R2.500, x0 (spawn during fight)
    MouthwateringMorbol = 0x47A4, // R4.550, x0 (spawn during fight)
    Yan = 0x47A8, // R1.000, x0 (spawn during fight)
    Mu = 0x47A7, // R1.800, x0 (spawn during fight)
    StickyPudding = 0x47A6, // R1.200, x0 (spawn during fight)
    GimmeCat = 0x47AB, // R1.650, x0 (spawn during fight)
    Jabberwock = 0x47A9, // R3.000, x0 (spawn during fight)
    FeatherRay = 0x47AA, // R1.600, x0 (spawn during fight)
    SweetShot = 0x47A2, // R1.500, x0 (spawn during fight)
    TempestPiece = 0x47A3, // R0.800, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 42932, // Boss->player, no cast, single-target

    MousseMural = 42684, // Boss->self, 5.0s cast, range 100 circle
    ColorRiotIceClose = 42641, // Boss->self, 5.0+2.0s cast, single-target
    ColorRiotFireClose = 42642, // Boss->self, 5.0+2.0s cast, single-target
    CoolBomb = 42643, // Helper->players, no cast, range 4 circle
    WarmBomb = 42644, // Helper->players, no cast, range 4 circle
    WingmarkBoss = 42614, // Boss->self, 4.0+0.9s cast, single-target
    ColorClashParty = 42635, // Boss->self, 3.0s cast, single-target
    ColorClashPairs = 42637, // Boss->self, 3.0s cast, single-target
    DoubleStyleAdds1 = 37834, // Boss->self, 12.0+0.9s cast, single-target
    DoubleStyleAdds2 = 37896, // Boss->self, 12.0+0.9s cast, single-target
    DoubleStyleAdds3 = 42621, // Boss->self, 12.0+0.9s cast, single-target
    DoubleStyleAdds4 = 42622, // Boss->self, 12.0+0.9s cast, single-target
    DoubleStyleAdds5 = 42623, // Boss->self, 12.0+0.9s cast, single-target
    DoubleStyleAdds6 = 42624, // Boss->self, 12.0+0.9s cast, single-target
    DoubleStyleAdds7 = 42625, // Boss->self, 12.0+0.9s cast, single-target
    DoubleStyleAdds8 = 42626, // Boss->self, 12.0+0.9s cast, single-target
    DarkMist = 42629, // CandiedSuccubus->self, 1.0s cast, range 30 circle
    ColorClash1 = 42639, // Helper->players, no cast, range 6 circle
    ColorClash2 = 42640, // Helper->players, no cast, range 6 circle
    StickyMousseVisual = 42645, // Boss->self, 5.0+0.6s cast, single-target
    StickyMousse = 42646, // Helper->players, no cast, range 4 circle, root
    Burst = 42647, // Helper->location, no cast, range 4 circle, rooted player explodes, need to stack
    SugarscapeDesert = 42600, // Boss->self, 1.0+7.0s cast, single-target
    LayerDesert1 = 42602, // Boss->self, 1.0+6.0s cast, single-target
    LayerDesert2 = 42604, // Boss->self, 1.0+6.0s cast, single-target
    SprayPain1 = 42657, // Helper->self, 7.0s cast, range 10 circle
    SprayPain2 = 39468, // Helper->self, 8.5s cast, range 10 circle
    CrowdBrulee = 39469, // Helper->location, no cast, range 6 circle
    Brulee = 42658, // Helper->location, no cast, range 15 circle
    PuddingGrafVisual = 42677, // Boss->self, 3.0s cast, single-target
    DoubleStyleBombs = 42627, // Boss->self, 8.0+0.9s cast, single-target
    PuddingGraf = 42678, // Helper->players, 5.0s cast, range 6 circle
    HeavenBombExplosion = 42620, // HeavenBomb->location, 1.0s cast, single-target
    SoulSugar = 42661, // Boss->self, 3.0s cast, single-target
    LivePainting1 = 42662, // Boss->self, 4.0s cast, single-target
    UnlimitedCraving = 39479, // GimmeCat->self, no cast, single-target
    CatDash = 42672, // GimmeCat->location, no cast, single-target
    YanAuto = 37920, // Yan->player, no cast, single-target
    MuAuto = 37919, // Mu->player, no cast, single-target
    ICraveViolence = 42673, // GimmeCat->self, 3.0s cast, range 6 circle
    LivePainting2 = 42663, // Boss->self, 4.0s cast, single-target
    WaterIIIVisual = 37831, // FeatherRay->self, 3.0s cast, single-target
    WaterIIISpread = 42671, // FeatherRay->players, no cast, range 8 circle
    LivePainting3 = 42664, // Boss->self, 4.0s cast, single-target
    ManxomeWindersnatch = 42669, // Jabberwock->player, no cast, single-target
    ReadyOreNot = 42666, // Boss->self, 7.0s cast, range 100 circle
    OreRigato = 42668, // Mu->self, 5.0s cast, range 100 circle
    LivePainting4 = 42665, // Boss->self, 4.0s cast, single-target
    HangryHiss = 42674, // GimmeCat->self, 5.0s cast, range 100 circle
    RallyingCheer = 42667, // Mu->Yan, no cast, single-target
    HeavenBombBurst = 42619, // HeavenBomb->location, 1.0s cast, range 15 circle
    PaintBombBurst = 42617, // PaintBomb->self, 1.0s cast, range 15 circle
    BadBreath = 42628, // MouthwateringMorbol->self, 1.0s cast, range 50 100-degree cone
    SlayousSnickerSnack = 42670, // Jabberwock->player, no cast, single-target
    SingleStyle = 39485, // Boss->self, 6.0+0.9s cast, single-target
    Rush = 42630, // SweetShot->location, 1.0s cast, width 7 rect charge
    SugarscapeRiver = 42595, // Boss->self, 1.0+7.0s cast, single-target
    DoubleStyleFire = 42631, // Boss->self, 6.0+0.9s cast, single-target
    DoubleStyleLightning = 42633, // Boss->self, 6.0+0.9s cast, single-target
    TasteOfFire = 42632, // Helper->players, no cast, range 6 circle
    LayerStorm = 42648, // Boss->self, 1.0+6.0s cast, single-target
    LightningFlash = 42652, // Helper->self, no cast, single-target - lightning VFX on screen, indicating baits have gone off
    TasteOfThunderDelayed = 42653, // Helper->location, 3.0s cast, range 3 circle
    Highlightning = 42651, // 47A3->self, 2.0s cast, range 21 circle
    LightningBolt = 42650, // Helper->location, 3.0s cast, range 4 circle
    LightningStorm = 42654, // Helper->players, no cast, range 8 circle
    TasteOfThunderSpread = 42634, // Helper->players, no cast, range 6 circle
    LevinDrop = 42655, // Helper->self, no cast, range 60 circle, one of these two is the electrified grass AOE but idk which
    LevinMerengue = 42656, // Helper->self, no cast, range 60 circle
    PuddingPartyVisual = 42605, // Boss->self, 4.0+1.0s cast, single-target
    PuddingParty = 42681, // Helper->players, no cast, range 6 circle
    LayerLava = 42649, // Boss->self, 1.0+6.0s cast, single-target
    MousseDripVisual = 42679, // Boss->self, 5.0s cast, single-target
    MousseDrip = 42680, // Helper->players, no cast, range 5 circle
    MoussacreVisual = 42682, // Boss->self, 4.0+1.0s cast, single-target
    Moussacre = 42683, // Helper->self, no cast, range 60 ?-degree cone, maybe 30?
    Explosion = 42659, // Helper->self, no cast, range 3 circle
    UnmitigatedExplosion = 42660, // Helper->self, no cast, range 100 circle, tower failure
    ArtisticAnarchy = 42685, // Boss->self, 8.0+0.9s cast, single-target
    RushEnrage = 42690, // SweetShot->location, 3.0s cast, width 7 rect charge
    BurstEnrage1 = 42687, // HeavenBomb->location, 3.0s cast, range 15 circle
    BadBreathEnrage = 42688, // MouthwateringMorbol->self, 3.0s cast, range 50 100-degree cone
    BurstEnrage2 = 42686, // PaintBomb->self, 3.0s cast, range 15 circle
    DarkMistEnrage = 42689, // CandiedSuccubus->self, 3.0s cast, range 30 circle

    BossDash = 42611, // Boss->location, no cast, single-target
    Unk1 = 42636, // Boss->self, no cast, single-target
    Unk2 = 42638, // Boss->self, no cast, single-target
}

public enum TetherID : uint
{
    Unk324 = 324, // player/StickyPudding->Boss
    PinkTether = 319, // HeavenBomb/PaintBomb/MouthwateringMorbol/CandiedSuccubus/player->Boss
    BlueTether = 320, // CandiedSuccubus/player/MouthwateringMorbol->Boss
    Canvas = 337, // Canvas->Boss
    FeatherRay = 17, // FeatherRay->player
}

public enum IconID : uint
{
    JabberwockPrey = 23, // player->self
    LightningStorm = 602, // player->self
    PuddingParty = 305, // player->self
    MousseDrip = 316, // player->self
}

public enum SID : uint
{
    MagicVulnerabilityUp = 2941, // Helper->player, extra=0x0
    WarmTint = 4451, // Helper->player, extra=0x0
    CoolTint = 4452, // Helper->player, extra=0x0
    Unk1 = 2056, // none->HeavenBomb, extra=0x36B
    Wingmark = 4450, // none->player, extra=0x0
    Stun = 4163, // none->player, extra=0x0
    MousseMine = 4453, // Helper->player, extra=0x0
    Sweltering = 4449, // none->player, extra=0x0, dot
    BurningUp = 4448, // none->player, extra=0x0, party stack
    HeatingUp = 4454, // none->player, extra=0x0, defamation
    FireResistanceDownII = 4383, // Helper->player, extra=0x0, applied by defam/stack in cactus phase
    SixFulmsUnder = 567, // none->player, extra=0x300/0x361/0x362/0x363
    HuffyCat = 4457, // none->GimmeCat, extra=0x1/0x2/0x3, 3 stacks = cat enrage (meteor)
    VulnerabilityUp = 3361, // FeatherRay->player, extra=0x0
    Dropsy = 3075, // none->player, extra=0x0
    Dropsy1 = 3076, // none->player, extra=0x0
    DamageUp = 3129, // Mu->Yan, extra=0x0
    Bind = 3625, // Jabberwock->player, extra=0x0
}
