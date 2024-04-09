namespace BossMod.Stormblood.Ultimate.UCOB;

class P3SeventhUmbralEra : Components.Knockback
{
    private DateTime _activation;

    public P3SeventhUmbralEra() : base(ActionID.MakeSpell(AID.SeventhUmbralEra), true) { }

    public override void Init(BossModule module) => _activation = WorldState.FutureTime(5.3f);

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        yield return new(Module.Bounds.Center, 11, _activation);
    }
}

class P3CalamitousFlame(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.CalamitousFlame));

class P3CalamitousBlaze(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.CalamitousBlaze));
