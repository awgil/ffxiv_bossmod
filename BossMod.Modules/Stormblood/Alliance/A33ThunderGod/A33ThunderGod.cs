#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Stormblood.Alliance.A33ThunderGod;

public enum OID : uint
{
    Boss = 0x25D7, // R19.880, x1
    Helper = 0x233C, // R0.500, x39, mixed types
    _Gen_EphemeralKnight = 0x25D8, // R7.100, x3
    _Gen_ = 0x261E, // R14.000, x3
    _Gen_Icewolf = 0x25D9, // R2.000, x0 (spawn during fight)

    ShadowbladePuddle = 0x1EAA54,
    DuskbladeTower = 0x1EAA55
}

public enum AID : uint
{
    _Weaponskill_Attack = 14151, // Boss->self, no cast, single-target
    _Weaponskill_Attack1 = 14152, // Helper->player, no cast, single-target
    _Weaponskill_CleansingStrike = 14161, // Boss->self, 3.0s cast, range 40 circle
    _Weaponskill_TGHolySword = 14153, // Boss->self, 6.0s cast, single-target
    _Weaponskill_HallowedBolt = 14155, // Helper->self, no cast, range 15 circle
    _Weaponskill_TGHolySword1 = 14160, // Boss->self, 6.0s cast, single-target
    _Weaponskill_JudgmentBlade = 15060, // Helper->self, no cast, range ?-35 donut
    _Weaponskill_Shadowblade = 14173, // Boss->self, 8.0s cast, single-target
    _Weaponskill_Shadowblade1 = 14174, // Helper->players, no cast, range 5 circle
    _Weaponskill_Shadowblade2 = 14175, // Helper->self, no cast, range 1 circle
    _Weaponskill_CrushHelm = 14163, // Helper->player, no cast, single-target
    _Weaponskill_CrushHelm1 = 14162, // Boss->self, 6.0s cast, single-target
    _Weaponskill_CrushHelm2 = 14164, // Helper->player, no cast, single-target
    _Weaponskill_Shadowblade3 = 14176, // Helper->self, no cast, range 70 circle
    _Weaponskill_TGHolySword2 = 14154, // Boss->self, 6.0s cast, single-target
    _Weaponskill_TGHolySword3 = 14159, // Boss->self, 6.0s cast, single-target
    _Weaponskill_NorthswainsStrike = 15059, // Helper->self, no cast, range 26 circle
    _Weaponskill_Duskblade = 14177, // Boss->self, 10.0s cast, range 70 circle
    _Weaponskill_CrushWeapon = 14165, // Boss->self, 4.0s cast, single-target
    _Weaponskill_CrushWeapon1 = 14166, // Helper->self, 4.0s cast, range 6 circle
    _Weaponskill_CrushWeapon2 = 14167, // Helper->self, no cast, range 6 circle
    _Weaponskill_Colosseum = 14178, // Boss->self, no cast, range 40 circle
    _AutoAttack_Attack = 870, // 25D8->player, no cast, single-target
    _Weaponskill_HallowedBolt1 = 14181, // 25D8->self, 5.0s cast, single-target
    _Weaponskill_HallowedBolt2 = 14182, // Helper->self, 5.0s cast, range 15 circle
    _Weaponskill_HallowedBolt3 = 14183, // Helper->self, 5.0s cast, range 15-30 donut
    _Weaponskill_HallowedBolt4 = 14184, // Helper->players, no cast, range 6 circle
    _Weaponskill_ = 14059, // Helper->self, no cast, single-target
    _Weaponskill_1 = 14372, // Helper->self, no cast, single-target
    _Weaponskill_BalanceAsunder = 14186, // Boss->self, no cast, single-target
    _Weaponskill_BalanceAsunder1 = 14187, // Helper->self, no cast, range 40 circle
    _Weaponskill_TGHolySword4 = 14156, // Boss->self, 8.0s cast, single-target
    _Weaponskill_DivineRuination = 14158, // Helper->self, no cast, range 16 circle
    _Weaponskill_CrushArmor = 14168, // Boss->self, 5.0s cast, single-target
    _Weaponskill_CrushArmor1 = 14169, // Helper->player, no cast, single-target
    _Weaponskill_CrushAccessory = 14171, // Helper->self, 8.0s cast, range 5-20 donut
    _Weaponskill_DivineRuination1 = 14179, // _Gen_EphemeralKnight->self, 5.0s cast, single-target
    _Weaponskill_CrushAccessory1 = 14170, // Boss->self, 8.0s cast, single-target
    _Weaponskill_DivineRuination2 = 14180, // Helper->self, no cast, range 70 width 6 rect
    _Weaponskill_TGHolySword5 = 14157, // Boss->self, 8.0s cast, single-target
    _Weaponskill_Burst = 14172, // _Gen_Icewolf->self, 24.0s cast, range 70 circle
}

