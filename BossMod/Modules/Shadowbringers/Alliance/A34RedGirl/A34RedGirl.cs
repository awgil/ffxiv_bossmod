
namespace BossMod.Shadowbringers.Alliance.A34RedGirl;

public enum OID : uint
{
    _Gen_ = 0x32E6, // R0.500, x16
    Boss = 0x32BB, // R7.500, x1
    Helper = 0x233C, // R0.500, x16 (spawn during fight), Helper type
    _Gen_RedGirl = 0x32BE, // R12.250, x3
    _Gen_WhiteLance = 0x32E3, // R1.000, x0 (spawn during fight)
    _Gen_BlackLance = 0x32E4, // R1.000, x0 (spawn during fight)
    _Gen_RedGirl1 = 0x32BC, // R2.250, x0 (spawn during fight)
    BossP2 = 0x32BD, // R12.250, x0 (spawn during fight)
    _Gen_BlackWall = 0x32EC, // R1.000, x0 (spawn during fight)
    RedSphereHelper = 0x32EA, // R0.500, x0 (spawn during fight)
    _Gen_WhiteWall = 0x32EB, // R1.000, x0 (spawn during fight)
    RedSphere = 0x32E9, // R4.000, x0 (spawn during fight)
    _Gen_BlackPylon = 0x32E8, // R1.500, x0 (spawn during fight)
    _Gen_WhitePylon = 0x32E7, // R1.500, x0 (spawn during fight)
    _Gen_BlackPylon1 = 0x32E5, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    _Weaponskill_Attack = 24597, // Helper->player, no cast, single-target
    _Weaponskill_Cruelty = 24594, // Boss->self, 5.0s cast, single-target
    _Weaponskill_Cruelty1 = 24596, // Helper->location, no cast, range 75 circle
    _Weaponskill_Shockwave = 24590, // Boss/BossP2->self, 2.0s cast, single-target
    _Weaponskill_GenerateBarrier = 24585, // Helper->self, 4.0s cast, range 24 width 3 rect
    _Weaponskill_GenerateBarrier1 = 24580, // Boss->self, 4.0s cast, single-target
    _Weaponskill_GenerateBarrier2 = 25363, // Helper->self, no cast, range 24 width 3 rect
    _Weaponskill_ShockWhite = 24591, // Helper->players, no cast, range 5 circle
    _Weaponskill_ShockWhite1 = 24592, // Helper->location, 4.0s cast, range 5 circle
    _Weaponskill_PointWhite = 24609, // _Gen_WhiteLance->self, no cast, range 24 width 6 rect
    _Weaponskill_PointWhite1 = 24607, // _Gen_WhiteLance->self, no cast, range 50 width 6 rect
    _Weaponskill_ShockBlack = 24972, // Helper->location, 4.0s cast, range 5 circle
    _Weaponskill_ShockBlack1 = 24593, // Helper->players, no cast, range 5 circle
    _Weaponskill_PointBlack = 24608, // _Gen_BlackLance->self, no cast, range 50 width 6 rect
    _Weaponskill_PointBlack1 = 24610, // _Gen_BlackLance->self, no cast, range 24 width 6 rect
    _Weaponskill_Vortex = 24599, // Helper->location, no cast, ???
    _Weaponskill_GenerateBarrier3 = 24584, // Helper->self, 4.0s cast, range 18 width 3 rect
    _Weaponskill_GenerateBarrier4 = 25362, // Helper->self, no cast, range 18 width 3 rect
    _Weaponskill_RecreateMeteor = 24903, // Boss->self, 2.0s cast, single-target
    _Weaponskill_WipeWhite = 24588, // Helper->self, 13.0s cast, range 75 circle
    _Weaponskill_ManipulateEnergy = 24600, // Boss->self, 4.0s cast, single-target
    _Weaponskill_ManipulateEnergy1 = 24602, // Helper->player, no cast, range 3 circle
    _Weaponskill_Replicate = 24586, // Boss->self, 3.0s cast, single-target
    _Weaponskill_DiffuseEnergy = 24611, // _Gen_RedGirl1->self, 5.0s cast, range 12 120-degree cone
    _Weaponskill_DiffuseEnergy1 = 24662, // _Gen_RedGirl1->self, no cast, range 12 120-degree cone
    _Weaponskill_SublimeTranscendence = 25098, // Boss->self, 5.0s cast, single-target
    _Weaponskill_SublimeTranscendence1 = 25099, // Helper->location, no cast, range 75 circle
    _Weaponskill_ = 24605, // Helper->location, no cast, single-target
    _Weaponskill_WaveWhite = 24973, // _Gen_RedSphere->self, 8.0s cast, range 22 circle
    _Weaponskill_WaveBlack = 24974, // _Gen_RedSphere->self, 8.0s cast, range 22 circle
    _Weaponskill_1 = 24606, // RedSphere->location, no cast, single-target
    _Weaponskill_Cruelty2 = 24595, // BossP2->self, 5.0s cast, single-target
    _Weaponskill_ChildsPlayNorth = 24612, // BossP2->self, 10.0s cast, single-target
    _Weaponskill_ChildsPlayEast = 24613, // BossP2->self, 10.0s cast, single-target
    _Weaponskill_Explosion = 24614, // _Gen_BlackPylon1->self, 15.0s cast, range 9 circle
    _Weaponskill_GenerateBarrier5 = 24582, // Helper->self, 4.0s cast, range 6 width 3 rect
    _Weaponskill_GenerateBarrier6 = 24583, // Helper->self, 4.0s cast, range 12 width 3 rect
    _Weaponskill_GenerateBarrier7 = 24581, // BossP2->self, 4.0s cast, single-target
    _Weaponskill_GenerateBarrier8 = 25360, // Helper->self, no cast, range 6 width 3 rect
    _Weaponskill_GenerateBarrier9 = 25361, // Helper->self, no cast, range 12 width 3 rect
    _Weaponskill_WipeBlack = 24589, // Helper->self, 13.0s cast, range 75 circle
    _Weaponskill_BigExplosion = 24615, // _Gen_BlackPylon/_Gen_WhitePylon->self, 6.0s cast, range 50 circle
}

