#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Global.Crucible.ZuPiece;

public enum OID : uint
{
    Boss = 0x4C96,
    Helper = 0x233C,
    _Gen_CockerelPiece = 0x4C97, // R0.400, x0 (spawn during fight)
    _Gen_PulletPiece = 0x4C98, // R0.400, x0 (spawn during fight)
    _Gen_ZuEgg = 0x4C99, // R0.500, x8
    _Gen_ZuEgg1 = 0x4C9A, // R0.500, x8
}

public enum AID : uint
{
    _AutoAttack_ = 49680, // Boss->player, no cast, single-target
    _Ability_Hatch = 48487, // 4C9A/4C99->self, 10.0s cast, single-target
    _Weaponskill_Breakbeak = 48488, // 4C97->player, no cast, single-target
    _Weaponskill_Crossbreeze = 48491, // Boss->self, 10.0s cast, single-target
    _Weaponskill_Crossbreeze1 = 48492, // Helper->self, 3.0s cast, range 50 width 8 cross
    _Weaponskill_FlyingFrenzy = 50465, // Boss->self, 8.0s cast, single-target
    _Weaponskill_CausticVomit = 48489, // 4C98->player, 13.0s cast, single-target
    _Weaponskill_FlyingFrenzy1 = 48490, // Boss->players, no cast, range 6 circle
}

public enum SID : uint
{
    _Gen_BroodRage = 5433, // none->Boss, extra=0x1/0x2/0x3
}

public enum IconID : uint
{
    _Gen_Icon_exclamation_8s_x = 569, // 4C9A/4C99->self
    _Gen_Icon_clossingwind_lockon_y2 = 686, // player->self
    _Gen_Icon_tank_lockonae_6m_8s_01x = 465, // player->self
}

class Babies(BossModule module) : ProximityAdds(module, [OID._Gen_CockerelPiece, OID._Gen_PulletPiece]);
class CausticVomit(BossModule module) : Components.CastInterruptHint(module, AID._Weaponskill_CausticVomit)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        foreach (var c in Casters)
            hints.SetPriority(c, 1);
    }
}

class CrossbreezeBait(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCross(50, 4), (uint)IconID._Gen_Icon_clossingwind_lockon_y2, AID._Weaponskill_Crossbreeze1, 8.1f, true)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        base.OnEventIcon(actor, iconID, targetID);

        foreach (ref var bait in CurrentBaits.AsSpan())
            bait.IgnoreRotation = true;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            CurrentBaits.Clear();
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var bait in ActiveBaitsOn(actor))
            foreach (var egg in WorldState.Actors.Where(a => (OID)a.OID is OID._Gen_ZuEgg or OID._Gen_ZuEgg1 && !a.IsDead))
                hints.AddForbiddenZone(ShapeDistance.Cross(egg.Position, default, 50, 4 + egg.HitboxRadius), bait.Activation);
    }
}
class Crossbreeze(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Crossbreeze1, new AOEShapeCross(50, 4));

class FlyingFrenzy(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(6), (uint)IconID._Gen_Icon_tank_lockonae_6m_8s_01x, AID._Weaponskill_FlyingFrenzy1, 8.1f, true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var bait in ActiveBaitsOn(actor))
            foreach (var egg in WorldState.Actors.Where(a => (OID)a.OID is OID._Gen_ZuEgg or OID._Gen_ZuEgg1 && !a.IsDead))
                hints.AddForbiddenZone(ShapeDistance.Circle(egg.Position, 6 + egg.HitboxRadius), bait.Activation);
    }
}

class ZuPieceStates : StateMachineBuilder
{
    public ZuPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Babies>()
            .ActivateOnEnter<CausticVomit>()
            .ActivateOnEnter<CrossbreezeBait>()
            .ActivateOnEnter<Crossbreeze>()
            .ActivateOnEnter<FlyingFrenzy>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1090, NameID = 14572)]
public class ZuPiece(ModuleInit init) : BossModule(init, new(120, -420), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);

        Arena.Actors(Enemies(OID._Gen_ZuEgg), ArenaColor.Object);
        Arena.Actors(Enemies(OID._Gen_ZuEgg1), ArenaColor.Object);
    }
}

