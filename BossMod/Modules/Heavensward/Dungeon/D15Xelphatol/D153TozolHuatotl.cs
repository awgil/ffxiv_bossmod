namespace BossMod.Heavensward.Dungeon.D15Xelphatol.D153TozolHuatotl;

public enum OID : uint
{
    Garuda = 0x17A4, // R2.890, x1
    AbalathianHornbill1 = 0x17A3, // R1.080, x4
    AbalathianHornbill2 = 0xD25, // R0.500, x2
    Boss = 0x17A2, // R3.000, x1
}

public enum AID : uint
{
    IxaliAero = 6611, // Boss->player, no cast, single-target
    IxaliAeroII = 6612, // Boss->self, 3.0s cast, range 40+R width 6 rect
    IxaliAeroIII = 6613, // Boss->self, 3.0s cast, range 50+R circle
    Hawk = 6614, // Boss->self, 5.0s cast, single-target
    Bill = 6618, // _Gen_AbalathianHornbill->player, 5.0s cast, range 5 circle
    IngurgitateCast = 6616, // _Gen_AbalathianHornbill->player, 5.0s cast, single-target
    IngurgitateDamage = 6617, // _Gen_AbalathianHornbill1->self, no cast, range 5 circle
    SummonGaruda = 6615, // Boss->location, 4.0s cast, single-target
    EyeOfTheStorm = 6619, // _Gen_AbalathianHornbill1->self, 6.0s cast, range 20 circle
    MistralSong = 6620, // _Gen_Garuda->self, 5.0s cast, range 30+R 120-degree cone
    WickedWheel = 6621, // _Gen_Garuda->self, 6.0s cast, range 7 circle
    AerialBlast = 6622, // _Gen_Garuda->self, 4.0s cast, range 50+R circle
}

public enum IconID : uint
{
    Bill = 70, // player
    Ingurgitate = 62, // player
}

class IxaliAeroII(BossModule module) : Components.StandardAOEs(module, AID.IxaliAeroII, new AOEShapeRect(40, 3));
class IxaliAeroIII(BossModule module) : Components.RaidwideCast(module, AID.IxaliAeroIII);
class MistralSong(BossModule module) : Components.StandardAOEs(module, AID.MistralSong, new AOEShapeCone(30, 60.Degrees()));
class WickedWheel(BossModule module) : Components.StandardAOEs(module, AID.WickedWheel, new AOEShapeCircle(7));
class AerialBlast(BossModule module) : Components.RaidwideCast(module, AID.AerialBlast);
class EyeOfTheStorm(BossModule module) : Components.StandardAOEs(module, AID.EyeOfTheStorm, new AOEShapeDonut(10, 20));
class Bill(BossModule module) : Components.SpreadFromCastTargets(module, AID.Bill, 5);
class Ingurgitate(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Ingurgitate, AID.IngurgitateCast, 5, 5.4f);

class TozolHuatotlStates : StateMachineBuilder
{
    public TozolHuatotlStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<EyeOfTheStorm>()
            .ActivateOnEnter<MistralSong>()
            .ActivateOnEnter<WickedWheel>()
            .ActivateOnEnter<AerialBlast>()
            .ActivateOnEnter<Bill>()
            .ActivateOnEnter<IxaliAeroIII>()
            .ActivateOnEnter<IxaliAeroII>()
            .ActivateOnEnter<Ingurgitate>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 182, NameID = 5272, Contributors = "xan")]
public class TozolHuatotl(WorldState ws, Actor primary) : BossModule(ws, primary, DefaultCenter, new ArenaBoundsCircle(20))
{
    // position of Garuda casting Eye of the Storm
    public static readonly WPos DefaultCenter = new(317.948f, -416.172f);
}
