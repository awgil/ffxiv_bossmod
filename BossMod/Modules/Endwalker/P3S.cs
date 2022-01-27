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
            FinalExaltation = 27691, // Boss->Boss
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
        private class HeatOfCondemnation : Component
        {
            public bool Active = false;

            private P3S _module;
            private ulong _inAnyAOE = 0; // players hit by aoe, excluding selves

            private static float _aoeRange = 6;

            public HeatOfCondemnation(P3S module) : base(module)
            {
                _module = module;
            }

            public override void Reset() => Active = false;

            public override void Update()
            {
                _inAnyAOE = 0;
                if (!Active)
                    return;

                foreach ((int i, var player) in _module.IterateRaidMembers().Where(indexPlayer => IsTethered(indexPlayer.Item2)))
                {
                    _inAnyAOE |= _module.FindRaidMembersInRange(i, _aoeRange);
                }
            }

            public override void AddHints(int slot, WorldState.Actor actor, List<string> hints)
            {
                if (!Active)
                    return;

                if (actor.Role == WorldState.ActorRole.Tank)
                {
                    if (!IsTethered(actor))
                    {
                        hints.Add("Grab the tether!");
                    }
                    else if (_module.IterateRaidMembersInRange(slot, _aoeRange).Any())
                    {
                        hints.Add("GTFO from raid!");
                    }
                }
                else
                {
                    if (IsTethered(actor))
                    {
                        hints.Add("Hit by tankbuster");
                    }
                    if (BitVector.IsVector64BitSet(_inAnyAOE, slot))
                    {
                        hints.Add("GTFO from aoe!");
                    }
                }
            }

            public override void DrawArenaForeground(MiniArena arena)
            {
                var pc = _module.Player();
                var boss = _module.Boss();
                if (!Active || boss == null || pc == null)
                    return;

                // currently we always show tethered targets with circles, and if pc is a tank, also untethered players
                foreach ((int i, var player) in _module.IterateRaidMembers())
                {
                    if (IsTethered(player))
                    {
                        arena.AddLine(player.Position, boss.Position, player.Role == WorldState.ActorRole.Tank ? arena.ColorSafe : arena.ColorDanger);
                        if (i != _module.PlayerSlot)
                            arena.Actor(player, arena.ColorDanger);
                        arena.AddCircle(player.Position, _aoeRange, arena.ColorDanger);
                    }
                    else if (pc.Role == WorldState.ActorRole.Tank)
                    {
                        arena.Actor(player, BitVector.IsVector64BitSet(_inAnyAOE, i) ? arena.ColorPlayerInteresting : arena.ColorPlayerGeneric);
                    }
                }
            }

            private bool IsTethered(WorldState.Actor player)
            {
                return player.Tether.ID == (uint)TetherID.HeatOfCondemnation;
            }
        }

        // state related to cinderwing
        private class Cinderwing : Component
        {
            public enum State { None, Left, Right }
            public State CurState = State.None;

            private P3S _module;

            public Cinderwing(P3S module) : base(module)
            {
                _module = module;
            }

            public override void Reset() => CurState = State.None;

            public override void AddHints(int slot, WorldState.Actor actor, List<string> hints)
            {
                var boss = _module.Boss();
                if (CurState == State.None || boss == null)
                    return;

                float rot = CurState == State.Left ? MathF.PI / 2 : -MathF.PI / 2;
                if (GeometryUtils.PointInCone(actor.Position - boss.Position, boss.Rotation + rot, MathF.PI / 2))
                {
                    hints.Add("GTFO from wing!");
                }
            }

            public override void DrawArenaBackground(MiniArena arena)
            {
                var boss = _module.Boss();
                if (CurState == State.None || boss == null)
                    return;

                float rot = CurState == State.Left ? MathF.PI / 2 : -MathF.PI / 2;
                arena.ZoneQuad(boss.Position, boss.Rotation + rot, 50, 0, 50, arena.ColorAOE);
            }
        }

        // state related to devouring brand mechanic
        private class DevouringBrand : Component
        {
            public bool Active = false;

            private P3S _module;

            private static float _halfWidth = 5;

            public DevouringBrand(P3S module) : base(module)
            {
                _module = module;
            }

            public override void Reset() => Active = false;

            public override void AddHints(int slot, WorldState.Actor actor, List<string> hints)
            {
                if (!Active)
                    return;

                var offset = actor.Position - _module.Arena.WorldCenter;
                if (MathF.Abs(offset.X) <= _halfWidth || MathF.Abs(offset.Z) <= _halfWidth)
                {
                    hints.Add("GTFO from brand!");
                }
            }

            public override void DrawArenaBackground(MiniArena arena)
            {
                if (!Active)
                    return;

                arena.ZoneQuad(arena.WorldCenter,  Vector3.UnitX, arena.WorldHalfSize, arena.WorldHalfSize, _halfWidth, arena.ColorAOE);
                arena.ZoneQuad(arena.WorldCenter,  Vector3.UnitZ, arena.WorldHalfSize, -_halfWidth, _halfWidth, arena.ColorAOE);
                arena.ZoneQuad(arena.WorldCenter, -Vector3.UnitZ, arena.WorldHalfSize, -_halfWidth, _halfWidth, arena.ColorAOE);
            }
        }

        // state related to trail of condemnation mechanic
        private class TrailOfCondemnation : Component
        {
            public enum State { None, Center, Sides }
            public State CurState = State.None;

            private P3S _module;

            private static float _halfWidth = 7.5f;
            private static float _sidesOffset = 12.5f;
            private static float _aoeRadius = 6;

            public TrailOfCondemnation(P3S module) : base(module)
            {
                _module = module;
            }

            public override void Reset() => CurState = State.None;

            public override void AddHints(int slot, WorldState.Actor actor, List<string> hints)
            {
                var boss = _module.Boss();
                if (CurState == State.None || boss == null || boss.Position == _module.Arena.WorldCenter)
                    return;

                var dir = Vector3.Normalize(_module.Arena.WorldCenter - boss.Position);
                if (CurState == State.Center)
                {
                    if (GeometryUtils.PointInRect(actor.Position - boss.Position, dir, 2 * _module.Arena.WorldHalfSize, 0, _halfWidth))
                    {
                        hints.Add("GTFO from aoe!");
                    }
                    if (_module.IterateRaidMembersInRange(slot, _aoeRadius).Any())
                    {
                        hints.Add("Spread!");
                    }
                }
                else
                {
                    var offset = _sidesOffset * new Vector3(-dir.Z, 0, dir.X);
                    if (GeometryUtils.PointInRect(actor.Position - boss.Position + offset, dir, 2 * _module.Arena.WorldHalfSize, 0, _halfWidth) ||
                        GeometryUtils.PointInRect(actor.Position - boss.Position - offset, dir, 2 * _module.Arena.WorldHalfSize, 0, _halfWidth))
                    {
                        hints.Add("GTFO from aoe!");
                    }
                    // note: sparks either target all tanks & healers or all dds - so correct pairings are always dd+tank/healer
                    int numStacked = 0;
                    bool goodPair = false;
                    foreach ((_, var pair) in _module.IterateRaidMembersInRange(slot, _aoeRadius))
                    {
                        ++numStacked;
                        goodPair = (actor.Role == WorldState.ActorRole.Tank || actor.Role == WorldState.ActorRole.Healer) != (pair.Role == WorldState.ActorRole.Tank || pair.Role == WorldState.ActorRole.Healer);
                    }
                    if (numStacked != 1)
                    {
                        hints.Add("Stack in pairs!");
                    }
                    else if (!goodPair)
                    {
                        hints.Add("Incorrect pairing!");
                    }
                }
            }

            public override void DrawArenaBackground(MiniArena arena)
            {
                var boss = _module.Boss();
                if (CurState == State.None || boss == null || boss.Position == arena.WorldCenter)
                    return;

                var dir = Vector3.Normalize(arena.WorldCenter - boss.Position);
                if (CurState == State.Center)
                {
                    arena.ZoneQuad(boss.Position, dir, 2 * arena.WorldHalfSize, 0, _halfWidth, arena.ColorAOE);
                }
                else
                {
                    var offset = _sidesOffset * new Vector3(-dir.Z, 0, dir.X);
                    arena.ZoneQuad(boss.Position + offset, dir, 2 * arena.WorldHalfSize, 0, _halfWidth, arena.ColorAOE);
                    arena.ZoneQuad(boss.Position - offset, dir, 2 * arena.WorldHalfSize, 0, _halfWidth, arena.ColorAOE);
                }
            }

            public override void DrawArenaForeground(MiniArena arena)
            {
                var pc = _module.Player();
                if (CurState == State.None || pc == null)
                    return;

                // draw all raid members, to simplify positioning
                foreach ((int i, var player) in _module.IterateRaidMembers())
                {
                    if (i != _module.PlayerSlot)
                    {
                        bool inRange = GeometryUtils.PointInCircle(player.Position - pc.Position, _aoeRadius);
                        arena.Actor(player, inRange ? arena.ColorPlayerInteresting : arena.ColorPlayerGeneric);
                    }
                }

                // draw circle around pc
                arena.AddCircle(pc.Position, _aoeRadius, arena.ColorDanger);
            }
        }

        // state related to 'single' fireplumes (normal or parts of gloryplume)
        private class FireplumeSingle : Component
        {
            private P3S _module;
            private Vector3? Position = null;

            private static float _radius = 15;

            public FireplumeSingle(P3S module) : base(module)
            {
                _module = module;
            }

            public override void Reset() => Position = null;

            public override void AddHints(int slot, WorldState.Actor actor, List<string> hints)
            {
                if (Position == null)
                    return;

                if (GeometryUtils.PointInCircle(actor.Position - Position.Value, _radius))
                {
                    hints.Add("GTFO from plume!");
                }
            }

            public override void DrawArenaBackground(MiniArena arena)
            {
                if (Position != null)
                {
                    arena.ZoneCircle(Position.Value, _radius, arena.ColorAOE);
                }
            }

            public override void OnCastStarted(WorldState.Actor actor)
            {
                var aid = (AID)actor.CastInfo!.ActionID;
                if (aid == AID.ExperimentalFireplumeSingleAOE || aid == AID.ExperimentalGloryplumeSingleAOE)
                    Position = actor.Position;
            }

            public override void OnCastFinished(WorldState.Actor actor)
            {
                var aid = (AID)actor.CastInfo!.ActionID;
                if (aid == AID.ExperimentalFireplumeSingleAOE || aid == AID.ExperimentalGloryplumeSingleAOE)
                    Position = null;
            }
        }

        // state related to 'multi' fireplumes (normal or parts of gloryplume)
        // active+pending: 0 = inactive, 1 = only center left, 2 = center and one of the orbs from last pair, ..., 9 = center and all four pairs left
        private class FireplumeMulti : Component
        {
            private P3S _module;
            private float _startingDirection;
            private int _numActiveCasts = 0;
            private int _numPendingOrbs = 0;

            private static float _pairOffset = 15;
            private static float _radius = 10;

            public FireplumeMulti(P3S module) : base(module)
            {
                _module = module;
            }

            public override void Reset() => _numActiveCasts = _numPendingOrbs = 0;

            public override void AddHints(int slot, WorldState.Actor actor, List<string> hints)
            {
                int numTotal = _numActiveCasts + _numPendingOrbs;
                if ((numTotal >= 1 && GeometryUtils.PointInCircle(actor.Position - _module.Arena.WorldCenter, _radius)) ||
                    (numTotal >= 3 && InPair(_startingDirection + MathF.PI / 4, actor)) ||
                    (numTotal >= 5 && InPair(_startingDirection - MathF.PI / 2, actor)) ||
                    (numTotal >= 7 && InPair(_startingDirection - MathF.PI / 4, actor)) ||
                    (numTotal >= 9 && InPair(_startingDirection, actor)))
                {
                    hints.Add("GTFO from plume!");
                }
            }

            public override void DrawArenaBackground(MiniArena arena)
            {
                int numTotal = _numActiveCasts + _numPendingOrbs;
                if (numTotal == 0)
                    return; // inactive

                if (numTotal < 9) // don't draw center aoe before first explosion, it's confusing - but start drawing it immediately after first explosion, to simplify positioning
                    arena.ZoneCircle(arena.WorldCenter, _radius, _numPendingOrbs == 0 ? arena.ColorDanger : arena.ColorAOE);

                // don't draw more than two next pairs
                if (numTotal >= 3 && numTotal <= 5)
                    DrawPair(arena, _startingDirection + MathF.PI / 4, _numPendingOrbs < 2);
                if (numTotal >= 5 && numTotal <= 7)
                    DrawPair(arena, _startingDirection - MathF.PI / 2, _numPendingOrbs < 4);
                if (numTotal >= 7)
                    DrawPair(arena, _startingDirection - MathF.PI / 4, _numPendingOrbs < 6);
                if (numTotal >= 9)
                    DrawPair(arena, _startingDirection, _numPendingOrbs < 8);
            }

            public override void OnCastStarted(WorldState.Actor actor)
            {
                var aid = (AID)actor.CastInfo!.ActionID;
                if (aid == AID.ExperimentalFireplumeMultiAOE || aid == AID.ExperimentalGloryplumeMultiAOE)
                {
                    if (_numActiveCasts++ == 0)
                    {
                        // start new sequence
                        _numPendingOrbs = 9;
                        var offset = actor.Position - _module.Arena.WorldCenter;
                        _startingDirection = MathF.Atan2(offset.X, offset.Z);
                    }
                    --_numPendingOrbs;
                }
            }

            public override void OnCastFinished(WorldState.Actor actor)
            {
                var aid = (AID)actor.CastInfo!.ActionID;
                if (aid == AID.ExperimentalFireplumeMultiAOE || aid == AID.ExperimentalGloryplumeMultiAOE)
                {
                    if (--_numActiveCasts <= 0)
                    {
                        // last cast ended, reset - even if we didn't execute all pending casts (e.g. if we wiped)
                        Reset();
                    }
                }
            }

            private bool InPair(float direction, WorldState.Actor actor)
            {
                var offset = _pairOffset * GeometryUtils.DirectionToVec3(direction);
                return GeometryUtils.PointInCircle(actor.Position - _module.Arena.WorldCenter - offset, _radius)
                    || GeometryUtils.PointInCircle(actor.Position - _module.Arena.WorldCenter + offset, _radius);
            }

            private void DrawPair(MiniArena arena, float direction, bool active)
            {
                var offset = _pairOffset * GeometryUtils.DirectionToVec3(direction);
                arena.ZoneCircle(arena.WorldCenter + offset, _radius, active ? arena.ColorDanger : arena.ColorAOE);
                arena.ZoneCircle(arena.WorldCenter - offset, _radius, active ? arena.ColorDanger : arena.ColorAOE);
            }
        }

        // state related to ashplumes (normal or parts of gloryplume)
        // normal ashplume is boss cast (with different IDs depending on stack/spread) + instant aoe some time later
        // gloryplume is one instant cast with animation only soon after boss cast + instant aoe some time later
        private class Ashplume : Component
        {
            public enum State { None, UnknownGlory, Stack, Spread }
            public State CurState = State.None;

            private P3S _module;

            private static float _stackRadius = 8;
            private static float _spreadRadius = 6;

            public Ashplume(P3S module) : base(module)
            {
                _module = module;
            }

            public override void Reset() => CurState = State.None;

            public override void AddHints(int slot, WorldState.Actor actor, List<string> hints)
            {
                if (CurState == State.Stack)
                {
                    // note: it seems to always target 1 tank & 1 healer, so correct stacks are always tanks+dd and healers+dd
                    int numStacked = 0;
                    bool haveTanks = actor.Role == WorldState.ActorRole.Tank;
                    bool haveHealers = actor.Role == WorldState.ActorRole.Healer;
                    foreach ((_, var pair) in _module.IterateRaidMembersInRange(slot, _stackRadius))
                    {
                        ++numStacked;
                        haveTanks |= pair.Role == WorldState.ActorRole.Tank;
                        haveHealers |= pair.Role == WorldState.ActorRole.Healer;
                    }
                    if (numStacked != 3)
                    {
                        hints.Add("Stack in fours!");
                    }
                    else if (haveTanks && haveHealers)
                    {
                        hints.Add("Incorrect stack!");
                    }
                }
                else if (CurState == State.Spread)
                {
                    if (_module.IterateRaidMembersInRange(slot, _spreadRadius).Any())
                    {
                        hints.Add("Spread!");
                    }
                }
            }

            public override void DrawArenaForeground(MiniArena arena)
            {
                var pc = _module.Player();
                if (CurState <= State.UnknownGlory || pc == null)
                    return;

                // draw all raid members, to simplify positioning
                float aoeRadius = CurState == State.Stack ? _stackRadius : _spreadRadius;
                foreach ((int i, var player) in _module.IterateRaidMembers())
                {
                    if (i != _module.PlayerSlot)
                    {
                        bool inRange = GeometryUtils.PointInCircle(player.Position - pc.Position, aoeRadius);
                        arena.Actor(player, inRange ? arena.ColorPlayerInteresting : arena.ColorPlayerGeneric);
                    }
                }

                // draw circle around pc
                arena.AddCircle(pc.Position, aoeRadius, arena.ColorDanger);
            }

            public override void OnEventCast(WorldState.CastResult info)
            {
                switch ((AID)info.ActionID)
                {
                    case AID.ExperimentalGloryplumeSpread:
                        CurState = State.Spread;
                        break;
                    case AID.ExperimentalGloryplumeStack:
                        CurState = State.Stack;
                        break;
                    case AID.ExperimentalGloryplumeSpreadAOE:
                    case AID.ExperimentalGloryplumeStackAOE:
                    case AID.ExperimentalAshplumeSpreadAOE:
                    case AID.ExperimentalAshplumeStackAOE:
                        CurState = State.None;
                        break;
                }
            }
        }

        // state related to darkened/brightened fire mechanic
        // this helper relies on waymarks 1-4, and assumes they don't change during fight - this is of course quite an assumption, but whatever...
        // TODO: consider how this can be improved...
        private class BrightenedFire
        {
            private ulong _myAdds = 0; // mask of adds pc should hit

            private static float _aoeRange = 7;

            public void Reset()
            {
                _myAdds = 0;
            }

            public void SetPlayerOrder(P3S self, uint order)
            {
                _myAdds = 0;

                var markID = (WorldState.Waymark)((uint)WorldState.Waymark.N1 + order % 4);
                var markPos = self.WorldState.GetWaymark(markID);
                if (markPos == null)
                    return;

                List<(int, float)> addsByRange = new();
                for (int i = 0; i < self._darkenedFires.Count; ++i)
                    addsByRange.Add(new(i, (self._darkenedFires[i].Position - markPos.Value).LengthSquared()));
                addsByRange.Sort((l, r) => l.Item2.CompareTo(r.Item2));
                foreach ((var index, _) in addsByRange.Take(2))
                    BitVector.SetVector64Bit(ref _myAdds, index);
            }

            public void DrawArenaForeground(P3S self)
            {
                if (self._darkenedFires.Count == 0)
                    return;

                // draw all adds
                for (int i = 0; i < self._darkenedFires.Count; ++i)
                {
                    var add = self._darkenedFires[i];
                    self.Arena.Actor(add.Position, add.Rotation, BitVector.IsVector64BitSet(_myAdds, i) ? self.Arena.ColorDanger : self.Arena.ColorPlayerGeneric);
                }

                // draw range circle
                var pc = self.RaidMember(self.PlayerSlot);
                if (pc != null)
                {
                    self.Arena.AddCircle(pc.Position, _aoeRange, self.Arena.ColorDanger);
                }
            }

            public void AddHints(P3S self, List<string> res)
            {
                var pc = self.RaidMember(self.PlayerSlot);
                if (pc == null || _myAdds == 0)
                    return;

                for (int i = 0; i < self._darkenedFires.Count; ++i)
                {
                    if (BitVector.IsVector64BitSet(_myAdds, i) && !GeometryUtils.PointInCircle(pc.Position - self._darkenedFires[i].Position, _aoeRange))
                    {
                        res.Add("Get closer to add! ");
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

            public void AddHints(P3S self, List<string> res)
            {
                if (_birdsAtRisk != 0)
                {
                    res.Add("Drag bird away! ");
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
                foreach (var bird in self._birdsLarge)
                {
                    var target = bird.Tether.Target != 0 ? self.WorldState.FindActor(bird.Tether.Target) : null;
                    if (target != null)
                    {
                        var offset = target.Position - bird.Position;
                        var dist = offset.Length();
                        self.Arena.ZoneQuad(bird.Position, offset / dist, dist, 0, _chargeHalfWidth, self.Arena.ColorAOE);
                    }
                }
            }

            public void DrawArenaForeground(P3S self)
            {
                var pc = self.RaidMember(self.PlayerSlot);
                if (!Active || pc == null)
                    return;

                var pcTetherTarget = pc.Tether.Target != 0 ? self.WorldState.FindActor(pc.Tether.Target) : null;
                if (pcTetherTarget != null)
                {
                    DrawTether(self, pc, pcTetherTarget, pc.Tether.ID);
                }

                foreach (var pcTetherSource in FindTetherSources(self, pc))
                {
                    DrawTether(self, pc, pcTetherSource, pcTetherSource.Tether.ID);

                    if (pcTetherSource.Type == WorldState.ActorType.Player)
                    {
                        foreach (var secondTetherSource in FindTetherSources(self, pcTetherSource))
                        {
                            DrawTether(self, pcTetherSource, secondTetherSource, secondTetherSource.Tether.ID);
                        }
                    }
                }
            }

            public void AddHints(P3S self, List<string> res)
            {
                var pc = self.RaidMember(self.PlayerSlot);
                if (!Active || pc == null)
                    return;

                foreach (var bird in self._birdsLarge)
                {
                    if (bird.Tether.Target == self.PlayerSlot)
                        continue;

                    var target = bird.Tether.Target != 0 ? self.WorldState.FindActor(bird.Tether.Target) : null;
                    if (target == null)
                        continue;

                    var dir = Vector3.Normalize(target.Position - bird.Position);
                    var normal = new Vector3(-dir.Z, 0, dir.X);
                    var pcOffset = Vector3.Dot(pc.Position - bird.Position, normal);
                    if (MathF.Abs(pcOffset) <= _chargeHalfWidth)
                    {
                        res.Add("GTFO from charge zone! ");
                        break;
                    }
                }
            }

            private IEnumerable<WorldState.Actor> FindTetherSources(P3S self, WorldState.Actor target)
            {
                return self._birdsLarge.Where(bird => bird.Tether.Target == target.InstanceID)
                    .Concat(self.RaidMembers.Where(player => player?.Tether.Target == target.InstanceID).Select(player => player!));
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
                    self.Arena.ZoneQuad(bird.Position, dir, 50, 0, _chargeHalfWidth, self.Arena.ColorAOE);
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

            public void AddHints(P3S self, List<string> res)
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
                        res.Add("GTFO from charge zone! ");
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
                if (Directions[0] != null)
                {
                    DrawZone(self, Directions[0], self.Arena.ColorDanger);
                    DrawZone(self, Directions[1], self.Arena.ColorAOE);
                }
                else
                {
                    DrawZone(self, Directions[1], self.Arena.ColorAOE);
                    DrawZone(self, Directions[2], self.Arena.ColorAOE);
                }
            }

            public void AddHints(P3S self, List<string> res)
            {
                var pc = self.RaidMember(self.PlayerSlot);
                if (pc == null)
                    return;

                if (InAOE(self, Directions[1], pc.Position) || InAOE(self, Directions[0] != null ? Directions[0] : Directions[2], pc.Position))
                {
                    res.Add("GTFO from cone! ");
                }
            }

            private void DrawZone(P3S self, float? dir, uint color)
            {
                if (dir != null)
                {
                    self.Arena.ZoneIsoscelesTri(self.Arena.WorldCenter, dir.Value, MathF.PI / 6, 50, color);
                    self.Arena.ZoneIsoscelesTri(self.Arena.WorldCenter, dir.Value + MathF.PI, MathF.PI / 6, 50, color);
                }
            }

            private bool InAOE(P3S self, float? dir, Vector3 pos)
            {
                if (dir == null)
                    return false;

                var toPos = Vector3.Normalize(pos - self.Arena.WorldCenter);
                var toDir = GeometryUtils.DirectionToVec3(dir.Value);
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
                var boss = self.Boss();
                if (!Active || boss == null)
                    return;

                // we determine failing players, trying to take two reasonable tactics in account:
                // either two tanks immune and soak everything, or each player is hit by one mechanic
                // for now, we consider tether target to be a "tank"
                int[] aoesPerPlayer = new int[self.RaidMembers.Length];

                foreach ((int i, var player) in self.IterateRaidMembers(true).Where(indexPlayer => indexPlayer.Item2.Tether.Target == boss.InstanceID))
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
                foreach ((int i, var player) in FindClosest(self, boss.Position).Take(3))
                {
                    _bossTargets.Add(i);
                    foreach ((int j, var other) in FindPlayersInWinds(self, boss.Position, player, cosHalfAngle))
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
                var boss = self.Boss();
                if (!Active || boss == null)
                    return;

                foreach (int i in _bossTargets)
                {
                    var player = self.RaidMembers[i];
                    if (player == null || player.Position == boss.Position)
                        continue;

                    var offset = player.Position - boss.Position;
                    float phi = MathF.Atan2(offset.X, offset.Z);
                    self.Arena.ZoneCone(boss.Position, 0, 50, phi - _coneHalfAngle, phi + _coneHalfAngle, self.Arena.ColorAOE);
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

            public void AddHints(P3S self, List<string> res)
            {
                // think of better hints here...
                if (_invulNeeded)
                {
                    res.Add("Press invul! ");
                }
                if (_failingPlayers != 0)
                {
                    res.Add("Storms failing! ");
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

            public void AddHints(P3S self, List<string> res)
            {
                if (CurState == State.None)
                    return;

                if (CurState == State.Knockback && (_testPos - self.Arena.WorldCenter).LengthSquared() >= self.Arena.WorldHalfSize * self.Arena.WorldHalfSize)
                {
                    res.Add("About to be knocked back into wall! ");
                }

                foreach (var twister in self._twisters.Where(twister => twister.CastInfo != null && twister.CastInfo.ActionID == (uint)AID.BurningTwister))
                {
                    var distSq = (twister.Position - _testPos).LengthSquared();
                    if (distSq >= _aoeInnerRadius * _aoeInnerRadius && distSq <= _aoeOuterRadius * _aoeOuterRadius)
                    {
                        res.Add("GTFO from aoe! ");
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

            public void AddHints(P3S self, List<string> res)
            {
                var pc = self.RaidMember(self.PlayerSlot);
                if (pc == null || _sources.Count == 0)
                    return;

                int aoeCount = 0;
                var cosHalfAngle = MathF.Cos(_coneHalfAngle);
                foreach ((var source, var dir) in _sources)
                {
                    var toDir = GeometryUtils.DirectionToVec3(dir);
                    var toCur = Vector3.Normalize(pc.Position - source.Position);
                    if (Vector3.Dot(toDir, toCur) >= cosHalfAngle)
                    {
                        ++aoeCount;
                    }
                }

                int deathTollStacks = pc.FindStatus((uint)SID.DeathsToll)?.StackCount ?? 0;
                if (aoeCount < deathTollStacks)
                {
                    res.Add($"Enter more aoes ({aoeCount}/{deathTollStacks})! ");
                }
                else if (aoeCount > deathTollStacks)
                {
                    res.Add($"GTFO from eyes ({aoeCount}/{deathTollStacks})! ");
                }
            }
        }

        private List<WorldState.Actor> _boss = new();
        private List<WorldState.Actor> _darkenedFires = new();
        private List<WorldState.Actor> _birdsSmall = new();
        private List<WorldState.Actor> _birdsLarge = new();
        private List<WorldState.Actor> _sunshadows = new();
        private List<WorldState.Actor> _twisters = new();
        private WorldState.Actor? Boss() => _boss.FirstOrDefault();

        private HeatOfCondemnation _heatOfCondemnation;
        private Cinderwing _cinderwing;
        private DevouringBrand _devouringBrand;
        private TrailOfCondemnation _trailOfCondemnation;
        private FireplumeSingle _fireplumeSingle;
        private FireplumeMulti _fireplumeMulti;
        private Ashplume _ashplume;
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
            _heatOfCondemnation = new(this);
            _cinderwing = new(this);
            _devouringBrand = new(this);
            _trailOfCondemnation = new(this);
            _fireplumeSingle = new(this);
            _fireplumeMulti = new(this);
            _ashplume = new(this);

            WorldState.ActorCastStarted += ActorCastStarted;
            WorldState.ActorCastFinished += ActorCastFinished;
            WorldState.EventIcon += EventIcon;

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

            s = CommonStates.Targetable(ref s.Next, Boss, false, 4.6f); // flies away
            s = BuildTrailOfCondemnationState(ref s.Next, 3.8f);
            s = BuildSmallBirdsState(ref s.Next, 6);
            s = BuildLargeBirdsState(ref s.Next, 3.4f);
            s = CommonStates.Targetable(ref s.Next, Boss, true, 5.2f, "Boss reappears");
            s.EndHint |= StateMachine.StateHint.PositioningEnd;

            s = CommonStates.Cast(ref s.Next, Boss, AID.DeadRebirth, 9.2f, 10, "DeadRebirth");
            s.EndHint |= StateMachine.StateHint.Raidwide;
            s = BuildHeatOfCondemnationState(ref s.Next, 9.2f);
            s = BuildFledglingFlightState(ref s.Next, 8.2f);
            s = BuildExperimentalGloryplumeMultiState(ref s.Next, 14.3f);
            s = BuildFountainOfFireState(ref s.Next, 12.2f);
            s = BuildScorchedExaltationState(ref s.Next, 5);
            s = BuildScorchedExaltationState(ref s.Next, 2);
            s = BuildHeatOfCondemnationState(ref s.Next, 5);
            s = CommonStates.Cast(ref s.Next, Boss, AID.FirestormsOfAsphodelos, 9.6f, 5, "Firestorm");
            s.EndHint |= StateMachine.StateHint.Raidwide;

            s = BuildFlamesOfAsphodelosState(ref s.Next, 3.2f);
            s = BuildExperimentalAshplumeCastState(ref s.Next, 2);
            s = BuildExperimentalAshplumeResolveState(ref s.Next, 6);

            s = BuildFlamesOfAsphodelosState(ref s.Next, 2);
            s = BuildStormsOfAsphodelosState(ref s.Next, 10);

            s = BuildDarkblazeTwisterState(ref s.Next, 2.2f);

            s = BuildScorchedExaltationState(ref s.Next, 2);
            s = BuildDeathTollState(ref s.Next, 7.2f);
            s = BuildExperimentalGloryplumeSingleState(ref s.Next, 7.3f);

            s = CommonStates.Targetable(ref s.Next, Boss, false, 3);
            s = BuildTrailOfCondemnationState(ref s.Next, 3.8f);
            s = CommonStates.Targetable(ref s.Next, Boss, true, 4.7f);

            s = BuildDevouringBrandState(ref s.Next, 5.1f);
            s = BuildScorchedExaltationState(ref s.Next, 6.2f);
            s = BuildScorchedExaltationState(ref s.Next, 2.2f);
            s = CommonStates.Cast(ref s.Next, Boss, AID.FinalExaltation, 2.1f, 10, "Enrage");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                WorldState.ActorCastStarted -= ActorCastStarted;
                WorldState.ActorCastFinished -= ActorCastFinished;
                WorldState.EventIcon -= EventIcon;
            }
            base.Dispose(disposing);
        }

        protected override void ResetModule()
        {
            _brightenedFire.Reset();
            _birdDistance.WatchedBirds = null;
            _birdTether.Active = false;
            _flamesOfAsphodelos.Reset();
            _stormsOfAsphodelos.Active = false;
            _darkblazeTwister.CurState = DarkblazeTwister.State.None;
            _fledglingFlight.Reset();
        }

        protected override void UpdateModule()
        {
            _birdDistance.Update(this);
            _stormsOfAsphodelos.Update(this);
            _darkblazeTwister.Update(this);
        }

        protected override void AddHints(int slot, WorldState.Actor actor, List<string> hints)
        {
            _brightenedFire.AddHints(this, hints);
            _birdDistance.AddHints(this, hints);
            _birdTether.AddHints(this, hints);
            _sunshadowTether.AddHints(this, hints);
            _flamesOfAsphodelos.AddHints(this, hints);
            _stormsOfAsphodelos.AddHints(this, hints);
            _darkblazeTwister.AddHints(this, hints);
            _fledglingFlight.AddHints(this, hints);
        }

        protected override void DrawArenaBackground()
        {
            _birdTether.DrawArenaBackground(this);
            _sunshadowTether.DrawArenaBackground(this);
            _flamesOfAsphodelos.DrawArenaBackground(this);
            _stormsOfAsphodelos.DrawArenaBackground(this);
            _darkblazeTwister.DrawArenaBackground(this);
            _fledglingFlight.DrawArenaBackground(this);
        }

        protected override void DrawArenaForegroundPost()
        {
            _brightenedFire.DrawArenaForeground(this);
            _birdDistance.DrawArenaForeground(this);
            _birdTether.DrawArenaForeground(this);
            _sunshadowTether.DrawArenaForeground(this);
            _stormsOfAsphodelos.DrawArenaForeground(this);
            _darkblazeTwister.DrawArenaForeground(this);

            Arena.Actor(Boss(), Arena.ColorEnemy);
            Arena.Actor(Player(), Arena.ColorPC);
        }

        protected override Dictionary<uint, RelevantEnemy> DefineRelevantEnemies()
        {
            Dictionary<uint, RelevantEnemy> res = new();
            res[(uint)OID.Boss] = new(_boss, true);
            res[(uint)OID.DarkenedFire] = new(_darkenedFires);
            res[(uint)OID.SunbirdSmall] = new(_birdsSmall);
            res[(uint)OID.SunbirdLarge] = new(_birdsLarge);
            res[(uint)OID.Sunshadow] = new(_sunshadows);
            res[(uint)OID.DarkblazeTwister] = new(_twisters);
            return res;
        }

        private StateMachine.State BuildScorchedExaltationState(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, Boss, AID.ScorchedExaltation, delay, 5, "AOE");
            s.EndHint |= StateMachine.StateHint.Raidwide;
            return s;
        }

        private StateMachine.State BuildHeatOfCondemnationState(ref StateMachine.State? link, float delay)
        {
            var cast = CommonStates.Cast(ref link, Boss, AID.HeatOfCondemnation, delay, 6, "Tether");
            cast.Enter = () => _heatOfCondemnation.Active = true;
            cast.Exit = () => _heatOfCondemnation.Active = false;
            cast.EndHint |= StateMachine.StateHint.Tankbuster;
            // note: actual AOE is about 1s after cast end
            return cast;
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
            var start = CommonStates.CastStart(ref link, Boss, dispatch, delay);
            start.EndHint |= StateMachine.StateHint.PositioningStart;

            var end = CommonStates.CastEnd(ref start.Next, Boss, 5, "Fireplume");
            return end;
        }

        // note - no positioning flags, since this is part of mechanics that manage it themselves
        // note - since it resolves in a complex way, make sure to add a resolve state!
        private StateMachine.State BuildExperimentalAshplumeCastState(ref StateMachine.State? link, float delay)
        {
            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.ExperimentalAshplumeStack] = new(null, () => _ashplume.CurState = Ashplume.State.Stack);
            dispatch[AID.ExperimentalAshplumeSpread] = new(null, () => _ashplume.CurState = Ashplume.State.Spread);
            var start = CommonStates.CastStart(ref link, Boss, dispatch, delay);
            var end = CommonStates.CastEnd(ref start.Next, Boss, 5, "Ashplume");
            end.EndHint |= StateMachine.StateHint.GroupWithNext;
            return end;
        }

        // note: automatically clears positioning flag
        private StateMachine.State BuildExperimentalAshplumeResolveState(ref StateMachine.State? link, float delay)
        {
            var resolve = CommonStates.Condition(ref link, delay, () => _ashplume.CurState == Ashplume.State.None, 1, "Ashplume resolve");
            resolve.Exit = () => _ashplume.CurState = Ashplume.State.None;
            resolve.EndHint |= StateMachine.StateHint.Raidwide | StateMachine.StateHint.PositioningEnd;
            return resolve;
        }

        private StateMachine.State BuildExperimentalGloryplumeMultiState(ref StateMachine.State? link, float delay)
        {
            // first part for this mechanic always seems to be "multi-plume", works just like fireplume
            // 9 helpers teleport to position, first pair almost immediately starts casting 26315s, 1 sec stagger between pairs, 7 sec for each cast
            // ~3 sec after cast ends, boss makes an instant cast that determines stack/spread (26316/26312), ~10 sec after that hits with real AOE (26317/26313)
            // note that our helpers rely on casts rather than states
            var cast = CommonStates.Cast(ref link, Boss, AID.ExperimentalGloryplumeMulti, delay, 5, "GloryplumeMulti");
            cast.Exit = () => _ashplume.CurState = Ashplume.State.UnknownGlory; // instant cast turns this into correct state in ~3 sec
            cast.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;

            var resolve = CommonStates.Condition(ref cast.Next, 13, () => _ashplume.CurState == Ashplume.State.None, 1, "Gloryplume resolve");
            resolve.Exit = () => _ashplume.CurState = Ashplume.State.None;
            resolve.EndHint |= StateMachine.StateHint.Raidwide | StateMachine.StateHint.PositioningEnd;
            return resolve;
        }

        private StateMachine.State BuildExperimentalGloryplumeSingleState(ref StateMachine.State? link, float delay)
        {
            // first part for this mechanic always seems to be "single-plume", works just like fireplume
            // helper teleports to position, almost immediately starts casting 26311, 6 sec for cast
            // ~3 sec after cast ends, boss makes an instant cast that determines stack/spread (26316/26312), ~4 sec after that hits with real AOE (26317/26313)
            // note that our helpers rely on casts rather than states
            var cast = CommonStates.Cast(ref link, Boss, AID.ExperimentalGloryplumeSingle, delay, 5, "GloryplumeSingle");
            cast.Exit = () => _ashplume.CurState = Ashplume.State.UnknownGlory; // instant cast turns this into correct state in ~3 sec
            cast.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;

            var resolve = CommonStates.Condition(ref cast.Next, 7.2f, () => _ashplume.CurState == Ashplume.State.None, 1, "Gloryplume resolve");
            resolve.Exit = () => _ashplume.CurState = Ashplume.State.None;
            resolve.EndHint |= StateMachine.StateHint.Raidwide | StateMachine.StateHint.PositioningEnd;
            return resolve;
        }

        private StateMachine.State BuildCinderwingState(ref StateMachine.State? link, float delay)
        {
            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.RightCinderwing] = new(null, () => _cinderwing.CurState = Cinderwing.State.Right);
            dispatch[AID.LeftCinderwing] = new(null, () => _cinderwing.CurState = Cinderwing.State.Left);
            var start = CommonStates.CastStart(ref link, Boss, dispatch, delay);

            var end = CommonStates.CastEnd(ref start.Next, Boss, 5, "Wing");
            end.Exit = () => _cinderwing.CurState = Cinderwing.State.None;
            return end;
        }

        private StateMachine.State BuildDevouringBrandState(ref StateMachine.State? link, float delay)
        {
            var devouring = CommonStates.Cast(ref link, Boss, AID.DevouringBrand, delay, 3, "DevouringBrand");
            devouring.EndHint |= StateMachine.StateHint.GroupWithNext;

            var fireplume = BuildExperimentalFireplumeState(ref devouring.Next, 2); // pos-start
            fireplume.EndHint |= StateMachine.StateHint.GroupWithNext;
            fireplume.Exit = () => _devouringBrand.Active = true;

            var breeze = CommonStates.Cast(ref fireplume.Next, Boss, AID.SearingBreeze, 7, 3, "SearingBreeze");
            breeze.EndHint |= StateMachine.StateHint.GroupWithNext;

            var wing = BuildCinderwingState(ref breeze.Next, 3);
            wing.Enter = () => _devouringBrand.Active = false;
            wing.EndHint |= StateMachine.StateHint.PositioningEnd;
            return wing;
        }

        private StateMachine.State BuildDarkenedFireState(ref StateMachine.State? link, float delay)
        {
            // TODO: helper for add placement - need to understand exact spawn mechanics for that...
            // 3s after cast ends, adds start casting 26299
            var addsStart = CommonStates.CastStart(ref link, Boss, AID.DarkenedFire, delay);
            addsStart.EndHint |= StateMachine.StateHint.PositioningStart;
            var addsEnd = CommonStates.CastEnd(ref addsStart.Next, Boss, 6, "DarkenedFire adds");
            addsEnd.EndHint |= StateMachine.StateHint.GroupWithNext;

            var numbers = CommonStates.Cast(ref addsEnd.Next, Boss, AID.BrightenedFire, 5, 5, "Numbers"); // numbers appear at the beginning of the cast, at the end he starts shooting 1-8
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
            var start = CommonStates.CastStart(ref link, Boss, dispatch, delay);
            var end = CommonStates.CastEnd(ref start.Next, Boss, 6);
            var resolve = CommonStates.Timeout(ref end.Next, 2, "Trail");
            resolve.Exit = () => _trailOfCondemnation.CurState = TrailOfCondemnation.State.None;
            return resolve;
        }

        // note: expects downtime at enter, clears when birds spawn, reset when birds die
        private StateMachine.State BuildSmallBirdsState(ref StateMachine.State? link, float delay)
        {
            var spawn = CommonStates.Simple(ref link, delay, "Small birds");
            spawn.Update = (_) => spawn.Done = _birdsSmall.Count > 0;
            spawn.Exit = () => _birdDistance.WatchedBirds = _birdsSmall;
            spawn.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.DowntimeEnd; // adds become targetable <1sec after spawn

            var enrage = CommonStates.Simple(ref spawn.Next, 25, "Small birds enrage");
            enrage.Update = (_) => enrage.Done = _birdsSmall.Find(x => !x.IsDead) == null;
            enrage.Exit = () => _birdDistance.WatchedBirds = null;
            enrage.EndHint |= StateMachine.StateHint.Raidwide | StateMachine.StateHint.DowntimeStart; // raidwide (26326) happens ~3sec after last bird death
            return enrage;
        }

        // note: expects downtime at enter, clears when birds spawn, reset when birds die
        private StateMachine.State BuildLargeBirdsState(ref StateMachine.State? link, float delay)
        {
            var spawn = CommonStates.Simple(ref link, delay, "Large birds");
            spawn.Update = (_) => spawn.Done = _birdsLarge.Count > 0;
            spawn.Exit = () => _birdTether.Active = true; // tethers appear ~5s after this and last for ~11s
            spawn.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.DowntimeEnd; // adds become targetable ~1sec after spawn

            Func<bool> allBirdsDead = () => _birdsLarge.Find(x => !x.IsDead) == null;
            var chargesDone = CommonStates.Condition(ref spawn.Next, 20, allBirdsDead, 0);
            chargesDone.Exit = () =>
            {
                _birdTether.Active = false;
                _birdDistance.WatchedBirds = _birdsLarge;
            };

            var enrage = CommonStates.Simple(ref chargesDone.Next, 35, "Large birds enrage");
            enrage.Update = (_) => enrage.Done = _birdsLarge.Find(x => !x.IsDead) == null;
            enrage.Exit = () => _birdDistance.WatchedBirds = null;
            enrage.EndHint |= StateMachine.StateHint.Raidwide | StateMachine.StateHint.DowntimeStart; // raidwide (26326) happens ~3sec after last bird death
            return enrage;
        }

        private StateMachine.State BuildFledglingFlightState(ref StateMachine.State? link, float delay)
        {
            // mechanic timeline:
            // 0s cast end
            // 2s icons appear
            // 8s 3540's teleport to players
            // 10s 3540's start casting 26342
            // 14s 3540's finish casting 26342
            // note that helper does relies on icons and cast events rather than states
            return CommonStates.Cast(ref link, Boss, AID.FledglingFlight, delay, 3, 8, "Eyes");
        }

        private StateMachine.State BuildDeathTollState(ref StateMachine.State? link, float delay)
        {
            // notes on mechanics:
            // - on 26349 cast end, debuffs with 25sec appear
            // - 12-15sec after 26350 cast starts, eyes finish casting their cones - at this point, there's about 5sec left on debuffs
            // note that helper does relies on icons and cast events rather than states
            var deathtoll = CommonStates.Cast(ref link, Boss, AID.DeathToll, delay, 6, "DeathToll");
            deathtoll.EndHint |= StateMachine.StateHint.GroupWithNext;

            var eyes = CommonStates.Cast(ref deathtoll.Next, Boss, AID.FledglingFlight, 3.2f, 3, "Eyes");
            eyes.EndHint |= StateMachine.StateHint.GroupWithNext;

            var agonies = CommonStates.Cast(ref eyes.Next, Boss, AID.LifesAgonies, 2, 24, "LifeAgonies");
            return agonies;
        }

        private StateMachine.State BuildFountainOfFireState(ref StateMachine.State? link, float delay)
        {
            // note: helper for sunshadow charges relies on tethers rather than states
            // TODO: healer helper - not even sure, mechanic looks so simple...
            var fountain = CommonStates.Cast(ref link, Boss, AID.FountainOfFire, delay, 6, "FountainOfFire");
            fountain.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;

            var charges = CommonStates.Cast(ref fountain.Next, Boss, AID.SunsPinion, 2, 6, 13, "Charges");
            charges.EndHint |= StateMachine.StateHint.PositioningEnd;
            return charges;
        }

        // note: positioning flag is not cleared in preparation for next mechanic (ashplume or storms)
        private StateMachine.State BuildFlamesOfAsphodelosState(ref StateMachine.State? link, float delay)
        {
            // note: flames helper is activated and deactivated automatically
            var flames = CommonStates.Cast(ref link, Boss, AID.FlamesOfAsphodelos, delay, 3, "Cones");
            flames.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;
            return flames;
        }

        private StateMachine.State BuildStormsOfAsphodelosState(ref StateMachine.State? link, float delay)
        {
            var start = CommonStates.CastStart(ref link, Boss, delay);
            start.Exit = () => _stormsOfAsphodelos.Active = true;

            var end = CommonStates.CastEnd(ref start.Next, Boss, 8, "Storms");
            end.Exit = () => _stormsOfAsphodelos.Active = false;
            end.EndHint |= StateMachine.StateHint.Raidwide | StateMachine.StateHint.Tankbuster;
            return end;
        }

        private StateMachine.State BuildDarkblazeTwisterState(ref StateMachine.State? link, float delay)
        {
            var twister = CommonStates.Cast(ref link, Boss, AID.DarkblazeTwister, delay, 4, "Twister");
            twister.Exit = () => _darkblazeTwister.CurState = DarkblazeTwister.State.Knockback;
            twister.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;

            var breeze = CommonStates.Cast(ref twister.Next, Boss, AID.SearingBreeze, 4, 3, "SearingBreeze");
            breeze.EndHint |= StateMachine.StateHint.GroupWithNext;

            var ashplume = BuildExperimentalAshplumeCastState(ref breeze.Next, 4);

            var knockback = CommonStates.Simple(ref ashplume.Next, 3, "Knockback");
            knockback.Update = (_) => knockback.Done = !_twisters.Any(twister => twister.CastInfo != null && twister.CastInfo.ActionID == (uint)AID.DarkTwister);
            knockback.Exit = () => _darkblazeTwister.CurState = DarkblazeTwister.State.AOE;
            knockback.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.Knockback;

            var aoe = CommonStates.Simple(ref knockback.Next, 2, "AOE");
            aoe.Update = (_) => aoe.Done = !_twisters.Any(twister => twister.CastInfo != null);
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
            if (arg.iconID >= 268 && arg.iconID <= 275 && arg.actorID == WorldState.PlayerActorID)
            {
                _brightenedFire.SetPlayerOrder(this, arg.iconID - 268);
            }
            else if (arg.iconID >= 296 && arg.iconID <= 299)
            {
                _fledglingFlight.AddPlayer(WorldState.FindActor(arg.actorID), arg.iconID - 296);
            }
        }
    }
}
