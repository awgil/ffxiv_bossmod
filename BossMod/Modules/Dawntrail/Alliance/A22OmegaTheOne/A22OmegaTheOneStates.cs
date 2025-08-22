namespace BossMod.Dawntrail.Alliance.A22OmegaTheOne;

class IonEfflux(BossModule module) : Components.RaidwideCast(module, AID.IonEfflux);
class Antimatter(BossModule module) : Components.SingleTargetCast(module, AID.Antimatter);
class EnergyRay(BossModule module) : Components.StandardAOEs(module, AID.EnergyRay1, new AOEShapeRect(40, 8));
class OmegaBlaster(BossModule module) : Components.GroupedAOEs(module, [AID.OmegaBlasterFirst, AID.OmegaBlasterSecond], new AOEShapeCone(50, 90.Degrees()), maxCasts: 1)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (IDs.Contains(spell.Action))
            Casters.SortBy(c => Module.CastFinishAt(c.CastInfo));
    }
}
class Crash(BossModule module) : Components.StandardAOEs(module, AID.Crash, new AOEShapeRect(40, 12));
class TractorBeam(BossModule module) : Components.Knockback(module, AID.TractorBeam)
{
    private readonly List<Actor> _casters = [];

    public override IEnumerable<Source> Sources(int slot, Actor actor) => _casters.Select(c => new Source(Arena.Center - new WDir(20, 0), 25, Module.CastFinishAt(c.CastInfo), null, 90.Degrees(), Kind.DirForward));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _casters.Add(caster);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _casters.Remove(caster);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var s in Sources(slot, actor))
            if (!IsImmune(slot, s.Activation))
                hints.AddForbiddenZone(new AOEShapeRect(25, 24), Arena.Center - new WDir(5, 0), 90.Degrees(), s.Activation);
    }
}
class AntiPersonnelMissile(BossModule module) : Components.SpreadFromCastTargets(module, AID.AntiPersonnelMissile, 6);

class SurfaceMissile(BossModule module) : Components.GenericAOEs(module, AID.SurfaceMissile)
{
    private readonly List<(WPos, DateTime)> _tiles = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.TakeWhileTime(_tiles, t => t.Item2, 1).Select(t => new AOEInstance(new AOEShapeRect(6, 10, 6), t.Item1, default, t.Item2));

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.SurfaceMissile)
        {
            var delay = _tiles.Count >= 8 ? 8.3f : _tiles.Count >= 4 ? 9.2f : 10.1f;
            _tiles.Add((actor.Position, WorldState.FutureTime(delay)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            if (_tiles.Count > 0)
                _tiles.RemoveAt(0);
        }
    }
}

class CitadelBuster(BossModule module) : Components.RaidwideCast(module, AID.CitadelBuster);
class HyperPulse(BossModule module) : Components.StandardAOEs(module, AID.HyperPulse, 24);
class ChemicalBomb(BossModule module) : Components.StandardAOEs(module, AID.ChemicalBomb, 20);

class A22OmegaTheOneStates : StateMachineBuilder
{
    private readonly A22OmegaTheOne _module;

    public A22OmegaTheOneStates(A22OmegaTheOne module) : base(module)
    {
        _module = module;

        SimplePhase(0, SinglePhase, "Boss death")
            .ActivateOnEnter<ArenaBounds>()
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed && _module.Ultima()?.IsDeadOrDestroyed == true;
    }

