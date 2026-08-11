#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Dawntrail.Foray.ForkedTower.FT11TwoHeadedAevis;

public enum OID : uint
{
    Boss = 0x4C11, // R18.000, x1
    GreenHead = 0x4C12, // R15.000, x1
    BlueHead = 0x4C13, // R15.000, x1
    GreenHeadAutos = 0x4C14, // R1.000, x1
    BlueHeadAutos = 0x4C15, // R1.000, x1
    Helper = 0x233C, // R0.500, x16, Helper type
    TetherHelper = 0x4C24, // R1.000, x2
    BallLightning = 0x4C16, // R2.400, x0 (spawn during fight)
    SwirlingOrb = 0x4C17, // R2.800, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_ = 47753, // 4C14->player, no cast, single-target
    _AutoAttack_1 = 47754, // 4C15->player, no cast, single-target
    _Ability_ = 50710, // Helper->player, no cast, single-target
    _Ability_1 = 50709, // Helper->player, no cast, single-target
    _Ability_Buffet = 49726, // Boss/4C12->self, 5.0s cast, single-target
    _Ability_2 = 49727, // 4C11->self, 5.0s cast, single-target
    _Weaponskill_Aethersplit = 48642, // 4C14->4C15, no cast, single-target
    _Ability_3 = 47615, // 4C11->self, 7.2+0.8s cast, single-target
    _Ability_PoisonBreath = 50715, // Boss->self, 8.0s cast, single-target
    _Ability_PoisonBreath1 = 47617, // Helper->location, 8.0s cast, range 18 circle
    _Ability_4 = 47614, // 4C11->self, 7.2+0.8s cast, single-target
    _Ability_5 = 48243, // Helper->location, 8.0s cast, range 30 circle
    _Ability_StormsBreath = 47616, // Helper->location, 8.0s cast, ???
    _Ability_StormsBreath1 = 47613, // 4C12->self, 8.0s cast, single-target
    _Ability_ThunderfrostTempest = 47735, // Boss/4C12->self, 5.0s cast, single-target
    _Ability_6 = 47736, // 4C11->self, 5.0s cast, single-target
    _Ability_ThunderfrostTempest1 = 47738, // Helper->self, no cast, ???, ice raidwide
    _Ability_ThunderfrostTempest2 = 47737, // Helper->self, no cast, ???, thunder raidwide
    _Ability_7 = 50656, // 4C11->self, 5.0s cast, single-target
    _Ability_TwoTerrors = 50655, // Boss/4C12->self, 6.0s cast, single-target
    _Ability_TwoTerrors1 = 50658, // Helper->self, 6.0s cast, range 40 width 10 rect
    _Ability_8 = 50657, // 4C11->self, 5.0s cast, single-target
    _Ability_HissingReprise = 49722, // Boss/4C12->self, 3.0s cast, single-target
    _Ability_9 = 49723, // 4C11->self, 3.0s cast, single-target
    _Ability_Buffet1 = 49724, // Helper->self, no cast, ???
    _Ability_Buffet2 = 49725, // Helper->self, no cast, ???
    _Ability_Summon = 47704, // Boss/4C12->self, 3.0s cast, single-target
    _Ability_10 = 47705, // 4C11->self, 3.0s cast, single-target
    _Ability_11 = 47643, // 4C11->self, 7.4s cast, single-target
    _Ability_IceCluster = 50698, // Helper->location, 8.0s cast, range 15 circle
    _Ability_IceCluster1 = 48220, // Boss->self, 8.0s cast, single-target
    _Ability_LightningCluster = 47644, // 4C14->location, 8.0s cast, single-target
    _Ability_IceCluster2 = 47645, // 4C15->location, 8.0s cast, single-target
    _Ability_LightningCluster1 = 47642, // 4C12->self, 8.0s cast, single-target
    _Ability_LightningCluster2 = 50697, // Helper->location, 8.0s cast, range 15 circle
    _Ability_Shock = 47706, // 4C16->self, 2.0s cast, range 15 circle
    _Ability_HypothermalCombustion = 47707, // 4C17->self, 2.0s cast, range 15 circle
    _Ability_12 = 47655, // 4C11->self, 5.3s cast, single-target
    _Ability_Blaze = 47659, // 4C14->location, 6.0s cast, single-target
    _Ability_Blaze1 = 50703, // Helper->location, 6.0s cast, range 5 circle
    _Ability_Blazeloop = 47654, // 4C12->self, 6.0s cast, single-target
    _Ability_Blazeloop1 = 47660, // Helper->self, 2.5s cast, range 5-60 donut
    _Ability_Blaze2 = 47663, // 4C14->location, 6.0s cast, single-target
    _Ability_Blaze3 = 50704, // Helper->location, 6.0s cast, range 5 circle
    _Ability_Blazeloop2 = 47661, // 4C12->self, 6.0s cast, single-target
    _Ability_Blazeloop3 = 47662, // Boss->self, 5.3+0.7s cast, single-target
    _Ability_13 = 47658, // 4C11->self, no cast, single-target
    _Ability_Blaze4 = 50705, // Helper->location, 6.0s cast, range 5 circle
    _Ability_Blaze5 = 47664, // 4C15->location, 6.0s cast, single-target
    _Ability_ArcaneRevelation = 49716, // Boss/4C12->self, 3.0s cast, single-target
    _Ability_14 = 49717, // 4C11->self, 3.0s cast, single-target
}

