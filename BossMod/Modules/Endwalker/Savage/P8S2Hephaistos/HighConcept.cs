namespace BossMod.Endwalker.Savage.P8S2;

// note: this is currently tailored to strat my static uses...
class HighConceptCommon(BossModule module) : BossComponent(module)
{
    public enum Mechanic { Explosion1, Towers1, Explosion2, Towers2, Done }
    public enum PlayerRole { Unassigned, Stack1, Stack2, Stack3, ShortAlpha, ShortBeta, ShortGamma, LongAlpha, LongBeta, LongGamma, Count }
    public enum TowerColor { Unknown, Purple, Blue, Green }

    public Mechanic NextMechanic { get; private set; } = Mechanic.Explosion1;
    protected TowerColor FirstTowers { get; private set; } // if assigned - two towers of same color at (0, +-10)
    protected TowerColor SecondTowersHC1 { get; private set; } // if assigned - four towers of same color at (0, +-5/15)
    protected List<(WPos p, TowerColor c)> SecondTowersHC2 { get; private set; } = [];
    protected int NumAssignedRoles { get; private set; }
    private readonly int[] _roleSlots = Utils.MakeArray((int)PlayerRole.Count, -1);
    private readonly PlayerRole[] _playerRoles = new PlayerRole[PartyState.MaxPartySize]; // for HC2, this doesn't have stack roles, since players also have long letters

    protected int SlotForRole(PlayerRole r) => _roleSlots[(int)r];
    protected PlayerRole RoleForSlot(int slot) => _playerRoles[slot];

    protected const float ShiftRadius = 20;
    protected const float SpliceRadius = 6;
    protected const float TowerRadius = 3;

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => NumAssignedRoles < 8 ? PlayerPriority.Irrelevant : PlayerPriority.Normal;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var role = (SID)status.ID switch
        {
            SID.ImperfectionAlpha => (status.ExpireAt - WorldState.CurrentTime).TotalSeconds > 15 ? PlayerRole.LongAlpha : PlayerRole.ShortAlpha,
            SID.ImperfectionBeta => (status.ExpireAt - WorldState.CurrentTime).TotalSeconds > 15 ? PlayerRole.LongBeta : PlayerRole.ShortBeta,
            SID.ImperfectionGamma => (status.ExpireAt - WorldState.CurrentTime).TotalSeconds > 15 ? PlayerRole.LongGamma : PlayerRole.ShortGamma,
            SID.Solosplice => PlayerRole.Stack1,
            SID.Multisplice => PlayerRole.Stack2,
            SID.Supersplice => PlayerRole.Stack3,
            _ => PlayerRole.Unassigned
        };

