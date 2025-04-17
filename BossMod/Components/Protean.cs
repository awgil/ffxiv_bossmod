namespace BossMod.Components;

// generic protean mechanic is a bunch of aoes baited in some manner by players that have to hit that player only
// TODO: combine with BaitAway
public abstract class GenericProtean(BossModule module, Enum aid, AOEShape shape) : CastCounter(module, aid)
{
    public AOEShape Shape { get; init; } = shape;

    public abstract IEnumerable<(Actor source, Actor target)> ActiveAOEs();

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (ActiveAOEs().Any(st => st.target != actor && IsPlayerClipped(st.source, st.target, actor)))
            hints.Add("GTFO from protean!");
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        var playerProteanSource = player != pc ? ActiveAOEs().FirstOrDefault(st => st.target == player).source : null;
        return playerProteanSource == null ? PlayerPriority.Irrelevant : IsPlayerClipped(playerProteanSource, player, pc) ? PlayerPriority.Danger : PlayerPriority.Normal;
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        // draw own protean (if any) and clipping proteans (if any)
        foreach (var (source, target) in ActiveAOEs())
            if (target == pc || IsPlayerClipped(source, target, pc))
                Shape.Draw(Arena, source.Position, Angle.FromDirection(target.Position - source.Position));
    }

    public bool IsPlayerClipped(Actor source, Actor target, Actor player) => Shape.Check(player.Position, source.Position, Angle.FromDirection(target.Position - source.Position));
}

// typical protean will originate from primary actor and hit all alive players
public class SimpleProtean(BossModule module, Enum aid, AOEShape shape) : GenericProtean(module, aid, shape)
{
    public override IEnumerable<(Actor source, Actor target)> ActiveAOEs() => Raid.WithoutSlot().Select(p => (Module.PrimaryActor, p));
}