public enum SID : uint
{
    _Gen_Doom = 1769, // Boss->player, extra=0x0
    _Gen_VulnerabilityUp = 202, // Helper->player, extra=0x1
    _Gen_PhysicalVulnerabilityUp = 200, // Helper->player, extra=0x1/0x2/0x3/0x4
    _Gen_Bleeding = 643, // Helper->player, extra=0x2
    _Gen_Weakness = 43, // none->player, extra=0x0
    _Gen_Transcendent = 418, // none->player, extra=0x0
    _Gen_Electrocution = 271, // Helper->player, extra=0x0
    _Gen_MagicVulnerabilityUp = 1138, // Helper->player, extra=0x0
    _Gen_PhysicalVulnerabilityUp1 = 695, // Helper->player, extra=0x0
}

public enum IconID : uint
{
    _Gen_Icon_m0537teg_a0h = 170, // player->self
    _Gen_Icon_tracking_lockon01i = 197, // player->self
    _Gen_Icon_lockon5_t0h = 23, // player->self
    _Gen_Icon_com_share0c = 62, // player->self
}

public enum TetherID : uint
{
    _Gen_Tether_chn_teke01h = 84, // Helper->player
}

class ShadowbladeBait(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(12), (uint)IconID._Gen_Icon_m0537teg_a0h, AID._Weaponskill_Shadowblade1, 8.2f, true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var b in CurrentBaits)
            if (b.Target == actor)
                hints.AddForbiddenZone(ShapeDistance.Circle(Arena.Center, 32), b.Activation);
    }
}

class ShadowbladePuddle(BossModule module) : Components.Voidzone(module, 12, OID.ShadowbladePuddle, m => m.TimelineState == 4);

class CrushHelm(BossModule module) : Components.CastCounter(module, AID._Weaponskill_CrushHelm2)
{
    DateTime TankbusterAt;

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (TankbusterAt != default)
            hints.Add("Tankbuster");
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_CrushHelm && TankbusterAt == default)
            TankbusterAt = WorldState.FutureTime(7.1f);

        if ((AID)spell.Action.ID == AID._Weaponskill_CrushHelm2)
            TankbusterAt = default;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (TankbusterAt != default)
            hints.AddPredictedDamage(Raid.WithSlot().WhereActor(a => a.Role == Role.Tank).Mask(), TankbusterAt, AIHints.PredictedDamageType.Tankbuster);
    }
}

class CrushHelmVuln(BossModule module) : BossComponent(module)
{
    BitMask CleanseTargets;
    DateTime TankbusterAt;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (TankbusterAt == default)
            return;

        if (CleanseTargets.Any())
            hints.Add("Cleanse tank(s)!", TankbusterAt < WorldState.FutureTime(2));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_CrushHelm && TankbusterAt == default)
            TankbusterAt = WorldState.FutureTime(7.1f);

        if ((AID)spell.Action.ID == AID._Weaponskill_CrushHelm2)
            TankbusterAt = default;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (TankbusterAt != default && TankbusterAt < WorldState.FutureTime(2.5f))
            hints.ShouldCleanse |= CleanseTargets;
    }

    public override void Update()
    {
        CleanseTargets.Reset();
        if (TankbusterAt != default)
            CleanseTargets = Raid.WithSlot().WhereActor(a => a.FindStatus(SID._Gen_PhysicalVulnerabilityUp, DateTime.MaxValue) != null && a.PendingDispels.Count == 0).Mask();
    }
}

class Duskblade(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_Duskblade);
class DuskbladeTower(BossModule module) : Components.GenericTowers(module)
{
    DateTime Activation;

    public override void OnActorTimelineStateChange(Actor actor, uint state)
    {
        if ((OID)actor.OID == OID.DuskbladeTower && actor.TimelineState == 1)
            Towers.Add(new(actor.Position, 3, 3, 8, activation: Activation));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_Duskblade)
        {
            Activation = Module.CastFinishAt(spell);
            foreach (ref var t in Towers.AsSpan())
                t.Activation = Activation;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_Duskblade)
            Towers.Clear();
    }
}

class CrushWeapon : Components.StandardChasingAOEs
{
    DateTime FirstCastStart;

