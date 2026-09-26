#pragma warning disable CA1707 // Identifiers should not contain underscores


namespace BossMod.Global.Crucible.TaurusPiece;

public enum OID : uint
{
    Boss = 0x4C54, // R2.240, x1
    Helper = 0x233C, // R0.500, x3 (spawn during fight), Helper type
    _Gen_AethericCharge = 0x4C55, // R1.000-3.010, x0 (spawn during fight)
    _Gen_TaurusPiece = 0x4C56, // R1.500, x0 (spawn during fight)
    _Gen_AhrimanPiece = 0x4C57, // R0.900, x0 (spawn during fight)
    _Gen_ = 0x4C8D, // R1.000, x10
}

public enum AID : uint
{
    _AutoAttack_ = 50784, // Boss->player, no cast, single-target
    _Weaponskill_MortalRay = 48143, // Boss->self, 5.0s cast, range 60 circle
    _Weaponskill_RuinousLocus = 48154, // Boss->self, 4.5+1.5s cast, single-target
    _Weaponskill_RuinousLocus1 = 48155, // Helper->self, 6.0s cast, range 8 circle
    _Weaponskill_RuinousRing = 48156, // Boss->self, 4.5+1.5s cast, single-target
    _Weaponskill_RuinousRing1 = 48157, // Helper->self, 6.0s cast, range 8-50 donut
    _Weaponskill_RayOfIgnorance = 48144, // Boss->self, 5.0+1.0s cast, single-target
    _Weaponskill_RayOfIgnorance1 = 48145, // Helper->player, no cast, single-target
    _Weaponskill_Burst = 48146, // 4C55->self, 1.5s cast, range 6 circle
    _Weaponskill_AetherialFissure = 48149, // Boss->self, 4.0s cast, single-target
    _Weaponskill_Burst1 = 48147, // 4C55->self, 1.5s cast, range 12 circle
    _Weaponskill_Aetherwave = 48150, // 4C56->self, 6.0s cast, range 50 width 10 rect
    _Weaponskill_Burst2 = 48148, // 4C55->self, 1.5s cast, range 18 circle
    _Weaponskill_Summon = 48151, // Boss->self, 3.0s cast, single-target
    _Weaponskill_MortalGaze = 48152, // 4C57->self, 4.5+0.5s cast, single-target
    _Weaponskill_MortalGaze1 = 48153, // Helper->self, 5.0s cast, range 50 circle
    _Spell_Stone = 48625, // 4C57->player, no cast, single-target
    _Weaponskill_RuinousExpansion = 48158, // Boss->self, 4.5+1.5s cast, single-target
    _Weaponskill_RuinousLocus2 = 48159, // Helper->self, 6.0s cast, range 8 circle
    _Weaponskill_RuinousExpansion1 = 48160, // Boss->self, no cast, single-target
    _Weaponskill_RuinousRing2 = 48161, // Helper->self, 9.5s cast, range 8-50 donut
    _Weaponskill_EyesOnMe = 48372, // 4C57->self, 10.0s cast, range 60 circle
    _Weaponskill_RuinousContraction = 48162, // Boss->self, 4.5+1.5s cast, single-target
    _Weaponskill_RuinousRing3 = 48163, // Helper->self, 6.0s cast, range 8-50 donut
    _Weaponskill_RuinousContraction1 = 48164, // Boss->self, no cast, single-target
    _Weaponskill_RuinousLocus3 = 48165, // Helper->self, 9.5s cast, range 8 circle
}

public enum SID : uint
{
    _Gen_Doom = 5421, // Boss->player, extra=0x358
    _Gen_ = 4215, // none->4C55, extra=0x1/0x2/0x3
}

public enum IconID : uint
{
    _Gen_Icon_lockon6_t0t = 234, // player->self
    _Gen_Icon_shisen_lockon_c0y2 = 667, // 4C57->self
}

public enum TetherID : uint
{
    _Gen_Tether_chn_sinentai01p = 102, // 4C8D->Boss
}

class MortalRay(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_MortalRay, "Raidwide + doom");
class RuinousRing(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_RuinousRing1, new AOEShapeDonut(8, 50));
class RuinousLocus(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_RuinousLocus1, 8);

