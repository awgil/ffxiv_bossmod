namespace BossMod.Endwalker.Savage.P8S2;

class Dominion : Components.UniformStackSpread
{
    public int NumDeformations { get; private set; }
    public int NumShifts { get; private set; }
    public List<Actor> Casters = [];
    private BitMask _secondOrder;

    private const float _towerRadius = 3;

    public Dominion(BossModule module) : base(module, 0, 6)
    {
        AddSpreads(Raid.WithoutSlot());
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (NumDeformations >= 4)
        {
            hints.Add($"Soak order: {(_secondOrder[slot] ? "2" : "1")}", ShouldSoak(slot) && !ActiveTowers().Any(t => actor.Position.InCircle(t.Position, _towerRadius)));
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (NumDeformations >= 4 && ShouldSoak(pcSlot))
            foreach (var tower in ActiveTowers())
                Arena.AddCircle(tower.Position, _towerRadius, ArenaColor.Danger, 2);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.OrogenicShift)
        {
            Casters.Add(caster);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.OrogenicShift)
        {
            ++NumShifts;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.OrogenicDeformation)
        {
            Spreads.Clear();
            _secondOrder.Set(Raid.FindSlot(spell.MainTargetID));
            ++NumDeformations;
        }
    }

    private bool ShouldSoak(int slot) => NumShifts == (_secondOrder[slot] ? 4 : 0);

    private IEnumerable<Actor> ActiveTowers()
    {
        if (NumShifts == 0 && Casters.Count >= 4)
            foreach (var c in Casters.Take(4))
                yield return c;
        else if (NumShifts == 4 && Casters.Count == 8)
            foreach (var c in Casters.Skip(4))
                yield return c;
    }
}