    public CrushWeapon(BossModule module) : base(module, new AOEShapeCircle(6), AID._Weaponskill_CrushWeapon1, AID._Weaponskill_CrushWeapon2, 5, 0.7f, 4)
    {
        ExcludedTargets = new(~0ul);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID._Gen_Icon_tracking_lockon01i)
        {
            ExcludedTargets.Clear(Raid.FindSlot(actor.InstanceID));
            if (FirstCastStart == default)
                FirstCastStart = WorldState.FutureTime(4.1f);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (FirstCastStart != default)
            foreach (var (_, player) in Raid.WithSlot().ExcludedFromMask(ExcludedTargets))
                Arena.AddCircle(player.Position, 6, ArenaColor.Danger);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (spell.Action == ActionFirst)
            FirstCastStart = default;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        var home = A33ThunderGod.GetHomePos(Raid.Members[slot].Alliance);
        if (home == default)
            return;

        if (!ExcludedTargets[slot] && FirstCastStart != default)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(home, 3), FirstCastStart);
        else if (Chasers.Any(c => c.NumRemaining == 4 && c.Target == actor))
        {
            // i arbitrarily decree that the target should start on the party "home" space and dodge to the left, while the rest of the party stands on the right
            // (the standard pf strat is "good luck")
            var safeSide = (Arena.Center - home).Normalized().OrthoL();
            hints.AddForbiddenZone(ShapeDistance.HalfPlane(home, safeSide), DateTime.MaxValue);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if (spell.Action == ActionRest && Chasers.Count == 0)
            ExcludedTargets = new(~0ul);
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => FirstCastStart != default && !ExcludedTargets[playerSlot]
            ? PlayerPriority.Danger
            : base.CalcPriority(pcSlot, pc, playerSlot, player, ref customColor);
}

class Bounds(BossModule module) : BossComponent(module)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_Colosseum)
            Arena.Bounds = A33ThunderGod.IntermissionBounds;

        if ((AID)spell.Action.ID == AID._Weaponskill_BalanceAsunder1)
            Arena.Bounds = A33ThunderGod.DefaultBounds;
    }
}

class EphemeralKnight(BossModule module) : Components.Adds(module, (uint)OID._Gen_EphemeralKnight);

class HallowedBoltInner(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_HallowedBolt2, 15);
class HallowedBoltOuter(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_HallowedBolt3, new AOEShapeDonut(15, 30));
class HallowedBoltStack(BossModule module) : Components.StackWithIcon(module, (uint)IconID._Gen_Icon_com_share0c, AID._Weaponskill_HallowedBolt4, 6, 5.1f, 6);
class BalanceAsunder(BossModule module) : Components.RaidwideInstant(module, AID._Weaponskill_BalanceAsunder1, 10.2f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if ((AID)spell.Action.ID == AID._Weaponskill_BalanceAsunder)
            Activation = WorldState.FutureTime(Delay);
    }
}

class CrushArmor(BossModule module) : BossComponent(module)
{
    DateTime NextHit;

    readonly DateTime[] ExpireAt = new DateTime[PartyState.MaxAllianceSize];

    readonly List<(Actor From, Actor To)> Tethers = [];

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID._Gen_Tether_chn_teke01h)
        {
            if (NextHit == default)
                NextHit = WorldState.FutureTime(5.1f);

            if (WorldState.Actors.Find(tether.Target) is { } target)
                Tethers.Add((source, target));
        }
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID._Gen_Tether_chn_teke01h)
        {
            Tethers.RemoveAll(t => t.From == source);
            if (Tethers.Count == 0)
                NextHit = default;
        }
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_PhysicalVulnerabilityUp1 && Raid.TryFindSlot(actor, out var slot))
            ExpireAt[slot] = status.ExpireAt;
    }

    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_PhysicalVulnerabilityUp1 && Raid.TryFindSlot(actor, out var slot))
            ExpireAt[slot] = default;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Tethers.Any(t => t.To == actor) && ExpireAt[slot] != default)
            hints.Add("Pass tether!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var (from, to) in Tethers)
        {
            if (Arena.Config.ShowOutlinesAndShadows)
                Arena.AddLine(from.Position, to.Position, 0xff000000, 2);
            Arena.AddLine(from.Position, to.Position, ArenaColor.Danger);
        }
    }
}

// TODO: ice voidzone
class CrushAccessory(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_CrushAccessory, new AOEShapeDonut(5, 20));

class Icewolf(BossModule module) : Components.Adds(module, (uint)OID._Gen_Icewolf);