public enum SID : uint
{
    _Gen_EpicHero = 4192, // none->player, extra=0x0
    _Gen_FatedVillain = 5401, // none->Boss, extra=0x0
    _Gen_FatedHero = 4194, // none->player, extra=0x0
    _Gen_EpicVillain = 5400, // none->4C12, extra=0x0
    _Gen_ = 2552, // none->4C11, extra=0x471/0x470
    _Gen_VulnerabilityUp = 2347, // Helper/4C16/4C17->player, extra=0x1/0x2/0x3/0x4/0x5
    _Gen_EasterlyReprise = 5403, // none->player, extra=0x0
    _Gen_WesterlyReprise = 5404, // none->player, extra=0x0
    _Gen_Weakness = 43, // none->player, extra=0x0
    _Gen_Transcendent = 418, // none->player, extra=0x0
    _Gen_BattleHigh = 4229, // none->player, extra=0x0
}

public enum IconID : uint
{
    _Gen_Icon_m0811trg02t0a1 = 585, // player->self
}

public enum TetherID : uint
{
    _Gen_Tether_chn_tergetfix2k1 = 429, // player->4C15/4C14
    _Gen_Tether_chn_m0560_0t2 = 411, // 4C15/4C14->4C24
    _Gen_Tether_chn_m0560_elc_0t2 = 412, // 4C14->4C24
    _Gen_Tether_chn_m0560_ice_0t2 = 413, // 4C15->4C24
}

class Heads(BossModule module) : Components.AddsMulti(module, [OID.GreenHead, OID.BlueHead]);
class DecisiveBattle(BossModule module) : Components.GenericInvincible(module)
{
    readonly int[] PlayerStates = Utils.MakeArray(8, -1);
    readonly Actor?[] Bosses = [null, null];

    protected override IEnumerable<Actor> ForbiddenTargets(int slot, Actor actor)
    {
        var ix = PlayerStates.BoundSafeAt(slot, -1);
        if (ix == 0 && Bosses[1] != null)
            yield return Bosses[1]!;
        if (ix == 1 && Bosses[0] != null)
            yield return Bosses[0]!;
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID._Gen_EpicHero:
                if (Raid.TryFindSlot(actor, out var slot))
                    PlayerStates[slot] = 0;
                break;
            case SID._Gen_FatedHero:
                if (Raid.TryFindSlot(actor, out slot))
                    PlayerStates[slot] = 1;
                break;
            case SID._Gen_EpicVillain:
                Bosses[0] = actor;
                break;
            case SID._Gen_FatedVillain:
                Bosses[1] = actor;
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID._Gen_EpicHero:
            case SID._Gen_FatedHero:
                if (Raid.TryFindSlot(actor, out var slot))
                    PlayerStates[slot] = -1;
                break;
            case SID._Gen_EpicVillain:
            case SID._Gen_FatedVillain:
                Array.Fill(Bosses, null);
                break;
        }
    }
}

