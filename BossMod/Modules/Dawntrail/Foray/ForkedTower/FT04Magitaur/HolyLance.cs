using Lumina.Extensions;

namespace BossMod.Dawntrail.Foray.ForkedTower.FT04Magitaur;

class HolyLance(BossModule module) : Components.GenericAOEs(module, AID._Ability_2)
{
    private DateTime _activation;

    public bool Enabled;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!Enabled)
            yield break;

        if (NumCasts % 4 == 0)
            yield return new(FT04Magitaur.NotPlatforms, Arena.Center, Activation: _activation);
        else
        {
            var platform = FT04Magitaur.Platforms[NumCasts < 4 ? 2 : NumCasts < 8 ? 1 : 0];
            yield return new(new AOEShapeRect(10, 10, 10), Arena.Center + platform.Item1, platform.Item2, Activation: _activation);
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.LuminousLance && _activation == default)
            _activation = WorldState.FutureTime(13);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _activation = WorldState.FutureTime(2);
        }
    }
}

// TODO: add hints to not clip other parties with outside-floor stack
class HolyIV(BossModule module) : Components.StackWithIcon(module, (uint)IconID._Gen_Icon_com_share2i, AID._Ability_HolyIV, 6, 8);

class PreyLancepoint(BossModule module) : BossComponent(module)
{
    public record struct Target(Actor Actor, int Platform, int Order, DateTime Activation);

    public readonly List<Target> Targets = [];

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_PreyLancepoint)
        {
            var delay = (status.ExpireAt - WorldState.CurrentTime).TotalSeconds;
            var order = delay switch
            {
                < 20 => 1,
                < 30 => 2,
                < 40 => 3,
                _ => 0
            };
            if (order == 0)
            {
                ReportError($"unrecognized duration for status on {actor}");
                return;
            }
            var platform = FT04Magitaur.GetPlatform(actor.Position);
            Targets.Add(new(actor, platform, order, status.ExpireAt));
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_PreyLancepoint)
            Targets.RemoveAll(t => t.Actor == actor);
    }

    public static readonly string[] PlatformNames = ["A", "B", "C"];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Targets.FirstOrNull(t => t.Actor == actor) is { } t)
        {
            if (t.Platform < 0)
            {
                hints.Add("Sac! Your stack cannot be resolved!");
                return;
            }

            var stackOutside = t.Platform switch
            {
                0 => t.Order == 3,
                1 => t.Order == 2,
                2 => t.Order == 1,
                _ => false
            };

            var stackStr = stackOutside ? "outside" : "inside";

            hints.Add($"Order: {t.Order}, platform: {PlatformNames[t.Platform]} {stackStr}", false);
        }
    }
}
