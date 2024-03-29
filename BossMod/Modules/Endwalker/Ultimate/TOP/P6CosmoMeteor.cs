namespace BossMod.Endwalker.Ultimate.TOP;

class P6CosmoMeteorPuddles : Components.SelfTargetedAOEs
{
    public P6CosmoMeteorPuddles() : base(ActionID.MakeSpell(AID.CosmoMeteorAOE), new AOEShapeCircle(10)) { }
}

class P6CosmoMeteorAddComet : Components.Adds
{
    public P6CosmoMeteorAddComet() : base((uint)OID.CosmoComet) { }
}

class P6CosmoMeteorAddMeteor : Components.Adds
{
    public P6CosmoMeteorAddMeteor() : base((uint)OID.CosmoMeteor) { }
}

class P6CosmoMeteorSpread : Components.UniformStackSpread
{
    public int NumCasts { get; private set; }

    public P6CosmoMeteorSpread() : base(0, 5) { }

    public override void Init(BossModule module) => AddSpreads(module.Raid.WithoutSlot(true));

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.CosmoMeteorSpread)
            ++NumCasts;
    }
}

class P6CosmoMeteorFlares : Components.UniformStackSpread
{
    public P6CosmoMeteorFlares() : base(6, 20, 5, alwaysShowSpreads: true) { } // TODO: verify flare falloff

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        if ((IconID)iconID == IconID.OptimizedMeteor)
        {
            AddSpread(actor, module.WorldState.CurrentTime.AddSeconds(8.1f));
            if (Spreads.Count == 3)
            {
                // TODO: how is the stack target selected?
                var stackTarget = module.Raid.WithoutSlot().FirstOrDefault(p => !IsSpreadTarget(p));
                if (stackTarget != null)
                    AddStack(stackTarget, module.WorldState.CurrentTime.AddSeconds(8.1f));
            }
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.CosmoMeteorStack or AID.CosmoMeteorFlare)
        {
            Spreads.Clear();
            Stacks.Clear();
        }
    }
}
