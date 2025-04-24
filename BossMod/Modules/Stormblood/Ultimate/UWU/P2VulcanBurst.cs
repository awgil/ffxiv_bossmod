namespace BossMod.Stormblood.Ultimate.UWU;

class VulcanBurst(BossModule module, AID aid, Actor? source) : Components.Knockback(module, aid)
{
    protected Actor? SourceActor = source;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (SourceActor != null)
            yield return new(SourceActor.Position, 15); // TODO: activation
    }
}

class P2VulcanBurst(BossModule module) : VulcanBurst(module, AID.VulcanBurst, ((UWU)module).Ifrit());
class P4VulcanBurst(BossModule module) : VulcanBurst(module, AID.VulcanBurstUltima, ((UWU)module).Ultima());
