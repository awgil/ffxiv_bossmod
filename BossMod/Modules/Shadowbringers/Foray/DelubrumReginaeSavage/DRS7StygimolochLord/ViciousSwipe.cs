namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS7StygimolochLord;

sealed class ViciousSwipe(BossModule module) : Components.GenericKnockback(module, (uint)AID.ViciousSwipe)
{
    private readonly Knockback[] _kb = [new(module.PrimaryActor.Position, 15f, module.WorldState.FutureTime(module.StateMachine.ActiveState?.Duration ?? default), _shape)];

    private static readonly AOEShapeCircle _shape = new(8f);

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => _kb;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.ZoneCircleOutline(Module.PrimaryActor.Position, _shape.Radius);
    }
}
