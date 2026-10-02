namespace BossMod.Global.Crucible.IceDragonPiece;

public enum OID : uint
{
    Boss = 0x4CC0, // R7.000, x1
    Helper = 0x233C, // R0.500, x16, Helper type

    WitheringEternity = 0x1E972A
}

public enum AID : uint
{
    _AutoAttack_ = 50784, // Boss->player, no cast, single-target
    _Ability_ = 48695, // Boss->location, no cast, single-target
    _Weaponskill_IcyTorment = 48698, // Boss->self, 4.7+1.8s cast, single-target
    _Weaponskill_IcyTorment1 = 48699, // Helper->self, 6.5s cast, range 100 width 15 rect
    _Weaponskill_RimeWreath = 48709, // Boss->self, 5.0s cast, range 100 circle
    _Weaponskill_IcyTorment2 = 48696, // Boss->self, 5.0+1.5s cast, single-target
    _Weaponskill_IcyTorment3 = 48697, // Helper->self, 6.5s cast, range 100 width 15 rect
    _Weaponskill_WitheringEternity = 48707, // Boss->self, 3.0s cast, single-target
    _Weaponskill_WitheringEternity1 = 48708, // Helper->location, 5.0s cast, range 9 circle
    _Weaponskill_SheetOfIce = 48710, // Boss->location, 4.5s cast, single-target
    _Weaponskill_SheetOfIce1 = 48712, // Helper->location, 5.5s cast, range 5 circle
}

class IcyTorment(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_IcyTorment1, AID._Weaponskill_IcyTorment3], new AOEShapeRect(100, 7.5f));
class RimeWreath(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_RimeWreath, "Raidwide + frostbite");
class WitheringEternity(BossModule module) : Components.VoidzoneAtCastTarget(module, 9, AID._Weaponskill_WitheringEternity1, OID.WitheringEternity, 0.9f);
class SheetOfIce(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_SheetOfIce1, 5);

class IceDragonPieceStates : StateMachineBuilder
{
    public IceDragonPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<IcyTorment>()
            .ActivateOnEnter<RimeWreath>()
            .ActivateOnEnter<WitheringEternity>()
            .ActivateOnEnter<SheetOfIce>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1091, NameID = 14606)]
public class IceDragonPiece(ModuleInit init) : BossModule(init, new(120, 0), new ArenaBoundsSquare(20));

