namespace BossMod.Endwalker.Criterion.C01ASS.C011Silkie;

class PuffTethers(BossModule module, bool originAtBoss) : BossComponent(module)
{
    private readonly bool _originAtBoss = originAtBoss;
    private readonly PuffTracker? _tracker = module.FindComponent<PuffTracker>();
    private SlipperySoap.Color _bossColor;

    private const uint _hintColor = 0x40008080;

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_tracker == null)
            return;
        DrawTetherHints(pc, _tracker.ChillingPuffs, false);
        DrawTetherHints(pc, _tracker.FizzlingPuffs, true);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_tracker == null)
            return;
        DrawTether(pc, _tracker.ChillingPuffs);
        DrawTether(pc, _tracker.FizzlingPuffs);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (actor != Module.PrimaryActor)
            return;
        var color = SlipperySoap.ColorForStatus(status.ID);
        if (color != SlipperySoap.Color.None)
            _bossColor = color;
    }

    private void DrawTetherHints(Actor player, List<Actor> puffs, bool yellow)
    {
        var source = puffs.Find(p => p.Tether.Target == player.InstanceID);
        if (source == null)
            return;

        var moveDir = (player.Position - source.Position).Normalized();
        var movePos = source.Position + 10 * moveDir;
        var moveAngle = Angle.FromDirection(moveDir);
        if (yellow)
        {
            C011Silkie.ShapeYellow.Draw(Arena, movePos, moveAngle + 45.Degrees(), _hintColor);
            C011Silkie.ShapeYellow.Draw(Arena, movePos, moveAngle + 135.Degrees(), _hintColor);
            C011Silkie.ShapeYellow.Draw(Arena, movePos, moveAngle - 135.Degrees(), _hintColor);
            C011Silkie.ShapeYellow.Draw(Arena, movePos, moveAngle - 45.Degrees(), _hintColor);
        }
        else
        {
            C011Silkie.ShapeBlue.Draw(Arena, movePos, moveAngle, _hintColor);
        }

        var bossOrigin = _originAtBoss ? Module.PrimaryActor.Position : Module.Center;
        switch (_bossColor)
        {
            case SlipperySoap.Color.Green:
                C011Silkie.ShapeGreen.Draw(Arena, bossOrigin, new(), _hintColor);
                break;
            case SlipperySoap.Color.Blue:
                C011Silkie.ShapeBlue.Draw(Arena, bossOrigin, new(), _hintColor);
                break;
        }
    }

    private void DrawTether(Actor player, List<Actor> puffs)
    {
        var source = puffs.Find(p => p.Tether.Target == player.InstanceID);
        if (source != null)
        {
            Arena.AddLine(source.Position, player.Position, ArenaColor.Danger);
        }
    }
}
class PuffTethers1(BossModule module) : PuffTethers(module, false);
class PuffTethers2(BossModule module) : PuffTethers(module, true);
