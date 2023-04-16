using System.Linq;

namespace BossMod.Shadowbringers.Ultimate.TEA
{
    // TODO: assign positions?
    class P3Inception4Cleaves : Components.GenericBaitAway
    {
        private static AOEShapeCone _shape = new(30, 45.Degrees()); // TODO: verify angle

        public P3Inception4Cleaves() : base(ActionID.MakeSpell(AID.AlphaSwordP3)) { }

        public override void Update(BossModule module)
        {
            CurrentBaits.Clear();
            var source = ((TEA)module).CruiseChaser();
            if (source != null)
                CurrentBaits.AddRange(module.Raid.WithoutSlot().SortedByRange(source.Position).Take(3).Select(t => new Bait(source, t, _shape)));
        }
    }
}
