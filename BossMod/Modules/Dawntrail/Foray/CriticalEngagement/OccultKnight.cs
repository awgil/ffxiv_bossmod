namespace BossMod.Dawntrail.Foray.CriticalEngagement.OccultKnight;

public enum OID : uint
{
    OccultKnight0 = 0x471E, // R3.000, x9 (spawn during fight)
    OccultKnight1 = 0x471F, // R3.000, x0 (spawn during fight), targetable
    Megaloknight = 0x4720, // R5.000, x0 (spawn during fight)
    OccultKnight2 = 0x4721, // R3.000, x0 (spawn during fight)
    DeathWallHelper = 0x4862, // R0.500, x1
    Helper = 0x233C, // R0.500, x0 (spawn during fight), Helper type
}

public enum AID : uint
{
    DeathWall = 41901, // DeathWallHelper->self, no cast, range 20-30 donut
    AutoAttackAdds = 42560, // OccultKnight0->player, no cast, single-target
    LineOfFire1 = 41817, // OccultKnight0->self, 5.0s cast, range 60 width 8 rect
    LineOfFire2 = 41818, // OccultKnight0->self, 5.0s cast, range 60 width 8 rect
    KnuckleCrusher = 41819, // OccultKnight0->self, 10.0s cast, range 15 circle
    KnuckleDownVisual = 41820, // OccultKnight0->self, 4.0s cast, single-target
    KnuckleDown = 41821, // Helper->self, no cast, ???
    SpinningSiegeCW = 41822, // OccultKnight0->self, 8.0s cast, range 60 width 6 cross
    SpinningSiegeCCW = 41823, // OccultKnight0->self, 8.0s cast, range 60 width 6 cross
    SpinningSiegeRepeat = 41824, // OccultKnight0->self, 0.5s cast, range 60 width 6 cross
    BlastKnucklesCast = 41826, // OccultKnight0->self, 5.0s cast, ???
    BlastKnuckles = 41891, // Helper->self, no cast, ???
    CageOfFire = 41825, // OccultKnight0->self, 7.0s cast, range 60 width 8 rect
    Moatmaker = 41827, // OccultKnight0->location, 3.0s cast, range 9 circle
    DualistFlurryFirst = 41828, // OccultKnight0->location, 10.0s cast, range 6 circle
    DualistFlurryRest = 41829, // OccultKnight0->location, no cast, single-target
    DualistFlurryAOE = 43152, // Helper->self, no cast, range 6 circle
    SpiritSling = 41834, // OccultKnight2->self, 3.5s cast, range 60 width 8 rect
    BarefistedDeath = 41830, // Megaloknight->self, 90.0s cast, range 60 circle
}

class OccultKnights(BossModule module) : Components.AddsMulti(module, [OID.OccultKnight0, OID.OccultKnight1, OID.Megaloknight]);

class LineOfFire(BossModule module) : Components.GroupedAOEs(module, [AID.LineOfFire1, AID.LineOfFire2], new AOEShapeRect(60, 4), highlightImminent: true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (NumCasts < 4 && Casters.Count > 0)
        {
            var invRect = ShapeContains.Rect(Arena.Center, 45.Degrees(), 8, 8, 8);
            hints.AddForbiddenZone(p => !invRect(p), Module.CastFinishAt(Casters[0].CastInfo));
        }
    }
}
class KnuckleCrusher(BossModule module) : Components.StandardAOEs(module, AID.KnuckleCrusher, 15, maxCasts: 4)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (Casters.Count >= 2 && NumCasts == 0)
            hints.AddForbiddenZone(ShapeContains.Donut(Casters[1].Position, 17, 60), Module.CastFinishAt(Casters[0].CastInfo));
    }
}

