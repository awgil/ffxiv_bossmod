
namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS7StygimolochLord;

class ViciousSwipe : Components.Knockback
{
    private Source? _source;

    private static readonly AOEShapeCircle _shape = new(8);

    public ViciousSwipe() : base(ActionID.MakeSpell(AID.ViciousSwipe)) { }

    public override IEnumerable<Source> Sources(int slot, Actor actor) => Utils.ZeroOrOne(_source);

    public override void Init(BossModule module) => _source = new(Module.PrimaryActor.Position, 15, WorldState.FutureTime(module.StateMachine.ActiveState?.Duration ?? 0), _shape);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        arena.AddCircle(Module.PrimaryActor.Position, _shape.Radius, ArenaColor.Danger);
    }
}
