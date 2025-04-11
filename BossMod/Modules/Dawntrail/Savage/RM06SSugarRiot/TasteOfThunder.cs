namespace BossMod.Dawntrail.Savage.RM06SSugarRiot;

class TasteOfThunder(BossModule module) : Components.GenericTowers(module)
{
    public bool Risky;

    private static readonly List<WPos> TowersOrdered = [
        new WPos(83f, 91f),
        new WPos(93f, 89f),
        new WPos(92f, 96f),
        new WPos(83f, 102f),
        new WPos(94f, 84f),
        new WPos(83f, 88f),
        new WPos(90f, 89f),
        new WPos(83f, 95f),
        new WPos(90f, 97.5f),
        new WPos(83f, 104f),
        new WPos(110f, 93f),
        new WPos(117f, 92f),
        new WPos(109f, 97f),
        new WPos(115f, 105f),
        new WPos(110f, 83f),
        new WPos(117f, 85f),
        new WPos(110f, 91f),
        new WPos(117f, 96f),
        new WPos(111f, 100f),
        new WPos(117f, 106f),
        new WPos(100f, 108f),
        new WPos(85f, 114f),
        new WPos(98f, 117f),
        new WPos(112f, 116f),
        new WPos(92f, 110f),
        new WPos(91f, 117f),
        new WPos(107f, 111f),
        new WPos(105f, 117f),
    ];

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index is >= 0x45 and <= 0x60 && state == 0x00020001)
            Towers.Add(new(TowersOrdered[index - 0x45], 3, activation: WorldState.FutureTime(19.2f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Explosion)
            Towers.RemoveAll(t => t.Position.AlmostEqual(caster.Position, 1));
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

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var t in Towers)
        {
            if (Arena.Config.ShowOutlinesAndShadows)
                Arena.AddCircle(t.Position, t.Radius, 0xFF000000, 3);
            Arena.AddCircle(t.Position, t.Radius, ArenaColor.Safe, Risky ? 2 : 1);
        }
    }
}