class SpinningSiege(BossModule module) : Components.GenericRotatingAOE(module)
{
    public static readonly AOEShape Shape = new AOEShapeCross(60, 3);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var increment = (AID)spell.Action.ID switch
        {
            AID.SpinningSiegeCCW => 9.Degrees(),
            AID.SpinningSiegeCW => -9.Degrees(),
            _ => default
        };
        if (increment != default)
            Sequences.Add(new()
            {
                Shape = Shape,
                Origin = caster.Position,
                Rotation = spell.Rotation,
                Increment = increment,
                NextActivation = Module.CastFinishAt(spell),
                SecondsBetweenActivations = 1.7f,
                NumRemainingCasts = 6,
                MaxShownAOEs = 2
            });
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.SpinningSiegeCCW or AID.SpinningSiegeCW or AID.SpinningSiegeRepeat)
            AdvanceSequence(caster.Position, spell.Rotation, WorldState.CurrentTime);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (NumCasts > 0)
            return;

        foreach (var seq in Sequences)
        {
            var dirCenter = (Arena.Center - seq.Origin).Normalized();
            var dirSweep = seq.Increment.Rad > 0 ? dirCenter.OrthoL() : dirCenter.OrthoR();
            hints.AddForbiddenZone(ShapeContains.Cone(seq.Origin, 50, Angle.FromDirection(dirSweep), 90.Degrees()), seq.NextActivation);
        }
    }
}

class CageOfFire : Components.StandardAOEs
{
    public CageOfFire(BossModule module) : base(module, AID.CageOfFire, new AOEShapeRect(60, 4), maxCasts: 5)
    {
        Risky = false;
    }
}
class BlastKnuckles(BossModule module) : Components.KnockbackFromCastTarget(module, AID.BlastKnucklesCast, 15)
{
    public int NumRealCasts { get; private set; }

    private readonly CageOfFire _cage = module.FindComponent<CageOfFire>()!;

    private bool HitByRect(WPos p) => _cage.ActiveCasters.Any(c => p.InRect(c.Position, c.Rotation, 60, 0, 4));

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => !pos.InCircle(Arena.Center, 20) || HitByRect(pos);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.FirstOrDefault() is { } caster)
        {
            var activation = Module.CastFinishAt(caster.CastInfo);
            if (IsImmune(slot, activation))
                return;

            hints.AddForbiddenZone(ShapeContains.Donut(caster.Position, 5, 40), Module.CastFinishAt(caster.CastInfo));
            hints.AddForbiddenZone(p =>
            {
                var dir = (p - Arena.Center).Normalized() * Distance;
                return HitByRect(p + dir);
            }, Module.CastFinishAt(caster.CastInfo));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if ((AID)spell.Action.ID == AID.BlastKnuckles)
            NumRealCasts++;
    }
}

class Moatmaker(BossModule module) : Components.StandardAOEs(module, AID.Moatmaker, 9);
class DualfistFlurry(BossModule module) : Components.Exaflare(module, new AOEShapeCircle(6), AID.DualistFlurryAOE)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DualistFlurryFirst)
            Lines.Add(new()
            {
                Next = spell.LocXZ,
                Advance = caster.Rotation.ToDirection() * 7,
                NextExplosion = Module.CastFinishAt(spell, 0.9f),
                TimeToMove = 2.1f,
                ExplosionsLeft = 6,
                MaxShownExplosions = 3
            });
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            var l = Lines.FindIndex(l => l.Next.AlmostEqual(caster.Position, 0.5f));
            if (l >= 0)
                AdvanceLine(Lines[l], caster.Position);
        }
    }
}

class SpiritSling(BossModule module) : Components.StandardAOEs(module, AID.SpiritSling, new AOEShapeRect(60, 4));

class OccultKnightStates : StateMachineBuilder
{
    private readonly OccultKnight _module;

    public OccultKnightStates(OccultKnight module) : base(module)
    {
        _module = module;
        SimplePhase(0, Phase1, "P1")
            .Raw.Update = () => _module.Megaloknight()?.IsDeadOrDestroyed == true || _module.Helper?.IsDeadOrDestroyed == true;
    }

