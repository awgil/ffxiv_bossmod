using static BossMod.PartyRolesConfig;

namespace BossMod.Dawntrail.Savage.RM10STheXtremes;

enum Color
{
    None,
    Red,
    Blue
}

enum Mechanic
{
    None,
    Stack,
    Spread,
    Tankbuster
}

record struct Source(Color Color, Mechanic Mechanic, WPos Origin);

static class AirHelpers
{
    public static Source? ProcessEffect(byte index, uint state)
    {
        if (index is >= 0x0E and <= 0x16)
        {
            var cm = state switch
            {
                0x00020001 => (Color.Blue, Mechanic.Spread),
                0x00200010 => (Color.Blue, Mechanic.Stack),
                0x00800040 => (Color.Blue, Mechanic.Tankbuster),
                0x02000100 => (Color.Red, Mechanic.Spread),
                0x08000400 => (Color.Red, Mechanic.Stack),
                0x20001000 => (Color.Red, Mechanic.Tankbuster),
                _ => default
            };
            if (cm.Item1 != default)
            {
                var ix = index - 0x0E;
                var col = ix % 3;
                var row = ix / 3;
                var pos = new WPos(87 + col * 13, 87 + row * 13);

                return new(cm.Item1, cm.Item2, pos);
            }
        }

        return null;
    }
}

class Air2Assignments : BossComponent
{
    readonly RM10STheXtremesConfig _config = Service.Config.Get<RM10STheXtremesConfig>();
    readonly PartyRolesConfig _roles = Service.Config.Get<PartyRolesConfig>();
    readonly Dictionary<Assignment, int> _roleOrder = [];
    BitMask _assigned;

    public record struct ColorOrder(int Order, Color Color);
    public readonly ColorOrder[] PlayerOrder = Utils.MakeArray<ColorOrder>(8, new(-1, Color.None));
    public bool HaveOrder { get; private set; }

    public Air2Assignments(BossModule module) : base(module)
    {
        switch (_config.IA2CleanseOrder)
        {
            case RM10STheXtremesConfig.CleanseOrder.HMR:
                _roleOrder.Clear();
                _roleOrder[Assignment.H1] = _roleOrder[Assignment.H2] = 0;
                _roleOrder[Assignment.M1] = _roleOrder[Assignment.M2] = 1;
                _roleOrder[Assignment.R1] = _roleOrder[Assignment.R2] = 2;
                _roleOrder[Assignment.MT] = _roleOrder[Assignment.OT] = 3;
                _roleOrder[Assignment.Unassigned] = -1;
                break;
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.XtremeFiresnaking && Raid.TryFindSlot(actor.InstanceID, out var slot))
        {
            _assigned.Set(slot);
            PlayerOrder[slot] = new(-1, Color.Red);
            Assign();
        }
        if ((SID)status.ID == SID.XtremeWatersnaking && Raid.TryFindSlot(actor.InstanceID, out var slot2))
        {
            _assigned.Set(slot2);
            PlayerOrder[slot2] = new(-1, Color.Blue);
            Assign();
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.XtremeFiresnaking or SID.XtremeWatersnaking && Raid.TryFindSlot(actor, out var slot))
            PlayerOrder[slot] = default;
    }

    void Assign()
    {
        if (_assigned.NumSetBits() < 8)
            return;

        var assignments = _roles.AssignmentsPerSlot(Raid);
        if (assignments.Length == 0)
        {
            ReportError("Party role assignment is invalid, not assigning order");
            return;
        }

        for (var slot = 0; slot < 8; slot++)
            PlayerOrder[slot].Order = _roleOrder.TryGetValue(assignments[slot], out var i) ? i : -1;

        HaveOrder = true;

        if (_roleOrder.Count > 0 && PlayerOrder.Distinct().Count() < 8)
        {
            ReportError($"Duplicate group/order assignment, debuffs might not have been assigned correctly");
            HaveOrder = false;
            for (var slot = 0; slot < 8; slot++)
                PlayerOrder[slot].Order = -1;
        }
    }
}

