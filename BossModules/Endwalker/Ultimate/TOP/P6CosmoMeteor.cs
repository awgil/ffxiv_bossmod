namespace BossMod.Endwalker.Ultimate.TOP;

class P6CosmoMeteorPuddles(BossModule module) : Components.StandardAOEs(module, AID.CosmoMeteorAOE, new AOEShapeCircle(10));

class P6CosmoMeteorAddComet(BossModule module) : Components.Adds(module, (uint)OID.CosmoComet);

class P6CosmoMeteorAddMeteor(BossModule module) : Components.Adds(module, (uint)OID.CosmoMeteor);

class P6CosmoMeteorSpread : Components.UniformStackSpread
{
    public int NumCasts { get; private set; }

    public P6CosmoMeteorSpread(BossModule module) : base(module, 0, 5)
    {
        AddSpreads(Raid.WithoutSlot(true));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.CosmoMeteorSpread)
            ++NumCasts;
    }
}

class P6CosmoMeteorFlares(BossModule module) : Components.UniformStackSpread(module, 6, 20, 5, alwaysShowSpreads: true) // TODO: verify flare falloff
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.OptimizedMeteor)
        {
            AddSpread(actor, WorldState.FutureTime(8.1f));
            if (Spreads.Count == 3)
            {
                // TODO: how is the stack target selected?
                var stackTarget = Raid.WithoutSlot().FirstOrDefault(p => !IsSpreadTarget(p));
                if (stackTarget != null)
                    AddStack(stackTarget, WorldState.FutureTime(8.1f));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.CosmoMeteorStack or AID.CosmoMeteorFlare)
        {
            Spreads.Clear();
            Stacks.Clear();
        }
    }
}
