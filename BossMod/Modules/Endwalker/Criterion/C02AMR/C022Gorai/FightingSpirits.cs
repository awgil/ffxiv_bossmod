namespace BossMod.Endwalker.VariantCriterion.C02AMR.C022Gorai;

abstract class FightingSpirits(BossModule module, uint aid) : Components.SimpleKnockbacks(module, aid, 16f);
sealed class NFightingSpirits(BossModule module) : FightingSpirits(module, (uint)AID.NFightingSpiritsAOE);
sealed class SFightingSpirits(BossModule module) : FightingSpirits(module, (uint)AID.SFightingSpiritsAOE);

sealed class WorldlyPursuitBait(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private readonly int[] _order = [-1, -1, -1, -1];

    private static readonly AOEShapeCross _shape = new(60f, 10f);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_order[slot] >= 0)
            hints.Add($"Order: {_order[slot] + 1}", false);
        base.AddHints(slot, actor, hints);
    }

    // TODO: reconsider when we start showing first hint...
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.FightingSpirits)
            UpdateBait();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.NWorldlyPursuitAOE or (uint)AID.SWorldlyPursuitAOE)
        {
            ++NumCasts;
            UpdateBait();
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var order = iconID switch
        {
            (uint)IconID.Order1 => 0,
            (uint)IconID.Order2 => 1,
            (uint)IconID.Order3 => 2,
            (uint)IconID.Order4 => 3,
            _ => -1,
        };
        if (order >= 0 && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            _order[slot] = order;
        }
    }

    private void UpdateBait()
    {
        CurrentBaits.Clear();
        var baiter = Raid[Array.IndexOf(_order, NumCasts)];
        if (baiter != null)
            CurrentBaits.Add(new(Module.PrimaryActor, baiter, _shape));
    }
}

sealed class WorldlyPursuitLast(BossModule module) : Components.GenericAOEs(module)
{
    private readonly DateTime _activation = module.WorldState.FutureTime(3.1f);

    private static readonly AOEShapeCross _shape = new(60f, 10f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return new AOEInstance[1] { new(_shape, Arena.Center, Angle.FromDirection(Arena.Center - Module.PrimaryActor.Position), _activation) };
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.NWorldlyPursuitAOE or (uint)AID.SWorldlyPursuitAOE)
            ++NumCasts;
    }
}
