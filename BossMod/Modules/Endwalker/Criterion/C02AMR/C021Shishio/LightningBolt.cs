namespace BossMod.Endwalker.VariantCriterion.C02AMR.C021Shishio;

abstract class LightningBolt(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, 6f);
sealed class NLightningBolt(BossModule module) : LightningBolt(module, (uint)AID.NLightningBoltAOE);
sealed class SLightningBolt(BossModule module) : LightningBolt(module, (uint)AID.SLightningBoltAOE);

sealed class CloudToCloud(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private readonly AOEShapeRect _shape1 = new(100f, 1f);
    private readonly AOEShapeRect _shape2 = new(100f, 3f);
    private readonly AOEShapeRect _shape3 = new(100f, 6f);

    public bool Active => _aoes.Count > 0;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var deadline = aoes[0].Activation.AddSeconds(1.4d);

        var index = 0;
        while (index < count)
        {
            ref var aoe = ref aoes[index];
            if (aoe.Activation >= deadline)
            {
                break;
            }
            ++index;
        }

        return aoes[..index];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var shape = spell.Action.ID switch
        {
            (uint)AID.NCloudToCloud1 or (uint)AID.SCloudToCloud1 => _shape1,
            (uint)AID.NCloudToCloud2 or (uint)AID.SCloudToCloud2 => _shape2,
            (uint)AID.NCloudToCloud3 or (uint)AID.SCloudToCloud3 => _shape3,
            _ => null
        };
        if (shape != null)
        {
            var loc = spell.LocXZ;
            var rot = spell.Rotation;
            _aoes.Add(new(shape, loc, rot, Module.CastFinishAt(spell), actorID: caster.InstanceID, shapeDistance: shape.Distance(loc, rot)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return;
        }
        var id = caster.InstanceID;
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        for (var i = 0; i < count; ++i)
        {
            if (aoes[i].ActorID == id)
            {
                _aoes.RemoveAt(i);
                ++NumCasts;
                return;
            }
        }
    }
}