class PoisonBreath(BossModule module) : Components.StandardAOEs(module, AID._Ability_PoisonBreath1, 18);

class ThunderfrostTempest(BossModule module) : Components.RaidwideCastDelay(module, AID._Ability_ThunderfrostTempest, AID._Ability_ThunderfrostTempest1, 0.8f);

class TwoTerrors(BossModule module) : Components.StandardAOEs(module, AID._Ability_TwoTerrors1, new AOEShapeRect(40, 5));

// in one component because it works better this way (i hate everything)
class Knockbacks(BossModule module) : Components.Knockback(module)
{
    record struct PlayerState(WDir Direction, DateTime Activation);

    readonly PlayerState[] _playerStates = new PlayerState[8];

    Actor? _stormsBreathCaster;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        var st = _playerStates[slot];
        if (st.Direction != default)
            yield return new(Arena.Center - st.Direction * 20, 20, st.Activation, null, st.Direction.ToAngle(), Kind.DirForward);

        if (_stormsBreathCaster is { } sb)
            yield return new(sb.CastInfo!.LocXZ, 14, Module.CastFinishAt(sb.CastInfo), Kind: Kind.AwayFromOrigin);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        WDir dir = (SID)status.ID switch
        {
            SID._Gen_EasterlyReprise => new(-1, 0),
            SID._Gen_WesterlyReprise => new(1, 0),
            _ => default
        };

        if (dir != default && Raid.TryFindSlot(actor, out var slot))
            _playerStates[slot] = new(dir, status.ExpireAt);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Ability_StormsBreath)
            _stormsBreathCaster = caster;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Ability_Buffet1 or AID._Ability_Buffet2)
            Array.Fill(_playerStates, default);

        if ((AID)spell.Action.ID == AID._Ability_StormsBreath)
            _stormsBreathCaster = null;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var src in Sources(slot, actor).Where(s => !IsImmune(slot, s.Activation)).Take(1))
        {
            if (src.Kind == Kind.DirForward)
            {
                hints.AddForbiddenZone(ShapeDistance.Rect(Arena.Center, src.Direction, 20, 5, 20), src.Activation);
                hints.AddForbiddenZone(ShapeDistance.InvertedRect(Arena.Center, default(Angle), 15, 15, 15), src.Activation);
            }

            if (src.Kind == Kind.AwayFromOrigin)
            {
                var inv = ShapeDistance.InvertedRect(Arena.Center, default(Angle), 20, 20, 20);
                var orig = src.Origin;
                hints.AddForbiddenZone(p =>
                {
                    var dir = (p - orig).Normalized() * 14;
                    return inv(p + dir);
                }, src.Activation);
            }
        }
    }
}

class TwoHeadedAevisStates : StateMachineBuilder
{
    public TwoHeadedAevisStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Heads>()
            .ActivateOnEnter<DecisiveBattle>()
            .ActivateOnEnter<PoisonBreath>()
            .ActivateOnEnter<ThunderfrostTempest>()
            .ActivateOnEnter<TwoTerrors>()
            .ActivateOnEnter<Knockbacks>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14489)]
public class TwoHeadedAevis(WorldState ws, Actor primary) : BossModule(ws, primary, new(-900, 700), new ArenaBoundsSquare(20))
{
    protected override bool CheckPull() => PrimaryActor.InCombat;
}
