namespace BossMod.Dawntrail.Foray.ForkedTower.FT03MarbleDragon;

class AddsTowersPre(BossModule module) : BossComponent(module)
{
    private readonly List<(Actor Tower, bool Imminent)> _towers = [];

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.IceTower)
            _towers.Add((actor, false));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.FrigidDive)
        {
            for (var i = 0; i < _towers.Count; i++)
            {
                if (_towers[i].Tower.Position.InRect(caster.Position, caster.Rotation, 60, 0, 5))
                    _towers.Ref(i).Imminent = true;
            }
        }

        if ((AID)spell.Action.ID == AID.ImitationBlizzardTower)
            _towers.RemoveAll(t => t.Tower.Position.AlmostEqual(spell.LocXZ, 1));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var (t, imm) in _towers)
            Components.GenericTowers.DrawTower(Arena, t.Position, 4, imm);
    }
}

// TODO: per-alliance hints
class AddsTowers(BossModule module) : Components.CastTowers(module, AID.ImitationBlizzardTower, 4, 4, int.MaxValue);

class AddsCross(BossModule module) : Components.GenericAOEs(module)
{
    private Actor? _cross;
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default && _cross is { } c)
            yield return new(ImitationBlizzard.Cross, c.Position, c.Rotation, _activation);
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.CrossPuddle)
            _cross = actor;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.FrigidDive)
            _activation = Module.CastFinishAt(spell, 4.2f);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ImitationBlizzardCross)
            _activation = default;
    }
}
