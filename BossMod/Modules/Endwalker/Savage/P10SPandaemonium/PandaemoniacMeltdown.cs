namespace BossMod.Endwalker.Savage.P10SPandaemonium;

// TODO: generalize (line stack/spread)
class PandaemoniacMeltdown(BossModule module) : Components.CastCounter(module, AID.PandaemoniacMeltdownStack)
{
    private Actor? _stackTarget;
    private readonly List<Actor> _spreadTargets = [];

    private static readonly AOEShapeRect _shapeStack = new(50, 3);
    private static readonly AOEShapeRect _shapeSpread = new(50, 2);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        bool isSpread = _spreadTargets.Contains(actor);
        if (_spreadTargets.Any(t => t != actor && _shapeSpread.Check(actor.Position, Module.PrimaryActor.Position, Angle.FromDirection(t.Position - Module.PrimaryActor.Position))))
            hints.Add("GTFO from other spreads!");
        if (isSpread && Raid.WithoutSlot().Exclude(actor).InShape(_shapeSpread, Module.PrimaryActor.Position, Angle.FromDirection(actor.Position - Module.PrimaryActor.Position)).Any())
            hints.Add("Aim spread away from raid!");

        if (_stackTarget == actor)
        {
            if (_spreadTargets.InShape(_shapeStack, Module.PrimaryActor.Position, Angle.FromDirection(actor.Position - Module.PrimaryActor.Position)).Any())
                hints.Add("Aim stack away from spreads!");
        }
        else if (_stackTarget != null)
        {
            bool inStack = _shapeStack.Check(actor.Position, Module.PrimaryActor.Position, Angle.FromDirection(_stackTarget.Position - Module.PrimaryActor.Position));
            if (isSpread == inStack)
                hints.Add(isSpread ? "GTFO from stack!" : "Stack in line!");
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return player == _stackTarget ? PlayerPriority.Interesting : _spreadTargets.Contains(player) ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        bool isSpread = _spreadTargets.Contains(pc);
        if (_stackTarget != null && _stackTarget != pc)
            _shapeStack.Draw(Arena, Module.PrimaryActor.Position, Angle.FromDirection(_stackTarget.Position - Module.PrimaryActor.Position), isSpread ? ArenaColor.AOE : ArenaColor.SafeFromAOE);
        foreach (var t in _spreadTargets.Exclude(pc))
            _shapeStack.Draw(Arena, Module.PrimaryActor.Position, Angle.FromDirection(t.Position - Module.PrimaryActor.Position), ArenaColor.AOE);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (pc == _stackTarget)
            _shapeStack.Outline(Arena, Module.PrimaryActor.Position, Angle.FromDirection(pc.Position - Module.PrimaryActor.Position), ArenaColor.Safe);
        else if (_spreadTargets.Contains(pc))
            _shapeSpread.Outline(Arena, Module.PrimaryActor.Position, Angle.FromDirection(pc.Position - Module.PrimaryActor.Position), ArenaColor.Danger);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if ((AID)spell.Action.ID == AID.PandaemoniacMeltdownTargetSelect)
            _stackTarget = WorldState.Actors.Find(spell.MainTargetID);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.PandaemoniacMeltdownSpread)
            _spreadTargets.Add(actor);
    }
}
