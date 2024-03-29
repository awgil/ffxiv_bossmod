namespace BossMod.Endwalker.Criterion.C02AMR.C022Gorai;

class FightingSpirits : Components.KnockbackFromCastTarget
{
    public FightingSpirits(AID aid) : base(ActionID.MakeSpell(aid), 16) { }
}
class NFightingSpirits : FightingSpirits { public NFightingSpirits() : base(AID.NFightingSpiritsAOE) { } }
class SFightingSpirits : FightingSpirits { public SFightingSpirits() : base(AID.SFightingSpiritsAOE) { } }

class WorldlyPursuitBait : Components.GenericBaitAway
{
    private int[] _order = { -1, -1, -1, -1 };

    private static readonly AOEShapeCross _shape = new(60, 10);

    public WorldlyPursuitBait() : base(centerAtTarget: true) { }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (_order[slot] >= 0)
            hints.Add($"Order: {_order[slot] + 1}", false);
        base.AddHints(module, slot, actor, hints, movementHints);
    }

    // TODO: reconsider when we start showing first hint...
    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.FightingSpirits)
            UpdateBait(module);
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NWorldlyPursuitAOE or AID.SWorldlyPursuitAOE)
        {
            ++NumCasts;
            UpdateBait(module);
        }
    }

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        var order = (IconID)iconID switch
        {
            IconID.Order1 => 0,
            IconID.Order2 => 1,
            IconID.Order3 => 2,
            IconID.Order4 => 3,
            _ => -1,
        };
        if (order >= 0 && module.Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            _order[slot] = order;
        }
    }

    private void UpdateBait(BossModule module)
    {
        CurrentBaits.Clear();
        var baiter = module.Raid[Array.IndexOf(_order, NumCasts)];
        if (baiter != null)
            CurrentBaits.Add(new(module.PrimaryActor, baiter, _shape));
    }
}

class WorldlyPursuitLast : Components.GenericAOEs
{
    private DateTime _activation;

    private static readonly AOEShapeCross _shape = new(60, 10);

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        yield return new(_shape, module.Bounds.Center, Angle.FromDirection(module.Bounds.Center - module.PrimaryActor.Position), _activation);
    }

    public override void Init(BossModule module) => _activation = module.WorldState.CurrentTime.AddSeconds(3.1f);

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NWorldlyPursuitAOE or AID.SWorldlyPursuitAOE)
            ++NumCasts;
    }
}
