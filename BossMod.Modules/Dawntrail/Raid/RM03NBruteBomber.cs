namespace BossMod.Dawntrail.Raid.RM03NBruteBomber;

public enum OID : uint
{
    Boss = 0x42C2, // R5.016, x1
    Helper = 0x233C, // R0.500, x35, Helper type
    LitFuse = 0x42C3, // R1.200, x8
    Refbot = 0x42C4, // R3.360, x1
    LariatHelper = 0x42C5, // R1.000, x4
}

public enum AID : uint
{
    AutoAttack = 39553, // Boss->player, no cast, single-target
    Teleport = 37803, // Boss->location, no cast, single-target

    BrutalImpact = 37846, // Boss->self, 5.0s cast, range 60 circle, raidwide first hit
    BrutalImpactRest = 37847, // Boss->self, no cast, range 60 circle, raidwide extra hits
    KnuckleSandwich = 37845, // Boss->players, 5.0s cast, range 6 circle, shared tankbuster
    MurderousMist = 37813, // Boss->self, 5.0s cast, range 40 270-degree cone
    DopingDraught = 37822, // Boss->self, 4.0s cast, single-target, buff
    BrutalBurn = 37928, // Boss->self, 4.5+0.5s cast, single-target, visual (stack)
    BrutalBurnAOE = 37929, // Helper->players, 5.0s cast, range 6 circle stack

    BrutalLariat1 = 39636, // Boss->location, 4.9+1.2s cast, single-target, visual
    BrutalLariat2 = 39637, // Boss->location, 4.9+1.2s cast, single-target, visual
    BrutalLariatHelper1 = 39670, // LariatHelper->self, 4.9s cast, single-target, visual
    BrutalLariatHelper2 = 39808, // LariatHelper->self, 4.9s cast, single-target, visual
    BrutalLariatHelper3 = 39671, // LariatHelper->self, no cast, single-target, visual
    BrutalLariatAOE1 = 39638, // Helper->self, 6.1s cast, range 50 width 34 rect
    BrutalLariatAOE2 = 39639, // Helper->self, 6.1s cast, range 50 width 34 rect
    LariatComboWestEast = 39644, // Boss->location, 4.9+1.2s cast, single-target, visual
    LariatComboWestWest = 39645, // Boss->location, 4.9+1.2s cast, single-target, visual
    LariatComboEastWest = 39646, // Boss->location, 4.9+1.2s cast, single-target, visual
    LariatComboEastEast = 39647, // Boss->location, 4.9+1.2s cast, single-target, visual
    LariatComboReturn1 = 39648, // Boss->location, 3.1s cast, single-target, visual
    LariatComboReturn2 = 39649, // Boss->location, 3.1s cast, single-target, visual
    LariatComboReturn3 = 39650, // Boss->location, 3.1s cast, single-target, visual
    LariatComboReturn4 = 39651, // Boss->location, 3.1s cast, single-target, visual
    LariatComboFirst1 = 39652, // Helper->self, 6.1s cast, range 70 width 34 rect
    LariatComboFirst2 = 39653, // Helper->self, 6.1s cast, range 70 width 34 rect
    LariatComboSecond1 = 39654, // Helper->self, 3.1s cast, range 50 width 34 rect
    LariatComboSecond2 = 39655, // Helper->self, 3.1s cast, range 50 width 34 rect

    BarbarousBarrage = 37810, // Boss->self, 4.0s cast, single-target, visual (towers)
    Explosion = 37811, // Helper->self, no cast, range 4 circle, tower, knockback 22
    UnmitigatedExplosion = 37812, // Helper->self, no cast, range 60 circle, tower fail

    FireSpinCW = 37840, // Boss->self, 4.5+0.5s cast, single-target, visual
    FireSpinCCW = 37841, // Boss->self, 4.5+0.5s cast, single-target, visual
    FireSpinVisual = 37842, // Boss->self, no cast, single-target
    FireSpinFirst = 39768, // Helper->self, 5.0s cast, range 40 60-degree cone
    FireSpinRest = 39769, // Helper->self, 0.5s cast, range 40 60-degree cone
    InfernalSpinCW = 39746, // Boss->self, 4.5+0.5s cast, single-target, visual
    InfernalSpinCCW = 39747, // Boss->self, 4.5+0.5s cast, single-target, visual
    InfernalSpinVisual = 39748, // Boss->self, no cast, single-target
    InfernalSpinFirst = 39770, // Helper->self, 5.0s cast, range 40 60-degree cone
    InfernalSpinRest = 39771, // Helper->self, 0.5s cast, range 40 60-degree cone
    ExplosiveRain1 = 37837, // Helper->self, 5.0s cast, range 8 circle
    ExplosiveRain2 = 37838, // Helper->self, 7.0s cast, range 8-16 donut
    ExplosiveRain3 = 37839, // Helper->self, 9.0s cast, range 16-24 donut
    ExplosiveRainPuddle = 38541, // Helper->self, 3.0s cast, range 6 circle

