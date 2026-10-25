using SavageRiot = BossMod.Dawntrail.Savage.RM06SSugarRiot.RM06SSugarRiot;

namespace BossMod.Dawntrail.Raid.RM06NSugarRiot;

public enum OID : uint
{
    Boss = 0x4799, // R4.000, x1
    Helper = 0x233C, // R0.500, x35, Helper type
    PaintBomb = 0x479A, // R0.800, x8
    HeavenBomb = 0x479B, // R0.800, x8
    ThrowUpTarget = 0x479C, // R1.500, x2
    SweetShot = 0x479D, // R1.500, x24
    TempestPiece = 0x479E, // R0.800, x0 (spawn during fight)
    Painting = 0x47B4, // R1.000, x1
}

public enum AID : uint
{
    AutoAttack = 42931, // Boss->player, no cast, single-target
    Teleport = 42611, // Boss->location, no cast, single-target

    MousseMural = 42607, // Boss->self, 5.0s cast, range 100 circle, raidwide
    SingleStyle1 = 42580, // Boss->self, 7.0+0.9s cast, single-target, visual (adds)
    SingleStyle2 = 42582, // Boss->self, 7.0+0.9s cast, single-target, visual (adds)
    SingleStyle3 = 42584, // Boss->self, 9.0+0.9s cast, single-target, visual (adds)
    SingleStyle4 = 42586, // Boss->self, 5.0+0.9s cast, single-target, visual (adds)
    DoubleStyle1 = 42591, // Boss->self, 8.0+0.9s cast, single-target, visual (adds)
    DoubleStyle2 = 42592, // Boss->self, 8.0+0.9s cast, single-target, visual (adds)
    DoubleStyle3 = 42593, // Boss->self, 8.0+0.9s cast, single-target, visual (adds)
    DoubleStyle4 = 42594, // Boss->self, 8.0+0.9s cast, single-target, visual (adds)
    BurstPaint = 42581, // PaintBomb->self, 1.5s cast, range 15 circle
    BurstHeaven = 42583, // HeavenBomb->location, 1.5s cast, range 15 circle
    Rush = 42585, // SweetShot->location, 1.5s cast, width 7 rect charge
    PuddingParty = 42605, // Boss->self, 4.0+1.0s cast, single-target, visual (multi-hit stack)
    PuddingPartyAOE = 42606, // Helper->players, no cast, range 6 circle
    ColorRiot = 42608, // Boss->self, 3.0+2.0s cast, single-target, visual (tankbusters)
    WarmBomb = 42609, // Helper->player, 5.0s cast, range 4 circle
    CoolBomb = 42610, // Helper->player, 5.0s cast, range 4 circle

    Sugarscape1 = 42600, // Boss->self, 1.0+7.0s cast, single-target, visual (desert)
    Sugarscape2 = 42595, // Boss->self, 1.0+7.0s cast, single-target, visual (river)
    Layer1 = 42601, // Boss->self, 1.0+6.0s cast, single-target, visual
    Layer2 = 42602, // Boss->self, 1.0+6.0s cast, single-target, visual
    Layer3 = 42604, // Boss->self, 1.0+6.0s cast, single-target, visual
    Layer4 = 42596, // Boss->self, 1.0+6.0s cast, single-target, visual
    Layer5 = 42598, // Boss->self, 1.0+6.0s cast, single-target, visual
    SprayPain = 42603, // Helper->self, 7.0s cast, range 10 circle
    MousseTouchUp = 42612, // Boss->self, 3.0s cast, single-target, visual (spreads)
    MousseTouchUpAOE = 42613, // Helper->player, 5.0s cast, range 6 circle spread
    TasteOfThunder = 42589, // Boss->self, 8.0+1.0s cast, single-target, visual (spreads outside river)
    TasteOfThunderAOE = 42590, // Helper->player, 6.0s cast, range 6 circle spread
    TasteOfFire = 42587, // Boss->self, 8.0+1.0s cast, single-target, visual (stacks inside river)
    TasteOfFireAOE = 42588, // Helper->players, 6.0s cast, range 6 circle stack
    LightningBolt = 42597, // Helper->location, 3.0s cast, range 4 circle
    Highlightning = 42599, // TempestPiece->self, 1.5s cast, range 21 circle
}

public enum IconID : uint
{
    PuddingParty = 305, // player
    ColorRiot = 342, // player
    MousseTouchUp = 139, // player
    TasteOfFire = 588, // player
    TasteOfThunder = 589, // player
}

public enum TetherID : uint
{
    SingleStyle = 324, // PaintBomb/HeavenBomb/SweetShot/ThrowUpTarget->Boss
    DoubleStyle1 = 319, // HeavenBomb/PaintBomb/ThrowUpTarget->Boss
    DoubleStyle2 = 320, // PaintBomb/SweetShot->Boss
    Painting = 337, // Painting->Boss
}

