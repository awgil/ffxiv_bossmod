namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS1TrinitySeeker;

class ActOfMercy : Components.GenericAOEs
{
    private DateTime _activation;
    private static readonly AOEShapeCross _shape = new(50, 4);

    public ActOfMercy() : base(ActionID.MakeSpell(AID.ActOfMercy)) { }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        yield return new(_shape, Module.PrimaryActor.Position, Module.PrimaryActor.Rotation, _activation);
    }

    public override void Init(BossModule module) => _activation = WorldState.FutureTime(7.6f); // from verdant path cast start
}