    FusesOfFury = 37814, // Boss->self, 4.0s cast, single-target, visual (fuses)
    SelfDestructShort = 37816, // LitFuse->self, 5.0s cast, range 8 circle
    SelfDestructLong = 37817, // LitFuse->self, 8.0s cast, range 8 circle
}

public enum SID : uint
{
    Trauma = 3796, // Helper->player, extra=0x1
    Doped = 4021, // none->Boss, extra=0x0
    LitFuseShort = 4015, // none->LitFuse, extra=0x2DF
    LitFuseLong = 4016, // none->LitFuse, extra=0x2E0
}

public enum IconID : uint
{
    BrutalBurn = 161, // player
    KnuckleSandwich = 259, // player
    SpinCW = 167, // Boss
    SpinCCW = 168, // Boss
}

class BrutalImpact(BossModule module) : Components.RaidwideMultiHit(module, AID.BrutalImpact, [AID.BrutalImpact, AID.BrutalImpactRest], 0, 1.1f, 6)
{
    public void SetHits(int hits)
    {
        if (Remaining <= 0)
            return;
        Remaining = hits;
        LastHit = NextHit.AddSeconds(1.1f * (hits - 1));
    }
}
class BrutalImpactRest(BossModule module) : Components.CastCounter(module, AID.BrutalImpactRest);
class KnuckleSandwich(BossModule module) : Components.CastSharedTankbuster(module, AID.KnuckleSandwich, 6);
class MurderousMist(BossModule module) : Components.StandardAOEs(module, AID.MurderousMist, new AOEShapeCone(40, 135.Degrees()));
class BrutalBurn(BossModule module) : Components.StackWithCastTargets(module, AID.BrutalBurnAOE, 6, 4);
class BrutalLariat(BossModule module) : Components.GroupedAOEs(module, [AID.BrutalLariatAOE1, AID.BrutalLariatAOE2], new AOEShapeRect(50, 17));
class ExplosiveRainPuddle(BossModule module) : Components.StandardAOEs(module, AID.ExplosiveRainPuddle, new AOEShapeCircle(6));

class LariatCombo(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _first = new(70, 17);
    private static readonly AOEShapeRect _second = new(50, 17);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(1);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.LariatComboWestEast:
            case AID.LariatComboEastEast:
                PredictSecond(spell, 1);
                break;
            case AID.LariatComboWestWest:
            case AID.LariatComboEastWest:
                PredictSecond(spell, -1);
                break;
            case AID.LariatComboFirst1:
            case AID.LariatComboFirst2:
                _aoes.Add(new(_first, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
                _aoes.SortBy(aoe => aoe.Activation);
                break;
            case AID.LariatComboSecond1:
            case AID.LariatComboSecond2:
                if (_aoes.Count > 0)
                    _aoes[^1] = new(_second, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.LariatComboFirst1 or AID.LariatComboFirst2 or AID.LariatComboSecond1 or AID.LariatComboSecond2)
        {
            ++NumCasts;
            if (_aoes.Count > 0)
                _aoes.RemoveAt(0);
        }
    }

    private void PredictSecond(ActorCastInfo spell, int side)
    {
        var from = spell.LocXZ;
        var dir = (from - Module.Center).Normalized();
        var origin = from + 20 * dir + 12 * side * new WDir(1, 0);
        _aoes.Add(new(_second, origin, Angle.FromDirection(-dir), Module.CastFinishAt(spell, 5.6f)));
        _aoes.SortBy(aoe => aoe.Activation);
    }
}

class BarbarousBarrageTowers(BossModule module) : Components.GenericTowers(module, AID.Explosion)
{
    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x01 && state == 0x00020004)
            foreach (var off in new WDir[] { new(6, -6), new(-6, 6), new(6, 6), new(-6, -6) })
                Towers.Add(new(Module.Center + off, 4, 1, 1, default, WorldState.FutureTime(10.1f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            Towers.Clear();
        }
    }
}

class BarbarousBarrageKnockback(BossModule module) : Components.Knockback(module, AID.Explosion)
{
    private readonly BarbarousBarrageTowers? _towers = module.FindComponent<BarbarousBarrageTowers>();

    private static readonly AOEShapeCircle _shape = new(4);

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_towers != null)
            foreach (var t in _towers.Towers)
                yield return new(t.Position, 22, t.Activation, _shape);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_towers == null)
            return;
        foreach (var t in _towers.Towers)
        {
            if (t.IsInside(actor) && !IsImmune(slot, t.Activation) && t.Activation < WorldState.FutureTime(5))
            {
                hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.ArmsLength), actor, ActionQueue.Priority.High);
                hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Surecast), actor, ActionQueue.Priority.High);
                return;
            }
        }
    }
}

