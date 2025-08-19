namespace BossMod.Dawntrail.Alliance.A24Ealdnarche;

class UranosCascade(BossModule module) : Components.BaitAwayCast(module, AID._Spell_UranosCascade1, new AOEShapeCircle(6), centerAtTarget: true);
class CronosSlingOut(BossModule module) : Components.StandardAOEs(module, AID._Spell_CronosSling1, 9);
class CronosSlingIn(BossModule module) : Components.StandardAOEs(module, AID._Spell_CronosSling4, new AOEShapeDonut(6, 70));
class CronosSlingCounter(BossModule module) : Components.CastCounterMulti(module, [AID._Spell_CronosSling1, AID._Spell_CronosSling4]);
class CronosSlingSide(BossModule module) : Components.GroupedAOEs(module, [AID._Spell_CronosSling2, AID._Spell_CronosSling5], new AOEShapeRect(70, 68));

class GaeaStream(BossModule module) : Components.Exaflare(module, new AOEShapeRect(4, 12))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Spell_GaeaStream1)
        {
            Lines.Add(new()
            {
                Next = spell.LocXZ,
                Advance = caster.Rotation.ToDirection() * 4,
                Rotation = caster.Rotation.ToDirection().Rounded().ToAngle(),
                NextExplosion = Module.CastFinishAt(spell),
                TimeToMove = 2.1f,
                ExplosionsLeft = 6,
                MaxShownExplosions = 2
            });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Spell_GaeaStream1 or AID._Spell_GaeaStream2)
        {
            NumCasts++;
            var ix = Lines.FindIndex(l => l.Next.AlmostEqual(caster.Position, 0.5f));
            if (ix >= 0)
                AdvanceLine(Lines[ix], caster.Position);
        }
    }
}

class EmpyrealVortexRaidwide(BossModule module) : Components.RaidwideCastDelay(module, AID._Spell_EmpyrealVortex, AID._Spell_EmpyrealVortex2, 1.7f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            if (NumCasts >= 5)
            {
                NumCasts = 0;
                Activation = default;
            }
            else
                Activation = WorldState.FutureTime(1.1f);
        }
    }
}
class EmpyrealVortexSpread(BossModule module) : Components.SpreadFromCastTargets(module, AID._Spell_EmpyrealVortex3, 5);
class EmpyrealVortexPuddle(BossModule module) : Components.StandardAOEs(module, AID._Spell_EmpyrealVortex4, 6);

class Sleepga(BossModule module) : Components.GenericAOEs(module, AID._Spell_Sleepga)
{
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Ability_ && !caster.Position.InCircle(Module.PrimaryActor.Position, 2))
        {
            var dir = Arena.Center - caster.Position;
            _aoe = new(new AOEShapeRect(70, 35), caster.Position, dir.ToAngle(), Module.CastFinishAt(spell, 6.5f));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _aoe = null;
        }
    }
}

class OmegaJavelin(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID._Gen_Icon_loc06sp_05ak1, AID._Spell_OmegaJavelin1, 6, 5.2f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == SpreadAction)
        {
            if (Spreads.Count > 0)
                Spreads.RemoveAt(0);
            NumFinishedSpreads++;
        }
    }
}
class OmegaJavelin2(BossModule module) : Components.StandardAOEs(module, AID._Spell_OmegaJavelin2, 6);

// tiles go NW -> SE by rows

// 1A -> 22 actual tiles
// 0x00040004 = despawn (no animation, transition cutscene)
// 0x02000004 = despawn
// 0x04000001 = spawn

// 2C -> 34 cards
// 00020001 -> card spawn
// 00080010 -> plus indicator
// 00200004 -> card disappear

