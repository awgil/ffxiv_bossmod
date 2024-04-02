namespace BossMod.Shadowbringers.Ultimate.TEA;

class P2SuperJump : Components.GenericBaitAway
{
    private static readonly AOEShapeCircle _shape = new(10);

    public P2SuperJump() : base(ActionID.MakeSpell(AID.SuperJumpAOE), centerAtTarget: true) { }

    public override void Update(BossModule module)
    {
        CurrentBaits.Clear();
        var source = ((TEA)module).BruteJustice();
        var target = source != null ? module.Raid.WithoutSlot().Farthest(source.Position) : null;
        if (source != null && target != null)
            CurrentBaits.Add(new(source, target, _shape));
    }
}
