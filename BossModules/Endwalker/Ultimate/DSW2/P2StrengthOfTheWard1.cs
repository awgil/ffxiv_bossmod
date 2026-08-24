namespace BossMod.Endwalker.Ultimate.DSW2;

// spreads
class P2StrengthOfTheWard1LightningStorm : Components.UniformStackSpread
{
    public P2StrengthOfTheWard1LightningStorm(BossModule module) : base(module, 0, 5)
    {
        AddSpreads(Raid.WithoutSlot(true));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.LightningStormAOE)
            Spreads.Clear();
    }
}

// charges
class P2StrengthOfTheWard1SpiralThrust(BossModule module) : Components.GenericAOEs(module, AID.SpiralThrust, "GTFO from charge aoe!")
{
    private readonly List<Actor> _knights = [];

    private static readonly AOEShapeRect _shape = new(52, 8);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var k in _knights)
            yield return new(_shape, k.Position, k.Rotation); // TODO: activation
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x1E43 && (OID)actor.OID is OID.SerVellguine or OID.SerPaulecrain or OID.SerIgnasse)
            _knights.Add(actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            _knights.Remove(caster);
            ++NumCasts;
        }
    }
}

// rings
class P2StrengthOfTheWard1HeavyImpact(BossModule module) : HeavyImpact(module, 8.2f);
