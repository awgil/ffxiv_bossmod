using Dalamud.Interface;

namespace BossMod.Dawntrail.Savage.RM12S2TheLindwurm;

class IdyllicDreamArena : Components.GenericAOEs
{
    readonly ArenaBoundsCustom PlatformBounds;
    readonly AOEShapeCustom InverseShape;

    DateTime _activation;

    public int State { get; private set; }

    public IdyllicDreamArena(BossModule module) : base(module)
    {
        var p1 = CurveApprox.Circle(10, 1 / 90f).Select(c => c - new WDir(14, 0));
        var p2 = CurveApprox.Circle(10, 1 / 90f).Select(c => c + new WDir(14, 0));
        var platformPoly = new PolygonClipper().Union(new(p1), new(p2));
        PlatformBounds = new(24, platformPoly);

        InverseShape = new(new PolygonClipper().Difference(new(CurveApprox.Circle(20, 1 / 90f)), new(platformPoly)));
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x21)
        {
            switch (state)
            {
                case 0x00200010:
                    _activation = WorldState.FutureTime(9.1f);
                    break;
                case 0x00800040:
                case 0x02000040:
                    _activation = default;
                    Arena.Bounds = PlatformBounds;
                    State = 1;
                    break;
                case 0x01000001:
                    _activation = default;
                    Arena.Bounds = new ArenaBoundsCircle(20);
                    State = 0;
                    break;
            }
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation == default)
            yield break;

        yield return new(InverseShape, Arena.Center, default, _activation);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // TODO: complex polygons should cache their own rasterization
        if (_activation != default)
            hints.AddForbiddenZone(z => !z.InCircle(new(114, 100), 10) && !z.InCircle(new(86, 100), 10), _activation);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_activation != default && !actor.Position.InCircle(new(114, 100), 10) && !actor.Position.InCircle(new(86, 100), 10))
            hints.Add("Go to platform!");
    }

    public void Predict(float seconds)
    {
        _activation = WorldState.FutureTime(seconds);
    }
}

class ArcadianArcanum(BossModule module) : Components.UniformStackSpread(module, 0, 6)
{
    public int NumCasts;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ArcadianArcanumCast)
        {
            foreach (var p in Raid.WithoutSlot())
                AddSpread(p, Module.CastFinishAt(spell, 1.4f));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ArcadianArcanum)
        {
            NumCasts++;
            Spreads.Clear();
        }
    }
}

class IdyllicDreamElementalMeteor(BossModule module) : Components.GenericTowers(module)
{
    public readonly List<(Actor, Element, WPos)> Meteors = [];

    public bool DrawIcons;

    public const uint ColorWind = 0xFF9ABB81;
    public const uint ColorDark = 0xFFE67AD2;
    public const uint ColorEarth = 0xFF81A1AD;
    public const uint ColorFire = 0xFF2A2DD5;

    void DrawElement(Element el, WPos p)
    {
        var (icon, col) = el switch
        {
            Element.Wind => (FontAwesomeIcon.Tornado, ColorWind),
            Element.Dark => (FontAwesomeIcon.StarOfLife, ColorDark),
            Element.Earth => (FontAwesomeIcon.Gem, ColorEarth),
            Element.Fire => (FontAwesomeIcon.Fire, ColorFire),
            _ => throw new NotImplementedException()
        };
        Arena.AddCircleFilled(p, 1.5f, ArenaColor.Background);
        Arena.IconWorld(p, icon, col);
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        Element? el = (OID)actor.OID switch
        {
            OID.MeteorWind => Element.Wind,
            OID.MeteorDark => Element.Dark,
            OID.MeteorEarth => Element.Earth,
            OID.MeteorFire => Element.Fire,
            _ => null
        };
        if (el != null && state == 0x00010002)
            Meteors.Add((actor, el.Value, actor.Position));
    }

    BitMask _lightVuln;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.LightResistanceDownII)
            _lightVuln.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.CosmicKiss)
        {
            NumCasts++;
            Towers.Clear();
            Meteors.Clear();
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);

        if (DrawIcons)
            foreach (var (_, e, p) in Meteors)
            {
                WPos center = p.X > 100 ? new(114, 100) : new(86, 100);
                DrawElement(e, p + (p - center));
            }
    }

    public void CreateTowers()
    {
        DrawIcons = true;
        EnableHints = false;
        foreach (var (_, e, p) in Meteors)
            Towers.Add(new(p, 3, forbiddenSoakers: e is Element.Wind or Element.Dark ? _lightVuln : ~_lightVuln, activation: WorldState.FutureTime(8.4f)));
    }
}

// knockback is applied by GimmickJump actorcontrol
// knockback distance seems to be variable, like it might be controlled by distance to tower center
class IdyllicDreamWindTower : Components.Knockback
{
    readonly List<Actor> _sources;
    readonly DateTime _activation;

