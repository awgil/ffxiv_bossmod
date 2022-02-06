using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BossMod
{
    public class P4S : BossModule
    {
        public enum OID : uint
        {
            Boss1 = 0x35FD, // first phase boss
            Pinax = 0x35FE, // '', 4x exist at start at [90/110, 90/110]
            Orb = 0x35FF, // orbs spawned by Belone Bursts
            Boss2 = 0x3600, // second phase boss (exists from start, but recreated on checkpoint)
            Akantha = 0x3601, // ?? 'akantha', 12 exist at start
            Helper = 0x233C, // 38 exist at start
        };

        public enum AID : uint
        {
            ElegantEviscerationSecond = 26649, // Boss->target, no cast, second hit
            SettingTheScene = 27083, // Boss->Boss
            ShiftStart = 27086, // Boss->Boss, no cast, tp to center, sword glowing (?)
            Pinax = 27087, // Boss->Boss
            MekhaneAcid = 27088, // Helper->target, no cast
            MekhaneLava = 27089, // Helper->target, no cast
            MekhaneWell = 27090, // Helper->Helper, no cast, affects arena
            MekhaneLevinstrike = 27091, // Helper->Helper, no cast, affects arena
            PinaxAcid = 27092, // Helper->Helper, affects corner
            PinaxLava = 27093, // Helper->Helper, affects corner
            PinaxWell = 27094, // Helper->Helper, affects corner
            PinaxLevinstrike = 27095, // Helper->Helper, affects corner
            Bloodrake = 27096, // Boss->Boss
            BeloneBursts = 27097, // Boss->Boss
            BeloneBurstsAOETank = 27098, // Orb->target, no cast
            BeloneBurstsAOEHealer = 27099, // Orb->target, no cast
            BeloneBurstsAOEDPS = 27100, // Orb->target, no cast
            BeloneCoils = 27101, // Boss->Boss
            BeloneCoilsDPS = 27102, // Helper->Helper, role towers ('no heals/tanks' variant)
            BeloneCoilsTH = 27103, // Helper->Helper, role towers ('no dps' variant)
            DirectorsBelone = 27110, // Boss->Boss
            DirectorsBeloneDebuffs = 27111, // Helper->target, no cast, just applies Role Call debuffs
            CursedCasting = 27113, // Helper->target, no cast, does something bad if no role call?..
            AethericChlamys = 27116, // Boss->Boss
            InversiveChlamys = 27117, // Boss->Boss
            InversiveChlamysAOE = 27119, // Helper->target, no cast, damage to tethered targets
            ElementalBelone = 27122, // Boss->Boss
            Periaktoi = 27124, // Boss->Boss
            PeriaktoiSafeAcid = 27125, // Helper->Helper (unconfirmed)
            PeriaktoiSafeLava = 27126, // Helper->Helper
            PeriaktoiSafeWell = 27127, // Helper->Helper
            PeriaktoiSafeLevinstrike = 27128, // Helper->Helper
            PeriaktoiDangerAcid = 27129, // Helper->Helper
            PeriaktoiDangerLava = 27130, // Helper->Helper
            PeriaktoiDangerWell = 27131, // Helper->Helper
            PeriaktoiDangerLevinstrike = 27132, // Helper->Helper
            NortherlyShiftCloak = 27133, // Boss->Boss
            SoutherlyShiftCloak = 27134, // Boss->Boss (unconfirmed)
            EasterlyShiftCloak = 27135, // Boss->Boss (unconfirmed)
            WesterlyShiftCloak = 27136, // Boss->Boss (unconfirmed)
            ShiftingStrikeCloak = 27137, // Helper->Helper
            NortherlyShiftSword = 27138, // Boss->Boss
            SoutherlyShiftSword = 27139, // Boss->Boss
            EasterlyShiftSword = 27140, // Boss->Boss
            WesterlyShiftSword = 27141, // Boss->Boss (unconfirmed)
            ShiftingStrikeSword = 27142, // Helper->Helper, sword attack
            ElegantEvisceration = 27144, // Boss->target
            Decollation = 27145, // Boss->Boss
            VengefulBelone = 28194, // Boss->Boss
            InversiveChlamysAOE2 = 28437, // Helper->target, no cast, damage to tethered targets (during belone coils)
        };

        public enum SID : uint
        {
            OrbRole = 2056,
            ThriceComeRuin = 2530,
            RoleCall = 2802,
            Miscast = 2803,
            ActingDPS = 2925,
            ActingHealer = 2926,
            ActingTank = 2927,
        }

        public enum TetherID : uint
        {
            ExplosiveAether = 17,
            Chlamys = 89,
            Bloodrake = 165,
        }

        // state related to director's belone (debuffs & tethers) mechanic
        private class DirectorsBelone : Component
        {
            public enum State { Inactive, TethersAssigned, DebuffsAssigned, Chlamys, Resolve }

            public State CurState = State.Inactive;

            private P4S _module;
            private ulong _tetherForbidden = 0;
            private ulong _tetherTargets = 0;
            private ulong _tetherInAOE = 0;
            private ulong _debuffForbidden = 0;
            private ulong _debuffTargets = 0;
            private ulong _debuffImmune = 0;

            private static float _debuffPassRange = 3; // not sure about this...
            private static float _tetherStackRange = 3; // this is arbitrary
            private static float _aoeRange = 5;

            public DirectorsBelone(P4S module)
            {
                _module = module;
            }

            public void AssignTethersFromBloodrake()
            {
                _tetherForbidden = BuildMask(_module.IterateRaidMembersWhere(actor => actor.Tether.ID == (uint)TetherID.Bloodrake));
                CurState = State.TethersAssigned;
            }

            public void AssignDebuffsFromBloodrake()
            {
                _debuffForbidden = BuildMask(_module.IterateRaidMembersWhere(actor => actor.Tether.ID == (uint)TetherID.Bloodrake));
                CurState = State.DebuffsAssigned;
            }

            public override void Reset()
            {
                CurState = State.Inactive;
                _tetherForbidden = _debuffForbidden = _debuffTargets = _debuffImmune = 0;
            }

            public override void Update()
            {
                _tetherTargets = _tetherInAOE = 0;
                switch (CurState)
                {
                    case State.Chlamys:
                    case State.Resolve:
                        foreach ((int i, var player) in _module.IterateRaidMembersWhere(actor => actor.Tether.ID == (uint)TetherID.Chlamys))
                        {
                            BitVector.SetVector64Bit(ref _tetherTargets, i);
                            _tetherInAOE |= _module.FindRaidMembersInRange(i, _aoeRange);
                        }
                        if (_tetherTargets == 0 && CurState == State.Resolve)
                            Reset();
                        break;
                }
            }

            public override void AddHints(int slot, WorldState.Actor actor, TextHints hints, MovementHints? movementHints)
            {
                if (CurState == State.Inactive)
                    return;

                if (_debuffForbidden != 0)
                {
                    if (!BitVector.IsVector64BitSet(_debuffForbidden, slot))
                    {
                        // we should be grabbing debuff
                        if (_debuffTargets == 0)
                        {
                            // debuffs not assigned yet => spread and prepare to grab
                            bool stacked = _module.IterateRaidMembersInRange(slot, _debuffPassRange).Any();
                            hints.Add("Debuffs: spread and prepare to handle!", stacked);
                        }
                        else if (BitVector.IsVector64BitSet(_debuffImmune, slot))
                        {
                            hints.Add("Debuffs: failed to handle");
                        }
                        else if (BitVector.IsVector64BitSet(_debuffTargets, slot))
                        {
                            hints.Add("Debuffs: OK", false);
                        }
                        else
                        {
                            hints.Add("Debuffs: grab!");
                        }
                    }
                    else
                    {
                        // we should be passing debuff
                        if (_debuffTargets == 0)
                        {
                            bool badStack = _module.IterateRaidMembers().Where(ip => ip.Item1 != slot && BitVector.IsVector64BitSet(_debuffForbidden, ip.Item1) && !GeometryUtils.PointInCircle(actor.Position - ip.Item2.Position, _debuffPassRange)).Any();
                            hints.Add("Debuffs: stack and prepare to pass!", badStack);
                        }
                        else if (BitVector.IsVector64BitSet(_debuffTargets, slot))
                        {
                            hints.Add("Debuffs: pass!");
                        }
                        else
                        {
                            hints.Add("Debuffs: avoid", false);
                        }
                    }
                }

                if (_tetherForbidden != 0)
                {
                    if (!BitVector.IsVector64BitSet(_tetherForbidden, slot))
                    {
                        // we should be grabbing tethers
                        if (_tetherTargets == 0)
                        {
                            // no tethers yet => stack and prepare to intercept
                            if (_debuffTargets == 0 || (_debuffTargets & _debuffForbidden) != 0)
                            {
                                // we're not done with debuffs yet, ignore tethers for now...
                                hints.Add("Tethers: prepare to intercept", false);
                            }
                            else
                            {
                                bool badStack = _module.IterateRaidMembers().Where(ip => ip.Item1 != slot && !BitVector.IsVector64BitSet(_tetherForbidden, ip.Item1) && !GeometryUtils.PointInCircle(actor.Position - ip.Item2.Position, _tetherStackRange)).Any();
                                hints.Add("Tethers: stack and prepare to intercept!", badStack);
                            }
                        }
                        else if (!BitVector.IsVector64BitSet(_tetherTargets, slot))
                        {
                            hints.Add("Tethers: intercept!");
                        }
                        else if (_module.IterateRaidMembersInRange(slot, _aoeRange).Any())
                        {
                            hints.Add("Tethers: GTFO from others!");
                        }
                        else
                        {
                            hints.Add("Tethers: OK", false);
                        }
                    }
                    else
                    {
                        // we should be passing tethers
                        if (_tetherTargets == 0)
                        {
                            // no tethers yet => stack and prepare to pass
                            if (_debuffTargets == 0 || (_debuffTargets & _debuffForbidden) != 0)
                            {
                                // we're not done with debuffs yet, ignore tethers for now...
                                hints.Add("Tethers: prepare to pass", false);
                            }
                            else
                            {
                                bool badStack = _module.IterateRaidMembers().Where(ip => ip.Item1 != slot && BitVector.IsVector64BitSet(_tetherForbidden, ip.Item1) && !GeometryUtils.PointInCircle(actor.Position - ip.Item2.Position, _tetherStackRange)).Any();
                                hints.Add("Tethers: stack and prepare to pass!", badStack);
                            }
                        }
                        else if (BitVector.IsVector64BitSet(_tetherTargets, slot))
                        {
                            hints.Add("Tethers: pass!");
                        }
                        else if (BitVector.IsVector64BitSet(_tetherInAOE, slot))
                        {
                            hints.Add("Tethers: GTFO from aoe!");
                        }
                        else
                        {
                            hints.Add("Tethers: avoid", false);
                        }
                    }
                }
            }

            public override void DrawArenaForeground(MiniArena arena)
            {
                if (CurState == State.Inactive)
                    return;

                var boss = _module.Boss1();
                ulong failingPlayers = (_debuffForbidden & _debuffTargets) | (_tetherForbidden & _tetherTargets);
                foreach ((int i, var player) in _module.IterateRaidMembers())
                {
                    bool failing = BitVector.IsVector64BitSet(failingPlayers, i);
                    bool inAOE = BitVector.IsVector64BitSet(_tetherInAOE, i);
                    arena.Actor(player, failing ? arena.ColorDanger : (inAOE ? arena.ColorPlayerInteresting : arena.ColorPlayerGeneric));

                    if (boss != null && player.Tether.ID == (uint)TetherID.Chlamys)
                    {
                        arena.AddLine(player.Position, boss.Position, failing ? arena.ColorDanger : arena.ColorSafe);
                        arena.AddCircle(player.Position, _aoeRange, arena.ColorDanger);
                    }
                }
            }

            public override void OnStatusGain(WorldState.Actor actor, int index)
            {
                switch ((SID)actor.Statuses[index].ID)
                {
                    case SID.RoleCall:
                        ModifyDebuff(_module.FindRaidMemberSlot(actor.InstanceID), ref _debuffTargets, true);
                        break;
                    case SID.Miscast:
                        ModifyDebuff(_module.FindRaidMemberSlot(actor.InstanceID), ref _debuffImmune, true);
                        break;
                }
            }

            public override void OnStatusLose(WorldState.Actor actor, int index)
            {
                switch ((SID)actor.Statuses[index].ID)
                {
                    case SID.RoleCall:
                        ModifyDebuff(_module.FindRaidMemberSlot(actor.InstanceID), ref _debuffTargets, false);
                        break;
                    case SID.Miscast:
                        ModifyDebuff(_module.FindRaidMemberSlot(actor.InstanceID), ref _debuffImmune, false);
                        break;
                }
            }

            private void ModifyDebuff(int slot, ref ulong vector, bool active)
            {
                if (slot >= 0)
                    BitVector.ModifyVector64Bit(ref vector, slot, active);
            }
        }

        // state related to pinax mechanics
        // note: for now, we assume Lava Mekhane always targets 2 healers
        private class Pinax : Component
        {
            public int NumFinished { get; private set; } = 0;

            private P4S _module;
            private WorldState.Actor? _acid;
            private WorldState.Actor? _fire;
            private WorldState.Actor? _water;
            private WorldState.Actor? _lighting;
            private ulong _acidInAOE = 0;

            private static float _acidAOERadius = 5;
            private static float _fireAOERadius = 6;
            private static float _knockbackRadius = 13;
            private static float _lightingSafeDistance = 15; // not sure about this, what is real safe distance?

            public Pinax(P4S module)
            {
                _module = module;
            }

            public override void Reset()
            {
                NumFinished = 0;
                _acid = _fire = _water = _lighting = null;
            }

            public override void Update()
            {
                _acidInAOE = 0;
                if (_acid != null)
                {
                    foreach ((int i, var player) in _module.IterateRaidMembers())
                        _acidInAOE |= _module.FindRaidMembersInRange(i, _acidAOERadius);
                }
            }

            public override void AddHints(int slot, WorldState.Actor actor, TextHints hints, MovementHints? movementHints)
            {
                if (_acid != null)
                {
                    if (GeometryUtils.PointInRect(actor.Position - _acid.Position, Vector3.UnitX, 10, 10, 10))
                    {
                        hints.Add("GTFO from acid square!");
                    }
                    if (BitVector.IsVector64BitSet(_acidInAOE, slot))
                    {
                        hints.Add("Spread!");
                    }
                }
                if (_fire != null)
                {
                    if (GeometryUtils.PointInRect(actor.Position - _fire.Position, Vector3.UnitX, 10, 10, 10))
                    {
                        hints.Add("GTFO from fire square!");
                    }
                    if (_module.IterateRaidMembersWhere(other => other.Role == WorldState.ActorRole.Healer && GeometryUtils.PointInCircle(actor.Position - other.Position, _fireAOERadius)).Count() != 1)
                    {
                        hints.Add("Stack in fours!");
                    }
                }
                if (_water != null)
                {
                    if (GeometryUtils.PointInRect(actor.Position - _water.Position, Vector3.UnitX, 10, 10, 10))
                    {
                        hints.Add("GTFO from water square!");
                    }
                    if (!_module.Arena.InBounds(AdjustPositionForKnockback(actor.Position, _water, _knockbackRadius)))
                    {
                        hints.Add("About to be knocked into wall!");
                    }
                }
                if (_lighting != null)
                {
                    if (GeometryUtils.PointInRect(actor.Position - _lighting.Position, Vector3.UnitX, 10, 10, 10))
                    {
                        hints.Add("GTFO from lighting square!");
                    }
                    if (GeometryUtils.PointInRect(actor.Position - _module.Arena.WorldCenter, Vector3.UnitX, _lightingSafeDistance, _lightingSafeDistance, _lightingSafeDistance))
                    {
                        hints.Add("GTFO from center!");
                    }
                }
            }

            public override void DrawArenaBackground(MiniArena arena)
            {
                if (_acid != null)
                {
                    arena.ZoneQuad(_acid.Position, Vector3.UnitX, 10, 10, 10, arena.ColorAOE);
                }
                if (_fire != null)
                {
                    arena.ZoneQuad(_fire.Position, Vector3.UnitX, 10, 10, 10, arena.ColorAOE);
                }
                if (_water != null)
                {
                    arena.ZoneQuad(_water.Position, Vector3.UnitX, 10, 10, 10, arena.ColorAOE);
                }
                if (_lighting != null)
                {
                    arena.ZoneQuad(_lighting.Position, Vector3.UnitX, 10, 10, 10, arena.ColorAOE);
                    arena.ZoneQuad(arena.WorldCenter, Vector3.UnitX, _lightingSafeDistance, _lightingSafeDistance, _lightingSafeDistance, arena.ColorAOE);
                }
            }

            public override void DrawArenaForeground(MiniArena arena)
            {
                var pc = _module.Player();
                if (pc == null)
                    return;

                if (_acid != null)
                {
                    arena.AddCircle(pc.Position, _acidAOERadius, arena.ColorDanger);
                    foreach ((int i, var player) in _module.IterateRaidMembers())
                        arena.Actor(player, BitVector.IsVector64BitSet(_acidInAOE, i) ? arena.ColorPlayerInteresting : arena.ColorPlayerGeneric);
                }
                if (_fire != null)
                {
                    foreach ((int i, var player) in _module.IterateRaidMembers())
                    {
                        if (player.Role == WorldState.ActorRole.Healer)
                        {
                            arena.Actor(player, arena.ColorDanger);
                            arena.AddCircle(pc.Position, _fireAOERadius, arena.ColorDanger);
                        }
                        else
                        {
                            arena.Actor(player, arena.ColorPlayerGeneric);
                        }
                    }
                }
                if (_water != null)
                {
                    var adjPos = AdjustPositionForKnockback(pc.Position, _module.Boss1(), _knockbackRadius);
                    if (adjPos != pc.Position)
                    {
                        arena.AddLine(pc.Position, adjPos, arena.ColorDanger);
                        arena.Actor(adjPos, pc.Rotation, arena.ColorDanger);
                    }
                }
            }

            public override void OnCastStarted(WorldState.Actor actor)
            {
                if (!actor.CastInfo!.IsSpell())
                    return;
                switch ((AID)actor.CastInfo!.ActionID)
                {
                    case AID.PinaxAcid:
                        _acid = actor;
                        break;
                    case AID.PinaxLava:
                        _fire = actor;
                        break;
                    case AID.PinaxWell:
                        _water = actor;
                        break;
                    case AID.PinaxLevinstrike:
                        _lighting = actor;
                        break;
                }
            }

            public override void OnCastFinished(WorldState.Actor actor)
            {
                if (!actor.CastInfo!.IsSpell())
                    return;
                switch ((AID)actor.CastInfo!.ActionID)
                {
                    case AID.PinaxAcid:
                        _acid = null;
                        ++NumFinished;
                        break;
                    case AID.PinaxLava:
                        _fire = null;
                        ++NumFinished;
                        break;
                    case AID.PinaxWell:
                        _water = null;
                        ++NumFinished;
                        break;
                    case AID.PinaxLevinstrike:
                        _lighting = null;
                        ++NumFinished;
                        break;
                }
            }
        }

        // state related to shift mechanics
        private class Shift : Component
        {
            private P4S _module;
            private WorldState.Actor? _caster;
            private bool _isSword;

            private static float _coneHalfAngle = MathF.PI / 3; // not sure about this...
            private static float _knockbackRange = 20; // TODO: find out real value

            public Shift(P4S module)
            {
                _module = module;
            }

            public override void Reset() => _caster = null;

            public override void AddHints(int slot, WorldState.Actor actor, TextHints hints, MovementHints? movementHints)
            {
                if (_caster == null)
                    return;

                if (_isSword && GeometryUtils.PointInCone(actor.Position - _caster.Position, _caster.Rotation, _coneHalfAngle))
                {
                    hints.Add("GTFO from sword!");
                }
                else if (!_isSword && !_module.Arena.InBounds(AdjustPositionForKnockback(actor.Position, _caster, _knockbackRange)))
                {
                    hints.Add("About to be knocked into wall!");
                }
            }

            public override void DrawArenaBackground(MiniArena arena)
            {
                if (_caster != null && _isSword)
                {
                    arena.ZoneCone(_caster.Position, 0, 50, _caster.Rotation - _coneHalfAngle, _caster.Rotation + _coneHalfAngle, arena.ColorAOE);
                }
            }

            public override void DrawArenaForeground(MiniArena arena)
            {
                var pc = _module.Player();
                if (pc != null && _caster != null && !_isSword)
                {
                    arena.AddCircle(_caster.Position, 5, arena.ColorSafe);

                    var adjPos = AdjustPositionForKnockback(pc.Position, _caster, _knockbackRange);
                    if (adjPos != pc.Position)
                    {
                        arena.AddLine(pc.Position, adjPos, arena.ColorDanger);
                        arena.Actor(adjPos, pc.Rotation, arena.ColorDanger);
                    }
                }
            }

            public override void OnCastStarted(WorldState.Actor actor)
            {
                if (actor.CastInfo!.IsSpell(AID.ShiftingStrikeCloak) || actor.CastInfo!.IsSpell(AID.ShiftingStrikeSword))
                {
                    _caster = actor;
                    _isSword = actor.CastInfo!.ActionID == (uint)AID.ShiftingStrikeSword;
                }
            }

            public override void OnCastFinished(WorldState.Actor actor)
            {
                if (actor.CastInfo!.IsSpell(AID.ShiftingStrikeCloak) || actor.CastInfo!.IsSpell(AID.ShiftingStrikeSword))
                    _caster = null;
            }
        }

        // state related to vengeful belone mechanic
        private class VengefulBelone : Component
        {
            private P4S _module;
            private List<WorldState.Actor> _orbs;
            private Dictionary<uint, WorldState.ActorRole> _orbTargets = new();
            private int _orbsExploded = 0;
            private int[] _playerRuinCount = new int[8];
            private WorldState.ActorRole[] _playerActingRole = new WorldState.ActorRole[8];

            private static float _burstRadius = 8;

            private WorldState.ActorRole OrbTarget(uint instanceID) => _orbTargets.GetValueOrDefault(instanceID, WorldState.ActorRole.None);

            public VengefulBelone(P4S module)
            {
                _module = module;
                _orbs = module.Enemies(OID.Orb);
            }

            public override void Reset()
            {
                _orbTargets.Clear();
                _orbsExploded = 0;
                Array.Fill(_playerRuinCount, 0);
                Array.Fill(_playerActingRole, WorldState.ActorRole.None);
            }

            public override void Update()
            {
                for (int i = 0; i < _playerRuinCount.Length; ++i)
                {
                    _playerRuinCount[i] = _module.RaidMembers[i]?.FindStatus((uint)SID.ThriceComeRuin)?.StackCount ?? 0;
                }
            }

            public override void AddHints(int slot, WorldState.Actor actor, TextHints hints, MovementHints? movementHints)
            {
                if (_orbTargets.Count == 0 || _orbsExploded == _orbTargets.Count)
                    return; // inactive

                int ruinCount = _playerRuinCount[slot];
                if (ruinCount > 2 || (ruinCount == 2 && _playerActingRole[slot] != WorldState.ActorRole.None))
                {
                    hints.Add("Failed orbs...");
                }

                if (_orbs.Where(orb => IsOrbLethal(slot, actor, OrbTarget(orb.InstanceID)) && GeometryUtils.PointInCircle(actor.Position - orb.Position, _burstRadius)).Any())
                {
                    hints.Add("GTFO from wrong orb!");
                }

                if (ruinCount < 2)
                {
                    // TODO: stack check...
                    hints.Add($"Pop next orb {ruinCount + 1}/2!", false);
                }
                else if (ruinCount == 2 && _playerActingRole[slot] == WorldState.ActorRole.None)
                {
                    hints.Add($"Avoid orbs", false);
                }
            }

            public override void DrawArenaForeground(MiniArena arena)
            {
                var pc = _module.Player();
                if (pc == null || _orbTargets.Count == 0 || _orbsExploded == _orbTargets.Count)
                    return;

                foreach (var orb in _orbs)
                {
                    var orbRole = OrbTarget(orb.InstanceID);
                    if (orbRole == WorldState.ActorRole.None)
                        continue; // this orb has already exploded

                    bool lethal = IsOrbLethal(_module.PlayerSlot, pc, orbRole);
                    arena.Actor(orb, lethal ? arena.ColorEnemy : arena.ColorDanger);

                    var target = _module.WorldState.FindActor(orb.Tether.Target);
                    if (target != null)
                    {
                        arena.AddLine(orb.Position, target.Position, arena.ColorDanger);
                    }

                    int goodInRange = 0, badInRange = 0;
                    foreach ((var i, var player) in _module.IterateRaidMembersInRange(orb.Position, _burstRadius))
                    {
                        if (IsOrbLethal(i, player, orbRole))
                            ++badInRange;
                        else
                            ++goodInRange;
                    }

                    bool goodToExplode = goodInRange == 2 && badInRange == 0;
                    arena.AddCircle(orb.Position, _burstRadius, goodToExplode ? arena.ColorSafe : arena.ColorDanger);
                }

                foreach ((int i, var player) in _module.IterateRaidMembers())
                {
                    bool nearLethalOrb = _orbs.Where(orb => IsOrbLethal(i, player, OrbTarget(orb.InstanceID)) && GeometryUtils.PointInCircle(player.Position - orb.Position, _burstRadius)).Any();
                    arena.Actor(player, nearLethalOrb ? arena.ColorPlayerInteresting : arena.ColorPlayerGeneric);
                }
            }

            public override void OnStatusGain(WorldState.Actor actor, int index)
            {
                switch ((SID)actor.Statuses[index].ID)
                {
                    case SID.OrbRole:
                        // TODO: or .Param???
                        _orbTargets[actor.InstanceID] = OrbRoleFromStatusParam(actor.Statuses[index].StackCount);
                        break;
                    case SID.ActingDPS:
                        ModifyActingRole(_module.FindRaidMemberSlot(actor.InstanceID), WorldState.ActorRole.Melee);
                        break;
                    case SID.ActingHealer:
                        ModifyActingRole(_module.FindRaidMemberSlot(actor.InstanceID), WorldState.ActorRole.Healer);
                        break;
                    case SID.ActingTank:
                        ModifyActingRole(_module.FindRaidMemberSlot(actor.InstanceID), WorldState.ActorRole.Tank);
                        break;
                }
            }

            public override void OnStatusLose(WorldState.Actor actor, int index)
            {
                switch ((SID)actor.Statuses[index].ID)
                {
                    case SID.ActingDPS:
                    case SID.ActingHealer:
                    case SID.ActingTank:
                        ModifyActingRole(_module.FindRaidMemberSlot(actor.InstanceID), WorldState.ActorRole.None);
                        break;
                }
            }

            public override void OnEventCast(WorldState.CastResult info)
            {
                if (info.IsSpell(AID.BeloneBurstsAOETank) || info.IsSpell(AID.BeloneBurstsAOEHealer) || info.IsSpell(AID.BeloneBurstsAOEDPS))
                {
                    _orbTargets[info.CasterID] = WorldState.ActorRole.None;
                    ++_orbsExploded;
                }
            }

            private WorldState.ActorRole OrbRoleFromStatusParam(uint param)
            {
                return param switch {
                    0x13A => WorldState.ActorRole.Tank,
                    0x13B => WorldState.ActorRole.Melee,
                    0x13C => WorldState.ActorRole.Healer,
                    _ => WorldState.ActorRole.None
                };
            }

            private bool IsOrbLethal(int slot, WorldState.Actor player, WorldState.ActorRole orbRole)
            {
                int ruinCount = _playerRuinCount[slot];
                if (ruinCount >= 2)
                    return true; // any orb is now lethal

                var actingRole = _playerActingRole[slot];
                if (ruinCount == 1 && actingRole != WorldState.ActorRole.None)
                    return orbRole != actingRole; // player must clear acting debuff, or he will die

                var playerRole = player.Role == WorldState.ActorRole.Ranged ? WorldState.ActorRole.Melee : player.Role;
                return orbRole == playerRole;
            }

            private void ModifyActingRole(int slot, WorldState.ActorRole role)
            {
                if (slot >= 0)
                    _playerActingRole[slot] = role;
            }
        }

        // state related to elemental belone mechanic (3 of 4 corners exploding)
        private class ElementalBelone : Component
        {
            enum Element : uint { Acid, Fire, Water, Lighting, Unknown }

            private P4S _module;
            private uint _cornerAssignments = 0; // (x >> (2*corner-id)) & 3 == element in corner
            private Element _safeElement = Element.Unknown;
            private List<Vector3> _imminentExplodingCorners = new();

            public ElementalBelone(P4S module)
            {
                _module = module;
            }

            public void AssignSafespotFromBloodrake()
            {
                uint forbiddenCorners = 0;
                foreach (var actor in _module.WorldState.Actors.Values.Where(actor => actor.Tether.ID == (uint)TetherID.Bloodrake && actor.OID == (uint)OID.Helper))
                    forbiddenCorners |= 1u << CornerFromPos(actor.Position);
                int safeCorner = BitOperations.TrailingZeroCount(~forbiddenCorners);
                _safeElement = (Element)((_cornerAssignments >> (2 * safeCorner)) & 3);
            }

            public override void Reset()
            {
                _cornerAssignments = 0;
                _safeElement = Element.Unknown;
                _imminentExplodingCorners.Clear();
            }

            public override void AddHints(int slot, WorldState.Actor actor, TextHints hints, MovementHints? movementHints)
            {
                if (_imminentExplodingCorners.Where(p => GeometryUtils.PointInRect(actor.Position - p, Vector3.UnitX, 10, 10, 10)).Any())
                {
                    hints.Add($"GTFO from exploding square");
                }
                if (_safeElement != Element.Unknown)
                {
                    hints.Add($"Safe square: {_safeElement}", false);
                }
            }

            public override void DrawArenaBackground(MiniArena arena)
            {
                foreach (var p in _imminentExplodingCorners)
                {
                    arena.ZoneQuad(p, Vector3.UnitX, 10, 10, 10, arena.ColorAOE);
                }
            }

            public override void OnCastStarted(WorldState.Actor actor)
            {
                if (!actor.CastInfo!.IsSpell())
                    return;
                switch ((AID)actor.CastInfo!.ActionID)
                {
                    case AID.PinaxAcid:
                        _cornerAssignments |= (uint)Element.Acid << (2 * CornerFromPos(actor.Position));
                        break;
                    case AID.PinaxLava:
                        _cornerAssignments |= (uint)Element.Fire << (2 * CornerFromPos(actor.Position));
                        break;
                    case AID.PinaxWell:
                        _cornerAssignments |= (uint)Element.Water << (2 * CornerFromPos(actor.Position));
                        break;
                    case AID.PinaxLevinstrike:
                        _cornerAssignments |= (uint)Element.Lighting << (2 * CornerFromPos(actor.Position));
                        break;
                    case AID.PeriaktoiDangerAcid:
                    case AID.PeriaktoiDangerLava:
                    case AID.PeriaktoiDangerWell:
                    case AID.PeriaktoiDangerLevinstrike:
                        _imminentExplodingCorners.Add(actor.Position);
                        break;
                }
            }

            private int CornerFromPos(Vector3 pos)
            {
                int corner = 0;
                if (pos.X > _module.Arena.WorldCenter.X)
                    corner |= 1;
                if (pos.Z > _module.Arena.WorldCenter.Z)
                    corner |= 2;
                return corner;
            }
        }

        private List<WorldState.Actor> _boss1;
        private List<WorldState.Actor> _boss2;
        private WorldState.Actor? Boss1() => _boss1.FirstOrDefault();
        private WorldState.Actor? Boss2() => _boss2.FirstOrDefault();

        private StateMachine.State? _phase1Start;
        private StateMachine.State? _phase2Start;

        public P4S(WorldState ws)
            : base(ws, 8)
        {
            _boss1 = RegisterEnemies(OID.Boss1, true);
            _boss2 = RegisterEnemies(OID.Boss2, true);
            RegisterEnemies(OID.Orb);

            RegisterComponent(new DirectorsBelone(this));
            RegisterComponent(new Pinax(this));
            RegisterComponent(new Shift(this));
            RegisterComponent(new VengefulBelone(this));
            RegisterComponent(new ElementalBelone(this));

            // checkpoint is triggered by boss becoming untargetable...
            BuildPhase1States();
            BuildPhase2States();
            InitialState = _phase1Start;
        }

        protected override void ResetModule()
        {
            InitialState = Boss1() != null ? _phase1Start : _phase2Start;
        }

        protected override void DrawArenaForegroundPost()
        {
            Arena.Actor(Boss1(), Arena.ColorEnemy);
            Arena.Actor(Player(), Arena.ColorPC);
        }

        private void BuildPhase1States()
        {
            StateMachine.State? s;
            s = BuildDecollationState(ref _phase1Start, 9.3f);
            s = BuildBloodrakeBeloneStates(ref s.Next, 4.2f);
            s = BuildDecollationState(ref s.Next, 3.4f);
            s = BuildElegantEviscerationState(ref s.Next, 4.2f);
            s = BuildPinaxStates(ref s.Next, 11.4f);
            s = BuildElegantEviscerationState(ref s.Next, 3.8f);
            s = BuildVengefulElementalBeloneStates(ref s.Next, 4.2f);
            s = BuildBeloneCoilsStates(ref s.Next, 8.2f);
            s = BuildDecollationState(ref s.Next, 3.4f);
            s = BuildElegantEviscerationState(ref s.Next, 4.2f);
            s = BuildPinaxStates(ref s.Next, 11.4f);
            s = BuildDecollationState(ref s.Next, 0); // note: cast starts ~0.2s before pinax resolve, whatever...
            s = BuildDecollationState(ref s.Next, 4.2f);
            s = BuildDecollationState(ref s.Next, 4.2f);
        }

        private StateMachine.State BuildDecollationState(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, Boss1, AID.Decollation, delay, 5, "AOE");
            s.EndHint |= StateMachine.StateHint.Raidwide;
            return s;
        }

        private StateMachine.State BuildElegantEviscerationState(ref StateMachine.State? link, float delay)
        {
            var cast = CommonStates.Cast(ref link, Boss1, AID.ElegantEvisceration, delay, 5, "Tankbuster");
            cast.EndHint |= StateMachine.StateHint.Tankbuster | StateMachine.StateHint.GroupWithNext;
            var resolve = CommonStates.Timeout(ref cast.Next, 3, "Tankbuster");
            resolve.EndHint |= StateMachine.StateHint.Tankbuster;
            return resolve;
        }

        private StateMachine.State BuildBloodrakeBeloneStates(ref StateMachine.State? link, float delay)
        {
            var beloneComp = FindComponent<DirectorsBelone>()!;

            var bloodrake1 = CommonStates.Cast(ref link, Boss1, AID.Bloodrake, delay, 4, "Bloodrake 1");
            bloodrake1.Enter = beloneComp.AssignTethersFromBloodrake;
            bloodrake1.EndHint |= StateMachine.StateHint.GroupWithNext;

            var aetheric = CommonStates.Cast(ref bloodrake1.Next, Boss1, AID.AethericChlamys, 3.2f, 4);

            var bloodrake2 = CommonStates.Cast(ref aetheric.Next, Boss1, AID.Bloodrake, 4.2f, 4, "Bloodrake 2");
            bloodrake2.Enter = beloneComp.AssignDebuffsFromBloodrake;
            bloodrake2.EndHint |= StateMachine.StateHint.GroupWithNext;

            var belone = CommonStates.Cast(ref bloodrake2.Next, Boss1, AID.DirectorsBelone, 4.2f, 5);

            var inv = CommonStates.Cast(ref belone.Next, Boss1, AID.InversiveChlamys, 9.2f, 7);
            inv.Enter = () => beloneComp.CurState = DirectorsBelone.State.Chlamys;
            inv.Exit = () => beloneComp.CurState = DirectorsBelone.State.Resolve;

            var resolve = CommonStates.Condition(ref inv.Next, 0.8f, () => beloneComp.CurState == DirectorsBelone.State.Inactive, "Chlamys");
            return resolve;
        }

        private StateMachine.State BuildPinaxStates(ref StateMachine.State? link, float delay)
        {
            var comp = FindComponent<Pinax>()!;

            var setting = CommonStates.Cast(ref link, Boss1, AID.SettingTheScene, delay, 4, "Scene");
            setting.EndHint |= StateMachine.StateHint.GroupWithNext;
            // ~1s after cast end, we get a bunch of env controls 8003759C, state=00020001
            // what I've seen so far:
            // 1. WF arrangement: indices 1, 2, 3, 4, 5, 10, 15, 20
            //    AL
            // index 5 corresponds to NE fire, index 10 corresponds to SE lighting, index 15 corresponds to SW acid, index 20 corresponds to NW water

            var pinax = CommonStates.Cast(ref setting.Next, Boss1, AID.Pinax, 8.2f, 5, "Pinax");
            pinax.EndHint |= StateMachine.StateHint.GroupWithNext;
            // timeline:
            //  0.0s pinax cast end
            //  1.0s square 1 activation: env control (.10 = 00800040), helper starts casting 27095
            //  4.0s square 2 activation: env control (.15 = 00800040), helper starts casting 27092
            //  7.0s square 1 env control (.10 = 02000001)
            // 10.0s square 2 env control (.15 = 02000001)
            //       square 1 cast finish (+ instant 27091)
            // 13.0s square 2 cast finish (+ instant 27088)
            // 14.0s square 3 activation: env control (.20 = 00800040), helper starts casting 27094
            // 20.0s square 3 env control (.20 = 02000001)
            // 23.0s square 3 cast finish (+ instant 27090)
            // 25.0s square 4 activation: env control (.05 = 00800040), helper starts casting 27093
            // 31.0s square 4 env control (.05 = 02000001)
            // 34.0s square 4 cast finish (+ instant 27089)

            var p1 = CommonStates.Condition(ref pinax.Next, 10, () => comp.NumFinished == 1, "Corner1");
            p1.EndHint |= StateMachine.StateHint.GroupWithNext;

            var p2 = CommonStates.Condition(ref p1.Next, 3, () => comp.NumFinished == 2, "Corner2");
            p2.EndHint |= StateMachine.StateHint.GroupWithNext;

            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.NortherlyShiftCloak] = new(null, () => { });
            dispatch[AID.SoutherlyShiftCloak] = new(null, () => { });
            dispatch[AID.EasterlyShiftCloak] = new(null, () => { });
            dispatch[AID.WesterlyShiftCloak] = new(null, () => { });
            dispatch[AID.NortherlyShiftSword] = new(null, () => { });
            dispatch[AID.SoutherlyShiftSword] = new(null, () => { });
            dispatch[AID.EasterlyShiftSword] = new(null, () => { });
            dispatch[AID.WesterlyShiftSword] = new(null, () => { });
            var shiftStart = CommonStates.CastStart(ref p2.Next, Boss1, dispatch, 3.6f);
            // together with this, one of the helpers starts casting 27142 or 27137

            var p3 = CommonStates.Condition(ref shiftStart.Next, 6.4f, () => comp.NumFinished == 3, "Corner3");
            p3.EndHint |= StateMachine.StateHint.GroupWithNext;

            var shiftEnd = CommonStates.CastEnd(ref p3.Next, Boss1, 1.6f, "Shift");
            shiftEnd.EndHint |= StateMachine.StateHint.GroupWithNext;

            var p4 = CommonStates.Condition(ref shiftEnd.Next, 9.4f, () => comp.NumFinished == 4, "Pinax resolve");
            p4.Exit = comp.Reset;
            return p4;
        }

        private StateMachine.State BuildVengefulElementalBeloneStates(ref StateMachine.State? link, float delay)
        {
            var compElemental = FindComponent<ElementalBelone>()!;

            var bloodrake3 = CommonStates.Cast(ref link, Boss1, AID.Bloodrake, delay, 4, "Bloodrake 3");
            bloodrake3.Enter = compElemental.AssignSafespotFromBloodrake;
            bloodrake3.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.Raidwide;

            var setting = CommonStates.Cast(ref bloodrake3.Next, Boss1, AID.SettingTheScene, 7.2f, 4, "Scene");
            setting.EndHint |= StateMachine.StateHint.GroupWithNext;

            var vengeful = CommonStates.Cast(ref setting.Next, Boss1, AID.VengefulBelone, 8.2f, 4, "Roles"); // acting X applied after cast end
            vengeful.EndHint |= StateMachine.StateHint.GroupWithNext;

            var elemental = CommonStates.Cast(ref vengeful.Next, Boss1, AID.ElementalBelone, 4.2f, 4); // 'elemental resistance down' applied after cast end

            var bloodrake4 = CommonStates.Cast(ref elemental.Next, Boss1, AID.Bloodrake, 4.2f, 4, "Bloodrake 4");
            bloodrake4.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.Raidwide;

            var bursts = CommonStates.Cast(ref bloodrake4.Next, Boss1, AID.BeloneBursts, 4.2f, 5, "Orbs"); // orbs appear at cast start, tether and start moving at cast end
            bursts.EndHint |= StateMachine.StateHint.GroupWithNext;

            var periaktoi = CommonStates.Cast(ref bursts.Next, Boss1, AID.Periaktoi, 9.2f, 5, "Square explode");
            periaktoi.Exit = compElemental.Reset;
            return periaktoi;
        }

        private StateMachine.State BuildBeloneCoilsStates(ref StateMachine.State? link, float delay)
        {
            var beloneComp = FindComponent<DirectorsBelone>()!;

            var bloodrake5 = CommonStates.Cast(ref link, Boss1, AID.Bloodrake, delay, 4, "Bloodrake 5");
            bloodrake5.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.Raidwide;

            var coils1 = CommonStates.Cast(ref bloodrake5.Next, Boss1, AID.BeloneCoils, 3.2f, 4, "Coils 1");
            coils1.EndHint |= StateMachine.StateHint.GroupWithNext;

            var inv1 = CommonStates.Cast(ref coils1.Next, Boss1, AID.InversiveChlamys, 3.2f, 7, "Chlamys");
            inv1.EndHint |= StateMachine.StateHint.GroupWithNext;

            var aetheric = CommonStates.Cast(ref inv1.Next, Boss1, AID.AethericChlamys, 3.2f, 4);

            var bloodrake6 = CommonStates.Cast(ref aetheric.Next, Boss1, AID.Bloodrake, 4.2f, 4, "Bloodrake 6");
            bloodrake6.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.Raidwide;

            var coils2 = CommonStates.Cast(ref bloodrake6.Next, Boss1, AID.BeloneCoils, 4.2f, 4, "Coils 2");
            coils2.EndHint |= StateMachine.StateHint.GroupWithNext;

            var belone = CommonStates.Cast(ref coils2.Next, Boss1, AID.DirectorsBelone, 9.2f, 5);

            var inv2 = CommonStates.Cast(ref belone.Next, Boss1, AID.InversiveChlamys, 9.2f, 7, "Chlamys");

            var resolve = CommonStates.Condition(ref inv2.Next, 0.8f, () => beloneComp.CurState == DirectorsBelone.State.Inactive, "Chlamys");
            return resolve;
        }

        private void BuildPhase2States()
        {
            //StateMachine.State? s;
        }
    }
}
