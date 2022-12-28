using System.Collections.Generic;

namespace BossMod.Stormblood.Ultimate.UWU
{
    class VulcanBurst : Components.Knockback
    {
        protected Actor? SourceActor;

        public VulcanBurst(AID aid) : base(ActionID.MakeSpell(aid)) { }

        public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
        {
            if (SourceActor != null)
                yield return new(SourceActor.Position, 15); // TODO: activation
        }
    }

    class P2VulcanBurst : VulcanBurst
    {
        public P2VulcanBurst() : base(AID.VulcanBurst) { }

        public override void Init(BossModule module)
        {
            SourceActor = ((UWU)module).Ifrit();
        }
    }

    class P4VulcanBurst : VulcanBurst
    {
        public P4VulcanBurst() : base(AID.VulcanBurstUltima) { }

        public override void Init(BossModule module)
        {
            SourceActor = ((UWU)module).Ultima();
        }
    }
}
