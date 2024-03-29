namespace BossMod.Endwalker.Savage.P2SHippokampos;

// state related to kampeos harma mechanic
// note that it relies on waymarks to determine safe spots...
class KampeosHarma : Components.CastCounter
{
    private WDir _startingOffset;
    private int[] _playerOrder = new int[8]; // 0 if unknown, then sq1 sq2 sq3 sq4 tri1 tri2 tri3 tri4

    public KampeosHarma() : base(ActionID.MakeSpell(AID.KampeosHarmaChargeBoss)) { }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        var safePos = GetSafeZone(module, slot);
        if (safePos != null && !actor.Position.InCircle(safePos.Value, 2))
        {
            hints.Add("Go to safe zone!");
            if (movementHints != null)
            {
                movementHints.Add(actor.Position, safePos.Value, ArenaColor.Danger);
            }
        }
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        var pos = GetSafeZone(module, pcSlot);
        if (pos != null)
            arena.AddCircle(pos.Value, 1, ArenaColor.Safe);
    }

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        if (iconID >= 145 && iconID <= 152)
        {
            _startingOffset = module.PrimaryActor.Position - module.Bounds.Center;

            int slot = module.Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
                _playerOrder[slot] = (int)(iconID - 144);
        }
    }

    private WPos? GetSafeZone(BossModule module, int slot)
    {
        switch (slot >= 0 ? _playerOrder[slot] : 0)
        {
            case 1: // sq 1 - opposite corner, hide after first charge
                return module.Bounds.Center + (NumCasts < 1 ? -1.2f : -1.4f) * _startingOffset;
            case 2: // sq 2 - same corner, hide after second charge
                return module.Bounds.Center + (NumCasts < 2 ? +1.2f : +1.4f) * _startingOffset;
            case 3: // sq 3 - opposite corner, hide before first charge
                return module.Bounds.Center + (NumCasts < 1 ? -1.4f : -1.2f) * _startingOffset;
            case 4: // sq 4 - same corner, hide before second charge
                return module.Bounds.Center + (NumCasts < 2 ? +1.4f : +1.2f) * _startingOffset;
            case 5: // tri 1 - waymark 1
                var wm1 = module.WorldState.Waymarks[Waymark.N1];
                return wm1 != null ? new(wm1.Value.XZ()) : null;
            case 6: // tri 2 - waymark 2
                var wm2 = module.WorldState.Waymarks[Waymark.N2];
                return wm2 != null ? new(wm2.Value.XZ()) : null;
            case 7: // tri 3 - waymark 3
                var wm3 = module.WorldState.Waymarks[Waymark.N3];
                return wm3 != null ? new(wm3.Value.XZ()) : null;
            case 8: // tri 4 - waymark 4
                var wm4 = module.WorldState.Waymarks[Waymark.N4];
                return wm4 != null ? new(wm4.Value.XZ()) : null;
        }
        return null;
    }
}
