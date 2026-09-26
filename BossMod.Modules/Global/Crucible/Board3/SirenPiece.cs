#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Global.Crucible.SirenPiece;

public enum OID : uint
{
    Boss = 0x4CA1, // R4.000, x1
    Helper = 0x233C, // R0.500, x9, Helper type
    _Gen_ShamblingPiece = 0x4CA3, // R0.750, x0 (spawn during fight)
    _Gen_CrawlingPiece = 0x4CA4, // R0.750, x0 (spawn during fight)
    _Gen_SweetSong = 0x4CA2, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_ = 50395, // Boss->players, no cast, range 9 ?-degree cone
    _Weaponskill_SongOfTorment = 48563, // Boss->player, 5.0s cast, single-target
    _Ability_ = 48564, // Boss->location, no cast, single-target
    _Weaponskill_UnmooringMelody = 48565, // Boss->self, no cast, single-target
    _Weaponskill_UnmooringMelody1 = 48566, // Helper->self, no cast, range 50 90-degree cone
    _Weaponskill_FeralLunge = 48569, // Boss->self, 3.8+0.2s cast, single-target
    _Weaponskill_FeralLunge1 = 48570, // Helper->self, 4.0s cast, range 50 width 16 rect
    _Weaponskill_Summon = 48567, // Boss->self, 3.0s cast, single-target
    _AutoAttack_1 = 49682, // 4CA3->none, no cast, single-target
    _Weaponskill_DeadMansDirge = 48572, // Boss->self, 5.6+1.4s cast, single-target
    _Weaponskill_DeadMansDirge1 = 48573, // Helper->self, 7.0s cast, range 12 circle
    _Weaponskill_DistantTune = 48576, // Boss->self, 3.0s cast, single-target
    _Weaponskill_Burst = 48577, // 4CA2->self, 1.0s cast, range 9 circle
    _Weaponskill_InvitingVerse = 48571, // Boss->self, 5.0s cast, range 40 circle
    _Weaponskill_DeathThroes = 48568, // _Gen_CrawlingPiece->player, no cast, single-target
    _Weaponskill_DeadMansDirge2 = 48574, // Boss->self, 6.2+0.8s cast, single-target
    _Weaponskill_DeadMansDirge3 = 48575, // Helper->self, 7.0s cast, range 2-43 donut
    _Weaponskill_Wallop = 50662, // _Gen_ShamblingPiece->self, 2.5s cast, range 5 width 3 rect
}

public enum IconID : uint
{
    _Gen_Icon_tank_lockon02k1 = 218, // player->self
}

public enum SID : uint
{
    _Gen_AboutFace = 2162, // Boss->player, extra=0x0
    _Gen_ForcedMarch = 1257, // Boss->player, extra=0x2/0x4
    _Gen_Confused = 1283, // 4C5D->player, extra=0x0
    _Gen_RightFace = 2164, // Boss->player, extra=0x0
    _Gen_LeftFace = 2163, // Boss->player, extra=0x0
}

class SongOfTorment(BossModule module) : Components.SingleTargetCast(module, AID._Weaponskill_SongOfTorment);

class UnmooringMelody(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_UnmooringMelody1)
{
    AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Ability_:
                var t = spell.TargetXZ;
                var d = Arena.Center - t;
                // verify angle, this seems way too wide
                _aoe = new(new AOEShapeCone(60, 45.Degrees()), t, d.ToAngle());
                break;
            case AID._Weaponskill_UnmooringMelody1:
                if (++NumCasts >= 12)
                {
                    _aoe = null;
                    NumCasts = 0;
                }
                break;
        }
    }
}
class FeralLunge(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_FeralLunge1, new AOEShapeRect(50, 8));

class Adds(BossModule module) : Components.AddsMulti(module, [OID._Gen_ShamblingPiece, OID._Gen_CrawlingPiece]);

class DeadMansDirge1(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_DeadMansDirge1, 12);
class DeadMansDirge2(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_DeadMansDirge3, new AOEShapeDonut(2, 43));
class Wallop(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Wallop, new AOEShapeRect(5, 1.5f));

class SweetSong(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_Burst)
{
    readonly List<(Actor Orb, DateTime Activation)> Orbs = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Orbs.Take(3).Select(o => new AOEInstance(new AOEShapeCircle(9), o.Orb.Position, default, o.Activation));

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID._Gen_SweetSong)
            Orbs.Add((actor, WorldState.FutureTime(5.8f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
            Orbs.RemoveAll(o => o.Orb == caster);
    }
}

class ForcedMarch(BossModule module) : Components.StatusDrivenForcedMarch(module, 3, 0, (uint)SID._Gen_AboutFace, (uint)SID._Gen_LeftFace, (uint)SID._Gen_RightFace);

class SirenPieceStates : StateMachineBuilder
{
    public SirenPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SongOfTorment>()
            .ActivateOnEnter<UnmooringMelody>()
            .ActivateOnEnter<FeralLunge>()
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<Wallop>()
            .ActivateOnEnter<DeadMansDirge1>()
            .ActivateOnEnter<DeadMansDirge2>()
            .ActivateOnEnter<SweetSong>()
            .ActivateOnEnter<ForcedMarch>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1090, NameID = 14583)]
public class SirenPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120, -420), new ArenaBoundsCircle(20));