class AirBaits(BossModule module) : Components.UntelegraphedBait(module)
{
    public record struct Source(WPos Origin, DateTime Activation, Color Color, Mechanic Mechanic, int Order);
    protected readonly List<Source> Sources = [];

    protected readonly List<Source> ActiveSources = []; // same order as CurrentBaits

    public static readonly AOEShapeCone Cone = new(60, 22.5f.Degrees());

    public float ActivationDelayFirst = 8.6f;
    public float ActivationDelayRest = 8.6f;

    private int _blueCounter;
    private int _redCounter;

    public int NumRedCasts { get; private set; }
    public int NumBlueCasts { get; private set; }

    private readonly int[] _seq = new int[3];

    public override void OnMapEffect(byte index, uint state)
    {
        if (AirHelpers.ProcessEffect(index, state) is { } source)
        {
            ref var counter = ref _seq[(int)source.Color];
            int order;
            if (source.Mechanic == Mechanic.Tankbuster)
                order = 3;
            else
                order = counter++;

            var delay = CurrentBaits.Count < 2 ? ActivationDelayFirst : ActivationDelayRest;
            Source s = new(source.Origin, WorldState.FutureTime(delay), source.Color, source.Mechanic, order);
            if (CurrentBaits.Count < 2)
                CurrentBaits.Add(DetermineBait(s));
            Sources.Add(s);
            if (ActiveSources.Count < 2)
                ActiveSources.Add(s);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ReEntryBlastAOE:
            case AID.VerticalBlastAOE:
                NumCasts++;
                NumRedCasts++;
                Advance();
                break;
            case AID.ReEntryPlungeAOE:
            case AID.VerticalPlungeAOE:
                NumCasts++;
                NumBlueCasts++;
                Advance();
                break;
            case AID.PlungingSnapAOE:
                if (++_blueCounter > 3)
                {
                    NumCasts++;
                    NumBlueCasts++;
                    Advance();
                    _blueCounter = 0;
                }
                break;
            case AID.BlastingSnapAOE:
                if (++_redCounter > 3)
                {
                    NumCasts++;
                    NumRedCasts++;
                    Advance();
                    _redCounter = 0;
                }
                break;
        }
    }

    protected virtual void Advance()
    {
        if (NumCasts % 2 == 0)
        {
            CurrentBaits.Clear();
            ActiveSources.Clear();
            ActiveSources.AddRange(Sources.Skip(NumCasts).Take(2));
            CurrentBaits.AddRange(ActiveSources.Select(DetermineBait));
        }
    }

    protected virtual Bait DetermineBait(Source src)
    {
        return src.Mechanic switch
        {
            Mechanic.Spread => new(src.Origin, default, Cone, src.Activation, count: 4, type: AIHints.PredictedDamageType.Raidwide, isProximity: true),
            Mechanic.Stack => new(src.Origin, default, Cone, src.Activation, count: 1, stackSize: 4, type: AIHints.PredictedDamageType.Shared, isProximity: true),
            Mechanic.Tankbuster => new(src.Origin, default, new AOEShapeCircle(6), src.Activation, count: 1, type: AIHints.PredictedDamageType.Tankbuster, forbiddenTargets: Raid.WithSlot().WhereActor(a => a.Role != Role.Tank).Mask(), isProximity: true, centerAtTarget: true),
            _ => throw new InvalidOperationException($"unknown mechanic type {src.Mechanic}"),
        };
    }
}

class AirPuddleCone(BossModule module) : FlamePuddle(module, [AID.BlastingSnapAOE, AID.ReEntryBlastAOE], new AOEShapeCone(60, 22.5f.Degrees()), OID.FlameCone);
class AirPuddleCircle(BossModule module) : FlamePuddle(module, AID.VerticalBlastAOE, new AOEShapeCircle(6), OID.FlamePuddle6, originAtTarget: true);

class Air2Baits : AirBaits
{
    readonly Air2Assignments _assignments;
    BitMask _stackTargets;
    int _numStacks;

