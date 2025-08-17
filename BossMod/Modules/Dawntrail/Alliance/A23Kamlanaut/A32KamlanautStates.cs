namespace BossMod.Dawntrail.Alliance.A23Kamlanaut;

class EnspiritedSwordplay(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_EnspiritedSwordplay);
class ProvingGround(BossModule module) : Components.GenericAOEs(module, AID._Spell_ProvingGround)
{
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
            yield return new AOEInstance(new AOEShapeCircle(5), Arena.Center, Activation: _activation);
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID == OID.ProvingGround)
            _activation = default;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _activation = Module.CastFinishAt(spell);
    }
}

class PG(BossModule module) : BossComponent(module)
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var e in Module.Enemies(OID.ProvingGround))
        {
            Arena.Actor(e.Position, e.Rotation, ArenaColor.Object);
            Arena.TextWorld(e.Position, $"{e.ModelState} {e.EventState}", ArenaColor.Object);
        }
    }
}

class ElementalBladeSmall(BossModule module) : Components.GroupedAOEs(module, [AID._Spell_FireBlade, AID._Spell_EarthBlade, AID._Spell_WaterBlade, AID._Spell_IceBlade, AID._Spell_LightningBlade1, AID._Spell_WindBlade1], new AOEShapeRect(80, 2.5f));
class ElementalBladeLarge(BossModule module) : Components.GroupedAOEs(module, [AID._Spell_FireBlade1, AID._Spell_EarthBlade1, AID._Spell_WaterBlade1, AID._Spell_IceBlade1, AID._Spell_LightningBlade, AID._Spell_WindBlade], new AOEShapeRect(80, 10));
class SublimeElementSmall(BossModule module) : Components.GroupedAOEs(module, [AID._Spell_SublimeFire1, AID._Spell_SublimeEarth, AID._Spell_SublimeWater, AID._Spell_SublimeIce, AID._Spell_SublimeLightning, AID._Spell_SublimeWind1], new AOEShapeCone(40, 11.Degrees()));
class SublimeElementLarge(BossModule module) : Components.GroupedAOEs(module, [AID._Spell_SublimeFire, AID._Spell_SublimeEarth1, AID._Spell_SublimeWater1, AID._Spell_SublimeIce1, AID._Spell_SublimeLightning1, AID._Spell_SublimeWind], new AOEShapeCone(40, 49.Degrees()));

class ElementalCounter(BossModule module) : Components.CastCounterMulti(module, [AID._Spell_FireBlade, AID._Spell_EarthBlade, AID._Spell_WaterBlade, AID._Spell_IceBlade, AID._Spell_LightningBlade1, AID._Spell_WindBlade1, AID._Spell_FireBlade1, AID._Spell_EarthBlade1, AID._Spell_WaterBlade1, AID._Spell_IceBlade1, AID._Spell_LightningBlade, AID._Spell_WindBlade, AID._Spell_SublimeFire1, AID._Spell_SublimeEarth, AID._Spell_SublimeWater, AID._Spell_SublimeIce, AID._Spell_SublimeLightning, AID._Spell_SublimeWind1, AID._Spell_SublimeFire, AID._Spell_SublimeEarth1, AID._Spell_SublimeWater1, AID._Spell_SublimeIce1, AID._Spell_SublimeLightning1, AID._Spell_SublimeWind]);

class PrincelyBlow(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(60, 5), (uint)IconID._Gen_Icon_z6r2b3_8sec_lockon_c0a1, AID._Weaponskill_PrincelyBlow1, 8.3f, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    public bool BridgePhase;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (BridgePhase)
        {
            // during bridge phase, we only try to avoid other tanks, and ignore allies (it's their job to make room for us)
            var predicted = new BitMask();
            foreach (var b in ActiveBaits)
            {
                predicted.Set(Raid.FindSlot(b.Target.InstanceID));
                if (b.Target != actor)
                    hints.AddForbiddenZone(b.Shape, BaitOrigin(b), b.Rotation, b.Activation);
            }

            if (predicted.Any())
                hints.AddPredictedDamage(predicted, CurrentBaits[0].Activation, DamageType);
        }
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }
}
class PrincelyBlowKnockback(BossModule module) : Components.Knockback(module, AID._Weaponskill_PrincelyBlow1)
{
    private DateTime _activation;
    private readonly List<Actor> _targets = [];

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        foreach (var t in _targets)
            yield return new(Module.PrimaryActor.Position, 30, _activation, new AOEShapeRect(60, 5), Module.PrimaryActor.AngleTo(t), Kind: Kind.DirForward);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_targets.Contains(actor) && !StopAtWall && !IsImmune(slot, _activation))
            hints.AddForbiddenZone(ShieldBash.SafetyShape(Module.PrimaryActor.Position), _activation);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID._Gen_Icon_z6r2b3_8sec_lockon_c0a1 && WorldState.Actors.Find(targetID) is { } tar)
        {
            _activation = WorldState.FutureTime(8.3f);
            _targets.Add(tar);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _targets.Clear();
        }
    }
}

