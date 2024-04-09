namespace BossMod.Endwalker.Savage.P10SPandaemonium;

class PandaemoniacRay(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PandaemoniacRayAOE), new AOEShapeRect(30, 25));

class JadePassage : Components.GenericAOEs
{
    private IReadOnlyList<Actor> _spheres;
    private DateTime _activation;

    private static readonly AOEShapeRect _shape = new(40, 1, 40);

    public JadePassage(BossModule module) : base(module, ActionID.MakeSpell(AID.JadePassage))
    {
        _spheres = module.Enemies(OID.ArcaneSphere);
        _activation = WorldState.FutureTime(3.6f);
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _spheres.Where(s => !s.IsDead).Select(s => new AOEInstance(_shape, s.Position, s.Rotation, s.CastInfo?.NPCFinishAt ?? _activation));
}
