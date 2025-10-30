namespace BossMod.Dawntrail.Extreme.Ex3Sphene;

class Aeroquell(BossModule module) : Components.StackWithCastTargets(module, AID.Aeroquell, 5, 4);
class AeroquellTwister(BossModule module) : Components.PersistentVoidzone(module, 5, m => m.Enemies(OID.Twister));
class MissingLink(BossModule module) : Components.Chains(module, (uint)TetherID.MissingLink, default, 25);

class WindOfChange(BossModule module) : Components.Knockback(module, AID.WindOfChange, true)
{
    private readonly Angle[] _directions = new Angle[PartyState.MaxPartySize];
    private DateTime _activation;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_directions[slot] != default)
            yield return new(actor.Position, 20, _activation, null, _directions[slot], Kind.DirForward);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var dir = (SID)status.ID switch
        {
            SID.WestWindOfChange => 90.Degrees(),
            SID.EastWindOfChange => -90.Degrees(),
            _ => default
        };
        if (dir != default && Raid.TryFindSlot(actor.InstanceID, out var slot))
        {
            _directions[slot] = dir;
            _activation = status.ExpireAt;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            if (Raid.TryFindSlot(spell.MainTargetID, out var slot))
                _directions[slot] = default;
        }
    }
}