class LightBlade(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_LightBlade, new AOEShapeRect(120, 6.5f));

// 83.785 -> 88.85
class SublimeEstoc(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_SublimeEstoc)
{
    private readonly List<(Actor, DateTime)> _casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _casters.Select(c => new AOEInstance(new AOEShapeRect(40, 2.5f), c.Item1.Position, c.Item1.Rotation, c.Item2));

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID._Gen_SublimeEstoc && id == 0x2488)
            _casters.Add((actor, WorldState.FutureTime(5.1f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _casters.RemoveAll(c => c.Item1 == caster);
        }
    }
}

class GreatWheel(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_GreatWheel, AID._Weaponskill_GreatWheel2, AID._Weaponskill_GreatWheel3, AID._Weaponskill_GreatWheel4], new AOEShapeCircle(10));
class GreatWheelCleave(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_GreatWheel1, new AOEShapeCone(80, 90.Degrees()));
class Fetters(BossModule module) : Components.CastCounter(module, default)
{
    public bool Finished;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == 3324)
            NumCasts++;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == 3324)
            Finished = true;
    }
}

class ArenaBounds(BossModule module) : BossComponent(module)
{
    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 2 && state == 0x00010001)
        {
            var arenaSmall = CurveApprox.Circle(20, 1 / 90f);
            IEnumerable<WDir> bridge(Angle a) => CurveApprox.Rect(new(5, 0), new(0, 20)).Select(d => (d + new WDir(0, 20)).Rotate(a));

            var oper = new PolygonClipper.Operand(bridge(180.Degrees()));
            oper.AddContour(bridge(60.Degrees()));
            oper.AddContour(bridge(-60.Degrees()));

            var arenaBig = Arena.Bounds.Clipper.Union(new(arenaSmall), oper);
            Arena.Bounds = new ArenaBoundsCustom(30, arenaBig);
        }
    }
}

class TranscendentUnion(BossModule module) : Components.RaidwideCastDelay(module, AID._Weaponskill_TranscendentUnion, AID._Spell_TranscendentUnion, 6.6f);

class ElementalResonance(BossModule module) : Components.StandardAOEs(module, AID._Spell_ElementalResonance, 18);
class EmpyrealBanishIII(BossModule module) : Components.SpreadFromCastTargets(module, AID._Spell_EmpyrealBanishIII, 5);
class IllumedEstoc(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_IllumedEstoc, new AOEShapeRect(120, 6.5f));
class ShieldBash(BossModule module) : Components.KnockbackFromCastTarget(module, AID._Weaponskill_ShieldBash, 30)
{
    private static readonly float _platformSafeRad = MathF.Atan2(5, 40);

    public static Func<WPos, bool> SafetyShape(WPos origin) => p =>
    {
        var d = p - origin;
        var angle = d.ToAngle();
        return !angle.AlmostEqual(180.Degrees(), _platformSafeRad) && !angle.AlmostEqual(60.Degrees(), _platformSafeRad) && !angle.AlmostEqual(-60.Degrees(), _platformSafeRad) || d.LengthSq() >= 100;
    };

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var src in Sources(slot, actor))
            if (!IsImmune(slot, src.Activation))
                hints.AddForbiddenZone(SafetyShape(src.Origin), src.Activation);
    }
}
class EmpyrealBanishIV(BossModule module) : Components.StackWithCastTargets(module, AID._Spell_EmpyrealBanishIV1, 5);

