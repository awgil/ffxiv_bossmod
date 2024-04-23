namespace BossMod.Endwalker.Savage.P10SPandaemonium;

class SteelWebStack(BossModule module) : Components.UniformStackSpread(module, 6, 0, 3)
{
    private BitMask _forbidden;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.DividingWings)
            _forbidden.Set(Raid.FindSlot(tether.Target));
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        switch ((IconID)iconID)
        {
            case IconID.SteelWeb:
                AddStack(actor, WorldState.FutureTime(6.1f), _forbidden);
                break;
            case IconID.EntanglingWeb:
                _forbidden.Set(Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SteelWebAOE)
            Stacks.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
    }
}

class SteelWebTethers(BossModule module) : BossComponent(module)
{
    private readonly List<(Actor from, Actor to, uint color)> _webs = [];

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var w in _webs)
            Arena.AddLine(w.from.Position, w.to.Position, w.color);
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID is TetherID.Web or TetherID.WebFail && WorldState.Actors.Find(tether.Target) is var target && target != null)
            _webs.Add((source, target, (TetherID)tether.ID == TetherID.Web ? ArenaColor.Danger : ArenaColor.Enemy));
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID is TetherID.Web or TetherID.WebFail)
            _webs.RemoveAll(w => w.from == source);
    }
}
