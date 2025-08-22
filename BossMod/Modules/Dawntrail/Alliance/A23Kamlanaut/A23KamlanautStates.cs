namespace BossMod.Dawntrail.Alliance.A23Kamlanaut;

class EnspiritedSwordplay(BossModule module) : Components.RaidwideCast(module, AID.EnspiritedSwordplay);
class ElementalBladeSmall(BossModule module) : Components.GroupedAOEs(module, [AID.FireBladeSmall, AID.EarthBladeSmall, AID.WaterBladeSmall, AID.IceBladeSmall, AID.LightningBladeSmall, AID.WindBladeSmall], new AOEShapeRect(80, 2.5f));
class ElementalBladeLarge(BossModule module) : Components.GroupedAOEs(module, [AID.FireBladeLarge, AID.EarthBladeLarge, AID.WaterBladeLarge, AID.IceBladeLarge, AID.LightningBladeLarge, AID.WindBladeLarge], new AOEShapeRect(80, 10));
class SublimeElementSmall(BossModule module) : Components.GroupedAOEs(module, [AID.SublimeFireSmall, AID.SublimeEarthSmall, AID.SublimeWaterSmall, AID.SublimeIceSmall, AID.SublimeLightningSmall, AID.SublimeWindSmall], new AOEShapeCone(40, 11.Degrees()));
class SublimeElementLarge(BossModule module) : Components.GroupedAOEs(module, [AID.SublimeFireLarge, AID.SublimeEarthLarge, AID.SublimeWaterLarge, AID.SublimeIceLarge, AID.SublimeLightningLarge, AID.SublimeWindLarge], new AOEShapeCone(40, 49.Degrees()));

class ElementalCounter(BossModule module) : Components.CastCounterMulti(module, [AID.FireBladeSmall, AID.EarthBladeSmall, AID.WaterBladeSmall, AID.IceBladeSmall, AID.LightningBladeSmall, AID.WindBladeSmall, AID.FireBladeLarge, AID.EarthBladeLarge, AID.WaterBladeLarge, AID.IceBladeLarge, AID.LightningBladeLarge, AID.WindBladeLarge, AID.SublimeFireSmall, AID.SublimeEarthSmall, AID.SublimeWaterSmall, AID.SublimeIceSmall, AID.SublimeLightningSmall, AID.SublimeWindSmall, AID.SublimeFireLarge, AID.SublimeEarthLarge, AID.SublimeWaterLarge, AID.SublimeIceLarge, AID.SublimeLightningLarge, AID.SublimeWindLarge]);

class LightBlade(BossModule module) : Components.StandardAOEs(module, AID.LightBlade, new AOEShapeRect(120, 6.5f));

class SublimeEstoc(BossModule module) : Components.GenericAOEs(module, AID.SublimeEstoc)
{
    private readonly List<(Actor, DateTime)> _casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _casters.Select(c => new AOEInstance(new AOEShapeRect(40, 2.5f), c.Item1.Position, c.Item1.Rotation, c.Item2));

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.SublimeEstoc && id == 0x2488)
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

class GreatWheel(BossModule module) : Components.GroupedAOEs(module, [AID.GreatWheel3, AID.GreatWheel1, AID.GreatWheel2, AID.GreatWheel4], new AOEShapeCircle(10));
class GreatWheelCleave(BossModule module) : Components.StandardAOEs(module, AID.GreatWheelCleave, new AOEShapeCone(80, 90.Degrees()));
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

