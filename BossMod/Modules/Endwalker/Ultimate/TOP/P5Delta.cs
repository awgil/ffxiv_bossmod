﻿using System.Collections.Generic;
using System.Linq;
using System;

namespace BossMod.Endwalker.Ultimate.TOP
{
    // note: this is all very tied to LPDU strat
    class P5Delta : BossComponent
    {
        public enum PairAssignment { None, Inner, Outer }
        public enum SideAssignment { None, North, South }

        public struct PlayerState
        {
            public int PartnerSlot;
            public bool IsLocal; // true = local smell aka green tether, false = remote smell aka blue tether
            public bool TetherBroken;
            public Actor? RocketPunch;
            public PairAssignment PairAssignment;
            public SideAssignment SideAssignment;
        }

        public int NumPunchesSpawned { get; private set; }
        public int NumTethersBroken { get; private set; }
        public bool TethersActive { get; private set; }
        public bool ExplosionsBaited { get; private set; }
        public Angle[] ArmRotations = new Angle[6]; // [0] = at rel north, then CCW
        public PlayerState[] Players = Utils.MakeArray(PartyState.MaxPartySize, new PlayerState() { PartnerSlot = -1 });
        private Actor? _nearWorld;
        private Actor? _distantWorld;
        private Actor? _monitorTarget;
        private Actor? _beyondDefenceTarget;
        private WDir _eyeDir; // relative north; beetle is rel west, final is rel east
        private WDir _monitorSafeDir;
        private WDir _swivelCannonSafeDir;
        private List<(int, int)> _localTethers = new();
        private List<(int, int)> _remoteTethers = new();

        public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        {
            var pcState = Players[pcSlot];
            var playerState = Players[playerSlot];
            return pcState.PartnerSlot == playerSlot ? PlayerPriority.Interesting
                : !ExplosionsBaited && pcState.SideAssignment != SideAssignment.None && pcState.IsLocal == playerState.IsLocal && pcState.SideAssignment == playerState.SideAssignment ? PlayerPriority.Danger
                : PlayerPriority.Normal;
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            var p = Players[pcSlot];
            var partner = p.TetherBroken ? null : module.Raid[p.PartnerSlot];
            if (partner != null)
                arena.AddLine(pc.Position, partner.Position, ArenaColor.Danger);

            foreach (var safeSpot in SafeSpotOffsets(module, pcSlot))
                arena.AddCircle(module.Bounds.Center + safeSpot, 1, ArenaColor.Safe);
        }

        public override void OnActorCreated(BossModule module, Actor actor)
        {
            if ((OID)actor.OID is OID.RocketPunch1 or OID.RocketPunch2)
            {
                var (closestSlot, closestPlayer) = module.Raid.WithSlot(true).Closest(actor.Position);
                if (closestPlayer != null)
                {
                    if (Players[closestSlot].RocketPunch != null)
                        module.ReportError(this, $"Multiple punches spawned for player #{closestSlot}");
                    Players[closestSlot].RocketPunch = actor;
                }

                if (++NumPunchesSpawned == PartyState.MaxPartySize)
                {
                    InitAssignments(module);
                }
            }
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            switch ((SID)status.ID)
            {
                case SID.HelloNearWorld:
                    _nearWorld = actor;
                    break;
                case SID.HelloDistantWorld:
                    _distantWorld = actor;
                    break;
                case SID.OversampledWaveCannonLoadingR:
                case SID.OversampledWaveCannonLoadingL:
                    _monitorTarget = actor;
                    break;
            }
        }

        public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            switch ((TetherID)tether.ID)
            {
                case TetherID.HWPrepLocalTether:
                case TetherID.HWPrepRemoteTether:
                    var s1 = module.Raid.FindSlot(source.InstanceID);
                    var s2 = module.Raid.FindSlot(tether.Target);
                    if (s1 >= 0 && s2 >= 0)
                    {
                        var isLocal = tether.ID == (uint)TetherID.HWPrepLocalTether;
                        Players[s1].PartnerSlot = s2;
                        Players[s2].PartnerSlot = s1;
                        Players[s1].IsLocal = Players[s2].IsLocal = isLocal;
                        (isLocal ? _localTethers : _remoteTethers).Add((s1, s2));
                    }
                    break;
                case TetherID.HWLocalTether:
                case TetherID.HWRemoteTether:
                    TethersActive = true;
                    break;
            }
        }

