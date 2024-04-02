
namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS7StygimolochLord;

class ViciousSwipe : Components.Knockback
{
    private Source? _source;

    private static readonly AOEShapeCircle _shape = new(8);

    public ViciousSwipe() : base(ActionID.MakeSpell(AID.ViciousSwipe)) { }

    public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor) => Utils.ZeroOrOne(_source);

    public override void Init(BossModule module) => _source = new(module.PrimaryActor.Position, 15, module.WorldState.CurrentTime.AddSeconds(module.StateMachine.ActiveState?.Duration ?? 0), _shape);

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        arena.AddCircle(module.PrimaryActor.Position, _shape.Radius, ArenaColor.Danger);
    }
}