    private void Phase1(uint id)
    {
        Condition(id, 300, () => Module.Enemies(OID.OccultKnight0).All(k => k.IsDeadOrDestroyed || !k.IsTargetable), "Adds 1 enrage")
            .ActivateOnEnter<OccultKnights>();

        LineOfFire(id + 0x10000, 10.2f);
        KnuckleCrusher(id + 0x20000, 11);

        Condition(id + 0x30000, 300, () => Module.Enemies(OID.OccultKnight0).All(k => k.IsDeadOrDestroyed), "Adds 2 enrage");

        SpinningSiege(id + 0x40000, 13.2f);
        BlastKnuckles(id + 0x50000, 7.3f);

        Condition(id + 0x60000, 300, () => Module.Enemies(OID.OccultKnight0).All(k => k.IsDeadOrDestroyed), "Adds 3 enrage");

        DualfistFlurry(id + 0x70000, 16.1f);

        Condition(id + 0x80000, 300, () => Module.Enemies(OID.OccultKnight0).All(k => k.IsDeadOrDestroyed), "Adds 4 enrage");

        Condition(id + 0x90000, 7.1f, () => Module.Enemies(OID.OccultKnight1).Any(k => k.IsTargetable), "Megaknight appears")
            .ActivateOnEnter<SpiritSling>();

        ActorCastStart(id + 0x90100, _module.Megaloknight, AID.BarefistedDeath, 50);
        ActorCastEnd(id + 0x90200, _module.Megaloknight, 90, name: "Enrage");
    }

    private void LineOfFire(uint id, float delay)
    {
        ComponentCondition<LineOfFire>(id + 0x10, delay, l => l.NumCasts > 0, "Lines 1")
            .ActivateOnEnter<LineOfFire>();
        ComponentCondition<LineOfFire>(id + 0x11, 2.1f, l => l.NumCasts > 2, "Lines 2");
        ComponentCondition<LineOfFire>(id + 0x12, 2.1f, l => l.NumCasts > 4, "Lines 3");
        ComponentCondition<LineOfFire>(id + 0x13, 2.1f, l => l.NumCasts > 8, "Lines 4")
            .DeactivateOnExit<LineOfFire>();
    }

    private void KnuckleCrusher(uint id, float delay)
    {
        ComponentCondition<KnuckleCrusher>(id, delay, k => k.NumCasts > 0, "Circles start")
            .ActivateOnEnter<KnuckleCrusher>();
        ComponentCondition<KnuckleCrusher>(id + 1, 4, k => k.NumCasts > 4, "Circles end")
            .DeactivateOnExit<KnuckleCrusher>();
    }

    private void SpinningSiege(uint id, float delay)
    {
        ComponentCondition<SpinningSiege>(id, delay, s => s.NumCasts > 0, "Rotating AOEs start")
            .ActivateOnEnter<SpinningSiege>();
        ComponentCondition<SpinningSiege>(id + 0x10, 8.7f, s => s.NumCasts >= 24, "Rotating AOEs end")
            .DeactivateOnExit<SpinningSiege>();
    }

    private void BlastKnuckles(uint id, float delay)
    {
        ComponentCondition<BlastKnuckles>(id, delay, b => b.NumRealCasts > 0, "Knockback")
            .ActivateOnEnter<CageOfFire>()
            .ActivateOnEnter<BlastKnuckles>();
        ComponentCondition<CageOfFire>(id + 0x10, 1, c => c.NumCasts > 0, "Lines 1")
            .ExecOnEnter<CageOfFire>(c => c.Risky = true);
        ComponentCondition<CageOfFire>(id + 0x12, 2.1f, c => c.NumCasts > 5, "Lines 2")
            .DeactivateOnExit<CageOfFire>()
            .DeactivateOnExit<BlastKnuckles>();
    }

    private void DualfistFlurry(uint id, float delay)
    {
        ComponentCondition<DualfistFlurry>(id, delay, d => d.NumCasts > 0, "Exaflares + puddles start")
            .ActivateOnEnter<DualfistFlurry>()
            .ActivateOnEnter<Moatmaker>();
        ComponentCondition<Moatmaker>(id + 0x10, 12.5f, m => m.NumCasts >= 16, "Exaflares + puddles end")
            .DeactivateOnExit<DualfistFlurry>()
            .DeactivateOnExit<Moatmaker>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13728, PrimaryActorOID = (uint)OID.DeathWallHelper)]
public class OccultKnight(WorldState ws, Actor primary) : BossModule(ws, primary, new(680, -280), new ArenaBoundsCircle(20))
{
    public Actor? Megaloknight() => _megaloknight;

    private Actor? _megaloknight;
    public Actor? Helper { get; private set; }

    protected override void UpdateModule()
    {
        _megaloknight ??= Enemies(OID.Megaloknight).FirstOrDefault();
        Helper ??= Enemies(OID.DeathWallHelper).FirstOrDefault();
    }

    public override bool DrawAllPlayers => true;

    protected override bool CheckPull() => PrimaryActor.InCombat;
}
