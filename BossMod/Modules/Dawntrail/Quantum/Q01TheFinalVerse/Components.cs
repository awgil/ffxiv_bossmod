namespace BossMod.Dawntrail.Quantum.Q01TheFinalVerse;

[Flags]
public enum Color
{
    None,
    Dark,
    Light
}

public class ColorTime
{
    public Color Color;
    public DateTime Expire;
}

class LightDark(BossModule module) : BossComponent(module)
{
    private readonly ColorTime[] _playerStates = Utils.GenArray(4, () => new ColorTime());

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.DarkVengeance && Raid.TryFindSlot(actor, out var slot))
        {
            _playerStates[slot].Color |= Color.Dark;
            _playerStates[slot].Expire = status.ExpireAt;
        }

        if ((SID)status.ID == SID.LightVengeance && Raid.TryFindSlot(actor, out var slot2))
        {
            _playerStates[slot2].Color |= Color.Light;
            _playerStates[slot2].Expire = status.ExpireAt;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.DarkVengeance && Raid.TryFindSlot(actor, out var slot))
            _playerStates[slot].Color ^= Color.Dark;

        if ((SID)status.ID == SID.LightVengeance && Raid.TryFindSlot(actor, out var slot2))
            _playerStates[slot2].Color ^= Color.Light;
    }

    public Color GetColor(int slot) => _playerStates.BoundSafeAt(slot)?.Color ?? Color.None;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_playerStates[slot].Color != Color.None)
        {
            var timer = (_playerStates[slot].Expire - WorldState.CurrentTime).TotalSeconds;
            if (timer < 10)
                hints.Add($"Buff remaining: {timer:f1}s");
        }
    }
}

class BossLightDark(BossModule module) : Components.GenericInvincible(module)
{
    private readonly LightDark _lightDark = module.FindComponent<LightDark>()!;

    private Actor? Eater;

    protected override IEnumerable<Actor> ForbiddenTargets(int slot, Actor actor)
    {
        // personal preference, don't like seeing the radar turn red when i die
        if (actor.IsDead)
            yield break;

        var color = _lightDark.GetColor(slot);
        if (!color.HasFlag(Color.Dark) && Eater != null)
            yield return Eater;
        if (!color.HasFlag(Color.Light))
            yield return Module.PrimaryActor;
    }

    public override void Update()
    {
        Eater ??= Module.Enemies(OID.DevouredEater).FirstOrDefault();
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CalcHPDifference() is var (boss, pct) && pct < 25)
            hints.Add($"HP: {boss} +{pct:f1}%");
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (CalcHPDifference() is var (boss, pct) && pct >= 25)
            hints.Add($"HP: {boss} +{pct:f1}%");
    }

    private (string Boss, float Percent)? CalcHPDifference()
    {
        if (Eater is { } e && Module.PrimaryActor.HPMP.CurHP > 1 && e.HPMP.CurHP > 1)
        {
            var ratioDiff = Module.PrimaryActor.HPRatio - e.HPRatio;
            return (ratioDiff > 0 ? "Eminent Grief" : "Devoured Eater", MathF.Abs(ratioDiff) * 100);
        }

        return null;
    }
}

class ChainsOfCondemnation(BossModule module) : Components.StayMove(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ChainsOfCondemnationFast or AID.ChainsOfCondemnationSlow)
            Array.Fill(PlayerStates, new PlayerState(Requirement.NoMove, Module.CastFinishAt(spell)));
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.ChainsOfCondemnation && Raid.TryFindSlot(actor, out var slot))
            SetState(slot, new PlayerState(Requirement.NoMove, WorldState.CurrentTime));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.ChainsOfCondemnation && Raid.TryFindSlot(actor, out var slot))
            ClearState(slot);
    }
}

class ScourgingBlaze(BossModule module) : Components.Exaflare(module, new AOEShapeCircle(5))
{
    private WDir _nextDir;
    private WPos? _safeSpot;

    public bool Draw;

    private readonly List<(WPos Source, WDir Direction)> _orbs = [];

