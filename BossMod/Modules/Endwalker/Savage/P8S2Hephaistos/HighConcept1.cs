using System;
using System.Linq;

namespace BossMod.Endwalker.Savage.P8S2
{
    // note: this is currently tailored to strat my static uses...
    class HighConcept1 : BossComponent
    {
        public enum Mechanic { Explosion1, Towers1, Explosion2, Towers2, Done }
        public enum PlayerRole { Unassigned, ShortAlpha, ShortBeta, ShortGamma, LongAlpha, LongBeta, LongGamma, Stack2, Stack3, Count }
        public enum TowerColor { Unknown, Purple, Blue, Green }

        public Mechanic NextMechanic { get; private set; } = Mechanic.Explosion1;
        private PlayerRole[] _playerRoles = new PlayerRole[PartyState.MaxPartySize];
        private int[] _roleSlots;
        private int _numAssignedRoles;
        private TowerColor _firstTowers;
        private TowerColor _secondTowers;

        private static float _shiftRadius = 20;
        private static float _spliceRadius = 6;
        private static float _towerRadius = 3;

        public HighConcept1()
        {
            _roleSlots = new int[(int)PlayerRole.Count];
            Array.Fill(_roleSlots, -1);
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_numAssignedRoles < 8)
                return;

            // TODO: consider adding stack/spread-like hints

            var hint = (NextMechanic, _playerRoles[slot]) switch
            {
                (Mechanic.Explosion1, PlayerRole.ShortAlpha) => "A -> N or chill",
                (Mechanic.Explosion1, PlayerRole.ShortBeta) => "B -> adjust or chill",
                (Mechanic.Explosion1, PlayerRole.ShortGamma) => "C -> S or chill",
                (Mechanic.Explosion1, PlayerRole.LongAlpha) => "2 -> A -> N+N",
                (Mechanic.Explosion1, PlayerRole.LongBeta) => "3 -> B -> N+adjust",
                (Mechanic.Explosion1, PlayerRole.LongGamma) => "3 -> C -> N+S",
                (Mechanic.Explosion1, PlayerRole.Stack2) => "2 -> A/B -> S",
                (Mechanic.Explosion1, PlayerRole.Stack3) => "3 -> C/B -> S",

                (Mechanic.Towers1, PlayerRole.ShortAlpha) => _firstTowers switch
                {
                    TowerColor.Purple => "chill -> S+N",
                    TowerColor.Blue or TowerColor.Green => "N -> chill",
                    _ => "N or chill"
                },
                (Mechanic.Explosion2 or Mechanic.Towers2, PlayerRole.ShortAlpha) => _firstTowers == TowerColor.Purple ? "S+N" : "chill",

                (Mechanic.Towers1, PlayerRole.ShortBeta) => _firstTowers switch
                {
                    TowerColor.Purple => "N -> chill",
                    TowerColor.Blue => "chill -> S+adjust",
                    TowerColor.Green => "S -> chill",
                    _ => "adjust or chill"
                },
                (Mechanic.Explosion2, PlayerRole.ShortBeta) => _firstTowers == TowerColor.Blue ? "S+adjust" : "chill",
                (Mechanic.Towers2, PlayerRole.ShortBeta) => _firstTowers != TowerColor.Blue ? "chill" : _secondTowers switch
                {
                    TowerColor.Purple => "S+N",
                    TowerColor.Blue => "chill",
                    TowerColor.Green => "S+S",
                    _ => "S+adjust"
                },

                (Mechanic.Towers1, PlayerRole.ShortGamma) => _firstTowers switch
                {
                    TowerColor.Purple or TowerColor.Blue => "S -> chill",
                    TowerColor.Green => "chill -> S+S",
                    _ => "S or chill"
                },
                (Mechanic.Explosion2 or Mechanic.Towers2, PlayerRole.ShortGamma) => _firstTowers == TowerColor.Green ? "S+S" : "chill",

                (Mechanic.Towers1 or Mechanic.Explosion2, PlayerRole.LongAlpha) => "A -> N+N",
                (Mechanic.Towers2, PlayerRole.LongAlpha) => _secondTowers != TowerColor.Purple ? "N+N" : "chill",

                (Mechanic.Towers1 or Mechanic.Explosion2, PlayerRole.LongBeta) => "B -> N+adjust",
                (Mechanic.Towers2, PlayerRole.LongBeta) => _secondTowers switch
                {
                    TowerColor.Purple => "N+N",
                    TowerColor.Blue => "chill",
                    TowerColor.Green => "N+S",
                    _ => "N+adjust"
                },

                (Mechanic.Towers1 or Mechanic.Explosion2, PlayerRole.LongGamma) => "C -> N+S",
                (Mechanic.Towers2, PlayerRole.LongGamma) => _secondTowers != TowerColor.Green ? "N+S" : "chill",

                (Mechanic.Towers1 or Mechanic.Explosion2, PlayerRole.Stack2) => _firstTowers switch
                {
                    TowerColor.Purple => "B -> S+adjust",
                    TowerColor.Blue or TowerColor.Green => "A -> S+N",
                    _ => "A/B -> S"
                },
                (Mechanic.Towers2, PlayerRole.Stack2) => _firstTowers == TowerColor.Purple ? _secondTowers switch
                {
                    TowerColor.Purple => "S+N",
                    TowerColor.Blue => "chill",
                    TowerColor.Green => "S+S",
                    _ => "S+adjust"
                } : _secondTowers != TowerColor.Purple ? "S+N" : "chill",

                (Mechanic.Towers1 or Mechanic.Explosion2, PlayerRole.Stack3) => _firstTowers switch
                {
                    TowerColor.Purple or TowerColor.Blue => "C -> S+S",
                    TowerColor.Green => "B -> S+adjust",
                    _ => "C/B -> S"
                },
                (Mechanic.Towers2, PlayerRole.Stack3) => _firstTowers == TowerColor.Green ? _secondTowers switch
                {
                    TowerColor.Purple => "S+N",
                    TowerColor.Blue => "chill",
                    TowerColor.Green => "S+S",
                    _ => "S+adjust"
                } : _secondTowers != TowerColor.Green ? "S+S" : "chill",

                _ => ""
            };
            if (hint.Length > 0)
                hints.Add(hint, false);
        }

