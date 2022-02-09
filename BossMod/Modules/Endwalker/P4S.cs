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
            ElegantEviscerationSecond = 26649, // Boss1->target, no cast, second hit
            SettingTheScene = 27083, // Boss1->Boss1
            ShiftStart = 27086, // Boss1->Boss1, no cast, tp to center, sword glowing (?)
            Pinax = 27087, // Boss1->Boss1
            MekhaneAcid = 27088, // Helper->target, no cast
            MekhaneLava = 27089, // Helper->target, no cast
            MekhaneWell = 27090, // Helper->Helper, no cast, affects arena
            MekhaneLevinstrike = 27091, // Helper->Helper, no cast, affects arena
            PinaxAcid = 27092, // Helper->Helper, affects corner
            PinaxLava = 27093, // Helper->Helper, affects corner
            PinaxWell = 27094, // Helper->Helper, affects corner
            PinaxLevinstrike = 27095, // Helper->Helper, affects corner
            Bloodrake = 27096, // Boss1->Boss1
            BeloneBursts = 27097, // Boss1->Boss1
            BeloneBurstsAOETank = 27098, // Orb->target, no cast
            BeloneBurstsAOEHealer = 27099, // Orb->target, no cast
            BeloneBurstsAOEDPS = 27100, // Orb->target, no cast
            BeloneCoils = 27101, // Boss1->Boss1
            BeloneCoilsDPS = 27102, // Helper->Helper, role towers ('no heals/tanks' variant)
            BeloneCoilsTH = 27103, // Helper->Helper, role towers ('no dps' variant)
            DirectorsBelone = 27110, // Boss1->Boss1
            DirectorsBeloneDebuffs = 27111, // Helper->target, no cast, just applies Role Call debuffs
            CursedCasting = 27113, // Helper->target, no cast, does something bad if no role call?..
            AethericChlamys = 27116, // Boss1->Boss1
            InversiveChlamys = 27117, // Boss1->Boss1
            InversiveChlamysAOE = 27119, // Helper->target, no cast, damage to tethered targets
            ElementalBelone = 27122, // Boss1->Boss1
            Periaktoi = 27124, // Boss1->Boss1
            PeriaktoiSafeAcid = 27125, // Helper->Helper (unconfirmed)
            PeriaktoiSafeLava = 27126, // Helper->Helper
            PeriaktoiSafeWell = 27127, // Helper->Helper
            PeriaktoiSafeLevinstrike = 27128, // Helper->Helper
            PeriaktoiDangerAcid = 27129, // Helper->Helper
            PeriaktoiDangerLava = 27130, // Helper->Helper
            PeriaktoiDangerWell = 27131, // Helper->Helper
            PeriaktoiDangerLevinstrike = 27132, // Helper->Helper
            NortherlyShiftCloak = 27133, // Boss1->Boss1
            SoutherlyShiftCloak = 27134, // Boss1->Boss1 (unconfirmed)
            EasterlyShiftCloak = 27135, // Boss1->Boss1 (unconfirmed)
            WesterlyShiftCloak = 27136, // Boss1->Boss1 (unconfirmed)
            ShiftingStrikeCloak = 27137, // Helper->Helper
            NortherlyShiftSword = 27138, // Boss1->Boss1
            SoutherlyShiftSword = 27139, // Boss1->Boss1
            EasterlyShiftSword = 27140, // Boss1->Boss1
            WesterlyShiftSword = 27141, // Boss1->Boss1 (unconfirmed)
            ShiftingStrikeSword = 27142, // Helper->Helper, sword attack
            ElegantEvisceration = 27144, // Boss1->target
            Decollation = 27145, // Boss1->Boss1
            AkanthaiAct1 = 27148, // Boss2->Boss2
            AkanthaiExplodeAOE = 27149, // Helper->Helper
            AkanthaiExplodeTower = 27150, // Helper->Helper
            AkanthaiExplodeKnockback = 27152, // Helper->Helper
            AkanthaiVisualTower = 27153, // Akantha->Akantha
            AkanthaiVisualAOE = 27154, // Akantha->Akantha
            AkanthaiVisualKnockback  = 27155, // Akantha->Akantha
            AkanthaiWaterBreakAOE = 27156, // Helper->targets, no cast
            AkanthaiDarkBreakAOE = 27158, // Helper->targets, no cast
            AkanthaiFireBreakAOE = 27160, // Helper->targets, no cast
            AkanthaiWindBreakAOE = 27162, // Helper->targets, no cast
            FleetingImpulseAOE = 27164, // Helper->target, no cast
            HellsSting = 27166, // Boss2->Boss2
            HellsStingSecond = 27167, // Boss2->Boss2, no cast
            HellsStingAOE1 = 27168, // Helper->Helper
            HellsStingAOE2 = 27169, // Helper->Helper
            KothornosKock = 27170, // Boss2->Boss2
            KothornosKickJump = 27171, // Boss2->target, no cast
            KothornosQuake2 = 27172, // Boss2->Boss2, no cast
            KothornosQuakeAOE = 27173, // Helper->target, no cast
            Nearsight = 27174, // Boss2->Boss2
            Farsight = 27175, // Boss2->Boss2
            NearsightAOE = 27176, // Helper->target, no cast
            DarkDesign = 27177, // Boss2->Boss2
            DarkDesignAOE = 27178, // Helper->location
            HeartStake = 27179, // Boss2->target
            UltimateImpulse = 27180, // Boss2->Boss2
            SearingStream = 27181, // Boss2->Boss2
            WreathOfThorns1 = 27183, // Boss2->Boss2
            WreathOfThorns2 = 27184, // Boss2->Boss2
            WreathOfThorns3 = 27185, // Boss2->Boss2
            WreathOfThorns4 = 27184, // Boss2->Boss2
            WreathOfThorns5 = 27188, // Boss2->Boss2
            WreathOfThorns6 = 27189, // Boss2->Boss2
            AkanthaiCurtainCall = 27190, // Boss2->Boss2
            Enrage = 27191, // Boss2->Boss2
            FarsightAOE = 28123, // Helper->target, no cast
            VengefulBelone = 28194, // Boss1->Boss1
            KothornosQuake1 = 28276, // Boss2->Boss2, no cast
            HeartStakeSecond = 28279, // Boss2->target, no cast
            DemigodDouble = 28280, // Boss2->target
            AkanthaiAct2 = 28340, // Boss2->Boss2
            AkanthaiAct3 = 28341, // Boss2->Boss2
            AkanthaiAct4 = 28342, // Boss2->Boss2
            AkanthaiFinale = 28343, // Boss2->Boss2
            FleetingImpulse = 28344, // Boss2->Boss2
            InversiveChlamysAOE2 = 28437, // Helper->target, no cast, damage to tethered targets (during belone coils)
        };

        public enum SID : uint
        {
            OrbRole = 2056,
            ThriceComeRuin = 2530,
            RoleCall = 2802,
            Miscast = 2803,
            Thornpricked = 2804,
            ActingDPS = 2925,
            ActingHealer = 2926,
            ActingTank = 2927,
        }

        public enum TetherID : uint
        {
            ExplosiveAether = 17,
            Chlamys = 89,
            Bloodrake = 165,
            WreathOfThornsPairsClose = 172,
            WreathOfThorns = 173, // also used when pairs are about to break
        }

        public enum IconID: uint
        {
            None = 0,
            AkanthaiWater = 300, // act 4
            AkanthaiDark = 301, // acts 2 & 4 & 6
            AkanthaiWind = 302, // acts 2 & 5
            AkanthaiFire = 303, // act 2
        }

        // state related to elegant evisceration mechanic (dual hit tankbuster)
        // TODO: consider showing some tank swap / invul hint...
        private class ElegantEvisceration : CommonComponents.CastCounter
        {
            public ElegantEvisceration() : base((uint)AID.ElegantEviscerationSecond) { }
        }

        // state related to belone coils mechanic (role towers)
        private class BeloneCoils : Component
        {
            public enum Soaker { Unknown, TankOrHealer, DamageDealer }

            public Soaker ActiveSoakers { get; private set; } = Soaker.Unknown;

            private P4S _module;
            private List<WorldState.Actor> _activeTowers = new(); // actor + tank-or-healer

            private static float _towerRadius = 4;

            public BeloneCoils(P4S module)
            {
                _module = module;
            }

            public bool IsValidSoaker(WorldState.Actor player)
            {
                switch (ActiveSoakers)
                {
                    case Soaker.TankOrHealer: return player.Role == WorldState.ActorRole.Tank || player.Role == WorldState.ActorRole.Healer;
                    case Soaker.DamageDealer: return player.Role == WorldState.ActorRole.Melee || player.Role == WorldState.ActorRole.Ranged;
                    default: return false;
                }
            }

            public override void Reset()
            {
                ActiveSoakers = Soaker.Unknown;
                _activeTowers.Clear();
            }

            public override void AddHints(int slot, WorldState.Actor actor, TextHints hints, MovementHints? movementHints)
            {
                if (ActiveSoakers == Soaker.Unknown)
                    return;

                bool isSoaking = _activeTowers.Where(tower => GeometryUtils.PointInCircle(actor.Position - tower.Position, _towerRadius)).Any();
                if (IsValidSoaker(actor))
                {
                    hints.Add("Soak the tower", !isSoaking);
                }
                else
                {
                    hints.Add("GTFO from tower", isSoaking);
                }
            }

            public override void DrawArenaForeground(MiniArena arena)
            {
                var pc = _module.Player();
                if (pc == null || ActiveSoakers == Soaker.Unknown)
                    return;

                bool validSoaker = IsValidSoaker(pc);
                foreach (var tower in _activeTowers)
                {
                    arena.AddCircle(tower.Position, _towerRadius, validSoaker ? arena.ColorSafe : arena.ColorDanger);
                }
            }

            public override void OnCastStarted(WorldState.Actor actor)
            {
                if (actor.CastInfo!.IsSpell(AID.BeloneCoilsDPS) || actor.CastInfo!.IsSpell(AID.BeloneCoilsTH))
                {
                    _activeTowers.Add(actor);
                    ActiveSoakers = actor.CastInfo!.ActionID == (uint)AID.BeloneCoilsDPS ? Soaker.DamageDealer : Soaker.TankOrHealer;
                }
            }

            public override void OnCastFinished(WorldState.Actor actor)
            {
                if (actor.CastInfo!.IsSpell(AID.BeloneCoilsDPS) || actor.CastInfo!.IsSpell(AID.BeloneCoilsTH))
                {
                    _activeTowers.Remove(actor);
                    if (_activeTowers.Count == 0)
                        ActiveSoakers = Soaker.Unknown;
                }
            }
        }

        // state related to inversive chlamys mechanic (tethers)
        private class InversiveChlamys : Component
        {
            private P4S _module;
            private bool _assignFromCoils = false;
            private ulong _tetherForbidden = 0;
            private ulong _tetherTargets = 0;
            private ulong _tetherInAOE = 0;

            private static float _aoeRange = 5;

            public bool TethersActive => _tetherTargets != 0;

            public InversiveChlamys(P4S module)
            {
                _module = module;
            }

            public void AssignFromBloodrake()
            {
                _tetherForbidden = BuildMask(_module.IterateRaidMembersWhere(actor => actor.Tether.ID == (uint)TetherID.Bloodrake));
            }

            public void AssignFromCoils()
            {
                _assignFromCoils = true;
            }

            public override void Reset()
            {
                _assignFromCoils = false;
                _tetherForbidden = 0;
            }

            public override void Update()
            {
                if (_assignFromCoils)
                {
                    var coils = _module.FindComponent<BeloneCoils>()!;
                    if (coils.ActiveSoakers != BeloneCoils.Soaker.Unknown)
                    {
                        _tetherForbidden = BuildMask(_module.IterateRaidMembersWhere(coils.IsValidSoaker));
                        _assignFromCoils = false;
                    }
                }

                _tetherTargets = _tetherInAOE = 0;
                if (_tetherForbidden == 0)
                    return;

                foreach ((int i, var player) in _module.IterateRaidMembersWhere(actor => actor.Tether.ID == (uint)TetherID.Chlamys))
                {
                    BitVector.SetVector64Bit(ref _tetherTargets, i);
                    _tetherInAOE |= _module.FindRaidMembersInRange(i, _aoeRange);
                }
            }

            public override void AddHints(int slot, WorldState.Actor actor, TextHints hints, MovementHints? movementHints)
            {
                if (_tetherForbidden == 0)
                    return;

                if (!BitVector.IsVector64BitSet(_tetherForbidden, slot))
                {
                    // we should be grabbing tethers
                    if (_tetherTargets == 0)
                    {
                        hints.Add("Tethers: prepare to intercept", false);
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
                        hints.Add("Tethers: prepare to pass", false);
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

            public override void DrawArenaForeground(MiniArena arena)
            {
                if (_tetherTargets == 0)
                    return;

                var boss = _module.Boss1();
                ulong failingPlayers = _tetherForbidden & _tetherTargets;
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
        }

        // state related to director's belone (debuffs) mechanic
        private class DirectorsBelone : Component
        {
            private P4S _module;
            private bool _assignFromCoils = false;
            private ulong _debuffForbidden = 0;
            private ulong _debuffTargets = 0;
            private ulong _debuffImmune = 0;

            private static float _debuffPassRange = 3; // not sure about this...

            public DirectorsBelone(P4S module)
            {
                _module = module;
            }

            public void AssignFromBloodrake()
            {
                _debuffForbidden = BuildMask(_module.IterateRaidMembersWhere(actor => actor.Tether.ID == (uint)TetherID.Bloodrake));
            }

            public void AssignFromCoils()
            {
                _assignFromCoils = true;
            }

            public override void Reset()
            {
                _assignFromCoils = false;
                _debuffForbidden = _debuffTargets = _debuffImmune = 0;
            }

            public override void Update()
            {
                if (_assignFromCoils)
                {
                    var coils = _module.FindComponent<BeloneCoils>()!;
                    if (coils.ActiveSoakers != BeloneCoils.Soaker.Unknown)
                    {
                        _debuffForbidden = BuildMask(_module.IterateRaidMembersWhere(coils.IsValidSoaker));
                        _assignFromCoils = false;
                    }
                }
            }

            public override void AddHints(int slot, WorldState.Actor actor, TextHints hints, MovementHints? movementHints)
            {
                if (_debuffForbidden == 0)
                    return;

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

            public override void DrawArenaForeground(MiniArena arena)
            {
                if (_debuffTargets == 0)
                    return;

                var boss = _module.Boss1();
                ulong failingPlayers = _debuffForbidden & _debuffTargets;
                foreach ((int i, var player) in _module.IterateRaidMembers())
                {
                    bool failing = BitVector.IsVector64BitSet(failingPlayers, i);
                    arena.Actor(player, failing ? arena.ColorDanger : arena.ColorPlayerGeneric);
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

        // state related to nearsight & farsight mechanics
        private class NearFarSight : Component
        {
            public enum State { None, Near, Far }
            public State CurState = State.None;

            private P4S _module;
            private ulong _targets = 0;
            private ulong _inAOE = 0;

            private static float _aoeRadius = 5;

            public NearFarSight(P4S module)
            {
                _module = module;
            }

            public override void Reset() => CurState = State.None;

            public override void Update()
            {
                _targets = _inAOE = 0;
                var boss = _module.Boss2();
                if (boss == null || CurState == State.None)
                    return;

                var playersByRange = _module.IterateRaidMembers().SortedByRange(boss.Position);
                foreach ((int i, var player) in CurState == State.Near ? playersByRange.Take(2) : playersByRange.TakeLast(2))
                {
                    BitVector.SetVector64Bit(ref _targets, i);
                    _inAOE |= _module.FindRaidMembersInRange(i, _aoeRadius);
                }
            }

            public override void AddHints(int slot, WorldState.Actor actor, TextHints hints, MovementHints? movementHints)
            {
                if (_targets == 0)
                    return;

                bool isTarget = BitVector.IsVector64BitSet(_targets, slot);
                bool shouldBeTarget = actor.Role == WorldState.ActorRole.Tank;
                bool isFailing = isTarget != shouldBeTarget;
                bool shouldBeNear = CurState == State.Near ? shouldBeTarget : !shouldBeTarget;
                hints.Add(shouldBeNear ? "Stay near boss" : "Stay on max melee", isFailing);
                if (BitVector.IsVector64BitSet(_inAOE, slot))
                {
                    hints.Add("GTFO from tanks!");
                }
            }

            public override void DrawArenaForeground(MiniArena arena)
            {
                var pc = _module.Player();
                if (pc == null || _targets == 0)
                    return;

                foreach ((int i, var player) in _module.IterateRaidMembers())
                {
                    if (BitVector.IsVector64BitSet(_targets, i))
                    {
                        arena.Actor(player, arena.ColorDanger);
                        arena.AddCircle(player.Position, _aoeRadius, arena.ColorDanger);
                    }
                    else
                    {
                        arena.Actor(player, BitVector.IsVector64BitSet(_inAOE, i) ? arena.ColorPlayerInteresting : arena.ColorPlayerGeneric);
                    }
                }
            }

            public override void OnEventCast(WorldState.CastResult info)
            {
                if (info.IsSpell(AID.NearsightAOE) || info.IsSpell(AID.FarsightAOE))
                    CurState = State.None;
            }
        }

        // state related to heart stake mechanic (dual hit tankbuster with bleed)
        // TODO: consider showing some tank swap / invul hint...
        private class HeartStake : CommonComponents.CastCounter
        {
            public HeartStake() : base((uint)AID.HeartStakeSecond) { }
        }

        // state related to hell's sting mechanic (part of curtain call sequence)
        private class HellsSting : Component
        {
            public int NumCasts { get; private set; } = 0;

            private P4S _module;
            private float? _direction = null;

            private static float _coneHalfAngle = MathF.PI / 12; // not sure about this...

            public HellsSting(P4S module)
            {
                _module = module;
            }

            public void Start(float dir)
            {
                _direction = dir;
            }

            public override void Reset()
            {
                NumCasts = 0;
                _direction = null;
            }

            public override void AddHints(int slot, WorldState.Actor actor, TextHints hints, MovementHints? movementHints)
            {
                var boss = _module.Boss2();
                if (_direction == null || NumCasts >= 2 || boss == null)
                    return;

                for (int i = 0; i < 8; ++i)
                {
                    float dir = _direction.Value + NumCasts * MathF.PI / 8 + i * MathF.PI / 4;
                    if (GeometryUtils.PointInCone(actor.Position - boss.Position, dir, _coneHalfAngle))
                    {
                        hints.Add("GTFO from cone!");
                        break;
                    }
                }
            }

            public override void DrawArenaBackground(MiniArena arena)
            {
                var boss = _module.Boss2();
                if (_direction == null || NumCasts >= 2 || boss == null)
                    return;

                for (int i = 0; i < 8; ++i)
                {
                    float dir = _direction.Value + NumCasts * MathF.PI / 8 + i * MathF.PI / 4;
                    arena.ZoneCone(boss.Position, 0, 50, dir - _coneHalfAngle, dir + _coneHalfAngle, arena.ColorAOE);
                }
            }

            public override void OnCastFinished(WorldState.Actor actor)
            {
                if (_direction != null && (actor.CastInfo!.IsSpell(AID.HellsStingAOE1) || actor.CastInfo!.IsSpell(AID.HellsStingAOE2)))
                    ++NumCasts;
            }
        }

        // common wreath of thorns constants
        private static float _wreathAOERadius = 20;
        private static float _wreathTowerRadius = 4;

        // state related to act 1 wreath of thorns
        private class WreathOfThorns1 : Component
        {
            public enum State { Inactive, FirstAOEs, Towers, LastAOEs }
            public State CurState { get; private set; } = State.Inactive;

            private P4S _module;
            private List<WorldState.Actor> _helpers;
            private List<WorldState.Actor> _relevantHelpers = new(); // 2 aoes -> 8 towers -> 2 aoes

            private IEnumerable<WorldState.Actor> _firstAOEs => _relevantHelpers.Take(2);
            private IEnumerable<WorldState.Actor> _towers => _relevantHelpers.Skip(2).Take(8);
            private IEnumerable<WorldState.Actor> _lastAOEs => _relevantHelpers.Skip(10);

            public WreathOfThorns1(P4S module)
            {
                _module = module;
                _helpers = module.Enemies(OID.Helper);
            }

            public void Begin()
            {
                CurState = State.FirstAOEs;
                // there should be two tethered helpers for aoes
                _relevantHelpers = _helpers.Where(actor => actor.Tether.ID == (uint)TetherID.WreathOfThorns).ToList();
            }

            public override void Reset()
            {
                CurState = State.Inactive;
                _relevantHelpers.Clear();
            }

            public override void AddHints(int slot, WorldState.Actor actor, TextHints hints, MovementHints? movementHints)
            {
                switch (CurState)
                {
                    case State.FirstAOEs:
                        if (_firstAOEs.InRadius(actor.Position, _wreathAOERadius).Any())
                        {
                            hints.Add("GTFO from AOE!");
                        }
                        break;
                    case State.Towers:
                        {
                            var soakedTower = _towers.InRadius(actor.Position, _wreathTowerRadius).FirstOrDefault();
                            if (soakedTower == null)
                            {
                                hints.Add("Soak the tower!");
                            }
                            else if (_module.IterateRaidMembersInRange(soakedTower.Position, _wreathTowerRadius).Any(ip => ip.Item1 != slot))
                            {
                                hints.Add("Multiple soakers for the tower!");
                            }
                        }
                        break;
                    case State.LastAOEs:
                        if (_lastAOEs.InRadius(actor.Position, _wreathAOERadius).Any())
                        {
                            hints.Add("GTFO from AOE!");
                        }
                        break;
                }
            }

            public override void DrawArenaBackground(MiniArena arena)
            {
                if (CurState == State.FirstAOEs || CurState == State.LastAOEs)
                    foreach (var aoe in CurState == State.FirstAOEs ? _firstAOEs : _lastAOEs)
                        arena.ZoneCircle(aoe.Position, _wreathAOERadius, arena.ColorAOE);
            }

            public override void DrawArenaForeground(MiniArena arena)
            {
                if (CurState == State.Towers)
                {
                    foreach (var tower in _towers)
                        arena.AddCircle(tower.Position, _wreathTowerRadius, arena.ColorSafe);
                    foreach ((_, var player) in _module.IterateRaidMembers())
                        arena.Actor(player, arena.ColorPlayerGeneric);
                }
            }

            public override void OnTethered(WorldState.Actor actor)
            {
                if (CurState != State.Inactive && actor.OID == (uint)OID.Helper && actor.Tether.ID == (uint)TetherID.WreathOfThorns)
                    _relevantHelpers.Add(actor);
            }

            public override void OnCastFinished(WorldState.Actor actor)
            {
                if (CurState == State.FirstAOEs && actor.CastInfo!.IsSpell(AID.AkanthaiExplodeAOE))
                    CurState = State.Towers;
                else if (CurState == State.Towers && actor.CastInfo!.IsSpell(AID.AkanthaiExplodeTower))
                    CurState = State.LastAOEs;
                else if (CurState == State.LastAOEs && actor.CastInfo!.IsSpell(AID.AkanthaiExplodeAOE))
                    CurState = State.Inactive;
            }
        }

        // state related to act 2 wreath of thorns
        // note: we assume that (1) dark targets soak all towers, (2) first fire to be broken is tank-healer pair (since their debuff is slightly shorter)
        // theoretically second set of towers could be soaked by first fire pair, but whatever...
        private class WreathOfThorns2 : Component
        {
            public enum State { Inactive, DarkDesign, FirstSet, SecondSet }
            public enum TetherType { None, Wind, FireDD, FireTH, Dark } // sorted in break priority order

            public State CurState { get; private set; } = State.Inactive;

            private P4S _module;
            private List<WorldState.Actor> _helpers;
            private List<WorldState.Actor> _relevantHelpers = new(); // 2 aoes -> 8 towers -> 2 aoes
            private IconID[] _playerIcons = new IconID[8];
            private int _numAOECasts = 0;

            private IEnumerable<WorldState.Actor> _firstSet => _relevantHelpers.Take(4);
            private IEnumerable<WorldState.Actor> _secondSet => _relevantHelpers.Skip(4);

            private static float _fireExplosionRadius = 6;

            public WreathOfThorns2(P4S module)
            {
                _module = module;
                _helpers = module.Enemies(OID.Helper);
            }

            public void Begin()
            {
                CurState = State.DarkDesign;
                // there should be four tethered helpers
                _relevantHelpers = _helpers.Where(actor => actor.Tether.ID == (uint)TetherID.WreathOfThorns).ToList();
            }

            public override void Reset()
            {
                CurState = State.Inactive;
                _relevantHelpers.Clear();
                Array.Fill(_playerIcons, IconID.None);
                _numAOECasts = 0;
            }

            public override void AddHints(int slot, WorldState.Actor actor, TextHints hints, MovementHints? movementHints)
            {
                if (CurState == State.Inactive)
                    return;

                if (CurState == State.DarkDesign)
                {
                    var darkSource = _module.IterateRaidMembers().Where(ip => _playerIcons[ip.Item1] == IconID.AkanthaiDark).FirstOrDefault().Item2;
                    if (actor == darkSource || actor.InstanceID == darkSource?.Tether.Target)
                    {
                        hints.Add("Break tether!");
                    }
                    else
                    {
                        hints.Add("Stay in center", false);
                    }
                }
                else
                {
                    (var nextTetherSource, var nextTetherType) = ActiveTethers().MaxBy(pt => pt.Item2);
                    bool isNextTether = actor == nextTetherSource || actor.InstanceID == nextTetherSource?.Tether.Target;
                    if (isNextTether)
                    {
                        hints.Add("Break tether!");
                    }

                    var relevantHelpers = CurState == State.FirstSet ? _firstSet : _secondSet;
                    if (relevantHelpers.Where(IsAOE).InRadius(actor.Position, _wreathAOERadius).Any())
                    {
                        hints.Add("GTFO from AOE!");
                    }

                    var soakedTower = relevantHelpers.Where(IsTower).InRadius(actor.Position, _wreathTowerRadius).FirstOrDefault();
                    if (_playerIcons[slot] != IconID.AkanthaiWind && _playerIcons[slot] != IconID.AkanthaiFire)
                    {
                        // note: we're assuming that players with 'dark' soak all towers
                        hints.Add("Soak the tower!", soakedTower == null);
                    }
                    else
                    {
                        if (soakedTower != null)
                        {
                            hints.Add("GTFO from tower!");
                        }

                        if (!isNextTether && nextTetherSource != null && (nextTetherType == TetherType.FireDD || nextTetherType == TetherType.FireTH))
                        {
                            bool nearFire = GeometryUtils.PointInCircle(nextTetherSource.Position - actor.Position, _fireExplosionRadius);
                            if (!nearFire)
                            {
                                var nextTetherTarget = _module.WorldState.FindActor(nextTetherSource.Tether.Target);
                                nearFire = nextTetherTarget != null && GeometryUtils.PointInCircle(nextTetherTarget.Position - actor.Position, _fireExplosionRadius);
                            }
                            hints.Add("Stack with breaking tether!", !nearFire);
                        }
                    }
                }
            }

            public override void DrawArenaBackground(MiniArena arena)
            {
                if (CurState == State.Inactive)
                    return;

                foreach (var aoe in (CurState == State.SecondSet ? _secondSet : _firstSet).Where(IsAOE))
                    arena.ZoneCircle(aoe.Position, _wreathAOERadius, arena.ColorAOE);
            }

            public override void DrawArenaForeground(MiniArena arena)
            {
                if (CurState == State.Inactive)
                    return;

                foreach (var tower in (CurState == State.SecondSet ? _secondSet : _firstSet).Where(IsTower))
                    arena.AddCircle(tower.Position, _wreathTowerRadius, arena.ColorSafe);

                foreach ((int i, var player) in _module.IterateRaidMembers())
                {
                    arena.Actor(player, arena.ColorPlayerGeneric);
                    var tetherTarget = player.Tether.Target != 0 ? _module.WorldState.FindActor(player.Tether.Target) : null;
                    if (tetherTarget != null)
                    {
                        bool breaking = player.Tether.ID == (uint)TetherID.WreathOfThorns;
                        arena.AddLine(player.Position, tetherTarget.Position, breaking ? arena.ColorDanger : arena.ColorSafe);
                        if (breaking && _playerIcons[i] == IconID.AkanthaiFire)
                        {
                            arena.AddCircle(player.Position, _fireExplosionRadius, arena.ColorDanger);
                            arena.AddCircle(tetherTarget.Position, _fireExplosionRadius, arena.ColorDanger);
                        }
                    }
                }
            }

            public override void OnTethered(WorldState.Actor actor)
            {
                if (CurState != State.Inactive && actor.OID == (uint)OID.Helper && actor.Tether.ID == (uint)TetherID.WreathOfThorns)
                    _relevantHelpers.Add(actor);
            }

            public override void OnCastFinished(WorldState.Actor actor)
            {
                if (CurState == State.DarkDesign && actor.CastInfo!.IsSpell(AID.DarkDesign))
                    CurState = State.FirstSet;
                else if (CurState == State.FirstSet && actor.CastInfo!.IsSpell(AID.AkanthaiExplodeAOE) && ++_numAOECasts >= 2)
                    CurState = State.SecondSet;
                else if (CurState == State.SecondSet && actor.CastInfo!.IsSpell(AID.AkanthaiExplodeAOE) && ++_numAOECasts >= 4)
                    CurState = State.Inactive;
            }

            public override void OnEventIcon(uint actorID, uint iconID)
            {
                if (CurState != State.Inactive)
                {
                    var slot = _module.FindRaidMemberSlot(actorID);
                    if (slot >= 0)
                        _playerIcons[slot] = (IconID)iconID;
                }
            }

            private bool IsTower(WorldState.Actor actor)
            {
                if (actor.Position.X < 90)
                    return actor.Position.Z > 100;
                else if (actor.Position.Z < 90)
                    return actor.Position.X < 100;
                else if (actor.Position.X > 110)
                    return actor.Position.Z < 100;
                else if (actor.Position.Z > 110)
                    return actor.Position.X > 100;
                else
                    return false;
            }

            private bool IsAOE(WorldState.Actor actor)
            {
                if (actor.Position.X < 90)
                    return actor.Position.Z < 100;
                else if (actor.Position.Z < 90)
                    return actor.Position.X > 100;
                else if (actor.Position.X > 110)
                    return actor.Position.Z > 100;
                else if (actor.Position.Z > 110)
                    return actor.Position.X < 100;
                else
                    return false;
            }

            private TetherType TetherPriority(WorldState.Actor player, IconID icon)
            {
                return icon switch
                {
                    IconID.AkanthaiDark => TetherType.Dark,
                    IconID.AkanthaiWind => TetherType.Wind,
                    IconID.AkanthaiFire => (player.Role == WorldState.ActorRole.Tank || player.Role == WorldState.ActorRole.Healer) ? TetherType.FireTH : TetherType.FireDD,
                    _ => TetherType.None
                };
            }

            private IEnumerable<(WorldState.Actor, TetherType)> ActiveTethers()
            {
                return _module.IterateRaidMembers()
                    .Select(ip => (ip.Item2, TetherPriority(ip.Item2, _playerIcons[ip.Item1])))
                    .Where(it => it.Item2 != TetherType.None);
            }
        }

        // state related to act 3 wreath of thorns
        private class WreathOfThorns3 : Component
        {
            public enum State { Inactive, RangedTowers, Knockback, MeleeTowers, Done }
            public State CurState { get; private set; } = State.Inactive;
            public int NumJumps { get; private set; } = 0;
            public int NumCones { get; private set; } = 0;

            private P4S _module;
            private List<WorldState.Actor> _helpers;
            private List<WorldState.Actor> _relevantHelpers = new(); // 4 towers -> knockback -> 4 towers
            private WorldState.Actor? _jumpTarget = null; // either predicted (if jump is imminent) or last actual (if cones are imminent)
            private ulong _coneTargets = 0;
            private ulong _playersInAOE = 0;

            private IEnumerable<WorldState.Actor> _rangedTowers => _relevantHelpers.Take(4);
            private IEnumerable<WorldState.Actor> _knockbackThorn => _relevantHelpers.Skip(4).Take(1);
            private IEnumerable<WorldState.Actor> _meleeTowers => _relevantHelpers.Skip(5);

            private static float _jumpAOERadius = 10;
            private static float _coneHalfAngle = MathF.PI / 4; // not sure about this...

            public WreathOfThorns3(P4S module)
            {
                _module = module;
                _helpers = module.Enemies(OID.Helper);
            }

            public void Begin()
            {
                CurState = State.RangedTowers;
                // there should be four tethered helpers
                _relevantHelpers = _helpers.Where(actor => actor.Tether.ID == (uint)TetherID.WreathOfThorns).ToList();
            }

            public override void Reset()
            {
                CurState = State.Inactive;
                NumJumps = NumCones = 0;
                _relevantHelpers.Clear();
                _jumpTarget = null;
            }

            public override void Update()
            {
                _coneTargets = _playersInAOE = 0;
                var boss = _module.Boss2();
                if (CurState == State.Inactive || boss == null)
                    return;

                if (NumCones == NumJumps)
                {
                    _jumpTarget = _module.IterateRaidMembers().SortedByRange(boss.Position).LastOrDefault().Item2;
                    _playersInAOE = _jumpTarget != null ? BuildMask(_module.IterateRaidMembersInRange(_jumpTarget.Position, _jumpAOERadius).Where(ip => ip.Item2 != _jumpTarget)) : 0;
                }
                else
                {
                    foreach ((int i, var player) in _module.IterateRaidMembers().SortedByRange(boss.Position).Take(3))
                    {
                        BitVector.SetVector64Bit(ref _coneTargets, i);
                        if (player.Position != boss.Position)
                        {
                            var direction = Vector3.Normalize(player.Position - boss.Position);
                            _playersInAOE |= BuildMask(_module.IterateOtherRaidMembers(i).Where(ip => GeometryUtils.PointInCone(ip.Item2.Position - boss.Position, direction, _coneHalfAngle)));
                        }
                    }
                }
            }

            public override void AddHints(int slot, WorldState.Actor actor, TextHints hints, MovementHints? movementHints)
            {
                if (CurState == State.Inactive)
                    return;

                if (CurState != State.Done)
                {
                    // TODO: consider raid comps with 3+ melee or ranged...
                    bool shouldSoakTower = CurState == State.RangedTowers
                        ? (actor.Role == WorldState.ActorRole.Ranged || actor.Role == WorldState.ActorRole.Healer)
                        : (actor.Role == WorldState.ActorRole.Melee || actor.Role == WorldState.ActorRole.Tank);
                    var soakedTower = (CurState == State.RangedTowers ? _rangedTowers : _meleeTowers).InRadius(actor.Position, _wreathTowerRadius).FirstOrDefault();
                    if (shouldSoakTower)
                    {
                        hints.Add("Soak the tower!", soakedTower == null);
                    }
                    else if (soakedTower != null)
                    {
                        hints.Add("GTFO from tower!");
                    }
                }

                if (BitVector.IsVector64BitSet(_playersInAOE, slot))
                {
                    hints.Add("GTFO from aoe!");
                }
                if (NumCones == NumJumps && actor == _jumpTarget && _playersInAOE != 0)
                {
                    hints.Add("GTFO from raid!");
                }
                if (NumCones != NumJumps && actor == _jumpTarget && BitVector.IsVector64BitSet(_coneTargets, slot))
                {
                    hints.Add("GTFO from boss!");
                }
            }

            public override void DrawArenaBackground(MiniArena arena)
            {
                var boss = _module.Boss2();
                if (_coneTargets != 0 && boss != null)
                {
                    foreach ((_, var player) in _module.IterateRaidMembers().Where(ip => BitVector.IsVector64BitSet(_coneTargets, ip.Item1) && boss.Position != ip.Item2.Position))
                    {
                        var offset = player.Position - boss.Position;
                        float phi = MathF.Atan2(offset.X, offset.Z);
                        arena.ZoneCone(boss.Position, 0, 50, phi - _coneHalfAngle, phi + _coneHalfAngle, arena.ColorAOE);
                    }
                }
            }

            public override void DrawArenaForeground(MiniArena arena)
            {
                if (CurState == State.Inactive)
                    return;

                foreach ((int i, var player) in _module.IterateRaidMembers())
                    arena.Actor(player, BitVector.IsVector64BitSet(_playersInAOE, i) ? arena.ColorPlayerInteresting : arena.ColorPlayerGeneric);

                if (CurState != State.Done)
                {
                    foreach (var tower in (CurState == State.RangedTowers ? _rangedTowers : _meleeTowers))
                        arena.AddCircle(tower.Position, _wreathTowerRadius, arena.ColorSafe);
                }

                if (NumCones != NumJumps)
                {
                    foreach ((_, var player) in _module.IterateRaidMembers().Where(ip => BitVector.IsVector64BitSet(_coneTargets, ip.Item1)))
                        arena.Actor(player, arena.ColorDanger);
                    arena.Actor(_jumpTarget, arena.ColorVulnerable);
                }
                else if (_jumpTarget != null)
                {
                    arena.Actor(_jumpTarget, arena.ColorDanger);
                    arena.AddCircle(_jumpTarget.Position, _jumpAOERadius, arena.ColorDanger);
                }
            }

            public override void OnTethered(WorldState.Actor actor)
            {
                if (CurState != State.Inactive && actor.OID == (uint)OID.Helper && actor.Tether.ID == (uint)TetherID.WreathOfThorns)
                    _relevantHelpers.Add(actor);
            }

            public override void OnCastFinished(WorldState.Actor actor)
            {
                if (CurState == State.RangedTowers && actor.CastInfo!.IsSpell(AID.AkanthaiExplodeTower))
                    CurState = State.Knockback;
                else if (CurState == State.Knockback && actor.CastInfo!.IsSpell(AID.AkanthaiExplodeKnockback))
                    CurState = State.MeleeTowers;
                else if (CurState == State.MeleeTowers && actor.CastInfo!.IsSpell(AID.AkanthaiExplodeTower))
                    CurState = State.Done;
            }

            public override void OnEventCast(WorldState.CastResult info)
            {
                if (info.IsSpell(AID.KothornosKickJump))
                {
                    ++NumJumps;
                    _jumpTarget = _module.WorldState.FindActor(info.MainTargetID);
                }
                else if (info.IsSpell(AID.KothornosQuake1) || info.IsSpell(AID.KothornosQuake2))
                {
                    ++NumCones;
                }
            }
        }

        // state related to act 4 wreath of thorns
        // note: we assume that aoes are popped in waymark order...
        private class WreathOfThorns4 : Component
        {
            public bool Active = false;

            private P4S _module;
            private IconID[] _playerIcons = new IconID[8];
            private WorldState.Actor?[] _playerTetherSource = new WorldState.Actor?[8];
            private int _doneTowers = 0;
            private int _doneAOEs = 0;

            private static float _waterExplosionRange = 10;

            public WreathOfThorns4(P4S module)
            {
                _module = module;
            }

            public override void Reset()
            {
                Active = false;
                Array.Fill(_playerIcons, IconID.None);
                Array.Fill(_playerTetherSource, null);
                _doneTowers = _doneAOEs = 0;
            }

            public override void AddHints(int slot, WorldState.Actor actor, TextHints hints, MovementHints? movementHints)
            {
                if (!Active)
                    return;

                if (_doneTowers < 4)
                {
                    if (_playerIcons[slot] == IconID.AkanthaiWater)
                    {
                        hints.Add("Break tether!");
                        if (_module.IterateRaidMembersInRange(slot, _waterExplosionRange).Any())
                        {
                            hints.Add("GTFO from others!");
                        }
                    }
                    else
                    {
                        var soakedTower = _playerTetherSource.Zip(_playerIcons).Where(si => si.Item1 != null && si.Item2 == IconID.AkanthaiWater).Select(si => si.Item1!).InRadius(actor.Position, _wreathTowerRadius).FirstOrDefault();
                        hints.Add("Soak the tower!", soakedTower == null);
                    }
                }
                else
                {
                    var nextAOE = NextAOE();
                    if (nextAOE != null)
                    {
                        if (nextAOE.Tether.Target == actor.InstanceID)
                        {
                            hints.Add("Break tether!");
                        }
                        if (GeometryUtils.PointInCircle(actor.Position - nextAOE.Position, _wreathAOERadius))
                        {
                            hints.Add("GTFO from AOE!");
                        }
                    }
                }
            }

            public override void DrawArenaBackground(MiniArena arena)
            {
                if (!Active || _doneTowers < 4)
                    return;

                var nextAOE = NextAOE();
                if (nextAOE != null)
                    arena.ZoneCircle(nextAOE.Position, _wreathAOERadius, arena.ColorAOE);
            }

            public override void DrawArenaForeground(MiniArena arena)
            {
                if (!Active)
                    return;

                var nextAOE = _doneTowers < 4 ? null : NextAOE();
                foreach (((var player, var icon), var source) in _module.RaidMembers.Zip(_playerIcons).Zip(_playerTetherSource))
                {
                    arena.Actor(player, arena.ColorPlayerGeneric);
                    if (player == null || source == null)
                        continue;

                    if (icon == IconID.AkanthaiWater)
                    {
                        arena.AddCircle(source.Position, _wreathTowerRadius, arena.ColorSafe);
                        arena.AddCircle(player.Position, _waterExplosionRange, arena.ColorDanger);
                        arena.AddLine(player.Position, source.Position, source.Tether.ID == (uint)TetherID.WreathOfThorns ? arena.ColorDanger : arena.ColorSafe);
                    }
                    else if (source == nextAOE)
                    {
                        arena.AddLine(player.Position, source.Position, source.Tether.ID == (uint)TetherID.WreathOfThorns ? arena.ColorDanger : arena.ColorSafe);
                    }
                }
            }

            public override void OnTethered(WorldState.Actor actor)
            {
                if (Active && actor.OID == (uint)OID.Helper)
                {
                    var slot = _module.FindRaidMemberSlot(actor.Tether.Target);
                    if (slot >= 0)
                        _playerTetherSource[slot] = actor;
                }
            }

            public override void OnUntethered(WorldState.Actor actor)
            {
                if (Active && actor.OID == (uint)OID.Helper)
                {
                    var slot = _module.FindRaidMemberSlot(actor.Tether.Target);
                    if (slot >= 0)
                        _playerTetherSource[slot] = null;
                }
            }

            public override void OnCastFinished(WorldState.Actor actor)
            {
                if (Active && actor.CastInfo!.IsSpell(AID.AkanthaiExplodeTower))
                    ++_doneTowers;
                if (Active && actor.CastInfo!.IsSpell(AID.AkanthaiExplodeAOE))
                    ++_doneAOEs;
            }

            public override void OnEventIcon(uint actorID, uint iconID)
            {
                if (Active)
                {
                    var slot = _module.FindRaidMemberSlot(actorID);
                    if (slot >= 0)
                        _playerIcons[slot] = (IconID)iconID;
                }
            }

            private WorldState.Actor? NextAOE()
            {
                // note: this is quite a hack to be honest...
                var pos = _doneAOEs < 4 ? _module.WorldState.GetWaymark((WorldState.Waymark)((int)WorldState.Waymark.N1 + _doneAOEs)) : null;
                return pos != null ? _playerTetherSource.Zip(_playerIcons).Where(si => si.Item1 != null && si.Item2 == IconID.AkanthaiDark).Select(si => si.Item1!).MinBy(s => (s.Position - pos.Value).LengthSquared()) : null;
            }
        }

        // state related to act 5 (finale) wreath of thorns
        private class WreathOfThorns5 : Component
        {
            public bool Active = false;

            private P4S _module;
            private List<uint> _playersOrder = new();
            private List<WorldState.Actor> _towersOrder = new();
            private int _castsDone = 0;

            private static float _impulseAOERadius = 5;

            public WreathOfThorns5(P4S module)
            {
                _module = module;
            }

            public override void Reset()
            {
                Active = false;
                _playersOrder.Clear();
                _towersOrder.Clear();
                _castsDone = 0;
            }

            public override void AddHints(int slot, WorldState.Actor actor, TextHints hints, MovementHints? movementHints)
            {
                if (!Active)
                    return;

                int order = _playersOrder.IndexOf(actor.InstanceID);
                if (order >= 0)
                {
                    hints.Add($"Order: {order}", false);

                    if (order >= _castsDone && order < _towersOrder.Count)
                    {
                        hints.Add("Soak tower!", !GeometryUtils.PointInCircle(actor.Position - _towersOrder[order].Position, _wreathTowerRadius));
                    }
                }

                if (_playersOrder.Count < 8)
                {
                    hints.Add("Spread!", _module.IterateRaidMembersInRange(slot, _impulseAOERadius).Any());
                }
            }

            public override void DrawArenaForeground(MiniArena arena)
            {
                var pc = _module.Player();
                if (!Active || pc == null)
                    return;

                int order = _playersOrder.IndexOf(pc.InstanceID);
                if (order >= _castsDone && order < _towersOrder.Count)
                    arena.AddCircle(_towersOrder[order].Position, _wreathTowerRadius, arena.ColorSafe);

                if (_playersOrder.Count < 8)
                {
                    arena.AddCircle(pc.Position, _impulseAOERadius, arena.ColorDanger);
                    foreach ((_, var player) in _module.IterateOtherRaidMembers(_module.PlayerSlot))
                        arena.Actor(player, GeometryUtils.PointInCircle(player.Position - pc.Position, _impulseAOERadius) ? arena.ColorPlayerInteresting : arena.ColorPlayerGeneric);
                }
            }

            public override void OnTethered(WorldState.Actor actor)
            {
                if (Active && actor.OID == (uint)OID.Helper)
                    _towersOrder.Add(actor);
            }

            public override void OnEventCast(WorldState.CastResult info)
            {
                if (!Active)
                    return;
                if (info.IsSpell(AID.FleetingImpulseAOE))
                    _playersOrder.Add(info.MainTargetID);
                else if (info.IsSpell(AID.AkanthaiExplodeTower))
                    ++_castsDone;
            }
        }

        // state related to curtain call mechanic
        // TODO: unhardcode relative order in pairs, currently tanks/healers pop first...
        private class CurtainCall : Component
        {
            private P4S _module;
            private bool _active = false;
            private int[] _playerOrder = new int[8];
            private int _numCasts = 0;

            public CurtainCall(P4S module)
            {
                _module = module;
            }

            public void Restart()
            {
                _active = true;
                _numCasts = 0;
                Array.Fill(_playerOrder, 0);
            }

            public override void Reset()
            {
                _active = false;
                _numCasts = 0;
                Array.Fill(_playerOrder, 0);
            }

            public override void AddHints(int slot, WorldState.Actor actor, TextHints hints, MovementHints? movementHints)
            {
                if (_active && _playerOrder[slot] > _numCasts)
                {
                    var relOrder = _playerOrder[slot] - _numCasts;
                    hints.Add($"Tether break order: {relOrder}", relOrder == 1);
                }
            }

            public override void DrawArenaForeground(MiniArena arena)
            {
                var pc = _module.Player();
                if (_active && _playerOrder[_module.PlayerSlot] > _numCasts && pc != null)
                {
                    var tetherTarget = pc.Tether.Target != 0 ? _module.WorldState.FindActor(pc.Tether.Target) : null;
                    if (tetherTarget != null)
                        arena.AddLine(pc.Position, tetherTarget.Position, pc.Tether.ID == (uint)TetherID.WreathOfThorns ? arena.ColorDanger : arena.ColorSafe);
                }
            }

            public override void OnStatusGain(WorldState.Actor actor, int index)
            {
                if (_active && actor.Statuses[index].ID == (uint)SID.Thornpricked)
                {
                    int slot = _module.FindRaidMemberSlot(actor.InstanceID);
                    if (slot >= 0)
                    {
                        _playerOrder[slot] = 2 * (int)(actor.Statuses[index].RemainingTime / 10); // 2/4/6/8
                        if (actor.Role == WorldState.ActorRole.Tank || actor.Role == WorldState.ActorRole.Healer)
                            --_playerOrder[slot];
                    }
                }
            }

            public override void OnStatusLose(WorldState.Actor actor, int index)
            {
                if (_active && actor.Statuses[index].ID == (uint)SID.Thornpricked)
                    ++_numCasts;
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
            RegisterEnemies(OID.Helper);

            RegisterComponent(new ElegantEvisceration());
            RegisterComponent(new BeloneCoils(this));
            RegisterComponent(new InversiveChlamys(this));
            RegisterComponent(new DirectorsBelone(this));
            RegisterComponent(new Pinax(this));
            RegisterComponent(new Shift(this));
            RegisterComponent(new VengefulBelone(this));
            RegisterComponent(new ElementalBelone(this));
            RegisterComponent(new NearFarSight(this));
            RegisterComponent(new HeartStake());
            RegisterComponent(new HellsSting(this));
            RegisterComponent(new WreathOfThorns1(this));
            RegisterComponent(new WreathOfThorns2(this));
            RegisterComponent(new WreathOfThorns3(this));
            RegisterComponent(new WreathOfThorns4(this));
            RegisterComponent(new WreathOfThorns5(this));
            RegisterComponent(new CurtainCall(this));

            BuildPhase1States();
            BuildPhase2States();

            var fork = CommonStates.Timeout(ref InitialState, 0);
            fork.PotentialSuccessors = new[] { _phase1Start!, _phase2Start! };
        }

        protected override void ResetModule()
        {
            InitialState!.Next = Boss1() != null ? _phase1Start : _phase2Start;
        }

        protected override void DrawArenaForegroundPost()
        {
            Arena.Actor(Boss1() ?? Boss2(), Arena.ColorEnemy);
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
            s = CommonStates.Targetable(ref s.Next, Boss1, false, 10, "Enrage"); // checkpoint is triggered by boss becoming untargetable...
        }

        private StateMachine.State BuildDecollationState(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, Boss1, AID.Decollation, delay, 5, "AOE");
            s.EndHint |= StateMachine.StateHint.Raidwide;
            return s;
        }

        private StateMachine.State BuildElegantEviscerationState(ref StateMachine.State? link, float delay)
        {
            var comp = FindComponent<ElegantEvisceration>()!;
            var cast = CommonStates.Cast(ref link, Boss1, AID.ElegantEvisceration, delay, 5, "Tankbuster");
            cast.Exit = comp.Reset;
            cast.EndHint |= StateMachine.StateHint.Tankbuster | StateMachine.StateHint.GroupWithNext;

            var second = CommonStates.Condition(ref cast.Next, 3, () => comp.NumCasts > 0, "Tankbuster");
            second.Exit = comp.Reset;
            second.EndHint |= StateMachine.StateHint.Tankbuster;
            return second;
        }

        private StateMachine.State BuildInversiveChlamysState(ref StateMachine.State? link, float delay)
        {
            var comp = FindComponent<InversiveChlamys>()!;
            var cast = CommonStates.Cast(ref link, Boss1, AID.InversiveChlamys, delay, 7);
            var resolve = CommonStates.Condition(ref cast.Next, 0.8f, () => !comp.TethersActive, "Chlamys");
            return resolve;
        }

        private StateMachine.State BuildBloodrakeBeloneStates(ref StateMachine.State? link, float delay)
        {
            // note: just before (~0.1s) every bloodrake cast start, its targets are tethered to boss
            // targets of first bloodrake will be killed if they are targets of chlamys tethers later
            var bloodrake1 = CommonStates.Cast(ref link, Boss1, AID.Bloodrake, delay, 4, "Bloodrake 1");
            bloodrake1.Enter = FindComponent<InversiveChlamys>()!.AssignFromBloodrake;
            bloodrake1.EndHint |= StateMachine.StateHint.GroupWithNext;

            // this cast is pure flavour and does nothing (replaces status 2799 'Aethersucker' with status 2800 'Casting Chlamys' on boss)
            var aetheric = CommonStates.Cast(ref bloodrake1.Next, Boss1, AID.AethericChlamys, 3.2f, 4);

            // targets of second bloodrake will be killed if they are targets of 'Cursed Casting' (which targets players with 'Role Call')
            var bloodrake2 = CommonStates.Cast(ref aetheric.Next, Boss1, AID.Bloodrake, 4.2f, 4, "Bloodrake 2");
            bloodrake2.Enter = FindComponent<DirectorsBelone>()!.AssignFromBloodrake;
            bloodrake2.EndHint |= StateMachine.StateHint.GroupWithNext;

            // this cast removes status 2799 'Aethersucker' from boss
            // right after it ends, instant cast 27111 applies 'Role Call' debuffs - corresponding component handles that
            var belone = CommonStates.Cast(ref bloodrake2.Next, Boss1, AID.DirectorsBelone, 4.2f, 5);

            // Cursed Casting happens right before (0.5s) chlamys resolve
            var inv = BuildInversiveChlamysState(ref belone.Next, 9.2f);
            inv.Exit = () =>
            {
                FindComponent<InversiveChlamys>()!.Reset();
                FindComponent<DirectorsBelone>()!.Reset();
            };
            return inv;
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

            // all other bloodrakes target all players
            // third bloodrake in addition 'targets' three of the four corner helpers - untethered one is safe during later mechanic
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
            var bloodrake5 = CommonStates.Cast(ref link, Boss1, AID.Bloodrake, delay, 4, "Bloodrake 5");
            bloodrake5.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.Raidwide;

            var coils1 = CommonStates.Cast(ref bloodrake5.Next, Boss1, AID.BeloneCoils, 3.2f, 4, "Coils 1");
            coils1.Exit = FindComponent<InversiveChlamys>()!.AssignFromCoils;
            coils1.EndHint |= StateMachine.StateHint.GroupWithNext;

            var inv1 = BuildInversiveChlamysState(ref coils1.Next, 3.2f);
            inv1.EndHint |= StateMachine.StateHint.GroupWithNext;

            var aetheric = CommonStates.Cast(ref inv1.Next, Boss1, AID.AethericChlamys, 2.4f, 4);

            var bloodrake6 = CommonStates.Cast(ref aetheric.Next, Boss1, AID.Bloodrake, 4.2f, 4, "Bloodrake 6");
            bloodrake6.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.Raidwide;

            var coils2 = CommonStates.Cast(ref bloodrake6.Next, Boss1, AID.BeloneCoils, 4.2f, 4, "Coils 2");
            coils2.Exit = FindComponent<DirectorsBelone>()!.AssignFromCoils;
            coils2.EndHint |= StateMachine.StateHint.GroupWithNext;

            var belone = CommonStates.Cast(ref coils2.Next, Boss1, AID.DirectorsBelone, 9.2f, 5);

            var inv2 = BuildInversiveChlamysState(ref belone.Next, 9.2f);
            inv2.Exit = () =>
            {
                FindComponent<InversiveChlamys>()!.Reset();
                FindComponent<DirectorsBelone>()!.Reset();
            };
            return inv2;
        }

        private void BuildPhase2States()
        {
            StateMachine.State? s;
            s = BuildSearingStreamState(ref _phase2Start, 10);
            s = BuildAkanthaiAct1States(ref s.Next, 10.2f);
            s = BuildFarNearSightState(ref s.Next, 1);
            s = BuildAkanthaiAct2States(ref s.Next, 7.1f);
            s = BuildUltimateImpulseState(ref s.Next, 0.3f);
            s = BuildAkanthaiAct3States(ref s.Next, 8.2f);
            s = BuildFarNearSightState(ref s.Next, 4.1f);
            s = BuildHeartStakeState(ref s.Next, 9.2f);
            s = BuildAkanthaiAct4States(ref s.Next, 4.2f);
            s = BuildSearingStreamState(ref s.Next, 9.3f);
            s = BuildAkanthaiAct5States(ref s.Next, 4.2f);
            s = BuildSearingStreamState(ref s.Next, 7.2f);
            s = BuildDemigodDoubleState(ref s.Next, 4.2f);
            s = BuildAkanthaiAct6States(ref s.Next, 8.2f);
            s = CommonStates.Cast(ref s.Next, Boss2, AID.Enrage, 4.4f, 10, "Enrage"); // not sure whether it's really an enrage, but it's unique aid with 10 sec cast...
        }

        private StateMachine.State BuildSearingStreamState(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, Boss2, AID.SearingStream, delay, 5, "AOE");
            s.EndHint |= StateMachine.StateHint.Raidwide;
            return s;
        }

        private StateMachine.State BuildUltimateImpulseState(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, Boss2, AID.UltimateImpulse, delay, 7, "AOE");
            s.EndHint |= StateMachine.StateHint.Raidwide;
            return s;
        }

        private StateMachine.State BuildFarNearSightState(ref StateMachine.State? link, float delay)
        {
            var comp = FindComponent<NearFarSight>()!;

            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.Nearsight] = new(null, () => comp.CurState = NearFarSight.State.Near);
            dispatch[AID.Farsight] = new(null, () => comp.CurState = NearFarSight.State.Far);
            var start = CommonStates.CastStart(ref link, Boss2, dispatch, delay);

            var end = CommonStates.CastEnd(ref start.Next, Boss2, 5);

            var resolve = CommonStates.Condition(ref end.Next, 1.1f, () => comp.CurState == NearFarSight.State.None, "Far/nearsight");
            resolve.Exit = comp.Reset;
            resolve.EndHint |= StateMachine.StateHint.Tankbuster;
            return resolve;
        }

        private StateMachine.State BuildDemigodDoubleState(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, Boss2, AID.DemigodDouble, delay, 5, "SharedTankbuster");
            s.EndHint |= StateMachine.StateHint.Tankbuster;
            return s;
        }

        private StateMachine.State BuildHeartStakeState(ref StateMachine.State? link, float delay)
        {
            var comp = FindComponent<HeartStake>()!;
            var cast = CommonStates.Cast(ref link, Boss2, AID.HeartStake, delay, 5, "Tankbuster");
            cast.Exit = comp.Reset;
            cast.EndHint |= StateMachine.StateHint.Tankbuster | StateMachine.StateHint.GroupWithNext;

            var second = CommonStates.Condition(ref cast.Next, 3.1f, () => comp.NumCasts > 0, "Tankbuster");
            second.Exit = comp.Reset;
            second.EndHint |= StateMachine.StateHint.Tankbuster;
            return second;
        }

        private StateMachine.State BuildHellStingStates(ref StateMachine.State? link, float delay)
        {
            // timeline:
            // 0.0s: cast start (boss visual + helpers real)
            // 2.4s: visual cast end
            // 3.0s: first aoes (helpers cast end)
            // 5.5s: boss visual instant cast + helpers start cast
            // 6.1s: second aoes (helpers cast end)
            var comp = FindComponent<HellsSting>()!;
            var cast = CommonStates.Cast(ref link, Boss2, AID.HellsSting, delay, 2.4f);
            cast.Enter = () => comp.Start(Boss2()?.Rotation ?? 0);

            var hit1 = CommonStates.Condition(ref cast.Next, 0.6f, () => comp.NumCasts > 0, "Cone");
            hit1.EndHint |= StateMachine.StateHint.GroupWithNext;

            var hit2 = CommonStates.Condition(ref hit1.Next, 3.1f, () => comp.NumCasts > 1, "Cone");
            hit1.Exit = comp.Reset;
            return hit2;
        }

        private StateMachine.State BuildAkanthaiAct1States(ref StateMachine.State? link, float delay)
        {
            // 'act 1' is 4 aoes (N/S/E/W) and 8 towers; explosion order is 2 opposite aoes -> all towers -> remaining aoes
            // 'intro' cast is pure flavour, it is cast together with 'visual' casts by towers and aoes
            // aoes are at (82/118, 100) and (100, 82/118), towers are at (95.05/104.95, 95.05/104.95) and (88.69/111.31, 88.69/111.31)
            var intro = CommonStates.Cast(ref link, Boss2, AID.AkanthaiAct1, delay, 5, "Act1");
            intro.EndHint |= StateMachine.StateHint.GroupWithNext;

            var aoe = BuildSearingStreamState(ref intro.Next, 4.2f);
            aoe.EndHint |= StateMachine.StateHint.GroupWithNext;

            // timeline:
            // -0.1s: first 2 aoes tethered
            //  0.0s: wreath cast start ==> component determines order and starts showing first aoe pair
            //  3.0s: towers tethered
            //  6.0s: last 2 aoes tethered
            //  8.0s: wreath cast end
            // 10.0s: first 2 aoes start cast 27149
            // 11.0s: first 2 aoes finish cast ==> component starts showing towers
            // 13.0s: towers start cast 27150
            // 14.0s: towers finish cast ==> component starts showing second aoe pair
            // 16.0s: last 2 aoes start cast 27149
            // 17.0s: last 2 aoes finish cast ==> component is reset
            // 18.0s: boss starts casting far/nearsight
            var comp = FindComponent<WreathOfThorns1>()!;
            var wreath = CommonStates.Cast(ref aoe.Next, Boss2, AID.WreathOfThorns1, 6.2f, 8, "Wreath1");
            wreath.Enter = comp.Begin;
            wreath.EndHint |= StateMachine.StateHint.GroupWithNext;

            var aoe1 = CommonStates.Condition(ref wreath.Next, 3, () => comp.CurState != WreathOfThorns1.State.FirstAOEs, "AOE 1");
            aoe1.EndHint |= StateMachine.StateHint.GroupWithNext;

            var aoe2 = CommonStates.Condition(ref aoe1.Next, 3, () => comp.CurState != WreathOfThorns1.State.Towers, "Towers");
            aoe2.EndHint |= StateMachine.StateHint.GroupWithNext;

            var aoe3 = CommonStates.Condition(ref aoe2.Next, 3, () => comp.CurState != WreathOfThorns1.State.LastAOEs, "AOE 2");
            aoe3.Exit = comp.Reset;
            return aoe3;
        }

        private StateMachine.State BuildAkanthaiAct2States(ref StateMachine.State? link, float delay)
        {
            // 'act 2' is 4 aoes and 4 towers + player pairwise tethers
            // 'intro' cast is pure flavour, it is cast together with 'visual' casts by towers and aoes
            // towers are at (96,82), (118,96), (104,118) and (82,104); aoes are at (104,82), (118,104), (96,118) and (82,96)
            var intro = CommonStates.Cast(ref link, Boss2, AID.AkanthaiAct2, delay, 5, "Act2");
            intro.EndHint |= StateMachine.StateHint.GroupWithNext;

            var dd = BuildDemigodDoubleState(ref intro.Next, 4.2f);
            dd.EndHint |= StateMachine.StateHint.GroupWithNext;

            // timeline:
            // -0.1s: two towers and two aoes tethered
            //  0.0s: wreath cast start ==> component determines order and starts showing first set
            //  3.0s: remaining tethers
            //  6.0s: wreath cast end
            //  6.8s: icons + tethers appear (1 dd pair and 1 tank-healer pair with fire, 1 dd pair with wind, 1 tank-healer pair with dark on healer) ==> component starts showing 'break' hint on dark pair and 'stack in center' hint on everyone else
            //  9.2s: dark design cast start
            // 11.8s: 'thornpricked' debuffs
            // 14.2s: dark design cast end ==> component starts showing 'gtfo from aoe/soak tower' hint + 'break' hint for next pair
            // 18.1s: first 2 aoes and towers start cast 27149/27150
            // 19.1s: first 2 aoes and towers finish cast => component starts showing second set
            // 25.1s: last 2 aoes and towers start cast 27149/27150
            // 26.1s: last 2 aoes and towers finish cast => component is reset
            // 26.4s: boss starts casting aoe
            // 27.8s: wind pair expires if not broken
            // 33.4s: boss finishes casting aoe
            var comp = FindComponent<WreathOfThorns2>()!;
            var wreath = CommonStates.Cast(ref dd.Next, Boss2, AID.WreathOfThorns2, 4.2f, 6, "Wreath2");
            wreath.Enter = comp.Begin;
            wreath.EndHint |= StateMachine.StateHint.GroupWithNext;

            var darkDesign = CommonStates.Cast(ref wreath.Next, Boss2, AID.DarkDesign, 3.2f, 5, "DarkDesign");
            darkDesign.EndHint |= StateMachine.StateHint.GroupWithNext;

            var resolve1 = CommonStates.Condition(ref darkDesign.Next, 4.9f, () => comp.CurState != WreathOfThorns2.State.FirstSet, "Resolve 1");
            resolve1.EndHint |= StateMachine.StateHint.GroupWithNext;

            var resolve2 = CommonStates.Condition(ref resolve1.Next, 7, () => comp.CurState != WreathOfThorns2.State.SecondSet, "Resolve 2");
            resolve2.Exit = comp.Reset;
            return resolve2;
        }

        private StateMachine.State BuildAkanthaiAct3States(ref StateMachine.State? link, float delay)
        {
            // 'act 3' is two sets of 4 towers + jumps and knockback from center
            // 'intro' cast is pure flavour, it is cast together with 'visual' casts by towers and knockback
            // towers are at (82.61/117.39, 104.66/95.34) and (87.27/112.73, 87.27/112.73)
            var intro = CommonStates.Cast(ref link, Boss2, AID.AkanthaiAct3, delay, 5, "Act3");
            intro.EndHint |= StateMachine.StateHint.GroupWithNext;

            // timeline:
            // -0.1s: four towers (E/W) tethered
            //  0.0s: wreath cast start ==> component should determine order and show spots for everyone (rdd/healers to soak, some tank to bait jump)
            //  3.0s: center tether
            //  6.0s: remaining tethers
            //  8.0s: wreath cast end
            // 11.2s: kick cast start
            // 16.1s: kick cast end
            // 16.3s: first jump ==> component should switch from jump to cone mode
            // 20.0s: first towers start cast 27150
            // 20.2s: cones 1 go off ==> component should switch to second jump mode
            // 21.0s: first towers finish cast => component should show second towers
            // 22.0s: central tower starts cast 27152
            // 23.0s: central tower finishes cast
            // 26.0s: second towers start cast 27150
            // 26.4s: second jump ==> component should switch to second cone mode
            // 27.0s: second towers finish cast
            // 30.4s: second cones
            var comp = FindComponent<WreathOfThorns3>()!;
            var wreath = CommonStates.Cast(ref intro.Next, Boss2, AID.WreathOfThorns3, 4.2f, 8, "Wreath3");
            wreath.Enter = comp.Begin;
            wreath.EndHint |= StateMachine.StateHint.GroupWithNext;

            var jump1 = CommonStates.Cast(ref wreath.Next, Boss2, AID.KothornosKock, 3.2f, 4.9f, "Jump1");
            jump1.EndHint |= StateMachine.StateHint.GroupWithNext;

            var cones1 = CommonStates.Condition(ref jump1.Next, 4.9f, () => comp.NumCones > 0, "Cones1");
            cones1.EndHint |= StateMachine.StateHint.GroupWithNext;

            var towers1 = CommonStates.Condition(ref cones1.Next, 0.8f, () => comp.CurState != WreathOfThorns3.State.RangedTowers, "Towers1");
            towers1.EndHint |= StateMachine.StateHint.GroupWithNext;

            var knockback = CommonStates.Condition(ref towers1.Next, 2, () => comp.CurState != WreathOfThorns3.State.Knockback, "Knockback");
            knockback.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.Knockback;

            var jump2 = CommonStates.Condition(ref knockback.Next, 3.4f, () => comp.NumJumps > 1, "Jump2");
            jump2.EndHint |= StateMachine.StateHint.GroupWithNext;

            var towers2 = CommonStates.Condition(ref jump2.Next, 0.6f, () => comp.CurState != WreathOfThorns3.State.MeleeTowers, "Towers2");
            towers2.EndHint |= StateMachine.StateHint.GroupWithNext;

            var cones2 = CommonStates.Condition(ref towers2.Next, 3.4f, () => comp.NumCones > 1, "Cones2");
            cones2.Exit = comp.Reset;
            return cones2;
        }

        private StateMachine.State BuildAkanthaiAct4States(ref StateMachine.State? link, float delay)
        {
            // 'act 4' is 4 towers + 4 aoes, tethered to players
            // 'intro' cast is pure flavour, it is cast together with 'visual' casts by towers and aoes
            // towers are at (82/118, 100) and (100, 82/118), aoes are at (87.27/112.73, 87.27/112.73)
            var intro = CommonStates.Cast(ref link, Boss2, AID.AkanthaiAct4, delay, 5, "Act4");
            intro.EndHint |= StateMachine.StateHint.GroupWithNext;

            var aoe1 = BuildSearingStreamState(ref intro.Next, 4.2f);
            aoe1.EndHint |= StateMachine.StateHint.GroupWithNext;

            // timeline:
            //  0.0s: wreath cast ends
            //  0.8s: icons and tethers appear
            //  3.2s: searing stream cast start
            //  5.8s: 'thornpricked' debuffs
            //  8.2s: searing stream cast end
            // .....: blow up tethers
            // 36.4s: ultimate impulse cast start
            var comp = FindComponent<WreathOfThorns4>()!;
            var wreath = CommonStates.Cast(ref aoe1.Next, Boss2, AID.WreathOfThorns4, 4.2f, 5, "Wreath4");
            wreath.Exit = () => comp.Active = true;
            wreath.EndHint |= StateMachine.StateHint.GroupWithNext;

            var aoe2 = BuildSearingStreamState(ref wreath.Next, 3.2f);
            aoe2.EndHint |= StateMachine.StateHint.GroupWithNext;

            var aoe3 = BuildUltimateImpulseState(ref aoe2.Next, 28.2f);
            aoe3.Exit = comp.Reset;
            return aoe3;
        }

        private StateMachine.State BuildAkanthaiAct5States(ref StateMachine.State? link, float delay)
        {
            // 'act 5' ('finale') is 8 staggered towers that should be soaked in correct order
            // 'intro' cast is pure flavour, it is cast together with 'visual' casts by towers
            // towers are at (88/112, 100), (100, 88/112), (91.5/108.5, 91.5/108.5)
            var intro = CommonStates.Cast(ref link, Boss2, AID.AkanthaiFinale, delay, 5, "Act5");
            intro.EndHint |= StateMachine.StateHint.GroupWithNext;

            // timeline:
            //  0.0s: wreath cast ends
            //  0.8s: icons and tethers appear
            //  3.2s: fleeting impulse cast start
            //  5.8s: 'thornpricked' debuffs
            //  8.1s: fleeting impulse cast ends
            //  8.4s: impulse hit 1 (~0.3 from cast end)
            //  9.9s: impulse hit 2 (~1.3 from prev for each next)
            // 11.2s: impulse hit 3
            // 12.5s: impulse hit 4
            // 13.9s: impulse hit 5
            // 15.2s: impulse hit 6
            // 16.6s: impulse hit 7
            // 17.9s: impulse hit 8
            // 18.8s: 'thornpricked' disappear and some sort of instant cast happens, that does nothing if there are no fails
            // 21.6s: first tether for wreath 6; tethers switch every ~0.5s
            // 21.7s: wreath 6 cast start
            // 27.7s: wreath 6 cast end
            // 29.8s: first tower starts 27150 cast
            // 30.8s: first tower finishes cast
            // ... towers are staggered by ~1.3s
            // 38.8s: near/farsight cast start
            // 39.1s: last tower finishes cast
            var comp = FindComponent<WreathOfThorns5>()!;
            var wreath5 = CommonStates.Cast(ref intro.Next, Boss2, AID.WreathOfThorns5, 4.2f, 5, "Wreath5");
            wreath5.Exit = () => comp.Active = true;
            wreath5.EndHint |= StateMachine.StateHint.GroupWithNext;

            var fleeting = CommonStates.Cast(ref wreath5.Next, Boss2, AID.FleetingImpulse, 3.2f, 4.9f, "Impulse");
            fleeting.EndHint |= StateMachine.StateHint.GroupWithNext;

            var wreath6 = CommonStates.Cast(ref fleeting.Next, Boss2, AID.WreathOfThorns6, 13.6f, 6, "Wreath6");
            wreath6.EndHint |= StateMachine.StateHint.GroupWithNext;

            var tb = BuildFarNearSightState(ref wreath6.Next, 11.2f);
            tb.Exit = comp.Reset;
            return tb;
        }

        private StateMachine.State BuildAkanthaiAct6States(ref StateMachine.State? link, float delay)
        {
            // timeline:
            //  0.0s: curtain call cast ends
            //  0.8s: icons and tethers appear
            //  5.8s: 'thornpricked' debuffs with 12/22/32/42 duration
            // 10.2s: hell sting 1 sequence start
            // 16.3s: hell sting 1 sequence end
            // 30.5s: hell sting 2 sequence start
            // 36.6s: hell sting 2 sequence end
            // 45.7s: aoe start
            // 52.7s: aoe end
            // 55.7s: 'thornpricked' debuffs
            // 59.9s: hell sting 3 sequence start
            // 66.0s: hell sting 3 sequence end
            // 80.2s: hell sting 4 sequence start
            // 86.3s: hell sting 4 sequence end
            // 95.4s: aoe start
            var comp = FindComponent<CurtainCall>()!;
            var intro = CommonStates.Cast(ref link, Boss2, AID.AkanthaiCurtainCall, delay, 5, "Act6");
            intro.Exit = comp.Restart;
            intro.EndHint |= StateMachine.StateHint.GroupWithNext;

            var sting1 = BuildHellStingStates(ref intro.Next, 10.2f);
            sting1.EndHint |= StateMachine.StateHint.GroupWithNext;

            var sting2 = BuildHellStingStates(ref sting1.Next, 14.2f);
            sting2.EndHint |= StateMachine.StateHint.GroupWithNext;

            var impulse1 = BuildUltimateImpulseState(ref sting2.Next, 9.1f);
            impulse1.Exit = comp.Restart;
            impulse1.EndHint |= StateMachine.StateHint.GroupWithNext;

            var sting3 = BuildHellStingStates(ref impulse1.Next, 7.2f);
            sting3.EndHint |= StateMachine.StateHint.GroupWithNext;

            var sting4 = BuildHellStingStates(ref sting3.Next, 14.2f);
            sting4.EndHint |= StateMachine.StateHint.GroupWithNext;

            var impulse2 = BuildUltimateImpulseState(ref sting4.Next, 9.1f);
            impulse2.Exit = comp.Reset;
            return impulse2;
        }
    }
}
