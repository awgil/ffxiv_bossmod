namespace BossMod.Components;

// generic component for tethers that need to be intercepted eg. to prevent a boss from gaining buffs - (Ported From BMR)
public class InterceptTether(BossModule module, ActionID aid, uint tetherID) : CastCounter(module, aid)
{
    public uint TID { get; init; } = tetherID;
    private readonly List<(Actor Player, Actor Enemy)> _tethers = [];
    private BitMask _tetheredPlayers;
    private const string hint = "Grab the tether!";
    public bool Active => _tethers.Count != 0;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!Active)
            return;
        if (!_tetheredPlayers[slot])
        {
            hints.Add(hint);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var side in _tethers)
            Arena.AddLine(side.Enemy.Position, side.Player.Position, side.Player.Type is ActorType.Player or ActorType.DutySupport ? ArenaColor.Safe : 0);
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

        var (player, enemy) = source.Type is ActorType.Player or ActorType.DutySupport ? (source, target) : (target, source);
        var playerSlot = Raid.FindSlot(player.InstanceID);
        return (playerSlot, player, enemy);
    }
}
