namespace BossMod.Endwalker.VariantCriterion.C03AAI.C032Lala;

class PlanarTactics(BossModule module) : Components.GenericAOEs(module)
{
    public struct PlayerState
    {
        public int SubtractiveStacks;
        public bool StackTarget;
        public WDir[]? StartingOffsets;
    }

    public List<AOEInstance> Mines = [];
    public PlayerState[] Players = new PlayerState[4];

    private static readonly AOEShapeRect _shape = new(8f, 4f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(Mines);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        ref readonly var p = ref Players[pcSlot];
        if (p.StartingOffsets != null)
        {
            var len = p.StartingOffsets.Length;
            for (var i = 0; i < len; ++i)
                Arena.ZoneCircleOutline(Arena.Center + p.StartingOffsets[i], 1f, Colors.Safe);
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.SubtractiveSuppressorAlpha:
                if (Raid.FindSlot(actor.InstanceID) is var slot3 && slot3 >= 0 && slot3 < Players.Length)
                    Players[slot3].SubtractiveStacks = status.Extra;
                break;
            case (uint)SID.SurgeVector:
                if (Raid.FindSlot(actor.InstanceID) is var slot4 && slot4 >= 0 && slot4 < Players.Length)
                    Players[slot4].StackTarget = true;
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.NArcaneMineAOE or (uint)AID.SArcaneMineAOE)
        {
            Mines.Add(new(_shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            if (Mines.Count == 8)
            {
                InitSafespots();
            }
        }
    }

    private void InitSafespots()
    {
        WDir safeCornerOffset = default;
        var count = Mines.Count;
        for (var i = 0; i < count; ++i)
            safeCornerOffset -= Mines[i].Origin + new WDir(0, 4) - Arena.Center;
        var relSouth = (safeCornerOffset + safeCornerOffset.OrthoL()) / 16f;
        var relWest = relSouth.OrthoR();
        var off1 = 5f * relSouth + 13f * relWest;
        var off2a = 3f * relSouth + 13f * relWest;
        var off2b = -8f * relSouth + 16f * relWest;
        var off3 = 13f * relSouth - 8f * relWest;
        var sumStacks = 0;
        var len = Players.Length;

        for (var i = 0; i < len; ++i)
        {
            ref readonly var p = ref Players[i];
            if (p.StackTarget)
                sumStacks += p.SubtractiveStacks;
        }

        for (var i = 0; i < len; ++i)
        {
            ref var p = ref Players[i];
            p.StartingOffsets = (p.SubtractiveStacks, sumStacks) switch
            {
                (1, _) => [off1],
                (2, 3) => [p.StackTarget ? off2b : off2a],
                (2, 4) => [off2a, off2b],
                (2, 5) => [p.StackTarget ? off2a : off2b],
                (3, _) => [off3],
                _ => null
            };
        }
    }
}

class PlanarTacticsForcedMarch : Components.GenericForcedMarch
{
    private readonly int[] _rotationCount = new int[4];
    private readonly Angle[] _rotation = new Angle[4];
    private DateTime _activation;

    public PlanarTacticsForcedMarch(BossModule module) : base(module)
    {
        MovementSpeed = 4;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_rotation[slot] != default)
            hints.Add($"Rotation: {(_rotation[slot].Rad < 0 ? "CW" : "CCW")}", false);
        base.AddHints(slot, actor, hints);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.TimesThreePlayer:
                _activation = status.ExpireAt;
                if (Raid.FindSlot(actor.InstanceID) is var slot1 && slot1 >= 0 && slot1 < _rotationCount.Length)
                    _rotationCount[slot1] = -1;
                break;
            case (uint)SID.TimesFivePlayer:
                _activation = status.ExpireAt;
                if (Raid.FindSlot(actor.InstanceID) is var slot2 && slot2 >= 0 && slot2 < _rotationCount.Length)
                    _rotationCount[slot2] = 1;
                break;
            case (uint)SID.ForcedMarch:
                State.GetOrAdd(actor.InstanceID).PendingMoves.Clear();
                ActivateForcedMovement(actor, status.ExpireAt);
                break;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var rot = iconID switch
        {
            (uint)IconID.PlayerRotateCW => -90f.Degrees(),
            (uint)IconID.PlayerRotateCCW => 90f.Degrees(),
            _ => default
        };
        if (rot != default && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0 && slot < _rotationCount.Length)
        {
            _rotation[slot] = rot * _rotationCount[slot];
            AddForcedMovement(actor, _rotation[slot], 6, _activation);
        }
    }
}