class Doom(BossModule module) : BossComponent(module)
{
    DateTime _deadline;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_deadline != default)
            hints.Add("Cleanse!", _deadline < WorldState.FutureTime(10));
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_deadline != default)
        {
            Arena.ZoneRect(new(520, -7.5f), default(Angle), 2.5f, 2.5f, 2.5f, ArenaColor.SafeFromAOE);
            Arena.ZoneRect(new(520, 7.5f), default(Angle), 2.5f, 2.5f, 2.5f, ArenaColor.SafeFromAOE);
            Arena.ZoneRect(new(530, 0), default(Angle), 2.5f, 2.5f, 2.5f, ArenaColor.SafeFromAOE);
            Arena.ZoneRect(new(510, 0), default(Angle), 2.5f, 2.5f, 2.5f, ArenaColor.SafeFromAOE);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_deadline != default)
        {
            // platforms take ~8.6 seconds to cleanse doom, we'll call it 9 to account for movement delay
            var moveDeadline = _deadline.AddSeconds(-9);
            static Func<WPos, float> plat(float x, float z) => ShapeDistance.InvertedRect(new(x, z), default(Angle), 2.5f, 2.5f, 2.5f);
            var plats = ShapeDistance.Intersection([plat(520, -7.5f), plat(520, 7.5f), plat(530, 0), plat(510, 0)]);
            hints.AddForbiddenZone(plats, moveDeadline);
        }
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_Doom)
            _deadline = status.ExpireAt;
    }

    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_Doom)
            _deadline = default;
    }
}

// 6.1s
class RayOfIgnorance(BossModule module) : Components.GenericBaitAway(module, AID._Weaponskill_RayOfIgnorance1)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID._Gen_Icon_lockon6_t0t)
            CurrentBaits.Add(new(actor, actor, new AOEShapeCircle(18), WorldState.FutureTime(7.3f)));
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID._Gen_AethericCharge)
            CurrentBaits.Clear();
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var bait in ActiveBaitsOn(actor))
        {
            hints.AddForbiddenZone(ShapeDistance.Intersection([.. CurveApprox.Rect(new(20, 0), new(0, 15)).Select(r => ShapeDistance.InvertedCircle(Arena.Center + r, 3))]), bait.Activation);
        }
    }
}

class Burst(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<(Actor Orb, float Size)> Orbs = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Orbs.Select(o => new AOEInstance(new AOEShapeCircle(o.Size), o.Orb.Position));

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID._Gen_AethericCharge)
            Orbs.Add((actor, 6));
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID == OID._Gen_AethericCharge)
            Orbs.RemoveAll(o => o.Orb == actor);
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_)
        {
            var ix = Orbs.FindIndex(o => o.Orb == actor);
            if (ix >= 0)
                Orbs.Ref(ix).Size = 6 * (status.Extra + 1);
        }
    }
}

class Aetherwave(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Aetherwave, new AOEShapeRect(50, 5))
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => base.ActiveAOEs(slot, actor).TakeSpan(p => p.Activation, TimeSpan.FromSeconds(1));
}

class MortalGaze(BossModule module) : Components.CastGaze(module, AID._Weaponskill_MortalGaze1);

class RuinousExpansion(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<AOEInstance> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted.Take(1);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Weaponskill_RuinousLocus2:
            case AID._Weaponskill_RuinousLocus3:
                _predicted.Add(new(new AOEShapeCircle(8), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
                _predicted.SortBy(p => p.Activation);
                break;
            case AID._Weaponskill_RuinousRing2:
            case AID._Weaponskill_RuinousRing3:
                _predicted.Add(new(new AOEShapeDonut(8, 50), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
                _predicted.SortBy(p => p.Activation);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_RuinousLocus2 or AID._Weaponskill_RuinousLocus3 or AID._Weaponskill_RuinousRing2 or AID._Weaponskill_RuinousRing3)
        {
            NumCasts++;
            if (_predicted.Count > 0)
                _predicted.RemoveAt(0);
        }
    }
}

class EyesOnMe(BossModule module) : Components.CastHint(module, AID._Weaponskill_EyesOnMe, "Kill before enrage!", true);

class TaurusPieceStates : StateMachineBuilder
{
    public TaurusPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MortalRay>()
            .ActivateOnEnter<Doom>()
            .ActivateOnEnter<RuinousRing>()
            .ActivateOnEnter<RuinousLocus>()
            .ActivateOnEnter<RayOfIgnorance>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<Aetherwave>()
            .ActivateOnEnter<MortalGaze>()
            .ActivateOnEnter<RuinousExpansion>()
            .ActivateOnEnter<EyesOnMe>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1089, NameID = 14546)]
public class TaurusPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(520, 0), new ArenaBoundsRect(20, 15));

