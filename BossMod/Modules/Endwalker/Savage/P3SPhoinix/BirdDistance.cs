namespace BossMod.Endwalker.Savage.P3SPhoinix;

// bird distance utility
// when small birds die and large birds appear, they cast 26328, and if it hits any other large bird, they buff
// when large birds die and sparkfledgeds appear, they cast 26329, and if it hits any other sparkfledged, they wipe the raid or something
// so we show range helper for dead birds
class BirdDistance : BossComponent
{
    private OID _watchedBirdsID;
    private BitMask _birdsAtRisk;

    private static readonly float _radius = 13;

    public BirdDistance(OID watchedBirdsID)
    {
        _watchedBirdsID = watchedBirdsID;
    }

    public override void Update(BossModule module)
    {
        _birdsAtRisk.Reset();
        var watchedBirds = module.Enemies(_watchedBirdsID);
        for (int i = 0; i < watchedBirds.Count; ++i)
        {
            var bird = watchedBirds[i];
            if (!bird.IsDead && watchedBirds.Where(other => other.IsDead).InRadius(bird.Position, _radius).Any())
            {
                _birdsAtRisk.Set(i);
            }
        }
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        var watchedBirds = module.Enemies(_watchedBirdsID);
        for (int i = 0; i < watchedBirds.Count; ++i)
        {
            var bird = watchedBirds[i];
            if (!bird.IsDead && bird.TargetID == actor.InstanceID && _birdsAtRisk[i])
            {
                hints.Add("Drag bird away!");
                return;
            }
        }
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        // draw alive birds tanked by PC and circles around dead birds
        var watchedBirds = module.Enemies(_watchedBirdsID);
        for (int i = 0; i < watchedBirds.Count; ++i)
        {
            var bird = watchedBirds[i];
            if (bird.IsDead)
            {
                arena.AddCircle(bird.Position, _radius, ArenaColor.Danger);
            }
            else if (bird.TargetID == pc.InstanceID)
            {
                arena.Actor(bird, _birdsAtRisk[i] ? ArenaColor.Enemy : ArenaColor.PlayerGeneric);
            }
        }
    }
}

class SmallBirdDistance : BirdDistance
{
    public SmallBirdDistance() : base(OID.SunbirdSmall) { }
}

class LargeBirdDistance : BirdDistance
{
    public LargeBirdDistance() : base(OID.SunbirdLarge) { }
}
