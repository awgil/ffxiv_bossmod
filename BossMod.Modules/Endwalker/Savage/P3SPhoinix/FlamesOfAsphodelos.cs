namespace BossMod.Endwalker.Savage.P3SPhoinix;

// state related to flames of asphodelos mechanic
class FlamesOfAsphodelos(BossModule module) : BossComponent(module)
{
    private readonly Angle?[] _directions = new Angle?[3];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (InAOE(_directions[1], actor.Position) || InAOE(_directions[0] != null ? _directions[0] : _directions[2], actor.Position))
        {
            hints.Add("GTFO from cone!");
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_directions[0] != null)
        {
            DrawZone(_directions[0], ArenaColor.Danger);
            DrawZone(_directions[1], ArenaColor.AOE);
        }
        else if (_directions[1] != null)
        {
            DrawZone(_directions[1], ArenaColor.Danger);
            DrawZone(_directions[2], ArenaColor.AOE);
        }
        else
        {
            DrawZone(_directions[2], ArenaColor.Danger);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
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

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
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

    private void DrawZone(Angle? dir, uint color)
    {
        if (dir != null)
        {
            Arena.ZoneIsoscelesTri(Module.Center, dir.Value, 30.Degrees(), 50, color);
            Arena.ZoneIsoscelesTri(Module.Center, dir.Value + 180.Degrees(), 30.Degrees(), 50, color);
        }
    }

    private bool InAOE(Angle? dir, WPos pos)
    {
        if (dir == null)
            return false;

        var toPos = (pos - Module.Center).Normalized();
        return MathF.Abs(dir.Value.ToDirection().Dot(toPos)) >= MathF.Cos(MathF.PI / 6);
    }
}
