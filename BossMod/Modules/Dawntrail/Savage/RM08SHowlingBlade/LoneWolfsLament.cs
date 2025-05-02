namespace BossMod.Dawntrail.Savage.RM08SHowlingBlade;

class LoneWolfTethers(BossModule module) : BossComponent(module)
{
    private readonly List<(Actor, Actor)> _close = [];
    private readonly List<(Actor, Actor)> _distant = [];

    public record struct Assignment(Role Partner, bool Close);

    public readonly Assignment[] Assignments = new Assignment[PartyState.MaxPartySize];

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        switch ((TetherID)tether.ID)
        {
            case TetherID.LamentClose:
                Assign(source, WorldState.Actors.Find(tether.Target)!, true);
                break;
            case TetherID.LamentDistant:
                Assign(source, WorldState.Actors.Find(tether.Target)!, false);
                break;
        }
    }

    private void Assign(Actor a, Actor b, bool close)
    {
        Assignments[Raid.FindSlot(a.InstanceID)] = new(b.Role, close);
        Assignments[Raid.FindSlot(b.InstanceID)] = new(a.Role, close);
        if (close)
            _close.Add((a, b));
        else
            _distant.Add((a, b));
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var (a, b) in _close)
            Arena.AddLine(a.Position, b.Position, ArenaColor.Safe);
        foreach (var (a, b) in _distant)
            Arena.AddLine(a.Position, b.Position, 0xFFFF0000);
    }
}

class LoneWolfTowers(BossModule module) : Components.GenericTowers(module)
{
    private readonly LoneWolfTethers _tethers = module.FindComponent<LoneWolfTethers>()!;
    private readonly RM08SHowlingBladeConfig _config = Service.Config.Get<RM08SHowlingBladeConfig>();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var count = (AID)spell.Action.ID switch
        {
            AID.ProwlingGaleOneTower => 1,
            AID.ProwlingGaleTwoTower => 2,
            AID.ProwlingGaleThreeTower => 3,
            _ => 0
        };
        if (count > 0)
        {
            var permitted = new BitMask();

            foreach (var (slot, player) in Raid.WithSlot())
            {
                var assignment = _tethers.Assignments[slot];

                if (_config.TowerHints == RM08SHowlingBladeConfig.LamentTowerStrategy.Rinon)
                {
                    if (count == 3)
                        permitted[slot] = player.Role == Role.Healer || assignment.Partner == Role.Healer && assignment.Close;

                    else if (count == 2)
                        permitted[slot] = (player.Role == Role.Tank || assignment.Partner == Role.Tank) && assignment.Close;

                    else
                    {
                        // north tower
                        if (caster.Position.Z < 100)
                            permitted[slot] = assignment.Partner == Role.Healer && !assignment.Close;
                        // east tower
                        else if (caster.Position.X > 100)
                            permitted[slot] = assignment.Partner == Role.Tank && !assignment.Close;
                        // west tower
                        else
                            permitted[slot] = player.Role == Role.Tank && !assignment.Close;
                    }
                }
                else
                {
                    permitted[slot] = true;
                }
            }

            Towers.Add(new(caster.Position, 2, count, count, ~permitted, Module.CastFinishAt(spell)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ProwlingGaleOneTower or AID.ProwlingGaleTwoTower or AID.ProwlingGaleThreeTower)
        {
            NumCasts++;
            Towers.RemoveAll(t => t.Position.AlmostEqual(caster.Position, 1));
        }
    }
}
