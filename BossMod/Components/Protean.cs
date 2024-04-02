namespace BossMod.Components;

// generic protean mechanic is a bunch of aoes baited in some manner by players that have to hit that player only
// TODO: combine with BaitAway
public abstract class GenericProtean : CastCounter
{
    public AOEShape Shape { get; private init; }

    public GenericProtean(ActionID aid, AOEShape shape) : base(aid)
    {
        Shape = shape;
    }

    public abstract IEnumerable<(Actor source, Actor target)> ActiveAOEs(BossModule module);

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (ActiveAOEs(module).Any(st => st.target != actor && IsPlayerClipped(st.source, st.target, actor)))
            hints.Add("GTFO from protean!");
    }

    public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        var playerProteanSource = player != pc ? ActiveAOEs(module).FirstOrDefault(st => st.target == player).source : null;
        return playerProteanSource == null ? PlayerPriority.Irrelevant : IsPlayerClipped(playerProteanSource, player, pc) ? PlayerPriority.Danger : PlayerPriority.Normal;
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        // draw own protean (if any) and clipping proteans (if any)
        foreach (var (source, target) in ActiveAOEs(module))
            if (target == pc || IsPlayerClipped(source, target, pc))
                Shape.Draw(arena, source.Position, Angle.FromDirection(target.Position - source.Position));
    }

    public bool IsPlayerClipped(Actor source, Actor target, Actor player) => Shape.Check(player.Position, source.Position, Angle.FromDirection(target.Position - source.Position));
}

// typical protean will originate from primary actor and hit all alive players
public class SimpleProtean : GenericProtean
{
    public SimpleProtean(ActionID aid, AOEShape shape) : base(aid, shape) { }

    public override IEnumerable<(Actor source, Actor target)> ActiveAOEs(BossModule module)
    {
        return module.Raid.WithoutSlot().Select(p => (module.PrimaryActor, p));
    }
}
