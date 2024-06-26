namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS1TrinitySeeker;

class ActOfMercy(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.ActOfMercy))
{
    private readonly DateTime _activation = module.WorldState.FutureTime(7.6f); // from verdant path cast start
    private static readonly AOEShapeCross _shape = new(50, 4);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        yield return new(_shape, Module.PrimaryActor.Position, Module.PrimaryActor.Rotation, _activation);
    }
}
