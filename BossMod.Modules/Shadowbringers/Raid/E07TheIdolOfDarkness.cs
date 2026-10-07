namespace BossMod.Shadowbringers.Raid.E07TheIdolOfDarkness;

public enum OID : uint
{
    Boss = 0x2D2D, // R28.000, The Idol of Darkness
    Helper = 0x233C, // R0.500
    Voidgate = 0x18D6, // R0.500, red/blue portals
    UnforgivenIdolatryLight = 0x2D2E, // R3.000
    UnforgivenIdolatryDark = 0x2E0A, // R3.000
    Idolatry = 0x2D30, // R3.000, phase 2 add
    Idolatry2 = 0x2D31, // R3.000, phase 2 add
    BossPart1 = 0x2D3A, // R1.000, Part
    BossPart2 = 0x2D3B, // R1.000, Part
}

public enum AID : uint
{
    AutoAttack = 19536, // Helper->player, no cast, single-target
    EmptyWave = 19538, // Boss->self, 4.0s cast, range 100 circle
    UnshadowedStake = 19537, // Boss->self, 5.0s cast, single-target
    StygianStake = 19507, // Helper->player, no cast, range 100 width 6 rect
    WordsOfMotion = 19499, // Boss->self, 2.5s cast, single-target
    LightsCourseVisual = 19511, // UnforgivenIdolatryLight->self, no cast, single-target
    LightsCourse = 19516, // UnforgivenIdolatryLight->self, 7.0s cast, range 50 width 10 rect
    LightsCourseShort = 20067, // UnforgivenIdolatryLight->self, 7.0s cast, range 25 width 10 rect (into portal)
    LightsCoursePortal = 19518, // UnforgivenIdolatryLight->self, 7.0s cast, range 50 width 10 rect (into portal)
    LightsCourseNight = 19490, // UnforgivenIdolatryLight->self, 2.0s cast, range 50 width 10 rect
    LightsCourseExit = 19520, // UnforgivenIdolatryLight->self, no cast, range 55 width 10 rect (portal exit)
    BetwixtWorlds = 19704, // Boss->self, no cast, single-target (portals)
    AwayWithThee = 19513, // Boss->self, 3.0s cast, single-target
    AwayWithTheeAdds = 20094, // Boss->self, 3.0s cast, single-target (phase 2)
    FalseTwilight = 19545, // Boss->self, 3.0s cast, single-target
    StygianSword = 19541, // Helper->location, 4.0s cast, range 6 circle
    SilverSledge = 19540, // Helper->player, 5.0s cast, range 6 circle
    UnjoinedAspect = 19515, // Boss->self, 4.0s cast, single-target
    UnjoinedAspectApply = 19510, // Helper->self, no cast, single-target
    WordsOfNight = 19500, // Boss->self, 2.5s cast, single-target
    DarksCourseVisual = 19512, // UnforgivenIdolatryDark->self, no cast, single-target
    DarksCourse = 19517, // UnforgivenIdolatryDark->self, 7.0s cast, range 50 width 10 rect
    DarksCourseNight = 19491, // UnforgivenIdolatryDark->self, 2.0s cast, range 50 width 10 rect
    DarksCoursePortal = 19521, // UnforgivenIdolatryDark->self, 7.0s cast, range 50 width 10 rect (into portal)
    DarksCourseExit = 19523, // UnforgivenIdolatryDark->self, no cast, range 55 width 10 rect (portal exit)
    IdolatryAppear = 19514, // Idolatry/Idolatry2->self, no cast, single-target
    AutoAttackAdd = 19529, // Idolatry/Idolatry2->player, no cast, single-target
    StrengthInNumbersDonut = 19532, // Idolatry/Idolatry2->self, 11.0s cast, range 8-20 donut
    StrengthInNumbersCircle = 19533, // Idolatry/Idolatry2->self, 5.0s cast, range 12 circle
    SilverShot = 19602, // Helper->players, 8.0s cast, range 6 circle
    SilverShot2 = 20093, // Helper->players, 8.0s cast, range 6 circle
    OverwhelmingForce = 19534, // Idolatry->self, 3.0s cast, single-target (enrage)
    OverwhelmingForceAOE = 19535, // Idolatry->self, 3.0s cast, range 100 circle
    Explosion = 19531, // Idolatry2/Idolatry->self, 8.0s cast, range 100 circle (enrage)
    EmptyFlood = 20054, // Boss->self, 7.0s cast, single-target
    EmptyFloodAOE = 20052, // Helper->self, 7.0s cast, range 100 circle
    EmptyFloodAlt = 19539, // Boss->self, 7.0s cast, single-target
    BlackSmoke = 19542, // Helper->self, no cast, range 5 circle (tower soak)
    BlackBlaze = 19543, // Helper->self, no cast, range 100 circle (tower fail)
    SilverShotVisual = 20092, // Boss->self, 3.0s cast, single-target
    //_Ability_ = 19504, // Boss->self, no cast, single-target
    //_Ability_1 = 19722, // Boss->self, no cast, single-target
    //_Ability_2 = 19506, // Boss->self, no cast, single-target
}