// 35 -> 3D tile indicators
// 00020001 -> disappearing tile
// 00080010 -> appearing tile (wireframe)
class Duplicate1(BossModule module) : Components.GenericAOEs(module, AID._Spell_Duplicate1)
{
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public static WPos GetTile(int i)
    {
        var col = i % 3;
        var row = i / 3;
        return new WPos(784 + col * 16, -816 + row * 16);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            _aoes.RemoveAll(c => c.Origin.AlmostEqual(caster.Position, 1));
            NumCasts++;
        }
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index is >= 0x2C and <= 0x34 && state == 0x00020001)
        {
            var tile = index - 0x2C;
            _aoes.Add(new(new AOEShapeCross(24, 8), GetTile(tile), default, WorldState.FutureTime(10.3f)));
        }
    }
}

class Duplicate2(BossModule module) : Components.GenericAOEs(module, AID._Spell_Duplicate2)
{
    private readonly List<AOEInstance> _aoes = [];
    private int _sourceTile = -1;
    private int _destinationTile = -1;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _sourceTile >= 0 && _destinationTile >= 0 ? _aoes : [];

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index is >= 0x2C and <= 0x34 && state == 0x00020001)
        {
            var tile = index - 0x2C;
            if (tile == _sourceTile && _destinationTile >= 0)
                tile = _destinationTile;

            _aoes.Add(new(new AOEShapeCross(24, 8), Duplicate1.GetTile(tile), default, WorldState.FutureTime(12.6f)));
        }

        if (index is >= 0x35 and <= 0x3D)
        {
            // disappearing tile
            if (state == 0x00020001)
            {
                _sourceTile = index - 0x35;
                UpdateAOE();
            }

            if (state == 0x00080010)
            {
                _destinationTile = index - 0x35;
                UpdateAOE();
            }
        }
    }

    private void UpdateAOE()
    {
        if (_sourceTile >= 0 && _destinationTile >= 0)
        {
            var srcPos = Duplicate1.GetTile(_sourceTile);
            var destPos = Duplicate1.GetTile(_destinationTile);
            for (var i = 0; i < _aoes.Count; i++)
                if (_aoes[i].Origin == srcPos)
                    _aoes.Ref(i).Origin = destPos;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            _aoes.RemoveAll(c => c.Origin.AlmostEqual(caster.Position, 1));
            _sourceTile = -1;
            _destinationTile = -1;
            NumCasts++;
        }
    }
}

class TileArena(BossModule module) : BossComponent(module)
{
    private BitMask _missing;

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index is >= 0x1A and <= 0x22)
        {
            if (state is 0x02000004 or 0x00040004)
                _missing.Set(index - 0x1A);

            if (state is 0x04000001)
                _missing.Clear(index - 0x1A);

            UpdateBounds();
        }
    }

    private void UpdateBounds()
    {
        var curBounds = CurveApprox.Rect(new(24, 0), new(0, 24));
        RelSimplifiedComplexPolygon poly = new(curBounds);
        foreach (var b in _missing.SetBits())
        {
            var dir = Duplicate1.GetTile(b) - Arena.Center;
            var tile = CurveApprox.Rect(new(8, 0), new(0, 8)).Select(d => d + dir);

            poly = Arena.Bounds.Clipper.Difference(new(poly), new(tile));
        }

        Arena.Bounds = new ArenaBoundsCustom(24, poly);
    }
}

class TileSwap(BossModule module) : Components.StandardAOEs(module, AID._Ability_2, new AOEShapeRect(16, 8));

class TileVanish(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index is >= 0x35 and <= 0x3D && state == 0x00020001)
            _aoe = new(new AOEShapeRect(8, 8, 8), Duplicate1.GetTile(index - 0x35), default, WorldState.FutureTime(7.9f), ArenaColor.Danger);

        if (index is >= 0x1A and <= 0x22 && state is 0x02000004 or 0x00040004)
            _aoe = null;
    }
}

class StellarBurst(BossModule module) : Components.GenericStackSpread(module)
{
    public int NumCasts;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Spell_StellarBurst1 && WorldState.Actors.Find(spell.TargetID) is { } target)
            Stacks.Add(new(target, 24, activation: Module.CastFinishAt(spell, 6.6f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Spell_StellarBurst2)
        {
            NumCasts++;
            Stacks.Clear();
        }
    }
}

