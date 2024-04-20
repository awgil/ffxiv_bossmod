namespace BossMod.Endwalker.Savage.P10SPandaemonium;

class EntanglingWebAOE(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.EntanglingWebAOE), 5);

class EntanglingWebHints(BossModule module) : BossComponent(module)
{
    private readonly IReadOnlyList<Actor> _pillars = module.Enemies(OID.Pillar);
    private readonly List<Actor> _targets = [];

    private const float _radius = 5;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_targets.Contains(actor))
        {
            var overlapCount = _targets.InRadiusExcluding(actor, _radius * 2).Count();
            if (overlapCount < 2 && !_pillars.InRadius(actor.Position, _radius).Any())
                hints.Add("Stand near pillar!");
            if (overlapCount == 0)
                hints.Add("Overlap with other web!");
        }
        else if (_targets.InRadius(actor.Position, _radius).Any())
        {
            hints.Add("GTFO from webs!");
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(_pillars, ArenaColor.Object, true);
        foreach (var t in _targets)
            Arena.AddCircle(t.Position, _radius, ArenaColor.Danger);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.EntanglingWebAOE && _targets.Count > 0)
            _targets.RemoveAt(0);
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.EntanglingWeb)
            _targets.Add(actor);
    }
}
