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

    private readonly ColorTime[] _playerStates = [new ColorTime(), new ColorTime(), new ColorTime(), new ColorTime()];

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

class ScourgingBlaze(BossModule module) : Components.Exaflare(module, new AOEShapeCircle(5))
{
    private WDir _nextDir;

    private readonly List<(WPos Source, WDir Direction)> _orbs = [];

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ScourgingBlazeHorizontalFirst or AID.ScourgingBlazeHorizontalSecond)
            _nextDir = new WDir(1, 0);

        if ((AID)spell.Action.ID is AID.ScourgingBlazeVerticalFirst or AID.ScourgingBlazeVerticalSecond)
            _nextDir = new WDir(0, 1);

        if ((AID)spell.Action.ID == AID.CrystalAppear)
            _orbs.Add((spell.TargetXZ, _nextDir));

        if ((AID)spell.Action.ID is AID.ScourgingBlazeFirst)
        {
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

    public const float Radius = 1.5f; // todo verify

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var color = (IconID)iconID switch
        {
            IconID.RingLight => Color.Light,
            IconID.RingDark => Color.Dark,
            _ => Color.None
        };

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
                    hints.AddForbiddenZone(ShapeContains.Circle(a.Position, Radius));
            }

            if (_partners.Count > 0)
                hints.AddForbiddenZone(ShapeContains.Intersection(_partners));
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

    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        if (updateID == 0x80000026 && param2 == 9)
        {
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

class SearingChainCross : Components.GenericBaitAway
{
    public SearingChainCross(BossModule module) : base(module, AID._Spell_SearingChains, true, centerAtTarget: true)
    {
        foreach (var p in Raid.WithoutSlot())
            CurrentBaits.Add(new(Module.PrimaryActor, p, new AOEShapeCross(50, 3), WorldState.FutureTime(5.6f), IgnoreRotation: true));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (CurrentBaits.Count > 0)
        {
            foreach (var player in Raid.WithoutSlot().Exclude(actor))
                hints.AddForbiddenZone(ShapeContains.Cross(player.Position, default, 50, 3), CurrentBaits[0].Activation);

            hints.AddPredictedDamage(Raid.WithSlot().Mask(), CurrentBaits[0].Activation.AddSeconds(1.1f));
        }
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

    //private BitMask _windows = new(3);

    // complete guess
    public const float WindowHalfWidth = 2;

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
            var col = AtWindow(pc.Position) ? ArenaColor.Safe : ArenaColor.Danger;
            Arena.AddLine(new WPos(-586 + WindowHalfWidth, -284), new WPos(-586 - WindowHalfWidth, -284), col, 2);
            Arena.AddLine(new WPos(-600 + WindowHalfWidth, -284), new WPos(-600 - WindowHalfWidth, -284), col, 2);
            Arena.AddLine(new WPos(-614 + WindowHalfWidth, -284), new WPos(-614 - WindowHalfWidth, -284), col, 2);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (PlayerRoles[slot] is PlayerRole.Target or PlayerRole.TargetNotFirst)
            hints.Add("Aim for window!", !AtWindow(actor.Position));
    }

    private static bool AtWindow(WPos p) => MathF.Abs(p.X + 586) < WindowHalfWidth || MathF.Abs(p.X + 600) < WindowHalfWidth || MathF.Abs(p.X + 614) < WindowHalfWidth;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_Spinelash1)
        {
            NumCasts++;
            Source = null;
            _target = null;
        }
    }

    public override void Update()
    {
        if (_target != null && Source != null)
            Source.PosRot.X = _target.Position.X;
    }
}

class Vodoriga(BossModule module) : Components.Adds(module, (uint)OID.VodorigaMinion, 1);
class TerrorEye(BossModule module) : Components.StandardAOEs(module, AID._Spell_TerrorEye, 6);
class Eruption(BossModule module) : Components.StandardAOEs(module, AID._Spell_Eruption, 6);

class Q01TheFinalVerseStates : StateMachineBuilder
{
    //private readonly Q01TheFinalVerse _module;

    public Q01TheFinalVerseStates(Q01TheFinalVerse module) : base(module)
    {
        //_module = module;

        DeathPhase(0, Phase1)
            .ActivateOnEnter<LightDark>()
            .ActivateOnEnter<BossLightDark>()
            .ActivateOnEnter<Vodoriga>()
            .ActivateOnEnter<TerrorEye>();
    }

    private void Phase1(uint id)
    {
        CastStartMulti(id, [AID.ScourgingBlazeHorizontalFirst, AID.ScourgingBlazeVerticalFirst], 9.3f)
            .ActivateOnEnter<ScourgingBlaze>();

        ComponentCondition<BoundsOfSinBind>(id + 0x10, 24, b => b.NumCasts > 0, "Bind")
            .ActivateOnEnter<BoundsOfSinBind>()
            .ActivateOnEnter<BoundsOfSinInOut>()
            .ActivateOnEnter<BoundsOfSinIcicle>()
            .ActivateOnEnter<Neutralize>();

        ComponentCondition<BoundsOfSinInOut>(id + 0x20, 6.7f, b => b.NumCasts > 0, "In/out")
            .DeactivateOnExit<BoundsOfSinInOut>()
            .DeactivateOnExit<BoundsOfSinIcicle>();

        ComponentCondition<Neutralize>(id + 0x30, 0.3f, n => n.NumIcons == 0, "Light/dark")
            .ActivateOnEnter<BleedTower>()
            .DeactivateOnExit<Neutralize>()
            .ExecOnExit<BleedTower>(t => t.EnableHints = true);

        ComponentCondition<BleedTower>(id + 0x40, 5, t => t.Towers.Count == 0, "Towers");

        ComponentCondition<ScourgingBlaze>(id + 0x100, 7.1f, b => b.NumCasts > 0, "Exaflares start")
            .ActivateOnEnter<BladeOfFirstLight>()
            .ActivateOnEnter<BallOfFire>()
            .ActivateOnEnter<BallOfFireBait>();

        // 73.27 -> 78.815 (bind) 79.914 (cross)
        ComponentCondition<SearingChain>(id + 0x10000, 20.9f, s => s.TethersAssigned, "Chains appear")
            .ActivateOnEnter<SearingChain>();

        ComponentCondition<SearingChainCross>(id + 0x10100, 6.6f, s => s.NumCasts > 0, "Crosses")
            .ActivateOnEnter<SearingChainCross>()
            .ActivateOnEnter<Spinelash>();

        ComponentCondition<Spinelash>(id + 0x20000, 2.2f, s => s.Source != null, "Target select");
        ComponentCondition<Spinelash>(id + 0x20001, 10.5f, s => s.NumCasts > 0, "Wild charge")
            .DeactivateOnExit<Spinelash>();

        Timeout(id + 0xFF0000, 10000, "???")
            .ActivateOnEnter<Eruption>();
    }
}
