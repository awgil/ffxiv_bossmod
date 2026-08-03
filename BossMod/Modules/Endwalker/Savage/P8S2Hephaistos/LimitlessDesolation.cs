namespace BossMod.Endwalker.Savage.P8S2;

class LimitlessDesolation : Components.UniformStackSpread
{
    public int NumAOEs;
    public int NumTowers;
    public int NumBursts;
    private BitMask _waitingForTowers;
    private BitMask _activeTowers;
    private readonly int[] _towerAssignments = Utils.MakeArray(PartyState.MaxPartySize, -1); // [slot] = tower index
    private readonly int[] _towerSlots = Utils.MakeArray(_towerOffsets.Length, -1); // [tower index] = slot
    private readonly bool _thRight = Service.Config.Get<P8S2Config>().LimitlessDesolationTHRight;

    private const float _towerRadius = 4f;
    private static readonly WDir[] _towerOffsets = [new(-15f, -15f), new(-15f, -5f), new(-15f, 5f), new(-5f, -15f),
        new(-5f, -5f), new(-5f, 5f), new(5f, -15f), new(5f, -5f), new(5f, 5f), new(15f, -15f), new(15f, -5f), new(15f, 5f)];

    public LimitlessDesolation(BossModule module) : base(module, default, 6f, raidwideOnResolve: false)
    {
        AddSpreads(Raid.WithoutSlot(false, true, true));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        var towerIndex = _towerAssignments[slot];
        if (towerIndex >= 0 && !actor.Position.InCircle(Arena.Center + _towerOffsets[towerIndex], _towerRadius))
            hints.Add("Soak assigned tower!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        var towerIndex = _towerAssignments[pcSlot];
        if (towerIndex >= 0)
            Arena.ZoneCircleOutline(Arena.Center + _towerOffsets[towerIndex], _towerRadius, Colors.Safe, 2f);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.TyrantsFire:
                Spreads.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
                _waitingForTowers.Set(Raid.FindSlot(spell.MainTargetID));
                ++NumAOEs;
                break;
            case (uint)AID.Burst:
                ++NumBursts;
                break;
        }
    }

    public override void OnMapEffect(byte index, uint state)
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
            case 0x00020001u: // appear
                _activeTowers.Set(towerIndex);
                var towerForTH = _thRight == towerIndex >= 6;
                var (slot, player) = Raid.WithSlot(true, true, true).FirstOrDefault(ia => _waitingForTowers[ia.Item1] && ia.Item2.Class.IsSupport() == towerForTH);
                if (player != null)
                {
                    _towerAssignments[slot] = towerIndex;
                    _towerSlots[towerIndex] = slot;
                    _waitingForTowers.Clear(slot);
                }
                ++NumTowers;
                break;
            case 0x00040004u: // disappear
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

class LimitlessDesolationTyrantsFlare(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TyrantsFlareLimitless, 8f);
