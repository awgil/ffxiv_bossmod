namespace BossMod.Endwalker.Extreme.Ex6Golbez;

class AzdajasShadow : BossComponent
{
    public enum Mechanic { Unknown, CircleStack, DonutSpread }

    public Mechanic CurMechanic { get; private set; }

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        if (CurMechanic != Mechanic.Unknown)
            hints.Add($"Next mechanic: {(CurMechanic == Mechanic.CircleStack ? "out -> stack" : "in -> spread")}");
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        var mechanic = (AID)spell.Action.ID switch
        {
            AID.AzdajasShadowCircleStack => Mechanic.CircleStack,
            AID.AzdajasShadowDonutSpread => Mechanic.DonutSpread,
            _ => Mechanic.Unknown
        };
        if (mechanic != Mechanic.Unknown)
            CurMechanic = mechanic;
    }
}

class FlamesOfEventide : Components.GenericBaitAway
{
    private int[] _playerStacks = new int[PartyState.MaxPartySize];

    private static readonly AOEShapeRect _shape = new(50, 3);

    public FlamesOfEventide() : base(ActionID.MakeSpell(AID.FlamesOfEventide)) { }

    public override void Update(BossModule module)
    {
        CurrentBaits.Clear();
        var target = module.WorldState.Actors.Find(module.PrimaryActor.TargetID);
        if (target != null)
            CurrentBaits.Add(new(module.PrimaryActor, target, _shape));
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (module.PrimaryActor.TargetID == actor.InstanceID)
        {
            if (_playerStacks[slot] >= 2)
                hints.Add("Pass aggro!");
            if (module.Raid.WithoutSlot().Exclude(actor).InShape(_shape, module.PrimaryActor.Position, Angle.FromDirection(actor.Position - module.PrimaryActor.Position)).Any())
                hints.Add("GTFO from raid!");
        }
        else if (CurrentBaits.Count > 0)
        {
            if (IsClippedBy(actor, CurrentBaits[0]))
                hints.Add("GTFO from tank!");
            if (_playerStacks[slot] < 2 && actor.Role == Role.Tank && module.Raid.FindSlot(module.PrimaryActor.TargetID) is var tankSlot && tankSlot >= 0 && _playerStacks[tankSlot] >= 2)
                hints.Add("Taunt!");
        }
    }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.FlamesOfEventide && module.Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            _playerStacks[slot] = status.Extra;
            if (status.Extra >= 2)
                ForbiddenPlayers.Set(slot);
        }
    }

    public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.FlamesOfEventide && module.Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            _playerStacks[slot] = 0;
            ForbiddenPlayers.Clear(slot);
        }
    }
}
