namespace BossMod.Endwalker.Unreal.Un3Sophia;

// shows all three demiurges + handles directional parry from first; the reason is to simplify condition checks
class Demiurges(BossModule module) : Components.DirectionalParry(module, (uint)OID.Demiurge1)
{
    private readonly IReadOnlyList<Actor> _second = module.Enemies(OID.Demiurge2);
    private readonly IReadOnlyList<Actor> _third = module.Enemies(OID.Demiurge3);

    public bool AddsActive => ActiveActors.Any() || _second.Any(a => a.IsTargetable && !a.IsDead) || _third.Any(a => a.IsTargetable && !a.IsDead);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        Arena.Actors(_second, ArenaColor.Enemy);
        Arena.Actors(_third, ArenaColor.Enemy);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var sides = (AID)spell.Action.ID switch
        {
            AID.VerticalKenoma => Side.Front | Side.Back,
            AID.HorizontalKenoma => Side.Left | Side.Right,
            _ => Side.None
        };
        if (sides != Side.None)
            PredictParrySide(caster.InstanceID, sides);
    }
}

class DivineSpark(BossModule module) : Components.CastGaze(module, AID.DivineSpark);
class GnosticRant(BossModule module) : Components.StandardAOEs(module, AID.GnosticRant, new AOEShapeCone(40, 135.Degrees()));
class GnosticSpear(BossModule module) : Components.StandardAOEs(module, AID.GnosticSpear, new AOEShapeRect(20.75f, 2));
class RingOfPain(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 5, AID.RingOfPain, m => m.Enemies(OID.RingOfPain).Where(z => z.EventState != 7), 1.7f);

class Infusion(BossModule module) : Components.GenericWildCharge(module, 5, AID.Infusion)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            Source = caster;
            foreach (var (slot, player) in Raid.WithSlot())
            {
                PlayerRoles[slot] = player.InstanceID == spell.TargetID ? PlayerRole.Target : PlayerRole.Share;
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Source = null;
    }
}
