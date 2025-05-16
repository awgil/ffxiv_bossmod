namespace BossMod.Components;

// generic component for tankbuster at tethered targets; tanks are supposed to intercept tethers and gtfo from the raid
public class TankbusterTether(BossModule module, Enum aid, uint tetherID, float radius) : CastCounter(module, aid)
{
    public uint TID { get; init; } = tetherID;
    public float Radius { get; init; } = radius;
    private readonly List<(Actor Player, Actor Enemy)> _tethers = [];
    private BitMask _tetheredPlayers;
    private BitMask _inAnyAOE; // players hit by aoe, excluding selves

    public bool Active => _tetheredPlayers.Any();

    public override void Update()
    {
        _inAnyAOE = new();
        foreach (int slot in _tetheredPlayers.SetBits())
        {
            var target = Raid[slot];
            if (target != null)
                _inAnyAOE |= Raid.WithSlot().InRadiusExcluding(target, Radius).Mask();
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!Active)
            return;

        if (actor.Role == Role.Tank)
        {
            if (!_tetheredPlayers[slot])
            {
                hints.Add("Grab the tether!");
            }
            else if (Raid.WithoutSlot().InRadiusExcluding(actor, Radius).Any())
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

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        if (_tetheredPlayers[playerSlot])
            return PlayerPriority.Danger;

        // for tanks, other players are interesting, since tank should not clip them
        if (pc.Role == Role.Tank)
            return _inAnyAOE[playerSlot] ? PlayerPriority.Interesting : PlayerPriority.Normal;

        // for non-tanks, other players are irrelevant
        return PlayerPriority.Irrelevant;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        // show tethered targets with circles
        foreach (var side in _tethers)
        {
            if (Arena.Config.ShowOutlinesAndShadows)
                Arena.AddLine(side.Enemy.Position, side.Player.Position, 0xFF000000, 2);
            Arena.AddLine(side.Enemy.Position, side.Player.Position, side.Player.Role == Role.Tank ? ArenaColor.Safe : ArenaColor.Danger);
            if (Arena.Config.ShowOutlinesAndShadows)
                Arena.AddCircle(side.Player.Position, Radius, 0xFF000000, 2);
            Arena.AddCircle(side.Player.Position, Radius, ArenaColor.Danger);
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        var sides = DetermineTetherSides(source, tether);
        if (sides != null)
        {
            _tethers.Add((sides.Value.Player, sides.Value.Enemy));
            _tetheredPlayers.Set(sides.Value.PlayerSlot);
        }
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        var sides = DetermineTetherSides(source, tether);
        if (sides != null)
        {
            _tethers.Remove((sides.Value.Player, sides.Value.Enemy));
            _tetheredPlayers.Clear(sides.Value.PlayerSlot);
        }
    }

    // we support both player->enemy and enemy->player tethers
    private (int PlayerSlot, Actor Player, Actor Enemy)? DetermineTetherSides(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID != TID)
            return null;

        var target = WorldState.Actors.Find(tether.Target);
        if (target == null)
            return null;

        var (player, enemy) = source.Type == ActorType.Player ? (source, target) : (target, source);
        if (player.Type != ActorType.Player || enemy.Type == ActorType.Player)
        {
            ReportError($"Unexpected tether pair: {source.InstanceID:X} -> {target.InstanceID:X}");
            return null;
        }

        if (!Raid.TryFindSlot(player, out var playerSlot))
        {
            ReportError($"Non-party-member player is tethered: {source.InstanceID:X} -> {target.InstanceID:X}");
            return null;
        }

        return (playerSlot, player, enemy);
    }
}