class MousseMural(BossModule module) : Components.RaidwideCast(module, AID.MousseMural);
class WarmBomb(BossModule module) : Components.BaitAwayCast(module, AID.WarmBomb, new AOEShapeCircle(4), true);
class CoolBomb(BossModule module) : Components.BaitAwayCast(module, AID.CoolBomb, new AOEShapeCircle(4), true);
class SprayPain(BossModule module) : Components.StandardAOEs(module, AID.SprayPain, new AOEShapeCircle(10), 10, highlightImminent: true);
class MousseTouchUp(BossModule module) : Components.SpreadFromCastTargets(module, AID.MousseTouchUpAOE, 6);
class TasteOfThunder(BossModule module) : Components.SpreadFromCastTargets(module, AID.TasteOfThunderAOE, 6);
class TasteOfFire(BossModule module) : Components.StackWithCastTargets(module, AID.TasteOfFireAOE, 6, 4, 4);
class LightningBolt(BossModule module) : Components.StandardAOEs(module, AID.LightningBolt, new AOEShapeCircle(4));

class PuddingParty(BossModule module) : Components.StackWithIcon(module, (uint)IconID.PuddingParty, AID.PuddingPartyAOE, 6, 5.1f, 4, 8, 5)
{
    public int NumCasts;
    public int MaxHits = 5;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action != StackAction)
            return;
        if (++NumCasts >= MaxHits)
            Stacks.Clear();
        else
            for (var i = 0; i < Stacks.Count; ++i)
                Stacks[i] = Stacks[i] with { Activation = WorldState.FutureTime(1.1f), Hits = MaxHits - NumCasts };
    }
}

class PuddingParty6 : PuddingParty
{
    public PuddingParty6(BossModule module) : base(module)
    {
        MaxHits = 6;
        StackHits = 6;
    }
}

class SingleDoubleStyle(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private bool _target;

    private static readonly AOEShapeCircle _circle = new(15);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        switch ((TetherID)tether.ID)
        {
            case TetherID.SingleStyle:
                switch ((OID)source.OID)
                {
                    case OID.PaintBomb:
                        AddCircle(source.Position, 8.7f);
                        break;
                    case OID.HeavenBomb:
                        AddCircle(source.Position + 16 * source.Rotation.ToDirection(), 8.7f);
                        break;
                    case OID.SweetShot:
                        AddRush(source, _target ? 11.7f : 10.7f);
                        break;
                    case OID.ThrowUpTarget:
                        _target = true;
                        break;
                }
                break;
            case TetherID.DoubleStyle1:
                switch ((OID)source.OID)
                {
                    case OID.PaintBomb:
                        AddCircle(source.Position, 9.6f);
                        break;
                    case OID.HeavenBomb:
                        AddCircle(source.Position + 16 * source.Rotation.ToDirection(), 9.6f);
                        break;
                    case OID.ThrowUpTarget:
                        _target = true;
                        for (var i = 0; i < _aoes.Count; ++i)
                            if (_aoes[i].Shape is AOEShapeRect)
                                _aoes[i] = RushTowardsCenter(_aoes[i].Origin, _aoes[i].Activation);
                        break;
                }
                break;
            case TetherID.DoubleStyle2:
                switch ((OID)source.OID)
                {
                    case OID.SweetShot:
                        AddRush(source, _target ? 10.7f : 9.6f);
                        break;
                    case OID.PaintBomb:
                        AddCircle(source.Position, 9.6f);
                        break;
                }
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID is AID.BurstPaint or AID.BurstHeaven or AID.Rush)
        {
            ++NumCasts;
            _aoes.Clear();
            _target = false;
        }
    }

    private void AddCircle(WPos position, float delay) => _aoes.Add(new(_circle, position, default, WorldState.FutureTime(delay)));

    private void AddRush(Actor source, float delay)
    {
        if (_target)
        {
            _aoes.Add(RushTowardsCenter(source.Position, WorldState.FutureTime(delay)));
        }
        else
        {
            _aoes.Add(new(new AOEShapeRect(60, 3.5f), source.Position, source.Rotation, WorldState.FutureTime(delay)));
        }
    }

    private AOEInstance RushTowardsCenter(WPos origin, DateTime activation)
    {
        var dir = Module.Center - origin;
        return new(new AOEShapeRect(dir.Length(), 3.5f), origin, Angle.FromDirection(dir), activation);
    }
}

