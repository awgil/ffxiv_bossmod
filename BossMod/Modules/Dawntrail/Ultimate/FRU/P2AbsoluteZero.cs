namespace BossMod.Dawntrail.Ultimate.FRU;

class P2AbsoluteZero(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.AbsoluteZeroAOE));

class P2SwellingFrost(BossModule module) : Components.Knockback(module, ActionID.MakeSpell(AID.SwellingFrost)) // TODO: verify whether it ignores KB
{
    private readonly DateTime _activation = module.WorldState.FutureTime(3.2f);

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        yield return new(Module.Center, 10, _activation);
    }
}

class P2SinboundBlizzard(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SinboundBlizzardAOE), new AOEShapeCone(50, 10.Degrees()));
class P2HiemalStorm(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.HiemalStormAOE), 7);
class P2HiemalRay(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 4, ActionID.MakeSpell(AID.HiemalRay), module => module.Enemies(OID.HiemalRayVoidzone).Where(z => z.EventState != 7), 0.7f);
