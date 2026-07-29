namespace BossMod.Dawntrail.Savage.M12SLindwurm;

sealed class FourthWallFusion1(BossModule module) : Components.StackWithIcon(module, (uint)IconID.FourthWallFusion1_Icon, (uint)AID.FourthWallFusion1, 6f, 5d, 4, 4);
sealed class FourthWallFusion2(BossModule module) : Components.StackWithIcon(module, (uint)IconID.FourthWallFusion2_Icon, (uint)AID.FourthWallFusion2, 6f, 5d, 6, 6);
sealed class VisceralBurst(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.VisceralBurst_Icon, (uint)AID.VisceralBurst, 6f, 5d);
sealed class DramaticLysis(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.DramaticLysis_Icon, (uint)AID.DramaticLysis4, 6f, 5d);
sealed class Slaughtershed(BossModule module) : Components.RaidwideCast(module, (uint)AID.Slaughtershed2);
sealed class SerpentineScourge(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect Shape = new(30f, 10f);

    private static readonly WPos West = new(90f, 85f);
    private static readonly WPos East = new(110f, 85f);

    private const float FirstDelay = 15f;
    private const float SecondDelay = 17f;

    private const float Preview = 8f; // how early to show
    private string? _hintText;

    private readonly List<AOEInstance> _aoes = [with(4)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count == 0)
            return [];

        var now = WorldState.CurrentTime;

        var span = CollectionsMarshal.AsSpan(_aoes);

        var bestIndex = -1;
        var bestTime = double.MaxValue;

        for (var i = 0; i < span.Length; ++i)
        {
            var dt = (span[i].Activation - now).TotalSeconds;

            // ignore far-future AOEs
            if (dt > Preview)
                continue;

            if (dt < bestTime)
            {
                bestTime = dt;
                bestIndex = i;
            }
        }

        return bestIndex >= 0 ? span.Slice(bestIndex, 1) : [];
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        WPos first, second;

        switch (spell.Action.ID)
        {
            case (uint)AID.SerpentineScourge_WestEast:
                first = West;
                second = East;
                _hintText = "Next: Cleave West -> East";
                break;

            case (uint)AID.SerpentineScourge_EastWest:
                first = East;
                second = West;
                _hintText = "Next: Cleave East -> West";
                break;

            case (uint)AID.SerpentineScourge2:
                if (_aoes.Count > 0)
                    _aoes.RemoveAt(0);

                _hintText = null;
                return;

            default:
                return;
        }

        var now = WorldState.CurrentTime;

        _aoes.Add(new(Shape, first, default, now.AddSeconds(FirstDelay)));
        _aoes.Add(new(Shape, second, default, now.AddSeconds(SecondDelay)));
    }
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_hintText != null)
            hints.Add(_hintText);
    }
}

sealed class RaptorKnuckles(BossModule module) : Components.GenericKnockback(module)
{
    private static readonly WPos West = new(82f, 89f);
    private static readonly WPos East = new(118f, 89f);
    private static readonly WPos SafeWest = new(84f, 89f);
    private static readonly WPos SafeEast = new(116f, 89f);

    private const float SafeRadius = 2f;

    private const float Distance = 30f;
    private const float FirstDelay = 15f;
    private const float SecondDelay = 17f;
    private const float Preview = 8f; // how early to show
    private string? _hintText;

    private readonly List<Knockback> _kbs = [with(4)];

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (_kbs.Count == 0)
            return [];

        var now = WorldState.CurrentTime;
        var span = CollectionsMarshal.AsSpan(_kbs);

        var bestIndex = -1;
        var bestTime = double.MaxValue;

        for (var i = 0; i < span.Length; ++i)
        {
            var dt = (span[i].Activation - now).TotalSeconds;

            if (dt > Preview)
                continue;

            if (dt < bestTime)
            {
                bestTime = dt;
                bestIndex = i;
            }
        }

        return bestIndex >= 0 ? span.Slice(bestIndex, 1) : [];
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        WPos first, second;

        switch (spell.Action.ID)
        {
            // Ordering events (boss)
            case (uint)AID.RaptorKnuckles_WestEast:
                first = West;
                second = East;
                _hintText = "Next: Knockback West -> East";
                break;

            case (uint)AID.RaptorKnuckles_EastWest:
                first = East;
                second = West;
                _hintText = "Next: Knockback East -> West";
                break;

            // Actual knockback execution (helper)
            case (uint)AID.RaptorKnuckles2:
                if (_kbs.Count > 0)
                    _kbs.RemoveAt(0);

                ++NumCasts;
                _hintText = null;
                return;

            default:
                return;
        }

        var now = WorldState.CurrentTime;

        _kbs.Add(new(first, Distance, now.AddSeconds(FirstDelay)));
        _kbs.Add(new(second, Distance, now.AddSeconds(SecondDelay)));
    }
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_hintText != null)
            hints.Add(_hintText);
    }
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        // we want to add hints but not override the knockback line drawings
        base.DrawArenaForeground(pcSlot, pc);

        if (_kbs.Count == 0)
            return;

        var now = WorldState.CurrentTime;
        var span = CollectionsMarshal.AsSpan(_kbs);

        var dt = (span[0].Activation - now).TotalSeconds;

        if (dt > Preview)
            return;

        ref var kb = ref span[0];

        // Determine side by origin X
        var safe = kb.Origin.X < Module.Center.X ? SafeWest : SafeEast;

        Arena.ZoneCircleOutline(safe, SafeRadius, Colors.Safe);
    }
}
