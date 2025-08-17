using Lumina.Extensions;

namespace BossMod.Dawntrail.Alliance.A22OmegaTheOne;

class IonEfflux(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_IonEfflux);
class Antimatter(BossModule module) : Components.SingleTargetCast(module, AID._Spell_Antimatter);
class EnergyRay(BossModule module) : Components.StandardAOEs(module, AID._Spell_EnergyRay, new AOEShapeRect(40, 8));
class OmegaBlaster(BossModule module) : Components.GroupedAOEs(module, [AID._Spell_OmegaBlaster, AID._Spell_OmegaBlaster1], new AOEShapeCone(50, 90.Degrees()), maxCasts: 1);
class Crash(BossModule module) : Components.StandardAOEs(module, AID._Ability_Crash, new AOEShapeRect(40, 12));
class TractorBeam(BossModule module) : Components.Knockback(module, AID._Spell_TractorBeam)
{
    private readonly List<Actor> _casters = [];

    public override IEnumerable<Source> Sources(int slot, Actor actor) => _casters.Select(c => new Source(new(780, 800), 25, Module.CastFinishAt(c.CastInfo), null, 90.Degrees(), Kind.DirForward));

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
                hints.AddForbiddenZone(new AOEShapeRect(25, 24), new WPos(795, 800), 90.Degrees(), s.Activation);
    }
}
class AntiPersonnelMissile(BossModule module) : Components.SpreadFromCastTargets(module, AID._Weaponskill_AntiPersonnelMissile1, 6);

// 56.71
// 59.71
// 62.73

// 66.84 (10.1)
// 68.94 (9.2)
// 71.06 (8.3)
class SurfaceMissile(BossModule module) : Components.GenericAOEs(module, AID._Spell_SurfaceMissile)
{
    private readonly List<(WPos, DateTime)> _tiles = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.TakeWhileTime(_tiles, t => t.Item2, 1).Select(t => new AOEInstance(new AOEShapeRect(6, 10, 6), t.Item1, default, t.Item2));

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID._Gen_Icon_z6r2_b2_lock_c0w)
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

class ManaScreen(BossModule module) : BossComponent(module)
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var ne in Module.Enemies(OID.ManaScreenNESW))
            Arena.AddLine(ne.Position + new WDir(-10, 8), ne.Position + new WDir(10, -8), ArenaColor.Object, 2);
        foreach (var nw in Module.Enemies(OID.ManaScreenNWSE))
            Arena.AddLine(nw.Position + new WDir(10, 8), nw.Position + new WDir(-10, -8), ArenaColor.Object, 2);
    }
}

class ReflectedRay(BossModule module) : Components.GenericAOEs(module)
{
    record struct Ray(WPos Origin, float Width, Angle Angle, DateTime Activation);
    private readonly List<Ray> _rays = [];

    private IEnumerable<Actor> Mirrors => Module.Enemies(OID.ManaScreenNESW).Concat(Module.Enemies(OID.ManaScreenNWSE));

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _rays.Select(r => new AOEInstance(new AOEShapeRect(60, r.Width), r.Origin, r.Angle, r.Activation));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Spell_EnergyRay)
        {
            if (Mirrors.FirstOrDefault(m => m.Position.InRect(caster.Position, caster.Rotation, 40, 0, 8)) is { } mirror)
            {
                var angleStart = caster.Rotation.ToDirection();
                var angle = mirror.OID == (uint)OID.ManaScreenNESW ? angleStart.OrthoL() : angleStart.OrthoR();
                _rays.Add(new(mirror.Position, 10, angle.ToAngle(), Module.CastFinishAt(spell, 0.7f)));

                if (Mirrors.Exclude(mirror).FirstOrDefault(m => m.Position.InRect(mirror.Position, angle.ToAngle(), 60, 0, 10)) is { } mirror2)
                {
                    var angle2 = mirror2.OID == (uint)OID.ManaScreenNESW ? angle.OrthoR() : angle.OrthoL();
                    _rays.Add(new(mirror2.Position, 8, angle2.ToAngle(), Module.CastFinishAt(spell, 1.3f)));
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Spell_EnergyRay1 or AID._Spell_EnergyRay2 or AID._Spell_EnergyRay3)
        {
            _rays.RemoveAll(r => r.Origin.AlmostEqual(caster.Position, 1) && r.Angle.AlmostEqual(spell.Rotation, 0.1f));
            NumCasts++;
        }
    }
}

class GuidedMissile(BossModule module) : Components.StandardAOEs(module, AID._Spell_GuidedMissile, 6);
class GuidedMissileBait(BossModule module) : Components.CastCounter(module, AID._Spell_GuidedMissile)
{
    private BitMask _targetsMask;

    record struct Bait(Actor Target, WDir Offset, DateTime Activation)
    {
        public readonly bool Clips(Actor a) => a.Position.InCircle(Target.Position + Offset, 6);
    }

