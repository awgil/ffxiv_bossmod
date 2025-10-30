namespace BossMod.Endwalker.Extreme.Ex6Golbez;

class AzdajasShadow(BossModule module) : BossComponent(module)
{
    public enum Mechanic { Unknown, CircleStack, DonutSpread }

    public Mechanic CurMechanic { get; private set; }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CurMechanic != Mechanic.Unknown)
            hints.Add($"Next mechanic: {(CurMechanic == Mechanic.CircleStack ? "out -> stack" : "in -> spread")}");
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
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

class FlamesOfEventide(BossModule module) : Components.GenericBaitAway(module, AID.FlamesOfEventide)
{
    private readonly int[] _playerStacks = new int[PartyState.MaxPartySize];

    private static readonly AOEShapeRect _shape = new(50, 3);

    public override void Update()
    {
        CurrentBaits.Clear();
        var target = WorldState.Actors.Find(Module.PrimaryActor.TargetID);
        if (target != null)
            CurrentBaits.Add(new(Module.PrimaryActor, target, _shape));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Module.PrimaryActor.TargetID == actor.InstanceID)
        {
            if (_playerStacks[slot] >= 2)
                hints.Add("Pass aggro!");
            if (Raid.WithoutSlot().Exclude(actor).InShape(_shape, Module.PrimaryActor.Position, Angle.FromDirection(actor.Position - Module.PrimaryActor.Position)).Any())
                hints.Add("GTFO from raid!");
        }
        else if (CurrentBaits.Count > 0)
        {
            if (IsClippedBy(actor, CurrentBaits[0]))
                hints.Add("GTFO from tank!");
            if (_playerStacks[slot] < 2 && actor.Role == Role.Tank && Raid.TryFindSlot(Module.PrimaryActor.TargetID, out var tankSlot) && _playerStacks[tankSlot] >= 2)
                hints.Add("Taunt!");
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.FlamesOfEventide && Raid.TryFindSlot(actor.InstanceID, out var slot))
        {
            _playerStacks[slot] = status.Extra;
            if (status.Extra >= 2)
                ForbiddenPlayers.Set(slot);
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.FlamesOfEventide && Raid.TryFindSlot(actor.InstanceID, out var slot))
        {
            _playerStacks[slot] = 0;
            ForbiddenPlayers.Clear(slot);
        }
    }
}
