namespace BossMod.Dawntrail.Alliance.A24Ealdnarche;

class UranosCascade(BossModule module) : Components.BaitAwayCast(module, AID.UranosCascade, new AOEShapeCircle(6), centerAtTarget: true);
class CronosSlingOut(BossModule module) : Components.StandardAOEs(module, AID.CronosSlingOut, 9);
class CronosSlingIn(BossModule module) : Components.StandardAOEs(module, AID.CronosSlingIn, new AOEShapeDonut(6, 70));
class CronosSlingCounter(BossModule module) : Components.CastCounterMulti(module, [AID.CronosSlingOut, AID.CronosSlingIn]);
class CronosSlingSide(BossModule module) : Components.GroupedAOEs(module, [AID.CronosSlingLeft, AID.CronosSlingRight], new AOEShapeRect(70, 68));

class EmpyrealVortexRaidwide(BossModule module) : Components.RaidwideCastDelay(module, AID.EmpyrealVortexCast, AID.EmpyrealVortexRaidwide, 1.7f)
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
class EmpyrealVortexSpread(BossModule module) : Components.SpreadFromCastTargets(module, AID.EmpyrealVortexSpread, 5);
class EmpyrealVortexPuddle(BossModule module) : Components.StandardAOEs(module, AID.EmpyrealVortexPuddle, 6);

class Sleepga(BossModule module) : Components.GenericAOEs(module, AID.Sleepga)
{
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.WarpTarget && !caster.Position.InCircle(Module.PrimaryActor.Position, 2))
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

class OmegaJavelin(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.OmegaJavelin, AID.OmegaJavelinSpread, 6, 5.2f)
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
class OmegaJavelin2(BossModule module) : Components.StandardAOEs(module, AID.OmegaJavelinRepeat, 6);

class StellarBurst(BossModule module) : Components.GenericStackSpread(module)
{
    public int NumCasts;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.StellarBurst1 && WorldState.Actors.Find(spell.TargetID) is { } target)
            Stacks.Add(new(target, 24, activation: Module.CastFinishAt(spell, 6.6f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.StellarBurst2)
        {
            NumCasts++;
            Stacks.Clear();
        }
    }
}

class TornadoAttract(BossModule module) : Components.KnockbackFromCastTarget(module, AID.TornadoAttract, 16, kind: Kind.TowardsOrigin)
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
class TornadoBoss(BossModule module) : Components.StandardAOEs(module, AID.TornadoPuddle, 5);
class Burst(BossModule module) : Components.StandardAOEs(module, AID.Burst, 7);

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
        if ((OID)actor.OID == OID.OrbitalWind)
            _wind.Add(actor);
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID == OID.OrbitalWind)
            _wind.Remove(actor);
    }
}

class Flare(BossModule module) : Components.StandardAOEs(module, AID.FlarePuddle, 5);
class FlareRect(BossModule module) : Components.StandardAOEs(module, AID.FlareRect, new AOEShapeRect(70, 3));

class Flood(BossModule module) : Components.StandardAOEs(module, AID.FloodProximity, 20);
class FloodDonut(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(8), new AOEShapeDonut(8, 16), new AOEShapeDonut(16, 24), new AOEShapeDonut(24, 36)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Flood1)
            AddSequence(caster.Position, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var order = (AID)spell.Action.ID switch
        {
            AID.Flood1 => 0,
            AID.Flood2 => 1,
            AID.Flood3 => 2,
            AID.Flood4 => 3,
            _ => -1
        };

        if (order >= 0)
            AdvanceSequence(order, caster.Position, WorldState.FutureTime(2));
    }
}

class A24EaldnarcheStates : StateMachineBuilder
{
    public A24EaldnarcheStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        UranosCascade(id, 6.1f);
        CronosSling(id + 0x10000, 5.1f);
        EmpyrealVortexP1(id + 0x11000, 6.4f);
        CronosSling(id + 0x12000, 4.4f);
        Sleepga(id + 0x20000, 3.4f);
        Cast(id + 0x21000, AID.GaeaStreamCast, 4.3f, 3)
            .ActivateOnEnter<GaeaStream>();
        ComponentCondition<GaeaStream>(id + 0x21010, 1, g => g.NumCasts > 0, "Exalines start");
        OmegaJavelin(id + 0x22000, 5.2f);

        Cast(id + 0x30000, AID.DuplicateCast, 3.7f, 2)
            .ActivateOnEnter<Duplicate1>();
        ComponentCondition<Duplicate1>(id + 0x30010, 11.1f, d => d.NumCasts > 0, "Tiles 1");
        Cast(id + 0x31000, AID.DuplicateCast, 2, 2);
        ComponentCondition<Duplicate1>(id + 0x31010, 11.1f, d => d.NumCasts > 10, "Tiles 2")
            .DeactivateOnExit<Duplicate1>();

        Cast(id + 0x32000, AID.Excelsior, 11.5f, 7)
            .ActivateOnEnter<TileSwap>()
            .ActivateOnEnter<TileArena>()
            .ActivateOnEnter<TileVanish>();
        Timeout(id + 0x32010, 0.6f, "Stun").SetHint(StateMachine.StateHint.DowntimeStart);

