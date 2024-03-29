namespace BossMod.Endwalker.Alliance.A21Nophica;

class MatronsBreath : BossComponent
{
    public int NumCasts { get; private set; }
    private IReadOnlyList<Actor> _blueSafe = ActorEnumeration.EmptyList;
    private IReadOnlyList<Actor> _goldSafe = ActorEnumeration.EmptyList;
    private List<Actor> _towers = new();

    private static readonly AOEShapeDonut _shape = new(8, 40); // TODO: verify safe zone radius

    public override void Init(BossModule module)
    {
        _blueSafe = module.Enemies(OID.BlueSafeZone);
        _goldSafe = module.Enemies(OID.GoldSafeZone);
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (_shape.Check(actor.Position, NextSafeZone))
            hints.Add("Go to correct safe zone!");
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        _shape.Draw(arena, NextSafeZone);
    }

    public override void OnActorCreated(BossModule module, Actor actor)
    {
        if ((OID)actor.OID is OID.BlueTower or OID.GoldTower)
            _towers.Add(actor);
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Blueblossoms or AID.Giltblossoms)
        {
            ++NumCasts;
            if (_towers.Count > 0)
                _towers.RemoveAt(0);
        }
    }

    private Actor? NextSafeZone => _towers.Count == 0 ? null : (OID)_towers[0].OID == OID.BlueTower ? _blueSafe.FirstOrDefault() : _goldSafe.FirstOrDefault();
}
