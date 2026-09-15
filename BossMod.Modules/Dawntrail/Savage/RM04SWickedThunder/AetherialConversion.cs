namespace BossMod.Dawntrail.Savage.RM04SWickedThunder;

class AetherialConversion(BossModule module) : Components.CastCounter(module, default)
{
    public enum Mechanic { None, AOE, Knockback }

    public Mechanic CurMechanic;
    public int FirstOffsetX;

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CurMechanic != default)
            hints.Add($"{CurMechanic} {(FirstOffsetX < 0 ? "L->R" : "R->L")}");
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var (mechanic, firstOffset) = (AID)spell.Action.ID switch
        {
            AID.AetherialConversionHitLR => (Mechanic.AOE, -10),
            AID.AetherialConversionKnockbackLR => (Mechanic.Knockback, -10),
            AID.AetherialConversionHitRL => (Mechanic.AOE, +10),
            AID.AetherialConversionKnockbackRL => (Mechanic.Knockback, +10),
            _ => default
        };
        if (mechanic != default)
        {
            CurMechanic = mechanic;
            FirstOffsetX = firstOffset;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.TailThrust or AID.SwitchOfTides)
            ++NumCasts;
    }
}

class AetherialConversionTailThrust(BossModule module) : Components.GenericAOEs(module, AID.TailThrust)
{
    private readonly AetherialConversion? _comp = module.FindComponent<AetherialConversion>();

    private static readonly AOEShapeCircle _shape = new(18);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_comp?.CurMechanic == AetherialConversion.Mechanic.AOE && _comp.NumCasts < 2)
            yield return new(_shape, Module.Center + new WDir(_comp.NumCasts == 0 ? _comp.FirstOffsetX : -_comp.FirstOffsetX, 0), default);
    }
}

class AetherialConversionSwitchOfTides(BossModule module) : Components.Knockback(module, AID.SwitchOfTides, true)
{
    private readonly AetherialConversion? _comp = module.FindComponent<AetherialConversion>();

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_comp?.CurMechanic == AetherialConversion.Mechanic.Knockback && _comp.NumCasts < 2)
            yield return new(Module.Center + new WDir(_comp.NumCasts == 0 ? _comp.FirstOffsetX : -_comp.FirstOffsetX, 0), 25);
    }
}
