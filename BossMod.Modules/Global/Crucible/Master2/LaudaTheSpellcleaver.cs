#pragma warning disable CA1707 // Identifiers should not contain underscores




namespace BossMod.Global.Crucible.LaudaTheSpellcleaver;

public enum OID : uint
{
    Boss = 0x4D1E, // R4.000, x1
    _Gen_CombustingBlade = 0x4D1F, // R1.000, x6
    _Gen_MoltenBlade = 0x4D20, // R1.000, x0 (spawn during fight)
    _Gen_EphemeralBlade = 0x4D21, // R1.500, x14
    _Gen_PossessiveBlade = 0x4D23, // R1.000, x2
    _Gen_ThanatosPiece = 0x4D24, // R2.000, x0 (spawn during fight)
    _Gen_ = 0x4E5E, // R1.400, x0 (spawn during fight)
    Helper = 0x233C, // R0.500, x23, Helper type

    CageOfMoltenMetal = 0x1EC0DD
}

public enum AID : uint
{
    _AutoAttack_ = 50860, // Boss->players, no cast, range 9 120-degree cone
    _Weaponskill_Thunderbolt = 49470, // Boss->self/players, 5.0s cast, range 50 width 6 rect
    _Weaponskill_VanishingDaggers = 49481, // Boss->self, 3.0s cast, single-target
    _Ability_ = 49436, // Boss->location, no cast, single-target
    _Weaponskill_Rush = 49482, // 4D21->self, 0.7+0.3s cast, single-target
    _Weaponskill_Rush1 = 49483, // Helper->self, 1.0s cast, range 50 width 5 rect
    _Weaponskill_BeastlyAura = 49452, // Boss->self, 6.0s cast, single-target
    _Weaponskill_BeastlyAura1 = 49453, // Helper->self, 7.0s cast, range 80 width 80 rect
    _Weaponskill_PointMaker = 49462, // Boss->self, 3.0s cast, single-target
    _Weaponskill_OverpoweringPoint = 50846, // Boss->self, 4.9+2.0s cast, single-target
    _Weaponskill_OverpoweringPoint1 = 50847, // Boss->self, no cast, single-target
    _Weaponskill_OverpoweringPoint2 = 49463, // 4D23->self, 1.7+0.3s cast, single-target
    _Weaponskill_OverpoweringPoint3 = 50848, // Helper->self, 2.0s cast, range 60 width 6 rect
    _Weaponskill_OverpoweringPoint4 = 49464, // Helper->self, 2.0s cast, range 60 width 6 rect
    _AutoAttack_1 = 49680, // 4D24->player, no cast, single-target
    _Weaponskill_UnseenForce = 49484, // Boss->self, 3.0s cast, single-target
    _Weaponskill_UnseenForce1 = 49485, // Boss->self, no cast, single-target
    _Weaponskill_UnseenForce2 = 49487, // Helper->player, no cast, single-target
    _Weaponskill_Shockwave = 49489, // Helper->player, no cast, single-target
    _Weaponskill_InfernalPain = 50541, // 4D24->self, 8.0s cast, range 40 circle
    _Weaponskill_Gyrocleave = 49450, // Boss->self, 6.0s cast, single-target
    _Weaponskill_Gyrocleave1 = 49451, // Helper->self, 7.0s cast, range 80 width 20 rect
    _Weaponskill_DeadlyDemesne = 49454, // Boss->self, 3.0s cast, single-target
    _Weaponskill_Fetters = 49458, // Helper->self, no cast, range 5 width 5 rect
    _Weaponskill_MoltenMetal = 49465, // Boss->self, 6.2+2.1s cast, single-target
    _Weaponskill_CageOfMoltenMetal = 49460, // Helper->self, 3.0s cast, range 12 circle
    _Weaponskill_MoltenMetal1 = 49466, // Boss->self, no cast, single-target
    _Weaponskill_MoltenMetal2 = 49467, // 4D20->Boss, 2.5s cast, single-target
    _Weaponskill_MoltenMetal3 = 49468, // Helper->self, 3.0s cast, range 6 circle
    _Spell_BeastlyFlare = 49469, // 4D20->self, 11.0s cast, range 80 circle
    _Weaponskill_CombustingBlades = 49447, // Boss->self, no cast, single-target
    _Weaponskill_CombustingBlades1 = 49437, // 4D1F->Boss, no cast, single-target
    _Weaponskill_CombustingBlades2 = 49438, // 4D1F->Boss, no cast, single-target
    _Weaponskill_CombustingBlades3 = 49439, // 4D1F->Boss, no cast, single-target
    _Weaponskill_CombustingBlades4 = 49440, // Helper->self, 0.5s cast, range 2 circle
    _Weaponskill_CombustingBlades5 = 49441, // Helper->self, 0.7s cast, range 2 circle
    _Weaponskill_CombustingBlades6 = 49442, // Helper->self, 0.9s cast, range 2 circle
    _Weaponskill_MagicalCombustion = 49443, // 4D1F->self, 5.0s cast, range 8 circle
    _Weaponskill_GluttonousGoring = 49448, // Boss->self, 8.0+0.6s cast, single-target
    _Weaponskill_GluttonousGoring1 = 49449, // Helper->self, 13.6s cast, range 40 circle
    _Weaponskill_CombustingBlades7 = 49444, // Boss->self, no cast, single-target
    _Weaponskill_GluttonousGutting = 49445, // Boss->self, 8.0+0.6s cast, single-target
    _Weaponskill_GluttonousGutting1 = 49446, // Helper->self, 13.6s cast, range 50 width 40 rect
}