class DR(BossModule module) : Components.DebugCasts(module, [AID._Weaponskill_DivineRuination], new AOEShapeCircle(16));

class A33ThunderGodStates : StateMachineBuilder
{
    public A33ThunderGodStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Bounds>()
            .ActivateOnEnter<TGHallowedBolt>()
            .ActivateOnEnter<TGJudgmentBlade>()
            .ActivateOnEnter<TGNorthswainsStrike>()
            .ActivateOnEnter<TGDivineRuination>()
            .ActivateOnEnter<ShadowbladeBait>()
            .ActivateOnEnter<ShadowbladePuddle>()
            .ActivateOnEnter<CrushHelm>()
            .ActivateOnEnter<CrushHelmVuln>()
            .ActivateOnEnter<Duskblade>()
            .ActivateOnEnter<DuskbladeTower>()
            .ActivateOnEnter<CrushWeapon>()
            .ActivateOnEnter<EphemeralKnight>()
            .ActivateOnEnter<HallowedBoltInner>()
            .ActivateOnEnter<HallowedBoltOuter>()
            .ActivateOnEnter<HallowedBoltStack>()
            .ActivateOnEnter<BalanceAsunder>()
            .ActivateOnEnter<CrushArmor>()
            .ActivateOnEnter<CrushAccessory>()
            .ActivateOnEnter<Icewolf>()
            .Raw.Update = () => Module.PrimaryActor is { HPMP.CurHP: 1, IsTargetable: false };
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 636, NameID = 7899)]
public class A33ThunderGod(ModuleInit init) : BossModule(init, new(-600, -600), DefaultBounds)
{
    public static readonly ArenaBoundsCustom DefaultBounds = MakeBounds();
    public static readonly ArenaBounds IntermissionBounds = new ArenaBoundsCircle(35.5f);

    static ArenaBoundsCustom MakeBounds()
    {
        var platform = CurveApprox.Circle(9.6f, 0.05f);
        var starting = 90;
        var clipper = new PolygonClipper();
        var platforms = new RelSimplifiedComplexPolygon();
        for (var i = 0; i < 6; i++)
            platforms.Parts.Add(new([.. platform.Select(p => p + (starting + 60 * i).Degrees().ToDirection() * 25)]));

        // please avert your eyes
        List<WPos> bridgeAbs = [
            new(-600, -579.7298f),
            new(-598.51733f, -579.9248f), new(-597.07306f, -581.2062f),
            new(-596, -573.1124f), new(-597.7342f, -573.97614f),
            new(-600, -573.67786f),
            new(-602.2658f, -573.97614f), new(-604, -573.1124f),
            new(-602.92694f, -581.2062f), new(-601.48267f, -579.9248f)
        ];

        var b23 = bridgeAbs[2] - bridgeAbs[1];
        bridgeAbs[2] = bridgeAbs[1] + b23 * 2;
        var b43 = bridgeAbs[3] - bridgeAbs[4];
        bridgeAbs[3] = bridgeAbs[4] + b43 * 2;
        var b67 = bridgeAbs[7] - bridgeAbs[6];
        bridgeAbs[7] = bridgeAbs[6] + b67 * 2;
        var b98 = bridgeAbs[8] - bridgeAbs[9];
        bridgeAbs[8] = bridgeAbs[9] + b98 * 2;

        bridgeAbs.Reverse();

        var bridgeS = ShrinkPoly.Shrink(bridgeAbs.Select(s => s - new WPos(-600, -600)), 0.4f);

        for (var i = 0; i < 6; i++)
            platforms = clipper.Union(new(platforms), new(bridgeS.Select(p => p.Rotate((60 * i).Degrees()))));

        return new(34.5f, platforms);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.CalculateModuleAIHints(slot, actor, assignment, hints);

        var home = GetHomePos(Raid.Members[slot].Alliance);
        if (home != default)
            hints.GoalZones.Add(AIHints.GoalSingleTarget(home, 22, 0.5f));
    }

    public static WPos GetHomePos(AllianceLetter l)
    {
        var center = new WPos(-600, -600);
        float? angle = l switch
        {
            AllianceLetter.A => -120,
            AllianceLetter.B => 0,
            AllianceLetter.C => 120,
            _ => null
        };
        if (angle.HasValue)
            return center + angle.Value.Degrees().ToDirection() * 22;

        return default;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        if (PrimaryActor.IsTargetable)
            Arena.ActorInsideBounds(PrimaryActor.Position, PrimaryActor.Rotation, ArenaColor.Enemy);
    }
}
