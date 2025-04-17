namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS1TrinitySeeker;

class MercifulMoon(BossModule module) : Components.GenericGaze(module, AID.MercifulMoon)
{
    private Eye? _eye;

    public override IEnumerable<Eye> ActiveEyes(int slot, Actor actor) => Utils.ZeroOrOne(_eye);

    public override void Update()
    {
        if (_eye == null && Module.Enemies(OID.AetherialOrb).FirstOrDefault() is var orb && orb != null)
            _eye = new(orb.Position, WorldState.FutureTime(5.8f)); // time from spawn to cast
    }
}