public enum SID : uint
{
    _Gen_Paralysis = 5388, // Boss->player, extra=0x0
    _Gen_ = 2552, // none->Boss, extra=0x483/0x482/0x478/0x477
    _Gen_Bind = 5555, // none->player, extra=0x0
    _Gen_UnseenForce = 5341, // Boss->player, extra=0x0
    _Gen_Petrification = 4891, // Helper->player, extra=0x0
    _Gen_Burns = 3065, // none->player, extra=0x0
    _Gen_Burns1 = 3066, // none->player, extra=0x0
}

public enum IconID : uint
{
    _Gen_Icon_tank_laser_5sec_lockon_c0a1 = 471, // player->self
    _Gen_Icon_lockon6_t0t = 234, // player->self
    _Gen_Icon_m0296_com_s5count3g2 = 276, // player->self
    _Gen_Icon_suteloc6s6m_1k1 = 669, // player->self
}

public enum TetherID : uint
{
    _Gen_Tether_chn_dark001f = 1, // Boss/4D23->player
}

// rush actiontimeline 11D2 -> cast event after 14.7s
class AutoAttack(BossModule module) : Components.Cleave(module, AID._AutoAttack_, new AOEShapeCone(9, 60.Degrees()), (uint)OID.Boss, activeWhileCasting: false)
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (WorldState.Actors.Find(WorldState.Client.ActivePet.InstanceID) is { } pet)
            Arena.Actor(pet, ArenaColor.PlayerGeneric);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Module.PrimaryActor.TargetID == actor.InstanceID && Module.PrimaryActor.CastInfo == null && Module.PrimaryActor.IsTargetable && WorldState.Actors.Find(WorldState.Client.ActivePet.InstanceID) is { } pet)
        {
            var dir = Module.PrimaryActor.AngleTo(pet);
            hints.AddForbiddenZone(ShapeDistance.Cone(Module.PrimaryActor.Position, 9, dir, 60.Degrees()), DateTime.MaxValue);
        }
    }
}

