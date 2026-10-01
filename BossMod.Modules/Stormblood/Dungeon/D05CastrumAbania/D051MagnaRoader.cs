namespace BossMod.Stormblood.Dungeon.D05CastrumAbania.D051MagnaRoader;

public enum OID : uint
{
    Boss = 0x1AA9, // R3.2
    MarkXLIIIMiniCannon = 0x1AAC, // R2.0
    TwelfthLegionTriarius = 0x1AAB, // R0.5
    TwelfthLegionOptio = 0x1AAA, // R0.5
    Helper = 0x18D6
}

public enum AID : uint
{
    AutoAttack1 = 872, // Boss->player, no cast, single-target
    AutoAttack2 = 870, // TwelfthLegionOptio->player, no cast, single-target

    MagitekFireII = 7957, // Boss->location, 3.0s cast, range 5 circle
    MagitekFireIII = 7958, // Boss->self, 3.0s cast, range 40+R circle

    WildSpeedVisual = 8318, // Boss->location, 6.0s cast, range 40+R width 6 rect
    HaywireVisual = 7959, // Boss->location, no cast, width 6 rect charge
    HaywireTelegraph = 7960, // Helper->location, 6.5s cast, range 40+R width 6 rect
    WildSpeed = 8184, // Helper->location, no cast, range 40+R width 6 rect

    MagitekPulseVisual1 = 7961, // MarkXLIIIMiniCannon->location, 3.0s cast, single-target
    MagitekPulseVisual2 = 8325, // TwelfthLegionTriarius->self, 3.0s cast, single-target
    MagitekPulse = 8336, // Helper->location, 3.0s cast, range 6 circle

    Wheel = 7956 // Boss->player, no cast, single-target, tankbuster
}

public enum SID : uint
{
    Fetters = 1399
}

class MagitekFireII(BossModule module) : Components.StandardAOEs(module, AID.MagitekFireII, 5);
class MagitekFireIII(BossModule module) : Components.RaidwideCast(module, AID.MagitekFireIII);
class MagitekPulse(BossModule module) : Components.StandardAOEs(module, AID.MagitekPulse, 6);
class Haywire(BossModule module) : Components.StandardAOEs(module, AID.HaywireTelegraph, new AOEShapeRect(43.2f, 3)); // TODO: confirm WildSpeedVisual vs HaywireTelegraph
class LegionAdds(BossModule module) : Components.AddsMulti(module, [OID.MarkXLIIIMiniCannon, OID.TwelfthLegionTriarius, OID.TwelfthLegionOptio], 1);

class D051MagnaRoaderStates : StateMachineBuilder
{
    public D051MagnaRoaderStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MagitekFireII>()
            .ActivateOnEnter<MagitekFireIII>()
            .ActivateOnEnter<MagitekPulse>()
            .ActivateOnEnter<Haywire>()
            .ActivateOnEnter<LegionAdds>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 242, NameID = 6263)] // TODO: clear after Haywire telegraph verify
public class D051MagnaRoader(ModuleInit init) : BossModule(init, init.Primary.Position, new ArenaBoundsCircle(20));
