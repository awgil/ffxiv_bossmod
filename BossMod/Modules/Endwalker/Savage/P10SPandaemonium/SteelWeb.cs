namespace BossMod.Endwalker.Savage.P10SPandaemonium;

class SteelWebStack : Components.UniformStackSpread
{
    private BitMask _forbidden;

    public SteelWebStack() : base(6, 0, 3) { }

    public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.DividingWings)
            _forbidden.Set(module.Raid.FindSlot(tether.Target));
    }

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        switch ((IconID)iconID)
        {
            case IconID.SteelWeb:
                AddStack(actor, module.WorldState.CurrentTime.AddSeconds(6.1f), _forbidden);
                break;
            case IconID.EntanglingWeb:
                _forbidden.Set(module.Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SteelWebAOE)
            Stacks.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
    }
}

class SteelWebTethers : BossComponent
{
    private List<(Actor from, Actor to, uint color)> _webs = new();

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        foreach (var w in _webs)
            arena.AddLine(w.from.Position, w.to.Position, w.color);
    }

    public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID is TetherID.Web or TetherID.WebFail && module.WorldState.Actors.Find(tether.Target) is var target && target != null)
            _webs.Add((source, target, (TetherID)tether.ID == TetherID.Web ? ArenaColor.Danger : ArenaColor.Enemy));
    }

    public override void OnUntethered(BossModule module, Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID is TetherID.Web or TetherID.WebFail)
            _webs.RemoveAll(w => w.from == source);
    }
}