class Spin(BossModule module) : Components.GenericRotatingAOE(module)
{
    private static readonly AOEShapeCone _shape = new(40, 30.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var increment = (AID)spell.Action.ID switch
        {
            AID.FireSpinCW or AID.InfernalSpinCW => -45.Degrees(),
            AID.FireSpinCCW or AID.InfernalSpinCCW => 45.Degrees(),
            _ => default
        };
        if (increment != default)
            Sequences.Add(new(_shape, caster.Position, spell.Rotation, increment, Module.CastFinishAt(spell, 0.5f), 1, 8));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count > 0 && (AID)spell.Action.ID is AID.FireSpinFirst or AID.FireSpinRest or AID.InfernalSpinFirst or AID.InfernalSpinRest)
            AdvanceSequence(0, WorldState.CurrentTime);
    }
}

class ExplosiveRain(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(8), new AOEShapeDonut(8, 16), new AOEShapeDonut(16, 24)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ExplosiveRain1)
            AddSequence(caster.Position, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var order = (AID)spell.Action.ID switch
        {
            AID.ExplosiveRain1 => 0,
            AID.ExplosiveRain2 => 1,
            AID.ExplosiveRain3 => 2,
            _ => -1
        };
        if (order >= 0 && !AdvanceSequence(order, caster.Position, WorldState.FutureTime(2)))
            ReportError($"Unexpected ring {order}");
    }
}

class LitFuse(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCircle _shape = new(8);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count == 0)
            yield break;
        var deadline = _aoes[0].Activation.AddSeconds(1);
        foreach (var aoe in _aoes)
            yield return aoe.Activation < deadline ? aoe with { Color = ArenaColor.Danger } : aoe with { Risky = false };
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.SelfDestructShort or AID.SelfDestructLong)
        {
            _aoes.Add(new(_shape, caster.Position, default, Module.CastFinishAt(spell)));
            _aoes.SortBy(aoe => aoe.Activation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.SelfDestructShort or AID.SelfDestructLong)
        {
            ++NumCasts;
            _aoes.RemoveAll(aoe => aoe.Origin.AlmostEqual(caster.Position, 1));
        }
    }
}