    private readonly WPos[] _safeSpots = [new(-605.75f, -287), new(-594.25f, -287), new(-605.75f, -313), new(-594.25f, -313)];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Draw ? base.ActiveAOEs(slot, actor) : [];

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ScourgingBlazeHorizontalFirst or AID.ScourgingBlazeHorizontalSecond)
            _nextDir = new WDir(1, 0);

        if ((AID)spell.Action.ID is AID.ScourgingBlazeVerticalFirst or AID.ScourgingBlazeVerticalSecond)
            _nextDir = new WDir(0, 1);

        if ((AID)spell.Action.ID == AID.CrystalAppear)
        {
            _orbs.Add((spell.TargetXZ, _nextDir));
            CalcSafespot();
        }

        if ((AID)spell.Action.ID is AID.ScourgingBlazeFirst)
        {
            _safeSpot = null;
            NumCasts++;
            var lines = Lines.Where(l => l.Next.InCircle(spell.TargetXZ, 1));
            foreach (var l in lines)
                AdvanceLine(l, spell.TargetXZ);
        }

        if ((AID)spell.Action.ID is AID.ScourgingBlazeRest)
        {
            NumCasts++;
            var ix = Lines.FindIndex(l => l.Next.AlmostEqual(spell.TargetXZ, 0.5f));
            if (ix >= 0)
                AdvanceLine(Lines[ix], spell.TargetXZ);
            else
                ReportError($"unrecognized exaflare at {spell.TargetXZ}");

            Lines.RemoveAll(l => l.ExplosionsLeft < 1);
            if (Lines.Count == 0)
            {
                Draw = false;
                NumCasts = 0;
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ScourgingBlazeHorizontalFirst or AID.ScourgingBlazeVerticalFirst)
        {
            _orbs.Clear();
            Lines.Clear();
        }

        if ((AID)spell.Action.ID == AID.ScourgingBlazeFirst)
        {
            foreach (var orb in _orbs)
            {
                if (orb.Source.InCircle(spell.LocXZ, 1))
                {
                    AddLine(spell.LocXZ, orb.Direction * 4, Module.CastFinishAt(spell));
                    AddLine(spell.LocXZ, orb.Direction * -4, Module.CastFinishAt(spell));
                }
            }
        }
    }

    private void AddLine(WPos source, WDir advance, DateTime next)
    {
        var numExplosions = 0;
        var tmp = source;
        while (tmp.InRect(Arena.Center, 0.Degrees(), 14.5f, 14.5f, 19.5f))
        {
            tmp += advance;
            numExplosions++;
        }

        Lines.Add(new()
        {
            Next = source,
            Advance = advance,
            NextExplosion = next,
            TimeToMove = 1.2f,
            ExplosionsLeft = numExplosions,
            MaxShownExplosions = Math.Min(5, numExplosions)
        });
    }

    private void CalcSafespot()
    {
        var candidates = _safeSpots.ToList();
        foreach (var orb in _orbs)
            candidates.RemoveAll(c => c.InRect(orb.Source, orb.Direction, 60, 60, 5));

        if (candidates.Count == 1)
            _safeSpot = candidates[0];
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (!Draw && _safeSpot is { } spot)
            Arena.AddCircle(spot, 0.75f, ArenaColor.Safe, 2);
    }
}

class BoundsOfSinInOut(BossModule module) : Components.GenericAOEs(module)
{
    private bool _active;
    private bool _donut;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_active)
        {
            if (_donut)
                yield return new(new AOEShapeDonut(8, 30), Arena.Center);
            else
                yield return new(new AOEShapeCircle(8), Arena.Center);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (!_active && (AID)spell.Action.ID == AID.BoundsOfSinIcicleDrop)
        {
            var dir = caster.DirectionTo(Arena.Center);
            _donut = dir.Dot(caster.Rotation.ToDirection()) < 0;
            _active = true;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.BoundsOfSinJailInside or AID.BoundsOfSinJailOutside)
        {
            NumCasts++;
            _active = false;
        }
    }
}

class BoundsOfSinIcicle(BossModule module) : Components.StandardAOEs(module, AID.BoundsOfSinIcicleDrop, 3, maxCasts: 10);

class BoundsOfSinBind(BossModule module) : Components.CastCounter(module, AID.BoundsOfSinBind);

class Neutralize(BossModule module) : BossComponent(module)
{
    private readonly Color[] _colors = new Color[4];
    public int NumIcons { get; private set; }

    // random guess
    private DateTime _resolve;

    public const float Radius = 1.5f; // todo verify

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var color = (IconID)iconID switch
        {
            IconID.RingLight => Color.Light,
            IconID.RingDark => Color.Dark,
            _ => Color.None
        };

        if (_resolve == default)
            _resolve = WorldState.FutureTime(5.1f);

