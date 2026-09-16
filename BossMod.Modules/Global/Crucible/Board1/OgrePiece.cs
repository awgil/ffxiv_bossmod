#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Global.Crucible.OgrePiece;

public enum OID : uint
{
    Boss = 0x4B8D, // R2.080, x1
    Helper = 0x233C, // R0.500, x27, Helper type
    _Gen_WispPiece = 0x4B8E, // R0.800, x0 (spawn during fight)
    _Gen_GreatWispPiece = 0x4DD4, // R1.600, x0 (spawn during fight)
    _Gen_BallOfFire = 0x4B8F, // R1.000, x0 (spawn during fight)
    Magma = 0x1EC025
}

public enum AID : uint
{
    _AutoAttack_ = 49682, // Boss->player, no cast, single-target
    _Ability_ = 46914, // Boss->location, no cast, single-target
    _Weaponskill_ScorchingSmite = 46913, // Boss->self, 5.0+1.0s cast, single-target
    _Weaponskill_ScorchingSmite1 = 46912, // Helper->self, 6.0s cast, range 40 120-degree cone
    _Weaponskill_Allfire = 46915, // Boss->self, 4.0s cast, range 40 circle
    _Weaponskill_Magma = 46916, // Helper->location, 3.0s cast, range 3 circle
    _Weaponskill_Magma1 = 46917, // Helper->location, 3.0s cast, range 5 circle
    _Weaponskill_ScorchingSmite2 = 46910, // Boss->self, 6.0s cast, single-target
    _Weaponskill_ScorchingSmite3 = 46911, // Boss->self, no cast, single-target
    _Weaponskill_ScorchingSmite4 = 49688, // Helper->self, 9.3s cast, range 40 120-degree cone
    _Ability_BurningWard = 46918, // Boss->self, 3.0s cast, single-target
    _Ability_FireCall = 46921, // Boss->self, 4.0s cast, single-target
    _Weaponskill_ArmOfPurgatory = 46922, // 4B8F->self, 1.0s cast, range 10 circle
}

public enum TetherID : uint
{
    _Gen_Tether_chn_tergetfix1f = 17, // 4B8F->player
}

public enum SID : uint
{
    _Gen_BurningWard = 4175, // Boss->Boss, extra=0x0
}

class Allfire(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_Allfire);
class ScorchingSmite(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_ScorchingSmite1, AID._Weaponskill_ScorchingSmite4], new AOEShapeCone(40, 60.Degrees()));
class MagmaSmall(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Magma, 3);
class MagmaLarge(BossModule module) : Components.VoidzoneAtCastTarget(module, 5, AID._Weaponskill_Magma1, OID.Magma, 0.6f);
class BurningWard(BossModule module) : Components.InvincibleStatus(module, (uint)SID._Gen_BurningWard);
class BurningWardMagma(BossModule module) : Components.Voidzone(module, 5, 0x1E9927);
class WispPiece(BossModule module) : Components.AddsMulti(module, [OID._Gen_WispPiece, OID._Gen_GreatWispPiece]);

class BallOfFire(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_ArmOfPurgatory, 10)
{
    readonly List<Actor> _sources = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var aoe in base.ActiveAOEs(slot, actor))
            yield return aoe;

        foreach (var src in _sources)
            yield return new(Shape, src.Position, default, WorldState.FutureTime(1));
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID._Gen_Tether_chn_tergetfix1f)
            _sources.Add(source);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (spell.Action == WatchedAction)
            _sources.Remove(caster);
    }
}

class OgrePieceStates : StateMachineBuilder
{
    public OgrePieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Allfire>()
            .ActivateOnEnter<ScorchingSmite>()
            .ActivateOnEnter<MagmaSmall>()
            .ActivateOnEnter<MagmaLarge>()
            .ActivateOnEnter<BurningWard>()
            .ActivateOnEnter<BurningWardMagma>()
            .ActivateOnEnter<WispPiece>()
            .ActivateOnEnter<BallOfFire>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1088, NameID = 14538)]
public class OgrePiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120, -420), new ArenaBoundsCircle(20));

