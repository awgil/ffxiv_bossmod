﻿using BossMod.Autorotation;

namespace BossMod.Dawntrail.Quest.TakingAStand;

public enum OID : uint
{
    Boss = 0x4211,
    Helper = 0x233C,
    Thingy = 0x421D,
}

public enum AID : uint
{
    _AutoAttack_Attack = 870, // Boss->player, no cast, single-target
    _Weaponskill_Roar = 37090, // Boss->self, 5.0s cast, range 45 circle
    _Ability_ = 37089, // Boss->location, no cast, single-target
    _Weaponskill_LethalSwipe = 37092, // Boss->self, 6.0s cast, range 45 180-degree cone
    _Weaponskill_MagickedStandard = 37098, // Boss->self, 4.0s cast, single-target
    _Weaponskill_Kickdown = 37101, // Boss->player, 5.0s cast, single-target
    _Weaponskill_ArcaneActivation = 37099, // 4212->self, 4.0s cast, range 10 circle
    _Weaponskill_Fireshower = 37751, // Boss->self, 3.0s cast, single-target
    _Weaponskill_Fireshower1 = 37802, // Helper->location, 4.0s cast, range 6 circle
    _Weaponskill_GreatLeap = 37093, // Boss->self, 8.5s cast, single-target
    _Weaponskill_GreatLeap1 = 37094, // Boss->location, no cast, range 18 circle
    _Weaponskill_ArcaneActivation1 = 37100, // 4213->self, 4.0s cast, range -10 donut
    _Weaponskill_ShadowSiblings = 37103, // Boss->self, 4.0s cast, single-target
    _Weaponskill_LethalSwipe1 = 37104, // 4215->self, 5.9+0.1s cast, range 10 circle
    _Weaponskill_SelfSacrifice = 37105, // 4215->self, no cast, range 10 circle
    _Weaponskill_Roar1 = 37091, // Boss->self, 5.0s cast, range 45 circle
    _Weaponskill_RiotousRampage = 37095, // Boss->location, 5.9s cast, single-target
    _Weaponskill_RiotousRampage1 = 37096, // Boss->location, no cast, single-target
    _Weaponskill_RiotousRampage2 = 36416, // Helper->self, 7.2s cast, range 4 circle
    _Weaponskill_HeavyImpact = 36444, // Helper->self, no cast, range 40 circle
    _Weaponskill_SupremeStandard = 37106, // Boss->self, 4.0s cast, single-target
    _Weaponskill_ = 37107, // Helper->self, no cast, range 50 circle
    _Weaponskill_1 = 37108, // Boss->self, 1.5s cast, single-target
    _Weaponskill_MightAndMagic = 13270, // Helper->self, 8.1s cast, range 50 width 10 rect
    _Weaponskill_MightAndMagic1 = 13271, // Helper->self, 8.8s cast, range 50 width 10 rect
    _AutoAttack_Attack1 = 873, // 421B->player, no cast, single-target
    _Ability_1 = 34873, // 4216/4217->location, no cast, single-target
    _Weaponskill_RunThrough = 37110, // 4216->self, 4.0s cast, range 45 width 5 rect
    _Weaponskill_RunThrough1 = 37111, // 4217->self, 4.0s cast, range 45 width 5 rect
    _Weaponskill_Fireflood = 37113, // Helper->location, 5.0s cast, range 40 circle
    _Weaponskill_Fireflood1 = 37112, // 4218->self, 5.0s cast, single-target
    _Weaponskill_TuraliStoneIII = 37114, // 4218->self, 4.0s cast, single-target
    _Weaponskill_TuraliStoneIII1 = 37115, // Helper->location, 3.0s cast, range 4 circle
    _Weaponskill_Desperation = 37119, // Boss->self, no cast, range 10 90-degree cone
    _Weaponskill_TuraliQuake = 37116, // 4218->self, 4.0s cast, single-target
    _Weaponskill_TuraliQuake1 = 37117, // Helper->location, 5.0s cast, range 9 circle
}

class TuraliQuake(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_TuraliQuake1), 9, maxCasts: 5);
class RunThrough(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_RunThrough), new AOEShapeRect(45, 2.5f));
class RunThrough1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_RunThrough1), new AOEShapeRect(45, 2.5f));
class Fireflood(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Fireflood), 10);
class TuraliStoneIII(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_TuraliStoneIII1), 4);
class RiotousRampage(BossModule module) : Components.CastTowers(module, ActionID.MakeSpell(AID._Weaponskill_RiotousRampage2), 4);
class ArcaneActivation1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_ArcaneActivation1), new AOEShapeDonut(3, 10));
class LethalSwipe1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_LethalSwipe1), new AOEShapeCircle(10));
class GreatLeap(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID._Weaponskill_GreatLeap))
{
    private DateTime? CastFinish;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (CastFinish == null || Module.Enemies(OID.Thingy).FirstOrDefault() is not Actor t)
            yield break;

