namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS3Dahu;

class SpitFlame(BossModule module) : Components.UniformStackSpread(module, 0, 4, alwaysShowSpreads: true, raidwideOnResolve: false)
{
    private readonly Actor?[] _targets = [null, null, null, null];
    private readonly IReadOnlyList<Actor> _adds = module.Enemies(OID.Marchosias);

    public override void Update()
    {
        Spreads.RemoveAll(s => s.Target.IsDead); // if target dies after being marked, cast will be skipped
        base.Update();
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (Array.IndexOf(_targets, actor) is var order && order >= 0)
        {
            hints.Add($"Order: {order + 1}", false);
            if (!_adds.Any(add => add.Position.InCircle(actor.Position, SpreadRadius)))
                hints.Add("Hit at least one add!");
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(_adds, ArenaColor.Object, true);
        base.DrawArenaForeground(pcSlot, pc);
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        var order = (IconID)iconID switch
        {
            IconID.SpitFlame1 => 1,
            IconID.SpitFlame2 => 2,
            IconID.SpitFlame3 => 3,
            IconID.SpitFlame4 => 4,
            _ => 0
        };
        if (order > 0)
        {
            AddSpread(actor);
            _targets[order - 1] = actor;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SpitFlameAOE)
            Spreads.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
    }
}
