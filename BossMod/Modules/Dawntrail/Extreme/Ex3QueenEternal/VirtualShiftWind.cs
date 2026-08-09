namespace BossMod.Dawntrail.Extreme.Ex3QueenEternal;

sealed class Aeroquell(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.Aeroquell, 5f, 4);
sealed class AeroquellTwister(BossModule module) : Components.Voidzone(module, 5f, GetTwister)
{
    private static List<Actor> GetTwister(BossModule module) => module.Enemies((uint)OID.Twister);
}
sealed class MissingLink(BossModule module) : Components.Chains(module, (uint)TetherID.MissingLink, default, 25f);

sealed class WindOfChange(BossModule module) : Components.GenericKnockback(module, (uint)AID.WindOfChange)
{
    private readonly Angle[] _directions = new Angle[PartyState.MaxPartySize];
    private DateTime _activation;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (_directions[slot] != default)
        {
            return new Knockback[1] { new(actor.Position, 20f, _activation, null, _directions[slot], Kind.DirForward, ignoreImmunes: true) };
        }
        return [];
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        var dir = status.ID switch
        {
            (uint)SID.WestWindOfChange => 90f.Degrees(),
            (uint)SID.EastWindOfChange => -90f.Degrees(),
            _ => default
        };
        if (dir != default && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            _directions[slot] = dir;
            _activation = status.ExpireAt;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            if (Raid.FindSlot(spell.MainTargetID) is var slot && slot >= 0)
            {
                _directions[slot] = default;
            }
        }
    }
}