        if (role != PlayerRole.Unassigned)
        {
            ++NumAssignedRoles;
            if (Raid.TryFindSlot(actor, out var slot) && _playerRoles[slot] < role) // priority order: letters > stacks (important for HC2)
                _playerRoles[slot] = role;

            _roleSlots[(int)role] = slot;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ConceptualShiftAlpha:
            case AID.ConceptualShiftBeta:
            case AID.ConceptualShiftGamma:
            case AID.Splicer1:
            case AID.Splicer2:
            case AID.Splicer3:
                if (NextMechanic is Mechanic.Explosion1 or Mechanic.Explosion2)
                    ++NextMechanic;
                break;
            case AID.ArcaneChannel:
                if (NextMechanic is Mechanic.Towers1 or Mechanic.Towers2)
                    ++NextMechanic;
                break;
        }
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state != 0x00020001)
            return;

        var firstColor = index switch
        {
            0x1E or 0x1F => TowerColor.Purple,
            0x28 or 0x29 => TowerColor.Blue,
            0x32 or 0x33 => TowerColor.Green,
            _ => TowerColor.Unknown
        };
        if (firstColor != TowerColor.Unknown)
            FirstTowers = firstColor;

        var secondColorHC1 = index switch
        {
            0x1A or 0x1B or 0x1C or 0x1D => TowerColor.Purple,
            0x24 or 0x25 or 0x26 or 0x27 => TowerColor.Blue,
            0x2E or 0x2F or 0x30 or 0x31 => TowerColor.Green,
            _ => TowerColor.Unknown
        };
        if (secondColorHC1 != TowerColor.Unknown)
            SecondTowersHC1 = secondColorHC1;

        var (secondColorHC2, secondOffsetHC2) = index switch
        {
            0x20 => (TowerColor.Purple, new(-5, -5)),
            0x21 => (TowerColor.Purple, new(+5, -5)),
            0x22 => (TowerColor.Purple, new(-5, +5)),
            0x23 => (TowerColor.Purple, new(+5, +5)),
            0x2A => (TowerColor.Blue, new(-5, -5)),
            0x2B => (TowerColor.Blue, new(+5, -5)),
            0x2C => (TowerColor.Blue, new(-5, +5)),
            0x2D => (TowerColor.Blue, new(+5, +5)),
            0x34 => (TowerColor.Green, new(-5, -5)),
            0x35 => (TowerColor.Green, new(+5, -5)),
            0x36 => (TowerColor.Green, new(-5, +5)),
            0x37 => (TowerColor.Green, new(+5, +5)),
            _ => (TowerColor.Unknown, new WDir())
        };
        if (secondColorHC2 != TowerColor.Unknown)
            SecondTowersHC2.Add((Module.Center + secondOffsetHC2, secondColorHC2));
    }

    protected void DrawExplosion(int slot, float radius, bool safe)
    {
        var source = Raid[slot];
        if (source != null)
            Arena.AddCircle(source.Position, radius, safe ? ArenaColor.Safe : ArenaColor.Danger);
    }

    protected void DrawTower(WPos pos, bool assigned) => Arena.AddCircle(pos, TowerRadius, assigned ? ArenaColor.Safe : ArenaColor.Danger, 2);
    protected void DrawTower(float offsetZ, bool assigned) => DrawTower(Module.Center + new WDir(0, offsetZ), assigned);

    protected void DrawTether(int slot1, int slot2, int pcSlot)
    {
        var a1 = Raid[slot1];
        var a2 = Raid[slot2];
        if (a1 != null && a2 != null)
            Arena.AddLine(a1.Position, a2.Position, pcSlot == slot1 || pcSlot == slot2 ? ArenaColor.Safe : ArenaColor.Danger);
    }
}

class HighConcept1(BossModule module) : HighConceptCommon(module)
{
    public bool LongGoS => Service.Config.Get<P8S2Config>().HC1LongGoS;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (NumAssignedRoles < 8)
            return;

        // TODO: consider adding stack/spread-like hints

