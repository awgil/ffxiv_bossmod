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
            Coherence = 26651, // Boss->Boss
            ChannelingFlow = 26654, // Boss->Boss
            Crash = 26657, // Helper->Helper, attack after arrows resolve
            KampeosHarma = 26659, // Boss->Boss
            KampeosHarmaCharge = 26660, // Boss->target, no cast
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
        private class SewageDeluge
        {
            public enum Corner { None, NW, NE, SW, SE };
            public Corner BlockedCorner = Corner.None;
            public bool Disable = false; // TODO: remove after confirming everything

            private static float _offsetCorner = 9.5f; // not sure
            private static float _cornerHalfSize = 4; // not sure
            private static float _connectHalfWidth = 2; // not sure
            private static float _cornerInner = _offsetCorner - _cornerHalfSize;
            private static float _cornerOuter = _offsetCorner + _cornerHalfSize;
            private static float _connectInner = _offsetCorner - _connectHalfWidth;
            private static float _connectOuter = _offsetCorner + _connectHalfWidth;

            private static Vector3[] _corners = { new(), new(-1, 0, -1), new(1, 0, -1), new(-1, 0, 1), new Vector3(1, 0, 1) };

            public void DrawArenaBackground(P2S self)
            {
                if (Disable || BlockedCorner == Corner.None)
                    return; // inactive

                // central area + H additionals
                self.Arena.ZoneQuad(self.Arena.WorldCenter,  Vector3.UnitX, _connectInner, _connectInner, _cornerInner, self.Arena.ColorAOE);
                // central V additionals
                self.Arena.ZoneQuad(self.Arena.WorldCenter,  Vector3.UnitZ, _connectInner, -_cornerInner, _cornerInner, self.Arena.ColorAOE);
                self.Arena.ZoneQuad(self.Arena.WorldCenter, -Vector3.UnitZ, _connectInner, -_cornerInner, _cornerInner, self.Arena.ColorAOE);
                // outer additionals
                self.Arena.ZoneQuad(self.Arena.WorldCenter,  Vector3.UnitX, _cornerOuter, -_connectOuter, _cornerInner, self.Arena.ColorAOE);
                self.Arena.ZoneQuad(self.Arena.WorldCenter, -Vector3.UnitX, _cornerOuter, -_connectOuter, _cornerInner, self.Arena.ColorAOE);
                self.Arena.ZoneQuad(self.Arena.WorldCenter,  Vector3.UnitZ, _cornerOuter, -_connectOuter, _cornerInner, self.Arena.ColorAOE);
                self.Arena.ZoneQuad(self.Arena.WorldCenter, -Vector3.UnitZ, _cornerOuter, -_connectOuter, _cornerInner, self.Arena.ColorAOE);
                // outer area
                self.Arena.ZoneQuad(self.Arena.WorldCenter,  Vector3.UnitX, self.Arena.WorldHalfSize, -_cornerOuter, self.Arena.WorldHalfSize, self.Arena.ColorAOE);
                self.Arena.ZoneQuad(self.Arena.WorldCenter, -Vector3.UnitX, self.Arena.WorldHalfSize, -_cornerOuter, self.Arena.WorldHalfSize, self.Arena.ColorAOE);
                self.Arena.ZoneQuad(self.Arena.WorldCenter,  Vector3.UnitZ, self.Arena.WorldHalfSize, -_cornerOuter, _cornerOuter, self.Arena.ColorAOE);
                self.Arena.ZoneQuad(self.Arena.WorldCenter, -Vector3.UnitZ, self.Arena.WorldHalfSize, -_cornerOuter, _cornerOuter, self.Arena.ColorAOE);

                var corner = self.Arena.WorldCenter + _corners[(int)BlockedCorner] * _offsetCorner;
                self.Arena.ZoneQuad(corner, Vector3.UnitX, _cornerHalfSize, _cornerHalfSize, _cornerHalfSize, self.Arena.ColorAOE);
            }

            public void DrawArenaForeground(P2S self)
            {
                if (Disable)
                    return;

                // inner border
                self.Arena.PathLineTo(self.Arena.WorldCenter + new Vector3(-_cornerInner,  0, -_cornerInner));
                self.Arena.PathLineTo(self.Arena.WorldCenter + new Vector3(-_cornerInner,  0, -_connectInner));
                self.Arena.PathLineTo(self.Arena.WorldCenter + new Vector3(+_cornerInner,  0, -_connectInner));
                self.Arena.PathLineTo(self.Arena.WorldCenter + new Vector3(+_cornerInner,  0, -_cornerInner));
                self.Arena.PathLineTo(self.Arena.WorldCenter + new Vector3(+_connectInner, 0, -_cornerInner));
                self.Arena.PathLineTo(self.Arena.WorldCenter + new Vector3(+_connectInner, 0, +_cornerInner));
                self.Arena.PathLineTo(self.Arena.WorldCenter + new Vector3(+_cornerInner,  0, +_cornerInner));
                self.Arena.PathLineTo(self.Arena.WorldCenter + new Vector3(+_cornerInner,  0, +_connectInner));
                self.Arena.PathLineTo(self.Arena.WorldCenter + new Vector3(-_cornerInner,  0, +_connectInner));
                self.Arena.PathLineTo(self.Arena.WorldCenter + new Vector3(-_cornerInner,  0, +_cornerInner));
                self.Arena.PathLineTo(self.Arena.WorldCenter + new Vector3(-_connectInner, 0, +_cornerInner));
                self.Arena.PathLineTo(self.Arena.WorldCenter + new Vector3(-_connectInner, 0, -_cornerInner));
                self.Arena.PathStroke(true, self.Arena.ColorBorder);

                // outer border
                self.Arena.PathLineTo(self.Arena.WorldCenter + new Vector3(-_cornerOuter,  0, -_cornerOuter));
                self.Arena.PathLineTo(self.Arena.WorldCenter + new Vector3(-_cornerInner,  0, -_cornerOuter));
                self.Arena.PathLineTo(self.Arena.WorldCenter + new Vector3(-_cornerInner,  0, -_connectOuter));
                self.Arena.PathLineTo(self.Arena.WorldCenter + new Vector3(+_cornerInner,  0, -_connectOuter));
                self.Arena.PathLineTo(self.Arena.WorldCenter + new Vector3(+_cornerInner,  0, -_cornerOuter));
                self.Arena.PathLineTo(self.Arena.WorldCenter + new Vector3(+_cornerOuter,  0, -_cornerOuter));
                self.Arena.PathLineTo(self.Arena.WorldCenter + new Vector3(+_cornerOuter,  0, -_cornerInner));
                self.Arena.PathLineTo(self.Arena.WorldCenter + new Vector3(+_connectOuter, 0, -_cornerInner));
                self.Arena.PathLineTo(self.Arena.WorldCenter + new Vector3(+_connectOuter, 0, +_cornerInner));
                self.Arena.PathLineTo(self.Arena.WorldCenter + new Vector3(+_cornerOuter,  0, +_cornerInner));
                self.Arena.PathLineTo(self.Arena.WorldCenter + new Vector3(+_cornerOuter,  0, +_cornerOuter));
                self.Arena.PathLineTo(self.Arena.WorldCenter + new Vector3(+_cornerInner,  0, +_cornerOuter));
                self.Arena.PathLineTo(self.Arena.WorldCenter + new Vector3(+_cornerInner,  0, +_connectOuter));
                self.Arena.PathLineTo(self.Arena.WorldCenter + new Vector3(-_cornerInner,  0, +_connectOuter));
                self.Arena.PathLineTo(self.Arena.WorldCenter + new Vector3(-_cornerInner,  0, +_cornerOuter));
                self.Arena.PathLineTo(self.Arena.WorldCenter + new Vector3(-_cornerOuter,  0, +_cornerOuter));
                self.Arena.PathLineTo(self.Arena.WorldCenter + new Vector3(-_cornerOuter,  0, +_cornerInner));
                self.Arena.PathLineTo(self.Arena.WorldCenter + new Vector3(-_connectOuter, 0, +_cornerInner));
                self.Arena.PathLineTo(self.Arena.WorldCenter + new Vector3(-_connectOuter, 0, -_cornerInner));
                self.Arena.PathLineTo(self.Arena.WorldCenter + new Vector3(-_cornerOuter,  0, -_cornerInner));
                self.Arena.PathStroke(true, self.Arena.ColorBorder);
            }
        }

        // state related to cataract mechanic
        private class Cataract
        {
            public enum State { None, Winged, Spoken }
            public State CurState = State.None;

            private static float _halfWidth = 7.5f;

            public void DrawArenaBackground(P2S self)
            {
                if (CurState == State.None || self._boss == null)
                    return;

                self.Arena.ZoneQuad(self._boss.Position, self._boss.Rotation, self.Arena.WorldHalfSize, self.Arena.WorldHalfSize, _halfWidth, self.Arena.ColorAOE);

                if (self._cataractHead?.CastInfo != null)
                {
                    float headRot = CurState == State.Winged ? MathF.PI : 0;
                    self.Arena.ZoneQuad(self._cataractHead.Position, self._cataractHead.Rotation + headRot, self.Arena.WorldHalfSize, 0, self.Arena.WorldHalfSize, self.Arena.ColorAOE);
                }
            }

            public void AddHints(P2S self, StringBuilder res)
            {
                var pc = self.RaidMember(self.PlayerSlot);
                if (CurState == State.None || self._boss == null || pc == null)
                    return;

                if (GeometryUtils.PointInRect(pc.Position - self._boss.Position, self._boss.Rotation, self.Arena.WorldHalfSize, self.Arena.WorldHalfSize, _halfWidth))
                {
                    res.Append("GTFO from cataract! ");
                    return;
                }

                if (self._cataractHead?.CastInfo != null)
                {
                    float headRot = CurState == State.Winged ? MathF.PI : 0;
                    if (GeometryUtils.PointInRect(pc.Position - self._cataractHead.Position, self._cataractHead.Rotation + headRot, self.Arena.WorldHalfSize, 0, self.Arena.WorldHalfSize))
                    {
                        res.Append("GTFO from cataract! ");
                    }
                }
            }
        }

        // state related to dissociation mechanic
        private class Dissociation
        {
            private static float _halfWidth = 10;

            public void DrawArenaBackground(P2S self)
            {
                if (self._dissocHead?.CastInfo != null)
                    self.Arena.ZoneQuad(self._dissocHead.Position, self._dissocHead.Rotation, 50, 0, _halfWidth, self.Arena.ColorAOE);
            }

            public void AddHints(P2S self, StringBuilder res)
            {
                var pc = self.RaidMember(self.PlayerSlot);
                if (pc != null && self._dissocHead?.CastInfo != null && GeometryUtils.PointInRect(pc.Position - self._dissocHead.Position, self._dissocHead.Rotation, 50, 0, _halfWidth))
                {
                    res.Append("GTFO from dissociation! ");
                }
            }
        }

        // state related to coherence mechanic
        private class Coherence
        {
            public bool Active;
            private int _closest = -1;

            private static float _aoeHalfWidth = 3;

            public void Update(P2S self)
            {
                _closest = -1;
                if (!Active || self._boss == null)
                    return;

                float minDistSq = 100000;
                foreach ((int i, var player) in self.IterateRaidMembers())
                {
                    float dist = (player.Position - self._boss.Position).LengthSquared();
                    if (dist < minDistSq)
                    {
                        minDistSq = dist;
                        _closest = i;
                    }
                }
            }

            public void DrawArenaBackground(P2S self)
            {
                var closest = self.RaidMember(_closest);
                if (!Active || self._boss == null || closest == null || self._boss.Position == closest.Position)
                    return;

                var dir = Vector3.Normalize(closest.Position - self._boss.Position);
                self.Arena.ZoneQuad(self._boss.Position, dir, 50, 0, _aoeHalfWidth, self.Arena.ColorAOE);
            }

            public void DrawArenaForeground(P2S self)
            {
                var closest = self.RaidMember(_closest);
                if (!Active || self._boss == null || closest == null || self._boss.Position == closest.Position)
                    return;

                // TODO: i'm not sure what are the exact mechanics - flare is probably distance-based, and ray is probably shared damage cast at closest target?..
                var dirToClosest = Vector3.Normalize(closest.Position - self._boss.Position);
                foreach ((int i, var player) in self.IterateRaidMembers())
                {
                    if (player.Tether.ID == (uint)TetherID.Coherence)
                    {
                        self.Arena.AddLine(player.Position, self._boss.Position, self.Arena.ColorDanger);
                        self.Arena.Actor(player.Position, player.Rotation, self.Arena.ColorDanger);
                    }
                    else if (i == _closest)
                    {
                        self.Arena.Actor(player.Position, player.Rotation, self.Arena.ColorDanger);
                    }
                    else
                    {
                        bool inAOE = GeometryUtils.PointInRect(player.Position - self._boss.Position, dirToClosest, 50, 0, _aoeHalfWidth);
                        self.Arena.Actor(player.Position, player.Rotation, inAOE ? self.Arena.ColorPlayerInteresting : self.Arena.ColorPlayerGeneric);
                    }
                }
            }
        }

        // state related to predatory avarice mechanic
        // driven purely by debuffs
        private class PredatoryAvarice
        {
            private ulong _playersWithTides = 0;
            private ulong _playersWithDepths = 0;
            private ulong _playersInTides = 0;
            private ulong _playersInDepths = 0;

            private static float _tidesRadius = 10;
            private static float _depthsRadius = 6;

            public bool Active => (_playersWithTides | _playersWithDepths) != 0;

            public void ModifyDebuff(int slot, bool tides, bool active)
            {
                if (slot < 0)
                    return;

                if (tides)
                    BitVector.ModifyVector64Bit(ref _playersWithTides, slot, active);
                else
                    BitVector.ModifyVector64Bit(ref _playersWithDepths, slot, active);
            }

            public void Update(P2S self)
            {
                _playersInTides = _playersInDepths = 0;
                if (!Active)
                    return;

                foreach ((int i, var actor) in self.IterateRaidMembers())
                {
                    if (BitVector.IsVector64BitSet(_playersWithTides, i))
                    {
                        _playersInTides |= self.FindRaidMembersInRange(i, _tidesRadius);
                    }
                    else if (BitVector.IsVector64BitSet(_playersWithDepths, i))
                    {
                        _playersInDepths |= self.FindRaidMembersInRange(i, _depthsRadius);
                    }
                }
            }

            public void DrawArenaForeground(P2S self)
            {
                var pc = self.RaidMember(self.PlayerSlot);
                if (!Active || pc == null)
                    return;

                bool pcHasTides = BitVector.IsVector64BitSet(_playersWithTides, self.PlayerSlot);
                bool pcHasDepths = BitVector.IsVector64BitSet(_playersWithDepths, self.PlayerSlot);
                foreach ((int i, var actor) in self.IterateRaidMembers())
                {
                    if (BitVector.IsVector64BitSet(_playersWithTides, i))
                    {
                        // tides are always drawn
                        self.Arena.AddCircle(actor.Position, _tidesRadius, self.Arena.ColorDanger);
                        self.Arena.Actor(actor.Position, actor.Rotation, self.Arena.ColorDanger);
                    }
                    else if (BitVector.IsVector64BitSet(_playersWithDepths, i) && !pcHasTides)
                    {
                        // depths are drawn only if pc has no tides - otherwise it is to be considered a generic player
                        self.Arena.AddCircle(actor.Position, _tidesRadius, self.Arena.ColorSafe);
                        self.Arena.Actor(actor.Position, actor.Rotation, self.Arena.ColorDanger);
                    }
                    else if (pcHasTides || pcHasDepths)
                    {
                        // other players are only drawn if pc has some debuff
                        bool playerInteresting = BitVector.IsVector64BitSet(pcHasTides ? _playersInTides : _playersInDepths, i);
                        self.Arena.Actor(actor.Position, actor.Rotation, playerInteresting ? self.Arena.ColorPlayerInteresting : self.Arena.ColorPlayerGeneric);
                    }
                }
            }

            public void AddHints(P2S self, StringBuilder res)
            {
                var pc = self.RaidMember(self.PlayerSlot);
                if (!Active || pc == null)
                    return;

                if (BitVector.IsVector64BitSet(_playersWithTides, self.PlayerSlot))
                {
                    if (self.IterateRaidMembersInRange(self.PlayerSlot, _tidesRadius).Any())
                    {
                        res.Append("GTFO from raid! ");
                    }
                }
                else
                {
                    if (BitVector.IsVector64BitSet(_playersInTides, self.PlayerSlot))
                    {
                        res.Append("GTFO from avarice! ");
                    }

                    bool warnToStack = BitVector.IsVector64BitSet(_playersWithDepths, self.PlayerSlot)
                        ? BitOperations.PopCount(_playersInDepths) < 4 :
                        !BitVector.IsVector64BitSet(_playersInDepths, self.PlayerSlot);
                    if (warnToStack)
                    {
                        res.Append("Stack with raid! ");
                    }
                }
            }
        }

        // state related to kampeos harma mechanic
        private class KampeosHarma
        {
            // icon order: sq1 sq2 sq3 sq4 tri1 tri2 tri3 tri4
            public int PlayerOrder = -1;
            public Vector3 StartingOffset = new();
            public int NumCharges = 0;

            public void Reset()
            {
                PlayerOrder = -1;
                NumCharges = 0;
            }

            public void DrawArenaForeground(P2S self)
            {
                var pos = GetSafeZone(self);
                if (pos != null)
                    self.Arena.AddCircle(pos.Value, 1, self.Arena.ColorSafe);
            }

            private Vector3? GetSafeZone(P2S self)
            {
                switch (PlayerOrder)
                {
                    case 0: // sq 1 - opposite corner, hide after first charge
                        return self.Arena.WorldCenter + (NumCharges < 1 ? -1.1f : -1.2f) * StartingOffset;
                    case 1: // sq 2 - same corner, hide after second charge
                        return self.Arena.WorldCenter + (NumCharges < 2 ? +1.1f : +1.2f) * StartingOffset;
                    case 2: // sq 3 - opposite corner, hide before first charge
                        return self.Arena.WorldCenter + (NumCharges < 1 ? -1.2f : -1.1f) * StartingOffset;
                    case 3: // sq 4 - same corner, hide before second charge
                        return self.Arena.WorldCenter + (NumCharges < 2 ? +1.2f : +1.1f) * StartingOffset;
                    case 4: // tri 1 - waymark 1
                        return self.WorldState.GetWaymark(WorldState.Waymark.N1);
                    case 5: // tri 2 - waymark 2
                        return self.WorldState.GetWaymark(WorldState.Waymark.N2);
                    case 6: // tri 3 - waymark 3
                        return self.WorldState.GetWaymark(WorldState.Waymark.N3);
                    case 7: // tri 4 - waymark 4
                        return self.WorldState.GetWaymark(WorldState.Waymark.N4);
                }
                return null;
            }
        }

        // state related to channeling [over]flow mechanics
        private class ChannelingFlow
        {
            public bool Active = false;

            private static float _typhoonHalfWidth = 2.5f;

            public void DrawArenaBackground(P2S self)
            {
                if (!Active)
                    return;

                foreach ((int i, var player) in self.IterateRaidMembers())
                {
                    foreach (var status in player.Statuses)
                    {
                        if (status.RemainingTime > 10)
                            continue;

                        switch ((SID)status.ID)
                        {
                            case SID.MarkFlowN:
                                self.Arena.ZoneQuad(player.Position, -Vector3.UnitZ, 50, 0, _typhoonHalfWidth, self.Arena.ColorAOE);
                                break;
                            case SID.MarkFlowS:
                                self.Arena.ZoneQuad(player.Position,  Vector3.UnitZ, 50, 0, _typhoonHalfWidth, self.Arena.ColorAOE);
                                break;
                            case SID.MarkFlowW:
                                self.Arena.ZoneQuad(player.Position, -Vector3.UnitX, 50, 0, _typhoonHalfWidth, self.Arena.ColorAOE);
                                break;
                            case SID.MarkFlowE:
                                self.Arena.ZoneQuad(player.Position,  Vector3.UnitX, 50, 0, _typhoonHalfWidth, self.Arena.ColorAOE);
                                break;
                        }
                    }
                }
            }
        }

        private WorldState.Actor? _boss;
        private WorldState.Actor? _cataractHead;
        private WorldState.Actor? _dissocHead;
        private SewageDeluge _sewageDeluge = new();
        private Cataract _cataract = new();
        private Dissociation _dissociation = new();
        private Coherence _coherence = new();
        private PredatoryAvarice _predatoryAvarice = new();
        private KampeosHarma _kampeosHarma = new();
        private ChannelingFlow _channelingFlow = new();

        public P2S(WorldState ws)
            : base(ws, 8)
        {
            WorldState.ActorStatusGain += ActorStatusGain;
            WorldState.ActorStatusLose += ActorStatusLose;
            WorldState.EventIcon += EventIcon;
            WorldState.EventCast += EventCast;
            WorldState.EventEnvControl += EventEnvControl;

            StateMachine.State? s;
            s = BuildMurkyDepthsState(ref InitialState, 10);
            s = BuildDoubledImpactState(ref s.Next, 5.2f);
            s = BuildSewageDelugeState(ref s.Next, 7.8f);
            s = BuildCataractState(ref s.Next, 14.6f);
            s = BuildCoherenceState(ref s.Next, 8.1f);
            s = BuildMurkyDepthsState(ref s.Next, 7.4f);
            s = BuildOminousBubblingState(ref s.Next, 3.7f);

            // avarice + cataract
            s = BuildPredatoryAvariceCastState(ref s.Next, 11.7f);
            s = BuildCataractState(ref s.Next, 9.7f, true);
            s = BuildPredatoryAvariceResolveState(ref s.Next, 6.1f);
            // note: deluge 1 ends here...

            // first flow
            s = CommonStates.Cast(ref s.Next, () => _boss, AID.ChannelingFlow, 8.5f, 5, "Flow 1");
            s.Exit = () => _channelingFlow.Active = true;
            s.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;
            s = CommonStates.Timeout(ref s.Next, 14);
            s.EndHint |= StateMachine.StateHint.PositioningEnd | StateMachine.StateHint.DowntimeStart;
            s = CommonStates.Timeout(ref s.Next, 3, "Flow resolve");
            s.Exit = () => _channelingFlow.Active = false;
            s.EndHint |= StateMachine.StateHint.DowntimeEnd | StateMachine.StateHint.Raidwide;

            s = BuildDoubledImpactState(ref s.Next, 8.2f);
            s = BuildMurkyDepthsState(ref s.Next, 5.2f);
            s = BuildSewageDelugeState(ref s.Next, 11.6f);
            s = BuildShockwaveState(ref s.Next, 9.6f);
            s = BuildKampeosHarmaState(ref s.Next, 4.5f);
            s = BuildDoubledImpactState(ref s.Next, 9.4f);
            s = BuildMurkyDepthsState(ref s.Next, 4.2f);

            // second flow (same statuses, different durations)
            s = CommonStates.Cast(ref s.Next, () => _boss, AID.ChannelingOverflow, 8.7f, 5, "Flow 2");
            s.Exit = () => _channelingFlow.Active = true;
            s.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;
            s = CommonStates.Cast(ref s.Next, () => _boss, AID.TaintedFlood, 4.2f, 3);
            s = CommonStates.Timeout(ref s.Next, 9, "Hit 1");
            s.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.Raidwide;
            s = CommonStates.Cast(ref s.Next, () => _boss, AID.TaintedFlood, 3.4f, 3);
            s = CommonStates.Timeout(ref s.Next, 9, "Hit 2");
            s.Exit = () => _channelingFlow.Active = false;
            s.EndHint |= StateMachine.StateHint.Raidwide | StateMachine.StateHint.PositioningEnd;

            s = BuildCataractState(ref s.Next, 1.2f); // too close to prev timeout...
            // note: deluge 2 ends here...

            // avarice + dissociation + cataract (note that we don't create separate avarice resolve state here, since it resolves just before cataract)
            s = BuildPredatoryAvariceCastState(ref s.Next, 15.2f);
            s = BuildDissociationState(ref s.Next, 2.4f);
            s = BuildCataractState(ref s.Next, 9.7f);

            // dissociation + eruption + flood + coherence
            s = BuildDissociationState(ref s.Next, 9.8f);
            s = CommonStates.Cast(ref s.Next, () => _boss, AID.SewageEruption, 8.2f, 5, "Eruption");
            s.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;
            s = CommonStates.Cast(ref s.Next, () => _boss, AID.TaintedFlood, 2.3f, 3, "Flood");
            s.EndHint |= StateMachine.StateHint.GroupWithNext;
            s = BuildCoherenceState(ref s.Next, 4.7f);
            s.EndHint |= StateMachine.StateHint.PositioningEnd;

            s = BuildDoubledImpactState(ref s.Next, 6.6f);
            s = BuildMurkyDepthsState(ref s.Next, 3.2f);
            s = BuildSewageDelugeState(ref s.Next, 12.7f);

            // flow 3 (with coherence)
            s = CommonStates.Cast(ref s.Next, () => _boss, AID.ChannelingOverflow, 11.9f, 5, "Flow 3");
            s.Exit = () => _channelingFlow.Active = true;
            s.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;
            s = BuildCoherenceState(ref s.Next, 5.5f); // first hit is around coherence cast end
            s.EndHint |= StateMachine.StateHint.GroupWithNext;
            s = CommonStates.Timeout(ref s.Next, 10, "Flow resolve"); // second hit
            s.Exit = () => _channelingFlow.Active = false;
            s.EndHint |= StateMachine.StateHint.PositioningEnd;

            // dissociation + eruption
            s = BuildDissociationState(ref s.Next, 7.4f);
            s = CommonStates.Cast(ref s.Next, () => _boss, AID.SewageEruption, 8.3f, 5, "Eruption");
            s.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;
            s = CommonStates.Timeout(ref s.Next, 5, "Resolve");
            s.EndHint |= StateMachine.StateHint.PositioningEnd;

            s = BuildOminousBubblingState(ref s.Next, 3.4f);
            s = BuildDoubledImpactState(ref s.Next, 6.2f);
            s = BuildMurkyDepthsState(ref s.Next, 7.2f);
            s = BuildMurkyDepthsState(ref s.Next, 6.2f);

            s = CommonStates.Cast(ref s.Next, () => _boss, AID.Enrage, 6, 10, "Enrage");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                WorldState.ActorStatusGain -= ActorStatusGain;
                WorldState.ActorStatusLose -= ActorStatusLose;
                WorldState.EventIcon -= EventIcon;
                WorldState.EventCast -= EventCast;
                WorldState.EventEnvControl -= EventEnvControl;
            }
            base.Dispose(disposing);
        }

        public override void Update()
        {
            base.Update();
            _predatoryAvarice.Update(this);
        }

        protected override void DrawHeader()
        {
            var hints = new StringBuilder();
            _cataract.AddHints(this, hints);
            _dissociation.AddHints(this, hints);
            _predatoryAvarice.AddHints(this, hints);
            ImGui.TextColored(ImGui.ColorConvertU32ToFloat4(0xff00ffff), hints.ToString());
        }

        protected override void DrawArena()
        {
            _sewageDeluge.DrawArenaBackground(this);
            _cataract.DrawArenaBackground(this);
            _dissociation.DrawArenaBackground(this);
            _channelingFlow.DrawArenaBackground(this);

            Arena.Border();
            _sewageDeluge.DrawArenaForeground(this);
            _coherence.DrawArenaForeground(this);
            _predatoryAvarice.DrawArenaForeground(this);
            _kampeosHarma.DrawArenaForeground(this);

            if (_boss != null)
                Arena.Actor(_boss.Position, _boss.Rotation, Arena.ColorEnemy);

            // draw player
            var pc = RaidMember(PlayerSlot);
            if (pc != null)
                Arena.Actor(pc.Position, pc.Rotation, Arena.ColorPC);
        }

        protected override void DrawFooter()
        {
            // TODO: temp, debug
            ImGui.Checkbox("Disable deluge", ref _sewageDeluge.Disable);
            ImGui.SameLine();
        }

        protected override void NonPlayerCreated(WorldState.Actor actor)
        {
            switch ((OID)actor.OID)
            {
                case OID.Boss:
                    if (_boss != null)
                        Service.Log($"[P2S] Created boss {actor.InstanceID} while another is still alive: {_boss.InstanceID}");
                    _boss = actor;
                    break;
                case OID.CataractHead:
                    if (_cataractHead != null)
                        Service.Log($"[P2S] Created cataract head {actor.InstanceID} while another is still alive: {_cataractHead.InstanceID}");
                    _cataractHead = actor;
                    break;
                case OID.DissociatedHead:
                    if (_dissocHead != null)
                        Service.Log($"[P2S] Created dissociated head {actor.InstanceID} while another is still alive: {_dissocHead.InstanceID}");
                    _dissocHead = actor;
                    break;
            }
        }

        protected override void NonPlayerDestroyed(WorldState.Actor actor)
        {
            switch ((OID)actor.OID)
            {
                case OID.Boss:
                    if (_boss != actor)
                        Service.Log($"[P2S] Destroying boss {actor.InstanceID} while another is active: {_boss?.InstanceID}");
                    else
                        _boss = null;
                    break;
                case OID.CataractHead:
                    if (_cataractHead != actor)
                        Service.Log($"[P2S] Destroying cataract head {actor.InstanceID} while another is active: {_cataractHead?.InstanceID}");
                    else
                        _cataractHead = null;
                    break;
                case OID.DissociatedHead:
                    if (_dissocHead != actor)
                        Service.Log($"[P2S] Destroying dissociated head {actor.InstanceID} while another is active: {_dissocHead?.InstanceID}");
                    else
                        _dissocHead = null;
                    break;
            }
        }

        protected override void Reset()
        {
            _sewageDeluge.BlockedCorner = SewageDeluge.Corner.None;
            _cataract.CurState = Cataract.State.None;
            _coherence.Active = false;
            _kampeosHarma.Reset();
            _channelingFlow.Active = false;
        }

        private StateMachine.State BuildMurkyDepthsState(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, () => _boss, AID.MurkyDepths, delay, 5, "MurkyDepths");
            s.EndHint |= StateMachine.StateHint.Raidwide;
            return s;
        }

        private StateMachine.State BuildDoubledImpactState(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, () => _boss, AID.DoubledImpact, delay, 5, "DoubledImpact");
            s.EndHint |= StateMachine.StateHint.Tankbuster;
            return s;
        }

        private StateMachine.State BuildSewageDelugeState(ref StateMachine.State? link, float delay)
        {
            // note: deluge component is controlled by events rather than states
            var s = CommonStates.Cast(ref link, () => _boss, AID.SewageDeluge, delay, 5, "Deluge");
            s.EndHint |= StateMachine.StateHint.Raidwide;
            return s;
        }

        // if group continues, last state won't clear positioning flag
        private StateMachine.State BuildCataractState(ref StateMachine.State? link, float delay, bool continueGroup = false)
        {
            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.SpokenCataract] = new(null, () => _cataract.CurState = Cataract.State.Spoken);
            dispatch[AID.WingedCataract] = new(null, () => _cataract.CurState = Cataract.State.Winged);
            var start = CommonStates.CastStart(ref link, () => _boss, dispatch, delay);
            start.EndHint |= StateMachine.StateHint.PositioningStart;

            var end = CommonStates.CastEnd(ref start.Next, () => _boss, 8, "Cataract");
            end.Exit = () => _cataract.CurState = Cataract.State.None;
            end.EndHint |= continueGroup ? StateMachine.StateHint.GroupWithNext : StateMachine.StateHint.PositioningEnd;
            return end;
        }

        // avarice is always part of the group; note that subsequent states always set up positioning flag a bit later
        private StateMachine.State BuildPredatoryAvariceCastState(ref StateMachine.State? link, float delay)
        {
            // note: avarice component is controlled by events rather than states
            var s = CommonStates.Cast(ref link, () => _boss, AID.PredatoryAvarice, delay, 4, "Avarice");
            s.EndHint |= StateMachine.StateHint.GroupWithNext;
            return s;
        }

        // avarice resolve always clears positioning flag
        private StateMachine.State BuildPredatoryAvariceResolveState(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Simple(ref link, delay, "Avarice resolve");
            s.Update = (float timeSinceActivation) => s.Done = !_predatoryAvarice.Active;
            s.EndHint |= StateMachine.StateHint.PositioningEnd | StateMachine.StateHint.Raidwide;
            return s;
        }

        // dissociation is always part of the group
        private StateMachine.State BuildDissociationState(ref StateMachine.State? link, float delay)
        {
            // note: dissociation component is controlled by events rather than states
            var s = CommonStates.Cast(ref link, () => _boss, AID.Dissociation, delay, 4, "Dissociation");
            s.EndHint |= StateMachine.StateHint.GroupWithNext;
            return s;
        }

        private StateMachine.State BuildCoherenceState(ref StateMachine.State? link, float delay)
        {
            var start = CommonStates.CastStart(ref link, () => _boss, AID.Coherence, delay);
            start.Exit = () => _coherence.Active = true;

            var end = CommonStates.CastEnd(ref start.Next, () => _boss, 12);

            var resolve = CommonStates.Timeout(ref end.Next, 4, "Coherence");
            resolve.Exit = () => _coherence.Active = false;
            resolve.EndHint |= StateMachine.StateHint.Raidwide;
            return resolve;
        }

        private StateMachine.State BuildShockwaveState(ref StateMachine.State? link, float delay)
        {
            // TODO: helper (knockback? or just make sure autorot uses arms length?)
            var s = CommonStates.Cast(ref link, () => _boss, AID.Shockwave, delay, 8, "Shockwave");
            s.EndHint |= StateMachine.StateHint.Knockback;
            return s;
        }

        // includes shockwave
        private StateMachine.State BuildOminousBubblingState(ref StateMachine.State? link, float delay)
        {
            // note: can determine bubbling targets by watching 233Cs cast OminousBubblingAOE on two targets
            var bubbling = CommonStates.Cast(ref link, () => _boss, AID.OminousBubbling, delay, 3, "TwoStacks");
            bubbling.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;
            var shockwave = BuildShockwaveState(ref bubbling.Next, 2.8f);
            shockwave.EndHint |= StateMachine.StateHint.GroupWithNext;
            var resolve = CommonStates.Timeout(ref shockwave.Next, 3, "AOE resolve");
            resolve.EndHint |= StateMachine.StateHint.Raidwide | StateMachine.StateHint.PositioningEnd;
            return resolve;
        }

        private StateMachine.State BuildKampeosHarmaState(ref StateMachine.State? link, float delay)
        {
            var start = CommonStates.CastStart(ref link, () => _boss, delay);
            start.Exit = () => _kampeosHarma.StartingOffset = _boss!.Position - Arena.WorldCenter;
            start.EndHint |= StateMachine.StateHint.PositioningStart;

            var end = CommonStates.CastEnd(ref start.Next, () => _boss, 8.4f, "Harma");
            end.EndHint |= StateMachine.StateHint.DowntimeStart | StateMachine.StateHint.GroupWithNext;

            var resolve = CommonStates.Timeout(ref end.Next, 8, "Harma resolve");
            resolve.Exit = () => _kampeosHarma.Reset();
            resolve.EndHint |= StateMachine.StateHint.DowntimeEnd | StateMachine.StateHint.PositioningEnd;
            return resolve;
        }

        private void ActorStatusGain(object? sender, (WorldState.Actor actor, int index) arg)
        {
            switch ((SID)arg.actor.Statuses[arg.index].ID)
            {
                case SID.MarkOfTides:
                    _predatoryAvarice.ModifyDebuff(FindRaidMemberSlot(arg.actor.InstanceID), true, true);
                    break;
                case SID.MarkOfDepths:
                    _predatoryAvarice.ModifyDebuff(FindRaidMemberSlot(arg.actor.InstanceID), false, true);
                    break;
            }
        }

        private void ActorStatusLose(object? sender, (WorldState.Actor actor, int index) arg)
        {
            switch ((SID)arg.actor.Statuses[arg.index].ID)
            {
                case SID.MarkOfTides:
                    _predatoryAvarice.ModifyDebuff(FindRaidMemberSlot(arg.actor.InstanceID), true, false);
                    break;
                case SID.MarkOfDepths:
                    _predatoryAvarice.ModifyDebuff(FindRaidMemberSlot(arg.actor.InstanceID), false, false);
                    break;
            }
        }

        private void EventIcon(object? sender, (uint actorID, uint iconID) arg)
        {
            if (arg.iconID >= 145 && arg.iconID <= 152 && arg.actorID == WorldState.PlayerActorID)
            {
                _kampeosHarma.PlayerOrder = (int)(arg.iconID - 145);
            }
        }

        private void EventCast(object? sender, WorldState.CastResult info)
        {
            switch ((AID)info.ActionID)
            {
                case AID.KampeosHarmaCharge:
                    ++_kampeosHarma.NumCharges;
                    break;
            }
        }

        private void EventEnvControl(object? sender, (uint featureID, byte index, uint state) arg)
        {
            // 800375A2: we typically get two events for index=0 (global) and index=N (corner)
            // state 00200010 - "prepare" (show aoe that is still harmless)
            // state 00020001 - "active" (dot in center/borders, oneshot in corner)
            // state 00080004 - "finish" (reset)
            if (arg.featureID == 0x800375A2 && arg.index > 0 && arg.index < 5)
            {
                switch (arg.state)
                {
                    case 0x00200010:
                        _sewageDeluge.BlockedCorner = (SewageDeluge.Corner)arg.index;
                        Service.Log($"[P2S] Debug: starting deluge, blocking corner {_sewageDeluge.BlockedCorner}");
                        break;
                    case 0x00080004:
                        _sewageDeluge.BlockedCorner = SewageDeluge.Corner.None;
                        Service.Log($"[P2S] Debug: ending deluge");
                        break;
                }
            }
        }
    }
}