    private void SinglePhase(uint id)
    {
        Cast(id, AID.IonEfflux, 5.3f, 6.5f, "Raidwide")
            .ActivateOnEnter<IonEfflux>()
            .ActivateOnEnter<OmegaBlaster>()
            .ActivateOnEnter<Antimatter>()
            .DeactivateOnExit<IonEfflux>();

        ComponentCondition<Antimatter>(id + 0x10, 4.7f, a => a.NumCasts > 0, "Tankbusters")
            .DeactivateOnExit<Antimatter>();

        ComponentCondition<EnergyRay>(id + 0x20, 12.4f, e => e.NumCasts > 0, "Lasers")
            .ActivateOnEnter<EnergyRay>()
            .DeactivateOnExit<EnergyRay>();

        OmegaBlaster(id + 0x30, 3.5f);
        DetractorBeam(id + 0x100, 3.6f);

        ComponentCondition<AntiPersonnelMissile>(id + 0x200, 4.8f, p => p.NumFinishedSpreads > 0, "Spreads")
            .ActivateOnEnter<AntiPersonnelMissile>()
            .DeactivateOnExit<AntiPersonnelMissile>();

        SurfaceMissile(id + 0x1000, 13.4f);

        ComponentCondition<EnergyRay>(id + 0x2000, 5, e => e.NumCasts > 0, "Lasers")
            .ActivateOnEnter<EnergyRay>()
            .ActivateOnEnter<AntiPersonnelMissile>()
            .DeactivateOnExit<EnergyRay>();

        ComponentCondition<AntiPersonnelMissile>(id + 0x2010, 4.1f, p => p.NumFinishedSpreads > 0, "Spreads")
            .DeactivateOnExit<AntiPersonnelMissile>();

        ComponentCondition<EnergyRay>(id + 0x3000, 16.5f, e => e.NumCasts > 0, "Laser 1")
            .ActivateOnEnter<EnergyRay>()
            .ActivateOnEnter<ReflectedRay>()
            .DeactivateOnExit<EnergyRay>();

        ComponentCondition<ReflectedRay>(id + 0x3010, 0.7f, r => r.NumCasts > 0, "Laser 2")
            .DeactivateOnExit<ReflectedRay>();

        ComponentCondition<GuidedMissile>(id + 0x4000, 15.4f, g => g.NumCasts > 0, "Offset baits")
            .ActivateOnEnter<GuidedMissile>()
            .ActivateOnEnter<GuidedMissileBait>()
            .DeactivateOnExit<GuidedMissile>()
            .DeactivateOnExit<GuidedMissileBait>();

        ActorCast(id + 0x5000, _module.Ultima, AID.TractorFieldCast, 0.6f, 5, true, "Stun + bosses disappear")
            .ActivateOnEnter<MultiMissile1>()
            .ActivateOnEnter<MultiMissile2>()
            .ActivateOnEnter<CitadelSiege>()
            .ActivateOnEnter<CitadelSiegeArena>()
            .ActivateOnEnter<CitadelSiegeJump>()
            .SetHint(StateMachine.StateHint.DowntimeStart);

        ComponentCondition<ArenaBounds>(id + 0x5050, 24.4f, b => b.Ship2)
            .DeactivateOnExit<MultiMissile1>()
            .DeactivateOnExit<MultiMissile2>()
            .DeactivateOnExit<CitadelSiege>()
            .DeactivateOnExit<CitadelSiegeArena>()
            .DeactivateOnExit<CitadelSiegeJump>();

        ActorTargetable(id + 0x6000, _module.Ultima, true, 3.3f, "Ultima reappear")
            .SetHint(StateMachine.StateHint.DowntimeEnd);

        ActorCast(id + 0x6010, _module.Ultima, AID.CitadelBuster, 1.1f, 6, true, "Raidwide")
            .ActivateOnEnter<CitadelBuster>()
            .DeactivateOnExit<CitadelBuster>();

        Cast(id + 0x6100, AID.HyperPulse, 0.9f, 5, "Proximity")
            .ActivateOnEnter<HyperPulse>()
            .DeactivateOnExit<HyperPulse>();

        ActorTargetable(id + 0x6110, () => _module.PrimaryActor, true, 2, "Omega reappear");

        DetractorBeam(id + 0x10000, 5.3f);

        ComponentCondition<GuidedMissile>(id + 0x11000, 10.9f, g => g.NumCasts > 0, "Offset baits")
            .ActivateOnEnter<GuidedMissile>()
            .ActivateOnEnter<GuidedMissileBait>()
            .DeactivateOnExit<GuidedMissile>()
            .DeactivateOnExit<GuidedMissileBait>();

        ComponentCondition<EnergyRay>(id + 0x12000, 13.2f, e => e.NumCasts > 0, "Laser 1")
            .ActivateOnEnter<EnergyRay>()
            .ActivateOnEnter<ReflectedRay>()
            .ActivateOnEnter<AntiPersonnelMissile>()
            .DeactivateOnExit<EnergyRay>();

        ComponentCondition<ReflectedRay>(id + 0x12010, 0.7f, r => r.NumCasts > 0, "Laser 2");
        ComponentCondition<ReflectedRay>(id + 0x12020, 0.6f, r => r.NumCasts > 1, "Laser 3")
            .DeactivateOnExit<ReflectedRay>();

        ComponentCondition<AntiPersonnelMissile>(id + 0x12030, 4.8f, p => p.NumFinishedSpreads > 0, "Spreads")
            .DeactivateOnExit<AntiPersonnelMissile>();

        ComponentCondition<ChemicalBomb>(id + 0x13000, 9.2f, c => c.NumCasts > 0, "Gigaflare 1")
            .ActivateOnEnter<SurfaceMissile>()
            .ActivateOnEnter<ChemicalBomb>();

        ComponentCondition<ChemicalBomb>(id + 0x13010, 9, c => c.NumCasts >= 4, "Gigaflare 4");

        Timeout(id + 0xFF0000, 10000, "Repeat mechanics until death")
            .ActivateOnEnter<AntiPersonnelMissile>()
            .ActivateOnEnter<CitadelBuster>()
            .ActivateOnEnter<EnergyRay>()
            .ActivateOnEnter<ReflectedRay>()
            .ActivateOnEnter<OmegaBlaster>()
            .ActivateOnEnter<IonEfflux>()
            .ActivateOnEnter<Antimatter>()
            .ActivateOnEnter<TractorBeam>()
            .ActivateOnEnter<GuidedMissile>()
            .ActivateOnEnter<GuidedMissileBait>()
            .ActivateOnEnter<Crash>();
    }

    private void OmegaBlaster(uint id, float delay, bool activate = false)
    {
        ComponentCondition<OmegaBlaster>(id, delay, o => o.NumCasts > 0, "Cleave 1")
            .ActivateOnEnter<OmegaBlaster>(activate);
        ComponentCondition<OmegaBlaster>(id + 1, 2.3f, o => o.NumCasts > 1, "Cleave 2")
            .DeactivateOnExit<OmegaBlaster>();
    }

    private void DetractorBeam(uint id, float delay)
    {
        ActorCastStart(id, _module.Ultima, AID.TractorBeamVisual, delay, true);

        ComponentCondition<TractorBeam>(id + 0x10, 10.6f, t => t.NumCasts > 0, "Knockback + crash")
            .ActivateOnEnter<TractorBeam>()
            .ActivateOnEnter<Crash>()
            .DeactivateOnExit<TractorBeam>()
            .DeactivateOnExit<Crash>();
    }

    private void SurfaceMissile(uint id, float delay, bool activate = true)
    {
        ComponentCondition<SurfaceMissile>(id, delay, s => s.NumCasts >= 4, "Tiles 1")
            .ActivateOnEnter<SurfaceMissile>(activate);
        ComponentCondition<SurfaceMissile>(id + 1, 2.1f, s => s.NumCasts >= 8, "Tiles 2");
        ComponentCondition<SurfaceMissile>(id + 2, 2.1f, s => s.NumCasts >= 12, "Tiles 3")
            .DeactivateOnExit<SurfaceMissile>();
    }
}
