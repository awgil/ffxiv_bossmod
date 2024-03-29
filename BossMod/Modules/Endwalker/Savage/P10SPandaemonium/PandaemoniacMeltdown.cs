namespace BossMod.Endwalker.Savage.P10SPandaemonium;

// TODO: generalize (line stack/spread)
class PandaemoniacMeltdown : Components.CastCounter
{
    private Actor? _stackTarget;
    private List<Actor> _spreadTargets = new();

    private static readonly AOEShapeRect _shapeStack = new(50, 3);
    private static readonly AOEShapeRect _shapeSpread = new(50, 2);

    public PandaemoniacMeltdown() : base(ActionID.MakeSpell(AID.PandaemoniacMeltdownStack)) { }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        bool isSpread = _spreadTargets.Contains(actor);
        if (_spreadTargets.Any(t => t != actor && _shapeSpread.Check(actor.Position, module.PrimaryActor.Position, Angle.FromDirection(t.Position - module.PrimaryActor.Position))))
            hints.Add("GTFO from other spreads!");
        if (isSpread && module.Raid.WithoutSlot().Exclude(actor).InShape(_shapeSpread, module.PrimaryActor.Position, Angle.FromDirection(actor.Position - module.PrimaryActor.Position)).Any())
            hints.Add("Aim spread away from raid!");

        if (_stackTarget == actor)
        {
            if (_spreadTargets.InShape(_shapeStack, module.PrimaryActor.Position, Angle.FromDirection(actor.Position - module.PrimaryActor.Position)).Any())
                hints.Add("Aim stack away from spreads!");
        }
        else if (_stackTarget != null)
        {
            bool inStack = _shapeStack.Check(actor.Position, module.PrimaryActor.Position, Angle.FromDirection(_stackTarget.Position - module.PrimaryActor.Position));
            if (isSpread == inStack)
                hints.Add(isSpread ? "GTFO from stack!" : "Stack in line!");
        }
    }

    public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return player == _stackTarget ? PlayerPriority.Interesting : _spreadTargets.Contains(player) ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        bool isSpread = _spreadTargets.Contains(pc);
        if (_stackTarget != null && _stackTarget != pc)
            _shapeStack.Draw(arena, module.PrimaryActor.Position, Angle.FromDirection(_stackTarget.Position - module.PrimaryActor.Position), isSpread ? ArenaColor.AOE : ArenaColor.SafeFromAOE);
        foreach (var t in _spreadTargets.Exclude(pc))
            _shapeStack.Draw(arena, module.PrimaryActor.Position, Angle.FromDirection(t.Position - module.PrimaryActor.Position), ArenaColor.AOE);
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (pc == _stackTarget)
            _shapeStack.Outline(arena, module.PrimaryActor.Position, Angle.FromDirection(pc.Position - module.PrimaryActor.Position), ArenaColor.Safe);
        else if (_spreadTargets.Contains(pc))
            _shapeSpread.Outline(arena, module.PrimaryActor.Position, Angle.FromDirection(pc.Position - module.PrimaryActor.Position), ArenaColor.Danger);
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(module, caster, spell);
        if ((AID)spell.Action.ID == AID.PandaemoniacMeltdownTargetSelect)
            _stackTarget = module.WorldState.Actors.Find(spell.MainTargetID);
    }

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.PandaemoniacMeltdownSpread)
            _spreadTargets.Add(actor);
    }
}