class BeastlyAura(BossModule module) : Components.Knockback(module, AID._Weaponskill_BeastlyAura1)
{
    readonly List<Actor> Casters = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Casters.Add(caster);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
            Casters.Remove(caster);
    }

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        foreach (var c in Casters)
        {
            var ci = c.CastInfo!;
            yield return new(ci.LocXZ, 20, Module.CastFinishAt(ci), new AOEShapeRect(80, 80), ci.Rotation + 90.Degrees(), Kind.DirForward);
            yield return new(ci.LocXZ, 20, Module.CastFinishAt(ci), new AOEShapeRect(80, 80), ci.Rotation - 90.Degrees(), Kind.DirForward);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count > 0)
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(Arena.Center, default(Angle), 5, 5, 2.5f), Module.CastFinishAt(Casters[0].CastInfo));
    }
}
class Thunderbolt(BossModule module) : Components.BaitAwayCast(module, AID._Weaponskill_Thunderbolt, new AOEShapeRect(50, 3));

class Rush(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_Rush1)
{
    readonly List<AOEInstance> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var nextActivation = default(DateTime);
        foreach (var c in Utils.TakeSpan(_predicted, p => p.Activation, TimeSpan.FromSeconds(3)))
        {
            var thisActivation = c.Activation;
            var color = ArenaColor.AOE;
            if (nextActivation == default)
                nextActivation = thisActivation.AddSeconds(0.5f);

            if (thisActivation < nextActivation)
                color = ArenaColor.Danger;
            yield return c with { Color = color };
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID._Gen_EphemeralBlade && id == 0x11D2)
            _predicted.Add(new(new AOEShapeRect(50, 2.5f), actor.Position, actor.Rotation, WorldState.FutureTime(14.7f)));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction && _predicted.FirstOrDefault(p => p.Origin.AlmostEqual(spell.LocXZ, 1)) is { } p)
        {
            p.Origin = spell.LocXZ;
            p.Rotation = spell.Rotation;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            if (_predicted.Count > 0)
                _predicted.RemoveAt(0);
        }
    }
}

class Gyrocleave(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Gyrocleave1, new AOEShapeRect(80, 10));

class OverpoweringPointBait(BossModule module) : Components.GenericBaitAway(module)
{
    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID._Gen_Tether_chn_dark001f && WorldState.Actors.Find(tether.Target) is { } target)
            CurrentBaits.Add(new(source, target, new AOEShapeRect(60, 3), WorldState.FutureTime(5.1f)));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (CurrentBaits.Count > 0)
            foreach (var e in Module.Enemies(OID._Gen_))
                Arena.AddCircle(e.Position, e.HitboxRadius, ArenaColor.Object);
        //Arena.ActorInsideBounds(e.Position, e.Rotation, ArenaColor.Object);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_OverpoweringPoint3 or AID._Weaponskill_OverpoweringPoint4)
            CurrentBaits.Clear();
    }
}

class OverpoweringPoint(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_OverpoweringPoint3, AID._Weaponskill_OverpoweringPoint4], new AOEShapeRect(60, 3));

class UnseenForce(BossModule module) : Components.Knockback(module, AID._Weaponskill_Shockwave, true)
{
    public readonly List<(Angle Offset, DateTime Activation)> Forces = [];

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        foreach (var src in Forces.Where(s => s.Activation != default))
            yield return new(actor.Position, 40, src.Activation, null, actor.Rotation + src.Offset, Kind.DirForward);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_UnseenForce2 && WorldState.Actors.Find(spell.MainTargetID) is { } target)
            Forces.Add(((target.Rotation - spell.Rotation).Normalized(), default));

        if (spell.Action == WatchedAction)
            Forces.Clear();
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var s in Forces.Where(s => s.Activation != default))
        {
            hints.AddForbiddenZone(ShapeDistance.Circle(Arena.Center, 20), s.Activation);

            var wantedDir = (Arena.Center - actor.Position).ToAngle() + s.Offset;
            hints.ForbiddenDirections.Add(((wantedDir + 180.Degrees()).Normalized(), 178.Degrees(), s.Activation));
        }
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_UnseenForce)
            foreach (ref var s in Forces.AsSpan())
                s.Activation = status.ExpireAt;
    }
}

