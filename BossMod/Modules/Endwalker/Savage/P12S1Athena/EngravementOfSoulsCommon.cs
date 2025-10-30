namespace BossMod.Endwalker.Savage.P12S1Athena;

class EngravementOfSoulsTethers(BossModule module) : Components.GenericBaitAway(module)
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

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (States[slot].TooClose)
            hints.Add("Stretch the tether!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        foreach (var b in CurrentBaits)
            Arena.Actor(b.Source, ArenaColor.Object, true);

        // TODO: consider drawing safespot based on configured strategy and mechanic order
        if (NumCasts == 0 && States[pcSlot] is var state && state.Source != null)
            Arena.AddLine(state.Source.Position, pc.Position, state.TooClose ? ArenaColor.Danger : ArenaColor.Safe);
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        var (type, tooClose) = (TetherID)tether.ID switch
        {
            TetherID.LightNear => (TetherType.Light, true),
            TetherID.DarkNear => (TetherType.Dark, true),
            TetherID.LightFar => (TetherType.Light, false),
            TetherID.DarkFar => (TetherType.Dark, false),
            _ => (TetherType.None, false)
        };
        if (type != TetherType.None && Raid.TryFindSlot(tether.Target, out var slot))
        {
            if (States[slot].Source == null)
            {
                States[slot] = new() { Source = source, Tether = type, TooClose = tooClose };
                CurrentBaits.Add(new(source, Raid[slot]!, _shape));
            }
            else if (States[slot].Source != source)
            {
                ReportError($"Multiple tethers on same player");
            }
            else if (States[slot].Tether != type)
            {
                ReportError($"Tether type changed");
            }
            else
            {
                States[slot].TooClose = tooClose;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.SearingRadiance or AID.Shadowsear)
        {
            ++NumCasts;
            CurrentBaits.RemoveAll(b => b.Source == caster);
        }
    }
}

// towers can not be soaked by same colored tilt
class EngravementOfSoulsTowers(BossModule module) : Components.GenericTowers(module)
{
    public bool CastsStarted { get; private set; }
    private BitMask _globallyForbidden; // these players can't close any of the towers due to vulns
    private BitMask _lightForbidden; // these players can't close light towers due to light debuff
    private BitMask _darkForbidden; // these players can't close light towers due to light debuff

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.UmbralTilt: // light guy can't close light tower
                _lightForbidden.Set(Raid.FindSlot(actor.InstanceID));
                break;
            case SID.AstralTilt: // dark guy can't close dark tower
                _darkForbidden.Set(Raid.FindSlot(actor.InstanceID));
                break;
            case SID.UmbralbrightSoul: // dropping a tower causes vuln
            case SID.AstralbrightSoul:
            case SID.HeavensflameSoul: // dropping a spread causes vuln
                _globallyForbidden.Set(Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.UmbralTilt: // light guy can't close light tower
                _lightForbidden.Clear(Raid.FindSlot(actor.InstanceID));
                break;
            case SID.AstralTilt: // dark guy can't close dark tower
                _darkForbidden.Clear(Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.UmbralAdvance:
                AddTower(spell.LocXZ, true, true);
                break;
            case AID.AstralAdvance:
                AddTower(spell.LocXZ, false, true);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.UmbralGlow:
                AddTower(caster.Position, true, false);
                break;
            case AID.AstralGlow:
                AddTower(caster.Position, false, false);
                break;
            case AID.UmbralAdvance:
            case AID.AstralAdvance:
                ++NumCasts;
                Towers.Clear();
                break;
        }
    }

    private void AddTower(WPos pos, bool isLight, bool realCast)
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
                ReportError("Unexpected predicted tower when real casts are in progress");
                return;
            }
        }

        Towers.Add(new(pos, 3, forbiddenSoakers: _globallyForbidden | (isLight ? _lightForbidden : _darkForbidden)));
    }
}