class RM03NBruteBomberStates : StateMachineBuilder
{
    public RM03NBruteBomberStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase)
            .ActivateOnEnter<ExplosiveRainPuddle>();
    }

    private void SinglePhase(uint id)
    {
        BrutalImpact(id, 12.2f, 3);
        KnuckleSandwich(id + 0x10000, 2.3f);
        BrutalLariat(id + 0x20000, 10.2f);
        BrutalLariat(id + 0x30000, 8.7f);
        MurderousMist(id + 0x40000, 4.3f);
        BarbarousBarrage(id + 0x50000, 6.2f);
        FireSpin(id + 0x60000, 2.4f);
        FusesOfFury(id + 0x70000, 4.1f);
        BrutalLariat(id + 0x80000, 4.0f);
        FusesOfFury(id + 0x90000, 5.7f);
        DopingDraught(id + 0xA0000, 9.0f);
        LariatCombo(id + 0xB0000, 33.0f);
        BrutalBurn(id + 0xC0000, 4.6f);
        BrutalImpact(id + 0xD0000, 1.4f, 5);
        LariatCombo(id + 0xE0000, 4.0f);
        InfernalSpin(id + 0xF0000, 12.4f);
        LariatCombo(id + 0x100000, 0.7f);
        // TODO: timeline past this lariat combo + enrage unseen
        BrutalBurn(id + 0x110000, 4.6f);
        BrutalImpact(id + 0x120000, 1.6f, 5);

        SimpleState(id + 0x130000, 120, "Until enrage")
            .ActivateOnEnter<BrutalImpact>()
            .ActivateOnEnter<KnuckleSandwich>()
            .ActivateOnEnter<MurderousMist>()
            .ActivateOnEnter<BrutalBurn>()
            .ActivateOnEnter<BrutalLariat>()
            .ActivateOnEnter<LariatCombo>()
            .ActivateOnEnter<BarbarousBarrageTowers>()
            .ActivateOnEnter<BarbarousBarrageKnockback>()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<ExplosiveRain>()
            .ActivateOnEnter<LitFuse>();
    }

    private void BrutalImpact(uint id, float delay, int extraHits)
    {
        Cast(id, AID.BrutalImpact, delay, 5, "Raidwide first hit")
            .ActivateOnEnter<BrutalImpact>()
            .ActivateOnEnter<BrutalImpactRest>()
            .ExecOnEnter<BrutalImpact>(c => c.SetHits(extraHits + 1))
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<BrutalImpactRest>(id + 0x10, 1.1f * extraHits, c => c.NumCasts >= extraHits, "Raidwide last hit")
            .DeactivateOnExit<BrutalImpact>()
            .DeactivateOnExit<BrutalImpactRest>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void KnuckleSandwich(uint id, float delay)
    {
        Cast(id, AID.KnuckleSandwich, delay, 5, "Shared tankbuster")
            .ActivateOnEnter<KnuckleSandwich>()
            .DeactivateOnExit<KnuckleSandwich>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void BrutalLariat(uint id, float delay)
    {
        CastMulti(id, [AID.BrutalLariat1, AID.BrutalLariat2], delay, 4.9f)
            .ActivateOnEnter<BrutalLariat>();
        ComponentCondition<BrutalLariat>(id + 0x10, 1.2f, c => c.NumCasts > 0, "Lariat")
            .DeactivateOnExit<BrutalLariat>();
    }

    private void LariatCombo(uint id, float delay)
    {
        CastMulti(id, [AID.LariatComboWestEast, AID.LariatComboWestWest, AID.LariatComboEastWest, AID.LariatComboEastEast], delay, 4.9f)
            .ActivateOnEnter<LariatCombo>();
        ComponentCondition<LariatCombo>(id + 0x10, 1.2f, c => c.NumCasts >= 1, "Lariat 1");
        ComponentCondition<LariatCombo>(id + 0x20, 4.4f, c => c.NumCasts >= 2, "Lariat 2")
            .DeactivateOnExit<LariatCombo>();
    }

    private void MurderousMist(uint id, float delay)
    {
        Cast(id, AID.MurderousMist, delay, 5, "Cone")
            .ActivateOnEnter<MurderousMist>()
            .DeactivateOnExit<MurderousMist>();
    }

    private void BarbarousBarrage(uint id, float delay)
    {
        Cast(id, AID.BarbarousBarrage, delay, 4)
            .ActivateOnEnter<BarbarousBarrageTowers>()
            .ActivateOnEnter<BarbarousBarrageKnockback>();
        ComponentCondition<BarbarousBarrageTowers>(id + 0x10, 11.1f, c => c.NumCasts > 0, "Towers")
            .DeactivateOnExit<BarbarousBarrageTowers>()
            .DeactivateOnExit<BarbarousBarrageKnockback>();
    }

    private void FireSpin(uint id, float delay)
    {
        CastMulti(id, [AID.FireSpinCW, AID.FireSpinCCW], delay, 4.5f)
            .ActivateOnEnter<Spin>();
        ComponentCondition<Spin>(id + 0x10, 0.5f, c => c.NumCasts > 0, "Spin start");
        ComponentCondition<Spin>(id + 0x20, 7, c => c.Sequences.Count == 0, "Spin end")
            .DeactivateOnExit<Spin>();
    }

    private void FusesOfFury(uint id, float delay)
    {
        Cast(id, AID.FusesOfFury, delay, 4)
            .ActivateOnEnter<LitFuse>();
        ComponentCondition<LitFuse>(id + 0x10, 7.4f, c => c.NumCasts >= 4, "Fuses 1");
        ComponentCondition<LitFuse>(id + 0x20, 3, c => c.NumCasts >= 8, "Fuses 2")
            .DeactivateOnExit<LitFuse>();
    }

    private void DopingDraught(uint id, float delay)
    {
        Cast(id, AID.DopingDraught, delay, 4, "Buff");
    }

    private void BrutalBurn(uint id, float delay)
    {
        Cast(id, AID.BrutalBurn, delay, 4.5f)
            .ActivateOnEnter<BrutalBurn>();
        ComponentCondition<BrutalBurn>(id + 0x10, 0.8f, c => c.NumFinishedStacks > 0, "Stack")
            .DeactivateOnExit<BrutalBurn>();
    }

    private void InfernalSpin(uint id, float delay)
    {
        CastMulti(id, [AID.InfernalSpinCW, AID.InfernalSpinCCW], delay, 4.5f)
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<ExplosiveRain>();
        ComponentCondition<Spin>(id + 0x10, 0.5f, c => c.NumCasts > 0, "Spin start");
        ComponentCondition<Spin>(id + 0x20, 7, c => c.Sequences.Count == 0, "Spin end")
            .DeactivateOnExit<Spin>();
        ComponentCondition<ExplosiveRain>(id + 0x30, 3.8f, c => c.NumCasts >= 6, "Rings end")
            .DeactivateOnExit<ExplosiveRain>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 989, NameID = 13356)]
public class RM03NBruteBomber(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsSquare(15));
