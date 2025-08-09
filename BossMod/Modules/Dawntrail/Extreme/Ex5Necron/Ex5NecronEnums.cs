#pragma warning disable CA1707 // Identifiers should not contain underscores

namespace BossMod.Dawntrail.Extreme.Ex5Necron;

public enum OID : uint
{
    _Gen_LoomingSpecter = 0x49DD, // R3.000, x1-3
    _Gen_LoomingSpecter1 = 0x4910, // R15.750, x2-3
    _Gen_Necron = 0x4945, // R1.000, x4
    Boss = 0x490B, // R20.000, x1
    _Gen_Necron1 = 0x49B2, // R0.000-0.500, x1, Part type
    Helper = 0x233C, // R0.500, x12-18 (spawn during fight), Helper type
    _Gen_IcyHands = 0x490C, // R3.575, x0 (spawn during fight)
    _Gen_AzureAether = 0x4949, // R1.000, x0-1 (spawn during fight)
    _Gen_AzureAether1 = 0x4914, // R1.000, x0 (spawn during fight)
    _Gen_IcyHands1 = 0x4911, // R3.300, x0 (spawn during fight)
    _Gen_BeckoningHands = 0x4912, // R5.500, x0 (spawn during fight)
    _Gen_AzureAether2 = 0x49B0, // R1.000, x0 (spawn during fight)
    _Gen_IcyHands2 = 0x4913, // R3.575, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_ = 44615, // 49B2->player, no cast, single-target
    _Weaponskill_BlueShockwave = 44592, // Boss->self, 6.0+1.0s cast, single-target
    _Weaponskill_BlueShockwave1 = 44593, // Helper->self, no cast, range 100 ?-degree cone
    _Weaponskill_BlueShockwave2 = 44594, // Boss->self, no cast, single-target
    _Weaponskill_FearOfDeath = 44550, // Boss->self, 5.0s cast, range 100 circle
    _Weaponskill_FearOfDeath1 = 44551, // Helper->location, 3.0s cast, range 3 circle
    _Weaponskill_ChokingGrasp = 44552, // 490C->self, no cast, range 24 width 6 rect
    _Weaponskill_ColdGrip = 44553, // Boss->self, 5.0+1.0s cast, single-target
    _Weaponskill_ColdGrip2 = 44554, // Boss->self, 5.0+1.0s cast, single-target
    _Weaponskill_ColdGrip1 = 44612, // Helper->self, 6.0s cast, range 100 width 12 rect
    _Weaponskill_ExistentialDread = 44555, // Helper->self, 1.0s cast, range 100 width 24 rect
    MementoMoriDarkRight = 44565, // Boss->self, 5.0s cast, range 100 width 12 rect
    MementoMoriDarkLeft = 44566, // Boss->self, 5.0s cast, range 100 width 12 rect
    _Weaponskill_ChokingGrasp1 = 44567, // 490C->self, 3.0s cast, range 24 width 6 rect
    _Weaponskill_SmiteOfGloom = 44602, // Helper->player, 4.0s cast, range 10 circle
    _Weaponskill_SmiteOfGloom1 = 44601, // Boss->self, 4.0s cast, single-target
    _Weaponskill_SoulReaping = 44556, // Boss->self, 4.0s cast, single-target
    _Weaponskill_TwofoldBlight = 44557, // Boss->self, 5.0s cast, single-target
    _Weaponskill_Aetherblight = 44607, // Boss->self, no cast, single-target
    _Weaponskill_Aetherblight1 = 44608, // Helper->self, 1.0s cast, range 100 width 12 rect
    _Weaponskill_Shockwave = 44559, // Helper->self, no cast, range 100 ?-degree cone
    _Weaponskill_TheEndsEmbrace = 44597, // Boss->self, 4.0s cast, single-target
    _Weaponskill_TheEndsEmbrace1 = 44598, // Helper->player, no cast, range 3 circle
    _Weaponskill_Aetherblight2 = 44563, // Boss->self, no cast, single-target
    _Weaponskill_Aetherblight3 = 45185, // Helper->self, 1.0s cast, range 100 width 12 rect
    _Weaponskill_GrandCross = 44568, // Boss->location, 7.0s cast, range 50 circle
    _Weaponskill_ = 44604, // Helper->location, 7.0s cast, range 9-60 donut
    _Weaponskill_GrandCross1 = 44571, // Helper->location, 3.0s cast, range 3 circle
    _Weaponskill_GrandCross2 = 44569, // Helper->self, 0.5s cast, range 100 width 4 rect
    _Weaponskill_Shock = 44573, // Helper->self, 5.0s cast, range 3 circle
    _Weaponskill_GrandCross3 = 44572, // Helper->players, 5.0s cast, range 3 circle
    _Weaponskill_GrandCross4 = 44570, // Helper->self, 4.0s cast, range 100 width 100 rect
    _Weaponskill_NeutronRing = 44575, // Boss->location, 7.0s cast, single-target
    _Weaponskill_FourfoldBlight = 44558, // Boss->self, 5.0s cast, single-target
    _Weaponskill_Shockwave1 = 44560, // Helper->self, no cast, range 100 ?-degree cone
    _Weaponskill_Electrify = 44574, // Helper->self, no cast, range 50 circle
    _Weaponskill_NeutronRing1 = 44576, // Helper->self, no cast, range 50 circle
    _Weaponskill_FearOfDeath2 = 44577, // Helper->location, 3.0s cast, range 3 circle
    _Weaponskill_ChokingGrasp2 = 44579, // 4911->self, 3.0s cast, range 24 width 6 rect
    _Weaponskill_MutedStruggle = 44578, // 4912->self/player, 3.0s cast, range 24 width 6 rect
    _Weaponskill_DarknessOfEternity = 44580, // Boss->self, 10.0s cast, single-target
    _Weaponskill_DarknessOfEternity1 = 44581, // Helper->location, no cast, range 50 circle
    _Weaponskill_Inevitability = 44583, // Helper->self, no cast, range 50 circle
    _Weaponskill_SpecterOfDeath = 44606, // Boss->self, 5.0s cast, single-target
    _Weaponskill_Invitation = 44591, // 4910->self, 4.3+0.7s cast, range 36 width 10 rect
    _Weaponskill_Invitation1 = 44818, // Helper->self, 5.0s cast, range 36 width 10 rect
    _Weaponskill_RelentlessReaping = 44564, // Boss->self, 15.0s cast, single-target
    _Weaponskill_CropRotation = 44610, // Boss->self, 3.0s cast, single-target
    _Weaponskill_TheFourthSeason = 45168, // Boss->self, 8.0s cast, single-target
    _Weaponskill_Aetherblight4 = 44561, // Boss->self, no cast, single-target
    _Weaponskill_Aetherblight5 = 45183, // Helper->self, 1.0s cast, range 20 circle
    _Weaponskill_Aetherblight6 = 44562, // Boss->self, no cast, single-target
    _Weaponskill_Aetherblight7 = 45184, // Helper->self, 1.0s cast, range ?-60 donut
    _Weaponskill_CircleOfLives = 44599, // Boss->self, 4.0s cast, single-target
    _Weaponskill_CircleOfLives1 = 44600, // 49B0->self, 7.0s cast, range ?-50 donut
    _Weaponskill_MassMacabre = 44595, // Boss->self, 4.0s cast, single-target
    _Weaponskill_MacabreMark = 44819, // Helper->location, no cast, range 3 circle
    _Weaponskill_SpreadingFear = 44596, // 4913->self, no cast, range 50 circle
    _Weaponskill_TheSecondSeason = 45167, // Boss->self, 8.0s cast, single-target
}

public enum IconID : uint
{
    StoreCircle = 604, // Boss->self
    StoreDonut = 605, // Boss->self
    StoreInverseRect = 606, // Boss->self
    StoreRect = 607, // Boss->self
    SmallSpread5s = 611, // player->self
    HugeSpread = 612, // player->self
    SmallSpread4s = 614, // player->self
    BlueShockwave = 615, // Boss->player
    Tankbuster = 381, // player->self
}
