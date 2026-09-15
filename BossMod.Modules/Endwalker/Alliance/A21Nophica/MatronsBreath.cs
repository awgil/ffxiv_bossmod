namespace BossMod.Endwalker.Alliance.A21Nophica;

class MatronsBreath(BossModule module) : BossComponent(module)
{
    public int NumCasts { get; private set; }
    private readonly IReadOnlyList<Actor> _blueSafe = module.Enemies(OID.BlueSafeZone);
    private readonly IReadOnlyList<Actor> _goldSafe = module.Enemies(OID.GoldSafeZone);
    private readonly List<Actor> _towers = [];

    private static readonly AOEShapeDonut _shape = new(8, 40); // TODO: verify safe zone radius

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_shape.Check(actor.Position, NextSafeZone))
            hints.Add("Go to correct safe zone!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        _shape.Draw(Arena, NextSafeZone);
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.BlueTower or OID.GoldTower)
            _towers.Add(actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
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
