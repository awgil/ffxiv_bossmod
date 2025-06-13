namespace BossMod.Shadowbringers.Alliance.A34RedGirl;

[ConfigDisplay(Parent = typeof(ShadowbringersConfig), Name = "The Tower at Paradigm's Breach - Red Girl")]
public class A34RedGirlConfig : ConfigNode
{
    [PropertyDisplay("Automatically attack closest enemies during hacking minigame")]
    public bool AutoHack = true;
}

public enum OID : uint
{
    Boss = 0x32BB, // R7.500, x1
    Helper = 0x233C, // R0.500, x16 (spawn during fight), Helper type
    Unk1 = 0x32E6, // R0.500, x16
    RedGirl = 0x32BE, // R12.250, x3
    RedGirlCaster = 0x32BC, // R2.250, x0 (spawn during fight)
    WhiteLance = 0x32E3, // R1.000, x0 (spawn during fight)
    BlackLance = 0x32E4, // R1.000, x0 (spawn during fight)

    WhitePylon = 0x32E7, // R1.500, x0 (spawn during fight)
    BlackPylon = 0x32E8, // R1.500, x0 (spawn during fight)
    RedSphere = 0x32E9, // R4.000, x0 (spawn during fight)
    RedSphereHelper = 0x32EA, // R0.500, x0 (spawn during fight)
    WhiteWall = 0x32EB, // R1.000, x0 (spawn during fight)
    BlackWall = 0x32EC, // R1.000, x0 (spawn during fight)

    BossP2 = 0x32BD, // R12.250, x0 (spawn during fight)
    BlackPylonP2 = 0x32E5, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 24597, // Helper->player, no cast, single-target
    CrueltyCast = 24594, // Boss->self, 5.0s cast, single-target
    Cruelty = 24596, // Helper->location, no cast, range 75 circle
    Shockwave = 24590, // Boss/BossP2->self, 2.0s cast, single-target
    GenerateBarrierCast = 24580, // Boss->self, 4.0s cast, single-target
    GenerateBarrierLong = 24585, // Helper->self, 4.0s cast, range 24 width 3 rect
    GenerateBarrierLongInstant = 25363, // Helper->self, no cast, range 24 width 3 rect
    ShockWhiteSpread = 24591, // Helper->players, no cast, range 5 circle
    ShockWhiteGround = 24592, // Helper->location, 4.0s cast, range 5 circle
    PointWhiteLong = 24607, // WhiteLance->self, no cast, range 50 width 6 rect
    PointBlackLong = 24608, // BlackLance->self, no cast, range 50 width 6 rect
    PointWhiteShort = 24609, // WhiteLance->self, no cast, range 24 width 6 rect
    PointBlackShort = 24610, // BlackLance->self, no cast, range 24 width 6 rect
    ShockBlackGround = 24972, // Helper->location, 4.0s cast, range 5 circle
    ShockBlackSpread = 24593, // Helper->players, no cast, range 5 circle
    Vortex = 24599, // Helper->location, no cast, ???
    GenerateBarrierMidLong = 24584, // Helper->self, 4.0s cast, range 18 width 3 rect
    GenerateBarrierMidLongInstant = 25362, // Helper->self, no cast, range 18 width 3 rect
    RecreateMeteor = 24903, // Boss->self, 2.0s cast, single-target
    WipeWhite = 24588, // Helper->self, 13.0s cast, range 75 circle
    WipeBlack = 24589, // Helper->self, 13.0s cast, range 75 circle
    ManipulateEnergyCast = 24600, // Boss->self, 4.0s cast, single-target
    ManipulateEnergy = 24602, // Helper->player, no cast, range 3 circle
    Replicate = 24586, // Boss->self, 3.0s cast, single-target
    DiffuseEnergy = 24611, // RedGirl1->self, 5.0s cast, range 12 120-degree cone
    DiffuseEnergyRepeat = 24662, // RedGirl1->self, no cast, range 12 120-degree cone
    SublimeTranscendenceCast = 25098, // Boss->self, 5.0s cast, single-target
    SublimeTranscendence = 25099, // Helper->location, no cast, range 75 circle

    WaveWhite = 24973, // RedSphere->self, 8.0s cast, range 22 circle
    WaveBlack = 24974, // RedSphere->self, 8.0s cast, range 22 circle
    BigExplosion = 24615, // BlackPylon/WhitePylon->self, 6.0s cast, range 50 circle

    CrueltyP2 = 24595, // BossP2->self, 5.0s cast, single-target
    ChildsPlayNorth = 24612, // BossP2->self, 10.0s cast, single-target
    ChildsPlayEast = 24613, // BossP2->self, 10.0s cast, single-target
    Explosion = 24614, // BlackPylon1->self, 15.0s cast, range 9 circle
    GenerateBarrierShort = 24582, // Helper->self, 4.0s cast, range 6 width 3 rect
    GenerateBarrierMidShort = 24583, // Helper->self, 4.0s cast, range 12 width 3 rect
    GenerateBarrierP2 = 24581, // BossP2->self, 4.0s cast, single-target
    GenerateBarrierShortInstant = 25360, // Helper->self, no cast, range 6 width 3 rect
    GenerateBarrierMidShortINstant = 25361, // Helper->self, no cast, range 12 width 3 rect

    Unk1 = 24605, // Helper->location, no cast, single-target
    Unk2 = 24606, // RedSphere->location, no cast, single-target
}

public enum SID : uint
{
    ProgramFFFFFFF = 2632, // none->player, extra=0x1AB
    Program000000 = 2633, // none->player, extra=0x1AC
    PayingThePiper = 1681, // none->player, extra=0x2, forced march
}

public enum IconID : uint
{
    ShockWhiteSlow = 262, // player->self
    ShockBlackSlow = 263, // player->self
    ShockWhiteFast = 264, // player->self
    Tankbuster = 218, // player->self
    TurnLeft = 168, // RedGirl1->self
    TurnRight = 167, // RedGirl1->self
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

class CrueltyP1(BossModule module) : Components.RaidwideCastDelay(module, AID.CrueltyCast, AID.Cruelty, 0.1f);
class CrueltyP2(BossModule module) : Components.RaidwideCastDelay(module, AID.CrueltyP2, AID.Cruelty, 0.1f);
class ShockGround(BossModule module) : Components.GroupedAOEs(module, [AID.ShockWhiteGround, AID.ShockBlackGround], new AOEShapeCircle(5));
class ShockWhiteFast(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.ShockWhiteFast, AID.ShockWhiteSpread, 5, 5.1f);
class ManipulateEnergy(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(3), (uint)IconID.Tankbuster, AID.ManipulateEnergy, 5.2f, centerAtTarget: true, damageType: AIHints.PredictedDamageType.Tankbuster);

class ShockWhiteSlow(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.ShockWhiteSlow, AID.ShockWhiteSpread, 5, 10.1f)
{
    public override void Update()
    {
        Spreads.RemoveAll(s => s.Target.IsDead);
    }
}
class ShockBlackSlow(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.ShockBlackSlow, AID.ShockBlackSpread, 5, 10.1f)
{
    public override void Update()
    {
        Spreads.RemoveAll(s => s.Target.IsDead);
    }
}

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

        var northZ = Arena.Center.Z - Arena.Bounds.Radius;

        // avoid janky clip
        foreach (var b in Enemies(OID.BossP2))
            Arena.ActorOutsideBounds(new(b.Position.X, MathF.Max(northZ, b.Position.Z)), b.Rotation, ArenaColor.Enemy);
    }
}
