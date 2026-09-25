#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Global.Crucible.ManticorePiece;

public enum OID : uint
{
    Boss = 0x4C53,
    Helper = 0x233C,
}

public enum AID : uint
{
    _AutoAttack_ = 49680, // Boss->player, no cast, single-target
    _Weaponskill_ArmAndHammer = 48124, // Boss->self, 5.0+0.6s cast, single-target
    _Weaponskill_ArmAndHammer1 = 48125, // Helper->self, 5.6s cast, range 30 ?-degree cone
    _Weaponskill_ArmAndHammer2 = 48122, // Boss->self, 5.0+0.6s cast, single-target
    _Weaponskill_ArmAndHammer3 = 48123, // Helper->self, 5.6s cast, range 30 ?-degree cone
    _Weaponskill_DeadlyHold = 48138, // Boss->player, 5.0s cast, single-target
    _Ability_ = 48126, // Boss->location, no cast, single-target
    _Weaponskill_Hammerleap = 48135, // Boss->location, 7.0+1.1s cast, single-target
    _Weaponskill_Hammerleap1 = 48137, // Helper->self, 8.1s cast, range 30 circle
    _Weaponskill_TailsAndHeads = 50410, // Boss->self, 3.5+0.4s cast, single-target
    _Weaponskill_TailsAndHeads1 = 50411, // Helper->self, 3.9s cast, range 40 180-degree cone
    _Weaponskill_TailsAndHeads2 = 50412, // Boss->self, 2.0+0.4s cast, single-target
    _Weaponskill_TailsAndHeads3 = 50413, // Helper->self, 2.4s cast, range 40 180-degree cone
    _Weaponskill_ChargeCast = 48128, // Helper->location, 1.5s cast, width 8 rect charge
    _Weaponskill_ChargeAndHammer = 48127, // Boss->self, 10.0s cast, single-target
    _Weaponskill_WildCharge = 48129, // Boss->location, no cast, single-target
    _Weaponskill_WildCharge1 = 48130, // Helper->location, 1.1s cast, width 8 rect charge
    _Weaponskill_ArmAndHammer4 = 48133, // Boss->self, no cast, single-target
    _Weaponskill_ArmAndHammer5 = 48134, // Helper->self, 0.6s cast, range 30 ?-degree cone
    _Weaponskill_ArmAndHammer6 = 48131, // Boss->self, no cast, single-target
    _Weaponskill_ArmAndHammer7 = 48132, // Helper->self, 0.6s cast, range 30 ?-degree cone
    _Weaponskill_HeadsAndTails = 48139, // Boss->self, 3.5+0.4s cast, single-target
    _Weaponskill_HeadsAndTails1 = 48140, // Helper->self, 3.9s cast, range 40 180-degree cone
    _Weaponskill_HeadsAndTails2 = 48141, // Boss->self, 2.0+0.4s cast, single-target
    _Weaponskill_HeadsAndTails3 = 48142, // Helper->self, 2.4s cast, range 40 180-degree cone

}

public enum SID : uint
{
    LeftArmGlow = 2193,
    RightArmGlow = 2056
}

class ArmAndHammer(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_ArmAndHammer1, AID._Weaponskill_ArmAndHammer3], new AOEShapeCone(30, 90.Degrees()));
class ArmAndHammer2(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_ArmAndHammer5, AID._Weaponskill_ArmAndHammer7], new AOEShapeCone(30, 90.Degrees()));
class DeadlyHold(BossModule module) : Components.SingleTargetCast(module, AID._Weaponskill_DeadlyHold);
class Hammerleap(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Hammerleap1, 30);
class TailsAndHeads(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_TailsAndHeads1, AID._Weaponskill_TailsAndHeads3], new AOEShapeCone(40, 90.Degrees()));
class HeadsAndTails(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_HeadsAndTails1, AID._Weaponskill_HeadsAndTails3], new AOEShapeCone(40, 90.Degrees()));

class ChargeAndHammer(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_ChargeAndHammer)
{
    private readonly List<(WPos From, WPos To, DateTime Activate)> _aoes = [];
    private readonly List<(WDir Dir, DateTime Activate)> _slams = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var i = 0;
        foreach (var aoe in Enumerable.Reverse(_aoes))
        {
            var dir = aoe.To - aoe.From;
            var len = dir.Length();
            yield return new AOEInstance(new AOEShapeRect(len, 4f), aoe.From, dir.ToAngle(), aoe.Activate, ++i == _aoes.Count ? ArenaColor.Danger : ArenaColor.AOE);
        }

        if (_aoes.Count == 0 && _slams.Count > 0 && Module.PrimaryActor.CastInfo is null)
        {
            var slam = _slams.First();
            var shape = new AOEShapeCone(30, 90.Degrees());
            var dir = slam.Dir;
            yield return new AOEInstance(shape, Module.Arena.Center, dir.ToAngle(), Activation: slam.Activate, Color: ArenaColor.AOE);

        }
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID is SID.LeftArmGlow or SID.RightArmGlow && (AID)actor.CastInfo?.Action.ID is AID._Weaponskill_ChargeAndHammer)
        {
            var dir = ((SID)status.ID == SID.LeftArmGlow ? actor.CastInfo.Rotation.ToDirection() : -actor.CastInfo.Rotation.ToDirection());
            _slams.Add((dir, status.ExpireAt));

        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_ChargeCast)
        {
            var next = _aoes.Count == 0 ? Module.CastFinishAt(spell) : _aoes[^1].Activate.AddSeconds(2.1f);
            _aoes.Add((caster.Position, spell.LocXZ, next));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_WildCharge)
        {
            if (_aoes.Count > 0)
                _aoes.RemoveAt(0);
        }
        if ((AID)spell.Action.ID is AID._Weaponskill_ArmAndHammer5 or AID._Weaponskill_ArmAndHammer7)
        {
            if (_slams.Count > 0)
                _slams.RemoveAt(0);
        }
    }
}

class ManticorePieceStates : StateMachineBuilder
{
    public ManticorePieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArmAndHammer>()
            .ActivateOnEnter<DeadlyHold>()
            .ActivateOnEnter<Hammerleap>()
            .ActivateOnEnter<ChargeAndHammer>()
            .ActivateOnEnter<ArmAndHammer2>()
            .ActivateOnEnter<HeadsAndTails>()
            .ActivateOnEnter<TailsAndHeads>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1089, NameID = 14545)]
public class ManticorePiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120, -420), new ArenaBoundsCircle(20));

