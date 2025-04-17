namespace BossMod.Endwalker.Savage.P8S1Hephaistos;

class Footprint(BossModule module) : Components.Knockback(module, AID.Footprint)
{
    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        yield return new(Module.PrimaryActor.Position, 20); // TODO: activation
    }
}
