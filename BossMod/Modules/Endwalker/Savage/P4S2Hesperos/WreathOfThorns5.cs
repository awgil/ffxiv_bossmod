namespace BossMod.Endwalker.Savage.P4S2Hesperos;

// state related to act 5 (finale) wreath of thorns
class WreathOfThorns5(BossModule module) : BossComponent(module)
{
    private readonly List<ulong> _playersOrder = [];
    private readonly List<Actor> _towersOrder = [];
    private int _castsDone;

    private const float _impulseAOERadius = 5;

    public override void AddHints(int slot, Actor actor, TextHints hints)
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
            hints.Add("Spread!", Raid.WithoutSlot().InRadiusExcluding(actor, _impulseAOERadius).Any());
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"Order: {string.Join(" -> ", _playersOrder.Skip(_castsDone).Select(id => WorldState.Actors.Find(id)?.Name ?? "???"))}");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        int order = _playersOrder.IndexOf(pc.InstanceID);
        if (order >= _castsDone && order < _towersOrder.Count)
            Arena.AddCircle(_towersOrder[order].Position, P4S2.WreathTowerRadius, ArenaColor.Safe);

        var pcTetherTarget = WorldState.Actors.Find(pc.Tether.Target);
        if (pcTetherTarget != null)
        {
            Arena.AddLine(pc.Position, pcTetherTarget.Position, pc.Tether.ID == (uint)TetherID.WreathOfThorns ? ArenaColor.Danger : ArenaColor.Safe);
        }

        if (_playersOrder.Count < 8)
        {
            Arena.AddCircle(pc.Position, _impulseAOERadius, ArenaColor.Danger);
            foreach (var player in Raid.WithoutSlot().Exclude(pc))
                Arena.Actor(player, player.Position.InCircle(pc.Position, _impulseAOERadius) ? ArenaColor.PlayerInteresting : ArenaColor.PlayerGeneric);
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (source.OID == (uint)OID.Helper)
            _towersOrder.Add(source);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
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