        var (shortTowers, longTowers) = LongGoS ? ("N", "S") : ("S", "N");
        var hint = (NextMechanic, RoleForSlot(slot)) switch
        {
            (Mechanic.Explosion1, PlayerRole.ShortAlpha) => "A -> N or chill",
            (Mechanic.Explosion1, PlayerRole.ShortBeta) => "B -> adjust or chill",
            (Mechanic.Explosion1, PlayerRole.ShortGamma) => "C -> S or chill",
            (Mechanic.Explosion1, PlayerRole.LongAlpha) => $"2 -> A -> {longTowers}+N",
            (Mechanic.Explosion1, PlayerRole.LongBeta) => $"3 -> B -> {longTowers}+adjust",
            (Mechanic.Explosion1, PlayerRole.LongGamma) => $"3 -> C -> {longTowers}+S",
            (Mechanic.Explosion1, PlayerRole.Stack2) => $"2 -> A/B -> {shortTowers}",
            (Mechanic.Explosion1, PlayerRole.Stack3) => $"3 -> C/B -> {shortTowers}",

            (Mechanic.Towers1, PlayerRole.ShortAlpha) => FirstTowers switch
            {
                TowerColor.Purple => $"chill -> {shortTowers}+N",
                TowerColor.Blue or TowerColor.Green => "N -> chill",
                _ => "N or chill"
            },
            (Mechanic.Explosion2 or Mechanic.Towers2, PlayerRole.ShortAlpha) => FirstTowers == TowerColor.Purple && SecondTowersHC1 != TowerColor.Purple ? $"{shortTowers}+N" : "chill",

            (Mechanic.Towers1, PlayerRole.ShortBeta) => FirstTowers switch
            {
                TowerColor.Purple => "N -> chill",
                TowerColor.Blue => $"chill -> {shortTowers}+adjust",
                TowerColor.Green => "S -> chill",
                _ => "adjust or chill"
            },
            (Mechanic.Explosion2, PlayerRole.ShortBeta) => FirstTowers == TowerColor.Blue ? $"{shortTowers}+adjust" : "chill",
            (Mechanic.Towers2, PlayerRole.ShortBeta) => FirstTowers != TowerColor.Blue ? "chill" : SecondTowersHC1 switch
            {
                TowerColor.Purple => $"{shortTowers}+N",
                TowerColor.Blue => "chill",
                TowerColor.Green => $"{shortTowers}+S",
                _ => $"{shortTowers}+adjust"
            },

            (Mechanic.Towers1, PlayerRole.ShortGamma) => FirstTowers switch
            {
                TowerColor.Purple or TowerColor.Blue => "S -> chill",
                TowerColor.Green => $"chill -> {shortTowers}+S",
                _ => "S or chill"
            },
            (Mechanic.Explosion2 or Mechanic.Towers2, PlayerRole.ShortGamma) => FirstTowers == TowerColor.Green && SecondTowersHC1 != TowerColor.Green ? $"{shortTowers}+S" : "chill",

            (Mechanic.Towers1 or Mechanic.Explosion2, PlayerRole.LongAlpha) => $"A -> {longTowers}+N",
            (Mechanic.Towers2, PlayerRole.LongAlpha) => SecondTowersHC1 != TowerColor.Purple ? $"{longTowers}+N" : "chill",

            (Mechanic.Towers1 or Mechanic.Explosion2, PlayerRole.LongBeta) => $"B -> {longTowers}+adjust",
            (Mechanic.Towers2, PlayerRole.LongBeta) => SecondTowersHC1 switch
            {
                TowerColor.Purple => $"{longTowers}+N",
                TowerColor.Blue => "chill",
                TowerColor.Green => $"{longTowers}+S",
                _ => $"{longTowers}+adjust"
            },

            (Mechanic.Towers1 or Mechanic.Explosion2, PlayerRole.LongGamma) => $"C -> {longTowers}+S",
            (Mechanic.Towers2, PlayerRole.LongGamma) => SecondTowersHC1 != TowerColor.Green ? $"{longTowers}+S" : "chill",

            (Mechanic.Towers1 or Mechanic.Explosion2, PlayerRole.Stack2) => FirstTowers switch
            {
                TowerColor.Purple => $"B -> {shortTowers}+adjust",
                TowerColor.Blue or TowerColor.Green => $"A -> {shortTowers}+N",
                _ => $"A/B -> {shortTowers}"
            },
            (Mechanic.Towers2, PlayerRole.Stack2) => FirstTowers == TowerColor.Purple ? SecondTowersHC1 switch
            {
                TowerColor.Purple => $"{shortTowers}+N",
                TowerColor.Blue => "chill",
                TowerColor.Green => $"{shortTowers}+S",
                _ => $"{shortTowers}+adjust"
            } : SecondTowersHC1 != TowerColor.Purple ? $"{shortTowers}+N" : "chill",

            (Mechanic.Towers1 or Mechanic.Explosion2, PlayerRole.Stack3) => FirstTowers switch
            {
                TowerColor.Purple or TowerColor.Blue => $"C -> {shortTowers}+S",
                TowerColor.Green => $"B -> {shortTowers}+adjust",
                _ => $"C/B -> {shortTowers}"
            },
            (Mechanic.Towers2, PlayerRole.Stack3) => FirstTowers == TowerColor.Green ? SecondTowersHC1 switch
            {
                TowerColor.Purple => $"{shortTowers}+N",
                TowerColor.Blue => "chill",
                TowerColor.Green => $"{shortTowers}+S",
                _ => $"{shortTowers}+adjust"
            } : SecondTowersHC1 != TowerColor.Green ? $"{shortTowers}+S" : "chill",

            _ => ""
        };
        if (hint.Length > 0)
            hints.Add(hint, false);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var pcRole = RoleForSlot(pcSlot);
        switch (NextMechanic)
        {
            case Mechanic.Explosion1:
                if (NumAssignedRoles >= 8)
                {
                    DrawExplosion(SlotForRole(PlayerRole.ShortAlpha), ShiftRadius, pcRole == PlayerRole.ShortAlpha);
                    DrawExplosion(SlotForRole(PlayerRole.ShortBeta), ShiftRadius, pcRole == PlayerRole.ShortBeta);
                    DrawExplosion(SlotForRole(PlayerRole.ShortGamma), ShiftRadius, pcRole == PlayerRole.ShortGamma);
                    DrawExplosion(SlotForRole(PlayerRole.Stack2), SpliceRadius, pcRole is PlayerRole.Stack2 or PlayerRole.LongAlpha);
                    DrawExplosion(SlotForRole(PlayerRole.Stack3), SpliceRadius, pcRole is PlayerRole.Stack3 or PlayerRole.LongBeta or PlayerRole.LongGamma);
                }
                break;
            case Mechanic.Towers1:
                if (FirstTowers != TowerColor.Unknown)
                {
                    var (roleN, roleS) = FirstTowers switch
                    {
                        TowerColor.Purple => (PlayerRole.ShortBeta, PlayerRole.ShortGamma),
                        TowerColor.Blue => (PlayerRole.ShortAlpha, PlayerRole.ShortGamma),
                        TowerColor.Green => (PlayerRole.ShortAlpha, PlayerRole.ShortBeta),
                        _ => (PlayerRole.Unassigned, PlayerRole.Unassigned)
                    };
                    DrawTower(-10, pcRole == roleN);
                    DrawTower(+10, pcRole == roleS);
                    DrawTether(SlotForRole(roleN), SlotForRole(roleS), pcSlot);
                }
                break;
            case Mechanic.Explosion2:
                DrawExplosion(SlotForRole(PlayerRole.LongAlpha), ShiftRadius, pcRole switch
                {
                    PlayerRole.LongAlpha => true,
                    PlayerRole.Stack2 => FirstTowers != TowerColor.Purple,
                    _ => false
                });
                DrawExplosion(SlotForRole(PlayerRole.LongBeta), ShiftRadius, pcRole switch
                {
                    PlayerRole.LongBeta => true,
                    PlayerRole.Stack2 => FirstTowers == TowerColor.Purple,
                    PlayerRole.Stack3 => FirstTowers == TowerColor.Green,
                    _ => false
                });
                DrawExplosion(SlotForRole(PlayerRole.LongGamma), ShiftRadius, pcRole switch
                {
                    PlayerRole.LongGamma => true,
                    PlayerRole.Stack3 => FirstTowers != TowerColor.Green,
                    _ => false
                });
                break;
            case Mechanic.Towers2:
                if (SecondTowersHC1 != TowerColor.Unknown)
                {
                    var roleSA = FirstTowers == TowerColor.Purple ? PlayerRole.ShortAlpha : PlayerRole.Stack2;
                    var roleSB = FirstTowers switch
                    {
                        TowerColor.Purple => PlayerRole.Stack2,
                        TowerColor.Green => PlayerRole.Stack3,
                        _ => PlayerRole.ShortBeta
                    };
                    var roleSC = FirstTowers == TowerColor.Green ? PlayerRole.ShortGamma : PlayerRole.Stack3;
                    var (roleLN, roleLS, roleSN, roleSS) = SecondTowersHC1 switch
                    {
                        TowerColor.Purple => (PlayerRole.LongBeta, PlayerRole.LongGamma, roleSB, roleSC),
                        TowerColor.Blue => (PlayerRole.LongAlpha, PlayerRole.LongGamma, roleSA, roleSC),
                        TowerColor.Green => (PlayerRole.LongAlpha, PlayerRole.LongBeta, roleSA, roleSB),
                        _ => (PlayerRole.Unassigned, PlayerRole.Unassigned, PlayerRole.Unassigned, PlayerRole.Unassigned)
                    };
                    var (sOff, lOff) = LongGoS ? (-10, +10) : (+10, -10);
                    DrawTower(lOff - 5, pcRole == roleLN);
                    DrawTower(lOff + 5, pcRole == roleLS);
                    DrawTower(sOff - 5, pcRole == roleSN);
                    DrawTower(sOff + 5, pcRole == roleSS);
                    DrawTether(SlotForRole(roleLN), SlotForRole(roleLS), pcSlot);
                    DrawTether(SlotForRole(roleSN), SlotForRole(roleSS), pcSlot);
                }
                break;
        }
    }
}

