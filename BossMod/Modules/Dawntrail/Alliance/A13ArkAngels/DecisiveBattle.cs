namespace BossMod.Dawntrail.Alliance.A13ArkAngels;

class DecisiveBattle(BossModule module) : Components.GenericInvincible(module, "Target correct boss!")
{
    enum Color
    {
        None,
        Epic,
        Fated,
        Vaunted
    }

    private readonly Actor?[] _bosses = new Actor?[4];
    private readonly Color[] _colors = new Color[PartyState.MaxAllianceSize];

    protected override IEnumerable<Actor> ForbiddenTargets(int slot, Actor actor)
    {
        var color = GetColor(slot);
        if (color == Color.None)
            yield break;

        for (var c = Color.Epic; c <= Color.Vaunted; c++)
            if (c != color && _bosses[(int)c] is { } boss)
                yield return boss;
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.EpicHero:
                Assign(actor, Color.Epic);
                break;
            case SID.FatedHero:
                Assign(actor, Color.Fated);
                break;
            case SID.VauntedHero:
                Assign(actor, Color.Vaunted);
                break;
            case SID.EpicVillain:
                _bosses[(int)Color.Epic] = actor;
                break;
            case SID.FatedVillain:
                _bosses[(int)Color.Fated] = actor;
                break;
            case SID.VauntedVillain:
                _bosses[(int)Color.Vaunted] = actor;
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.EpicHero:
            case SID.FatedHero:
            case SID.VauntedHero:
                Assign(actor, Color.None);
                break;
        }
    }

    private void Assign(Actor a, Color c)
    {
        if (Raid.TryFindSlot(a, out var slot))
            _colors[slot] = c;
    }

    private Color GetColor(int slot) => _colors.BoundSafeAt(slot, Color.None);
}
