namespace BossMod.Endwalker.Savage.P3SPhoinix;

// state related to trail of condemnation mechanic
class TrailOfCondemnation(BossModule module) : BossComponent(module)
{
    public bool Done { get; private set; }
    private readonly bool _isCenter = module.PrimaryActor.CastInfo?.IsSpell(AID.TrailOfCondemnationCenter) ?? false;
    private const float _aoeRadius = 6;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Module.PrimaryActor.Position == Arena.Center)
            return;
        if (_isCenter)
        {
            if (Raid.WithoutSlot(false, true, true).InRadiusExcluding(actor, _aoeRadius).Any())
            {
                hints.Add("Spread!");
            }
        }
        else
        {
            // note: sparks either target all tanks & healers or all dds - so correct pairings are always dd+tank/healer
            var numStacked = 0;
            var goodPair = false;
            foreach (var pair in Raid.WithoutSlot(false, true, true).InRadiusExcluding(actor, _aoeRadius))
            {
                ++numStacked;
                goodPair = (actor.Role == Role.Tank || actor.Role == Role.Healer) != (pair.Role == Role.Tank || pair.Role == Role.Healer);
            }
            if (numStacked != 1)
            {
                hints.Add("Stack in pairs!");
            }
            else if (!goodPair)
            {
                hints.Add("Incorrect pairing!");
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        // draw all raid members, to simplify positioning
        foreach (var player in Raid.WithoutSlot(false, true, true).Exclude(pc))
        {
            var inRange = player.Position.InCircle(pc.Position, _aoeRadius);
            Arena.Actor(player, inRange ? Colors.PlayerInteresting : Colors.PlayerGeneric);
        }

        // draw circle around pc
        Arena.ZoneCircleOutline(pc.Position, _aoeRadius, Colors.Danger);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.FlareOfCondemnation or AID.SparksOfCondemnation)
            Done = true;
    }
}