class A23KamlanautStates : StateMachineBuilder
{
    public A23KamlanautStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase)
            .ActivateOnEnter<EnspiritedSwordplay>()
            .ActivateOnEnter<LightBlade>()
            .ActivateOnEnter<ArenaBounds>();
    }

    private void SinglePhase(uint id)
    {
        Cast(id, AID._Weaponskill_EnspiritedSwordplay, 6.2f, 5, "Raidwide");
        Cast(id + 0x100, AID._Spell_ProvingGround, 5.6f, 3, "Puddle appear")
            .ActivateOnEnter<ProvingGround>();

        SublimeElements2(id + 0x10000, 5.1f);
        PrincelyBlow(id + 0x20000, 2.2f);
        Cast(id + 0x30000, AID._Weaponskill_LightBlade, 2.2f, 3, "Line AOE")
            .ActivateOnEnter<SublimeEstoc>();
        ComponentCondition<SublimeEstoc>(id + 0x30010, 6.1f, s => s.NumCasts > 0, "Swords")
            .DeactivateOnExit<SublimeEstoc>();

        CastMulti(id + 0x30100, [AID._Weaponskill_GreatWheel, AID._Weaponskill_GreatWheel2, AID._Weaponskill_GreatWheel3, AID._Weaponskill_GreatWheel4], 0.1f, 3, "Circle")
            .ActivateOnEnter<GreatWheel>()
            .ActivateOnEnter<GreatWheelCleave>();

        ComponentCondition<GreatWheelCleave>(id + 0x30110, 5.9f, c => c.NumCasts > 0, "Half-arena cleave")
            .DeactivateOnExit<GreatWheel>()
            .DeactivateOnExit<GreatWheelCleave>()
            .DeactivateOnExit<ProvingGround>();

        Cast(id + 0x30200, AID._Weaponskill_EnspiritedSwordplay, 2.3f, 5, "Raidwide");
        Cast(id + 0x30300, AID._Weaponskill_EsotericScrivening, 14.5f, 6)
            .ActivateOnEnter<Fetters>();

        ComponentCondition<Fetters>(id + 0x30400, 6.1f, f => f.NumCasts > 0, "Stun")
            .SetHint(StateMachine.StateHint.DowntimeStart);

        ComponentCondition<Fetters>(id + 0x30500, 10, f => f.Finished, "Stun end")
            .SetHint(StateMachine.StateHint.DowntimeEnd);

        // 6.6
        Cast(id + 0x40000, AID._Weaponskill_TranscendentUnion, 11, 5, "Raidwides start")
            .ActivateOnEnter<TranscendentUnion>();

        ComponentCondition<TranscendentUnion>(id + 0x40010, 6.6f, t => t.NumCasts > 0, "Raidwides end")
            .DeactivateOnExit<TranscendentUnion>();

        Cast(id + 0x40100, AID._Weaponskill_EsotericPalisade, 13, 3)
            .ActivateOnEnter<EmpyrealBanishIII>();

        ElementalResonance(id + 0x40200, 3.1f);

        ComponentCondition<EmpyrealBanishIII>(id + 0x40300, 0.3f, b => b.NumFinishedSpreads > 0, "Spreads")
            .DeactivateOnExit<EmpyrealBanishIII>();

        ComponentCondition<IllumedEstoc>(id + 0x40400, 10.9f, i => i.NumCasts > 0, "Clones")
            .ActivateOnEnter<IllumedEstoc>()
            .ActivateOnEnter<ShieldBash>();

        Cast(id + 0x40500, AID._Weaponskill_ShieldBash, 10, 7, "Knockback")
            .DeactivateOnExit<ShieldBash>();

        Cast(id + 0x40600, AID._Spell_EmpyrealBanishIV, 5.2f, 5)
            .ActivateOnEnter<EmpyrealBanishIV>();
        ComponentCondition<EmpyrealBanishIV>(id + 0x40610, 1, b => b.NumFinishedStacks > 0, "Stack")
            .DeactivateOnExit<EmpyrealBanishIV>();

        PrincelyBlow(id + 0x50000, 2.2f, true);

        Cast(id + 0x50100, AID._Spell_ProvingGround, 11.4f, 3, "Puddle appear")
            .ActivateOnEnter<ProvingGround>();

        SublimeElements1(id + 0x51000, 3.1f);
        Cast(id + 0x52000, AID._Weaponskill_LightBlade, 3.2f, 3, "Line AOE")
            .ActivateOnEnter<EmpyrealBanishIII>()
            .ActivateOnEnter<SublimeEstoc>();

        ComponentCondition<EmpyrealBanishIII>(id + 0x52100, 6, b => b.NumFinishedSpreads > 0, "Spreads + swords")
            .DeactivateOnExit<EmpyrealBanishIII>();

        CastMulti(id + 0x52200, [AID._Weaponskill_GreatWheel, AID._Weaponskill_GreatWheel2, AID._Weaponskill_GreatWheel3, AID._Weaponskill_GreatWheel4], 0.1f, 3, "Circle")
            .ActivateOnEnter<EmpyrealBanishIV>()
            .ActivateOnEnter<GreatWheel>()
            .ActivateOnEnter<GreatWheelCleave>();

        Cast(id + 0x52300, AID._Weaponskill_EnspiritedSwordplay, 8.2f, 5, "Raidwide");

        Timeout(id + 0xFF0000, 10000, "Repeat mechanics until death")
            .ActivateOnEnter<ElementalResonance>()
            .ActivateOnEnter<PrincelyBlow>()
            .ActivateOnEnter<PrincelyBlowKnockback>()
            .ActivateOnEnter<ShieldBash>()
            .ActivateOnEnter<EmpyrealBanishIII>()
            .ActivateOnEnter<ElementalBladeSmall>()
            .ActivateOnEnter<ElementalBladeLarge>()
            .ActivateOnEnter<SublimeElementLarge>()
            .ActivateOnEnter<SublimeElementSmall>();
    }

    private void SublimeElements2(uint id, float delay)
    {
        Cast(id, AID._Weaponskill_ElementalBlade, delay, 8);
        Cast(id + 0x10, AID._Weaponskill_SublimeElements, 3.1f, 8)
            .ActivateOnEnter<ElementalCounter>()
            .ActivateOnEnter<ElementalBladeSmall>()
            .ActivateOnEnter<ElementalBladeLarge>()
            .ActivateOnEnter<SublimeElementLarge>()
            .ActivateOnEnter<SublimeElementSmall>();
        ComponentCondition<ElementalCounter>(id + 0x20, 1, c => c.NumCasts >= 6, "Safe spot");

        Cast(id + 0x100, AID._Weaponskill_ElementalBlade, 2.2f, 8);
        Cast(id + 0x110, AID._Weaponskill_SublimeElements, 3.1f, 8);
        ComponentCondition<ElementalCounter>(id + 0x120, 1, c => c.NumCasts >= 12, "Safe spot")
            .DeactivateOnExit<ElementalCounter>()
            .DeactivateOnExit<ElementalBladeSmall>()
            .DeactivateOnExit<ElementalBladeLarge>()
            .DeactivateOnExit<SublimeElementLarge>()
            .DeactivateOnExit<SublimeElementSmall>();
    }

    private void SublimeElements1(uint id, float delay)
    {
        Cast(id, AID._Weaponskill_ElementalBlade1, delay, 11);
        Cast(id + 0x10, AID._Weaponskill_SublimeElements, 3.1f, 8)
            .ActivateOnEnter<ElementalCounter>()
            .ActivateOnEnter<ElementalBladeSmall>()
            .ActivateOnEnter<ElementalBladeLarge>()
            .ActivateOnEnter<SublimeElementLarge>()
            .ActivateOnEnter<SublimeElementSmall>();
        ComponentCondition<ElementalCounter>(id + 0x20, 1, c => c.NumCasts >= 6, "Safe spot")
            .DeactivateOnExit<ElementalCounter>()
            .DeactivateOnExit<ElementalBladeSmall>()
            .DeactivateOnExit<ElementalBladeLarge>()
            .DeactivateOnExit<SublimeElementLarge>()
            .DeactivateOnExit<SublimeElementSmall>();
    }

    private void ElementalResonance(uint id, float delay, int numPuddles = 1)
    {
        Cast(id, AID._Weaponskill_CrystallineResonance, delay, 3, "Crystals start")
            .ActivateOnEnter<ElementalResonance>();

        ComponentCondition<ElementalResonance>(id + 2, 7.9f, e => e.NumCasts > 0, "Crystal 1");
        ComponentCondition<ElementalResonance>(id + 3, 7.1f, e => e.NumCasts > numPuddles, "Crystal 2");
        ComponentCondition<ElementalResonance>(id + 4, 7, e => e.NumCasts > numPuddles * 2, "Crystal 3")
            .DeactivateOnExit<ElementalResonance>();
    }

    private void PrincelyBlow(uint id, float delay, bool bridgePhase = false)
    {
        CastStart(id, AID._Weaponskill_PrincelyBlow, delay)
            .ActivateOnEnter<PrincelyBlow>()
            .ActivateOnEnter<PrincelyBlowKnockback>()
            .ExecOnEnter<PrincelyBlow>(b => b.BridgePhase = bridgePhase)
            .ExecOnEnter<PrincelyBlowKnockback>(k => k.StopAtWall = !bridgePhase);

        ComponentCondition<PrincelyBlow>(id + 0x10, 8.2f, p => p.NumCasts > 0, "Tankbusters")
            .DeactivateOnExit<PrincelyBlow>()
            .DeactivateOnExit<PrincelyBlowKnockback>();
    }

    //private void XXX(uint id, float delay)
}
