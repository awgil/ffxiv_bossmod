namespace BossMod.Endwalker.Unreal.Un3Sophia;

// shows all three demiurges + handles directional parry from first; the reason is to simplify condition checks
class Demiurges : Components.DirectionalParry
{
    private IReadOnlyList<Actor> _second = ActorEnumeration.EmptyList;
    private IReadOnlyList<Actor> _third = ActorEnumeration.EmptyList;

    public bool AddsActive => ActiveActors.Any() || _second.Any(a => a.IsTargetable && !a.IsDead) || _third.Any(a => a.IsTargetable && !a.IsDead);

    public Demiurges() : base((uint)OID.Demiurge1) { }

    public override void Init(BossModule module)
    {
        base.Init(module);
        _second = module.Enemies(OID.Demiurge2);
        _third = module.Enemies(OID.Demiurge3);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(module, pcSlot, pc, arena);
        arena.Actors(_second, ArenaColor.Enemy);
        arena.Actors(_third, ArenaColor.Enemy);
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

class DivineSpark(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.DivineSpark));

class GnosticRant(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.GnosticRant), new AOEShapeCone(40, 135.Degrees()));

class GnosticSpear(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.GnosticSpear), new AOEShapeRect(20.75f, 2, 0.75f));

class RingOfPain(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 5, ActionID.MakeSpell(AID.RingOfPain), m => m.Enemies(OID.RingOfPain).Where(z => z.EventState != 7), 1.7f);

class Infusion : Components.GenericWildCharge
{
    public Infusion() : base(5, ActionID.MakeSpell(AID.Infusion)) { }

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
