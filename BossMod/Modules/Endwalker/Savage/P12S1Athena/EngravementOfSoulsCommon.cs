namespace BossMod.Endwalker.Savage.P12S1Athena;

class EngravementOfSoulsTethers : Components.GenericBaitAway
{
    public enum TetherType { None, Light, Dark }

    public struct PlayerState
    {
        public Actor? Source;
        public TetherType Tether;
        public bool TooClose;
    }

    public PlayerState[] States = new PlayerState[PartyState.MaxPartySize];

    private static readonly AOEShapeRect _shape = new(60, 3);

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        base.AddHints(module, slot, actor, hints, movementHints);
        if (States[slot].TooClose)
            hints.Add("Stretch the tether!");
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        base.DrawArenaForeground(module, pcSlot, pc, arena);

        foreach (var b in CurrentBaits)
            arena.Actor(b.Source, ArenaColor.Object, true);

        // TODO: consider drawing safespot based on configured strategy and mechanic order
        if (NumCasts == 0 && States[pcSlot] is var state && state.Source != null)
            arena.AddLine(state.Source.Position, pc.Position, state.TooClose ? ArenaColor.Danger : ArenaColor.Safe);
    }

    public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
    {
        var (type, tooClose) = (TetherID)tether.ID switch
        {
            TetherID.LightNear => (TetherType.Light, true),
            TetherID.DarkNear => (TetherType.Dark, true),
            TetherID.LightFar => (TetherType.Light, false),
            TetherID.DarkFar => (TetherType.Dark, false),
            _ => (TetherType.None, false)
        };
        if (type != TetherType.None && module.Raid.FindSlot(tether.Target) is var slot && slot >= 0)
        {
            if (States[slot].Source == null)
            {
                States[slot] = new() { Source = source, Tether = type, TooClose = tooClose };
                CurrentBaits.Add(new(source, module.Raid[slot]!, _shape));
            }
            else if (States[slot].Source != source)
            {
                module.ReportError(this, $"Multiple tethers on same player");
            }
            else if (States[slot].Tether != type)
            {
                module.ReportError(this, $"Tether type changed");
            }
            else
            {
                States[slot].TooClose = tooClose;
            }
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.SearingRadiance or AID.Shadowsear)
        {
            ++NumCasts;
            CurrentBaits.RemoveAll(b => b.Source == caster);
        }
    }
}

// towers can not be soaked by same colored tilt
class EngravementOfSoulsTowers : Components.GenericTowers
{
    public bool CastsStarted { get; private set; }
    private BitMask _globallyForbidden; // these players can't close any of the towers due to vulns
    private BitMask _lightForbidden; // these players can't close light towers due to light debuff
    private BitMask _darkForbidden; // these players can't close light towers due to light debuff

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.UmbralTilt: // light guy can't close light tower
                _lightForbidden.Set(module.Raid.FindSlot(actor.InstanceID));
                break;
            case SID.AstralTilt: // dark guy can't close dark tower
                _darkForbidden.Set(module.Raid.FindSlot(actor.InstanceID));
                break;
            case SID.UmbralbrightSoul: // dropping a tower causes vuln
            case SID.AstralbrightSoul:
            case SID.HeavensflameSoul: // dropping a spread causes vuln
                _globallyForbidden.Set(module.Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.UmbralTilt: // light guy can't close light tower
                _lightForbidden.Clear(module.Raid.FindSlot(actor.InstanceID));
                break;
            case SID.AstralTilt: // dark guy can't close dark tower
                _darkForbidden.Clear(module.Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.UmbralAdvance:
                AddTower(module, spell.LocXZ, true, true);
                break;
            case AID.AstralAdvance:
                AddTower(module, spell.LocXZ, false, true);
                break;
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.UmbralGlow:
                AddTower(module, caster.Position, true, false);
                break;
            case AID.AstralGlow:
                AddTower(module, caster.Position, false, false);
                break;
            case AID.UmbralAdvance:
            case AID.AstralAdvance:
                ++NumCasts;
                Towers.Clear();
                break;
        }
    }

    private void AddTower(BossModule module, WPos pos, bool isLight, bool realCast)
    {
        if (realCast != CastsStarted)
        {
            if (realCast)
            {
                CastsStarted = true;
                Towers.Clear();
            }
            else
            {
                module.ReportError(this, "Unexpected predicted tower when real casts are in progress");
                return;
            }
        }

        Towers.Add(new(pos, 3, forbiddenSoakers: _globallyForbidden | (isLight ? _lightForbidden : _darkForbidden)));
    }
}