    public IdyllicDreamWindTower(BossModule module) : base(module, ignoreImmunes: true)
    {
        _sources = [.. Module.FindComponent<IdyllicDreamElementalMeteor>()!.Meteors.Where(m => m.Item2 == Element.Wind).Select(m => m.Item1)];
        _activation = module.FindComponent<IdyllicDreamElementalMeteor>()!.Towers[0].Activation;
    }

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        foreach (var s in _sources)
            if (actor.Position.InCircle(s.Position, 3))
                yield return new(s.Position, 23.5f, _activation);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        // hack: there is no cast that corresponds with the knockback, so we just assume dark and aero go off simultaneously
        if ((AID)spell.Action.ID == AID.LindwurmsDarkII)
            _sources.Clear();
    }
}

class IdyllicDreamLindwurmsDarkII : Components.GenericBaitAway
{
    readonly List<Actor> _sources;
    readonly DateTime _activation;

    public IdyllicDreamLindwurmsDarkII(BossModule module) : base(module)
    {
        EnableHints = false;
        _sources = [.. Module.FindComponent<IdyllicDreamElementalMeteor>()!.Meteors.Where(m => m.Item2 == Element.Dark).Select(m => m.Item1)];
        _activation = module.FindComponent<IdyllicDreamElementalMeteor>()!.Towers[0].Activation;
    }

    public override void Update()
    {
        CurrentBaits.Clear();

        foreach (var src in _sources)
        {
            if (Raid.WithoutSlot().Closest(src.Position) is { } player && player.Position.InCircle(src.Position, 3))
                CurrentBaits.Add(new(src, player, new AOEShapeRect(50, 5), _activation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.LindwurmsDarkII)
            _sources.Clear();
    }
}

class IdyllicDreamDoom(BossModule module) : BossComponent(module)
{
    readonly List<Actor> _victims = [];

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Doom)
            _victims.Add(actor);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Doom)
            _victims.Remove(actor);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor.Class.CanEsuna())
            foreach (var t in _victims.Where(v => v.PendingDispels.Count == 0))
                hints.Add($"Cleanse {t.Name}!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var v in _victims.Where(v => v.PendingDispels.Count == 0))
            hints.ShouldCleanse.Set(Raid.FindSlot(v.InstanceID));
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => pc.Class.CanEsuna() && _victims.Contains(player) && player.PendingDispels.Count == 0 ? PlayerPriority.Critical : PlayerPriority.Normal;
}

class IdyllicDreamHotBlooded : Components.StayMove
{
    readonly List<Actor> _sources;
    readonly DateTime _activation;

    bool _castHappened;

    public IdyllicDreamHotBlooded(BossModule module) : base(module)
    {
        _sources = [.. Module.FindComponent<IdyllicDreamElementalMeteor>()!.Meteors.Where(m => m.Item2 == Element.Fire).Select(m => m.Item1)];
        _activation = module.FindComponent<IdyllicDreamElementalMeteor>()!.Towers[0].Activation;
    }

    public override void Update()
    {
        if (_castHappened)
            return;

        Array.Fill(PlayerStates, default);
        foreach (var (i, player) in Raid.WithSlot())
        {
            if (_sources.Any(s => player.Position.InCircle(s.Position, 3)))
                PlayerStates[i] = new(Requirement.NoMove, _activation);
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.HotBlooded)
        {
            _castHappened = true;
            if (Raid.TryFindSlot(actor, out var slot))
                PlayerStates[slot] = new(Requirement.NoMove, default);
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.HotBlooded && Raid.TryFindSlot(actor, out var slot))
            PlayerStates[slot] = default;
    }
}

class LindwurmsStoneIII(BossModule module) : Components.StandardAOEs(module, AID.LindwurmsStoneIII, 4);

class LindwurmsPortent : Components.GenericBaitAway
{
    readonly List<(Actor Source, bool Far, DateTime Expire)> _sources = [];

    public LindwurmsPortent(BossModule module) : base(module)
    {
        EnableHints = false;
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.FarawayPortent:
                _sources.Add((actor, true, status.ExpireAt));
                break;
            case SID.NearbyPortent:
                _sources.Add((actor, false, status.ExpireAt));
                break;
        }
    }

    public override void Update()
    {
        CurrentBaits.Clear();

        foreach (var (src, far, exp) in _sources)
        {
            var target = far ? Raid.WithoutSlot().Farthest(src.Position) : Raid.WithoutSlot().Exclude(src).Closest(src.Position);
            if (target != null)
                CurrentBaits.Add(new(src, target, new AOEShapeCone(60, 15.Degrees()), exp));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.LindwurmsThunderII)
        {
            NumCasts++;
            _sources.Clear();
        }
    }
}
