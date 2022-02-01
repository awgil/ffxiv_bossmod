using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace BossMod
{
    public class P2S : BossModule
    {
        public enum OID : uint
        {
            Boss = 0x359B,
            CataractHead = 0x359C,
            DissociatedHead = 0x386A,
            Helper = 0x233C, // x24
        };

        public enum AID : uint
        {
            SewageDeluge = 26640, // Boss->Boss
            SpokenCataractSecondary = 26641, // CHead->CHead, half behind is safe
            WingedCataractSecondaryDissoc = 26644, // CHead->CHead, half in front is safe
            WingedCataractSecondary = 26645, // CHead->CHead, half in front is safe
            SpokenCataract = 26647, // Boss->Boss
            WingedCataract = 26648, // Boss->Boss
            CoherenceAOE = 26640, // Boss->n/a, no cast, aoe around tether target
            Coherence = 26651, // Boss->Boss
            CoherenceRay = 26652, //Boss->Boss, no cast, ray on closest target
            ChannelingFlow = 26654, // Boss->Boss
            Crash = 26657, // Helper->Helper, attack after arrows resolve
            KampeosHarma = 26659, // Boss->Boss
            KampeosHarmaChargeBoss = 26660, // Boss->target, no cast
            KampeosHarmaChargeHead = 26661, // CHead->target, no cast, first 3 charges
            KampeosHarmaChargeLast = 26662, // CHead->n/a, no cast, 4th charge
            PredatoryAvarice = 26663, // Boss->Boss
            OminousBubbling = 26666, // Boss->Boss
            OminousBubblingAOE = 26667, // Helper->targets
            Dissociation = 26668, // Boss->Boss
            DissociationAOE = 26670, // DHead->DHead
            Shockwave = 26671, // Boss->impact location
            SewageEruption = 26672, // Boss->Boss
            SewageEruptionAOE = 26673, // Helper->Helper
            DoubledImpact = 26674, // Boss->MT
            MurkyDepths = 26675, // Boss->Boss
            Enrage = 26676, // Boss->Boss
            TaintedFlood = 26679, // Boss->Boss
            TaintedFloodAOE = 26680, // Helper->targets
            ChannelingOverflow = 28098, // Boss->Boss (both 2nd and 3rd arrows)
        };

        public enum SID : uint
        {
            MarkOfTides = 2768, // avarice - gtfo (tank+dd)
            MarkOfDepths = 2769, // avarice - stack (healer)
            MarkFlowN = 2770, // 'fore', points north
            MarkFlowS = 2771, // 'rear', points south
            MarkFlowW = 2772, // 'left', points west
            MarkFlowE = 2773, // 'right', points east
        }

        public enum TetherID : uint
        {
            Coherence = 84,
        }

        // state related to sewage deluge mechanic
        private class SewageDeluge : Component
        {
            public enum Corner { None, NW, NE, SW, SE };

            private P2S _module;
            private Corner _blockedCorner = Corner.None;

            private static float _offsetCorner = 9.5f; // not sure
            private static float _cornerHalfSize = 4; // not sure
            private static float _connectHalfWidth = 2; // not sure
            private static float _cornerInner = _offsetCorner - _cornerHalfSize;
            private static float _cornerOuter = _offsetCorner + _cornerHalfSize;
            private static float _connectInner = _offsetCorner - _connectHalfWidth;
            private static float _connectOuter = _offsetCorner + _connectHalfWidth;

            private static Vector3[] _corners = { new(), new(-1, 0, -1), new(1, 0, -1), new(-1, 0, 1), new Vector3(1, 0, 1) };

            public SewageDeluge(P2S module)
            {
                _module = module;
            }

            public override void Reset() => _blockedCorner = Corner.None;

            public override void DrawArenaBackground(MiniArena arena)
            {
                if (_blockedCorner == Corner.None)
                    return;

                // central area + H additionals
                arena.ZoneQuad(arena.WorldCenter,  Vector3.UnitX, _connectInner, _connectInner, _cornerInner, arena.ColorAOE);
                // central V additionals
                arena.ZoneQuad(arena.WorldCenter,  Vector3.UnitZ, _connectInner, -_cornerInner, _cornerInner, arena.ColorAOE);
                arena.ZoneQuad(arena.WorldCenter, -Vector3.UnitZ, _connectInner, -_cornerInner, _cornerInner, arena.ColorAOE);
                // outer additionals
                arena.ZoneQuad(arena.WorldCenter,  Vector3.UnitX, _cornerOuter, -_connectOuter, _cornerInner, arena.ColorAOE);
                arena.ZoneQuad(arena.WorldCenter, -Vector3.UnitX, _cornerOuter, -_connectOuter, _cornerInner, arena.ColorAOE);
                arena.ZoneQuad(arena.WorldCenter,  Vector3.UnitZ, _cornerOuter, -_connectOuter, _cornerInner, arena.ColorAOE);
                arena.ZoneQuad(arena.WorldCenter, -Vector3.UnitZ, _cornerOuter, -_connectOuter, _cornerInner, arena.ColorAOE);
                // outer area
                arena.ZoneQuad(arena.WorldCenter,  Vector3.UnitX, arena.WorldHalfSize, -_cornerOuter, arena.WorldHalfSize, arena.ColorAOE);
                arena.ZoneQuad(arena.WorldCenter, -Vector3.UnitX, arena.WorldHalfSize, -_cornerOuter, arena.WorldHalfSize, arena.ColorAOE);
                arena.ZoneQuad(arena.WorldCenter,  Vector3.UnitZ, arena.WorldHalfSize, -_cornerOuter, _cornerOuter, arena.ColorAOE);
                arena.ZoneQuad(arena.WorldCenter, -Vector3.UnitZ, arena.WorldHalfSize, -_cornerOuter, _cornerOuter, arena.ColorAOE);

                var corner = arena.WorldCenter + _corners[(int)_blockedCorner] * _offsetCorner;
                arena.ZoneQuad(corner, Vector3.UnitX, _cornerHalfSize, _cornerHalfSize, _cornerHalfSize, arena.ColorAOE);
            }

            public override void DrawArenaForeground(MiniArena arena)
            {
                // inner border
                arena.PathLineTo(arena.WorldCenter + new Vector3(-_cornerInner,  0, -_cornerInner));
                arena.PathLineTo(arena.WorldCenter + new Vector3(-_cornerInner,  0, -_connectInner));
                arena.PathLineTo(arena.WorldCenter + new Vector3(+_cornerInner,  0, -_connectInner));
                arena.PathLineTo(arena.WorldCenter + new Vector3(+_cornerInner,  0, -_cornerInner));
                arena.PathLineTo(arena.WorldCenter + new Vector3(+_connectInner, 0, -_cornerInner));
                arena.PathLineTo(arena.WorldCenter + new Vector3(+_connectInner, 0, +_cornerInner));
                arena.PathLineTo(arena.WorldCenter + new Vector3(+_cornerInner,  0, +_cornerInner));
                arena.PathLineTo(arena.WorldCenter + new Vector3(+_cornerInner,  0, +_connectInner));
                arena.PathLineTo(arena.WorldCenter + new Vector3(-_cornerInner,  0, +_connectInner));
                arena.PathLineTo(arena.WorldCenter + new Vector3(-_cornerInner,  0, +_cornerInner));
                arena.PathLineTo(arena.WorldCenter + new Vector3(-_connectInner, 0, +_cornerInner));
                arena.PathLineTo(arena.WorldCenter + new Vector3(-_connectInner, 0, -_cornerInner));
                arena.PathStroke(true, arena.ColorBorder);

                // outer border
                arena.PathLineTo(arena.WorldCenter + new Vector3(-_cornerOuter,  0, -_cornerOuter));
                arena.PathLineTo(arena.WorldCenter + new Vector3(-_cornerInner,  0, -_cornerOuter));
                arena.PathLineTo(arena.WorldCenter + new Vector3(-_cornerInner,  0, -_connectOuter));
                arena.PathLineTo(arena.WorldCenter + new Vector3(+_cornerInner,  0, -_connectOuter));
                arena.PathLineTo(arena.WorldCenter + new Vector3(+_cornerInner,  0, -_cornerOuter));
                arena.PathLineTo(arena.WorldCenter + new Vector3(+_cornerOuter,  0, -_cornerOuter));
                arena.PathLineTo(arena.WorldCenter + new Vector3(+_cornerOuter,  0, -_cornerInner));
                arena.PathLineTo(arena.WorldCenter + new Vector3(+_connectOuter, 0, -_cornerInner));
                arena.PathLineTo(arena.WorldCenter + new Vector3(+_connectOuter, 0, +_cornerInner));
                arena.PathLineTo(arena.WorldCenter + new Vector3(+_cornerOuter,  0, +_cornerInner));
                arena.PathLineTo(arena.WorldCenter + new Vector3(+_cornerOuter,  0, +_cornerOuter));
                arena.PathLineTo(arena.WorldCenter + new Vector3(+_cornerInner,  0, +_cornerOuter));
                arena.PathLineTo(arena.WorldCenter + new Vector3(+_cornerInner,  0, +_connectOuter));
                arena.PathLineTo(arena.WorldCenter + new Vector3(-_cornerInner,  0, +_connectOuter));
                arena.PathLineTo(arena.WorldCenter + new Vector3(-_cornerInner,  0, +_cornerOuter));
                arena.PathLineTo(arena.WorldCenter + new Vector3(-_cornerOuter,  0, +_cornerOuter));
                arena.PathLineTo(arena.WorldCenter + new Vector3(-_cornerOuter,  0, +_cornerInner));
                arena.PathLineTo(arena.WorldCenter + new Vector3(-_connectOuter, 0, +_cornerInner));
                arena.PathLineTo(arena.WorldCenter + new Vector3(-_connectOuter, 0, -_cornerInner));
                arena.PathLineTo(arena.WorldCenter + new Vector3(-_cornerOuter,  0, -_cornerInner));
                arena.PathStroke(true, arena.ColorBorder);
            }

            public override void OnEventEnvControl(uint featureID, byte index, uint state)
            {
                // 800375A2: we typically get two events for index=0 (global) and index=N (corner)
                // state 00200010 - "prepare" (show aoe that is still harmless)
                // state 00020001 - "active" (dot in center/borders, oneshot in corner)
                // state 00080004 - "finish" (reset)
                if (featureID == 0x800375A2 && index > 0 && index < 5)
                {
                    if (state == 0x00200010)
                        _blockedCorner = (Corner)index;
                    else if (state == 0x00080004)
                        _blockedCorner = Corner.None;
                }
            }
        }

        // state related to cataract mechanic
        private class Cataract : Component
        {
            public enum State { None, Winged, Spoken }
            public State CurState = State.None;

            private P2S _module;

            private static float _halfWidth = 7.5f;

            public Cataract(P2S module)
            {
                _module = module;
            }

            public override void Reset() => CurState = State.None;

            public override void AddHints(int slot, WorldState.Actor actor, TextHints hints, MovementHints? movementHints)
            {
                if (CurState == State.None)
                    return;

                var boss = _module.Boss();
                var head = _module.CataractHead();
                float headRot = CurState == State.Winged ? MathF.PI : 0;
                if ((boss != null && GeometryUtils.PointInRect(actor.Position - boss.Position, boss.Rotation, _module.Arena.WorldHalfSize, _module.Arena.WorldHalfSize, _halfWidth)) ||
                    (head != null && GeometryUtils.PointInRect(actor.Position - head.Position, head.Rotation + headRot, _module.Arena.WorldHalfSize, 0, _module.Arena.WorldHalfSize)))
                {
                    hints.Add("GTFO from cataract!");
                }
            }

            public override void DrawArenaBackground(MiniArena arena)
            {
                if (CurState == State.None)
                    return;

                var boss = _module.Boss();
                if (boss != null)
                {
                    arena.ZoneQuad(boss.Position, boss.Rotation, arena.WorldHalfSize, arena.WorldHalfSize, _halfWidth, arena.ColorAOE);
                }

                var head = _module.CataractHead();
                if (head != null)
                {
                    float headRot = CurState == State.Winged ? MathF.PI : 0;
                    arena.ZoneQuad(head.Position, head.Rotation + headRot, arena.WorldHalfSize, 0, arena.WorldHalfSize, arena.ColorAOE);
                }
            }
        }

        // state related to dissociation mechanic
        private class Dissociation : Component
        {
            public bool Active = false; // note that it resets automatically on dissociation cast

            private P2S _module;

            private static float _halfWidth = 10;

            public Dissociation(P2S module)
            {
                _module = module;
            }

            public override void Reset() => Active = false;

            public override void AddHints(int slot, WorldState.Actor actor, TextHints hints, MovementHints? movementHints)
            {
                var head = _module.DissociatedHead();
                if (!Active || head == null || _module.Arena.InBounds(head.Position))
                    return; // inactive or head not teleported yet

                if (GeometryUtils.PointInRect(actor.Position - head.Position, head.Rotation, 50, 0, _halfWidth))
                {
                    hints.Add("GTFO from dissociation!");
                }
            }

            public override void DrawArenaBackground(MiniArena arena)
            {
                var head = _module.DissociatedHead();
                if (!Active || head == null || _module.Arena.InBounds(head.Position))
                    return; // inactive or head not teleported yet

                arena.ZoneQuad(head.Position, head.Rotation, 50, 0, _halfWidth, arena.ColorAOE);
            }

            public override void OnCastFinished(WorldState.Actor actor)
            {
                if (actor == _module.DissociatedHead() && actor.CastInfo!.IsSpell(AID.DissociationAOE))
                    Active = false;
            }
        }

        // state related to coherence mechanic
        // TODO: i'm not 100% sure how exactly it selects target for aoe ray, and who should that be...
        private class Coherence : Component
        {
            public bool Active; // note that it resets automatically on ray cast

            private P2S _module;
            private WorldState.Actor? _closest;
            private ulong _inRay = 0;

            private static float _aoeRadius = 10; // not sure about this - actual range is 60, but it has some sort of falloff
            private static float _rayHalfWidth = 3;

            public Coherence(P2S module)
            {
                _module = module;
            }

            public override void Reset() => Active = false;

            public override void Update()
            {
                _closest = null;
                _inRay = 0;
                var boss = _module.Boss();
                if (!Active || boss == null)
                    return;

                float minDistSq = 100000;
                foreach ((int i, var player) in _module.IterateRaidMembers())
                {
                    if (boss.Tether.Target == player.InstanceID)
                        continue; // assume both won't target same player for tethers and ray...

                    float dist = (player.Position - boss.Position).LengthSquared();
                    if (dist < minDistSq)
                    {
                        minDistSq = dist;
                        _closest = player;
                    }
                }

                if (_closest == null)
                    return;

                var dirToClosest = Vector3.Normalize(_closest.Position - boss.Position);
                foreach ((int i, var player) in _module.IterateRaidMembers())
                {
                    if (boss.Tether.Target == player.InstanceID)
                        continue; // assume both won't target same player for tethers and ray...
                    if (player == _closest || GeometryUtils.PointInRect(player.Position - boss.Position, dirToClosest, 50, 0, _rayHalfWidth))
                        BitVector.SetVector64Bit(ref _inRay, i);
                }
            }

            public override void AddHints(int slot, WorldState.Actor actor, TextHints hints, MovementHints? movementHints)
            {
                if (!Active)
                    return;

                var boss = _module.Boss();
                if (boss?.Tether.Target == actor.InstanceID)
                {
                    if (actor.Role != WorldState.ActorRole.Tank)
                    {
                        hints.Add("Pass tether to tank!");
                    }
                    else if (_module.IterateRaidMembersInRange(slot, _aoeRadius).Any())
                    {
                        hints.Add("GTFO from raid!");
                    }
                }
                else if (actor == _closest)
                {
                    if (actor.Role != WorldState.ActorRole.Tank)
                    {
                        hints.Add("Go behind tank!");
                    }
                    else if (BitOperations.PopCount(_inRay) < 7)
                    {
                        hints.Add("Make sure ray hits everyone!");
                    }
                }
                else
                {
                    if (actor.Role == WorldState.ActorRole.Tank)
                    {
                        hints.Add("Go in front of raid!");
                    }
                    else if (!BitVector.IsVector64BitSet(_inRay, slot))
                    {
                        hints.Add("Go behind tank!");
                    }
                }
            }

            public override void DrawArenaBackground(MiniArena arena)
            {
                var boss = _module.Boss();
                if (!Active || boss == null || _closest == null || boss.Position == _closest.Position)
                    return;

                var dir = Vector3.Normalize(_closest.Position - boss.Position);
                arena.ZoneQuad(boss.Position, dir, 50, 0, _rayHalfWidth, arena.ColorAOE);
            }

            public override void DrawArenaForeground(MiniArena arena)
            {
                var boss = _module.Boss();
                if (!Active || boss == null || _closest == null || boss.Position == _closest.Position)
                    return;

                // TODO: i'm not sure what are the exact mechanics - flare is probably distance-based, and ray is probably shared damage cast at closest target?..
                foreach ((int i, var player) in _module.IterateRaidMembers())
                {
                    if (boss.Tether.Target == player.InstanceID)
                    {
                        arena.AddLine(player.Position, boss.Position, arena.ColorDanger);
                        arena.Actor(player, arena.ColorDanger);
                        arena.AddCircle(player.Position, _aoeRadius, arena.ColorDanger);
                    }
                    else if (player == _closest)
                    {
                        arena.Actor(player, arena.ColorDanger);
                    }
                    else
                    {
                        arena.Actor(player, BitVector.IsVector64BitSet(_inRay, i) ? arena.ColorPlayerInteresting : arena.ColorPlayerGeneric);
                    }
                }
            }

            public override void OnEventCast(WorldState.CastResult info)
            {
                if (info.IsSpell(AID.CoherenceRay))
                    Active = false;
            }
        }

        // state related to predatory avarice mechanic
        private class PredatoryAvarice : Component
        {
            private P2S _module;
            private ulong _playersWithTides = 0;
            private ulong _playersWithDepths = 0;
            private ulong _playersInTides = 0;
            private ulong _playersInDepths = 0;

            private static float _tidesRadius = 10;
            private static float _depthsRadius = 6;

            public bool Active => (_playersWithTides | _playersWithDepths) != 0;

            public PredatoryAvarice(P2S module)
            {
                _module = module;
            }

            public override void Reset() => _playersWithTides = _playersWithDepths = 0;

            public override void Update()
            {
                _playersInTides = _playersInDepths = 0;
                if (!Active)
                    return;

                foreach ((int i, var actor) in _module.IterateRaidMembers())
                {
                    if (BitVector.IsVector64BitSet(_playersWithTides, i))
                    {
                        _playersInTides |= _module.FindRaidMembersInRange(i, _tidesRadius);
                    }
                    else if (BitVector.IsVector64BitSet(_playersWithDepths, i))
                    {
                        _playersInDepths |= _module.FindRaidMembersInRange(i, _depthsRadius);
                    }
                }
            }

            public override void AddHints(int slot, WorldState.Actor actor, TextHints hints, MovementHints? movementHints)
            {
                if (!Active)
                    return;

                if (BitVector.IsVector64BitSet(_playersWithTides, slot))
                {
                    if (_module.IterateRaidMembersInRange(slot, _tidesRadius).Any())
                    {
                        hints.Add("GTFO from raid!");
                    }
                }
                else
                {
                    if (BitVector.IsVector64BitSet(_playersInTides, slot))
                    {
                        hints.Add("GTFO from avarice!");
                    }

                    bool warnToStack = BitVector.IsVector64BitSet(_playersWithDepths, slot)
                        ? BitOperations.PopCount(_playersInDepths) < 6
                        : !BitVector.IsVector64BitSet(_playersInDepths, slot);
                    if (warnToStack)
                    {
                        hints.Add("Stack with raid!");
                    }
                }
            }

            public override void DrawArenaForeground(MiniArena arena)
            {
                var pc = _module.Player();
                if (!Active || pc == null)
                    return;

                bool pcHasTides = BitVector.IsVector64BitSet(_playersWithTides, _module.PlayerSlot);
                bool pcHasDepths = BitVector.IsVector64BitSet(_playersWithDepths, _module.PlayerSlot);
                foreach ((int i, var actor) in _module.IterateRaidMembers())
                {
                    if (BitVector.IsVector64BitSet(_playersWithTides, i))
                    {
                        // tides are always drawn
                        arena.AddCircle(actor.Position, _tidesRadius, arena.ColorDanger);
                        arena.Actor(actor, arena.ColorDanger);
                    }
                    else if (BitVector.IsVector64BitSet(_playersWithDepths, i) && !pcHasTides)
                    {
                        // depths are drawn only if pc has no tides - otherwise it is to be considered a generic player
                        arena.AddCircle(actor.Position, _tidesRadius, arena.ColorSafe);
                        arena.Actor(actor, arena.ColorDanger);
                    }
                    else if (pcHasTides || pcHasDepths)
                    {
                        // other players are only drawn if pc has some debuff
                        bool playerInteresting = BitVector.IsVector64BitSet(pcHasTides ? _playersInTides : _playersInDepths, i);
                        arena.Actor(actor.Position, actor.Rotation, playerInteresting ? arena.ColorPlayerInteresting : arena.ColorPlayerGeneric);
                    }
                }
            }

            public override void OnStatusGain(WorldState.Actor actor, int index)
            {
                switch ((SID)actor.Statuses[index].ID)
                {
                    case SID.MarkOfTides:
                        ModifyDebuff(_module.FindRaidMemberSlot(actor.InstanceID), ref _playersWithTides, true);
                        break;
                    case SID.MarkOfDepths:
                        ModifyDebuff(_module.FindRaidMemberSlot(actor.InstanceID), ref _playersWithDepths, true);
                        break;
                }
            }

            public override void OnStatusLose(WorldState.Actor actor, int index)
            {
                switch ((SID)actor.Statuses[index].ID)
                {
                    case SID.MarkOfTides:
                        ModifyDebuff(_module.FindRaidMemberSlot(actor.InstanceID), ref _playersWithTides, false);
                        break;
                    case SID.MarkOfDepths:
                        ModifyDebuff(_module.FindRaidMemberSlot(actor.InstanceID), ref _playersWithDepths, false);
                        break;
                }
            }

            private void ModifyDebuff(int slot, ref ulong vector, bool active)
            {
                if (slot >= 0)
                    BitVector.ModifyVector64Bit(ref vector, slot, active);
            }
        }

        // state related to kampeos harma mechanic
        // note that it relies on waymarks to determine safe spots...
        private class KampeosHarma : Component
        {
            private P2S _module;
            private int[] _playerOrder = new int[8]; // 0 if unknown, then sq1 sq2 sq3 sq4 tri1 tri2 tri3 tri4
            private Vector3 _startingOffset = new();
            private int _numCharges = 0;

            public KampeosHarma(P2S module)
            {
                _module = module;
            }

            public void Start(Vector3 bossPosition)
            {
                _startingOffset = bossPosition - _module.Arena.WorldCenter;
            }

            public override void Reset()
            {
                Array.Fill(_playerOrder, 0);
                _numCharges = 0;
            }

            public override void AddHints(int slot, WorldState.Actor actor, TextHints hints, MovementHints? movementHints)
            {
                var safePos = GetSafeZone(slot);
                if (safePos != null && !GeometryUtils.PointInCircle(actor.Position - safePos.Value, 1))
                    hints.Add("Go to safe zone!");
            }

            public override void DrawArenaForeground(MiniArena arena)
            {
                var pos = GetSafeZone(_module.PlayerSlot);
                if (pos != null)
                    arena.AddCircle(pos.Value, 1, arena.ColorSafe);
            }

            public override void OnEventIcon(uint actorID, uint iconID)
            {
                if (iconID >= 145 && iconID <= 152)
                {
                    int slot = _module.FindRaidMemberSlot(actorID);
                    if (slot >= 0)
                        _playerOrder[slot] = (int)(iconID - 144);
                }
            }

            public override void OnEventCast(WorldState.CastResult info)
            {
                if (info.IsSpell(AID.KampeosHarmaChargeBoss))
                    ++_numCharges;
            }

            private Vector3? GetSafeZone(int slot)
            {
                switch (slot >= 0 ? _playerOrder[slot] : 0)
                {
                    case 1: // sq 1 - opposite corner, hide after first charge
                        return _module.Arena.WorldCenter + (_numCharges < 1 ? -1.1f : -1.2f) * _startingOffset;
                    case 2: // sq 2 - same corner, hide after second charge
                        return _module.Arena.WorldCenter + (_numCharges < 2 ? +1.1f : +1.2f) * _startingOffset;
                    case 3: // sq 3 - opposite corner, hide before first charge
                        return _module.Arena.WorldCenter + (_numCharges < 1 ? -1.2f : -1.1f) * _startingOffset;
                    case 4: // sq 4 - same corner, hide before second charge
                        return _module.Arena.WorldCenter + (_numCharges < 2 ? +1.2f : +1.1f) * _startingOffset;
                    case 5: // tri 1 - waymark 1
                        return _module.WorldState.GetWaymark(WorldState.Waymark.N1);
                    case 6: // tri 2 - waymark 2
                        return _module.WorldState.GetWaymark(WorldState.Waymark.N2);
                    case 7: // tri 3 - waymark 3
                        return _module.WorldState.GetWaymark(WorldState.Waymark.N3);
                    case 8: // tri 4 - waymark 4
                        return _module.WorldState.GetWaymark(WorldState.Waymark.N4);
                }
                return null;
            }
        }

        // state related to channeling [over]flow mechanics
        // TODO: improve (hint if too close to or missing a partner at very least...)
        private class ChannelingFlow : Component
        {
            public bool Active = false;

            private P2S _module;

            private static float _typhoonHalfWidth = 2.5f;

            public ChannelingFlow(P2S module)
            {
                _module = module;
            }

            public override void Reset() => Active = false;

            public override void DrawArenaBackground(MiniArena arena)
            {
                if (!Active)
                    return;

                foreach ((int i, var player) in _module.IterateRaidMembers())
                {
                    foreach (var status in player.Statuses)
                    {
                        if (status.RemainingTime > 10)
                            continue;

                        switch ((SID)status.ID)
                        {
                            case SID.MarkFlowN:
                                arena.ZoneQuad(player.Position, -Vector3.UnitZ, 50, 0, _typhoonHalfWidth, arena.ColorAOE);
                                break;
                            case SID.MarkFlowS:
                                arena.ZoneQuad(player.Position,  Vector3.UnitZ, 50, 0, _typhoonHalfWidth, arena.ColorAOE);
                                break;
                            case SID.MarkFlowW:
                                arena.ZoneQuad(player.Position, -Vector3.UnitX, 50, 0, _typhoonHalfWidth, arena.ColorAOE);
                                break;
                            case SID.MarkFlowE:
                                arena.ZoneQuad(player.Position,  Vector3.UnitX, 50, 0, _typhoonHalfWidth, arena.ColorAOE);
                                break;
                        }
                    }
                }
            }
        }

        private List<WorldState.Actor> _boss;
        private List<WorldState.Actor> _cataractHead;
        private List<WorldState.Actor> _dissociatedHead;
        private WorldState.Actor? Boss() => _boss.FirstOrDefault();
        private WorldState.Actor? CataractHead() => _cataractHead.FirstOrDefault();
        private WorldState.Actor? DissociatedHead() => _dissociatedHead.FirstOrDefault();

        public P2S(WorldState ws)
            : base(ws, 8)
        {
            _boss = RegisterEnemies(OID.Boss, true);
            _cataractHead = RegisterEnemies(OID.CataractHead, true);
            _dissociatedHead = RegisterEnemies(OID.DissociatedHead, true);

            RegisterComponent(new SewageDeluge(this));
            RegisterComponent(new Cataract(this));
            RegisterComponent(new Dissociation(this));
            RegisterComponent(new Coherence(this));
            RegisterComponent(new PredatoryAvarice(this));
            RegisterComponent(new KampeosHarma(this));
            RegisterComponent(new ChannelingFlow(this));

            StateMachine.State? s;
            s = BuildMurkyDepthsState(ref InitialState, 10);
            s = BuildDoubledImpactState(ref s.Next, 5.2f);
            s = BuildSewageDelugeState(ref s.Next, 7.8f);
            s = BuildCataractState(ref s.Next, 14.6f);
            s = BuildCoherenceState(ref s.Next, 8.1f);
            s = BuildMurkyDepthsState(ref s.Next, 8.2f);
            s = BuildOminousBubblingState(ref s.Next, 3.7f);

            // avarice + cataract
            s = BuildPredatoryAvariceCastState(ref s.Next, 10.9f);
            s = BuildCataractState(ref s.Next, 9.7f, true);
            s = BuildPredatoryAvariceResolveState(ref s.Next, 6.2f);
            // note: deluge 1 ends here...

            // flow 1
            s = CommonStates.Cast(ref s.Next, Boss, AID.ChannelingFlow, 8.5f, 5, "Flow 1");
            s.Exit = () => FindComponent<ChannelingFlow>()!.Active = true;
            s.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;
            s = CommonStates.Timeout(ref s.Next, 14);
            s.EndHint |= StateMachine.StateHint.PositioningEnd | StateMachine.StateHint.DowntimeStart;
            s = CommonStates.Timeout(ref s.Next, 3, "Flow resolve");
            s.Exit = () => FindComponent<ChannelingFlow>()!.Active = false;
            s.EndHint |= StateMachine.StateHint.DowntimeEnd | StateMachine.StateHint.Raidwide;

            s = BuildDoubledImpactState(ref s.Next, 8.2f);
            s = BuildMurkyDepthsState(ref s.Next, 5.2f);
            s = BuildSewageDelugeState(ref s.Next, 11.6f);
            s = BuildShockwaveState(ref s.Next, 9.6f);
            s = BuildKampeosHarmaState(ref s.Next, 4.5f);
            s = BuildDoubledImpactState(ref s.Next, 9.9f);
            s = BuildMurkyDepthsState(ref s.Next, 4.2f);

            // flow 2 (same statuses, different durations)
            s = CommonStates.Cast(ref s.Next, Boss, AID.ChannelingOverflow, 8.6f, 5, "Flow 2");
            s.Exit = () => FindComponent<ChannelingFlow>()!.Active = true;
            s.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;
            s = CommonStates.Cast(ref s.Next, Boss, AID.TaintedFlood, 4.2f, 3);
            s = CommonStates.Timeout(ref s.Next, 9, "Hit 1");
            s.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.Raidwide;
            s = CommonStates.Cast(ref s.Next, Boss, AID.TaintedFlood, 3.4f, 3);
            s = CommonStates.Timeout(ref s.Next, 9, "Hit 2");
            s.Exit = () => FindComponent<ChannelingFlow>()!.Active = false;
            s.EndHint |= StateMachine.StateHint.Raidwide | StateMachine.StateHint.PositioningEnd;

            s = BuildCataractState(ref s.Next, 1.2f);
            // note: deluge 2 ends here...

            // avarice + dissociation + cataract (note that we don't create separate avarice/dissociation resolve states here, since they all resolve at almost the same time)
            s = BuildPredatoryAvariceCastState(ref s.Next, 15.2f);
            s = BuildDissociationState(ref s.Next, 2.4f);
            s = BuildCataractState(ref s.Next, 9.7f);

            // dissociation + eruption + flood + coherence
            s = BuildDissociationState(ref s.Next, 9.8f);
            s = CommonStates.Cast(ref s.Next, Boss, AID.SewageEruption, 8.2f, 5, "Eruption");
            s.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;
            s = CommonStates.Cast(ref s.Next, Boss, AID.TaintedFlood, 2.3f, 3, "Flood");
            s.EndHint |= StateMachine.StateHint.GroupWithNext;
            s = BuildCoherenceState(ref s.Next, 4.7f);
            s.EndHint |= StateMachine.StateHint.PositioningEnd;

            s = BuildDoubledImpactState(ref s.Next, 7.5f);
            s = BuildMurkyDepthsState(ref s.Next, 3.2f);
            s = BuildSewageDelugeState(ref s.Next, 12.8f);

            // flow 3 (with coherence)
            s = CommonStates.Cast(ref s.Next, Boss, AID.ChannelingOverflow, 11.7f, 5, "Flow 3");
            s.Exit = () => FindComponent<ChannelingFlow>()!.Active = true;
            s.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;
            s = BuildCoherenceState(ref s.Next, 5.5f); // first hit is around coherence cast end
            s.EndHint |= StateMachine.StateHint.GroupWithNext;
            s = CommonStates.Timeout(ref s.Next, 10, "Flow resolve"); // second hit
            s.Exit = () => FindComponent<ChannelingFlow>()!.Active = false;
            s.EndHint |= StateMachine.StateHint.PositioningEnd;

            // dissociation + eruption
            s = BuildDissociationState(ref s.Next, 7.4f);
            s = CommonStates.Cast(ref s.Next, Boss, AID.SewageEruption, 8.3f, 5, "Eruption");
            s.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;
            s = CommonStates.Timeout(ref s.Next, 5, "Resolve");
            s.EndHint |= StateMachine.StateHint.PositioningEnd;

            s = BuildOminousBubblingState(ref s.Next, 3.4f);
            s = BuildDoubledImpactState(ref s.Next, 5.4f);
            s = BuildMurkyDepthsState(ref s.Next, 7.2f);
            s = BuildMurkyDepthsState(ref s.Next, 6.2f);

            s = CommonStates.Cast(ref s.Next, Boss, AID.Enrage, 5.3f, 10, "Enrage");
        }

        protected override void DrawArenaForegroundPost()
        {
            Arena.Actor(Boss(), Arena.ColorEnemy);
            Arena.Actor(Player(), Arena.ColorPC);
        }

        private StateMachine.State BuildMurkyDepthsState(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, Boss, AID.MurkyDepths, delay, 5, "MurkyDepths");
            s.EndHint |= StateMachine.StateHint.Raidwide;
            return s;
        }

        private StateMachine.State BuildDoubledImpactState(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, Boss, AID.DoubledImpact, delay, 5, "DoubledImpact");
            s.EndHint |= StateMachine.StateHint.Tankbuster;
            return s;
        }

        private StateMachine.State BuildSewageDelugeState(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, Boss, AID.SewageDeluge, delay, 5, "Deluge");
            s.EndHint |= StateMachine.StateHint.Raidwide;
            return s;
        }

        // if group continues, last state won't clear positioning flag
        private StateMachine.State BuildCataractState(ref StateMachine.State? link, float delay, bool continueGroup = false)
        {
            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.SpokenCataract] = new(null, () => FindComponent<Cataract>()!.CurState = Cataract.State.Spoken);
            dispatch[AID.WingedCataract] = new(null, () => FindComponent<Cataract>()!.CurState = Cataract.State.Winged);
            var start = CommonStates.CastStart(ref link, Boss, dispatch, delay);
            start.EndHint |= StateMachine.StateHint.PositioningStart;

            var end = CommonStates.CastEnd(ref start.Next, Boss, 8, "Cataract");
            end.Exit = () => FindComponent<Cataract>()!.CurState = Cataract.State.None;
            end.EndHint |= continueGroup ? StateMachine.StateHint.GroupWithNext : StateMachine.StateHint.PositioningEnd;
            return end;
        }

        // avarice is always part of the group; note that subsequent states always set up positioning flag a bit later
        private StateMachine.State BuildPredatoryAvariceCastState(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, Boss, AID.PredatoryAvarice, delay, 4, "Avarice");
            s.EndHint |= StateMachine.StateHint.GroupWithNext;
            return s;
        }

        // avarice resolve always clears positioning flag
        private StateMachine.State BuildPredatoryAvariceResolveState(ref StateMachine.State? link, float delay)
        {
            var comp = FindComponent<PredatoryAvarice>()!;
            var s = CommonStates.Condition(ref link, delay, () => !comp.Active, "Avarice resolve");
            s.EndHint |= StateMachine.StateHint.PositioningEnd | StateMachine.StateHint.Raidwide;
            return s;
        }

        // dissociation is always part of the group
        private StateMachine.State BuildDissociationState(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, Boss, AID.Dissociation, delay, 4, "Dissociation");
            s.Exit = () => FindComponent<Dissociation>()!.Active = true;
            s.EndHint |= StateMachine.StateHint.GroupWithNext;
            return s;
        }

        private StateMachine.State BuildCoherenceState(ref StateMachine.State? link, float delay)
        {
            var comp = FindComponent<Coherence>()!;

            var start = CommonStates.CastStart(ref link, Boss, AID.Coherence, delay);
            start.Exit = () => comp.Active = true;

            var end = CommonStates.CastEnd(ref start.Next, Boss, 12);

            var resolve = CommonStates.Condition(ref end.Next, 3.1f, () => !comp.Active, "Coherence");
            resolve.Exit = () => comp.Active = false;
            resolve.EndHint |= StateMachine.StateHint.Raidwide;
            return resolve;
        }

        private StateMachine.State BuildShockwaveState(ref StateMachine.State? link, float delay)
        {
            // TODO: some component (knockback? or just make sure autorot uses arms length?)
            var s = CommonStates.Cast(ref link, Boss, AID.Shockwave, delay, 8, "Shockwave");
            s.EndHint |= StateMachine.StateHint.Knockback;
            return s;
        }

        // includes shockwave
        private StateMachine.State BuildOminousBubblingState(ref StateMachine.State? link, float delay)
        {
            // note: can determine bubbling targets by watching 233Cs cast OminousBubblingAOE on two targets
            // TODO: some component...
            var bubbling = CommonStates.Cast(ref link, Boss, AID.OminousBubbling, delay, 3, "TwoStacks");
            bubbling.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;
            var shockwave = BuildShockwaveState(ref bubbling.Next, 2.8f);
            shockwave.EndHint |= StateMachine.StateHint.GroupWithNext;
            var resolve = CommonStates.Timeout(ref shockwave.Next, 3.8f, "AOE resolve");
            resolve.EndHint |= StateMachine.StateHint.Raidwide | StateMachine.StateHint.PositioningEnd;
            return resolve;
        }

        private StateMachine.State BuildKampeosHarmaState(ref StateMachine.State? link, float delay)
        {
            var comp = FindComponent<KampeosHarma>()!;

            var start = CommonStates.CastStart(ref link, Boss, delay);
            start.Exit = () => comp.Start(Boss()!.Position);
            start.EndHint |= StateMachine.StateHint.PositioningStart;

            var end = CommonStates.CastEnd(ref start.Next, Boss, 8.4f, "Harma");
            end.EndHint |= StateMachine.StateHint.DowntimeStart | StateMachine.StateHint.GroupWithNext;

            var resolve = CommonStates.Condition(ref end.Next, 7.5f, () => Boss()?.IsTargetable ?? true, "Harma resolve", 1, 1); // protection for boss becoming untargetable slightly later than cast end
            resolve.Exit = () => comp.Reset();
            resolve.EndHint |= StateMachine.StateHint.DowntimeEnd | StateMachine.StateHint.PositioningEnd;
            return resolve;
        }
    }
}
