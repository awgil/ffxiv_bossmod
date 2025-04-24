namespace BossMod.Endwalker.Extreme.Ex4Barbariccia;

class StiffBreeze(BossModule module) : Components.GenericAOEs(module, AID.Tousle)
{
    private static readonly AOEShape _shape = new AOEShapeCircle(1); // note: actual aoe, if triggered, has radius 2, but we care about triggering radius

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return Module.Enemies(OID.StiffBreeze).Select(o => new AOEInstance(_shape, o.Position));
    }
}
