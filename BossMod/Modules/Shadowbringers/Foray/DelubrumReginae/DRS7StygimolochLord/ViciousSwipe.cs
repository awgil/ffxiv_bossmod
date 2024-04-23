namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS7StygimolochLord;

class ViciousSwipe(BossModule module) : Components.Knockback(module, ActionID.MakeSpell(AID.ViciousSwipe))
{
    private Source? _source = new(module.PrimaryActor.Position, 15, module.WorldState.FutureTime(module.StateMachine.ActiveState?.Duration ?? 0), _shape);

    private static readonly AOEShapeCircle _shape = new(8);

    public override IEnumerable<Source> Sources(int slot, Actor actor) => Utils.ZeroOrOne(_source);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.AddCircle(Module.PrimaryActor.Position, _shape.Radius, ArenaColor.Danger);
    }
}