    private readonly List<Bait> _baits = [];

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        switch ((IconID)iconID)
        {
            case IconID._Gen_Icon_z6r2_trg_e_t0w:
                AddBait(actor, new(5, 0));
                break;
            case IconID._Gen_Icon_z6r2_trg_w_t0w:
                AddBait(actor, new(-5, 0));
                break;
            case IconID._Gen_Icon_z6r2_trg_s_t0w:
                AddBait(actor, new(0, 5));
                break;
            case IconID._Gen_Icon_z6r2_trg_n_t0w:
                AddBait(actor, new(0, -5));
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            _baits.Clear();
            _targetsMask.Reset();
        }
    }

    private void AddBait(Actor actor, WDir dir)
    {
        _targetsMask.Set(Raid.FindSlot(actor.InstanceID));
        _baits.Add(new(actor, dir, WorldState.FutureTime(9.8f)));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_baits.FirstOrNull(b => b.Target == actor) is { } thisBait)
        {
            if (Raid.WithoutSlot().Exclude(actor).Any(thisBait.Clips))
                hints.Add("Bait away from party!");
        }

        if (_baits.Any(b => b.Target != actor && b.Clips(actor)))
            hints.Add("GTFO from baited AOE!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_baits.FirstOrNull(b => b.Target == actor) is { } thisBait)
        {
            List<Func<WPos, bool>> selfShape = [];
            foreach (var r in Raid.WithoutSlot().Exclude(actor))
                selfShape.Add(ShapeContains.Circle(r.Position - thisBait.Offset, 6));

            if (selfShape.Count > 0)
                hints.AddForbiddenZone(ShapeContains.Union(selfShape), thisBait.Activation);
        }

        List<Func<WPos, bool>> othersShape = [];
        foreach (var b in _baits.Where(b => b.Target != actor))
            othersShape.Add(ShapeContains.Circle(b.Target.Position + b.Offset, 6));

        if (othersShape.Count > 0)
            hints.AddForbiddenZone(ShapeContains.Union(othersShape), _baits[0].Activation);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var b in _baits)
        {
            if (Arena.Config.ShowOutlinesAndShadows)
                Arena.AddCircle(b.Target.Position + b.Offset, 6, 0xFF000000, 2);
            Arena.AddCircle(b.Target.Position + b.Offset, 6, ArenaColor.Danger);
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => _targetsMask[playerSlot] ? PlayerPriority.Danger : base.CalcPriority(pcSlot, pc, playerSlot, player, ref customColor);
}

class ArenaBounds(BossModule module) : BossComponent(module)
{
    public bool Ship2 { get; private set; }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x25 && state == 0x00020001)
        {
            Ship2 = true;
            Arena.Center = new(735, 800);
            Arena.Bounds = new ArenaBoundsRect(20, 23.8f);
        }
    }
}

class MultiMissile1(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_MultiMissile1, 6);
class MultiMissile2(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_MultiMissile2, 10);
class CitadelSiege(BossModule module) : Components.StandardAOEs(module, AID._Spell_CitadelSiege2, new AOEShapeRect(48, 5));
class CitadelSiegeArena(BossModule module) : Components.GenericAOEs(module)
{
    private BitMask _arena;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var b in _arena.SetBits())
        {
            var center = new WPos(815 - 10 * b, 800);
            yield return new(new AOEShapeRect(24, 5, 24), center);
        }
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x18)
        {
            switch (state)
            {
                case 0x00020001:
                    _arena.Set(0);
                    break;
                case 0x00200010:
                    _arena.Set(1);
                    break;
                case 0x00800040:
                    _arena.Set(2);
                    break;
                case 0x02000100:
                    _arena.Set(3);
                    break;
            }
        }
    }
}

class CitadelSiegeJump(BossModule module) : BossComponent(module)
{
    private DateTime _deadline;

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x19 && state == 0x00020001)
        {
            _deadline = WorldState.FutureTime(21.3f);

            var arenaRect = CurveApprox.Rect(new(20, 0), new(0, 23.8f));
            var doubleCenter = new WPos(767.5f, 800);

            var b1 = new RelSimplifiedComplexPolygon([new RelPolygonWithHoles([.. arenaRect.Select(r => r + new WDir(32.5f, 0))]), new RelPolygonWithHoles([.. arenaRect.Select(r => r - new WDir(32.5f, 0))])]);
            Arena.Center = doubleCenter;
            Arena.Bounds = new ArenaBoundsCustom(52.5f, b1);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_deadline != default && pc.Position.X >= 780)
            Arena.ZoneRect(new WPos(780, 800), new WPos(785, 800), 24, ArenaColor.SafeFromAOE);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_deadline != default && actor.Position.X >= 780)
            hints.Add("Jump!", false);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_deadline != default && actor.Position.X >= 780)
            hints.AddForbiddenZone(p => p.X > 785, _deadline);
    }
}

class CitadelBuster(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_CitadelBuster);
class HyperPulse(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_HyperPulse, 24);
class ChemicalBomb(BossModule module) : Components.StandardAOEs(module, AID._Spell_ChemicalBomb, 20);

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
        Cast(id, AID._Weaponskill_IonEfflux, 5.3f, 6.5f, "Raidwide")
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

        ActorCast(id + 0x5000, _module.Ultima, AID._Weaponskill_TractorField, 0.6f, 5, true, "Stun + bosses disappear")
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

        ActorCast(id + 0x6010, _module.Ultima, AID._Weaponskill_CitadelBuster, 1.1f, 6, true, "Raidwide")
            .ActivateOnEnter<CitadelBuster>()
            .DeactivateOnExit<CitadelBuster>();

        Cast(id + 0x6100, AID._Weaponskill_HyperPulse, 0.9f, 5, "Proximity")
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

        ComponentCondition<AntiPersonnelMissile>(id + 0x12030, 4.7f, p => p.NumFinishedSpreads > 0, "Spreads")
            .DeactivateOnExit<AntiPersonnelMissile>();

        ComponentCondition<ChemicalBomb>(id + 0x13000, 9.2f, c => c.NumCasts > 0, "Gigaflare 1")
            .ActivateOnEnter<ChemicalBomb>();

        ComponentCondition<ChemicalBomb>(id + 0x13010, 9, c => c.NumCasts >= 4, "Gigaflare 4");

        Timeout(id + 0xFF0000, 10000, "Repeat mechanics until death")
            .ActivateOnEnter<SurfaceMissile>()
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
        ActorCastStart(id, _module.Ultima, AID._Weaponskill_TractorBeam, delay, true);

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
