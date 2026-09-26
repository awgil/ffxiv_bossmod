#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Global.Crucible.ElderTablitaurPiece;

public enum OID : uint
{
    Boss = 0x4C5F, // R3.600, x1
    _Gen_YoungerTablitaurPiece = 0x4C60, // R3.600, x1
    _Gen_ = 0x4E00, // R1.000, x2
    Helper = 0x233C, // R0.500, x6, Helper type
}

public enum AID : uint
{
    _AutoAttack_ = 50216, // Boss/4C60->player, no cast, single-target
    _Ability_ = 48188, // Boss/4C60->location, no cast, single-target
    _Weaponskill_1111TonzeSwing = 48193, // Boss/4C60->self, 7.0+1.0s cast, single-target
    _Weaponskill_1111TonzeSwing1 = 48194, // Helper->self, 8.0s cast, range 23 circle
    _Weaponskill_1111TonzeSwing2 = 48195, // 4C60/Boss->self, 13.0+1.0s cast, single-target
    _Weaponskill_1111TonzeSwing3 = 48196, // Helper->self, 14.0s cast, range 23 circle
    _Weaponskill_1000TonzeSwipe = 48189, // 4C60/Boss->self, 6.5+0.5s cast, single-target
    _Weaponskill_1000TonzeSwipe1 = 48190, // Helper->self, 7.0s cast, range 60 width 60 rect
    _Weaponskill_1000TonzeSwipe2 = 48191, // Boss/4C60->self, 9.5+0.5s cast, single-target
    _Weaponskill_1000TonzeSwipe3 = 48192, // Helper->self, 10.0s cast, range 60 width 60 rect
    _Weaponskill_10TonzeStomp = 48197, // Boss/4C60->location, 6.3+0.7s cast, single-target
    _Weaponskill_Shockwave = 48199, // Helper->self, 7.0s cast, range 5-60 donut
    _Weaponskill_10TonzeStomp1 = 48198, // Helper->self, 7.0s cast, range 5 circle
    _Weaponskill_10TonzeStomp2 = 48200, // 4C60/Boss->location, 9.8+0.7s cast, single-target
    _Weaponskill_10TonzeStomp3 = 48201, // Helper->self, 10.5s cast, range 5 circle
    _Weaponskill_Shockwave1 = 48202, // Helper->self, 10.5s cast, range 5-60 donut
    _Ability_RallyingCheer = 48203, // 4C60->Boss, 7.0s cast, single-target
    _Weaponskill_100TonzeSlash = 48204, // Boss->self/player, 9.0s cast, range 65 width 8 rect
    _Weaponskill_EndlessSwing = 48205, // 4C60/Boss->self, 5.0+1.0s cast, single-target
    _Weaponskill_EndlessSwing1 = 48206, // Helper->4C60/Boss, 6.0s cast, range 8 circle
    _Weaponskill_EndlessSwing2 = 48207, // Helper->Boss/4C60, no cast, range 8 circle
    _Weaponskill_EndlessSwipes = 48209, // 4C60/Boss->self, 5.0s cast, single-target
    _Weaponskill_EndlessSwipes1 = 48210, // Helper->self, 5.2s cast, range 40 60-degree cone
    _Weaponskill_EndlessSwipes2 = 48211, // 4C60/Boss->self, no cast, single-target
    _Weaponskill_EndlessSwipes3 = 48212, // Helper->self, 0.5s cast, range 40 ?-degree cone
    _Weaponskill_EndlessSwipes4 = 48208, // Boss/4C60->self, 5.0s cast, single-target
    _Weaponskill_DisorientingGroan = 48213, // Boss->self, no cast, single-target
    _Weaponskill_EndlessSlashes = 48214, // Boss->self, 9.0s cast, single-target
}

public enum IconID : uint
{
    _Gen_Icon_tank_laser_7sec_lockon01p = 412, // player->self
    _Gen_Icon_d1004turning_left_c0p = 168, // 4C60/Boss->self
    _Gen_Icon_d1004turning_right_c0p = 167, // Boss/4C60->self
}

public enum TetherID : uint
{
    _Gen_Tether_chn_m0354_0c = 54, // Boss/4C60->4E00
    _Gen_Tether_chn_dark001f = 1, // Boss->player
    _Gen_Tether_chn_o5d1_d_b_1p1 = 260, // 4C60->Boss
    _Gen_Tether_chn_tergetfix1f = 17, // Boss/4C60->player
}

class C1111TonzeSwing(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_1111TonzeSwing1, AID._Weaponskill_1111TonzeSwing3], new AOEShapeCircle(23), maxCasts: 1);
class C1000TonzeSwipe(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_1000TonzeSwipe1, AID._Weaponskill_1000TonzeSwipe3], new AOEShapeRect(60, 30), maxCasts: 1);
class C10TonzeStomp(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_10TonzeStomp1, AID._Weaponskill_10TonzeStomp3], new AOEShapeCircle(5));