        if (color != Color.None && Raid.TryFindSlot(actor, out var slot))
        {
            _colors[slot] = color;
            NumIcons++;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_colors[slot] != default)
        {
            var stacked = Raid.WithSlot().InRadiusExcluding(actor, Radius).ToList();

            if (stacked.Any(s => _colors[s.Item1] == _colors[slot]))
                hints.Add("GTFO from same color!");
            else
                hints.Add("Stack with opposite color!", !stacked.Any(s => _colors[s.Item1] != _colors[slot]));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_colors[slot] != default)
        {
            List<Func<WPos, bool>> _partners = [];

            foreach (var (s, a) in Raid.WithSlot().Exclude(actor))
            {
                if (_colors[s] != _colors[slot])
                    _partners.Add(ShapeContains.Donut(a.Position, Radius, 60));
                else
                    hints.AddForbiddenZone(ShapeContains.Circle(a.Position, Radius), _resolve);
            }

            if (_partners.Count > 0)
                hints.AddForbiddenZone(ShapeContains.Intersection(_partners), _resolve);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_colors[pcSlot] != default)
        {
            Arena.AddCircle(pc.Position, Radius, ArenaColor.Safe);

            foreach (var (slot, player) in Raid.WithSlot().Exclude(pc))
            {
                if (_colors[slot] == _colors[pcSlot])
                    Arena.AddCircle(player.Position, Radius, ArenaColor.Danger);
            }
        }
    }

    public override void Update()
    {
        if (_resolve != default && _resolve <= WorldState.CurrentTime)
        {
            _resolve = default;
            Array.Fill(_colors, default);
            NumIcons = 0;
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => _colors[pcSlot] == default ? PlayerPriority.Normal : _colors[pcSlot] == _colors[playerSlot] ? PlayerPriority.Danger : PlayerPriority.Interesting;
}

class BleedTower : Components.GenericTowers
{
    private static readonly WPos[] _towerPositions = [
        new(-607.78f, -307.78f),
        new(-592.22f, -307.78f),
        new(-607.78f, -292.22f),
        new(-592.22f, -292.22f),
    ];

    private BitMask _forbiddenPlayers;

    public BleedTower(BossModule module) : base(module)
    {
        EnableHints = false;

        _forbiddenPlayers = Raid.WithSlot().WhereActor(a => a.FindStatus(SID.DarkVengeance) != null).Mask();

        Towers.AddRange(_towerPositions.Select(p => new Tower(p, 2, forbiddenSoakers: _forbiddenPlayers)));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (EnableHints && _forbiddenPlayers[slot])
            hints.Add("Get light debuff!");
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.LightVengeance && Raid.TryFindSlot(actor, out var slot))
        {
            _forbiddenPlayers.Clear(slot);
            UpdateMask();
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.LightVengeance && Raid.TryFindSlot(actor, out var slot))
        {
            _forbiddenPlayers.Set(slot);
            UpdateMask();
        }
    }

    private void UpdateMask()
    {
        for (var i = 0; i < Towers.Count; i++)
            Towers.Ref(i).ForbiddenSoakers = _forbiddenPlayers;
    }

    public override void OnMapEffect(byte index, uint state)
    {
        var off = index - 0x1B;
        if (off is >= 0 and < 4)
        {
            if (state == 0x00080004)
                Towers.RemoveAll(t => t.Position == _towerPositions[off]);
        }
    }
}

class BladeOfFirstLight(BossModule module) : Components.GroupedAOEs(module, [AID.BladeOfFirstLightFast, AID.BladeOfFirstLightSlow], new AOEShapeRect(30, 7.5f));

class BallOfFireBait(BossModule module) : BossComponent(module)
{
    private bool _active;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.BallOfFireCastFast:
            case AID.BallOfFireCastSlow:
                _active = true;
                break;
            case AID.BallOfFirePuddle:
                _active = false;
                break;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_active)
            Arena.AddCircle(pc.Position, 6, ArenaColor.Danger);
    }
}

class BallOfFire(BossModule module) : Components.StandardAOEs(module, AID.BallOfFirePuddle, 6);

class SearingChain(BossModule module) : Components.Chains(module, (uint)TetherID._Gen_Tether_chn_hfchain1f, chainLength: 20, activationDelay: 5);

class SearingChainCross(BossModule module) : Components.GenericBaitAway(module, AID._Spell_SearingChains, true, centerAtTarget: true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (CurrentBaits.Count > 0)
        {
            foreach (var player in Raid.WithoutSlot().Exclude(actor))
                hints.AddForbiddenZone(ShapeContains.Cross(player.Position, default, 50, 3), CurrentBaits[0].Activation);

            hints.AddPredictedDamage(Raid.WithSlot().Mask(), CurrentBaits[0].Activation.AddSeconds(1.1f));
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.SearingChains)
            CurrentBaits.Add(new(Module.PrimaryActor, actor, new AOEShapeCross(50, 3), WorldState.FutureTime(5.6f), IgnoreRotation: true));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Spell_SearingChains)
        {
            NumCasts++;
            CurrentBaits.Clear();
        }
    }
}