class Quicksand(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    private static readonly AOEShapeCircle _shape = new(23);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is < 0x1F or > 0x23)
            return;
        if (state == 0x00020001)
        {
            var pos = index switch
            {
                0x20 => new WPos(100, 80),
                0x21 => new WPos(100, 120),
                0x22 => new WPos(120, 100),
                0x23 => new WPos(80, 100),
                _ => Module.Center
            };
            _aoe = new(_shape, pos, default, WorldState.FutureTime(6));
        }
        else if (state == 0x00080004)
        {
            _aoe = null;
        }
    }
}

class River(BossModule module) : BossComponent(module)
{
    private bool _riverSafe;
    private DateTime _activation;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0x04)
            return;
        switch (state)
        {
            case 0x00200010:
                Arena.Bounds = SavageRiot.BoundsMinusRiver;
                break;
            case 0x08000004:
                Arena.Bounds = new ArenaBoundsSquare(20);
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.TasteOfThunder:
                _riverSafe = false;
                _activation = Module.CastFinishAt(spell, 1.1f);
                break;
            case AID.TasteOfFire:
                _riverSafe = true;
                _activation = Module.CastFinishAt(spell, 1.2f);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.TasteOfThunderAOE or AID.TasteOfFireAOE)
            _activation = default;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_activation == default)
            return;
        var inRiver = SavageRiot.RiverPoly.Contains(actor.Position - Arena.Center);
        if (_riverSafe)
            hints.Add("Stand in water!", !inRiver);
        else
            hints.Add("Avoid water!", inRiver);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_activation == default)
            return;
        var sdf = new AOEShapeCustom(SavageRiot.RiverPoly).GetSdf(Arena.Center, default);
        hints.AddForbiddenZone(_riverSafe ? sdf.Inverted() : sdf, _activation);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_activation != default)
            Arena.ZoneComplex(Arena.Center, default, SavageRiot.RiverPoly, _riverSafe ? ArenaColor.SafeFromAOE : ArenaColor.AOE);
    }
}

// TODO: storm cloud never seen in a replay
class TempestPiece(BossModule module) : Components.GenericAOEs(module, AID.Highlightning)
{
    private Actor? _cloud;
    private WPos? _next;
    private DateTime _activation;

    private static readonly AOEShapeCircle _shape = new(21);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_next).Select(p => new AOEInstance(_shape, p, default, _activation));

    public override void Update()
    {
        if (_cloud == null || _next != null)
            return;
        var move = Angle.FromDirection(_cloud.LastFrameMovement);
        if (move == default)
            return;
        if (move.AlmostEqual(120.Degrees(), 60.Degrees().Rad))
            _next = new(115, 92);
        else if (move.AlmostEqual(-120.Degrees(), 60.Degrees().Rad))
            _next = new(87, 92);
        else if (move.AlmostEqual(default, 60.Degrees().Rad))
            _next = new(100, 115);
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.TempestPiece)
        {
            _cloud = actor;
            _next = actor.Position;
            _activation = WorldState.FutureTime(6.9f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            _next = null;
            _activation = WorldState.FutureTime(10.5f);
        }
    }
}