public enum SID : uint
{
    AstralEffect = 2238, // none->player, light aspect — soak dark
    UmbralEffect = 2239, // none->player, dark aspect — soak light
    ForwardWithThee = 2240, // none->player
    BackWithThee = 2241, // none->player
    LeftWithThee = 2242, // none->player
    RightWithThee = 2243, // none->player
}

public enum TetherID : uint
{
    UnshadowedStake = 37, // Boss->player
    Voidgate = 113, // Voidgate->Boss
}

class EmptyWave(BossModule module) : Components.RaidwideCast(module, AID.EmptyWave);
class EmptyFlood(BossModule module) : Components.RaidwideCast(module, AID.EmptyFloodAOE);

class UnshadowedStake(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeRect(100, 3), (uint)TetherID.UnshadowedStake, AID.StygianStake)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.UnshadowedStake)
        {
            var act = Module.CastFinishAt(spell);
            foreach (ref var b in CurrentBaits.AsSpan())
                b.Activation = act;
        }
    }
}

class StygianSword(BossModule module) : Components.StandardAOEs(module, AID.StygianSword, 6);
class SilverSledge : Components.StackWithCastTargets
{
    public SilverSledge(BossModule module) : base(module, AID.SilverSledge, 6, 2, 4)
    {
        PermitOverlap = false;
    }
}

class SilverShot(BossModule module) : Components.SpreadFromCastTargets(module, AID.SilverShot, 6);
class SilverShot2(BossModule module) : Components.SpreadFromCastTargets(module, AID.SilverShot2, 6);

class StrengthInNumbersDonut(BossModule module) : Components.StandardAOEs(module, AID.StrengthInNumbersDonut, new AOEShapeDonut(8, 20))
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // don't add hints if you have AwayWithThee cause they fight each other
        if (Module.FindComponent<AwayWithThee>()?.ActiveFor(actor) == true)
            return;
        base.AddAIHints(slot, actor, assignment, hints);
    }
}

class StrengthInNumbersCircle(BossModule module) : Components.StandardAOEs(module, AID.StrengthInNumbersCircle, 12)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // don't add hints if you have AwayWithThee cause they fight each other
        if (Module.FindComponent<AwayWithThee>()?.ActiveFor(actor) == true)
            return;
        base.AddAIHints(slot, actor, assignment, hints);
    }
}

class Explosion(BossModule module) : Components.CastHint(module, AID.Explosion, "Kill adds!", true);

class AwayWithThee(BossModule module) : Components.Knockback(module, ignoreImmunes: true)
{
    private readonly List<(Actor Target, SID Sid, DateTime Activation, Angle LockedFace)> _sources = [];
    private const float Distance = 15;

    public bool ActiveFor(Actor actor) => _sources.Any(s => s.Target == actor);

