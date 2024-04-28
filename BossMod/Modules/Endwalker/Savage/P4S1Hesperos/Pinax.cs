namespace BossMod.Endwalker.Savage.P4S1Hesperos;

// state related to pinax mechanics
class Pinax(BossModule module) : BossComponent(module)
{
    private enum Order { Unknown, LUWU, WULU, LFWA, LAWF, WFLA, WALF }

    public int NumFinished { get; private set; }
    private Order _order;
    private Actor? _acid;
    private Actor? _fire;
    private Actor? _water;
    private Actor? _lighting;

    private const float _acidAOERadius = 5;
    private const float _fireAOERadius = 6;
    private const float _knockbackRadius = 13;
    private const float _lightingSafeDistance = 16; // linear falloff until 16, then constant (not sure whether it is true distance-based or max-coord-based)

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_acid != null)
        {
            if (actor.Position.InRect(_acid.Position, new WDir(1, 0), 10, 10, 10))
            {
                hints.Add("GTFO from acid square!");
            }
            hints.Add("Spread!", Raid.WithoutSlot().InRadiusExcluding(actor, _acidAOERadius).Any());
        }
        if (_fire != null)
        {
            if (actor.Position.InRect(_fire.Position, new WDir(1, 0), 10, 10, 10))
            {
                hints.Add("GTFO from fire square!");
            }
            hints.Add("Stack in fours!", Raid.WithoutSlot().Where(x => x.Role == Role.Healer).InRadius(actor.Position, _fireAOERadius).Count() != 1);
        }
        if (_water != null)
        {
            if (actor.Position.InRect(_water.Position, new WDir(1, 0), 10, 10, 10))
            {
                hints.Add("GTFO from water square!");
            }
            if (!Module.InBounds(Components.Knockback.AwayFromSource(actor.Position, Module.Center, _knockbackRadius)))
            {
                hints.Add("About to be knocked into wall!");
            }
        }
        if (_lighting != null)
        {
            if (actor.Position.InRect(_lighting.Position, new WDir(1, 0), 10, 10, 10))
            {
                hints.Add("GTFO from lighting square!");
            }
            hints.Add("GTFO from center!", actor.Position.InRect(Module.Center, new WDir(1, 0), _lightingSafeDistance, _lightingSafeDistance, _lightingSafeDistance));
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        string order = _order switch
        {
            Order.LUWU => "Lightning - ??? - Water - ???",
            Order.WULU => "Water - ??? - Lightning - ???",
            Order.LFWA => "Lightning - Fire - Water - Acid",
            Order.LAWF => "Lightning - Acid - Water - Fire",
            Order.WFLA => "Water - Fire - Lightning - Acid",
            Order.WALF => "Water - Acid - Lightning - Fire",
            _ => "???"
        };
        hints.Add($"Pinax order: {order}");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_acid != null)
        {
            Arena.ZoneRect(_acid.Position, new WDir(1, 0), 10, 10, 10, ArenaColor.AOE);
        }
        if (_fire != null)
        {
            Arena.ZoneRect(_fire.Position, new WDir(1, 0), 10, 10, 10, ArenaColor.AOE);
        }
        if (_water != null)
        {
            Arena.ZoneRect(_water.Position, new WDir(1, 0), 10, 10, 10, ArenaColor.AOE);
        }
        if (_lighting != null)
        {
            Arena.ZoneRect(_lighting.Position, new WDir(1, 0), 10, 10, 10, ArenaColor.AOE);
            Arena.ZoneRect(Module.Center, new WDir(1, 0), _lightingSafeDistance, _lightingSafeDistance, _lightingSafeDistance, ArenaColor.AOE);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_acid != null)
        {
            Arena.AddCircle(pc.Position, _acidAOERadius, ArenaColor.Danger);
            foreach (var player in Raid.WithoutSlot().Exclude(pc))
                Arena.Actor(player, player.Position.InCircle(pc.Position, _acidAOERadius) ? ArenaColor.PlayerInteresting : ArenaColor.PlayerGeneric);
        }
        if (_fire != null)
        {
            foreach (var player in Raid.WithoutSlot())
            {
                if (player.Role == Role.Healer)
                {
                    Arena.Actor(player, ArenaColor.Danger);
                    Arena.AddCircle(player.Position, _fireAOERadius, ArenaColor.Danger);
                }
                else
                {
                    Arena.Actor(player, ArenaColor.PlayerGeneric);
                }
            }
        }
        if (_water != null)
        {
            var adjPos = Components.Knockback.AwayFromSource(pc.Position, Module.Center, _knockbackRadius);
            if (adjPos != pc.Position)
            {
                Arena.AddLine(pc.Position, adjPos, ArenaColor.Danger);
                Arena.Actor(adjPos, pc.Rotation, ArenaColor.Danger);
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.PinaxAcid:
                _acid = caster;
                if (_order == Order.WULU)
                    _order = Order.WALF;
                else if (_order == Order.LUWU)
                    _order = Order.LAWF;
                break;
            case AID.PinaxLava:
                _fire = caster;
                if (_order == Order.WULU)
                    _order = Order.WFLA;
                else if (_order == Order.LUWU)
                    _order = Order.LFWA;
                break;
            case AID.PinaxWell:
                _water = caster;
                if (_order == Order.Unknown)
                    _order = Order.WULU;
                break;
            case AID.PinaxLevinstrike:
                _lighting = caster;
                if (_order == Order.Unknown)
                    _order = Order.LUWU;
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.PinaxAcid:
                _acid = null;
                ++NumFinished;
                break;
            case AID.PinaxLava:
                _fire = null;
                ++NumFinished;
                break;
            case AID.PinaxWell:
                _water = null;
                ++NumFinished;
                break;
            case AID.PinaxLevinstrike:
                _lighting = null;
                ++NumFinished;
                break;
        }
    }
}
