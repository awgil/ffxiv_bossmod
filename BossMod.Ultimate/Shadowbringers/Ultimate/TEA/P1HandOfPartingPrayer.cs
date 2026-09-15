namespace BossMod.Shadowbringers.Ultimate.TEA;

// TODO: determine when mechanic is selected; determine threshold
class P1HandOfPartingPrayer(BossModule module) : BossComponent(module)
{
    public bool Resolved { get; private set; }

    public override void AddGlobalHints(GlobalHints hints)
    {
        var hint = (Module.Enemies(OID.LiquidHand).FirstOrDefault()?.ModelState.ModelState ?? 0) switch
        {
            19 => "Split boss & hand",
            20 => "Stack boss & hand",
            _ => ""
        };
        if (hint.Length > 0)
            hints.Add(hint);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.HandOfParting or AID.HandOfPrayer)
            Resolved = true;
    }
}