class QuakeZone(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _cast;
    private BitMask _active;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_cast is { } c)
            yield return c;

        foreach (var b in _active.SetBits())
            yield return new AOEInstance(new AOEShapeRect(8, 8, 8), Duplicate1.GetTile(b));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Spell_Quake or AID._Spell_Freeze)
            _cast = new(new AOEShapeRect(16, 24), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Spell_Quake or AID._Spell_Freeze)
            NumCasts++;
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index is >= 0x1A and <= 0x22)
        {
            if (state is 0x00080010 or 0x00400080)
            {
                _cast = null;
                _active.Set(index - 0x1A);
            }

            if (state is 0x00200001 or 0x01000001)
                _active.Clear(index - 0x1A);
        }
    }
}

class TornadoAttract(BossModule module) : Components.KnockbackFromCastTarget(module, AID._Spell_Tornado, 16, kind: Kind.TowardsOrigin)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var s in Sources(slot, actor))
            if (!IsImmune(slot, s.Activation))
                hints.AddForbiddenZone(p =>
                {
                    var dir = p - s.Origin;
                    return dir.LengthSq() < 441 || !dir.InRect(new(0, 1), 24, 24, 8) && !dir.InRect(new(1, 0), 24, 24, 8);
                }, s.Activation);
    }
}
class TornadoBoss(BossModule module) : Components.StandardAOEs(module, AID._Spell_Tornado1, 5);
class Burst(BossModule module) : Components.StandardAOEs(module, AID._Spell_Burst, 7);

class OrbitalWind(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _wind = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _wind.Select(w => new AOEInstance(new AOEShapeCircle(3), w.Position, w.Rotation));

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var aoe in ActiveAOEs(slot, actor))
        {
            hints.AddForbiddenZone(aoe.Shape, aoe.Origin);
            hints.AddForbiddenZone(ShapeContains.Capsule(aoe.Origin, aoe.Rotation, 8, 3), WorldState.FutureTime(2));
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID._Gen_OrbitalWind)
            _wind.Add(actor);
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID == OID._Gen_OrbitalWind)
            _wind.Remove(actor);
    }
}

class Flare(BossModule module) : Components.StandardAOEs(module, AID._Spell_Flare, 5);
class FlareRect(BossModule module) : Components.StandardAOEs(module, AID._Spell_Flare1, new AOEShapeRect(70, 3));

class A24EaldnarcheStates : StateMachineBuilder
{
    public A24EaldnarcheStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        Cast(id, AID._Spell_UranosCascade, 6.1f, 4)
            .ActivateOnEnter<UranosCascade>();

        ComponentCondition<UranosCascade>(id + 3, 1, c => c.NumCasts > 0, "Tankbusters")
            .DeactivateOnExit<UranosCascade>();

        CronosSling(id + 0x10000, 5.1f);
        EmpyrealVortexP1(id + 0x11000, 6.4f);
        CronosSling(id + 0x12000, 4.4f);
        Sleepga(id + 0x20000, 3.4f);
        Cast(id + 0x21000, AID._Spell_GaeaStream, 4.3f, 3)
            .ActivateOnEnter<GaeaStream>();
        ComponentCondition<GaeaStream>(id + 0x21010, 1, g => g.NumCasts > 0, "Exalines start");
        OmegaJavelin(id + 0x22000, 5.2f);

        Cast(id + 0x30000, AID._Spell_Duplicate, 3.7f, 2)
            .ActivateOnEnter<Duplicate1>();
        ComponentCondition<Duplicate1>(id + 0x30010, 11.1f, d => d.NumCasts > 0, "Tiles 1");
        Cast(id + 0x31000, AID._Spell_Duplicate, 2, 2);
        ComponentCondition<Duplicate1>(id + 0x31010, 11.1f, d => d.NumCasts > 10, "Tiles 2")
            .DeactivateOnExit<Duplicate1>();

        Cast(id + 0x32000, AID._Spell_Excelsior, 11.5f, 7)
            .ActivateOnEnter<TileSwap>()
            .ActivateOnEnter<TileArena>()
            .ActivateOnEnter<TileVanish>();
        Timeout(id + 0x32010, 0.6f, "Stun").SetHint(StateMachine.StateHint.DowntimeStart);

