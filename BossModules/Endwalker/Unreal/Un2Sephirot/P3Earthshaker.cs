namespace BossMod.Endwalker.Unreal.Un2Sephirot;

class P3Earthshaker(BossModule module) : Components.GenericAOEs(module, AID.EarthShakerAOE)
{
    private BitMask _targets;

    public bool Active => _targets.Any() && NumCasts < 2;

    private static readonly AOEShape _shape = new AOEShapeCone(60, 15.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var origin = Module.Enemies(OID.BossP3).FirstOrDefault();
        if (origin == null)
            yield break;

        // TODO: timing...
        foreach (var target in Raid.WithSlot(true).IncludedInMask(_targets))
            yield return new(_shape, origin.Position, Angle.FromDirection(target.Item2.Position - origin.Position));
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => _targets[playerSlot] ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.Earthshaker)
            _targets.Set(Raid.FindSlot(actor.InstanceID));
    }
}