        public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        {
            return _numAssignedRoles < 8 ? PlayerPriority.Irrelevant : PlayerPriority.Normal;
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            var pcRole = _playerRoles[pcSlot];
            switch (NextMechanic)
            {
                case Mechanic.Explosion1:
                    if (_numAssignedRoles >= 8)
                    {
                        DrawExplosion(module, PlayerRole.ShortAlpha, _shiftRadius, pcRole == PlayerRole.ShortAlpha);
                        DrawExplosion(module, PlayerRole.ShortBeta, _shiftRadius, pcRole == PlayerRole.ShortBeta);
                        DrawExplosion(module, PlayerRole.ShortGamma, _shiftRadius, pcRole == PlayerRole.ShortGamma);
                        DrawExplosion(module, PlayerRole.Stack2, _spliceRadius, pcRole is PlayerRole.Stack2 or PlayerRole.LongAlpha);
                        DrawExplosion(module, PlayerRole.Stack3, _spliceRadius, pcRole is PlayerRole.Stack3 or PlayerRole.LongBeta or PlayerRole.LongGamma);
                    }
                    break;
                case Mechanic.Towers1:
                    if (_firstTowers != TowerColor.Unknown)
                    {
                        var (roleN, roleS) = _firstTowers switch
                        {
                            TowerColor.Purple => (PlayerRole.ShortBeta, PlayerRole.ShortGamma),
                            TowerColor.Blue => (PlayerRole.ShortAlpha, PlayerRole.ShortGamma),
                            TowerColor.Green => (PlayerRole.ShortAlpha, PlayerRole.ShortBeta),
                            _ => (PlayerRole.Unassigned, PlayerRole.Unassigned)
                        };
                        DrawTower(module, -10, pcRole == roleN);
                        DrawTower(module, +10, pcRole == roleS);
                        if (pcRole == roleN || pcRole == roleS)
                            DrawTether(module, roleN, roleS);
                    }
                    break;
                case Mechanic.Explosion2:
                    DrawExplosion(module, PlayerRole.LongAlpha, _shiftRadius, pcRole switch
                    {
                        PlayerRole.LongAlpha => true,
                        PlayerRole.Stack2 => _firstTowers != TowerColor.Purple,
                        _ => false
                    });
                    DrawExplosion(module, PlayerRole.LongBeta, _shiftRadius, pcRole switch
                    {
                        PlayerRole.LongBeta => true,
                        PlayerRole.Stack2 => _firstTowers == TowerColor.Purple,
                        PlayerRole.Stack3 => _firstTowers == TowerColor.Green,
                        _ => false
                    });
                    DrawExplosion(module, PlayerRole.LongGamma, _shiftRadius, pcRole switch
                    {
                        PlayerRole.LongGamma => true,
                        PlayerRole.Stack3 => _firstTowers != TowerColor.Green,
                        _ => false
                    });
                    break;
                case Mechanic.Towers2:
                    if (_secondTowers != TowerColor.Unknown)
                    {
                        var roleSA = _firstTowers == TowerColor.Purple ? PlayerRole.ShortAlpha : PlayerRole.Stack2;
                        var roleSB = _firstTowers switch
                        {
                            TowerColor.Purple => PlayerRole.Stack2,
                            TowerColor.Green => PlayerRole.Stack3,
                            _ => PlayerRole.ShortBeta
                        };
                        var roleSC = _firstTowers == TowerColor.Green ? PlayerRole.ShortGamma : PlayerRole.Stack3;
                        var (roleNN, roleNS, roleSN, roleSS) = _secondTowers switch
                        {
                            TowerColor.Purple => (PlayerRole.LongBeta, PlayerRole.LongGamma, roleSB, roleSC),
                            TowerColor.Blue => (PlayerRole.LongAlpha, PlayerRole.LongGamma, roleSA, roleSC),
                            TowerColor.Green => (PlayerRole.LongAlpha, PlayerRole.LongBeta, roleSA, roleSB),
                            _ => (PlayerRole.Unassigned, PlayerRole.Unassigned, PlayerRole.Unassigned, PlayerRole.Unassigned)
                        };
                        DrawTower(module, -15, pcRole == roleNN);
                        DrawTower(module, -5, pcRole == roleNS);
                        DrawTower(module, +5, pcRole == roleSN);
                        DrawTower(module, +15, pcRole == roleSS);
                        if (pcRole == roleNN || pcRole == roleNS)
                            DrawTether(module, roleNN, roleNS);
                        else if (pcRole == roleSN || pcRole == roleSS)
                            DrawTether(module, roleSN, roleSS);
                    }
                    break;
            }
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            var role = (SID)status.ID switch
            {
                SID.ImperfectionAlpha => (status.ExpireAt - module.WorldState.CurrentTime).TotalSeconds > 15 ? PlayerRole.LongAlpha : PlayerRole.ShortAlpha,
                SID.ImperfectionBeta  => (status.ExpireAt - module.WorldState.CurrentTime).TotalSeconds > 15 ? PlayerRole.LongBeta  : PlayerRole.ShortBeta,
                SID.ImperfectionGamma => (status.ExpireAt - module.WorldState.CurrentTime).TotalSeconds > 15 ? PlayerRole.LongGamma : PlayerRole.ShortGamma,
                SID.Multisplice => PlayerRole.Stack2,
                SID.Supersplice => PlayerRole.Stack3,
                _ => PlayerRole.Unassigned
            };

            if (role != PlayerRole.Unassigned)
            {
                ++_numAssignedRoles;
                var slot = module.Raid.FindSlot(actor.InstanceID);
                SlotForRole(role) = slot;
                if (slot >= 0)
                    _playerRoles[slot] = role;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.ConceptualShiftAlpha:
                case AID.ConceptualShiftBeta:
                case AID.ConceptualShiftGamma:
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

        public override void OnEventEnvControl(BossModule module, uint directorID, byte index, uint state)
        {
            if (directorID != 0x800375AB || state != 0x00020001)
                return;
            switch (index)
            {
                case 0x1E: case 0x1F:
                    _firstTowers = TowerColor.Purple;
                    break;
                case 0x28: case 0x29:
                    _firstTowers = TowerColor.Blue;
                    break;
                case 0x32: case 0x33:
                    _firstTowers = TowerColor.Green;
                    break;
                case 0x1A: case 0x1B: case 0x1C: case 0x1D:
                    _secondTowers = TowerColor.Purple;
                    break;
                case 0x24: case 0x25: case 0x26: case 0x27:
                    _secondTowers = TowerColor.Blue;
                    break;
                case 0x2E: case 0x2F: case 0x30: case 0x31:
                    _secondTowers = TowerColor.Green;
                    break;
            }
        }

        private ref int SlotForRole(PlayerRole r) => ref _roleSlots[(int)r];

        private void DrawExplosion(BossModule module, PlayerRole role, float radius, bool safe)
        {
            var source = module.Raid[SlotForRole(role)];
            if (source != null)
                module.Arena.AddCircle(source.Position, radius, safe ? ArenaColor.Safe : ArenaColor.Danger);
        }

        private void DrawTower(BossModule module, float offsetZ, bool assigned)
        {
            module.Arena.AddCircle(module.Bounds.Center + new WDir(0, offsetZ), _towerRadius, assigned ? ArenaColor.Safe : ArenaColor.Danger, 2);
        }

        private void DrawTether(BossModule module, PlayerRole role1, PlayerRole role2)
        {
            var a1 = module.Raid[SlotForRole(role1)];
            var a2 = module.Raid[SlotForRole(role2)];
            if (a1 != null && a2 != null)
                module.Arena.AddLine(a1.Position, a2.Position, ArenaColor.Safe);
        }
    }
}