        yield return new AOEInstance(new AOEShapeCircle(18 / 3.58f * t.HitboxRadius), t.Position, Activation: CastFinish.Value);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var c in ActiveAOEs(slot, actor))
            if (c.Risky && (c.Activation - WorldState.CurrentTime).TotalSeconds < 5)
                hints.AddForbiddenZone(c.Shape, c.Origin, c.Rotation, c.Activation);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (ActiveAOEs(slot, actor).Any(c => c.Risky && c.Check(actor.Position) && (c.Activation - WorldState.CurrentTime).TotalSeconds < 5))
            hints.Add(WarningText);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            CastFinish = Module.CastFinishAt(spell).AddSeconds(1.3f);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID._Weaponskill_GreatLeap1)
            CastFinish = null;
    }
}
class Fireshower(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Fireshower1), 6);
class ArcaneActivation(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_ArcaneActivation), new AOEShapeCircle(10));
class Kickdown(BossModule module) : Components.Knockback(module, ActionID.MakeSpell(AID._Weaponskill_Kickdown))
{
    private readonly List<Actor> Casters = [];

    public override IEnumerable<Source> Sources(int slot, Actor actor) => Casters.Select(c => new Source(c.Position, 18, Module.CastFinishAt(c.CastInfo)));

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count == 0)
            return;

        var source = Casters[0];
        var arenaBounds = ShapeDistance.InvertedCircle(Arena.Center, 20);

        float kbdist(WPos playerPos)
        {
            var dir = (playerPos - source.Position).Normalized();
            var expected = playerPos + 18 * dir;
            return arenaBounds(expected);
        }

        hints.AddForbiddenZone(kbdist, Module.CastFinishAt(source.CastInfo));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Casters.Clear();
    }
}
class LethalSwipe(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_LethalSwipe), new AOEShapeCone(45, 90.Degrees()));
class Roar(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Weaponskill_Roar));
class RoarBounds(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Roar))
{
    private AOEInstance? Bounds;
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(Bounds);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Bounds = new AOEInstance(new AOEShapeDonut(20, 100), Arena.Center, default, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            Bounds = null;
            Arena.Bounds = new ArenaBoundsCircle(20);
        }
    }
}

class AutoWuk(WorldState ws) : StatelessRotation(ws, 3)
{
    protected override void Exec(Actor? primaryTarget)
    {
        if (primaryTarget == null)
            return;

        if (World.Party.LimitBreakCur == 10000)
            UseAction(Roleplay.AID.DawnlitConviction, primaryTarget, 100);

        var numAOETargets = Hints.PotentialTargets.Count(x => x.Actor.Position.InCircle(primaryTarget.Position, 8));

        var gcd = ComboAction switch
        {
            Roleplay.AID.ClawOfTheBraax => Roleplay.AID.FangsOfTheBraax,
            Roleplay.AID.FangsOfTheBraax => Roleplay.AID.TailOfTheBraax,
            Roleplay.AID.TuraliFervor => Roleplay.AID.TuraliJudgment,
            Roleplay.AID.TrialsOfTural => Roleplay.AID.TuraliFervor,
            _ => numAOETargets > 1 ? Roleplay.AID.TrialsOfTural : Roleplay.AID.ClawOfTheBraax
        };

        UseAction(gcd, primaryTarget);
        UseAction(Roleplay.AID.BeakOfTheLuwatena, primaryTarget, -5);

        if (Player.DistanceToHitbox(primaryTarget) < 3)
            UseAction(Roleplay.AID.RunOfTheRroneek, primaryTarget, -10);

        if (Player.HPMP.CurHP * 2 < Player.HPMP.MaxHP)
            UseAction(Roleplay.AID.LuwatenaPulse, Player, -10);
    }
}

class WukAI(BossModule module) : Components.RotationModule<AutoWuk>(module);

class BakoolJaJaStates : StateMachineBuilder
{
    public BakoolJaJaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Roar>()
            .ActivateOnEnter<RoarBounds>()
            .ActivateOnEnter<LethalSwipe>()
            .ActivateOnEnter<WukAI>()
            .ActivateOnEnter<Kickdown>()
            .ActivateOnEnter<ArcaneActivation>()
            .ActivateOnEnter<ArcaneActivation1>()
            .ActivateOnEnter<LethalSwipe1>()
            .ActivateOnEnter<Fireshower>()
            .ActivateOnEnter<GreatLeap>()
            .ActivateOnEnter<RiotousRampage>()
            .ActivateOnEnter<Fireflood>()
            .ActivateOnEnter<RunThrough>()
            .ActivateOnEnter<RunThrough1>()
            .ActivateOnEnter<TuraliStoneIII>()
            .ActivateOnEnter<TuraliQuake>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70438, NameID = 12677)]
public class BakoolJaJa(WorldState ws, Actor primary) : BossModule(ws, primary, new(500, -175), new ArenaBoundsCircle(25))
{
    protected override bool CheckPull() => true;
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) => hints.PrioritizeTargetsByOID(OID.Boss);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
    }
}
