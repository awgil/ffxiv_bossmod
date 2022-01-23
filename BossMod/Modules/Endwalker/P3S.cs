using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace BossMod
{
    public class P3S : BossModule
    {
        public enum OID : uint
        {
            Boss = 0x353F,
            Sparkfledged = 0x3540, // spawned mid fight, "eyes" with cone aoe
            SunbirdSmall = 0x3541, // spawned mid fight
            SunbirdLarge = 0x3543, // spawned mid fight
            Sunshadow = 0x3544, // spawned mid fight, mini birds that charge during fountains of fire
            DarkenedFire = 0x3545, // spawned mid fight
            FountainOfFire = 0x3546, // spawned mid fight, towers that healers soak
            DarkblazeTwister = 0x3547, // spawned mid fight, tornadoes
            SparkfledgedUnknown = 0x3800, // spawned mid fight, have weird kind... - look like "eyes" during death toll?..
            Helper = 0x233C, // x45
        };

        public enum AID : uint
        {
            FledglingFlight = 26282, // Boss->Boss
            DarkenedFire = 26297, // Boss->Boss
            DarkenedFireFail = 26298, // DarkenedFire->DarkenedFire, no cast - wipe if they spawn too close to each other
            DarkenedBlaze = 26299, // DarkenedFire->DarkenedFire, 22sec cast
            BrightenedFire = 26300, // Boss->Boss
            BrightenedFireAOE = 26301, // Boss->target, no cast
            ExperimentalFireplumeSingle = 26302, // Boss->Boss
            ExperimentalFireplumeSingleAOE = 26303, // Helper->Helper
            ExperimentalFireplumeMulti = 26304, // Boss->Boss
            ExperimentalFireplumeMultiAOE = 26305, // Helper->Helper
            ExperimentalAshplumeStack = 26306, // Boss->Boss
            ExperimentalAshplumeStackAOE = 26307, // Helper->targets, no cast
            ExperimentalAshplumeSpread = 26308, // Boss->Boss
            ExperimentalAshplumeSpreadAOE = 26309, // Helper->targets, no cast, 7sec after cast end
            ExperimentalGloryplumeSingle = 26310, // Boss->Boss, single+whatever variant
            ExperimentalGloryplumeSingleAOE = 26311, // Helper->Helper, 'normal single plume' aoe
            ExperimentalGloryplumeSpread = 26312, // Boss->Boss, no cast, cast 3sec after gloryplume cast end for 'spread' variant and determines visual cue
            ExperimentalGloryplumeSpreadAOE = 26313, // Helper->target, no cast, actual damage, ~10sec after cue
            ExperimentalGloryplumeMulti = 26314, // Boss->Boss, multi+whatever variant
            ExperimentalGloryplumeMultiAOE = 26315, // Helper->Helper, 'normal multi plume' aoes
            ExperimentalGloryplumeStack = 26316, // Boss->Boss, no cast, no cast, cast 3sec after gloryplume cast end for 'stack' variant and determines visual cue
            ExperimentalGloryplumeStackAOE = 26317, // Helper->target, no cast, actual damage, ~10sec after cue
            DevouringBrand = 26318, // Boss->Boss
            DevouringBrandMiniAOE = 26319, // Helper->Helper (ones standing on cardinals)
            DevouringBrandLargeAOE = 26321, // Helper->Helper (ones standing on cardinals)
            GreatWhirlwindSmall = 26323, // SunbirdSmall->SunbirdSmall (enrage)
            GreatWhirlwindLarge = 26325, // SunbirdLarge->SunbirdLarge (enrage)
            FlamesOfUndeath = 26326, // Boss->Boss, no cast - aoe when small or big birds all die (?)
            JointPyre = 26329, // Sparkfledged->Sparkfledged, no cast - aoe when big birds die too close to each other (?)
            FireglideSweep = 26336, // SunbirdLarge->SunbirdLarge (charges)
            FireglideSweepAOE = 26337, // SunbirdLarge->targets, no cast (charge aoe)
            DeadRebirth = 26340, // Boss->Boss
            AshenEye = 26342, // Sparkfledged->Sparkfledged, eye cone
            FountainOfFire = 26343, // Boss->Boss
            SunsPinion = 26346, // Boss->Boss
            Fireglide = 26348, // Sunshadow->target, no cast (charge aoe)
            DeathToll = 26349, // Boss->Boss
            LifesAgonies = 26350, // Boss->Boss
            FirestormsOfAsphodelos = 26352, // Boss->Boss
            FlamesOfAsphodelos = 26353, // Boss->Boss
            FlamesOfAsphodelosAOE1 = 26354, // Helper->Helper, first cone, 7sec cast
            FlamesOfAsphodelosAOE2 = 26355, // Helper->Helper, first cone, 8sec cast
            FlamesOfAsphodelosAOE3 = 26356, // Helper->Helper, first cone, 9sec cast
            StormsOfAsphodelos = 26357, // Boss->Boss
            WindsOfAsphodelos = 26358, // Helper->targets, no cast, some damage during storms
            BeaconsOfAsphodelos = 26359, // Helper->targets, no cast, some damage during storms
            DarkblazeTwister = 26360, // Boss->Boss
            DarkTwister = 26361, // Twister->Twister, knockback
            BurningTwister = 26362, // Twister->Twister, aoe
            TrailOfCondemnationCenter = 26363, // Boss->Boss (central aoe variant - spread)
            TrailOfCondemnationSides = 26364, // Boss->Boss (side aoe variant - stack in pairs)
            TrailOfCondemnationAOE = 26365, // Helper->Helper (actual aoe that hits those who fail the mechanic)
            FlareOfCondemnation = 26366, // Helper->target, no cast, hit and apply fire resist debuff (spread variant)
            SparksOfCondemnation = 26367, // Helper->target, no cast, hit and apply fire resist debuff (pairs variant)
            HeatOfCondemnation = 26368, // Boss->Boss
            HeatOfCondemnationAOE = 26369, // Helper->target, no cast, hit and apply fire resist debuff
            RightCinderwing = 26370, // Boss->Boss
            LeftCinderwing = 26371, // Boss->Boss
            SearingBreeze = 26372, // Boss->Boss
            SearingBreezeAOE = 36373, // Helper->Helper
            ScorchedExaltation = 26374, // Boss->Boss
            DevouringBrandAOE = 28035, // Helper->Helper (one in the center), 20sec cast
        };

        public enum SID : uint
        {
            DeathsToll = 2762,
        }

        public enum TetherID : uint
        {
            LargeBirdFar = 1,
            LargeBirdClose = 57,
            HeatOfCondemnation = 89,
            BurningTwister = 167,
            DarkTwister = 168,
        }

        // state related to heat of condemnation tethers
        private class HeatOfCondemnation
        {
            public bool Active = false;
            private ulong _inAnyAOE = 0; // players hit by aoe, excluding selves
            private ulong _inMyAOE = 0; // players hit by my aoe, if pc is tethered

            private static float _aoeRange = 6;

            public void Update(P3S self)
            {
                _inAnyAOE = _inMyAOE = 0;
                if (!Active)
                    return;

                foreach ((int i, var player) in self.IterateRaidMembers().Where(indexPlayer => IsTethered(indexPlayer.Item2)))
                {
                    var inPlayerAOE = self.FindRaidMembersInRange(i, _aoeRange);
                    _inAnyAOE |= inPlayerAOE;
                    if (i == self.PlayerSlot)
                        _inMyAOE = inPlayerAOE;
                }
            }

            public void DrawArenaForeground(P3S self)
            {
                var pc = self.RaidMember(self.PlayerSlot);
                if (!Active || self._boss == null || pc == null)
                    return;

                // currently helper always shows tethered targets with circles, and if pc himself is tethered, also untethered players (so that tanks can see that they don't hit anyone)
                // TODO: consider having different logic depending on whether player is tank
                bool showUntethered = IsTethered(pc);
                foreach ((int i, var player) in self.IterateRaidMembers())
                {
                    if (IsTethered(player))
                    {
                        self.Arena.AddLine(player.Position, self._boss.Position, self.Arena.ColorDanger);
                        self.Arena.Actor(player.Position, player.Rotation, self.Arena.ColorDanger);
                        self.Arena.AddCircle(player.Position, _aoeRange, self.Arena.ColorDanger);
                    }
                    else if (showUntethered)
                    {
                        self.Arena.Actor(player.Position, player.Rotation, BitVector.IsVector64BitSet(_inAnyAOE, i) ? self.Arena.ColorPlayerInteresting : self.Arena.ColorPlayerGeneric);
                    }
                }
            }

            public void AddHints(P3S self, StringBuilder res)
            {
                var pc = self.RaidMember(self.PlayerSlot);
                if (!Active || pc == null)
                    return;

                if (IsTethered(pc) && _inMyAOE != 0)
                {
                    res.Append("GTFO from raid! ");
                }
                else if (!IsTethered(pc) && BitVector.IsVector64BitSet(_inAnyAOE, self.PlayerSlot))
                {
                    res.Append("GTFO from aoe! ");
                }
            }

            private bool IsTethered(WorldState.Actor player)
            {
                return player.Tether.ID == (uint)TetherID.HeatOfCondemnation;
            }
        }

        // state related to cinderwing
        private class Cinderwing
        {
            public enum State { None, Left, Right }
            public State CurState = State.None;

            public void DrawArenaBackground(P3S self)
            {
                if (CurState == State.None || self._boss == null)
                    return;

                var offFront = 50 * self._boss.DirectionFront();
                var offSide = CurState == State.Right ? new Vector3(-offFront.Z, 0, offFront.X) : new Vector3(offFront.Z, 0, -offFront.X);
                var pFront = self._boss.Position + offFront;
                var pBack = self._boss.Position - offFront;

                self.Arena.ZoneQuad(pFront, pFront + offSide, pBack + offSide, pBack, self.Arena.ColorAOE);
            }

            public void AddHints(P3S self, StringBuilder res)
            {
                if (CurState == State.None || self._boss == null)
                    return;

                var pc = self.RaidMember(self.PlayerSlot);
                if (pc == null)
                    return;

                var pcOffset = pc.Position - self._boss.Position;
                var bossDir = self._boss.DirectionFront();
                var bossSide = CurState == State.Right ? new Vector3(-bossDir.Z, 0, bossDir.X) : new Vector3(bossDir.Z, 0, -bossDir.X);
                if (Vector3.Dot(pcOffset, bossSide) >= 0)
                {
                    res.Append("GTFO from wing! ");
                }
            }
        }

        // state related to devouring brand mechanic
        private class DevouringBrand
        {
            public bool Active = false;

            private static float _halfWidth = 5;

            public void DrawArenaBackground(P3S self)
            {
                if (!Active)
                    return;

                var off1 = new Vector3(_halfWidth, 0, self.Arena.WorldHalfSize);
                var off2 = new Vector3(self.Arena.WorldHalfSize, 0, _halfWidth);
                var off3 = new Vector3(-_halfWidth, 0, _halfWidth);
                self.Arena.ZoneRect(self.Arena.WorldCenter - off1, self.Arena.WorldCenter + off1, self.Arena.ColorAOE);
                self.Arena.ZoneRect(self.Arena.WorldCenter - off2, self.Arena.WorldCenter + off3, self.Arena.ColorAOE);
                self.Arena.ZoneRect(self.Arena.WorldCenter - off3, self.Arena.WorldCenter + off2, self.Arena.ColorAOE);
            }

            public void AddHints(P3S self, StringBuilder res)
            {
                var pc = self.RaidMember(self.PlayerSlot);
                if (!Active || pc == null)
                    return;

                var pcOffset = pc.Position - self.Arena.WorldCenter;
                if (MathF.Abs(pcOffset.X) <= _halfWidth || MathF.Abs(pcOffset.Z) <= _halfWidth)
                {
                    res.Append("GTFO from brand! ");
                }
            }
        }

        // state related to trail of condemnation mechanic
        private class TrailOfCondemnation
        {
            public enum State { None, Center, Sides }
            public State CurState = State.None;
            private ulong _playersInMyAOE = 0;

            private static float _halfWidth = 7.5f;
            private static float _sidesOffset = 12.5f;
            private static float _aoeRadius = 6;

            public void Update(P3S self)
            {
                _playersInMyAOE = CurState != State.None ? self.FindRaidMembersInRange(self.PlayerSlot, _aoeRadius) : 0;
            }

            public void DrawArenaBackground(P3S self)
            {
                if (CurState == State.None || self._boss == null || self._boss.Position == self.Arena.WorldCenter)
                    return;

                var dir = Vector3.Normalize(self.Arena.WorldCenter - self._boss.Position);
                var normal = new Vector3(-dir.Z, 0, dir.X);
                if (CurState == State.Center)
                {
                    DrawZone(self, self._boss.Position, dir, normal);
                }
                else
                {
                    DrawZone(self, self._boss.Position + _sidesOffset * normal, dir, normal);
                    DrawZone(self, self._boss.Position - _sidesOffset * normal, dir, normal);
                }
            }

            public void DrawArenaForeground(P3S self)
            {
                if (CurState == State.None)
                    return;

                // draw all raid members, to simplify positioning
                foreach ((int i, var player) in self.IterateRaidMembers())
                {
                    self.Arena.Actor(player.Position, player.Rotation, BitVector.IsVector64BitSet(_playersInMyAOE, i) ? self.Arena.ColorPlayerInteresting : self.Arena.ColorPlayerGeneric);
                }

                // draw circle around pc
                var pc = self.RaidMember(self.PlayerSlot);
                if (pc != null)
                {
                    self.Arena.AddCircle(pc.Position, _aoeRadius, self.Arena.ColorDanger);
                }
            }

            public void AddHints(P3S self, StringBuilder res)
            {
                var pc = self.RaidMember(self.PlayerSlot);
                if (CurState == State.None || pc == null || self._boss == null || self._boss.Position == self.Arena.WorldCenter)
                    return;

                var dir = Vector3.Normalize(self.Arena.WorldCenter - self._boss.Position);
                var normal = new Vector3(-dir.Z, 0, dir.X);
                if (CurState == State.Center)
                {
                    if (InZone(pc.Position, self._boss.Position, normal))
                    {
                        res.Append("GTFO from aoe! ");
                    }
                    if (_playersInMyAOE != 0)
                    {
                        res.Append("Spread! ");
                    }
                }
                else
                {
                    if (InZone(pc.Position, self._boss.Position + _sidesOffset * normal, normal) ||
                        InZone(pc.Position, self._boss.Position - _sidesOffset * normal, normal))
                    {
                        res.Append("GTFO from aoe! ");
                    }
                    // note: it seems to always target tanks & healers, so consider detecting incorrect pairings
                    if (BitOperations.PopCount(_playersInMyAOE) != 1)
                    {
                        res.Append("Stack in pairs! ");
                    }
                }
            }

            private void DrawZone(P3S self, Vector3 origin, Vector3 dir, Vector3 normal)
            {
                var sideOff = _halfWidth * normal;
                var farOff = 2 * self.Arena.WorldHalfSize * dir;
                var p1 = origin + sideOff;
                var p2 = origin - sideOff;
                self.Arena.ZoneQuad(p1, p1 + farOff, p2 + farOff, p2, self.Arena.ColorAOE);
            }

            private bool InZone(Vector3 pos, Vector3 origin, Vector3 normal)
            {
                return MathF.Abs(Vector3.Dot(pos - origin, normal)) <= _halfWidth;
            }
        }

        // state related to 'single' fireplumes (normal or parts of gloryplume)
        private class FireplumeSingle
        {
            public Vector3? Position = null;

            private static float _radius = 15;

            public void DrawArenaBackground(P3S self)
            {
                if (Position != null)
                {
                    self.Arena.ZoneCircle(Position.Value, _radius, self.Arena.ColorAOE);
                }
            }

            public void AddHints(P3S self, StringBuilder res)
            {
                var pc = self.RaidMember(self.PlayerSlot);
                if (Position == null || pc == null)
                    return;

                if ((pc.Position - Position.Value).LengthSquared() <= _radius * _radius)
                {
                    res.Append("GTFO from plume! ");
                }
            }
        }

        // state related to 'multi' fireplumes (normal or parts of gloryplume)
        // active+pending: 0 = inactive, 1 = only center left, 2 = center and one of the orbs from last pair, ..., 9 = center and all four pairs left
        private class FireplumeMulti
        {
            private float _startingDirection;
            private int _numActiveCasts = 0;
            private int _numPendingOrbs = 0;

            private static float _pairOffset = 15;
            private static float _radius = 10;

            public void CastStart(Vector3 offsetFromCenter)
            {
                if (_numActiveCasts++ == 0)
                {
                    // start new sequence
                    _numPendingOrbs = 9;
                    _startingDirection = MathF.Atan2(offsetFromCenter.Z, offsetFromCenter.X);
                }
                --_numPendingOrbs;
            }

            public void CastEnd()
            {
                if (--_numActiveCasts == 0)
                {
                    // last cast ended, reset - even if we didn't execute all pending casts (e.g. if we wiped)
                    _numPendingOrbs = 0;
                }
            }

            public void DrawArenaBackground(P3S self)
            {
                if (_numActiveCasts == 0)
                    return; // inactive

                int numTotal = _numActiveCasts + _numPendingOrbs;
                if (numTotal < 9) // don't draw center aoe before first explosion, it's confusing - but start drawing it immediately after first explosion, to simplify positioning
                    self.Arena.ZoneCircle(self.Arena.WorldCenter, _radius, _numPendingOrbs == 0 ? self.Arena.ColorDanger : self.Arena.ColorAOE);

                // don't draw more than two next pairs
                if (numTotal >= 3 && numTotal <= 5)
                    DrawPair(self, _startingDirection - MathF.PI / 4, _numPendingOrbs < 2);
                if (numTotal >= 5 && numTotal <= 7)
                    DrawPair(self, _startingDirection + MathF.PI / 2, _numPendingOrbs < 4);
                if (numTotal >= 7)
                    DrawPair(self, _startingDirection + MathF.PI / 4, _numPendingOrbs < 6);
                if (numTotal >= 9)
                    DrawPair(self, _startingDirection, _numPendingOrbs < 8);
            }

            public void AddHints(P3S self, StringBuilder res)
            {
                if (_numActiveCasts == 0)
                    return;

                var pc = self.RaidMember(self.PlayerSlot);
                if (pc == null)
                    return;

                if (InAOE(self, pc))
                {
                    res.Append("GTFO from plume! ");
                }
            }

            private void DrawPair(P3S self, float direction, bool active)
            {
                var offset = _pairOffset * new Vector3(MathF.Cos(direction), 0, MathF.Sin(direction));
                self.Arena.ZoneCircle(self.Arena.WorldCenter + offset, _radius, active ? self.Arena.ColorDanger : self.Arena.ColorAOE);
                self.Arena.ZoneCircle(self.Arena.WorldCenter - offset, _radius, active ? self.Arena.ColorDanger : self.Arena.ColorAOE);
            }

            private bool InAOE(P3S self, WorldState.Actor actor)
            {
                if ((self.Arena.WorldCenter - actor.Position).LengthSquared() <= _radius * _radius)
                    return true;

                int numTotal = _numActiveCasts + _numPendingOrbs;
                if (numTotal >= 3 && InPair(self, _startingDirection - MathF.PI / 4, actor))
                    return true;
                if (numTotal >= 5 && InPair(self, _startingDirection + MathF.PI / 2, actor))
                    return true;
                if (numTotal >= 7 && InPair(self, _startingDirection + MathF.PI / 4, actor))
                    return true;
                if (numTotal >= 9 && InPair(self, _startingDirection, actor))
                    return true;
                return false;
            }

            private bool InPair(P3S self, float direction, WorldState.Actor actor)
            {
                var offset = _pairOffset * new Vector3(MathF.Cos(direction), 0, MathF.Sin(direction));
                if ((self.Arena.WorldCenter + offset - actor.Position).LengthSquared() <= _radius * _radius)
                    return true;
                if ((self.Arena.WorldCenter - offset - actor.Position).LengthSquared() <= _radius * _radius)
                    return true;
                return false;
            }
        }

        // state related to ashplumes (normal or parts of gloryplume)
        private class Ashplume
        {
            public enum State { None, Stack, Spread }
            public State CurState = State.None;
            private ulong _playersInMyAOE = 0;

            private static float _stackRadius = 8;
            private static float _spreadRadius = 6;

            public void Update(P3S self)
            {
                _playersInMyAOE = CurState != State.None ? self.FindRaidMembersInRange(self.PlayerSlot, CurState == State.Stack ? _stackRadius : _spreadRadius) : 0;
            }

            public void DrawArenaForeground(P3S self)
            {
                if (CurState == State.None)
                    return;

                // draw all raid members, to simplify positioning
                foreach ((int i, var player) in self.IterateRaidMembers())
                {
                    self.Arena.Actor(player.Position, player.Rotation, BitVector.IsVector64BitSet(_playersInMyAOE, i) ? self.Arena.ColorPlayerInteresting : self.Arena.ColorPlayerGeneric);
                }

                // draw circle around pc
                var pc = self.RaidMember(self.PlayerSlot);
                if (pc != null)
                {
                    self.Arena.AddCircle(pc.Position, CurState == State.Stack ? _stackRadius : _spreadRadius, self.Arena.ColorDanger);
                }
            }

            public void AddHints(P3S self, StringBuilder res)
            {
                switch (CurState)
                {
                    case State.Stack:
                        // note: it seems to always target tanks & healers, so consider detecting incorrect pairings
                        if (BitOperations.PopCount(_playersInMyAOE) != 3)
                        {
                            res.Append("Stack in fours! ");
                        }
                        break;
                    case State.Spread:
                        if (_playersInMyAOE != 0)
                        {
                            res.Append("Spread! ");
                        }
                        break;
                }
            }
        }

        // state related to brightened fire mechanic
        // this helper relies on waymarks 1-4, and assumes they don't change during fight - this is of course quite an assumption, but whatever...
        private class BrightenedFire
        {
            private List<(WorldState.Actor, float)> _closestAdds = new(); // add + distance to mark

            private static float _aoeRange = 7;

            public void Reset()
            {
                _closestAdds.Clear();
            }

            public void SetPlayerOrder(P3S self, uint order)
            {
                _closestAdds.Clear();

                var markID = (WorldState.Waymark)((uint)WorldState.Waymark.N1 + order % 4);
                var markPos = self.WorldState.GetWaymark(markID);
                if (markPos == null)
                    return;

                foreach (var add in self._darkenedFires)
                    _closestAdds.Add(new(add, (add.Position - markPos.Value).LengthSquared()));
                _closestAdds.Sort((l, r) => l.Item2.CompareTo(r.Item2));
            }

            public void DrawArenaForeground(P3S self)
            {
                if (_closestAdds.Count == 0)
                    return;

                // draw two closest adds
                foreach ((var add, _) in _closestAdds.Take(2))
                {
                    self.Arena.Actor(add.Position, add.Rotation, self.Arena.ColorDanger);
                }

                // draw range circle
                var pc = self.RaidMember(self.PlayerSlot);
                if (pc != null)
                {
                    self.Arena.AddCircle(pc.Position, _aoeRange, self.Arena.ColorDanger);
                }
            }

            public void AddHints(P3S self, StringBuilder res)
            {
                var pc = self.RaidMember(self.PlayerSlot);
                if (_closestAdds.Count == 0 || pc == null)
                    return;

                foreach ((var add, _) in _closestAdds.Take(2))
                {
                    if ((pc.Position - add.Position).LengthSquared() >= _aoeRange * _aoeRange)
                    {
                        res.Append("Get closer to add! ");
                        return;
                    }
                }
            }
        }

        // bird distance utility
        // when small birds die and large birds appear, they cast 26328, and if it hits any other large bird, they buff
        // when large birds die and sparkfledgeds appear, they cast 26329, and if it hits any other sparkfledged, they wipe the raid or something
        // so we show range helper for dead birds
        private class BirdDistance
        {
            public List<WorldState.Actor>? WatchedBirds = null;
            private bool _active;
            private ulong _birdsAtRisk = 0; // mask

            private static float _radius = 13;

            public void Update(P3S self)
            {
                // distance utility is active only if pc is tanking at least one bird
                _birdsAtRisk = 0;
                _active = false;
                if (WatchedBirds == null)
                    return;

                for (int i = 0; i < WatchedBirds.Count; ++i)
                {
                    var bird = WatchedBirds[i];
                    if (!bird.IsDead && bird.TargetID == self.WorldState.PlayerActorID)
                    {
                        _active = true;
                        if (WatchedBirds.Where(other => other.IsDead && (bird.Position - other.Position).LengthSquared() <= _radius * _radius).Any())
                        {
                            BitVector.SetVector64Bit(ref _birdsAtRisk, i);
                        }
                    }
                }
            }

            public void DrawArenaForeground(P3S self)
            {
                if (!_active || WatchedBirds == null)
                    return;

                // draw alive birds tanked by PC and circles around dead birds
                for (int i = 0; i < WatchedBirds.Count; ++i)
                {
                    var bird = WatchedBirds[i];
                    if (bird.IsDead)
                    {
                        self.Arena.AddCircle(bird.Position, _radius, self.Arena.ColorDanger);
                    }
                    else if (bird.TargetID == self.WorldState.PlayerActorID)
                    {
                        self.Arena.Actor(bird.Position, bird.Rotation, BitVector.IsVector64BitSet(_birdsAtRisk, i) ? self.Arena.ColorEnemy : self.Arena.ColorPlayerGeneric);
                    }
                }
            }

            public void AddHints(P3S self, StringBuilder res)
            {
                if (_birdsAtRisk != 0)
                {
                    res.Append("Drag bird away! ");
                }
            }
        }

        // state related to large bird tethers
        private class BirdTether
        {
            public bool Active = false;

            private static float _chargeHalfWidth = 3;

            public void DrawArenaBackground(P3S self)
            {
                if (!Active)
                    return;

                // draw aoe zones for imminent charges (TODO: only if player is standing in one, to reduce visual clutter?)
                foreach ((var from, var to) in ImminentCharges(self))
                {
                    var dir = Vector3.Normalize(to.Position - from.Position);
                    var off = _chargeHalfWidth * new Vector3(-dir.Z, 0, dir.X);
                    self.Arena.ZoneQuad(from.Position + off, to.Position + off, to.Position - off, from.Position - off, self.Arena.ColorAOE);
                }
            }

            public void DrawArenaForeground(P3S self)
            {
                var pc = self.RaidMember(self.PlayerSlot);
                if (!Active || pc == null || pc.Tether.Target == 0)
                    return; // nothing to do if pc is not tethered to anything

                var pcTetherTarget = self.WorldState.FindActor(pc.Tether.Target);
                if (pcTetherTarget == null)
                    return; // something weird, whatever...

                DrawTether(self, pc, pcTetherTarget, pc.Tether.ID);

                if (pcTetherTarget.Type == WorldState.ActorType.Enemy)
                {
                    // pc is tethered to phoenix, so find another player who is tethered to pc (if any)
                    // note that there should be 0 or 1 such players...
                    foreach (var raidMember in self.RaidMembers)
                    {
                        if (raidMember != null && raidMember.Tether.Target == pc.InstanceID)
                        {
                            DrawTether(self, pc, raidMember, raidMember.Tether.ID);
                        }
                    }
                }
                else
                {
                    // pc is tethered to another player, follow that chain to phoenix
                    var nextTetherTarget = pcTetherTarget.Tether.Target != 0 ? self.WorldState.FindActor(pcTetherTarget.Tether.Target) : null;
                    if (nextTetherTarget != null)
                    {
                        DrawTether(self, pcTetherTarget, nextTetherTarget, pcTetherTarget.Tether.ID);
                    }
                }
            }

            public void AddHints(P3S self, StringBuilder res)
            {
                var pc = self.RaidMember(self.PlayerSlot);
                if (!Active || pc == null)
                    return;

                foreach ((var from, var to) in ImminentCharges(self))
                {
                    if (to == pc)
                        continue;

                    var dir = Vector3.Normalize(to.Position - from.Position);
                    var normal = new Vector3(-dir.Z, 0, dir.X);
                    var pcOffset = Vector3.Dot(pc.Position - from.Position, normal);
                    if (MathF.Abs(pcOffset) <= _chargeHalfWidth)
                    {
                        res.Append("GTFO from charge zone! ");
                        break;
                    }
                }
            }

            // return pairs (bird, player)
            private IEnumerable<(WorldState.Actor, WorldState.Actor)> ImminentCharges(P3S self)
            {
                return self.RaidMembers
                    .Where(player => player != null && player.Tether.Target != 0)
                    .Select(player => (self.WorldState.FindActor(player!.Tether.Target), player!))
                    .Where(fromTo => fromTo.Item1 != null && fromTo.Item1.Type == WorldState.ActorType.Enemy && fromTo.Item1.Position != fromTo.Item2.Position)
                    .Select(fromTo => (fromTo.Item1!, fromTo.Item2));
            }

            private void DrawTether(P3S self, WorldState.Actor source, WorldState.Actor target, uint tetherID)
            {
                self.Arena.AddLine(source.Position, target.Position, (tetherID == (uint)TetherID.LargeBirdFar) ? self.Arena.ColorSafe : self.Arena.ColorDanger);
                self.Arena.Actor(target.Position, target.Rotation, target.Type == WorldState.ActorType.Enemy ? self.Arena.ColorEnemy : self.Arena.ColorPlayerGeneric);
            }
        }

        // state related to sunshadow tethers during fountain of fire mechanics
        private class SunshadowTether
        {
            private static float _chargeHalfWidth = 3;

            public void DrawArenaBackground(P3S self)
            {
                foreach (var bird in self._sunshadows)
                {
                    if (bird.Tether.Target == 0 || bird.Tether.Target == self.WorldState.PlayerActorID)
                        continue;

                    var target = self.WorldState.FindActor(bird.Tether.Target);
                    if (target == null || bird.Position == target.Position)
                        continue;

                    var dir = Vector3.Normalize(target.Position - bird.Position);
                    var front = dir * 50; // charge continues to "infinity"
                    var off = _chargeHalfWidth * new Vector3(-dir.Z, 0, dir.X);
                    self.Arena.ZoneQuad(bird.Position + off, front + off, front - off, bird.Position - off, self.Arena.ColorAOE);
                }
            }

            public void DrawArenaForeground(P3S self)
            {
                var pc = self.RaidMember(self.PlayerSlot);
                var myBird = self._sunshadows.Find(bird => bird.Tether.Target == self.WorldState.PlayerActorID);
                if (pc == null || myBird == null)
                    return;

                self.Arena.AddLine(myBird.Position, pc.Position, self.Arena.ColorSafe);
                self.Arena.Actor(myBird.Position, myBird.Rotation, self.Arena.ColorEnemy);
            }

            public void AddHints(P3S self, StringBuilder res)
            {
                var pc = self.RaidMember(self.PlayerSlot);
                if (pc == null)
                    return;

                foreach (var bird in self._sunshadows)
                {
                    if (bird.Tether.Target == 0 || bird.Tether.Target == self.WorldState.PlayerActorID)
                        continue;

                    var target = self.WorldState.FindActor(bird.Tether.Target);
                    if (target == null || bird.Position == target.Position)
                        continue;

                    var dir = Vector3.Normalize(target.Position - bird.Position);
                    var normal = new Vector3(-dir.Z, 0, dir.X);
                    var pcOffset = Vector3.Dot(pc.Position - bird.Position, normal);
                    if (MathF.Abs(pcOffset) <= _chargeHalfWidth)
                    {
                        res.Append("GTFO from charge zone! ");
                        break;
                    }
                }
            }
        }

        // state related to flames of asphodelos mechanic (note that it is activated and deactivated based on helper casts rather than states due to tricky overlaps)
        private class FlamesOfAsphodelos
        {
            public float?[] Directions = new float?[3];

            public void Reset()
            {
                for (int i = 0; i < Directions.Length; ++i)
                    Directions[i] = null;
            }

            public void DrawArenaBackground(P3S self)
            {
                DrawZone(self, Directions[0] != null ? Directions[0] : Directions[2]);
                DrawZone(self, Directions[1]);
            }

            public void AddHints(P3S self, StringBuilder res)
            {
                var pc = self.RaidMember(self.PlayerSlot);
                if (pc == null)
                    return;

                if (InAOE(self, Directions[1], pc.Position) || InAOE(self, Directions[0] != null ? Directions[0] : Directions[2], pc.Position))
                {
                    res.Append("GTFO from cone! ");
                }
            }

            private void DrawZone(P3S self, float? dir)
            {
                if (dir != null)
                {
                    self.Arena.ZoneIsoscelesTri(self.Arena.WorldCenter, dir.Value, MathF.PI / 6, 50, self.Arena.ColorAOE);
                    self.Arena.ZoneIsoscelesTri(self.Arena.WorldCenter, dir.Value + MathF.PI, MathF.PI / 6, 50, self.Arena.ColorAOE);
                }
            }

            private bool InAOE(P3S self, float? dir, Vector3 pos)
            {
                if (dir == null)
                    return false;

                var toPos = Vector3.Normalize(pos - self.Arena.WorldCenter);
                var toDir = new Vector3(MathF.Sin(dir.Value), 0, MathF.Cos(dir.Value));
                return MathF.Abs(Vector3.Dot(toPos, toDir)) >= MathF.Cos(MathF.PI / 6);
            }
        }

        // state related to storms of asphodelos mechanics
        private class StormsOfAsphodelos
        {
            public bool Active = false;
            private List<int> _bossTargets = new();
            private List<int> _twisterTargets = new();
            private List<int> _tetherTargets = new();
            private ulong _failingPlayers = 0;
            private bool _invulNeeded = false;

            private static float _coneHalfAngle = MathF.PI / 12; // not sure about this!!!
            private static float _beaconRadius = 6;

            public void Update(P3S self)
            {
                _bossTargets.Clear();
                _twisterTargets.Clear();
                _tetherTargets.Clear();
                _failingPlayers = 0;
                _invulNeeded = false;
                if (!Active || self._boss == null)
                    return;

                // we determine failing players, trying to take two reasonable tactics in account:
                // either two tanks immune and soak everything, or each player is hit by one mechanic
                // for now, we consider tether target to be a "tank"
                int[] aoesPerPlayer = new int[self.RaidMembers.Length];

                foreach ((int i, var player) in self.IterateRaidMembers(true).Where(indexPlayer => indexPlayer.Item2.Tether.Target == self._boss.InstanceID))
                {
                    _tetherTargets.Add(i);

                    ++aoesPerPlayer[i];
                    foreach ((int j, var other) in self.IterateRaidMembersInRange(i, _beaconRadius))
                    {
                        ++aoesPerPlayer[j];
                        BitVector.SetVector64Bit(ref _failingPlayers, j); // standing in other's tether is a fail even if it's the only aoe hitting a player...
                    }
                }

                float cosHalfAngle = MathF.Cos(_coneHalfAngle);
                foreach ((int i, var player) in FindClosest(self, self._boss.Position).Take(3))
                {
                    _bossTargets.Add(i);
                    foreach ((int j, var other) in FindPlayersInWinds(self, self._boss.Position, player, cosHalfAngle))
                    {
                        ++aoesPerPlayer[j];
                    }
                }

                foreach (var twister in self._twisters)
                {
                    (var i, var player) = FindClosest(self, twister.Position).FirstOrDefault();
                    if (player == null)
                    {
                        _twisterTargets.Add(-1);
                        continue;
                    }

                    _twisterTargets.Add(i);
                    foreach ((int j, var other) in FindPlayersInWinds(self, twister.Position, player, cosHalfAngle))
                    {
                        ++aoesPerPlayer[j];
                    }
                }

                for (int i = 0; i < aoesPerPlayer.Length; ++i)
                {
                    if (aoesPerPlayer[i] <= 1)
                        continue; // this player is safe

                    if (!_tetherTargets.Contains(i))
                    {
                        BitVector.SetVector64Bit(ref _failingPlayers, i);
                    }
                    else if (i == self.PlayerSlot)
                    {
                        _invulNeeded = true;
                    }
                }
            }

            public void DrawArenaBackground(P3S self)
            {
                if (!Active || self._boss == null)
                    return;

                foreach (int i in _bossTargets)
                {
                    var player = self.RaidMembers[i];
                    if (player == null || player.Position == self._boss.Position)
                        continue;

                    var offset = player.Position - self._boss.Position;
                    float phi = MathF.Atan2(offset.X, offset.Z);
                    self.Arena.ZoneCone(self._boss.Position, 0, 50, phi - _coneHalfAngle, phi + _coneHalfAngle, self.Arena.ColorAOE);
                }

                foreach ((var twister, int i) in self._twisters.Zip(_twisterTargets))
                {
                    var player = self.RaidMember(i); // not sure if twister could really have invalid target, but let's be safe...
                    if (player == null || player.Position == twister.Position)
                        continue;

                    var offset = player.Position - twister.Position;
                    float phi = MathF.Atan2(offset.X, offset.Z);
                    self.Arena.ZoneCone(twister.Position, 0, 50, phi - _coneHalfAngle, phi + _coneHalfAngle, self.Arena.ColorAOE);
                }

                foreach (int i in _tetherTargets)
                {
                    var player = self.RaidMembers[i];
                    if (player == null)
                        continue;

                    self.Arena.ZoneCircle(player.Position, _beaconRadius, self.Arena.ColorAOE);
                }
            }

            public void DrawArenaForeground(P3S self)
            {
                if (!Active)
                    return;

                foreach ((int i, var player) in self.IterateRaidMembers())
                {
                    bool active = _tetherTargets.Contains(i) || _bossTargets.Contains(i) || _twisterTargets.Contains(i);
                    bool failing = BitVector.IsVector64BitSet(_failingPlayers, i);
                    self.Arena.Actor(player.Position, player.Rotation, active ? self.Arena.ColorDanger : (failing ? self.Arena.ColorPlayerInteresting : self.Arena.ColorPlayerGeneric));
                }
            }

            public void AddHints(P3S self, StringBuilder res)
            {
                // think of better hints here...
                if (_invulNeeded)
                {
                    res.Append("Press invul! ");
                }
                if (_failingPlayers != 0)
                {
                    res.Append("Storms failing! ");
                }
            }

            private IEnumerable<(int, WorldState.Actor)> FindClosest(P3S self, Vector3 position)
            {
                return self.IterateRaidMembers()
                    .Select(indexPlayer => (indexPlayer.Item1, indexPlayer.Item2, (indexPlayer.Item2.Position - position).LengthSquared()))
                    .OrderBy(indexPlayerDist => indexPlayerDist.Item3)
                    .Select(indexPlayerDist => (indexPlayerDist.Item1, indexPlayerDist.Item2));
            }

            private IEnumerable<(int, WorldState.Actor)> FindPlayersInWinds(P3S self, Vector3 origin, WorldState.Actor target, float cosHalfAngle)
            {
                var dir = Vector3.Normalize(target.Position - origin);
                foreach ((int i, var player) in self.IterateRaidMembers())
                {
                    var playerDir = Vector3.Normalize(player.Position - origin);
                    if (Vector3.Dot(dir, playerDir) >= cosHalfAngle)
                        yield return (i, player);
                }
            }
        }

        // state related to darkblaze twister mechanics
        private class DarkblazeTwister
        {
            public enum State { None, Knockback, AOE }
            public State CurState = State.None;
            private Vector3 _testPos = new();

            private static float _knockbackRange = 17;
            private static float _aoeInnerRadius = 7; // not sure about this...
            private static float _aoeOuterRadius = 20;

            public void Update(P3S self)
            {
                _testPos = new();
                var pc = self.RaidMember(self.PlayerSlot);
                if (CurState == State.None || pc == null)
                    return;

                _testPos = pc.Position;
                if (CurState != State.Knockback)
                    return;

                var twister = self._twisters.Find(twister => twister.CastInfo != null && twister.CastInfo.ActionID == (uint)AID.DarkTwister);
                if (twister == null || pc.Position == twister.Position)
                    return;

                var dir = Vector3.Normalize(pc.Position - twister.Position);
                _testPos += _knockbackRange * dir;
            }

            public void DrawArenaBackground(P3S self)
            {
                if (CurState == State.None)
                    return;

                foreach (var twister in self._twisters.Where(twister => twister.CastInfo != null && twister.CastInfo.ActionID == (uint)AID.BurningTwister))
                {
                    self.Arena.ZoneCone(twister.Position, _aoeInnerRadius, _aoeOuterRadius, 0, 2 * MathF.PI, self.Arena.ColorAOE);
                }
            }

            public void DrawArenaForeground(P3S self)
            {
                var pc = self.RaidMember(self.PlayerSlot);
                if (CurState != State.Knockback || pc == null || _testPos == pc.Position)
                    return;

                self.Arena.AddLine(pc.Position, _testPos, self.Arena.ColorDanger);
                self.Arena.Actor(_testPos, pc.Rotation, self.Arena.ColorDanger);
            }

            public void AddHints(P3S self, StringBuilder res)
            {
                if (CurState == State.None)
                    return;

                if (CurState == State.Knockback && (_testPos - self.Arena.WorldCenter).LengthSquared() >= self.Arena.WorldHalfSize * self.Arena.WorldHalfSize)
                {
                    res.Append("About to be knocked back into wall! ");
                }

                foreach (var twister in self._twisters.Where(twister => twister.CastInfo != null && twister.CastInfo.ActionID == (uint)AID.BurningTwister))
                {
                    var distSq = (twister.Position - _testPos).LengthSquared();
                    if (distSq >= _aoeInnerRadius * _aoeInnerRadius && distSq <= _aoeOuterRadius * _aoeOuterRadius)
                    {
                        res.Append("GTFO from aoe! ");
                        break;
                    }
                }
            }
        }

        // state related to fledgling flight & death toll mechanics
        private class FledglingFlight
        {
            public enum State { None, Players, Sparkfledgeds }
            private State _curState = State.None;
            private List<(WorldState.Actor, float)> _sources = new();

            private static float _coneHalfAngle = MathF.PI / 8; // not sure about this

            public void Reset(State newState = State.None)
            {
                _curState = newState;
                _sources.Clear();
            }

            public void AddPlayer(WorldState.Actor? player, uint directionIndex)
            {
                if (_curState != State.Players)
                {
                    Reset(State.Players);
                }

                if (player != null)
                {
                    // 0 -> E, 1 -> W, 2 -> S, 3 -> N
                    float dir = (directionIndex < 2 ? MathF.PI / 2 : 0) + directionIndex * MathF.PI;
                    _sources.Add(new(player, dir));
                }
            }

            public void AddCaster(WorldState.Actor caster)
            {
                if (_curState != State.Sparkfledgeds)
                {
                    Reset(State.Sparkfledgeds);
                }
                _sources.Add(new(caster, caster.Rotation));
            }

            public void RemoveCaster(WorldState.Actor caster)
            {
                int index = _sources.FindIndex(x => x.Item1 == caster);
                if (index < 0)
                    return;

                _sources.RemoveAt(index);
                if (_sources.Count == 0)
                {
                    Reset();
                }
            }

            public void DrawArenaBackground(P3S self)
            {
                foreach ((var source, var dir) in _sources)
                {
                    self.Arena.ZoneIsoscelesTri(source.Position, dir, _coneHalfAngle, 50, self.Arena.ColorAOE);
                }
            }

            public void AddHints(P3S self, StringBuilder res)
            {
                var pc = self.RaidMember(self.PlayerSlot);
                if (pc == null || _sources.Count == 0)
                    return;

                int aoeCount = 0;
                var cosHalfAngle = MathF.Cos(_coneHalfAngle);
                foreach ((var source, var dir) in _sources)
                {
                    var toDir = new Vector3(MathF.Sin(dir), 0, MathF.Cos(dir));
                    var toCur = Vector3.Normalize(pc.Position - source.Position);
                    if (Vector3.Dot(toDir, toCur) >= cosHalfAngle)
                    {
                        ++aoeCount;
                    }
                }

                int deathTollStacks = pc.FindStatus((uint)SID.DeathsToll)?.StackCount ?? 0;
                if (aoeCount < deathTollStacks)
                {
                    res.Append($"Enter more aoes ({aoeCount}/{deathTollStacks})! ");
                }
                else if (aoeCount > deathTollStacks)
                {
                    res.Append($"GTFO from eyes ({aoeCount}/{deathTollStacks})! ");
                }
            }
        }

        private WorldState.Actor? _boss;
        private List<WorldState.Actor> _darkenedFires = new();
        private List<WorldState.Actor> _birdsSmall = new();
        private List<WorldState.Actor> _birdsLarge = new();
        private List<WorldState.Actor> _sunshadows = new();
        private List<WorldState.Actor> _twisters = new();
        private HeatOfCondemnation _heatOfCondemnation = new();
        private Cinderwing _cinderwing = new();
        private DevouringBrand _devouringBrand = new();
        private TrailOfCondemnation _trailOfCondemnation = new();
        private FireplumeSingle _fireplumeSingle = new();
        private FireplumeMulti _fireplumeMulti = new();
        private Ashplume _ashplume = new();
        private BrightenedFire _brightenedFire = new();
        private BirdDistance _birdDistance = new();
        private BirdTether _birdTether = new();
        private SunshadowTether _sunshadowTether = new();
        private FlamesOfAsphodelos _flamesOfAsphodelos = new();
        private StormsOfAsphodelos _stormsOfAsphodelos = new();
        private DarkblazeTwister _darkblazeTwister = new();
        private FledglingFlight _fledglingFlight = new();

        public P3S(WorldState ws)
            : base(ws, 8)
        {
            WorldState.ActorCastStarted += ActorCastStarted;
            WorldState.ActorCastFinished += ActorCastFinished;
            WorldState.EventIcon += EventIcon;
            WorldState.EventCast += EventCast;

            Arena.IsCircle = true;

            StateMachine.State? s;
            s = BuildScorchedExaltationState(ref InitialState, 8);
            s = BuildHeatOfCondemnationState(ref s.Next, 3.2f);

            s = BuildExperimentalFireplumeState(ref s.Next, 6.2f); // pos-start
            s.EndHint |= StateMachine.StateHint.GroupWithNext;
            s = BuildCinderwingState(ref s.Next, 5.7f);
            s.EndHint |= StateMachine.StateHint.PositioningEnd;

            s = BuildDarkenedFireState(ref s.Next, 8.2f);
            s = BuildHeatOfCondemnationState(ref s.Next, 6.6f);
            s = BuildScorchedExaltationState(ref s.Next, 3.2f);
            s = BuildDevouringBrandState(ref s.Next, 7.2f);
            s = BuildHeatOfCondemnationState(ref s.Next, 3);
            s = BuildExperimentalFireplumeState(ref s.Next, 3.2f); // pos-start

            s = CommonStates.Timeout(ref s.Next, 4);
            s.EndHint |= StateMachine.StateHint.DowntimeStart; // flies away (TODO: trigger by becoming non-targetable)
            s = BuildTrailOfCondemnationState(ref s.Next, 4);
            s = BuildSmallBirdsState(ref s.Next, 6);
            s = BuildLargeBirdsState(ref s.Next, 3.4f);
            s = CommonStates.Timeout(ref s.Next, 5, "Boss reappears"); // TODO: trigger by becoming targetable
            s.EndHint |= StateMachine.StateHint.PositioningEnd | StateMachine.StateHint.DowntimeEnd; // boss reappears

            s = CommonStates.Cast(ref s.Next, () => _boss, AID.DeadRebirth, 9.4f, 10, "DeadRebirth");
            s.EndHint |= StateMachine.StateHint.Raidwide;
            s = BuildHeatOfCondemnationState(ref s.Next, 9.2f);
            s = BuildFledglingFlightState(ref s.Next, 8.2f);
            s = BuildExperimentalGloryplumeMultiState(ref s.Next, 14.3f);
            s = BuildFountainOfFireState(ref s.Next, 12.2f);
            s = BuildScorchedExaltationState(ref s.Next, 5);
            s = BuildScorchedExaltationState(ref s.Next, 2);
            s = BuildHeatOfCondemnationState(ref s.Next, 5);
            s = CommonStates.Cast(ref s.Next, () => _boss, AID.FirestormsOfAsphodelos, 9.6f, 5, "Firestorm");
            s.EndHint |= StateMachine.StateHint.Raidwide;

            s = BuildFlamesOfAsphodelosState(ref s.Next, 3.2f);
            s = BuildExperimentalAshplumeCastState(ref s.Next, 2);
            s = BuildExperimentalAshplumeResolveState(ref s.Next, 6);

            s = BuildFlamesOfAsphodelosState(ref s.Next, 2);
            s = BuildStormsOfAsphodelosState(ref s.Next, 10);

            s = BuildDarkblazeTwisterState(ref s.Next, 2);

            s = BuildScorchedExaltationState(ref s.Next, 2);
            s = BuildDeathTollState(ref s.Next, 7.2f);
            s = BuildExperimentalGloryplumeSingleState(ref s.Next, 7);
            s = BuildTrailOfCondemnationState(ref s.Next, 7);

            // brand+fireplume+breeze+wing -> 2x aoe -> enrage
            s = CommonStates.Simple(ref s.Next, 2, "?????");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                WorldState.ActorCastStarted -= ActorCastStarted;
                WorldState.ActorCastFinished -= ActorCastFinished;
                WorldState.EventIcon -= EventIcon;
                WorldState.EventCast -= EventCast;
            }
            base.Dispose(disposing);
        }

        public override void Update()
        {
            base.Update();
            _heatOfCondemnation.Update(this);
            _trailOfCondemnation.Update(this);
            _ashplume.Update(this);
            _birdDistance.Update(this);
            _stormsOfAsphodelos.Update(this);
            _darkblazeTwister.Update(this);
        }

        protected override void DrawHeader()
        {
            var hints = new StringBuilder();
            _heatOfCondemnation.AddHints(this, hints);
            _cinderwing.AddHints(this, hints);
            _devouringBrand.AddHints(this, hints);
            _trailOfCondemnation.AddHints(this, hints);
            _fireplumeSingle.AddHints(this, hints);
            _fireplumeMulti.AddHints(this, hints);
            _ashplume.AddHints(this, hints);
            _brightenedFire.AddHints(this, hints);
            _birdDistance.AddHints(this, hints);
            _birdTether.AddHints(this, hints);
            _sunshadowTether.AddHints(this, hints);
            _flamesOfAsphodelos.AddHints(this, hints);
            _stormsOfAsphodelos.AddHints(this, hints);
            _darkblazeTwister.AddHints(this, hints);
            _fledglingFlight.AddHints(this, hints);
            ImGui.TextColored(ImGui.ColorConvertU32ToFloat4(0xff00ffff), hints.ToString());
        }

        protected override void DrawArena()
        {
            _cinderwing.DrawArenaBackground(this);
            _devouringBrand.DrawArenaBackground(this);
            _trailOfCondemnation.DrawArenaBackground(this);
            _fireplumeSingle.DrawArenaBackground(this);
            _fireplumeMulti.DrawArenaBackground(this);
            _birdTether.DrawArenaBackground(this);
            _sunshadowTether.DrawArenaBackground(this);
            _flamesOfAsphodelos.DrawArenaBackground(this);
            _stormsOfAsphodelos.DrawArenaBackground(this);
            _darkblazeTwister.DrawArenaBackground(this);
            _fledglingFlight.DrawArenaBackground(this);

            Arena.Border();

            if (_boss != null)
                Arena.Actor(_boss.Position, _boss.Rotation, Arena.ColorEnemy);

            _heatOfCondemnation.DrawArenaForeground(this);
            _trailOfCondemnation.DrawArenaForeground(this);
            _ashplume.DrawArenaForeground(this);
            _brightenedFire.DrawArenaForeground(this);
            _birdDistance.DrawArenaForeground(this);
            _birdTether.DrawArenaForeground(this);
            _sunshadowTether.DrawArenaForeground(this);
            _stormsOfAsphodelos.DrawArenaForeground(this);
            _darkblazeTwister.DrawArenaForeground(this);

            // draw player
            var pc = RaidMember(PlayerSlot);
            if (pc != null)
                Arena.Actor(pc.Position, pc.Rotation, Arena.ColorPC);
        }

        protected override void NonPlayerCreated(WorldState.Actor actor)
        {
            switch ((OID)actor.OID)
            {
                case OID.Boss:
                    if (_boss != null)
                        Service.Log($"[P3S] Created boss {actor.InstanceID} while another boss {_boss.InstanceID} is still alive");
                    _boss = actor;
                    break;
                case OID.DarkenedFire:
                    _darkenedFires.Add(actor);
                    break;
                case OID.SunbirdSmall:
                    _birdsSmall.Add(actor);
                    break;
                case OID.SunbirdLarge:
                    _birdsLarge.Add(actor);
                    break;
                case OID.Sunshadow:
                    _sunshadows.Add(actor);
                    break;
                case OID.DarkblazeTwister:
                    _twisters.Add(actor);
                    break;
            }
        }

        protected override void NonPlayerDestroyed(WorldState.Actor actor)
        {
            switch ((OID)actor.OID)
            {
                case OID.Boss:
                    if (_boss != actor)
                        Service.Log($"[P3S] Destroying boss {actor.InstanceID} while active boss is different: {_boss?.InstanceID}");
                    else
                        _boss = null;
                    break;
                case OID.DarkenedFire:
                    _darkenedFires.Remove(actor);
                    break;
                case OID.SunbirdSmall:
                    _birdsSmall.Remove(actor);
                    break;
                case OID.SunbirdLarge:
                    _birdsLarge.Remove(actor);
                    break;
                case OID.Sunshadow:
                    _sunshadows.Remove(actor);
                    break;
                case OID.DarkblazeTwister:
                    _twisters.Remove(actor);
                    break;
            }
        }

        protected override void Reset()
        {
            _heatOfCondemnation.Active = false;
            _cinderwing.CurState = Cinderwing.State.None;
            _devouringBrand.Active = false;
            _trailOfCondemnation.CurState = TrailOfCondemnation.State.None;
            _ashplume.CurState = Ashplume.State.None;
            _brightenedFire.Reset();
            _birdDistance.WatchedBirds = null;
            _birdTether.Active = false;
            _flamesOfAsphodelos.Reset();
            _stormsOfAsphodelos.Active = false;
            _darkblazeTwister.CurState = DarkblazeTwister.State.None;
            _fledglingFlight.Reset();
        }

        private StateMachine.State BuildScorchedExaltationState(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, () => _boss, AID.ScorchedExaltation, delay, 5, "AOE");
            s.EndHint |= StateMachine.StateHint.Raidwide;
            return s;
        }

        private StateMachine.State BuildHeatOfCondemnationState(ref StateMachine.State? link, float delay)
        {
            var start = CommonStates.CastStart(ref link, () => _boss, AID.HeatOfCondemnation, delay);
            start.Exit = () => _heatOfCondemnation.Active = true;

            var end = CommonStates.CastEnd(ref start.Next, () => _boss, 6, "Tether");
            end.Exit = () => _heatOfCondemnation.Active = false;
            end.EndHint |= StateMachine.StateHint.Tankbuster;
            // note: actual AOE is about 1s after cast end
            return end;
        }

        // note - positioning state is set at the end, make sure to clear later - this is because this mechanic overlaps with other stuff
        private StateMachine.State BuildExperimentalFireplumeState(ref StateMachine.State? link, float delay)
        {
            // mechanics:
            // 1. single-plume version: immediately after cast end, 1 helper teleports to position and starts casting 26303, which takes 6s
            // 2. multi-plume version: immediately after cast end, 9 helpers teleport to positions and start casting 26305
            //    first pair starts cast almost immediately, then pairs 2-4 and finally central start their cast with 1 sec between them; each cast lasts 2 sec
            // so center (last/only) plume hits around 6s after cast end
            // note that our helpers rely on 233C casts rather than states
            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.ExperimentalFireplumeSingle] = new(null, () => { });
            dispatch[AID.ExperimentalFireplumeMulti] = new(null, () => { });
            var start = CommonStates.CastStart(ref link, () => _boss, dispatch, delay);
            start.EndHint |= StateMachine.StateHint.PositioningStart;

            var end = CommonStates.CastEnd(ref start.Next, () => _boss, 5, "Fireplume");
            return end;
        }

        // note - no positioning flags, since this is part of mechanics that manage it themselves
        // note - since it resolves in a complex way, make sure to add a resolve state!
        private StateMachine.State BuildExperimentalAshplumeCastState(ref StateMachine.State? link, float delay)
        {
            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.ExperimentalAshplumeStack] = new(null, () => _ashplume.CurState = Ashplume.State.Stack);
            dispatch[AID.ExperimentalAshplumeSpread] = new(null, () => _ashplume.CurState = Ashplume.State.Spread);
            var start = CommonStates.CastStart(ref link, () => _boss, dispatch, delay);
            var end = CommonStates.CastEnd(ref start.Next, () => _boss, 5, "Ashplume");
            end.EndHint |= StateMachine.StateHint.GroupWithNext;
            return end;
        }

        // note: automatically clears positioning flag
        private StateMachine.State BuildExperimentalAshplumeResolveState(ref StateMachine.State? link, float delay)
        {
            var resolve = CommonStates.Simple(ref link, delay, "Ashplume resolve");
            resolve.Update = (float timeSinceTransition) => resolve.Done = _ashplume.CurState == Ashplume.State.None; // it will automatically turn off on cast...
            resolve.EndHint |= StateMachine.StateHint.Raidwide | StateMachine.StateHint.PositioningEnd;
            return resolve;
        }

        private StateMachine.State BuildExperimentalGloryplumeMultiState(ref StateMachine.State? link, float delay)
        {
            // first part for this mechanic always seems to be "multi-plume", works just like fireplume
            // 9 helpers teleport to position, first pair almost immediately starts casting 26315s, 1 sec stagger between pairs, 7 sec for each cast
            // ~3 sec after cast ends, boss makes an instant cast that determines stack/spread (26316/26312), ~10 sec after that hits with real AOE (26317/26313)
            // note that our helpers rely on casts rather than states
            var cast = CommonStates.Cast(ref link, () => _boss, AID.ExperimentalGloryplumeMulti, delay, 5, "GloryplumeMulti");
            cast.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;
            var resolve = CommonStates.Simple(ref cast.Next, 13, "Gloryplume resolve");
            resolve.Update = (float timeSinceActivation) => resolve.Done = _ashplume.CurState == Ashplume.State.None;
            resolve.EndHint |= StateMachine.StateHint.Raidwide | StateMachine.StateHint.PositioningEnd;
            return resolve;
        }

        private StateMachine.State BuildExperimentalGloryplumeSingleState(ref StateMachine.State? link, float delay)
        {
            // first part for this mechanic always seems to be "single-plume", works just like fireplume
            // helper teleports to position, almost immediately starts casting 26311, 6 sec for cast
            // ~3 sec after cast ends, boss makes an instant cast that determines stack/spread (26316/26312), ~4 sec after that hits with real AOE (26317/26313)
            // note that our helpers rely on casts rather than states
            var cast = CommonStates.Cast(ref link, () => _boss, AID.ExperimentalGloryplumeSingle, delay, 5, "GloryplumeSingle");
            cast.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;
            var resolve = CommonStates.Simple(ref cast.Next, 7, "Gloryplume resolve");
            resolve.Update = (float timeSinceActivation) => resolve.Done = _ashplume.CurState == Ashplume.State.None;
            resolve.EndHint |= StateMachine.StateHint.Raidwide | StateMachine.StateHint.PositioningEnd;
            return resolve;
        }

        private StateMachine.State BuildCinderwingState(ref StateMachine.State? link, float delay, bool endBrand = false)
        {
            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.RightCinderwing] = new(null, () => _cinderwing.CurState = Cinderwing.State.Right);
            dispatch[AID.LeftCinderwing] = new(null, () => _cinderwing.CurState = Cinderwing.State.Left);
            var start = CommonStates.CastStart(ref link, () => _boss, dispatch, delay);
            if (endBrand)
                start.Exit = () => _devouringBrand.Active = false;

            var end = CommonStates.CastEnd(ref start.Next, () => _boss, 5, "Wing");
            end.Exit = () => _cinderwing.CurState = Cinderwing.State.None;
            return end;
        }

        private StateMachine.State BuildDevouringBrandState(ref StateMachine.State? link, float delay)
        {
            var devouring = CommonStates.Cast(ref link, () => _boss, AID.DevouringBrand, delay, 3, "DevouringBrand");
            devouring.EndHint |= StateMachine.StateHint.GroupWithNext;

            var fireplume = BuildExperimentalFireplumeState(ref devouring.Next, 2); // pos-start
            fireplume.EndHint |= StateMachine.StateHint.GroupWithNext;
            fireplume.Exit = () => _devouringBrand.Active = true;

            var breeze = CommonStates.Cast(ref fireplume.Next, () => _boss, AID.SearingBreeze, 7, 3, "SearingBreeze");
            breeze.EndHint |= StateMachine.StateHint.GroupWithNext;

            var wing = BuildCinderwingState(ref breeze.Next, 3, true);
            wing.EndHint |= StateMachine.StateHint.PositioningEnd;
            return wing;
        }

        private StateMachine.State BuildDarkenedFireState(ref StateMachine.State? link, float delay)
        {
            // TODO: helper for add placement - need to understand exact spawn mechanics for that...
            // 3s after cast ends, adds start casting 26299
            var addsStart = CommonStates.CastStart(ref link, () => _boss, AID.DarkenedFire, delay);
            addsStart.EndHint |= StateMachine.StateHint.PositioningStart;
            var addsEnd = CommonStates.CastEnd(ref addsStart.Next, () => _boss, 6, "DarkenedFire adds");
            addsEnd.EndHint |= StateMachine.StateHint.GroupWithNext;

            var numbers = CommonStates.Cast(ref addsEnd.Next, () => _boss, AID.BrightenedFire, 5, 5, "Numbers"); // numbers appear at the beginning of the cast, at the end he starts shooting 1-8
            numbers.EndHint |= StateMachine.StateHint.GroupWithNext;

            // note: first aoe happens ~0.2sec after cast end, then each next ~1.2sec after previous - consider resetting helper based on cast?..
            var lastAOE = CommonStates.Timeout(ref numbers.Next, 8);
            lastAOE.Exit = () => _brightenedFire.Reset();

            var resolve = CommonStates.Timeout(ref lastAOE.Next, 7, "DarkenedFire resolve");
            resolve.EndHint |= StateMachine.StateHint.PositioningEnd;
            return resolve;
        }

        private StateMachine.State BuildTrailOfCondemnationState(ref StateMachine.State? link, float delay)
        {
            // at this point boss teleports to one of the cardinals
            // parallel to this one of the helpers casts 26365 (actual aoe fire trails)
            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.TrailOfCondemnationCenter] = new(null, () => _trailOfCondemnation.CurState = TrailOfCondemnation.State.Center);
            dispatch[AID.TrailOfCondemnationSides] = new(null, () => _trailOfCondemnation.CurState = TrailOfCondemnation.State.Sides);
            var start = CommonStates.CastStart(ref link, () => _boss, dispatch, delay);
            var end = CommonStates.CastEnd(ref start.Next, () => _boss, 6);
            var resolve = CommonStates.Timeout(ref end.Next, 2, "Trail");
            resolve.Exit = () => _trailOfCondemnation.CurState = TrailOfCondemnation.State.None;
            return resolve;
        }

        // note: expects downtime at enter, clears when birds spawn, reset when birds die
        private StateMachine.State BuildSmallBirdsState(ref StateMachine.State? link, float delay)
        {
            var spawn = CommonStates.Simple(ref link, delay, "Small birds");
            spawn.Update = (float timeSinceTransition) => spawn.Done = _birdsSmall.Count > 0;
            spawn.Exit = () => _birdDistance.WatchedBirds = _birdsSmall;
            spawn.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.DowntimeEnd; // adds become targetable <1sec after spawn

            var enrage = CommonStates.Simple(ref spawn.Next, 25, "Small birds enrage");
            enrage.Update = (float timeSinceTransition) => enrage.Done = _birdsSmall.Find(x => !x.IsDead) == null;
            enrage.Exit = () => _birdDistance.WatchedBirds = null;
            enrage.EndHint |= StateMachine.StateHint.Raidwide | StateMachine.StateHint.DowntimeStart; // raidwide (26326) happens ~3sec after last bird death
            return enrage;
        }

        // note: expects downtime at enter, clears when birds spawn, reset when birds die
        private StateMachine.State BuildLargeBirdsState(ref StateMachine.State? link, float delay)
        {
            var spawn = CommonStates.Simple(ref link, delay, "Large birds");
            spawn.Update = (float timeSinceTransition) => spawn.Done = _birdsLarge.Count > 0;
            spawn.Exit = () => _birdTether.Active = true; // tethers appear ~5s after this and last for ~11s
            spawn.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.DowntimeEnd; // adds become targetable ~1sec after spawn

            var chargesDone = CommonStates.Timeout(ref spawn.Next, 20);
            chargesDone.Exit = () =>
            {
                _birdTether.Active = false;
                _birdDistance.WatchedBirds = _birdsLarge;
            };

            var enrage = CommonStates.Simple(ref chargesDone.Next, 35, "Large birds enrage");
            enrage.Update = (float timeSinceTransition) => enrage.Done = _birdsLarge.Find(x => !x.IsDead) == null;
            enrage.Exit = () => _birdDistance.WatchedBirds = null;
            enrage.EndHint |= StateMachine.StateHint.Raidwide | StateMachine.StateHint.DowntimeStart; // raidwide (26326) happens ~3sec after last bird death
            return enrage;
        }

        private StateMachine.State BuildFledglingFlightState(ref StateMachine.State? link, float delay)
        {
            // mechanic timeline:
            // 0s cast end
            // 2s icons appear (19B - points east, 19C - points west, 19D - points south, 19E - points north)
            // 8s 3540's teleport to players
            // 10s 3540's start casting 26342
            // 14s 3540's finish casting 26342
            // note that helper does relies on icons and cast events rather than states
            return CommonStates.Cast(ref link, () => _boss, AID.FledglingFlight, delay, 3, 8, "Eyes");
        }

        private StateMachine.State BuildDeathTollState(ref StateMachine.State? link, float delay)
        {
            // notes on mechanics:
            // - on 26349 cast end, debuffs with 25sec appear
            // - 12-15sec after 26350 cast starts, eyes finish casting their cones - at this point, there's about 5sec left on debuffs
            // note that helper does relies on icons and cast events rather than states
            var deathtoll = CommonStates.Cast(ref link, () => _boss, AID.DeathToll, 7.2f, 6, "DeathToll");
            deathtoll.EndHint |= StateMachine.StateHint.GroupWithNext;

            var eyes = CommonStates.Cast(ref deathtoll.Next, () => _boss, AID.FledglingFlight, 3.2f, 3, "Eyes");
            eyes.EndHint |= StateMachine.StateHint.GroupWithNext;

            var agonies = CommonStates.Cast(ref eyes.Next, () => _boss, AID.LifesAgonies, 2, 24, "LifeAgonies");
            return agonies;
        }

        private StateMachine.State BuildFountainOfFireState(ref StateMachine.State? link, float delay)
        {
            // note: helper for sunshadow charges relies on tethers rather than states
            // TODO: healer helper - not even sure, mechanic looks so simple...
            var fountain = CommonStates.Cast(ref link, () => _boss, AID.FountainOfFire, delay, 6, "FountainOfFire");
            fountain.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;

            var charges = CommonStates.Cast(ref fountain.Next, () => _boss, AID.SunsPinion, 2, 6, 13, "Charges");
            charges.EndHint |= StateMachine.StateHint.PositioningEnd;
            return charges;
        }

        // note: positioning flag is not cleared in preparation for next mechanic (ashplume or storms)
        private StateMachine.State BuildFlamesOfAsphodelosState(ref StateMachine.State? link, float delay)
        {
            // note: flames helper is activated and deactivated automatically
            var flames = CommonStates.Cast(ref link, () => _boss, AID.FlamesOfAsphodelos, delay, 3, "Cones");
            flames.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;
            return flames;
        }

        private StateMachine.State BuildStormsOfAsphodelosState(ref StateMachine.State? link, float delay)
        {
            var start = CommonStates.CastStart(ref link, () => _boss, delay);
            start.Exit = () => _stormsOfAsphodelos.Active = true;

            var end = CommonStates.CastEnd(ref start.Next, () => _boss, 8, "Storms");
            end.Exit = () => _stormsOfAsphodelos.Active = false;
            end.EndHint |= StateMachine.StateHint.Raidwide | StateMachine.StateHint.Tankbuster;
            return end;
        }

        private StateMachine.State BuildDarkblazeTwisterState(ref StateMachine.State? link, float delay)
        {
            var twister = CommonStates.Cast(ref link, () => _boss, AID.DarkblazeTwister, delay, 4, "Twister");
            twister.Exit = () => _darkblazeTwister.CurState = DarkblazeTwister.State.Knockback;
            twister.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;

            var breeze = CommonStates.Cast(ref twister.Next, () => _boss, AID.SearingBreeze, 4, 3, "SearingBreeze");
            breeze.EndHint |= StateMachine.StateHint.GroupWithNext;

            var ashplume = BuildExperimentalAshplumeCastState(ref breeze.Next, 4);

            var knockback = CommonStates.Simple(ref ashplume.Next, 3, "Knockback");
            knockback.Update = (float timeSinceTransition) => knockback.Done = !_twisters.Any(twister => twister.CastInfo != null && twister.CastInfo.ActionID == (uint)AID.DarkTwister);
            knockback.Exit = () => _darkblazeTwister.CurState = DarkblazeTwister.State.AOE;
            knockback.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.Knockback;

            var aoe = CommonStates.Simple(ref knockback.Next, 2, "AOE");
            aoe.Update = (float timeSinceTransition) => aoe.Done = !_twisters.Any(twister => twister.CastInfo != null);
            aoe.Exit = () => _darkblazeTwister.CurState = DarkblazeTwister.State.None;
            aoe.EndHint |= StateMachine.StateHint.GroupWithNext;

            var resolve = BuildExperimentalAshplumeResolveState(ref aoe.Next, 2);
            return resolve;
        }

        private void ActorCastStarted(object? sender, WorldState.Actor actor)
        {
            switch ((OID)actor.OID)
            {
                case OID.Helper:
                    switch ((AID)actor.CastInfo!.ActionID)
                    {
                        case AID.ExperimentalFireplumeSingleAOE:
                        case AID.ExperimentalGloryplumeSingleAOE:
                            _fireplumeSingle.Position = actor.Position;
                            break;
                        case AID.ExperimentalFireplumeMultiAOE:
                        case AID.ExperimentalGloryplumeMultiAOE:
                            _fireplumeMulti.CastStart(actor.Position - Arena.WorldCenter);
                            break;
                        case AID.FlamesOfAsphodelosAOE1:
                            _flamesOfAsphodelos.Directions[0] = actor.Rotation;
                            break;
                        case AID.FlamesOfAsphodelosAOE2:
                            _flamesOfAsphodelos.Directions[1] = actor.Rotation;
                            break;
                        case AID.FlamesOfAsphodelosAOE3:
                            _flamesOfAsphodelos.Directions[2] = actor.Rotation;
                            break;
                    }
                    break;
                case OID.Sparkfledged:
                    if ((AID)actor.CastInfo!.ActionID == AID.AshenEye)
                    {
                        _fledglingFlight.AddCaster(actor);
                    }
                    break;
            }
        }

        private void ActorCastFinished(object? sender, WorldState.Actor actor)
        {
            switch ((OID)actor.OID)
            {
                case OID.Helper:
                    switch ((AID)actor.CastInfo!.ActionID)
                    {
                        case AID.ExperimentalFireplumeSingleAOE:
                        case AID.ExperimentalGloryplumeSingleAOE:
                            _fireplumeSingle.Position = null;
                            break;
                        case AID.ExperimentalFireplumeMultiAOE:
                        case AID.ExperimentalGloryplumeMultiAOE:
                            _fireplumeMulti.CastEnd();
                            break;
                        case AID.FlamesOfAsphodelosAOE1:
                            _flamesOfAsphodelos.Directions[0] = null;
                            break;
                        case AID.FlamesOfAsphodelosAOE2:
                            _flamesOfAsphodelos.Directions[1] = null;
                            break;
                        case AID.FlamesOfAsphodelosAOE3:
                            _flamesOfAsphodelos.Directions[2] = null;
                            break;
                    }
                    break;
                case OID.Sparkfledged:
                    if ((AID)actor.CastInfo!.ActionID == AID.AshenEye)
                    {
                        _fledglingFlight.RemoveCaster(actor);
                    }
                    break;
            }
        }

        private void EventIcon(object? sender, (uint actorID, uint iconID) arg)
        {
            if (arg.iconID >= 0x17F && arg.iconID <= 0x186 && arg.actorID == WorldState.PlayerActorID)
            {
                _brightenedFire.SetPlayerOrder(this, arg.iconID - 0x17F);
            }
            else if (arg.iconID >= 0x19B && arg.iconID <= 0x19E)
            {
                _fledglingFlight.AddPlayer(WorldState.FindActor(arg.actorID), arg.iconID - 0x19B);
            }
        }

        private void EventCast(object? sender, WorldState.CastResult info)
        {
            switch ((OID)info.CasterID)
            {
                case OID.Boss:
                    switch ((AID)info.ActionID)
                    {
                        case AID.ExperimentalGloryplumeSpread:
                            _ashplume.CurState = Ashplume.State.Spread;
                            break;
                        case AID.ExperimentalGloryplumeStack:
                            _ashplume.CurState = Ashplume.State.Stack;
                            break;
                    }
                    break;
                case OID.Helper:
                    switch ((AID)info.ActionID)
                    {
                        case AID.ExperimentalGloryplumeSpreadAOE:
                        case AID.ExperimentalGloryplumeStackAOE:
                        case AID.ExperimentalAshplumeSpreadAOE:
                        case AID.ExperimentalAshplumeStackAOE:
                            _ashplume.CurState = Ashplume.State.None;
                            break;
                    }
                    break;
            }
        }
    }
}
