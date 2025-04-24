namespace BossMod.Dawntrail.Extreme.Ex2ZoraalJa;

class DrumOfVollokPlatforms(BossModule module) : BossComponent(module)
{
    public bool Active;

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index != 11)
            return;

        switch (state)
        {
            case 0x00800040:
                Module.Arena.Bounds = Ex2ZoraalJa.NWPlatformBounds;
                Module.Arena.Center += 15 * 135.Degrees().ToDirection();
                Active = true;
                break;
            case 0x02000100:
                Module.Arena.Bounds = Ex2ZoraalJa.NEPlatformBounds;
                Module.Arena.Center += 15 * (-135).Degrees().ToDirection();
                Active = true;
                break;
        }
    }
}

class DrumOfVollok(BossModule module) : Components.StackWithCastTargets(module, AID.DrumOfVollokAOE, 4, 2, 2);

class DrumOfVollokKnockback(BossModule module) : Components.Knockback(module, ignoreImmunes: true)
{
    private readonly DrumOfVollok? _main = module.FindComponent<DrumOfVollok>();

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_main == null || _main.Stacks.Any(s => s.Target == actor))
            yield break;
        foreach (var s in _main.Stacks)
            if (actor.Position.InCircle(s.Target.Position, s.Radius))
                yield return new(s.Target.Position, 25, s.Activation);
    }
}
