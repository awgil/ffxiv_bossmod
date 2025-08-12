namespace BossMod.RealmReborn.Dungeon.D26Snowcloak.D262Yeti;

public enum OID : uint
{
    Boss = 0x3977, // R3.800, x?
    Snowball1 = 0x233C, // R0.500, x?, Helper type
    Snowball2 = 0x3978, // R1.000-4.000, x?
}
public enum AID : uint
{
    Attack = 872, // D20/D1B/D1D/D23/D19/D24/D07/D1F/3977->player, no cast, single-target
    Buffet = 29585, // 3977->self, 3.0s cast, range 12 120-degree cone
    Northerlies = 29582, // 3977->self, 5.0s cast, range 80 circle
    FrozenMist = 29583, // 3977->self, no cast, single-target
    Updrift = 29584, // 3977->self, 4.0s cast, range 80 circle
    A = 29588, // 3978->self, no cast, single-target
    B = 29587, // 3978->self, no cast, single-targe
    Spin = 29586, // 3977->self, 5.0s cast, range 11 circle
    FrozenSpike = 25583, // 3977->self, 4.5+0.5s cast, single-target
    FrozenSpike1 = 29592, // 233C->player, 5.0s cast, range 6 circle
    FrozenCircle = 29591, // 233C->self, 5.0s cast, range ?-40 donut
    HeavySnow = 29589, // 233C->self, 6.5s cast, range 15 circle
    LightSnow = 29590, // 233C->self, 7.0s cast, range 2 circle
}
public enum IconID : uint
{
    IceSpike = 67, // player->self
}

class Buffet(BossModule module) : Components.StandardAOEs(module, AID.Buffet, new AOEShapeCone(12, 60.Degrees()));
class Northerlies(BossModule module) : Components.RaidwideCast(module, AID.Northerlies);
class Spin(BossModule module) : Components.StandardAOEs(module, AID.Spin, new AOEShapeCircle(11));
class FrozenSpike(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.IceSpike, AID.FrozenSpike1, 6, 1, drawAllSpreads: true);
class FrozenCircle(BossModule module) : Components.StandardAOEs(module, AID.FrozenCircle, new AOEShapeDonut(10, 40));
class HeavySnow(BossModule module) : Components.StandardAOEs(module, AID.HeavySnow, new AOEShapeCircle(15));
class LightSnow(BossModule module) : Components.StandardAOEs(module, AID.LightSnow, new AOEShapeCircle(2));
class D262YetiStates : StateMachineBuilder
{
    public D262YetiStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Buffet>()
            .ActivateOnEnter<Northerlies>()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<FrozenSpike>()
            .ActivateOnEnter<FrozenCircle>()
            .ActivateOnEnter<HeavySnow>()
            .ActivateOnEnter<LightSnow>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 27, NameID = 3040)]
public class D262Yeti(WorldState ws, Actor primary) : BossModule(ws, primary, new(-98.1f, -115.6f), new ArenaBoundsCircle(19));
