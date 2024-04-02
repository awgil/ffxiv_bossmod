namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS1TrinitySeeker;

class MercifulMoon : Components.GenericGaze
{
    private Eye? _eye;

    public MercifulMoon() : base(ActionID.MakeSpell(AID.MercifulMoon)) { }

    public override IEnumerable<Eye> ActiveEyes(BossModule module, int slot, Actor actor) => Utils.ZeroOrOne(_eye);

    public override void Update(BossModule module)
    {
        if (_eye == null && module.Enemies(OID.AetherialOrb).FirstOrDefault() is var orb && orb != null)
            _eye = new(orb.Position, module.WorldState.CurrentTime.AddSeconds(5.8f)); // time from spawn to cast
    }
}
