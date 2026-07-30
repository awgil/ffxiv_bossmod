namespace BossMod.Endwalker.Savage.P3SPhoinix;

// bird distance utility
// when small birds die and large birds appear, they cast 26328, and if it hits any other large bird, they buff
// when large birds die and sparkfledgeds appear, they cast 26329, and if it hits any other sparkfledged, they wipe the raid or something
// so we show range helper for dead birds
class BirdDistance(BossModule module, uint watchedBirdsID) : BossComponent(module)
{
    private readonly uint _watchedBirdsID = watchedBirdsID;
    private BitMask _birdsAtRisk;

    private const float _radius = 13f;

    public override void Update()
    {
        _birdsAtRisk.Reset();
        var watchedBirds = Module.Enemies(_watchedBirdsID);
        for (var i = 0; i < watchedBirds.Count; ++i)
        {
            var bird = watchedBirds[i];
            if (!bird.IsDead && watchedBirds.Where(other => other.IsDead).InRadius(bird.Position, _radius).Any())
            {
                _birdsAtRisk.Set(i);
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var watchedBirds = Module.Enemies(_watchedBirdsID);
        for (var i = 0; i < watchedBirds.Count; ++i)
        {
            var bird = watchedBirds[i];
            if (!bird.IsDead && bird.TargetID == actor.InstanceID && _birdsAtRisk[i])
            {
                hints.Add("Drag bird away!");
                return;
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        // draw alive birds tanked by PC and circles around dead birds
        var watchedBirds = Module.Enemies(_watchedBirdsID);
        for (var i = 0; i < watchedBirds.Count; ++i)
        {
            var bird = watchedBirds[i];
            if (bird.IsDead)
            {
                Arena.ZoneCircleOutline(bird.Position, _radius, Colors.Danger);
            }
            else if (bird.TargetID == pc.InstanceID)
            {
                Arena.Actor(bird, _birdsAtRisk[i] ? Colors.Enemy : Colors.PlayerGeneric);
            }
        }
    }
}

class SmallBirdDistance(BossModule module) : BirdDistance(module, (uint)OID.SunbirdSmall);
class LargeBirdDistance(BossModule module) : BirdDistance(module, (uint)OID.SunbirdLarge);
