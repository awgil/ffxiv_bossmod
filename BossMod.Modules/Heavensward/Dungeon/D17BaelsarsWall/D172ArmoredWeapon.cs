namespace BossMod.Heavensward.Dungeon.D17BaelsarsWall.D172ArmoredWeapon;

public enum OID : uint
{
    Boss = 0x193A, // R5.400, x1
    MagitekSlasher = 0x193C, // R1.05
    MagitekBit = 0x193B, // R0.9
    Helper2 = 0x19A,
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 7351, // Boss->player, no cast, single-target
    AutoAttack2 = 870, // MagitekSlasher->player, no cast, single-target
    Teleport = 7359, // MagitekBit->location, no cast, ???

    MagitekCannon = 7352, // Boss->player, no cast, single-target
    Launcher = 7356, // Boss->self, 3.0s cast, range 80+R circle, raidwide
    DynamicSensoryJammer = 7353, // Boss->self, 3.0s cast, range 80+R circle, applies extreme caution
    DynamicSensoryJammerFail = 7354, // Helper2->player, no cast, single-target, extreme caution fail

    DiffractiveLaserVisual = 31352, // Helper->self, 2.0s cast, range 5 circle
    DiffractiveLaser1 = 31469, // Boss->location, 4.0s cast, range 5 circle
    DiffractiveLaser2 = 7355, // Boss->location, 4.0s cast, range 5 circle
    DistressBeacon = 7358, // Boss->self, 3.0s cast, single-target

    MagitekBit = 7357, // Boss->self, 3.0s cast, single-target
    AssaultCannon = 7360 // MagitekBit->self, 4.0s cast, range 40+R width 2 rect
}

public enum SID : uint
{
    ExtremeCaution = 1132
}

class Launcher(BossModule module) : Components.RaidwideCast(module, AID.Launcher);
class DynamicSensoryJammer(BossModule module) : Components.CastHint(module, AID.DynamicSensoryJammer, "Stop moving!", true); // TODO: Extreme Caution / StayMove once status timing is confirmed
class DiffractiveLaserVisual(BossModule module) : Components.StandardAOEs(module, AID.DiffractiveLaserVisual, 5);
class DiffractiveLaser1(BossModule module) : Components.StandardAOEs(module, AID.DiffractiveLaser1, 5);
class DiffractiveLaser2(BossModule module) : Components.StandardAOEs(module, AID.DiffractiveLaser2, 5);
class AssaultCannon(BossModule module) : Components.StandardAOEs(module, AID.AssaultCannon, new AOEShapeRect(40.9f, 1));
class MagitekAdds(BossModule module) : Components.AddsMulti(module, [OID.MagitekBit, OID.MagitekSlasher], 1);

class D172ArmoredWeaponStates : StateMachineBuilder
{
    public D172ArmoredWeaponStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Launcher>()
            .ActivateOnEnter<DynamicSensoryJammer>()
            .ActivateOnEnter<DiffractiveLaserVisual>()
            .ActivateOnEnter<DiffractiveLaser1>()
            .ActivateOnEnter<DiffractiveLaser2>()
            .ActivateOnEnter<AssaultCannon>()
            .ActivateOnEnter<MagitekAdds>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 219, NameID = 5562)] // TODO: clear after Extreme Caution StayMove
public class D172ArmoredWeapon(ModuleInit init) : BossModule(init, new(116, 0), new ArenaBoundsSquare(19.5f));
