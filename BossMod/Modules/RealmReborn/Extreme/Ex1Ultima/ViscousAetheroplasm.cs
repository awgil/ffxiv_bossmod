namespace BossMod.RealmReborn.Extreme.Ex1Ultima;

class ViscousAetheroplasm : Components.Cleave
{
    public bool NeedTankSwap { get; private set; }
    private int[] _stacks = new int[PartyState.MaxPartySize];

    public ViscousAetheroplasm() : base(ActionID.MakeSpell(AID.ViscousAetheroplasm), new AOEShapeCircle(2), originAtTarget: true) { }

    public override void Update()
    {
        var tankSlot = WorldState.Party.FindSlot(Module.PrimaryActor.TargetID);
        NeedTankSwap = tankSlot >= 0 && _stacks[tankSlot] >= 4;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(module, slot, actor, hints, movementHints);

        if (NeedTankSwap && actor.Role == Role.Tank)
            hints.Add(Module.PrimaryActor.TargetID == actor.InstanceID ? "Pass aggro to co-tank!" : "Taunt boss!");
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.ViscousAetheroplasm)
            UpdateStacks(module, actor, status.Extra);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.ViscousAetheroplasm)
            UpdateStacks(module, actor, 0);
    }

    private void UpdateStacks(BossModule module, Actor actor, int stacks)
    {
        int slot = Raid.FindSlot(actor.InstanceID);
        if (slot >= 0)
            _stacks[slot] = stacks;
    }
}
