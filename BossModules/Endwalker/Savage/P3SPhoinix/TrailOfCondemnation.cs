namespace BossMod.Endwalker.Savage.P3SPhoinix;

// state related to trail of condemnation mechanic
class TrailOfCondemnation(BossModule module) : BossComponent(module)
{
    public bool Done { get; private set; }
    private readonly bool _isCenter = module.PrimaryActor.CastInfo?.IsSpell(AID.TrailOfCondemnationCenter) ?? false;

    private const float _halfWidth = 7.5f;
    private const float _sidesOffset = 12.5f;
    private const float _aoeRadius = 6;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Module.PrimaryActor.Position == Module.Center)
            return;

        var dir = (Module.Center - Module.PrimaryActor.Position).Normalized();
        if (_isCenter)
        {
            if (actor.Position.InRect(Module.PrimaryActor.Position, dir, 2 * Module.Bounds.Radius, 0, _halfWidth))
            {
                hints.Add("GTFO from aoe!");
            }
            if (Raid.WithoutSlot().InRadiusExcluding(actor, _aoeRadius).Any())
            {
                hints.Add("Spread!");
            }
        }
        else
        {
            var offset = _sidesOffset * dir.OrthoR();
            if (actor.Position.InRect(Module.PrimaryActor.Position + offset, dir, 2 * Module.Bounds.Radius, 0, _halfWidth) ||
                actor.Position.InRect(Module.PrimaryActor.Position - offset, dir, 2 * Module.Bounds.Radius, 0, _halfWidth))
            {
                hints.Add("GTFO from aoe!");
            }
            // note: sparks either target all tanks & healers or all dds - so correct pairings are always dd+tank/healer
            int numStacked = 0;
            bool goodPair = false;
            foreach (var pair in Raid.WithoutSlot().InRadiusExcluding(actor, _aoeRadius))
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

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Module.PrimaryActor.Position == Module.Center)
            return;

        var dir = (Module.Center - Module.PrimaryActor.Position).Normalized();
        if (_isCenter)
        {
            Arena.ZoneRect(Module.PrimaryActor.Position, dir, 2 * Module.Bounds.Radius, 0, _halfWidth, ArenaColor.AOE);
        }
        else
        {
            var offset = _sidesOffset * dir.OrthoR();
            Arena.ZoneRect(Module.PrimaryActor.Position + offset, dir, 2 * Module.Bounds.Radius, 0, _halfWidth, ArenaColor.AOE);
            Arena.ZoneRect(Module.PrimaryActor.Position - offset, dir, 2 * Module.Bounds.Radius, 0, _halfWidth, ArenaColor.AOE);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        // draw all raid members, to simplify positioning
        foreach (var player in Raid.WithoutSlot().Exclude(pc))
        {
            bool inRange = player.Position.InCircle(pc.Position, _aoeRadius);
            Arena.Actor(player, inRange ? ArenaColor.PlayerInteresting : ArenaColor.PlayerGeneric);
        }

        // draw circle around pc
        Arena.AddCircle(pc.Position, _aoeRadius, ArenaColor.Danger);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.FlareOfCondemnation or AID.SparksOfCondemnation)
            Done = true;
    }
}
