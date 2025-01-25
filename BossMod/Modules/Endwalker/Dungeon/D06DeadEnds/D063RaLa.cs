namespace BossMod.Endwalker.Dungeon.D06DeadEnds.D063RaLa;
public enum OID : uint
{
    Boss = 0x34C7,
    Helper = 0x233C,
}

public enum AID : uint
{
    _AutoAttack_Attack = 870, // Boss->player, no cast, single-target
    _Weaponskill_WarmGlow = 25950, // Boss->self, 5.0s cast, range 40 circle
    _Weaponskill_Pity = 25949, // Boss->player, 5.0s cast, single-target
    _Ability_Prance = 25937, // Boss->location, 5.0s cast, single-target
    _Ability_Prance1 = 25938, // Boss->location, no cast, single-target
    _Ability_LamellarLight = 25939, // Helper->self, 6.0s cast, range 15 circle
    _Ability_ = 25941, // Boss->location, no cast, single-target
    _Weaponskill_Lifesbreath = 25940, // Boss->self, 4.0s cast, range 50 width 10 rect
    _Ability_LamellarLight1 = 25942, // 34C8->self, 3.0s cast, single-target
    _Ability_LamellarLight2 = 25951, // Helper->self, 3.0s cast, range 40 width 4 rect
    _Ability_Benevolence = 25945, // Boss->self, 5.0s cast, single-target
    _Ability_Benevolence1 = 25946, // Helper->players, 5.4s cast, range 6 circle
    _Weaponskill_LovingEmbrace = 25943, // Boss->self, 7.0s cast, range 45 180-degree cone
    _Ability_StillEmbrace = 25947, // Boss->self, 5.0s cast, single-target
    _Ability_StillEmbrace1 = 25948, // Helper->player, 5.4s cast, range 6 circle
}

class WarmGlow(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Weaponskill_WarmGlow));
class Prance(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Ability_LamellarLight), new AOEShapeCircle(15), maxCasts: 3);
class Lifesbreath(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Lifesbreath), new AOEShapeRect(50, 5));
class LamellarLight(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Ability_LamellarLight2), new AOEShapeRect(40, 2));
class Benevolence(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID._Ability_Benevolence1), 6);
class StillEmbrace(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID._Ability_StillEmbrace1), 6);
class LovingEmbrace(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_LovingEmbrace), new AOEShapeCone(45, 90.Degrees()));
class Pity(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID._Weaponskill_Pity));

class RaLaStates : StateMachineBuilder
{
    public RaLaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WarmGlow>()
            .ActivateOnEnter<Prance>()
            .ActivateOnEnter<Lifesbreath>()
            .ActivateOnEnter<LamellarLight>()
            .ActivateOnEnter<Benevolence>()
            .ActivateOnEnter<StillEmbrace>()
            .ActivateOnEnter<LovingEmbrace>()
            .ActivateOnEnter<Pity>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 792, NameID = 10316)]
public class RaLa(WorldState ws, Actor primary) : BossModule(ws, primary, new(-380, -135), new ArenaBoundsCircle(20));

