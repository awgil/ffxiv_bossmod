namespace BossMod.Components;

// generic 'directional parry' component that shows actors and sides it's forbidden to attack them from
// uses common status + custom prediction
public class DirectionalParry(BossModule module, uint actorOID) : Adds(module, actorOID)
{
    [Flags]
    public enum Side
    {
        None = 0x0,
        Front = 0x1,
        Back = 0x2,
        Left = 0x4,
        Right = 0x8,
        All = 0xF
    }

    public const uint ParrySID = 680; // common 'directional parry' status

    private readonly Dictionary<ulong, int> _actorStates = []; // value == active-side | (imminent-side << 4)
    public bool Active => _actorStates.Values.Any(s => ActiveSides(s) != Side.None);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var target = Actors.FirstOrDefault(w => w.InstanceID == actor.TargetID);
        if (target != null && _actorStates.TryGetValue(actor.TargetID, out var targetState))
        {
            var forbiddenSides = ActiveSides(targetState) | ImminentSides(targetState);
            var attackDir = (actor.Position - target.Position).Normalized();
            var facing = target.Rotation.ToDirection();
            bool attackingFromForbidden = attackDir.Dot(facing) switch
            {
                > 0.7071067f => forbiddenSides.HasFlag(Side.Front),
                < -0.7071067f => forbiddenSides.HasFlag(Side.Back),
                _ => forbiddenSides.HasFlag(attackDir.Dot(facing.OrthoL()) > 0 ? Side.Left : Side.Right)
            };
            if (attackingFromForbidden)
            {
                hints.Add("Attack target from unshielded side!");
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        foreach (var a in ActiveActors)
        {
            if (_actorStates.TryGetValue(a.InstanceID, out var aState))
            {
                var active = ActiveSides(aState);
                var imminent = ImminentSides(aState);
                DrawParry(a, 0.Degrees(), active, imminent, Side.Front);
                DrawParry(a, 180.Degrees(), active, imminent, Side.Back);
                DrawParry(a, 90.Degrees(), active, imminent, Side.Left);
                DrawParry(a, 270.Degrees(), active, imminent, Side.Right);
            }
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == ParrySID)
        {
            // TODO: front+back is 3, left+right is C, but I don't really know which is which, didn't see examples yet...
            // remove any predictions
            _actorStates[actor.InstanceID] = status.Extra & 0xF;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == ParrySID)
        {
            UpdateState(actor.InstanceID, ActorState(actor.InstanceID) & ~0xF);
        }
    }

    public void PredictParrySide(ulong instanceID, Side sides)
    {
        UpdateState(instanceID, ((int)sides << 4) | ActorState(instanceID) & 0xF);
    }

    private void DrawParry(Actor actor, Angle offset, Side active, Side imminent, Side check)
    {
        if (active.HasFlag(check))
            DrawParry(actor, offset, ArenaColor.Enemy);
        else if (imminent.HasFlag(check))
            DrawParry(actor, offset, ArenaColor.Danger);
    }

    private void DrawParry(Actor actor, Angle offset, uint color)
    {
        var dir = actor.Rotation + offset;
        Arena.PathArcTo(actor.Position, 1.5f, (dir - 45.Degrees()).Rad, (dir + 45.Degrees()).Rad);
        Arena.PathStroke(false, color);
    }

    private int ActorState(ulong instanceID) => _actorStates.GetValueOrDefault(instanceID, 0);

    private void UpdateState(ulong instanceID, int state)
    {
        if (state == 0)
            _actorStates.Remove(instanceID);
        else
            _actorStates[instanceID] = state;
    }

    private static Side ActiveSides(int state) => (Side)(state & (int)Side.All);
    private static Side ImminentSides(int state) => ActiveSides(state >> 4);
}
