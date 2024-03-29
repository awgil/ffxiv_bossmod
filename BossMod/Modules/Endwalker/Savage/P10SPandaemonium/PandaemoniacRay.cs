namespace BossMod.Endwalker.Savage.P10SPandaemonium;

class PandaemoniacRay : Components.SelfTargetedAOEs
{
    public PandaemoniacRay() : base(ActionID.MakeSpell(AID.PandaemoniacRayAOE), new AOEShapeRect(30, 25)) { }
}

class JadePassage : Components.GenericAOEs
{
    private IReadOnlyList<Actor> _spheres = ActorEnumeration.EmptyList;
    private DateTime _activation;

    private static readonly AOEShapeRect _shape = new(40, 1, 40);

    public JadePassage() : base(ActionID.MakeSpell(AID.JadePassage)) { }

    public override void Init(BossModule module)
    {
        _spheres = module.Enemies(OID.ArcaneSphere);
        _activation = module.WorldState.CurrentTime.AddSeconds(3.6f);
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        return _spheres.Where(s => !s.IsDead).Select(s => new AOEInstance(_shape, s.Position, s.Rotation, s.CastInfo?.NPCFinishAt ?? _activation));
    }
}
