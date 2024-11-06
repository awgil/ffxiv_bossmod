namespace BossMod.Heavensward.Dungeon.D15Xelphatol.D153TozolHuatotl;

public enum OID : uint
{
    _Gen_Garuda = 0x17A4, // R2.890, x1
    _Gen_AbalathianHornbill = 0x17A3, // R1.080, x4
    _Gen_AbalathianHornbill1 = 0xD25, // R0.500, x2
    Boss = 0x17A2, // R3.000, x1
}

public enum AID : uint
{
    _AutoAttack_Attack = 872, // Boss->player, no cast, single-target
    _Spell_IxaliAero = 6611, // Boss->player, no cast, single-target
    _Spell_IxaliAeroIII = 6613, // Boss->self, 3.0s cast, range 50+R circle
    _Ability_Hawk = 6614, // Boss->self, 5.0s cast, single-target
    _Weaponskill_Bill = 6618, // _Gen_AbalathianHornbill->player, 5.0s cast, range 5 circle
    _Spell_IxaliAeroII = 6612, // Boss->self, 3.0s cast, range 40+R width 6 rect
    _Weaponskill_Ingurgitate = 6616, // _Gen_AbalathianHornbill->player, 5.0s cast, single-target
    _Weaponskill_Ingurgitate1 = 6617, // _Gen_AbalathianHornbill1->self, no cast, range 5 circle
    _Ability_SummonGaruda = 6615, // Boss->location, 4.0s cast, single-target
    _Weaponskill_EyeOfTheStorm = 6619, // _Gen_AbalathianHornbill1->self, 6.0s cast, range 20 circle
    _Weaponskill_MistralSong = 6620, // _Gen_Garuda->self, 5.0s cast, range 30+R 120-degree cone
    _Weaponskill_WickedWheel = 6621, // _Gen_Garuda->self, 6.0s cast, range 7 circle
    _Weaponskill_AerialBlast = 6622, // _Gen_Garuda->self, 4.0s cast, range 50+R circle
}

public enum IconID : uint
{
    _Gen_Icon_70 = 70, // player
    _Gen_Icon_62 = 62, // player
}

class IxaliAeroII(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_IxaliAeroII), new AOEShapeRect(40, 3));
class IxaliAeroIII(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Spell_IxaliAeroIII));
class MistralSong(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_MistralSong), new AOEShapeCone(30, 60.Degrees()));
class WickedWheel(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_WickedWheel), new AOEShapeCircle(7));
class AerialBlast(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Weaponskill_AerialBlast));
class EyeOfTheStorm(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_EyeOfTheStorm), new AOEShapeDonut(10, 20));
class Bill(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID._Weaponskill_Bill), 5);
class Ingurgitate(BossModule module) : Components.StackWithIcon(module, 62, ActionID.MakeSpell(AID._Weaponskill_Ingurgitate), 5, 5.4f);

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

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 182, NameID = 5272)]
public class TozolHuatotl(WorldState ws, Actor primary) : BossModule(ws, primary, DefaultCenter, new ArenaBoundsCircle(20))
{
    // position of Garuda casting Eye of the Storm
    public static readonly WPos DefaultCenter = new(317.948f, -416.172f);
}

