using System.Runtime.InteropServices;

namespace BossMod.Dawntrail.Extreme.Ex7Doomtrain;

class MultiToot(BossModule module) : Components.GenericStackSpread(module)
{
    public int NumCasts { get; private set; }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        switch ((IconID)iconID)
        {
            case IconID.DoubleToot:
                foreach (var player in Raid.WithoutSlot().OrderByDescending(r => r.Class.GetRole2() == Role2.Healer).Take(2))
                    Stacks.Add(new(player, 6, minSize: 2, maxSize: 3, WorldState.FutureTime(10)));
                break;
            case IconID.TripleToot:
                foreach (var player in Raid.WithoutSlot().Where(r => r.Class.GetRole2() != Role2.Tank))
                    Spreads.Add(new(player, 6, WorldState.FutureTime(10)));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Aetherochar or AID.Aetherosote)
        {
            Stacks.Clear();
            Spreads.Clear();
        }
    }
}

// tankbuster happens ~9.7 seconds after Horn icon (icon has no visual, just SFX)
// train distance determined by SID 4541 on Ghost Train actor
class AetherialRay(BossModule module) : Components.GenericBaitAway(module, AID.AetherialRay, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    private Angle _nextRotation;
    private Actor? _nextSource;
    private DateTime _nextActivation;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Distance)
        {
            _nextRotation = status.Extra switch
            {
                0x578 => 170.Degrees(),
                0x960 => 106.Degrees(),
                _ => default
            };
            if (_nextRotation == default)
                ReportError($"Unrecognized status {status.Extra:X} on Ghost Train, don't know where to predict");
        }

        if ((SID)status.ID == SID.Stop)
        {
            foreach (ref var bait in CollectionsMarshal.AsSpan(CurrentBaits))
                bait.Source = actor;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.Horn)
        {
            var gh = Module.Enemies(OID.GhostTrain)[0];
            var g = (gh.Position - Arena.Center).ToAngle() + _nextRotation;
            var offset = g.ToDirection() * 25;
            _nextSource = new Actor(1, 0, 819, 0, "fake actor", 0, ActorType.Enemy, Class.ACN, 1, new Vector4(Arena.Center.X + offset.X, gh.PosRot.Y, Arena.Center.Z + offset.Z, 0));
            _nextActivation = WorldState.FutureTime(9.7f);
        }

        if ((IconID)iconID == IconID.AetherialRay && _nextSource != null)
            CurrentBaits.Add(new(_nextSource, actor, new AOEShapeCone(50, 17.5f.Degrees()), _nextActivation));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            CurrentBaits.Clear();
        }
    }
}

class RunawayTrain(BossModule module) : Components.RaidwideInstant(module, AID.RunawayTrainRaidwide, 15.2f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if ((AID)spell.Action.ID == AID.RunawayTrainEnd)
            Activation = WorldState.FutureTime(Delay);
    }
}
