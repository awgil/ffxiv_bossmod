namespace BossMod.Shadowbringers.Alliance.A25Compound2P;

class FourPartsResolve(BossModule module) : Components.GenericBaitAway(module)
{
    private int progress;
    private readonly Actor?[] _targets = new Actor?[4];

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var index = (IconID)iconID switch
        {
            IconID.Order1 => 0,
            IconID.Order2 => 1,
            IconID.Order3 => 2,
            IconID.Order4 => 3,
            _ => -1
        };
        if (index >= 0)
            _targets[index] = actor;

        if (index == 0)
            AddBait(actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.FourPartsResolveJump or AID.FourPartsResolveRect)
        {
            progress++;
            if (CurrentBaits.Count > 0)
                CurrentBaits.RemoveAt(0);
            if (progress >= 4)
            {
                Array.Fill(_targets, null);
                progress = 0;
            }
            else
                AddBait(_targets[progress]);
        }
    }

    private void AddBait(Actor? target)
    {
        if (target == null)
            return;
        var src = ((A25Compound2P)Module).BossP2;
        if (src == null)
            return;

        var activation = progress switch
        {
            0 => WorldState.FutureTime(7.4f),
            2 => WorldState.FutureTime(4.4f),
            _ => WorldState.FutureTime(2.4f),
        };

        AOEShape shape = progress % 2 == 0 ? new AOEShapeCircle(6) : new AOEShapeRect(85, 6);
        CenterAtTarget = shape is AOEShapeCircle;
        CurrentBaits.Add(new(src, target, shape, activation));
    }
}
