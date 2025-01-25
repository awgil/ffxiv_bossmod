namespace BossMod.Endwalker.Dungeon.D10Troia.D102Beatrice;

public enum OID : uint
{
    Boss = 0x396D,
    Helper = 0x233C,
}

public enum AID : uint
{
    _AutoAttack_Attack = 872, // Boss->player, no cast, single-target
    _Ability_ = 29820, // Boss->location, no cast, single-target
    _Weaponskill_EyeOfTroia = 29818, // Boss->self, 4.0s cast, range 40 circle
    _Weaponskill_DeathForeseen = 29821, // Helper->self, 5.0s cast, range 40 circle
    _Ability_BeatificScorn = 29811, // Boss->self, 4.0s cast, single-target
    _Ability_BeatificScorn1 = 29816, // Boss->self, no cast, single-target
    _Ability_BeatificScorn2 = 29814, // Boss->self, no cast, single-target
    _Ability_BeatificScorn3 = 29815, // Boss->self, no cast, single-target
    _Weaponskill_BeatificScorn = 29817, // Helper->self, 10.0s cast, range 9 circle
    _Ability_1 = 29819, // Boss->self, no cast, single-target
    _Weaponskill_Hush = 29824, // Boss->player, 5.0s cast, single-target
    _Weaponskill_Voidshaker = 29822, // Boss->self, 5.0s cast, range 20 120-degree cone
    _Weaponskill_VoidNail = 29823, // Helper->player, 5.0s cast, range 6 circle
    _Weaponskill_DeathForeseen1 = 29828, // Helper->self, 8.0s cast, range 40 circle
    _Weaponskill_ToricVoid = 29829, // Boss->self, 4.0s cast, single-target
    _Weaponskill_ToricVoid1 = 31207, // Helper->self, 4.0s cast, range 10-20 donut
    _Weaponskill_ToricVoid2 = 31206, // Boss->self, no cast, single-target
    _Ability_BeatificScorn4 = 29813, // Boss->self, 4.0s cast, single-target
    _Weaponskill_Antipressure = 31208, // Helper->players, 7.0s cast, range 6 circle
    _Ability_BeatificScorn5 = 29812, // Boss->self, no cast, single-target
}

class EyeOfTroia(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Weaponskill_EyeOfTroia));
class DeathForeseen(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID._Weaponskill_DeathForeseen));
class DeathForeseen1(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID._Weaponskill_DeathForeseen1))
{
    public override IEnumerable<Eye> ActiveEyes(int slot, Actor actor) => base.ActiveEyes(slot, actor).Take(2);
}
class BeatificScorn(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_BeatificScorn), new AOEShapeCircle(9), maxCasts: 5);
class Hush(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID._Weaponskill_Hush));
class Voidshaker(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Voidshaker), new AOEShapeCone(20, 60.Degrees()));
class VoidNail(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID._Weaponskill_VoidNail), 6);
class ToricVoid(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_ToricVoid1), new AOEShapeDonut(10, 20));
class Antipressure(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID._Weaponskill_Antipressure), 6);

class BeatriceStates : StateMachineBuilder
{
    public BeatriceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<EyeOfTroia>()
            .ActivateOnEnter<DeathForeseen>()
            .ActivateOnEnter<DeathForeseen1>()
            .ActivateOnEnter<BeatificScorn>()
            .ActivateOnEnter<Hush>()
            .ActivateOnEnter<Voidshaker>()
            .ActivateOnEnter<VoidNail>()
            .ActivateOnEnter<ToricVoid>()
            .ActivateOnEnter<Antipressure>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 869, NameID = 11384)]
public class Beatrice(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -148), new ArenaBoundsCircle(20));

