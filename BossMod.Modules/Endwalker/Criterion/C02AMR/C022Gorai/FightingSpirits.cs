namespace BossMod.Endwalker.Criterion.C02AMR.C022Gorai;

class FightingSpirits(BossModule module, AID aid) : Components.KnockbackFromCastTarget(module, aid, 16);
class NFightingSpirits(BossModule module) : FightingSpirits(module, AID.NFightingSpiritsAOE);
class SFightingSpirits(BossModule module) : FightingSpirits(module, AID.SFightingSpiritsAOE);

class WorldlyPursuitBait(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private readonly int[] _order = [-1, -1, -1, -1];

    private static readonly AOEShapeCross _shape = new(60, 10);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_order[slot] >= 0)
            hints.Add($"Order: {_order[slot] + 1}", false);
        base.AddHints(slot, actor, hints);
    }

    // TODO: reconsider when we start showing first hint...
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.FightingSpirits)
            UpdateBait();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NWorldlyPursuitAOE or AID.SWorldlyPursuitAOE)
        {
            ++NumCasts;
            UpdateBait();
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var order = (IconID)iconID switch
        {
            IconID.Order1 => 0,
            IconID.Order2 => 1,
            IconID.Order3 => 2,
            IconID.Order4 => 3,
            _ => -1,
        };
        if (order >= 0 && Raid.TryFindSlot(actor.InstanceID, out var slot))
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

class WorldlyPursuitLast(BossModule module) : Components.GenericAOEs(module)
{
    private readonly DateTime _activation = module.WorldState.FutureTime(3.1f);

    private static readonly AOEShapeCross _shape = new(60, 10);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        yield return new(_shape, Module.Center, Angle.FromDirection(Module.Center - Module.PrimaryActor.Position), _activation);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NWorldlyPursuitAOE or AID.SWorldlyPursuitAOE)
            ++NumCasts;
    }
}
