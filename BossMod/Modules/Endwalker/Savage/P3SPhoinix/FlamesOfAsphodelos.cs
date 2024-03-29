namespace BossMod.Endwalker.Savage.P3SPhoinix;

// state related to flames of asphodelos mechanic
class FlamesOfAsphodelos : BossComponent
{
    private Angle?[] _directions = new Angle?[3];

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (InAOE(module, _directions[1], actor.Position) || InAOE(module, _directions[0] != null ? _directions[0] : _directions[2], actor.Position))
        {
            hints.Add("GTFO from cone!");
        }
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (_directions[0] != null)
        {
            DrawZone(arena, _directions[0], ArenaColor.Danger);
            DrawZone(arena, _directions[1], ArenaColor.AOE);
        }
        else if (_directions[1] != null)
        {
            DrawZone(arena, _directions[1], ArenaColor.Danger);
            DrawZone(arena, _directions[2], ArenaColor.AOE);
        }
        else
        {
            DrawZone(arena, _directions[2], ArenaColor.Danger);
        }
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FlamesOfAsphodelosAOE1:
                _directions[0] = caster.Rotation;
                break;
            case AID.FlamesOfAsphodelosAOE2:
                _directions[1] = caster.Rotation;
                break;
            case AID.FlamesOfAsphodelosAOE3:
                _directions[2] = caster.Rotation;
                break;
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FlamesOfAsphodelosAOE1:
                _directions[0] = null;
                break;
            case AID.FlamesOfAsphodelosAOE2:
                _directions[1] = null;
                break;
            case AID.FlamesOfAsphodelosAOE3:
                _directions[2] = null;
                break;
        }
    }

    private void DrawZone(MiniArena arena, Angle? dir, uint color)
    {
        if (dir != null)
        {
            arena.ZoneIsoscelesTri(arena.Bounds.Center, dir.Value, 30.Degrees(), 50, color);
            arena.ZoneIsoscelesTri(arena.Bounds.Center, dir.Value + 180.Degrees(), 30.Degrees(), 50, color);
        }
    }

    private bool InAOE(BossModule module, Angle? dir, WPos pos)
    {
        if (dir == null)
            return false;

        var toPos = (pos - module.Bounds.Center).Normalized();
        return MathF.Abs(dir.Value.ToDirection().Dot(toPos)) >= MathF.Cos(MathF.PI / 6);
    }
}