    private static WDir DirAt(SID sid, Angle rotation) => sid switch
    {
        SID.ForwardWithThee => rotation.ToDirection(),
        SID.BackWithThee => (rotation + 180.Degrees()).ToDirection(),
        SID.LeftWithThee => rotation.ToDirection().OrthoL(),
        SID.RightWithThee => rotation.ToDirection().OrthoR(),
        _ => default
    };

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        foreach (var s in _sources)
        {
            if (s.Target != actor)
                continue;
            var dir = DirAt(s.Sid, actor.Rotation);
            if (dir != default)
                yield return new(actor.Position, Distance, s.Activation, null, dir.ToAngle(), Kind.DirForward);
        }
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID is SID.ForwardWithThee or SID.BackWithThee or SID.LeftWithThee or SID.RightWithThee)
        {
            // Lock facing at gain so later retargets/add swaps cannot swing the AI teleport plan.
            var prefer = WorldState.Actors.Find(actor.TargetID);
            var face = prefer is { } t && t.InstanceID != actor.InstanceID ? Angle.FromDirection(t.Position - actor.Position) : actor.Rotation;
            _sources.Add((actor, (SID)status.ID, status.ExpireAt, face));
        }
    }

    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID is SID.ForwardWithThee or SID.BackWithThee or SID.LeftWithThee or SID.RightWithThee)
            _sources.RemoveAll(s => s.Target == actor);
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        if (!Module.InBounds(pos))
            return true;
        if (Module.FindComponent<BlackSmoke>() is { Towers.Count: > 0 } towers)
            return !towers.Towers.Any(t => t.IsInside(pos));
        foreach (var c in Module.Components.OfType<Components.GenericAOEs>())
            if (c.ActiveAOEs(slot, actor).Any(a => a.Risky && a.Check(pos)))
                return true;
        return false;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var (target, sid, act, lockedFace) in _sources)
        {
            if (target != actor)
                continue;

            var dir = DirAt(sid, lockedFace);
            if (dir == default)
                continue;

            var offset = Distance * dir;
            var r = Module.Bounds.Radius - 1; // bit of margin
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(Arena.Center - offset, new WDir(0, 1), r, r, r), act);

            foreach (var c in Module.Components.OfType<Components.GenericAOEs>())
            {
                foreach (var aoe in c.ActiveAOEs(slot, actor))
                {
                    if (aoe.Risky)
                        hints.AddForbiddenZone(aoe.Shape.Distance(aoe.Origin - offset, aoe.Rotation), act);
                }
            }

            if (Module.FindComponent<BlackSmoke>() is { Towers.Count: > 0 } towers)
            {
                var preimages = towers.Towers.Select(t => ShapeDistance.Circle(t.Position - offset, t.Radius)).ToList();
                var soak = ShapeDistance.Union(preimages);
                hints.AddForbiddenZone(p => -soak(p), act);
                foreach (var t in towers.Towers)
                    hints.GoalZones.Add(AIHints.GoalSingleTarget(t.Position - offset, t.Radius));
            }

            hints.ForbiddenDirections.Add(((lockedFace + 180.Degrees()).Normalized(), 178.Degrees(), act));
            var remain = (float)(act - WorldState.CurrentTime).TotalSeconds;
            if (remain is > 0 and < 0.5f) // stop attacks before resolve so autorot doesn't change facing
                hints.MaxCastTime = Math.Min(hints.MaxCastTime, Math.Max(0, remain - 0.4f));
        }
    }
}

