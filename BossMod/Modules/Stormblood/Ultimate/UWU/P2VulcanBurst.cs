using System.Collections.Generic;

namespace BossMod.Stormblood.Ultimate.UWU
{
    class VulcanBurst : Components.KnockbackFromPoints
    {
        protected Actor? Source;

        public VulcanBurst(AID aid) : base(15, ActionID.MakeSpell(aid)) { }

        public override IEnumerable<WPos> Sources(BossModule module)
        {
            if (Source != null)
                yield return Source.Position;
        }
    }

    class P2VulcanBurst : VulcanBurst
    {
        public P2VulcanBurst() : base(AID.VulcanBurst) { }

        public override void Init(BossModule module)
        {
            Source = ((UWU)module).Ifrit();
        }
    }

    class P4VulcanBurst : VulcanBurst
    {
        public P4VulcanBurst() : base(AID.VulcanBurstUltima) { }

        public override void Init(BossModule module)
        {
            Source = ((UWU)module).Ultima();
        }
    }
}
