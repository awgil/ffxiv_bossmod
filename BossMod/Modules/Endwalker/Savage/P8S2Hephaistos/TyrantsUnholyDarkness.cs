namespace BossMod.Endwalker.Savage.P8S2;

class TyrantsUnholyDarkness(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.TyrantsUnholyDarknessAOE))
{
    private const float _radius = 6;

    // TODO: we need some sort of a threat info in worldstate to determine targets properly...
    public bool IsTarget(Actor actor) => actor.Role == Role.Tank;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (IsTarget(actor))
        {
            hints.Add("GTFO from raid!", Raid.WithoutSlot().InRadiusExcluding(actor, _radius).Any());
        }
        else if (Raid.WithoutSlot().InRadiusExcluding(actor, _radius).Any(IsTarget))
        {
            hints.Add("GTFO from tanks!");
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => IsTarget(player) ? PlayerPriority.Danger : PlayerPriority.Irrelevant;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var target in Raid.WithoutSlot().Where(IsTarget))
            Arena.AddCircle(target.Position, _radius, ArenaColor.Danger);
    }
}
