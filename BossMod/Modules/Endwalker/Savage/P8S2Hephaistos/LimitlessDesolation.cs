namespace BossMod.Endwalker.Savage.P8S2;

class LimitlessDesolation : Components.UniformStackSpread
{
    public int NumAOEs { get; private set; }
    public int NumTowers { get; private set; }
    public int NumBursts { get; private set; }
    private BitMask _waitingForTowers;
    private BitMask _activeTowers;
    private int[] _towerAssignments = Utils.MakeArray(PartyState.MaxPartySize, -1); // [slot] = tower index
    private int[] _towerSlots = Utils.MakeArray(_towerOffsets.Length, -1); // [tower index] = slot
    private bool _thRight = Service.Config.Get<P8S2Config>().LimitlessDesolationTHRight;

    private static readonly float _towerRadius = 4;
    private static readonly WDir[] _towerOffsets = { new(-15, -15), new(-15, -5), new(-15, 5), new(-5, -15), new(-5, -5), new(-5, 5), new(5, -15), new(5, -5), new(5, 5), new(15, -15), new(15, -5), new(15, 5) };

    public LimitlessDesolation() : base(0, 6, alwaysShowSpreads: true, raidwideOnResolve: false) { }

    public override void Init(BossModule module)
    {
        AddSpreads(module.Raid.WithoutSlot());
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        base.AddHints(module, slot, actor, hints, movementHints);

        var towerIndex = _towerAssignments[slot];
        if (towerIndex >= 0 && !actor.Position.InCircle(module.Bounds.Center + _towerOffsets[towerIndex], _towerRadius))
            hints.Add("Soak assigned tower!");
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        base.DrawArenaForeground(module, pcSlot, pc, arena);

        var towerIndex = _towerAssignments[pcSlot];
        if (towerIndex >= 0)
            arena.AddCircle(module.Bounds.Center + _towerOffsets[towerIndex], _towerRadius, ArenaColor.Safe, 2);
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.TyrantsFire:
                Spreads.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
                _waitingForTowers.Set(module.Raid.FindSlot(spell.MainTargetID));
                ++NumAOEs;
                break;
            case AID.Burst:
                ++NumBursts;
                break;
        }
    }

    public override void OnEventEnvControl(BossModule module, byte index, uint state)
    {
        var towerIndex = index switch
        {
            0x4C => 0,
            0x4D => 3,
            0x4E => 6,
            0x4F => 9,
            0x50 => 1,
            // there is no tower #4 at (-5, -5) ???
            0x0A => 7,
            0x51 => 10,
            0x54 => 2,
            0x0B => 5,
            0x0C => 8,
            0x55 => 11,
            _ => -1,
        };
        if (towerIndex < 0)
            return;

        // 00020001 = appear
        // 00200010 = occupied
        // 00400001 = unoccupied
        // 00080004 = explode
        switch (state)
        {
            case 0x00020001: // appear
                _activeTowers.Set(towerIndex);
                bool towerForTH = _thRight == towerIndex >= 6;
                var (slot, player) = module.Raid.WithSlot(true).Where(ia => _waitingForTowers[ia.Item1] && ia.Item2.Class.IsSupport() == towerForTH).FirstOrDefault();
                if (player != null)
                {
                    _towerAssignments[slot] = towerIndex;
                    _towerSlots[towerIndex] = slot;
                    _waitingForTowers.Clear(slot);
                }
                ++NumTowers;
                break;
            case 0x00040004: // disappear
                _activeTowers.Clear(towerIndex);
                var assignedSlot = _towerSlots[towerIndex];
                if (assignedSlot >= 0)
                {
                    _towerAssignments[assignedSlot] = -1;
                }
                break;
            case 0x00200010: // become soaked
            case 0x00400001: // become unsoaked
                break;
        }
    }
}

class LimitlessDesolationTyrantsFlare : Components.LocationTargetedAOEs
{
    public LimitlessDesolationTyrantsFlare() : base(ActionID.MakeSpell(AID.TyrantsFlareLimitless), 8) { }
}
