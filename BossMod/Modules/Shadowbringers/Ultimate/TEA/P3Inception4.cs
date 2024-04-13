namespace BossMod.Shadowbringers.Ultimate.TEA;

// TODO: assign positions?
class P3Inception4Cleaves(BossModule module) : Components.GenericBaitAway(module, ActionID.MakeSpell(AID.AlphaSwordP3))
{
    private static readonly AOEShapeCone _shape = new(30, 45.Degrees()); // TODO: verify angle

    public override void Update()
    {
        CurrentBaits.Clear();
        var source = ((TEA)Module).CruiseChaser();
        if (source != null)
            CurrentBaits.AddRange(Raid.WithoutSlot().SortedByRange(source.Position).Take(3).Select(t => new Bait(source, t, _shape)));
    }
}