class Spinelash(BossModule module) : Components.GenericWildCharge(module, 4, null, 60)
{
    private Actor? _target;

    public BitMask IntactWindows = BitMask.Build(0, 1, 2);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID._Gen_Icon_lockon5_t0h && Raid.TryFindSlot(actor, out var slot))
        {
            _target = actor;
            Activation = WorldState.FutureTime(10.5f);
            PlayerRoles[slot] = actor.Role == Role.Tank ? PlayerRole.Target : PlayerRole.TargetNotFirst;
            foreach (var (s, p) in Raid.WithSlot().Exclude(actor))
                PlayerRoles[s] = p.Role == Role.Tank ? PlayerRole.Share : PlayerRole.ShareNotFirst;

            // TODO: refactor GenericWildCharge so this isn't necessary :(
            Source = new Actor(1, 0, 819, 0, "fake actor", 0, ActorType.Enemy, Class.ACN, 1, Module.PrimaryActor.PosRot with { X = actor.Position.X });
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (PlayerRoles[pcSlot] is PlayerRole.Target or PlayerRole.TargetNotFirst)
        {
            var col = AimedAtWindow(pc.Position) ? ArenaColor.Safe : ArenaColor.Danger;
            foreach (var bit in IntactWindows.SetBits())
                Arena.AddLine(new WPos(WindowCenterX(bit) + 7, -284), new WPos(WindowCenterX(bit) - 7, -284), col, 2);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (PlayerRoles[slot] is PlayerRole.Target or PlayerRole.TargetNotFirst)
            hints.Add("Aim for window!", !AimedAtWindow(actor.Position));
    }

    private float WindowCenterX(int windowBit) => -614 + windowBit * 14;

    private bool AimedAtWindow(WPos pos)
    {
        foreach (var win in IntactWindows.SetBits())
            if (MathF.Abs(pos.X - WindowCenterX(win)) < 7)
                return true;

        return false;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_Spinelash1)
        {
            NumCasts++;
            Source = null;
            _target = null;
            Array.Fill(PlayerRoles, PlayerRole.Ignore);
        }
    }

    public override void Update()
    {
        if (_target != null && Source != null)
            Source.PosRot.X = _target.Position.X;
    }

    public override void OnMapEffect(byte index, uint state)
    {
        var window = index - 0x18;
        if (window is >= 0 and <= 2 && state == 0x00020001)
            IntactWindows.Clear(window);
    }
}

class Vodoriga(BossModule module) : Components.Adds(module, (uint)OID.VodorigaMinion, 1);
class TerrorEye(BossModule module) : Components.StandardAOEs(module, AID._Spell_TerrorEye, 6);
class Eruption(BossModule module) : Components.StandardAOEs(module, AID._Spell_Eruption, 6);

class BladeHints(BossModule module) : BossComponent(module)
{
    private readonly string?[] _hints = new string?[2];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.BladeOfFirstLightInsideFast:
                _hints[0] = "Outside safe";
                break;
            case AID.BladeOfFirstLightInsideSlow:
                _hints[1] = "outside safe";
                break;
            case AID.BladeOfFirstLightOutsideFast:
                _hints[0] = "Inside safe";
                break;
            case AID.BladeOfFirstLightOutsideSlow:
                _hints[1] = "outside safe";
                break;

            case AID.BallOfFireCastFast:
                _hints[0] = "Baited puddles";
                break;
            case AID.BallOfFireCastSlow:
                _hints[1] = "baited puddles";
                break;

            case AID.ChainsOfCondemnationFast:
                _hints[0] = "Stay";
                break;
            case AID.ChainsOfCondemnationSlow:
                _hints[1] = "stay";
                break;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_hints[0] is { } h1 && _hints[1] is { } h2)
            hints.Add($"{h1} => {h2}", false);
    }
}

class ShackleSpreadHint(BossModule module) : Components.GenericStackSpread(module, alwaysShowSpreads: true, raidwideOnResolve: false)
{
    public bool Shackles { get; private set; }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Spell_ShacklesOfGreaterSanctity)
        {
            foreach (var player in Raid.WithoutSlot())
            {
                if (player.Role == Role.Healer)
                    Spreads.Add(new(player, 21, Module.CastFinishAt(spell)));
                else if (player.Role != Role.Tank)
                    Spreads.Add(new(player, 8, Module.CastFinishAt(spell)));
            }
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.ShackledHealing)
        {
            Shackles = true;
            Spreads.Clear();
        }
    }
}

