namespace BossMod.Dawntrail.Savage.RM10STheXtremes;

class BubbleBounds(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
            yield return new(new AOEShapeDonut(20, 100), Arena.Center, default, _activation);
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 1)
        {
            if (state == 0x00200010)
                _activation = WorldState.FutureTime(5.1f);
            if (state == 0x00800040)
            {
                _activation = default;
                Arena.Bounds = new ArenaBoundsCircle(20);
            }
            if (state == 0x00020001)
                Arena.Bounds = new ArenaBoundsSquare(20);
        }
    }
}

class DeepAerial(BossModule module) : Components.CastTowers(module, AID.DeepAerialTower, 6, 2, 2);

class WateryGrave(BossModule module) : Components.Adds(module, (uint)OID.WateryGrave, 1)
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        foreach (var actor in ActiveActors)
            Arena.AddCircle(actor.Position, 4, ArenaColor.Object);
    }
}

class BubbleTether(BossModule module) : Components.GenericAOEs(module)
{
    private Actor? TargetRed;
    private Actor? TargetBlue;

    private DateTime _next;

    private BitMask _bubble;

    // guessing
    public const int StretchDistance = 28;

    private readonly Actor?[] _tetheredTo = new Actor?[8];

    private Actor? BaitSource(Actor? target) => (target == TargetRed || target == TargetBlue) && Raid.TryFindSlot(target?.InstanceID ?? 0, out var slot) ? _tetheredTo[slot] : null;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (source.OID == (uint)OID.TetherHelper && Raid.TryFindSlot(tether.Target, out var slot))
            _tetheredTo[slot] = source;
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_bubble[slot])
            yield break;

        if (actor != TargetRed && BaitSource(TargetRed) is { } r)
            yield return new(new AOEShapeRect(60, 4), r.Position, r.AngleTo(TargetRed!), _next);
        if (actor != TargetBlue && BaitSource(TargetBlue) is { } b)
            yield return new(new AOEShapeRect(60, 4), b.Position, b.AngleTo(TargetBlue!), _next);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        switch ((IconID)iconID)
        {
            case IconID.BubbleTetherBlue:
                _next = WorldState.FutureTime(5.3f);
                TargetBlue = actor;
                break;
            case IconID.BubbleTetherRed:
                _next = WorldState.FutureTime(5.3f);
                TargetRed = actor;
                break;
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.WateryGrave)
            _bubble.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (BaitSource(pc) is { } source)
            Arena.AddRect(source.Position, source.DirectionTo(pc), 60, 0, 4, ArenaColor.Danger);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (BaitSource(actor) is { } source)
        {
            var hitBubble = Module.Enemies(OID.WateryGrave).Any(b => AIHints.TargetInAOERect(b, source.Position, source.DirectionTo(actor), 60, 4));
            if (actor == TargetRed)
                hints.Add("Hit bubble!", !hitBubble);
            else
                hints.Add("Bait away from bubble!", hitBubble);

            if (actor.Position.InCircle(source.Position, StretchDistance))
                hints.Add("Stretch tether!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (BaitSource(actor) is { } source)
        {
            hints.AddForbiddenZone(ShapeContains.Circle(source.Position, StretchDistance), _next);
            // we need to bait to arena edge, otherwise allies won't have room to stretch tether
            hints.AddForbiddenZone(ShapeContains.Circle(Arena.Center, 18), _next);

            // hit or avoid bubble depending on color
            if (Module.Enemies(OID.WateryGrave).FirstOrDefault() is { } bubble)
            {
                var clipCone = ShapeContains.Cone(source.Position, 100, source.AngleTo(bubble), Angle.Asin(8 / (bubble.Position - source.Position).Length()));
                if (actor == TargetRed)
                    hints.AddForbiddenZone(p => !clipCone(p), _next);
                else
                    hints.AddForbiddenZone(clipCone, _next);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.XtremeWaveRedRect:
                NumCasts++;
                if (Raid.TryFindSlot(TargetRed?.InstanceID ?? 0, out var s))
                {
                    _tetheredTo[s] = null;
                    TargetRed = null;
                }
                break;
            case AID.XtremeWaveBlueRect:
                NumCasts++;
                if (Raid.TryFindSlot(TargetBlue?.InstanceID ?? 0, out var b))
                {
                    _tetheredTo[b] = null;
                    TargetBlue = null;
                }
                break;
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => _bubble[pcSlot] ? PlayerPriority.Normal : player == TargetRed || player == TargetBlue ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
}

class ScathingSteam(BossModule module) : Components.RaidwideCast(module, AID.ScathingSteam);
class ImpactZone(BossModule module) : Components.CastCounter(module, AID.ImpactZone2);
