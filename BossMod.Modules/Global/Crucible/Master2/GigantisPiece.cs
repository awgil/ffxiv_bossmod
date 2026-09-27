namespace BossMod.Global.Crucible.GigantisPiece;

public enum OID : uint
{
    Helper = 0x233C, // R0.500, x4, Helper type
    Boss = 0x4D03, // R4.000, x1
    _Gen_CongealedLightning = 0x4D05, // R2.500, x0 (spawn during fight)
    _Gen_CongealedLightning1 = 0x4D06, // R0.500, x0 (spawn during fight)
    _Gen_CongealedKindling = 0x4D07, // R2.500, x0 (spawn during fight)
    _Gen_CongealedKindling1 = 0x4D08, // R0.500, x0 (spawn during fight)
    _Gen_CyclopsPiece = 0x4D04, // R2.000, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_ = 50786, // Boss->player, no cast, single-target
    _Weaponskill_GiganticRage = 49360, // Boss->self, 5.0+1.0s cast, single-target
    _Weaponskill_GiganticRage1 = 49361, // Helper->self, 6.0s cast, range 40 180-degree cone
    _Spell_Thunder = 48623, // _Gen_CongealedLightning/_Gen_CongealedLightning1->player, no cast, single-target
    _Weaponskill_SmashingStamp = 49377, // Boss->self, 2.6+0.4s cast, single-target
    _Weaponskill_ = 49379, // Helper->self, 3.0s cast, range 7 width 4 rect
    _Weaponskill_SmashingStamp1 = 49378, // Helper->self, 3.0s cast, range 40 width 8 rect
    _Weaponskill_GiganticRage2 = 49358, // Boss->self, 5.0+1.0s cast, single-target
    _Weaponskill_GiganticRage3 = 49359, // Helper->self, 6.0s cast, range 15 circle
    _Weaponskill_Afterspark = 49364, // Helper->location, 3.0s cast, range 2 circle
    _Weaponskill_Afterspark1 = 49365, // Helper->self, no cast, range 60 circle, tower miss
    _Spell_Fire = 48622, // _Gen_CongealedKindling/_Gen_CongealedKindling1->player, no cast, single-target
    _Weaponskill_Rupture = 49373, // _Gen_CongealedLightning->self, 7.0s cast, range 40 circle
    _Weaponskill_GiganticRage4 = 49362, // Boss->self, 5.0+1.0s cast, single-target
    _Weaponskill_GiganticRage5 = 49363, // Helper->self, 6.0s cast, range 40 180-degree cone
    _Weaponskill_Afterburn = 49366, // Helper->self, 3.0s cast, range 6 circle
    _AutoAttack_1 = 50398, // _Gen_CyclopsPiece->player, no cast, single-target
    _Weaponskill_Glower = 49367, // _Gen_CyclopsPiece->self, 4.0s cast, range 40 width 3 rect
    _Spell_Camaraderie = 49368, // _Gen_CyclopsPiece->Boss, 8.0s cast, single-target
    _Weaponskill_Rupture1 = 49375, // _Gen_CongealedKindling->self, 7.0s cast, range 40 circle
}

public enum SID : uint
{
    _Gen_ = 2056, // none->Boss, extra=0x499 (lightning)/0x49A (fire)
    _Gen_Bleeding = 3077, // none->player, extra=0x0
    _Gen_Bleeding1 = 3078, // none->player, extra=0x0
}

public enum IconID : uint
{
    _Gen_Icon_lockon6_t0t = 234, // player->self
}

class GiganticRageFrontBack(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_GiganticRage1, AID._Weaponskill_GiganticRage5], new AOEShapeCone(40, 90.Degrees()));
class GiganticRageCircle(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_GiganticRage3, 15);
class BigSlime1(BossModule module) : Components.AddsPointless(module, (uint)OID._Gen_CongealedLightning);
class BigSlime2(BossModule module) : Components.AddsPointless(module, (uint)OID._Gen_CongealedKindling);
class SmallSlime(BossModule module) : Components.AddsMulti(module, [OID._Gen_CongealedLightning1, OID._Gen_CongealedKindling1]);
class Afterburn(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Afterburn, 6);
class Afterspark(BossModule module) : Components.CastTowers(module, AID._Weaponskill_Afterspark, 2);
class Rupture1(BossModule module) : Components.CastInterruptHint(module, AID._Weaponskill_Rupture);
class Rupture2(BossModule module) : Components.CastInterruptHint(module, AID._Weaponskill_Rupture1);

class SmashBait(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(10, 2, -3), (uint)IconID._Gen_Icon_lockon6_t0t, AID._Weaponskill_, 6)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            CurrentBaits.Clear();
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var bait in ActiveBaitsOn(actor))
        {
            var targets = WorldState.Actors.Where(a => (OID)a.OID is OID._Gen_CongealedLightning or OID._Gen_CongealedKindling).ToList();
            if (Module.PrimaryActor.FindStatus(SID._Gen_) is { } status)
            {
                if (status.Extra == 0x499)
                    targets.RemoveAll(t => (OID)t.OID == OID._Gen_CongealedLightning);
                if (status.Extra == 0x49A)
                    targets.RemoveAll(t => (OID)t.OID == OID._Gen_CongealedKindling);
            }

            var dest = targets.Select(t => ShapeDistance.InvertedCircle(t.Position, t.HitboxRadius + 1)).ToList();
            if (dest.Count > 0)
                hints.AddForbiddenZone(ShapeDistance.Intersection(dest), bait.Activation);
        }
    }
}
class SmashingStamp(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_SmashingStamp1, new AOEShapeRect(40, 4));

class CyclopsPiece(BossModule module) : Components.Adds(module, (uint)OID._Gen_CyclopsPiece);
class Glower(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Glower, new AOEShapeRect(40, 1.5f));
class Camaraderie(BossModule module) : Components.CastInterruptHint(module, AID._Spell_Camaraderie);

class GigantisPieceStates : StateMachineBuilder
{
    public GigantisPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GiganticRageFrontBack>()
            .ActivateOnEnter<GiganticRageCircle>()
            .ActivateOnEnter<BigSlime1>()
            .ActivateOnEnter<BigSlime2>()
            .ActivateOnEnter<SmallSlime>()
            .ActivateOnEnter<SmashBait>()
            .ActivateOnEnter<SmashingStamp>()
            .ActivateOnEnter<Rupture1>()
            .ActivateOnEnter<Rupture2>()
            .ActivateOnEnter<Afterburn>()
            .ActivateOnEnter<Afterspark>()
            .ActivateOnEnter<CyclopsPiece>()
            .ActivateOnEnter<Glower>()
            .ActivateOnEnter<Camaraderie>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1092, NameID = 14670)]
public class GigantisPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120, -420), new ArenaBoundsCircle(20));

