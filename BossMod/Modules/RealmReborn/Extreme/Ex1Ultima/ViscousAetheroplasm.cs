namespace BossMod.RealmReborn.Extreme.Ex1Ultima;

class ViscousAetheroplasm(BossModule module) : Components.Cleave(module, AID.ViscousAetheroplasm, new AOEShapeCircle(2), originAtTarget: true)
{
    public bool NeedTankSwap { get; private set; }
    private readonly int[] _stacks = new int[PartyState.MaxPartySize];

    public override void Update()
    {
        NeedTankSwap = Raid.TryFindSlot(Module.PrimaryActor.TargetID, out var tankSlot) && _stacks[tankSlot] >= 4;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (NeedTankSwap && actor.Role == Role.Tank)
            hints.Add(Module.PrimaryActor.TargetID == actor.InstanceID ? "Pass aggro to co-tank!" : "Taunt boss!");
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.ViscousAetheroplasm)
            UpdateStacks(actor, status.Extra);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.ViscousAetheroplasm)
            UpdateStacks(actor, 0);
    }

    private void UpdateStacks(Actor actor, int stacks)
    {
        if (Raid.TryFindSlot(actor, out var slot))
            _stacks[slot] = stacks;
    }
}