// only draw spread on healer to reduce visual clutter
// DPS will naturally avoid healer (because of defam) and tank (because of jail)
class ShackleHint(BossModule module) : BossComponent(module)
{
    private Actor? Healer;
    public bool Expired;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.ShackledHealing)
            Healer = actor;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.ShackledHealing)
        {
            Healer = null;
            Expired = true;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Healer != null)
            Arena.AddCircle(Healer.Position, 21, ArenaColor.Danger);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor == Healer)
        {
            var count = Raid.WithoutSlot().InRadiusExcluding(actor, 21).Count();
            hints.Add($"Allies in radius: {count}", false);
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => player == Healer ? PlayerPriority.Interesting : base.CalcPriority(pcSlot, pc, playerSlot, player, ref customColor);
}

class ArcaneFont(BossModule module) : Components.Adds(module, (uint)OID.ArcaneFont)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var jail = actor.FindStatus(SID.HellishEarth) != null;

        foreach (var add in ActiveActors)
            hints.SetPriority(add, jail ? AIHints.Enemy.PriorityInvincible : 1);
    }
}

class HellishEarthPull(BossModule module) : Components.Knockback(module, ignoreImmunes: true)
{
    private Actor? Caster;
    private Actor? Target;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (Caster != null)
        {
            var activation = Module.CastFinishAt(Caster.CastInfo);

            if (actor == Target)
                yield return new(Caster.Position, 60, activation, Kind: Kind.TowardsOrigin);
            else if (!PlayerImmunes[slot].ImmuneAt(activation))
                yield return new(Caster.Position, 10, activation, Kind: Kind.TowardsOrigin);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor == Target)
        {
            if (actor.Role != Role.Tank)
                hints.Add("Too far from boss!");
        }
        else if (Target != null && actor.Role == Role.Tank)
            hints.Add("Go far to bait tether!");
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.HellishEarthPullTether)
            Caster = caster;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.HellishEarthPullTether or AID.HellishEarthPull)
        {
            Target = null;
            Caster = null;
            NumCasts++;
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID._Gen_Tether_chn_fire001f && WorldState.Actors.Find(tether.Target) is { } tar)
            Target = tar;
    }
}

class ManifoldLashingsTower(BossModule module) : Components.GenericTowers(module, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_ManifoldLashings1)
        {
            Towers.Add(new(caster.Position, 2, forbiddenSoakers: Raid.WithSlot().WhereActor(a => a.Role != Role.Tank).Mask(), activation: Module.CastFinishAt(spell)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_ManifoldLashings1 or AID._Weaponskill_ManifoldLashings2)
            NumCasts++;
    }
}
class ManifoldLashingsTail(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_ManifoldLashings3, new AOEShapeRect(42, 4.5f));

class UnholyDarkness(BossModule module) : Components.RaidwideCast(module, AID._Spell_UnholyDarkness1);

class DrainAether(BossModule module) : Components.CastCounterMulti(module, [AID.DrainAetherLightFast, AID.DrainAetherLightSlow, AID.DrainAetherDarkFast, AID.DrainAetherDarkSlow])
{
    private readonly LightDark _ld = module.FindComponent<LightDark>()!;
    private readonly List<(Color, DateTime)> _colors = [];
    private Actor? _sinBearer;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.SinBearer)
            _sinBearer = actor;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.SinBearer)
            _sinBearer = null;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            // eminent grief
            case AID.DrainAetherLightFast:
            case AID.DrainAetherLightSlow:
                _colors.Add((Color.Light, Module.CastFinishAt(spell)));
                _colors.SortBy(c => c.Item2);
                break;
            case AID.DrainAetherDarkFast:
            case AID.DrainAetherDarkSlow:
                _colors.Add((Color.Dark, Module.CastFinishAt(spell)));
                _colors.SortBy(c => c.Item2);
                break;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_colors.Count > 0)
        {
            var nextColor = _colors[0].Item1;
            if (nextColor == Color.Light && actor == _sinBearer)
                hints.Add($"Stay dark!", false);
            else
                hints.Add($"Correct color: {nextColor}", !_ld.GetColor(slot).HasFlag(nextColor));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (WatchedActions.Contains(spell.Action))
        {
            NumCasts++;
            if (_colors.Count > 0)
                _colors.RemoveAt(0);
        }
    }
}

class Flameborn(BossModule module) : Components.AddsPointless(module, 0x48F3);