class LightsAndDarksCourse(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(AOEInstance AOE, bool Astral, bool PortalExit)> _aoes = [];
    private readonly List<(WPos Pos, Angle Rot)> _pendingExitPortals = [];
    private readonly List<(WPos Pos, Angle Rot)> _exitPortals = [];
    private readonly List<(bool Astral, DateTime When)> _pendingPortalEntries = [];
    private int _portalEntries;

    private static readonly AOEShapeRect _shape50 = new(50, 5);
    private static readonly AOEShapeRect _shape25 = new(25, 5);
    private static readonly AOEShapeRect _shape55 = new(55, 5);

    public override void Update()
    {
        var cutoff = WorldState.CurrentTime.AddSeconds(-0.5f); // I have no idea but sometimes they just don't resolve
        _aoes.RemoveAll(a => a.AOE.Activation < cutoff);

        if (_pendingPortalEntries.Count > 0)
        {
            // same-wave mixed, offset light first then dark
            foreach (var e in _pendingPortalEntries.OrderBy(e => e.When).ThenByDescending(e => e.Astral))
                PredictPortalExits(e.Astral);
            _pendingPortalEntries.Clear();
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var astral = actor.FindStatus(SID.AstralEffect) != null;
        var umbral = actor.FindStatus(SID.UmbralEffect) != null;
        DateTime earliest = default;
        foreach (var (aoe, isAstral, _) in _aoes.OrderBy(a => a.AOE.Activation))
        {
            // light soaks dark, dark soaks light
            if (astral && !isAstral || umbral && isAstral)
                continue;
            if (earliest == default) // only show imminent or else the entire arena gets covered
                earliest = aoe.Activation.AddSeconds(0.5f);
            if (aoe.Activation > earliest)
                yield break;
            yield return aoe;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var astral = actor.FindStatus(SID.AstralEffect) != null;
        var umbral = actor.FindStatus(SID.UmbralEffect) != null;
        if ((astral || umbral) && _aoes.Count > 0)
            hints.Add(astral ? "Soak dark lane" : "Soak light lane");
        base.AddHints(slot, actor, hints);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.BetwixtWorlds:
                _exitPortals.Clear();
                _exitPortals.AddRange(_pendingExitPortals);
                _pendingExitPortals.Clear();
                _portalEntries = 0;
                _aoes.RemoveAll(a => a.PortalExit);
                break;
            case AID.LightsCourseExit:
            case AID.DarksCourseExit:
                NumCasts++;
                RemoveClosestPortalExit(caster.Position);
                break;
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID != (uint)TetherID.Voidgate || source.OID != (uint)OID.Voidgate)
            return;
        // N/S are always entry voidgates, E/W are always exits. Just easier this way
        if (Math.Abs(source.Position.X - Arena.Center.X) <= 15)
            return;
        if (!_pendingExitPortals.Any(p => p.Pos.AlmostEqual(source.Position, 1)))
            _pendingExitPortals.Add((source.Position, source.Rotation));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.LightsCourse:
            case AID.LightsCoursePortal:
            case AID.LightsCourseNight:
                _aoes.Add((new(_shape50, caster.Position, spell.Rotation, Module.CastFinishAt(spell)), true, false));
                break;
            case AID.LightsCourseShort:
                _aoes.Add((new(_shape25, caster.Position, spell.Rotation, Module.CastFinishAt(spell)), true, false));
                break;
            case AID.DarksCourse:
            case AID.DarksCoursePortal:
            case AID.DarksCourseNight:
                _aoes.Add((new(_shape50, caster.Position, spell.Rotation, Module.CastFinishAt(spell)), false, false));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var aid = (AID)spell.Action.ID;
        switch (aid)
        {
            case AID.LightsCourse:
            case AID.LightsCourseShort:
            case AID.LightsCoursePortal:
            case AID.LightsCourseNight:
            case AID.DarksCourse:
            case AID.DarksCoursePortal:
            case AID.DarksCourseNight:
                _aoes.RemoveAll(a => !a.PortalExit && a.AOE.Origin.AlmostEqual(caster.Position, 1) && a.AOE.Rotation.AlmostEqual(spell.Rotation, 0.1f));
                break;
        }

        if (aid is AID.LightsCourseShort or AID.LightsCoursePortal or AID.DarksCoursePortal)
            _pendingPortalEntries.Add((aid is not AID.DarksCoursePortal, WorldState.CurrentTime));
    }

    private void RemoveClosestPortalExit(WPos pos)
    {
        var best = -1;
        var bestDist = float.MaxValue;
        for (var i = 0; i < _aoes.Count; ++i)
        {
            if (!_aoes[i].PortalExit)
                continue;
            var d = (_aoes[i].AOE.Origin - pos).LengthSq();
            if (d < bestDist)
            {
                bestDist = d;
                best = i;
            }
        }
        if (best >= 0)
            _aoes.RemoveAt(best);
    }

    private void PredictPortalExits(bool astral)
    {
        if (_exitPortals.Count == 0)
            return;

        // words of night + multi-portal resolves faster than words of motion portal redirects
        var night = Raid.WithoutSlot().Any(p => p.FindStatus(SID.AstralEffect) != null || p.FindStatus(SID.UmbralEffect) != null);
        var delay = night ? 3.2f : 6.1f;
        var act = WorldState.FutureTime(delay);
        var sign = (_portalEntries % 2 == 0) ? 1 : -1;

        foreach (var (pos, rot) in _exitPortals)
        {
            var baseOff = rot.Rad > 0 ? 5f : -5f; // west facing: +Z first, east facing: -Z first. Second set flips the other lane
            var origin = new WPos(pos.X, pos.Z + baseOff * sign);
            _aoes.Add((new(_shape55, origin, rot, act), astral, true));
        }
        _portalEntries++;
    }
}

class BlackSmoke(BossModule module) : Components.GenericTowers(module, AID.BlackSmoke)
{
    public override void OnMapEffect(byte index, uint state)
    {
        if (state != 0x00200010)
            return;
        var pos = index switch
        {
            0x0E => new WPos(100, 93),
            0x0F => new WPos(100, 107),
            _ => default
        };
        if (pos != default && !Towers.Any(t => t.Position.AlmostEqual(pos, 1)))
            Towers.Add(new(pos, 5, activation: WorldState.FutureTime(15.1f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.BlackSmoke or AID.BlackBlaze)
        {
            NumCasts++;
            Towers.Clear();
        }
    }
}

class IdolatryAdds(BossModule module) : Components.AddsMulti(module, [OID.Idolatry, OID.Idolatry2]);

class E07TheIdolOfDarknessStates : StateMachineBuilder
{
    public E07TheIdolOfDarknessStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<EmptyWave>()
            .ActivateOnEnter<EmptyFlood>()
            .ActivateOnEnter<UnshadowedStake>()
            .ActivateOnEnter<LightsAndDarksCourse>()
            .ActivateOnEnter<AwayWithThee>()
            .ActivateOnEnter<StygianSword>()
            .ActivateOnEnter<SilverSledge>()
            .ActivateOnEnter<SilverShot>()
            .ActivateOnEnter<SilverShot2>()
            .ActivateOnEnter<StrengthInNumbersDonut>()
            .ActivateOnEnter<StrengthInNumbersCircle>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<BlackSmoke>()
            .ActivateOnEnter<IdolatryAdds>();
    }
}

[ModuleInfo(Contributors = "croizat", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 726, NameID = 9298)]
public class E07TheIdolOfDarkness(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsSquare(20));