        Targetable(id + 0x32100, false, 4.8f);
        Targetable(id + 0x33000, true, 30.3f, "Boss reappears");

        Duplicate2(id + 0x40000, 2.1f);
        StellarBurst(id + 0x41000, 1);
        Cast(id + 0x42000, AID.AncientTriad, 10.8f, 6)
            .ActivateOnEnter<Flood>()
            .ActivateOnEnter<FloodDonut>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<QuakeZone>()
            .ActivateOnEnter<Flare>()
            .ActivateOnEnter<FlareRect>()
            .ActivateOnEnter<OrbitalWind>()
            .ActivateOnEnter<TornadoAttract>()
            .ActivateOnEnter<TornadoBoss>();

        Cast(id + 0x43000, AID.GaeaStreamCast, 12.7f, 3)
            .DeactivateOnExit<Flood>()
            .DeactivateOnExit<FloodDonut>()
            .DeactivateOnExit<Burst>()
            .DeactivateOnExit<QuakeZone>()
            .DeactivateOnExit<Flare>()
            .DeactivateOnExit<FlareRect>()
            .DeactivateOnExit<OrbitalWind>()
            .DeactivateOnExit<TornadoAttract>()
            .DeactivateOnExit<TornadoBoss>();

        UranosCascade(id + 0x44000, 15.2f);
        Duplicate2(id + 0x45000, 4.3f);
        OmegaJavelin(id + 0x46000, 1.1f);
        Cast(id + 0x50000, AID.GaeaStreamCast, 50, 3);
        CronosSling(id + 0x51000, 6.1f);
        StellarBurst(id + 0x52000, 3.9f);

        Cast(id + 0x60000, AID.AncientTriad, 10.8f, 6)
            .ActivateOnEnter<Flood>()
            .ActivateOnEnter<FloodDonut>()
            .ActivateOnEnter<EmpyrealVortexPuddle>()
            .ActivateOnEnter<EmpyrealVortexRaidwide>()
            .ActivateOnEnter<EmpyrealVortexSpread>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<QuakeZone>()
            .ActivateOnEnter<Flare>()
            .ActivateOnEnter<FlareRect>()
            .ActivateOnEnter<OrbitalWind>()
            .ActivateOnEnter<TornadoAttract>()
            .ActivateOnEnter<TornadoBoss>()
            .ActivateOnEnter<Duplicate2>()
            .ActivateOnEnter<StellarBurst>()
            .ActivateOnEnter<OmegaJavelin>()
            .ActivateOnEnter<OmegaJavelin2>()
            .ActivateOnEnter<CronosSlingIn>()
            .ActivateOnEnter<CronosSlingOut>()
            .ActivateOnEnter<CronosSlingSide>()
            .ActivateOnEnter<Sleepga>();

        Timeout(id + 0x61000, 10000, "Repeat mechanics until death");
    }

    private void UranosCascade(uint id, float delay)
    {
        Cast(id, AID.UranosCascadeCast, delay, 4)
            .ActivateOnEnter<UranosCascade>();

        ComponentCondition<UranosCascade>(id + 3, 1, c => c.NumCasts > 0, "Tankbusters")
            .DeactivateOnExit<UranosCascade>();
    }

    private void CronosSling(uint id, float delay)
    {
        CastMulti(id, [AID.CronosSlingCast1, AID.CronosSlingCast4, AID.CronosSlingCast2, AID.CronosSlingCast3], delay, 7)
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
        Cast(id, AID.EmpyrealVortexCast, delay, 4)
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
        Cast(id, AID.WarpCast, delay, 4)
            .ActivateOnEnter<Sleepga>();

        ComponentCondition<Sleepga>(id + 0x10, 6.5f, s => s.NumCasts > 0, "Safe corner")
            .DeactivateOnExit<Sleepga>();
    }

    private void OmegaJavelin(uint id, float delay)
    {
        CastStart(id, AID.OmegaJavelinCast, delay)
            .ActivateOnEnter<OmegaJavelin>()
            .ActivateOnEnter<OmegaJavelin2>();

        ComponentCondition<OmegaJavelin>(id + 0x10, 5.1f, j => j.NumFinishedSpreads > 0, "Spreads")
            .DeactivateOnExit<OmegaJavelin>();
        ComponentCondition<OmegaJavelin2>(id + 0x20, 4.5f, j => j.NumCasts > 0, "Puddles")
            .DeactivateOnExit<OmegaJavelin2>();
    }

    private void Duplicate2(uint id, float delay)
    {
        Cast(id, AID.DuplicateCast, delay, 2)
            .ActivateOnEnter<Duplicate2>();

        Cast(id + 0x10, AID.VisionsOfParadise, 2.1f, 7);

        ComponentCondition<Duplicate2>(id + 0x20, 4.1f, d => d.NumCasts > 0, "Safe tile")
            .DeactivateOnExit<Duplicate2>();
    }

    private void StellarBurst(uint id, float delay)
    {
        Cast(id, AID.StellarBurstCast, delay, 4)
            .ActivateOnEnter<StellarBurst>();

        ComponentCondition<StellarBurst>(id + 0x10, 7.6f, s => s.NumCasts > 0, "Stack")
            .DeactivateOnExit<StellarBurst>();
    }
}
