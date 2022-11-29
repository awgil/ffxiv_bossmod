using System.Collections.Generic;

namespace BossMod.Stormblood.Ultimate.UWU
{
    class P2VulcanBurst : Components.KnockbackFromPoints
    {
        public P2VulcanBurst() : base(15, ActionID.MakeSpell(AID.VulcanBurst)) { }

        public override IEnumerable<WPos> Sources(BossModule module)
        {
            var source = ((UWU)module).Ifrit();
            if (source != null)
                yield return source.Position;
        }
    }
}
