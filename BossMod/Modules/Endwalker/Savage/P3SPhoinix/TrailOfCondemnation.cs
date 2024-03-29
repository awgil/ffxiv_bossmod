namespace BossMod.Endwalker.Savage.P3SPhoinix;

// state related to trail of condemnation mechanic
class TrailOfCondemnation : BossComponent
{
    public bool Done { get; private set; } = false;
    private bool _isCenter;

    private static readonly float _halfWidth = 7.5f;
    private static readonly float _sidesOffset = 12.5f;
    private static readonly float _aoeRadius = 6;

    public override void Init(BossModule module)
    {
        _isCenter = module.PrimaryActor.CastInfo?.IsSpell(AID.TrailOfCondemnationCenter) ?? false;
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (module.PrimaryActor.Position == module.Bounds.Center)
            return;

        var dir = (module.Bounds.Center - module.PrimaryActor.Position).Normalized();
        if (_isCenter)
        {
            if (actor.Position.InRect(module.PrimaryActor.Position, dir, 2 * module.Bounds.HalfSize, 0, _halfWidth))
            {
                hints.Add("GTFO from aoe!");
            }
            if (module.Raid.WithoutSlot().InRadiusExcluding(actor, _aoeRadius).Any())
            {
                hints.Add("Spread!");
            }
        }
        else
        {
            var offset = _sidesOffset * dir.OrthoR();
            if (actor.Position.InRect(module.PrimaryActor.Position + offset, dir, 2 * module.Bounds.HalfSize, 0, _halfWidth) ||
                actor.Position.InRect(module.PrimaryActor.Position - offset, dir, 2 * module.Bounds.HalfSize, 0, _halfWidth))
            {
                hints.Add("GTFO from aoe!");
            }
            // note: sparks either target all tanks & healers or all dds - so correct pairings are always dd+tank/healer
            int numStacked = 0;
            bool goodPair = false;
            foreach (var pair in module.Raid.WithoutSlot().InRadiusExcluding(actor, _aoeRadius))
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

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (module.PrimaryActor.Position == module.Bounds.Center)
            return;

        var dir = (module.Bounds.Center - module.PrimaryActor.Position).Normalized();
        if (_isCenter)
        {
            arena.ZoneRect(module.PrimaryActor.Position, dir, 2 * module.Bounds.HalfSize, 0, _halfWidth, ArenaColor.AOE);
        }
        else
        {
            var offset = _sidesOffset * dir.OrthoR();
            arena.ZoneRect(module.PrimaryActor.Position + offset, dir, 2 * module.Bounds.HalfSize, 0, _halfWidth, ArenaColor.AOE);
            arena.ZoneRect(module.PrimaryActor.Position - offset, dir, 2 * module.Bounds.HalfSize, 0, _halfWidth, ArenaColor.AOE);
        }
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        // draw all raid members, to simplify positioning
        foreach (var player in module.Raid.WithoutSlot().Exclude(pc))
        {
            bool inRange = player.Position.InCircle(pc.Position, _aoeRadius);
            arena.Actor(player, inRange ? ArenaColor.PlayerInteresting : ArenaColor.PlayerGeneric);
        }

        // draw circle around pc
        arena.AddCircle(pc.Position, _aoeRadius, ArenaColor.Danger);
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.FlareOfCondemnation or AID.SparksOfCondemnation)
            Done = true;
    }
}