class ThanatosPiece(BossModule module) : Components.Adds(module, (uint)OID._Gen_ThanatosPiece, 1);
class InfernalPain(BossModule module) : Components.CastHint(module, AID._Weaponskill_InfernalPain, "Enrage!", true);

class CageOfMoltenMetalPre(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_CageOfMoltenMetal)
{
    readonly List<AOEInstance> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted;

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if ((OID)actor.OID == OID.CageOfMoltenMetal && state == 0x00010002)
            _predicted.Add(new(new AOEShapeCircle(12), actor.Position, default, WorldState.FutureTime(11.2f)));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction && _predicted.Count > 0)
            _predicted.RemoveAt(0);
    }
}

class MoltenMetal(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_MoltenMetal3, 6);
class CageOfMoltenMetal(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_CageOfMoltenMetal, 12);
class MagicalCombustion(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_MagicalCombustion, 8);

class GluttonousGutting(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_GluttonousGutting1, new AOEShapeRect(50, 20))
{
    readonly UnseenForce _uf = module.FindComponent<UnseenForce>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var c in Casters)
        {
            if (_uf.Forces.Count > 0)
                // knockback pending, hide on opposite side of boss
                hints.AddForbiddenZone(ShapeDistance.Circle(c.CastInfo!.LocXZ, 30), _uf.Forces[0].Activation);
            else
                hints.AddForbiddenZone(Shape, c.CastInfo!.LocXZ, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo));
        }
    }
}
class GluttonousGoring(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_GluttonousGoring1, 40)
{
    readonly UnseenForce _uf = module.FindComponent<UnseenForce>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var c in Casters)
        {
            if (_uf.Forces.Count > 0)
                // knockback pending, hide under boss so we get knocked to the other side
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(c.CastInfo!.LocXZ, 5), _uf.Forces[0].Activation);
            else
                hints.AddForbiddenZone(Shape, c.CastInfo!.LocXZ, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo));
        }
    }
}

// figure out radius
class BeastlyFlare(BossModule module) : Components.ProximityAOEs(module, AID._Spell_BeastlyFlare, 20);

class LaudaTheSpellcleaverStates : StateMachineBuilder
{
    public LaudaTheSpellcleaverStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AutoAttack>()
            .ActivateOnEnter<Rush>()
            .ActivateOnEnter<Thunderbolt>()
            .ActivateOnEnter<BeastlyAura>()
            .ActivateOnEnter<Gyrocleave>()
            .ActivateOnEnter<OverpoweringPointBait>()
            .ActivateOnEnter<OverpoweringPoint>()
            .ActivateOnEnter<UnseenForce>()
            .ActivateOnEnter<ThanatosPiece>()
            .ActivateOnEnter<InfernalPain>()
            .ActivateOnEnter<CageOfMoltenMetalPre>()
            .ActivateOnEnter<CageOfMoltenMetal>()
            .ActivateOnEnter<MoltenMetal>()
            .ActivateOnEnter<BeastlyFlare>()
            .ActivateOnEnter<MagicalCombustion>()
            .ActivateOnEnter<GluttonousGutting>()
            .ActivateOnEnter<GluttonousGoring>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1092, NameID = 14693)]
public class LaudaTheSpellcleaver(WorldState ws, Actor primary) : BossModule(ws, primary, new(520, -420), CustomBounds)
{
    public static readonly ArenaBoundsCustom CustomBounds = MakeBounds();

    private static ArenaBoundsCustom MakeBounds()
    {
        var c = new PolygonClipper();

        static PolygonClipper.Operand P(float x, float z) => new(CurveApprox.Rect(new WDir(x, z), new WDir(2.5f, 0), new WDir(0, 2.5f)));

        return new(25, c.UnionAll(new(CurveApprox.Rect(new(10, 0), new(0, 20))), P(0, 22.5f), P(0, -22.5f), P(12.5f, 2.5f), P(-12.5f, -2.5f), P(12.5f, -7.5f), P(-12.5f, 7.5f)));
    }
}

