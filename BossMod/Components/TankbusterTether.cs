namespace BossMod.Components;

// generic component for tankbuster at tethered targets; tanks are supposed to intercept tethers and gtfo from the raid
public class TankbusterTether : CastCounter
{
    public uint TID { get; private init; }
    public float Radius { get; private init; }
    private List<(Actor Player, Actor Enemy)> _tethers = new();
    private BitMask _tetheredPlayers;
    private BitMask _inAnyAOE; // players hit by aoe, excluding selves

    public bool Active => _tetheredPlayers.Any();

    public TankbusterTether(ActionID aid, uint tetherID, float radius) : base(aid)
    {
        TID = tetherID;
        Radius = radius;
    }

    public override void Update(BossModule module)
    {
        _inAnyAOE = new();
        foreach (int slot in _tetheredPlayers.SetBits())
        {
            var target = module.Raid[slot];
            if (target != null)
                _inAnyAOE |= module.Raid.WithSlot().InRadiusExcluding(target, Radius).Mask();
        }
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (!Active)
            return;

        if (actor.Role == Role.Tank)
        {
            if (!_tetheredPlayers[slot])
            {
                hints.Add("Grab the tether!");
            }
            else if (module.Raid.WithoutSlot().InRadiusExcluding(actor, Radius).Any())
            {
                hints.Add("GTFO from raid!");
            }
        }
        else
        {
            if (_tetheredPlayers[slot])
            {
                hints.Add("Hit by tankbuster");
            }
            if (_inAnyAOE[slot])
            {
                hints.Add("GTFO from tanks!");
            }
        }
    }

    public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        if (_tetheredPlayers[playerSlot])
            return PlayerPriority.Danger;

        // for tanks, other players are interesting, since tank should not clip them
        if (pc.Role == Role.Tank)
            return _inAnyAOE[playerSlot] ? PlayerPriority.Interesting : PlayerPriority.Normal;

        // for non-tanks, other players are irrelevant
        return PlayerPriority.Irrelevant;
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        // show tethered targets with circles
        foreach (var side in _tethers)
        {
            if (arena.Config.ShowOutlinesAndShadows)
                arena.AddLine(side.Enemy.Position, side.Player.Position, 0xFF000000, 2);
            arena.AddLine(side.Enemy.Position, side.Player.Position, side.Player.Role == Role.Tank ? ArenaColor.Safe : ArenaColor.Danger);
            if (arena.Config.ShowOutlinesAndShadows)
                arena.AddCircle(side.Player.Position, Radius, 0xFF000000, 2);
            arena.AddCircle(side.Player.Position, Radius, ArenaColor.Danger);
        }
    }

    public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
    {
        var sides = DetermineTetherSides(module, source, tether);
        if (sides != null)
        {
            _tethers.Add((sides.Value.Player, sides.Value.Enemy));
            _tetheredPlayers.Set(sides.Value.PlayerSlot);
        }
    }

    public override void OnUntethered(BossModule module, Actor source, ActorTetherInfo tether)
    {
        var sides = DetermineTetherSides(module, source, tether);
        if (sides != null)
        {
            _tethers.Remove((sides.Value.Player, sides.Value.Enemy));
            _tetheredPlayers.Clear(sides.Value.PlayerSlot);
        }
    }

    // we support both player->enemy and enemy->player tethers
    private (int PlayerSlot, Actor Player, Actor Enemy)? DetermineTetherSides(BossModule module, Actor source, ActorTetherInfo tether)
    {
        if (tether.ID != TID)
            return null;

        var target = module.WorldState.Actors.Find(tether.Target);
        if (target == null)
            return null;

        var (player, enemy) = source.Type == ActorType.Player ? (source, target) : (target, source);
        if (player.Type != ActorType.Player || enemy.Type == ActorType.Player)
        {
            module.ReportError(this, $"Unexpected tether pair: {source.InstanceID:X} -> {target.InstanceID:X}");
            return null;
        }

        var playerSlot = module.Raid.FindSlot(player.InstanceID);
        if (playerSlot < 0)
        {
            module.ReportError(this, $"Non-party-member player is tethered: {source.InstanceID:X} -> {target.InstanceID:X}");
            return null;
        }

        return (playerSlot, player, enemy);
    }
}
