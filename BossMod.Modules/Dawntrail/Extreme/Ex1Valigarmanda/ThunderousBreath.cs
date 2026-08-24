namespace BossMod.Dawntrail.Extreme.Ex1Valigarmanda;

class ThunderousBreath : Components.CastCounter
{
    public ThunderousBreath(BossModule module) : base(module, AID.ThunderousBreathAOE)
    {
        var platform = module.FindComponent<ThunderPlatform>();
        if (platform != null)
            foreach (var (i, _) in module.Raid.WithSlot(true))
                platform.RequireHint[i] = platform.RequireLevitating[i] = true;
    }
}

class ArcaneLighning(BossModule module) : Components.GenericAOEs(module, AID.ArcaneLightning)
{
    public readonly List<AOEInstance> AOEs = [];

    private static readonly AOEShapeRect _shape = new(50, 2.5f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs;

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.ArcaneSphere)
        {
            AOEs.Add(new(_shape, actor.Position, actor.Rotation, WorldState.FutureTime(8.6f)));
        }
    }
}