public enum SID : uint
{
    ProgramFFFFFFF = 2632, // none->player, extra=0x1AB
    Program000000 = 2633, // none->player, extra=0x1AC
    _Gen_ = 2160, // none->31A8, extra=0x1C94
    _Gen_PayingThePiper = 1681, // none->player, extra=0x2
}

public enum IconID : uint
{
    ShockWhiteSlow = 262, // player->self
    ShockBlackSlow = 263, // player->self
    ShockWhiteFast = 264, // player->self
    Tankbuster = 218, // player->self
    TurnLeft = 168, // _Gen_RedGirl1->self
    TurnRight = 167, // _Gen_RedGirl1->self
}

public enum TetherID : uint
{
    Chain = 149, // player->BossP2
}

public enum Shade
{
    None,
    Black,
    White
}

class CrueltyP1(BossModule module) : Components.RaidwideCastDelay(module, AID._Weaponskill_Cruelty, AID._Weaponskill_Cruelty1, 0.1f);
class CrueltyP2(BossModule module) : Components.RaidwideCastDelay(module, AID._Weaponskill_Cruelty2, AID._Weaponskill_Cruelty1, 0.1f);
class ShockGround(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_ShockWhite1, AID._Weaponskill_ShockBlack], new AOEShapeCircle(5));
class ShockWhiteFast(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.ShockWhiteFast, AID._Weaponskill_ShockWhite, 5, 5.1f);
class ManipulateEnergy(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(3), (uint)IconID.Tankbuster, AID._Weaponskill_ManipulateEnergy1, 5.2f, centerAtTarget: true, damageType: AIHints.PredictedDamageType.Tankbuster);

class ShockWhiteSlow(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.ShockWhiteSlow, AID._Weaponskill_ShockWhite, 5, 10.1f);
class ShockBlackSlow(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.ShockBlackSlow, AID._Weaponskill_ShockBlack1, 5, 10.1f);

class A34RedGirlStates : StateMachineBuilder
{
    public A34RedGirlStates(BossModule module) : base(module)
    {
        // TODO: figure out how to disambiguate wipes
        SimplePhase(0, Phase1, "P1").Raw.Update = () => module.Enemies(OID.RedSphere).Any();
        SimplePhase(1, Phase2, "Hacking").Raw.Update = () => module.Enemies(OID.BossP2).Any();
        SimplePhase(2, Phase3, "P2").Raw.Update = () => module.Enemies(OID.BossP2).All(b => b.IsDeadOrDestroyed);
    }

    private void Phase1(uint id)
    {
        Timeout(id, 9999, "P1 enrage")
            .ActivateOnEnter<CrueltyP1>()
            .ActivateOnEnter<GenerateBarrier1>()
            .ActivateOnEnter<GenerateBarrier2>()
            .ActivateOnEnter<GenerateBarrier3>()
            .ActivateOnEnter<GenerateBarrier4>()
            .ActivateOnEnter<BarrierVoidzone>()
            .ActivateOnEnter<Barrier>()
            .ActivateOnEnter<ShockGround>()
            .ActivateOnEnter<ShockWhiteSlow>()
            .ActivateOnEnter<ShockWhiteFast>()
            .ActivateOnEnter<ShockBlackSlow>()
            .ActivateOnEnter<Point>()
            .ActivateOnEnter<Wipe>()
            .ActivateOnEnter<DiffuseEnergy>()
            .ActivateOnEnter<ManipulateEnergy>();
    }

    private void Phase2(uint id)
    {
        Timeout(id, 9999, "P2 enrage")
            .ActivateOnEnter<Hacking>()
            .ActivateOnEnter<HackingWalls>()
            .ActivateOnEnter<HackingPylons>()
            .ActivateOnEnter<HackModule>()
            .ActivateOnEnter<RedSphere>();
    }

    private void Phase3(uint id)
    {
        Timeout(id, 9999, "P3 enrage")
            .ActivateOnEnter<GenerateBarrier1>()
            .ActivateOnEnter<GenerateBarrier2>()
            .ActivateOnEnter<GenerateBarrier3>()
            .ActivateOnEnter<GenerateBarrier4>()
            .ActivateOnEnter<BarrierVoidzone>()
            .ActivateOnEnter<Barrier>()
            .ActivateOnEnter<ChildsPlay>()
            .ActivateOnEnter<ShockWhiteSlow>()
            .ActivateOnEnter<ShockWhiteFast>()
            .ActivateOnEnter<ShockBlackSlow>()
            .ActivateOnEnter<PylonExplosion>()
            .ActivateOnEnter<Point>()
            .ActivateOnEnter<Wipe>()
            .ActivateOnEnter<CrueltyP2>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 779, NameID = 9950)]
public class A34RedGirl(WorldState ws, Actor primary) : BossModule(ws, primary, new(845, -851), new ArenaBoundsSquare(24.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        // avoid janky clip
        foreach (var b in Enemies(OID.BossP2))
            Arena.ActorInsideBounds(b.Position, b.Rotation, ArenaColor.Enemy);
    }
}

[ConfigDisplay(Parent = typeof(ShadowbringersConfig), Name = "The Tower at Paradigm's Breach - Red Girl")]
public class A34RedGirlConfig : ConfigNode
{
    [PropertyDisplay("Automatically attack closest enemies during hacking minigame")]
    public bool AutoHack = true;
}
