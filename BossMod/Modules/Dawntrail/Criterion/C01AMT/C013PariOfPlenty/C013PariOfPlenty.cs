namespace BossMod.Dawntrail.Criterion.C01AMT.C013PariOfPlenty;

// TODO
//  - Complete timeline - end of fight - need enrage log
//  - Pari's Curse - Add stack/spread AOEs?
//  - Towers add cleaves
//  - FireFlight:
//        - find out who gets the stack
//        - Add Enums value for left and right instead of using side number int
//  - WheelOfFableFlight:
//      - find out who gets the stack
//      - Add initial AOE for the cleaves
//  - Improve visual for SpurningFlames? - Really not needed, but could add indictors on the direction to go instead of showing the AOEs (assumes BD)
//  - Pari's Curse - Mechanic works fine, just a lot of stuff for solving it - Remove gridMap if possible and find a different way to do it

class HeatBurst(BossModule module) : Components.RaidwideCast(module, (uint)AID.HeatBurst);

class FireOfVictory(BossModule module) : Components.BaitAwayCast(module, (uint)AID.FireOfVictory, 4f, true, true, true, AIHints.PredictedDamageType.Tankbuster);

class RedCrystals(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BurningGleam, new AOEShapeCross(40, 5), 3);

class RedCrystals2 : Components.SimpleAOEs {
    public RedCrystals2(BossModule module) : base(module, (uint)AID.BurningGleam, new AOEShapeCross(40, 5), 2) {
        Color = Colors.Danger;
    }
}

class KindleFlameStackIcon(BossModule module) : Components.StackTogether(module, (uint)IconID.StackIcon, 5, 6) {
    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        foreach (var target in Targets) {
            Arena.ZoneCircleOutline(target.Position, Radius, Colors.Safe);
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP,
    StatesType = typeof(PariOfPlentyStates),
    ConfigType = null, // replace null with typeof(PariOfPlentyConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = null, // replace null with typeof(AID) if applicable
    StatusIDType = null, // replace null with typeof(SID) if applicable
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.PariOfPlenty,
    Contributors = "",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.VariantCriterion,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1079u,
    NameID = 14274u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class PariOfPlenty(WorldState ws, Actor primary) : BossModule(ws, primary, new(-760f, -805f), new ArenaBoundsSquare(20f));