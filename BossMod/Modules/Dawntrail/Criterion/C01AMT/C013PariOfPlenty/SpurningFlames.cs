namespace BossMod.Dawntrail.Criterion.C01AMT.C013PariOfPlenty;

class SpurningFlames(BossModule module) : Components.RaidwideCast(module, (uint)AID.SpurningFlames);

class ScouringScorn(BossModule module) : Components.RaidwideCast(module, (uint)AID.ScouringScorn);

class ImpassionedSparks(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ImpassionedSparks3, new AOEShapeCircle(8f), 8);

class BurningPillar(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BurningPillar)
        {
            aoes.Add(new AOEInstance(new AOEShapeCircle(10f), caster.Position, default, default, Colors.Danger));
            NumCasts++;
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return CollectionsMarshal.AsSpan(aoes);
    }
}

class BurningPillarSpreads(BossModule module) : Components.IconStackSpread(module, default, (uint)IconID.HotFootSpread, default, (uint)AID.HotFoot, default, 10f, default);

class FireChains(BossModule module) : Components.Chains(module, (uint)TetherID.FireChain, default, 15F);