namespace BossMod.Endwalker.Savage.P4S2Hesperos;

// state related to act 5 (finale) wreath of thorns
class WreathOfThorns5 : BossComponent
{
    private List<ulong> _playersOrder = new();
    private List<Actor> _towersOrder = new();
    private int _castsDone = 0;

    private static readonly float _impulseAOERadius = 5;

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        int order = _playersOrder.IndexOf(actor.InstanceID);
        if (order >= 0)
        {
            hints.Add($"Order: {order + 1}", false);

            if (order >= _castsDone && order < _towersOrder.Count)
            {
                hints.Add("Soak tower!", !actor.Position.InCircle(_towersOrder[order].Position, P4S2.WreathTowerRadius));
            }
        }

        if (_playersOrder.Count < 8)
        {
            hints.Add("Spread!", module.Raid.WithoutSlot().InRadiusExcluding(actor, _impulseAOERadius).Any());
        }
    }

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        hints.Add($"Order: {string.Join(" -> ", _playersOrder.Skip(_castsDone).Select(id => module.WorldState.Actors.Find(id)?.Name ?? "???"))}");
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        int order = _playersOrder.IndexOf(pc.InstanceID);
        if (order >= _castsDone && order < _towersOrder.Count)
            arena.AddCircle(_towersOrder[order].Position, P4S2.WreathTowerRadius, ArenaColor.Safe);

        var pcTetherTarget = module.WorldState.Actors.Find(pc.Tether.Target);
        if (pcTetherTarget != null)
        {
            arena.AddLine(pc.Position, pcTetherTarget.Position, pc.Tether.ID == (uint)TetherID.WreathOfThorns ? ArenaColor.Danger : ArenaColor.Safe);
        }

        if (_playersOrder.Count < 8)
        {
            arena.AddCircle(pc.Position, _impulseAOERadius, ArenaColor.Danger);
            foreach (var player in module.Raid.WithoutSlot().Exclude(pc))
                arena.Actor(player, player.Position.InCircle(pc.Position, _impulseAOERadius) ? ArenaColor.PlayerInteresting : ArenaColor.PlayerGeneric);
        }
    }

    public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
    {
        if (source.OID == (uint)OID.Helper)
            _towersOrder.Add(source);
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FleetingImpulseAOE:
                _playersOrder.Add(spell.MainTargetID);
                break;
            case AID.AkanthaiExplodeTower:
                ++_castsDone;
                break;
        }
    }
}