    public Air2Baits(BossModule module) : base(module)
    {
        _assignments = module.FindComponent<Air2Assignments>()!;
        ActivationDelayFirst = 9.6f;
        ActivationDelayRest = 11.6f;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        var o = _assignments.PlayerOrder[slot];
        if (o.Color == Color.None)
            return;

        var s = $"Color: {o.Color}";
        if (o.Order >= 0)
        {
            var o2 = o.Order == 3 ? "tank" : (o.Order + 1).ToString();
            s += $", order: {o2}";
        }
        hints.Add(s, false);
    }

    public override void OnMapEffect(byte index, uint state)
    {
        base.OnMapEffect(index, state);

        UpdateStacks();
    }

    public override void AddAIHints(int slot, Actor actor, Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (_assignments.PlayerOrder[slot].Color == Color.None)
            return;

        if (CurrentBaits.FirstOrNull(b => !b.ForbiddenTargets[slot]) is { } b)
            // prevent dash shenanigans from ruining proximity baits
            hints.AddForbiddenZone(ShapeContains.Donut(b.Origin, 10, 100), b.Activation);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        foreach (var (slot, player) in Raid.WithSlot())
            if (_stackTargets[slot])
            {
                if (Arena.Config.ShowOutlinesAndShadows)
                    Arena.AddCircle(player.Position, 15, 0xFF000000, 2);
                Arena.AddCircle(player.Position, 15, ArenaColor.Safe);
            }

        if (!_assignments.HaveOrder)
            return;

        var p = _assignments.PlayerOrder[pcSlot];

        if (p.Color == Color.None)
            return;

        // if mechanic hasn't started yet, highlight first bait position (always cw from corner)
        if (NumCasts == 0 && CurrentBaits.Count == 0)
        {
            WPos baitSource = (p.Order == 0) == (p.Color == Color.Red)
                ? new(100, 87)
                : new(100, 113);

            Arena.AddCircle(baitSource, 1, ArenaColor.Safe);
        }

        foreach (var b in CurrentBaits)
        {
            if (b.ForbiddenTargets[pcSlot])
                continue;

            Arena.AddCircle(b.Origin, 1, ArenaColor.Safe);
        }
    }

    protected override Bait DetermineBait(Source src)
    {
        var bait = base.DetermineBait(src);
        if (!_assignments.HaveOrder)
            return bait;

        BitMask forbiddenPlayers = new();
        for (var i = 0; i < 8; i++)
        {
            var o = _assignments.PlayerOrder[i];
            if (o.Color == Color.None)
                continue;
            var forbiddenTarget = o.Order == src.Order ? o.Color == src.Color : o.Color != src.Color;
            if (forbiddenTarget)
                forbiddenPlayers.Set(i);
        }

        bait.ForbiddenTargets |= forbiddenPlayers;
        return bait;
    }

    void UpdateStacks()
    {
        _stackTargets.Reset();

        foreach (var nextBait in ActiveSources)
        {
            var ix = Array.FindIndex(_assignments.PlayerOrder, p => p.Order == nextBait.Order && p.Color != nextBait.Color);
            _stackTargets.Set(ix);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if ((AID)spell.Action.ID is AID.Bailout2 or AID.Bailout1)
            _stackTargets.Reset();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if ((AID)spell.Action.ID is AID.Bailout2 or AID.Bailout1)
        {
            _numStacks++;
            if (_numStacks % 2 == 0)
            {
                base.Advance();
                UpdateStacks();
                EnableHints = true;
            }
        }
    }

    protected override void Advance()
    {
        CurrentBaits.Clear();
        EnableHints = false;
    }
}

class Bailout(BossModule module) : Components.UniformStackSpread(module, 15, 0)
{
    public int NumCasts { get; private set; }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Bailout2 or AID.Bailout1 && WorldState.Actors.Find(spell.TargetID) is { } tar)
            AddStack(tar, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Bailout2 or AID.Bailout1)
        {
            Stacks.Clear();
            NumCasts++;
        }
    }
}
