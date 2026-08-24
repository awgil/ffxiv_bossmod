namespace BossMod.Endwalker.Savage.P10SPandaemonium;

class PandaemoniacRay(BossModule module) : Components.StandardAOEs(module, AID.PandaemoniacRayAOE, new AOEShapeRect(30, 25));

class JadePassage(BossModule module) : Components.GenericAOEs(module, AID.JadePassage)
{
    private readonly IReadOnlyList<Actor> _spheres = module.Enemies(OID.ArcaneSphere);
    private readonly DateTime _activation = module.WorldState.FutureTime(3.6f);

    private static readonly AOEShapeRect _shape = new(40, 1, 40);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _spheres.Where(s => !s.IsDead).Select(s => new AOEInstance(_shape, s.Position, s.Rotation, Module.CastFinishAt(s.CastInfo, 0, _activation)));
}
