namespace BossMod.Endwalker.Savage.P3SPhoinix;

// state related to 'single' and 'multi' fireplumes (normal or parts of gloryplume)
class Fireplume : BossComponent
{
    private WPos? _singlePos = null;
    private Angle _multiStartingDirection;
    private int _multiStartedCasts = 0;
    private int _multiFinishedCasts = 0;

    private static readonly float _singleRadius = 15;
    private static readonly float _multiRadius = 10;
    private static readonly float _multiPairOffset = 15;

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (_singlePos != null && actor.Position.InCircle(_singlePos.Value, _singleRadius))
        {
            hints.Add("GTFO from plume!");
        }

        if (_multiStartedCasts > _multiFinishedCasts)
        {
            if (_multiFinishedCasts > 0 && actor.Position.InCircle(module.Bounds.Center, _multiRadius) ||
                _multiFinishedCasts < 8 && InPair(module, _multiStartingDirection + 45.Degrees(), actor) ||
                _multiFinishedCasts < 6 && InPair(module, _multiStartingDirection - 90.Degrees(), actor) ||
                _multiFinishedCasts < 4 && InPair(module, _multiStartingDirection - 45.Degrees(), actor) ||
                _multiFinishedCasts < 2 && InPair(module, _multiStartingDirection, actor))
            {
                hints.Add("GTFO from plume!");
            }
        }
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (_singlePos != null)
        {
            arena.ZoneCircle(_singlePos.Value, _singleRadius, ArenaColor.AOE);
        }

        if (_multiStartedCasts > _multiFinishedCasts)
        {
            if (_multiFinishedCasts > 0) // don't draw center aoe before first explosion, it's confusing - but start drawing it immediately after first explosion, to simplify positioning
                arena.ZoneCircle(module.Bounds.Center, _multiRadius, _multiFinishedCasts >= 6 ? ArenaColor.Danger : ArenaColor.AOE);

            // don't draw more than two next pairs
            if (_multiFinishedCasts < 8)
                DrawPair(arena, _multiStartingDirection + 45.Degrees(), _multiStartedCasts > 6 && _multiFinishedCasts >= 4);
            if (_multiFinishedCasts < 6)
                DrawPair(arena, _multiStartingDirection - 90.Degrees(), _multiStartedCasts > 4 && _multiFinishedCasts >= 2);
            if (_multiFinishedCasts < 4)
                DrawPair(arena, _multiStartingDirection - 45.Degrees(), _multiStartedCasts > 2);
            if (_multiFinishedCasts < 2)
                DrawPair(arena, _multiStartingDirection, true);
        }
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ExperimentalFireplumeSingleAOE:
            case AID.ExperimentalGloryplumeSingleAOE:
                _singlePos = caster.Position;
                break;
            case AID.ExperimentalFireplumeMultiAOE:
            case AID.ExperimentalGloryplumeMultiAOE:
                if (_multiStartedCasts++ == 0)
                    _multiStartingDirection = Angle.FromDirection(caster.Position - module.Bounds.Center);
                break;
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ExperimentalFireplumeSingleAOE:
            case AID.ExperimentalGloryplumeSingleAOE:
                _singlePos = null;
                break;
            case AID.ExperimentalFireplumeMultiAOE:
            case AID.ExperimentalGloryplumeMultiAOE:
                ++_multiFinishedCasts;
                break;
        }
    }

    private bool InPair(BossModule module, Angle direction, Actor actor)
    {
        var offset = _multiPairOffset * direction.ToDirection();
        return actor.Position.InCircle(module.Bounds.Center - offset, _multiRadius)
            || actor.Position.InCircle(module.Bounds.Center + offset, _multiRadius);
    }

    private void DrawPair(MiniArena arena, Angle direction, bool imminent)
    {
        var offset = _multiPairOffset * direction.ToDirection();
        arena.ZoneCircle(arena.Bounds.Center + offset, _multiRadius, imminent ? ArenaColor.Danger : ArenaColor.AOE);
        arena.ZoneCircle(arena.Bounds.Center - offset, _multiRadius, imminent ? ArenaColor.Danger : ArenaColor.AOE);
    }
}