class HighConcept2(BossModule module) : HighConceptCommon(module)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (NumAssignedRoles < 8)
            return;

        // TODO: consider adding stack/spread-like hints

        var hint = (NextMechanic, RoleForSlot(slot)) switch
        {
            (Mechanic.Explosion1, PlayerRole.ShortAlpha) => "A -> N or chill",
            (Mechanic.Explosion1, PlayerRole.ShortBeta) => "B -> adjust or chill",
            (Mechanic.Explosion1, PlayerRole.ShortGamma) => "C -> S or chill",
            (Mechanic.Explosion1, PlayerRole.LongAlpha) => SlotForRole(PlayerRole.Stack1) == slot ? "1 -> A -> E towers" : "2 -> A -> E towers",
            (Mechanic.Explosion1, PlayerRole.LongBeta) => SlotForRole(PlayerRole.Stack1) == slot ? "1 -> B -> E towers" : "2 -> B -> E towers",
            (Mechanic.Explosion1, PlayerRole.LongGamma) => SlotForRole(PlayerRole.Stack1) == slot ? "1 -> C -> W towers" : "2 -> C -> W towers",
            (Mechanic.Explosion1, PlayerRole.Unassigned) => "A -> merge -> drag E/W",

            (Mechanic.Towers1, PlayerRole.ShortAlpha) => FirstTowers switch
            {
                TowerColor.Purple => "chill -> W towers",
                TowerColor.Blue or TowerColor.Green => "N -> drag N",
                _ => "N or chill"
            },
            (Mechanic.Explosion2 or Mechanic.Towers2, PlayerRole.ShortAlpha) => FirstTowers == TowerColor.Purple ? "W towers" : "drag N",

            (Mechanic.Towers1, PlayerRole.ShortBeta) => FirstTowers switch
            {
                TowerColor.Purple => "N -> drag N",
                TowerColor.Blue => "chill -> W towers",
                TowerColor.Green => "S -> drag S",
                _ => "adjust or chill"
            },
            (Mechanic.Explosion2 or Mechanic.Towers2, PlayerRole.ShortBeta) => FirstTowers switch
            {
                TowerColor.Purple => "drag N",
                TowerColor.Green => "drag S",
                _ => "W towers"
            },

            (Mechanic.Towers1, PlayerRole.ShortGamma) => FirstTowers switch
            {
                TowerColor.Purple or TowerColor.Blue => "S -> drag S",
                TowerColor.Green => "chill -> W towers",
                _ => "S or chill"
            },
            (Mechanic.Explosion2 or Mechanic.Towers2, PlayerRole.ShortGamma) => FirstTowers == TowerColor.Green ? "W towers" : "drag S",

            (Mechanic.Towers1 or Mechanic.Explosion2, PlayerRole.LongAlpha) => "A -> E towers",
            (Mechanic.Towers1 or Mechanic.Explosion2, PlayerRole.LongBeta) => "B -> E towers",
            (Mechanic.Towers1 or Mechanic.Explosion2, PlayerRole.LongGamma) => "C -> W towers",

            (Mechanic.Towers2, PlayerRole.LongAlpha or PlayerRole.LongBeta) => "E towers",
            (Mechanic.Towers2, PlayerRole.LongGamma) => "W towers",

            (Mechanic.Towers1 or Mechanic.Explosion2 or Mechanic.Towers2, PlayerRole.Unassigned) => "merge -> drag E/W",

            _ => ""
        };
        if (hint.Length > 0)
            hints.Add(hint, false);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var pcRole = RoleForSlot(pcSlot);
        switch (NextMechanic)
        {
            case Mechanic.Explosion1:
                if (NumAssignedRoles >= 8)
                {
                    DrawExplosion(SlotForRole(PlayerRole.ShortAlpha), ShiftRadius, pcRole is PlayerRole.ShortAlpha or PlayerRole.Unassigned);
                    DrawExplosion(SlotForRole(PlayerRole.ShortBeta), ShiftRadius, pcRole == PlayerRole.ShortBeta);
                    DrawExplosion(SlotForRole(PlayerRole.ShortGamma), ShiftRadius, pcRole == PlayerRole.ShortGamma);
                    DrawExplosion(SlotForRole(PlayerRole.Stack1), SpliceRadius, pcRole is PlayerRole.LongAlpha or PlayerRole.LongBeta or PlayerRole.LongGamma && SlotForRole(PlayerRole.Stack1) == pcSlot);
                    DrawExplosion(SlotForRole(PlayerRole.Stack2), SpliceRadius, pcRole is PlayerRole.LongAlpha or PlayerRole.LongBeta or PlayerRole.LongGamma && SlotForRole(PlayerRole.Stack1) != pcSlot);
                }
                break;
            case Mechanic.Towers1:
                if (FirstTowers != TowerColor.Unknown)
                {
                    var (roleN, roleS) = FirstTowers switch
                    {
                        TowerColor.Purple => (PlayerRole.ShortBeta, PlayerRole.ShortGamma),
                        TowerColor.Blue => (PlayerRole.ShortAlpha, PlayerRole.ShortGamma),
                        TowerColor.Green => (PlayerRole.ShortAlpha, PlayerRole.ShortBeta),
                        _ => (PlayerRole.Unassigned, PlayerRole.Unassigned)
                    };
                    DrawTower(-10, pcRole == roleN);
                    DrawTower(+10, pcRole == roleS);
                    DrawTether(SlotForRole(roleN), SlotForRole(roleS), pcSlot);
                }
                break;
            case Mechanic.Explosion2:
                DrawExplosion(SlotForRole(PlayerRole.LongAlpha), ShiftRadius, pcRole == PlayerRole.LongAlpha);
                DrawExplosion(SlotForRole(PlayerRole.LongBeta), ShiftRadius, pcRole == PlayerRole.LongBeta);
                DrawExplosion(SlotForRole(PlayerRole.LongGamma), ShiftRadius, pcRole == PlayerRole.LongGamma);
                break;
            case Mechanic.Towers2:
                var roleE1 = PlayerRole.LongAlpha;
                var roleE2 = PlayerRole.LongBeta;
                var roleW1 = PlayerRole.LongGamma;
                var roleW2 = FirstTowers == TowerColor.Purple ? PlayerRole.ShortAlpha : PlayerRole.ShortBeta; // short gamma always merges, since first towers can't be green
                foreach (var t in SecondTowersHC2)
                {
                    var (role1, role2) = t.c == TowerColor.Green ? (roleE1, roleE2) : (roleW1, roleW2);
                    DrawTower(t.p, pcRole == role1 || pcRole == role2);
                }
                DrawTether(SlotForRole(roleE1), SlotForRole(roleE2), pcSlot);
                DrawTether(SlotForRole(roleW1), SlotForRole(roleW2), pcSlot);
                break;
        }
    }
}