class RM06NSugarRiotStates : StateMachineBuilder
{
    public RM06NSugarRiotStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase)
            .ActivateOnEnter<SingleDoubleStyle>()
            .ActivateOnEnter<SprayPain>()
            .ActivateOnEnter<Quicksand>()
            .ActivateOnEnter<River>()
            .ActivateOnEnter<LightningBolt>()
            .ActivateOnEnter<TempestPiece>();
    }

    private void SinglePhase(uint id)
    {
        MousseMural(id, 6.1f);
        Style(id + 0x10000, [AID.SingleStyle1], 12.6f, 7, 1, 4.6f);
        Style(id + 0x20000, [AID.SingleStyle2], 5.3f, 7, 2, 4.7f);
        Style(id + 0x30000, [AID.SingleStyle3], 1.5f, 9, 3, 4.7f);
        Cast(id + 0x40000, AID.SingleStyle4, 1.5f, 5);
        Style(id + 0x50000, [AID.SingleStyle3], 5.1f, 9, 4, 5.7f);
        PuddingParty(id + 0x60000, 0.5f, false);
        ColorRiot(id + 0x70000, 2.9f);
        Cast(id + 0x80000, AID.Sugarscape1, 10.4f, 1, "Desert");
        Cast(id + 0x90000, AID.Layer1, 19.3f, 1);
        Cast(id + 0xA0000, AID.Layer2, 16.3f, 1);
        ComponentCondition<SprayPain>(id + 0xA0010, 14.2f, c => c.NumCasts > 0, "Puddles start");
        Cast(id + 0xB0000, AID.Layer3, 20.1f, 1);
        MousseTouchUp(id + 0xC0000, 23.2f);
        MousseMural(id + 0xD0000, 1.2f);
        ColorRiot(id + 0xE0000, 4.2f);
        Style(id + 0xF0000, DoubleStyles, 9.6f, 8, 5, 4.6f);
        Style(id + 0x100000, DoubleStyles, 2.6f, 8, 6, 4.6f);
        PuddingParty(id + 0x110000, 3.6f, true);
        Cast(id + 0x120000, AID.Sugarscape2, 9, 1, "River");
        TasteOfThunderFire(id + 0x130000, 14.2f);
        MousseMural(id + 0x140000, 2);
        Cast(id + 0x150000, AID.Layer4, 7.5f, 1);
        ComponentCondition<LightningBolt>(id + 0x150010, 11.1f, c => c.NumCasts > 0, "Bolts start");
        Cast(id + 0x160000, AID.Layer5, 12.4f, 1);

        SimpleState(id + 0x170000, 120, "Until enrage")
            .ActivateOnEnter<MousseMural>()
            .ActivateOnEnter<PuddingParty>()
            .ActivateOnEnter<WarmBomb>()
            .ActivateOnEnter<CoolBomb>()
            .ActivateOnEnter<MousseTouchUp>()
            .ActivateOnEnter<TasteOfThunder>()
            .ActivateOnEnter<TasteOfFire>();
    }

    private void MousseMural(uint id, float delay)
    {
        Cast(id, AID.MousseMural, delay, 5, "Raidwide")
            .ActivateOnEnter<MousseMural>()
            .DeactivateOnExit<MousseMural>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private static readonly AID[] DoubleStyles = [AID.DoubleStyle1, AID.DoubleStyle2, AID.DoubleStyle3, AID.DoubleStyle4];

    private void Style(uint id, AID[] aids, float delay, float castTime, int count, float resolveDelay)
    {
        CastMulti(id, aids, delay, castTime);
        ComponentCondition<SingleDoubleStyle>(id + 0x10, resolveDelay, c => c.NumCasts >= count, "Adds");
    }

    private void PuddingParty(uint id, float delay, bool six)
    {
        if (six)
            CastStart(id, AID.PuddingParty, delay)
                .ActivateOnEnter<PuddingParty6>();
        else
            CastStart(id, AID.PuddingParty, delay)
                .ActivateOnEnter<PuddingParty>();
        CastEnd(id + 1, 4);
        if (six)
            ComponentCondition<PuddingParty6>(id + 0x10, 6.4f, c => c.NumCasts >= 6, "Multi-hit stack")
                .DeactivateOnExit<PuddingParty6>();
        else
            ComponentCondition<PuddingParty>(id + 0x10, 5.3f, c => c.NumCasts >= 5, "Multi-hit stack")
                .DeactivateOnExit<PuddingParty>();
    }

    private void ColorRiot(uint id, float delay)
    {
        Cast(id, AID.ColorRiot, delay, 3)
            .ActivateOnEnter<WarmBomb>()
            .ActivateOnEnter<CoolBomb>();
        ComponentCondition<WarmBomb>(id + 0x10, 2, c => c.NumCasts > 0, "Tankbusters")
            .DeactivateOnExit<WarmBomb>()
            .DeactivateOnExit<CoolBomb>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void MousseTouchUp(uint id, float delay)
    {
        Cast(id, AID.MousseTouchUp, delay, 3)
            .ActivateOnEnter<MousseTouchUp>();
        ComponentCondition<MousseTouchUp>(id + 0x10, 6, c => c.NumFinishedSpreads > 0, "Spreads")
            .DeactivateOnExit<MousseTouchUp>();
    }

    private void TasteOfThunderFire(uint id, float delay)
    {
        CastMulti(id, [AID.TasteOfThunder, AID.TasteOfFire], delay, 8)
            .ActivateOnEnter<TasteOfThunder>()
            .ActivateOnEnter<TasteOfFire>();
        Condition(id + 0x10, 1.2f, () => TasteResolved() >= 1, "Spreads / stacks");
        CastMulti(id + 0x20, [AID.TasteOfThunder, AID.TasteOfFire], 4, 8);
        Condition(id + 0x30, 1.2f, () => TasteResolved() >= 2, "Stacks / spreads")
            .DeactivateOnExit<TasteOfThunder>()
            .DeactivateOnExit<TasteOfFire>();
    }

    private int TasteResolved() => (Module.FindComponent<TasteOfThunder>()?.NumFinishedSpreads > 0 ? 1 : 0) + (Module.FindComponent<TasteOfFire>()?.NumFinishedStacks > 0 ? 1 : 0);
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1021, NameID = 13822)]
public class RM06NSugarRiot(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsSquare(20));