class Shockwave(BossModule module) : Components.RaidwideCast(module, AID.Shockwave);
class TranscendentUnion(BossModule module) : Components.RaidwideCastDelay(module, AID.TranscendentUnionCast, AID.TranscendentUnion, 6.6f);
class ElementalResonance(BossModule module) : Components.StandardAOEs(module, AID.ElementalResonance, 18);
class EmpyrealBanishIII(BossModule module) : Components.SpreadFromCastTargets(module, AID.EmpyrealBanishIII, 5);
class IllumedEstoc(BossModule module) : Components.StandardAOEs(module, AID.IllumedEstoc, new AOEShapeRect(120, 6.5f));
class ShieldBash(BossModule module) : Components.KnockbackFromCastTarget(module, AID.ShieldBash, 30)
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
class EmpyrealBanishIV(BossModule module) : Components.StackWithCastTargets(module, AID.EmpyrealBanishIV, 5);

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
        Cast(id, AID.EnspiritedSwordplay, 6.2f, 5, "Raidwide");
        Cast(id + 0x100, AID.ProvingGround, 5.6f, 3, "Puddle appear")
            .ActivateOnEnter<ProvingGround>();

        SublimeElements2(id + 0x10000, 5.1f);
        PrincelyBlow(id + 0x20000, 2.2f);
        Cast(id + 0x30000, AID.LightBlade, 2.2f, 3, "Line AOE")
            .ActivateOnEnter<SublimeEstoc>();
        ComponentCondition<SublimeEstoc>(id + 0x30010, 6.1f, s => s.NumCasts > 0, "Swords")
            .DeactivateOnExit<SublimeEstoc>();

        CastMulti(id + 0x30100, [AID.GreatWheel3, AID.GreatWheel1, AID.GreatWheel2, AID.GreatWheel4], 0.1f, 3, "Circle")
            .ActivateOnEnter<GreatWheel>()
            .ActivateOnEnter<GreatWheelCleave>();

        ComponentCondition<GreatWheelCleave>(id + 0x30110, 5.9f, c => c.NumCasts > 0, "Half-arena cleave")
            .DeactivateOnExit<GreatWheel>()
            .DeactivateOnExit<GreatWheelCleave>()
            .DeactivateOnExit<ProvingGround>();

        Cast(id + 0x30200, AID.EnspiritedSwordplay, 2.3f, 5, "Raidwide");
        Cast(id + 0x30300, AID.EsotericScrivening, 14.5f, 6)
            .ActivateOnEnter<Fetters>();

        ComponentCondition<Fetters>(id + 0x30400, 6.1f, f => f.NumCasts > 0, "Stun")
            .SetHint(StateMachine.StateHint.DowntimeStart);

        ComponentCondition<Fetters>(id + 0x30500, 10, f => f.Finished, "Stun end")
            .SetHint(StateMachine.StateHint.DowntimeEnd);

        ComponentCondition<Shockwave>(id + 0x30600, 7.4f, s => s.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<Shockwave>()
            .DeactivateOnExit<Shockwave>();

        Cast(id + 0x40000, AID.TranscendentUnionCast, 3.7f, 5, "Raidwides start")
            .ActivateOnEnter<TranscendentUnion>();

        ComponentCondition<TranscendentUnion>(id + 0x40010, 6.6f, t => t.NumCasts > 0, "Raidwides end")
            .DeactivateOnExit<TranscendentUnion>();

        Cast(id + 0x40100, AID.EsotericPalisade, 13, 3)
            .ActivateOnEnter<EmpyrealBanishIII>();

        ElementalResonance(id + 0x40200, 3.1f);

        ComponentCondition<EmpyrealBanishIII>(id + 0x40300, 0.3f, b => b.NumFinishedSpreads > 0, "Spreads")
            .DeactivateOnExit<EmpyrealBanishIII>();

        ComponentCondition<IllumedEstoc>(id + 0x40400, 10.9f, i => i.NumCasts > 0, "Clones")
            .ActivateOnEnter<IllumedEstoc>()
            .ActivateOnEnter<ShieldBash>();

        Cast(id + 0x40500, AID.ShieldBash, 10, 7, "Knockback")
            .DeactivateOnExit<ShieldBash>();

        Cast(id + 0x40600, AID.EmpyrealBanishIVCast, 5.2f, 5)
            .ActivateOnEnter<EmpyrealBanishIV>();
        ComponentCondition<EmpyrealBanishIV>(id + 0x40610, 1, b => b.NumFinishedStacks > 0, "Stack")
            .DeactivateOnExit<EmpyrealBanishIV>();

        PrincelyBlow(id + 0x50000, 2.2f, true);

        Cast(id + 0x50100, AID.ProvingGround, 11.4f, 3, "Puddle appear")
            .ActivateOnEnter<ProvingGround>();

        SublimeElements1(id + 0x51000, 3.1f);
        Cast(id + 0x52000, AID.LightBlade, 3.2f, 3, "Line AOE")
            .ActivateOnEnter<EmpyrealBanishIII>()
            .ActivateOnEnter<SublimeEstoc>();

        ComponentCondition<EmpyrealBanishIII>(id + 0x52100, 6, b => b.NumFinishedSpreads > 0, "Spreads + swords")
            .DeactivateOnExit<EmpyrealBanishIII>();

        CastMulti(id + 0x52200, [AID.GreatWheel3, AID.GreatWheel1, AID.GreatWheel2, AID.GreatWheel4], 0.1f, 3, "Circle")
            .ActivateOnEnter<EmpyrealBanishIV>()
            .ActivateOnEnter<GreatWheel>()
            .ActivateOnEnter<GreatWheelCleave>();

        Cast(id + 0x52300, AID.EnspiritedSwordplay, 8.2f, 5, "Raidwide");

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
        Cast(id, AID.ElementalBladeFast, delay, 8);
        Cast(id + 0x10, AID.SublimeElements, 3.1f, 8)
            .ActivateOnEnter<ElementalCounter>()
            .ActivateOnEnter<ElementalBladeSmall>()
            .ActivateOnEnter<ElementalBladeLarge>()
            .ActivateOnEnter<SublimeElementLarge>()
            .ActivateOnEnter<SublimeElementSmall>();
        ComponentCondition<ElementalCounter>(id + 0x20, 1, c => c.NumCasts >= 6, "Safe spot");

        Cast(id + 0x100, AID.ElementalBladeFast, 2.2f, 8);
        Cast(id + 0x110, AID.SublimeElements, 3.1f, 8);
        ComponentCondition<ElementalCounter>(id + 0x120, 1, c => c.NumCasts >= 12, "Safe spot")
            .DeactivateOnExit<ElementalCounter>()
            .DeactivateOnExit<ElementalBladeSmall>()
            .DeactivateOnExit<ElementalBladeLarge>()
            .DeactivateOnExit<SublimeElementLarge>()
            .DeactivateOnExit<SublimeElementSmall>();
    }

    private void SublimeElements1(uint id, float delay)
    {
        Cast(id, AID.ElementalBladeSlow, delay, 11);
        Cast(id + 0x10, AID.SublimeElements, 3.1f, 8)
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
        Cast(id, AID.CrystallineResonance, delay, 3, "Crystals start")
            .ActivateOnEnter<ElementalResonance>();

        ComponentCondition<ElementalResonance>(id + 2, 7.9f, e => e.NumCasts > 0, "Crystal 1");
        ComponentCondition<ElementalResonance>(id + 3, 7.1f, e => e.NumCasts > numPuddles, "Crystal 2");
        ComponentCondition<ElementalResonance>(id + 4, 7, e => e.NumCasts > numPuddles * 2, "Crystal 3")
            .DeactivateOnExit<ElementalResonance>();
    }

    private void PrincelyBlow(uint id, float delay, bool bridgePhase = false)
    {
        CastStart(id, AID.PrincelyBlowCast, delay)
            .ActivateOnEnter<PrincelyBlow>()
            .ActivateOnEnter<PrincelyBlowKnockback>()
            .ExecOnEnter<PrincelyBlow>(b => b.BridgePhase = bridgePhase)
            .ExecOnEnter<PrincelyBlowKnockback>(k => k.StopAtWall = !bridgePhase);

        ComponentCondition<PrincelyBlow>(id + 0x10, 8.2f, p => p.NumCasts > 0, "Tankbusters")
            .DeactivateOnExit<PrincelyBlow>()
            .DeactivateOnExit<PrincelyBlowKnockback>();
    }
}
