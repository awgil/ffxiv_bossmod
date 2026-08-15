namespace BossMod.Dawntrail.Foray.ForkedTower.FT14Index;

public enum OID : uint
{
    Boss = 0x4B5F, // R7.500, x1
    Helper = 0x233C, // R0.500, x15 (spawn during fight), Helper type
    _Gen_HolyLance = 0x4B62, // R1.000, x3
    _Gen_TranscribedIndex = 0x4B6F, // R7.500, x3
    _Gen_Index = 0x4B72, // R1.000, x3
    _Gen_BallOfLevin = 0x4B66, // R1.500, x0 (spawn during fight)
    _Gen_SwirlingOrb = 0x4B64, // R1.500, x0 (spawn during fight)
    _Gen_SummonedBomb = 0x4B60, // R2.100, x0 (spawn during fight)
    _Gen_ForetoldPhenomenon = 0x4B63, // R1.000, x0 (spawn during fight)
    _Gen_BallOfFire = 0x4B65, // R1.500, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_ = 48421, // Boss->player, no cast, single-target
    _Spell_Flare = 48415, // Boss->self, 5.0s cast, single-target
    _Spell_Flare1 = 48417, // Helper->self, no cast, ???
    _Weaponskill_SealedImplements = 48384, // Boss->self, 5.0+2.0s cast, single-target
    _Weaponskill_RomeosBallad = 48385, // Helper->self, 7.0s cast, range 15 circle
    _Weaponskill_ = 50665, // Boss->self, no cast, single-target
    _Weaponskill_SealedImplements1 = 48386, // Boss->self, 5.0+2.1s cast, single-target
    _Weaponskill_Aim = 48387, // Helper->self, 7.1s cast, range 11 circle
    _Spell_OmniElements = 48394, // Boss->self, 4.0+1.0s cast, single-target
    _Spell_OmniElements1 = 48395, // Helper->self, no cast, ???
    _Weaponskill_ElementaryEvocation = 48400, // Boss->self, 3.0s cast, single-target
    _Spell_ThunderIV = 48398, // Helper->self, no cast, range 30 ?-degree cone
    _Weaponskill_ElementaryExpansion = 48399, // Boss->self, 3.0s cast, single-target
    _Spell_FireIV = 48396, // Helper->self, no cast, range 30 ?-degree cone
    _Spell_BlizzardIV = 48397, // Helper->self, no cast, range 30 ?-degree cone
    _Spell_ElementaryChemistry = 48401, // Boss->self, 3.9+1.1s cast, single-target
    _Spell_ElementaryChemistry1 = 48402, // Helper->self, no cast, ???
    _Weaponskill_1 = 48905, // Helper->self, 6.0s cast, range 15 width 15 rect
    _Weaponskill_PropulsiveProphecy = 48403, // Boss->self, 3.0s cast, single-target
    _Weaponskill_Jump = 48404, // 4B6F->self, no cast, single-target
    _Weaponskill_Shockwave = 48405, // 4B62->self, 5.0s cast, single-target
    _Weaponskill_Shockwave1 = 48406, // Helper->self, 5.0s cast, ???
    _Weaponskill_Summon = 48408, // Boss->self, 3.0s cast, single-target
    _Weaponskill_DuologyOfImplements = 48388, // Boss->self, 5.0+1.0s cast, single-target
    _Weaponskill_Iainuki = 48389, // Helper->self, 6.0s cast, range 30 60-degree cone
    _Weaponskill_SealedImplements2 = 48904, // Boss->self, no cast, single-target
    _Weaponskill_WindSlash = 48391, // Helper->self, 6.0s cast, range 30 60-degree cone
    _Spell_AllKnowingFlames = 48418, // Boss->self, 5.0s cast, single-target
    _Spell_AllConsumingFlames = 48420, // Helper->players, no cast, range 6 circle
    _Weaponskill_Predict = 48412, // Boss->self, 3.0s cast, single-target
    _Spell_Starfall = 48413, // 4B63->self, 0.5s cast, range 10 circle
    _Spell_Cleansing = 48414, // 4B63->self, 0.5s cast, range ?-15 donut
    _Spell_Dualcast = 48407, // Boss->self, 3.0s cast, single-target
    _Spell_Flare2 = 48416, // Boss->self, no cast, single-target
}

public enum SID : uint
{
    _Gen_SealOfTheHarp = 5535, // none->Boss, extra=0x404
    _Gen_SealOfTheBow = 5534, // none->Boss, extra=0x401
    _Gen_SealOfTheBlade = 5533, // none->Boss, extra=0x402
    _Gen_SealOfTheBell = 5532, // none->Boss, extra=0x403
    _Gen_ = 2552, // none->4B63, extra=0x44D/0x44C
    _Gen_Dualcast = 5438, // Boss->Boss, extra=0x0

}

public enum IconID : uint
{
    _Gen_Icon_loc06sp_05ak1 = 466, // player->self
}

public enum TetherID : uint
{
    _Gen_Tether_chn_m0947_t1_p = 363, // 4B66->4B66, lightning
    _Gen_Tether_chn_m0947_i1_p = 364, // 4B64->4B64, ice
    _Gen_Tether_chn_m0947_f1_p = 365, // 4B65->4B65, fire
    _Gen_Tether_chn_m0361_mainte_1i = 88, // 4B72->4B63, this is for the ball/donut tether thing
}

class Bounds(BossModule module) : BossComponent(module)
{
    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0)
        {
            if (state == 0x00020001)
                Arena.Bounds = FT14Index.MakeIndexBounds(true);
            if (state == 0x00080004)
                Arena.Bounds = FT14Index.MakeIndexBounds(false);
        }
    }
}

class Flare(BossModule module) : Components.RaidwideCastDelay(module, AID._Spell_Flare, AID._Spell_Flare1, 0.8f);
class OmniElements(BossModule module) : Components.RaidwideCastDelay(module, AID._Spell_OmniElements, AID._Spell_OmniElements1, 1.1f);
class RomeosBallad(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_RomeosBallad, 15);
class Aim(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Aim, 11);

class FT14IndexStates : StateMachineBuilder
{
    public FT14IndexStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Bounds>()
            .ActivateOnEnter<Flare>()
            .ActivateOnEnter<OmniElements>()
            .ActivateOnEnter<RomeosBallad>()
            .ActivateOnEnter<Aim>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14717, BitmapType = BossModuleInfo.BitmapType.Disabled)]
public class FT14Index(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -628), MakeIndexBounds(false))
{
    public static ArenaBoundsCustom MakeIndexBounds(bool allPlatforms)
    {
        IEnumerable<WDir> platformSlice = [new WDir(7.5f, 0), new WDir(7.5f, 28), new WDir(-7.5f, 28), new WDir(-7.5f, 0)];
        // widened so the connection between slices is clean
        IEnumerable<WDir> noPlatform = [new WDir(8, 0), new WDir(8, 13), new WDir(-8, 13), new WDir(-8, 0)];

        var poly = new RelSimplifiedComplexPolygon(platformSlice);

        for (var i = 1; i < 6; i++)
        {
            var isPlat = i % 2 == 0 || allPlatforms;
            var shape = (isPlat ? platformSlice : noPlatform).Select(r => r.Rotate((i * 60).Degrees()));
            poly = new PolygonClipper().Union(new(poly), new(shape));
        }

        var holePoint = new WDir(2.886742f, 5);
        var holePoly = Enumerable.Range(0, 6).Select(i => holePoint.Rotate((i * 60).Degrees()));

        poly = new PolygonClipper().Difference(new(poly), new(holePoly));

        return new(new WDir(7.5f, 28).Length(), poly);
    }
}
