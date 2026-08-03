namespace BossMod.Dawntrail.Extreme.Ex4Zelenia;

sealed class ThornyVine(BossModule module) : Components.Chains(module, (uint)TetherID.ThornyVine, default, 25f)
{
    private readonly Emblazon _emblazon = module.FindComponent<Emblazon>()!;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!TethersAssigned && _emblazon.WedgeCenterDirection != default)
        {
            Arena.ZoneCircleOutline(Arena.Center - 5f * _emblazon.WedgeCenterDirection, 3f, Colors.Safe);
        }
        base.DrawArenaForeground(pcSlot, pc);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!TethersAssigned && _emblazon.WedgeCenterDirection != default)
        {
            hints.Add("Meet at marked spot for chains!");
        }
        else
        {
            base.AddHints(slot, actor, hints);
        }
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        base.OnUntethered(source, tether);
        if (tether.ID == (uint)TetherID.ThornyVine)
        {
            ++NumCasts;
        }
    }
}
