namespace BossMod.Endwalker.Savage.P10SPandaemonium;

class EntanglingWebAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EntanglingWebAOE, 5);

class EntanglingWebHints(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _pillars = module.Enemies((uint)OID.Pillar);
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
        Arena.Actors(_pillars, Colors.Object, true);
        for (var i = 0; i < _targets.Count; ++i)
            Arena.ZoneCircleOutline(_targets[i].Position, _radius, Colors.Danger);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.EntanglingWebAOE && _targets.Count > 0)
            _targets.RemoveAt(0);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.EntanglingWeb)
            _targets.Add(actor);
    }
}