class Shockwave(BossModule module) : Components.Knockback(module)
{
    readonly List<Actor> Casters = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_Shockwave or AID._Weaponskill_Shockwave1)
        {
            Casters.Add(caster);
            Casters.SortBy(s => Module.CastFinishAt(s.CastInfo));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_Shockwave or AID._Weaponskill_Shockwave1)
        {
            NumCasts++;
            Casters.Remove(caster);
        }
    }

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        foreach (var c in Casters.Take(2))
            yield return new Source(c.CastInfo!.LocXZ, 20, Module.CastFinishAt(c.CastInfo));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var s in Sources(slot, actor).Where(s => !IsImmune(slot, s.Activation)).Take(1))
        {
            var tc = s.Origin + (Arena.Center - s.Origin).Normalized() * 6;
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(tc, 1), s.Activation);
        }
    }
}

class RallyingCheer(BossModule module) : Components.CastInterruptHint(module, AID._Ability_RallyingCheer);

class C100TonzeSlash(BossModule module) : Components.BaitAwayCast(module, AID._Weaponskill_100TonzeSlash, new AOEShapeRect(65, 4));

class EndlessSwingFirst(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_EndlessSwing1, 8);

// he casts at least 22 times
class EndlessSwing(BossModule module) : Components.Voidzone(module, 8, 0, a => a.IsTargetable, 8)
{
    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if ((OID)actor.OID is OID.Boss or OID._Gen_YoungerTablitaurPiece && modelState == 57)
            AddSource(actor);
    }
}

class EndlessSwipes(BossModule module) : Components.GenericRotatingAOE(module)
{
    Angle _rotation;
    Actor? _caster;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_EndlessSwipes1)
        {
            _caster = caster;
            Init();
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        switch ((IconID)iconID)
        {
            case IconID._Gen_Icon_d1004turning_left_c0p:
                _rotation = 30.Degrees();
                Init();
                break;
            case IconID._Gen_Icon_d1004turning_right_c0p:
                _rotation = -30.Degrees();
                Init();
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_EndlessSwipes1 or AID._Weaponskill_EndlessSwipes3)
            AdvanceSequence(caster.Position, spell.Rotation, WorldState.CurrentTime);

        // safeguard, dunno if this can happen
        if ((AID)spell.Action.ID is AID._Weaponskill_DisorientingGroan)
            Sequences.Clear();
    }

    void Init()
    {
        if (Sequences.Count > 0 || _rotation == default || _caster == null)
            return;

        Sequences.Add(new()
        {
            Shape = new AOEShapeCone(40, 30.Degrees()),
            Origin = _caster.CastInfo!.LocXZ,
            Rotation = _caster.CastInfo!.Rotation,
            Increment = _rotation,
            NextActivation = Module.CastFinishAt(_caster.CastInfo),
            SecondsBetweenActivations = 1.6f,
            NumRemainingCasts = 13,
            MaxShownAOEs = 3
        });
    }
}

class ElderTablitaurPieceStates : StateMachineBuilder
{
    public ElderTablitaurPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<C1111TonzeSwing>()
            .ActivateOnEnter<C1000TonzeSwipe>()
            .ActivateOnEnter<C10TonzeStomp>()
            .ActivateOnEnter<Shockwave>()
            .ActivateOnEnter<RallyingCheer>()
            .ActivateOnEnter<C100TonzeSlash>()
            .ActivateOnEnter<EndlessSwingFirst>()
            .ActivateOnEnter<EndlessSwing>()
            .ActivateOnEnter<EndlessSwipes>()
            .Raw.Update = () => module.PrimaryActor.IsDeadOrDestroyed && ((ElderTablitaurPiece)module).YoungerTablitaur is { IsDeadOrDestroyed: true };
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1089, NameID = 14555)]
public class ElderTablitaurPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120, 0), new ArenaBoundsSquare(20))
{
    public Actor? YoungerTablitaur;

    protected override void UpdateModule()
    {
        YoungerTablitaur ??= Enemies(OID._Gen_YoungerTablitaurPiece).FirstOrDefault();
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!PrimaryActor.IsDead)
            hints.GoalZones.Add(AIHints.GoalSingleTarget(PrimaryActor.Position, PrimaryActor.HitboxRadius + 3.5f, 0.1f));
        if (YoungerTablitaur is { IsDead: false } y)
            hints.GoalZones.Add(AIHints.GoalSingleTarget(y.Position, y.HitboxRadius + 3.5f, 0.1f));
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);

        Arena.Actor(YoungerTablitaur, ArenaColor.Enemy);
    }
}

