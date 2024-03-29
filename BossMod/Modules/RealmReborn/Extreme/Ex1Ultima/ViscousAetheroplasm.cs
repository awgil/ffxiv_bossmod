namespace BossMod.RealmReborn.Extreme.Ex1Ultima;

class ViscousAetheroplasm : Components.Cleave
{
    public bool NeedTankSwap { get; private set; }
    private int[] _stacks = new int[PartyState.MaxPartySize];

    public ViscousAetheroplasm() : base(ActionID.MakeSpell(AID.ViscousAetheroplasm), new AOEShapeCircle(2), originAtTarget: true) { }

    public override void Update(BossModule module)
    {
        var tankSlot = module.WorldState.Party.FindSlot(module.PrimaryActor.TargetID);
        NeedTankSwap = tankSlot >= 0 && _stacks[tankSlot] >= 4;
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        base.AddHints(module, slot, actor, hints, movementHints);

        if (NeedTankSwap && actor.Role == Role.Tank)
            hints.Add(module.PrimaryActor.TargetID == actor.InstanceID ? "Pass aggro to co-tank!" : "Taunt boss!");
    }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.ViscousAetheroplasm)
            UpdateStacks(module, actor, status.Extra);
    }

    public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.ViscousAetheroplasm)
            UpdateStacks(module, actor, 0);
    }

    private void UpdateStacks(BossModule module, Actor actor, int stacks)
    {
        int slot = module.Raid.FindSlot(actor.InstanceID);
        if (slot >= 0)
            _stacks[slot] = stacks;
    }
}
