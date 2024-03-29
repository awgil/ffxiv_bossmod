namespace BossMod.Endwalker.Savage.P8S2;

class TyrantsUnholyDarkness : Components.CastCounter
{
    private static readonly float _radius = 6;

    // TODO: we need some sort of a threat info in worldstate to determine targets properly...
    public bool IsTarget(Actor actor) => actor.Role == Role.Tank;

    public TyrantsUnholyDarkness() : base(ActionID.MakeSpell(AID.TyrantsUnholyDarknessAOE)) { }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (IsTarget(actor))
        {
            hints.Add("GTFO from raid!", module.Raid.WithoutSlot().InRadiusExcluding(actor, _radius).Any());
        }
        else if (module.Raid.WithoutSlot().InRadiusExcluding(actor, _radius).Any(IsTarget))
        {
            hints.Add("GTFO from tanks!");
        }
    }

    public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return IsTarget(player) ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        foreach (var target in module.Raid.WithoutSlot().Where(IsTarget))
            arena.AddCircle(target.Position, _radius, ArenaColor.Danger);
    }
}