        public override void OnUntethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            if ((TetherID)tether.ID is TetherID.HWLocalTether or TetherID.HWRemoteTether)
            {
                var s1 = module.Raid.FindSlot(source.InstanceID);
                var s2 = module.Raid.FindSlot(tether.Target);
                if (s1 >= 0 && s2 >= 0)
                {
                    Players[s1].TetherBroken = Players[s2].TetherBroken = true;
                    ++NumTethersBroken;
                }
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.DeltaExplosion:
                    ExplosionsBaited = true;
                    break;
                case AID.DeltaOversampledWaveCannonR:
                    _monitorSafeDir = -(spell.Rotation - 90.Degrees()).ToDirection();
                    break;
                case AID.DeltaOversampledWaveCannonL:
                    _monitorSafeDir = -(spell.Rotation + 90.Degrees()).ToDirection();
                    break;
                case AID.SwivelCannonR:
                case AID.SwivelCannonL:
                    _swivelCannonSafeDir = -spell.Rotation.ToDirection();
                    break;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.BeyondDefenseAOE)
                _beyondDefenceTarget = module.WorldState.Actors.Find(spell.MainTargetID);
        }

        public override void OnActorPlayActionTimelineEvent(BossModule module, Actor actor, ushort id)
        {
            if (id == 0x1E43)
            {
                switch ((OID)actor.OID)
                {
                    case OID.BeetleHelper:
                        _eyeDir = (actor.Position - module.Bounds.Center).Normalized().OrthoR();
                        break;
                    case OID.FinalHelper:
                        _eyeDir = (actor.Position - module.Bounds.Center).Normalized().OrthoL();
                        break;
                }
            }
        }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if ((OID)actor.OID is OID.LeftArmUnit or OID.RightArmUnit)
            {
                var rotation = (IconID)iconID switch {
                    IconID.RotateCW => -20.Degrees(),
                    IconID.RotateCCW => 20.Degrees(),
                    _ => default
                };
                if (rotation != default)
                    ArmRotations[ArmIndex(actor.Position - module.Bounds.Center)] = rotation;
            }
        }

        public WDir ArmOffset(int index) => 20 * (Angle.FromDirection(_eyeDir) + index * 60.Degrees()).ToDirection();
        public int ArmIndex(WDir offset) => _eyeDir.Dot(offset) switch
        {
            > 19 => 0,
            < -19 => 3,
            > 0 => _eyeDir.OrthoL().Dot(offset) > 0 ? 1 : 5,
            _ => _eyeDir.OrthoL().Dot(offset) > 0 ? 2 : 4,
        };

        private void InitAssignments(BossModule module)
        {
            // 1. assign initial inner/outer
            float slotToOffsetX(int slot) => _eyeDir.OrthoR().Dot((module.Raid[slot]?.Position ?? module.Bounds.Center) - module.Bounds.Center);
            float pairToOffsetX((int s1, int s2) slots) => MathF.Abs(slotToOffsetX(slots.s1) + slotToOffsetX(slots.s2));
            var outerLocal = _localTethers.MaxBy(pairToOffsetX);
            var outerRemote = _remoteTethers.MaxBy(pairToOffsetX);
            foreach (ref var p in Players.AsSpan())
            {
                var outerSlots = p.IsLocal ? outerLocal : outerRemote;
                p.PairAssignment = p.PartnerSlot == outerSlots.Item1 || p.PartnerSlot == outerSlots.Item2 ? PairAssignment.Outer : PairAssignment.Inner;
            }
            // 2. assign initial north/south
            foreach (var (s1, s2) in _localTethers.Concat(_remoteTethers))
            {
                var p1 = module.Raid[s1];
                var p2 = module.Raid[s2];
                if (p1 != null && p2 != null)
                {
                    var p12n = _eyeDir.Dot(p1.Position - p2.Position) > 0;
                    Players[s1].SideAssignment = p12n ? SideAssignment.North : SideAssignment.South;
                    Players[s2].SideAssignment = p12n ? SideAssignment.South : SideAssignment.North;
                }
            }
            // 3. swap inner north/south if needed
            foreach (ref var p in Players.AsSpan())
            {
                if (p.PairAssignment != PairAssignment.Inner)
                    continue; // outer stay where they are

                var outerSlots = p.IsLocal ? outerLocal : outerRemote;
                var nearbyOuterSlot = Players[outerSlots.Item1].SideAssignment == p.SideAssignment ? outerSlots.Item1 : outerSlots.Item2;
                if (p.RocketPunch?.OID == Players[nearbyOuterSlot].RocketPunch?.OID)
                {
                    // swap side
                    p.SideAssignment = p.SideAssignment == SideAssignment.North ? SideAssignment.South : SideAssignment.North;
                }
            }
        }

        private IEnumerable<WDir> SafeSpotOffsets(BossModule module, int slot)
        {
            var p = Players[slot];
            if (p.PartnerSlot < 0 || _eyeDir == default)
                yield break; // no safe spots yet

            if (p.RocketPunch == null)
            {
                // no punches yet, show all 4 possible spots
                if (p.IsLocal)
                {
                    // green tethers go to final side
                    yield return TransformRelNorth(9, -11);
                    yield return TransformRelNorth(9, +11);
                    yield return TransformRelNorth(13, -11);
                    yield return TransformRelNorth(13, +11);
                }
                else
                {
                    // blue tethers go to beetle side
                    yield return TransformRelNorth(-9, -8);
                    yield return TransformRelNorth(-9, +8);
                    yield return TransformRelNorth(-13, -4);
                    yield return TransformRelNorth(-13, +4);
                }
                yield break;
            }

            if (p.SideAssignment == SideAssignment.None || p.PairAssignment == PairAssignment.None)
                yield break; // we should have correct assignments by now

            var dirZ = p.SideAssignment == SideAssignment.North ? -1 : 1; // fully symmetrical
            if (!ExplosionsBaited)
            {
                // now we should have correct assignments
                if (p.IsLocal)
                    yield return TransformRelNorth(11, 11 * dirZ);
                else
                    yield return TransformRelNorth(-13, (p.PairAssignment == PairAssignment.Inner && NumTethersBroken == 0 ? 8 : 4) * dirZ);
                yield break;
            }

            if (_beyondDefenceTarget == null)
            {
                // bait spots near arms
                if (p.IsLocal)
                {
                    if (p.PairAssignment == PairAssignment.Inner)
                        yield return BaitOffset(dirZ > 0 ? 3 : 0);
                    else
                        yield return BaitOffset(dirZ > 0 ? 4 : 5);
                }
                else
                {
                    if (p.PairAssignment == PairAssignment.Inner)
                        yield return TransformRelNorth(0, 5 * dirZ);
                    else
                        yield return BaitOffset(dirZ > 0 ? 2 : 1);
                }
                yield break;
            }

            if (_swivelCannonSafeDir == default)
            {
                if (p.IsLocal)
                {
                    // monitor soak spots
                    var dirX = p.PairAssignment == PairAssignment.Inner ? -1 : +1;
                    yield return TransformRelNorth(7 * dirX, 13 * dirZ);
                }
                else if (module.Raid[slot] != _beyondDefenceTarget)
                {
                    // central stack
                    yield return (module.Raid[slot] == _monitorTarget ? 5 : 2.5f) * _monitorSafeDir;
                }
                else
                {
                    // beyond defense target wants to run outside stack (TODO: select direction that is convenient for monitor target)
                    var stackPos = (module.Raid[slot] == _monitorTarget ? 5 : 2.5f) * _monitorSafeDir;
                    var horizOffset = TransformRelNorth(15, 0);
                    yield return stackPos + horizOffset;
                    yield return stackPos - horizOffset;
                }
                yield break;
            }

            {
                var relNorthSafe = _swivelCannonSafeDir.Dot(_eyeDir) > 0;
                var safeDirZ = relNorthSafe ? -1 : 1;
                if (p.IsLocal)
                {
                    var startingFromSafe = (p.SideAssignment == SideAssignment.North) == relNorthSafe;
                    if (p.PairAssignment == PairAssignment.Inner)
                        yield return TransformRelNorth(-10, (startingFromSafe ? 12 : 6) * safeDirZ);
                    else if (startingFromSafe)
                        yield return TransformRelNorth(15, 11 * safeDirZ);
                    else
                        yield return TransformRelNorth(-18, 2 * safeDirZ);
                }
                else if (_distantWorld == module.Raid[slot])
                {
                    yield return TransformRelNorth(0, 19 * safeDirZ);
                }
                else if (_nearWorld == module.Raid[slot])
                {
                    yield return TransformRelNorth(0, 6 * safeDirZ);
                }
                else
                {
                    yield return TransformRelNorth(9, 15 * safeDirZ);
                }
            }
        }

        // x positive is east (final side), z positive is south
        private WDir TransformRelNorth(float x, float z) => x * _eyeDir.OrthoR() - z * _eyeDir;
        private WDir BaitOffset(int index) => 19 * (Angle.FromDirection(_eyeDir) + index * 60.Degrees() - 0.15f * ArmRotations[index]).ToDirection(); // 5 degrees offset in correct direction
    }

    class P5DeltaOpticalLaser : Components.GenericAOEs
    {
        public Actor? Source { get; private set; }
        private DateTime _activation;

        private static AOEShapeRect _shape = new(100, 8);

        public P5DeltaOpticalLaser() : base(ActionID.MakeSpell(AID.OpticalLaser)) { }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (Source != null)
                yield return new(_shape, Source.Position, Source.Rotation, _activation);
        }

        // at this point eye is in correct position
        public override void OnActorPlayActionTimelineEvent(BossModule module, Actor actor, ushort id)
        {
            if ((OID)actor.OID is OID.BeetleHelper or OID.FinalHelper && id == 0x1E43)
            {
                Source ??= module.Enemies(OID.OpticalUnit).FirstOrDefault();
                _activation = module.WorldState.CurrentTime.AddSeconds(20);
            }
        }
    }

    class P5DeltaExplosion : Components.LocationTargetedAOEs
    {
        private P5Delta? _delta;

        public P5DeltaExplosion() : base(ActionID.MakeSpell(AID.DeltaExplosion), 3) { }

        public override void Init(BossModule module) => _delta = module.FindComponent<P5Delta>();

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_delta == null || Casters.Count > 0)
                return;
            var ps = _delta.Players[pcSlot];
            var partner = module.Raid.WithSlot(true).WhereSlot(i => _delta.Players[i].IsLocal == ps.IsLocal && i != ps.PartnerSlot && _delta.Players[i].RocketPunch?.OID != ps.RocketPunch?.OID).FirstOrDefault().Item2;
            if (partner != null)
                arena.AddCircle(partner.Position, Shape.Radius, ArenaColor.Safe);
        }
    }

    class P5DeltaHyperPulse : Components.GenericAOEs
    {
        private P5Delta? _delta;
        private List<AOEInstance> _aoes = new();

        private static AOEShapeRect _shape = new(100, 4);
        private static int _numRepeats = 6;

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_aoes.Count > 0)
            {
                foreach (var aoe in _aoes)
                {
                    yield return aoe;
                }
            }
            else if (_delta != null)
            {
                for (int i = 0; i < _delta.ArmRotations.Length; ++i)
                {
                    var pos = module.Bounds.Center + _delta.ArmOffset(i);
                    if (module.Raid.WithoutSlot().Closest(pos) == actor)
                    {
                        var angle = Angle.FromDirection(actor.Position - pos);
                        for (int j = 0; j < _numRepeats; ++j)
                        {
                            yield return new(_shape, pos, angle + j * _delta.ArmRotations[i], risky: false);
                        }
                    }
                }
            }
        }

        public override void Init(BossModule module) => _delta = module.FindComponent<P5Delta>();

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.DeltaHyperPulseFirst && _delta != null)
            {
                var rot = _delta.ArmRotations[_delta.ArmIndex(caster.Position - module.Bounds.Center)];
                for (int i = 0; i < _numRepeats; ++i)
                {
                    _aoes.Add(new(_shape, caster.Position, (spell.Rotation + i * rot).Normalized(), spell.NPCFinishAt.AddSeconds(i * 0.6)));
                }
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.DeltaHyperPulseFirst or AID.DeltaHyperPulseRest)
            {
                ++NumCasts;
                var count = _aoes.RemoveAll(aoe => aoe.Origin.AlmostEqual(caster.Position, 1) && aoe.Rotation.AlmostEqual(caster.Rotation, 0.1f));
                if (count != 1)
                    module.ReportError(this, $"Single cast removed {count} aoes");
            }
        }
    }

    class P5DeltaOversampledWaveCannon : Components.UniformStackSpread
    {
        private P5Delta? _delta;
        private Actor? _boss;
        private Angle _bossAngle;
        private BitMask _bossIntendedTargets;
        private Actor? _player;
        private Angle _playerAngle;
        private BitMask _playerIntendedTargets;

        private static AOEShapeRect _shape = new(50, 50);

        public P5DeltaOversampledWaveCannon() : base(0, 7) { }

        public override void Init(BossModule module) => _delta = module.FindComponent<P5Delta>();

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            base.AddHints(module, slot, actor, hints, movementHints);

            if (_player == actor)
            {
                // ensure we hit only two intended targets
                hints.Add("Aim monitor!", module.Raid.WithSlot().Exclude(actor).Where(ip => _shape.Check(ip.Item2.Position, actor.Position, actor.Rotation + _playerAngle) != _playerIntendedTargets[ip.Item1]).Any());
            }
            else if (_player != null)
            {
                var hit = _shape.Check(actor.Position, _player.Position, _player.Rotation + _playerAngle);
                if (hit != _playerIntendedTargets[slot])
                    hints.Add(hit ? "GTFO from player monitor!" : "Soak player monitor!");
            }

            if (_boss != null)
            {
                var hit = _shape.Check(actor.Position, _boss.Position, _boss.Rotation + _bossAngle);
                if (hit != _bossIntendedTargets[slot])
                    hints.Add(hit ? "GTFO from boss monitor!" : "Soak boss monitor!");
            }
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_boss != null)
                _shape.Draw(arena, _boss.Position, _boss.Rotation + _bossAngle, _bossIntendedTargets[pcSlot] ? ArenaColor.SafeFromAOE : ArenaColor.AOE);
            if (_player != null)
                _shape.Draw(arena, _player.Position, _player.Rotation + _playerAngle, _playerIntendedTargets[pcSlot] ? ArenaColor.SafeFromAOE : ArenaColor.AOE);
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            var angle = (SID)status.ID switch
            {
                SID.OversampledWaveCannonLoadingL => 90.Degrees(),
                SID.OversampledWaveCannonLoadingR => -90.Degrees(),
                _ => default
            };
            if (angle != default && module.Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            {
                _player = actor;
                _playerAngle = angle;
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            var angle = (AID)spell.Action.ID switch
            {
                AID.DeltaOversampledWaveCannonL => 90.Degrees(),
                AID.DeltaOversampledWaveCannonR => -90.Degrees(),
                _ => default
            };
            if (angle == default)
                return;
            _boss = caster;
            _bossAngle = angle;
            if (_delta == null)
                return;
            var bossSide = angle.Rad > 0 ? P5Delta.SideAssignment.South : P5Delta.SideAssignment.North;
            foreach (var (i, p) in module.Raid.WithSlot(true))
            {
                var ps = _delta.Players[i];
                if (ps.IsLocal)
                {
                    AddSpread(p, spell.NPCFinishAt); // assume only intended targets will be hit, otherwise chances are it will be all random
                    if (ps.SideAssignment == bossSide)
                        _bossIntendedTargets.Set(i);
                    else
                        _playerIntendedTargets.Set(i);
                }
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.OversampledWaveCannonAOE)
                Spreads.Clear();
        }
    }

    class P5DeltaSwivelCannon : Components.GenericAOEs
    {
        public AOEInstance? AOE;

        private static AOEShapeCone _shape = new(60, 105.Degrees());

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (AOE != null)
                yield return AOE.Value;
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.SwivelCannonR or AID.SwivelCannonL)
                AOE = new(_shape, caster.Position, spell.Rotation, spell.NPCFinishAt);
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.SwivelCannonR or AID.SwivelCannonL)
                AOE = null;
        }
    }
}
