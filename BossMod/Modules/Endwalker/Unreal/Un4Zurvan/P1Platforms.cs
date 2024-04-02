namespace BossMod.Endwalker.Unreal.Un4Zurvan;

class P1Platforms : Components.GenericAOEs
{
    public List<AOEInstance> ForbiddenPlatforms = new();

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => ForbiddenPlatforms;

    public override void OnActorEAnim(BossModule module, Actor actor, uint state)
    {
        var dir = (OID)actor.OID switch
        {
            OID.PlatformE => 90.Degrees(),
            OID.PlatformN => 180.Degrees(),
            OID.PlatformW => -90.Degrees(),
            _ => default
        };
        if (dir == default)
            return;

        switch (state)
        {
            case 0x00040008:
                ForbiddenPlatforms.Add(new(new AOEShapeCone(20, 45.Degrees()), module.Bounds.Center, dir, module.WorldState.CurrentTime.AddSeconds(5)));
                break;
            case 0x00100020:
                ++NumCasts;
                break;
        }
    }
}
