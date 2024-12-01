namespace BossMod.Dawntrail.Alliance.A13ArkAngels;

class Utsusemi(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _clones = [];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var source = TetheredClone(actor);
        if (source != null)
            hints.Add("Kite the add!", (source.Position - actor.Position).LengthSq() < 100);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var source = TetheredClone(pc);
        if (source != null)
        {
            Arena.Actor(source, ArenaColor.Object, true);
            Arena.AddLine(source.Position, pc.Position, ArenaColor.Danger);
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Utsusemi)
            _clones.Add(source);
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Utsusemi)
            _clones.Remove(source);
    }

    private Actor? TetheredClone(Actor target) => _clones.Find(c => c.Tether.Target == target.InstanceID);
}