        Targetable(id + 0x32100, false, 4.8f);
        Targetable(id + 0x33000, true, 30.3f, "Boss reappears");

        Cast(id + 0x40000, AID._Spell_Duplicate, 2.1f, 2);

        SimpleState(id + 0xFF0000, 10000, "???")
            .ActivateOnEnter<Duplicate2>()
            .ActivateOnEnter<StellarBurst>()
            .ActivateOnEnter<QuakeZone>()
            .ActivateOnEnter<TornadoAttract>()
            .ActivateOnEnter<TornadoBoss>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<OrbitalWind>()
            .ActivateOnEnter<Flare>()
            .ActivateOnEnter<FlareRect>()
            .ActivateOnEnter<OmegaJavelin>()
            .ActivateOnEnter<OmegaJavelin2>()
            .ActivateOnEnter<CronosSlingIn>()
            .ActivateOnEnter<CronosSlingOut>()
            .ActivateOnEnter<CronosSlingSide>()
            .ActivateOnEnter<Sleepga>();
    }

    private void CronosSling(uint id, float delay)
    {
        CastMulti(id, [AID._Spell_CronosSling, AID._Spell_CronosSling3], delay, 7)
            .ActivateOnEnter<CronosSlingIn>()
            .ActivateOnEnter<CronosSlingOut>()
            .ActivateOnEnter<CronosSlingSide>()
            .ActivateOnEnter<CronosSlingCounter>();

        ComponentCondition<CronosSlingCounter>(id + 0x10, 0.5f, c => c.NumCasts > 0, "In/out");
        ComponentCondition<CronosSlingSide>(id + 0x20, 5.8f, c => c.NumCasts > 0, "Side")
            .DeactivateOnExit<CronosSlingIn>()
            .DeactivateOnExit<CronosSlingOut>()
            .DeactivateOnExit<CronosSlingSide>()
            .DeactivateOnExit<CronosSlingCounter>();
    }

    private void EmpyrealVortexP1(uint id, float delay)
    {
        Cast(id, AID._Spell_EmpyrealVortex, delay, 4)
            .ActivateOnEnter<EmpyrealVortexPuddle>()
            .ActivateOnEnter<EmpyrealVortexSpread>()
            .ActivateOnEnter<EmpyrealVortexRaidwide>();

        ComponentCondition<EmpyrealVortexRaidwide>(id + 0x10, 1, r => r.NumCasts > 0, "Raidwides start");
        ComponentCondition<EmpyrealVortexRaidwide>(id + 0x11, 4.3f, r => r.NumCasts == 0, "Raidwides end")
            .DeactivateOnExit<EmpyrealVortexRaidwide>();

        ComponentCondition<EmpyrealVortexSpread>(id + 0x20, 5, s => s.NumFinishedSpreads > 0, "Spreads")
            .DeactivateOnExit<EmpyrealVortexPuddle>()
            .DeactivateOnExit<EmpyrealVortexSpread>();
    }

    private void Sleepga(uint id, float delay)
    {
        Cast(id, AID._Ability_Warp, delay, 4)
            .ActivateOnEnter<Sleepga>();

        ComponentCondition<Sleepga>(id + 0x10, 6.5f, s => s.NumCasts > 0, "Safe corner")
            .DeactivateOnExit<Sleepga>();
    }

    private void OmegaJavelin(uint id, float delay)
    {
        CastStart(id, AID._Spell_OmegaJavelin, delay)
            .ActivateOnEnter<OmegaJavelin>()
            .ActivateOnEnter<OmegaJavelin2>();

        ComponentCondition<OmegaJavelin>(id + 0x10, 5.1f, j => j.NumFinishedSpreads > 0, "Spreads")
            .DeactivateOnExit<OmegaJavelin>();
        ComponentCondition<OmegaJavelin2>(id + 0x20, 4.5f, j => j.NumCasts > 0, "Puddles")
            .DeactivateOnExit<OmegaJavelin2>();
    }
}
