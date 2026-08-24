namespace BossMod.Dawntrail.Savage.RM07SBruteAbominator;

class P2GlowerPower(BossModule module) : Components.StandardAOEs(module, AID.P2GlowerPower, new AOEShapeRect(65, 7));

class P2ElectrogeneticForce(BossModule module) : Components.UniformStackSpread(module, 0, 6, alwaysShowSpreads: true)
{
    public int NumCasts;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.P2GlowerPowerVisual)
            AddSpreads(Raid.WithoutSlot(), WorldState.FutureTime(3.9f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ElectrogeneticForce)
        {
            NumCasts++;
            Spreads.Clear();
        }
    }
}

class P3GlowerPower(BossModule module) : Components.StandardAOEs(module, AID.P3GlowerPower, new AOEShapeRect(65, 7));

class P3ElectrogeneticForce : Components.UniformStackSpread
{
    public int NumCasts;
    public bool Risky;

    public P3ElectrogeneticForce(BossModule module) : base(module, 0, 6, alwaysShowSpreads: true)
    {
        AddSpreads(Raid.WithoutSlot(), WorldState.FutureTime(11.3f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ElectrogeneticForce)
        {
            NumCasts++;
            Spreads.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Risky)
            base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Risky)
            base.AddAIHints(slot, actor, assignment, hints);
    }
}
