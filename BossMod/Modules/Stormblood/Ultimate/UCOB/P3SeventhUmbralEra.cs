namespace BossMod.Stormblood.Ultimate.UCOB;

class P3SeventhUmbralEra : Components.Knockback
{
    private DateTime _activation;

    public P3SeventhUmbralEra() : base(ActionID.MakeSpell(AID.SeventhUmbralEra), true) { }

    public override void Init(BossModule module) => _activation = module.WorldState.CurrentTime.AddSeconds(5.3f);

    public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
    {
        yield return new(module.Bounds.Center, 11, _activation);
    }
}

class P3CalamitousFlame : Components.CastCounter
{
    public P3CalamitousFlame() : base(ActionID.MakeSpell(AID.CalamitousFlame)) { }
}

class P3CalamitousBlaze : Components.CastCounter
{
    public P3CalamitousBlaze() : base(ActionID.MakeSpell(AID.CalamitousBlaze)) { }
}
