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

            public HeatOfCondemnation(P3S module)
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

            public Cinderwing(P3S module)
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

            public DevouringBrand(P3S module)
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

            public TrailOfCondemnation(P3S module)
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

            public FireplumeSingle(P3S module)
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

            public FireplumeMulti(P3S module)
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

            public Ashplume(P3S module)
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

        // state relared to darkened fire add placement mechanic
        // adds should be neither too close (or they insta explode and wipe raid) nor too far (or during brightened fire someone wouldn't be able to hit two adds)
        private class DarkenedFire : Component
        {
            public bool Active = false;

            private P3S _module;

            private static float _minRange = 10; // TODO: verify...
            private static float _maxRange = 13; // brigthened fire aoe radius is 7, so this is x2 minus some room for positioning

            public DarkenedFire(P3S module)
            {
                _module = module;
            }

            public override void Reset() => Active = false;

            public override void AddHints(int slot, WorldState.Actor actor, List<string> hints)
            {
                if (!Active)
                    return;

                bool haveTooClose = false;
                int numInRange = 0;
                foreach ((_, var player) in _module.IterateRaidMembers().Where(indexPlayer => CanBothBeTargets(indexPlayer.Item2, actor)))
                {
                    var distance = player.Position - actor.Position;
                    haveTooClose |= GeometryUtils.PointInCircle(distance, _minRange);
                    if (GeometryUtils.PointInCircle(distance, _maxRange))
                        ++numInRange;
                }

                if (haveTooClose)
                {
                    hints.Add("Too close to other players!");
                }
                else if (numInRange < 2)
                {
                    hints.Add("Too far from other players!");
                }
            }

            public override void DrawArenaForeground(MiniArena arena)
            {
                var pc = _module.Player();
                if (!Active || pc == null)
                    return;

                // draw other potential targets, to simplify positioning
                bool healerOrTank = pc.Role == WorldState.ActorRole.Tank || pc.Role == WorldState.ActorRole.Healer;
                foreach ((int i, var player) in _module.IterateRaidMembers().Where(indexPlayer => CanBothBeTargets(indexPlayer.Item2, pc)))
                {
                    var distance = player.Position - pc.Position;
                    bool tooClose = GeometryUtils.PointInCircle(distance, _minRange);
                    bool inRange = GeometryUtils.PointInCircle(distance, _maxRange);
                    arena.Actor(player, tooClose ? arena.ColorDanger : (inRange ? arena.ColorPlayerInteresting : arena.ColorPlayerGeneric));
                }

                // draw circles around pc
                arena.AddCircle(pc.Position, _minRange, arena.ColorDanger);
                arena.AddCircle(pc.Position, _maxRange, arena.ColorSafe);
            }

            private bool CanBothBeTargets(WorldState.Actor one, WorldState.Actor two)
            {
                // TODO: verify this assumption, I don't know how exactly it works...
                return one.InstanceID != two.InstanceID && (one.Role == WorldState.ActorRole.Tank || one.Role == WorldState.ActorRole.Healer) == (two.Role == WorldState.ActorRole.Tank || two.Role == WorldState.ActorRole.Healer);
            }
        }

        // state related to brightened fire mechanic
        // this helper relies on waymarks 1-4, and assumes they don't change during fight - this is of course quite an assumption, but whatever...
        private class BrightenedFire : Component
        {
            public bool Active = false;
            public int NumCastsHappened = 0;

            private P3S _module;
            private List<WorldState.Actor> _darkenedFires;
            private int[] _playerOrder = new int[8]; // 0 if unknown, 1-8 otherwise

            private static float _aoeRange = 7;

            public BrightenedFire(P3S module)
            {
                _module = module;
                _darkenedFires = module.Enemies(OID.DarkenedFire);
            }

            public override void Reset()
            {
                Active = false;
                NumCastsHappened = 0;
                Array.Fill(_playerOrder, 0);
            }

            public override void AddHints(int slot, WorldState.Actor actor, List<string> hints)
            {
                if (!Active || _playerOrder[slot] <= NumCastsHappened)
                    return;

                int numHitAdds = _darkenedFires.Where(fire => GeometryUtils.PointInCircle(fire.Position - actor.Position, _aoeRange)).Count();
                if (numHitAdds < 2)
                {
                    hints.Add("Get closer to adds!");
                }
            }

            public override void DrawArenaForeground(MiniArena arena)
            {
                var pc = _module.Player();
                if (!Active || pc == null || _playerOrder[_module.PlayerSlot] <= NumCastsHappened)
                    return;

                // find two closest adds to player's mark
                var pos = PositionForOrder(_playerOrder[_module.PlayerSlot]);
                var firesRange = new (WorldState.Actor, float)[_darkenedFires.Count];
                for (int i = 0; i < _darkenedFires.Count; ++i)
                {
                    var fire = _darkenedFires[i];
                    firesRange[i] = (fire, (fire.Position - pos).LengthSquared());
                }
                Array.Sort(firesRange, (l, r) => l.Item2.CompareTo(r.Item2));

                // draw all adds
                for (int i = 0; i < firesRange.Length; ++i)
                {
                    arena.Actor(firesRange[i].Item1, i < 2 ? arena.ColorDanger : arena.ColorPlayerGeneric);
                }

                // draw range circle
                arena.AddCircle(pc.Position, _aoeRange, arena.ColorDanger);
            }

            public override void OnEventCast(WorldState.CastResult info)
            {
                if ((AID)info.ActionID == AID.BrightenedFireAOE)
                    ++NumCastsHappened;
            }

            public override void OnEventIcon(uint actorID, uint iconID)
            {
                if (iconID >= 268 && iconID <= 275)
                {
                    int slot = _module.FindRaidMemberSlot(actorID);
                    if (slot >= 0)
                        _playerOrder[slot] = (int)iconID - 267;
                }
            }

            private Vector3 PositionForOrder(int order)
            {
                // TODO: consider how this can be improved...
                var markID = (WorldState.Waymark)((int)WorldState.Waymark.N1 + (order - 1) % 4);
                return _module.WorldState.GetWaymark(markID) ?? _module.Arena.WorldCenter;
            }
        }

        // bird distance utility
        // when small birds die and large birds appear, they cast 26328, and if it hits any other large bird, they buff
        // when large birds die and sparkfledgeds appear, they cast 26329, and if it hits any other sparkfledged, they wipe the raid or something
        // so we show range helper for dead birds
        private class BirdDistance : Component
        {
            public List<WorldState.Actor>? WatchedBirds = null;

            private P3S _module;
            private ulong _birdsAtRisk = 0; // mask

            private static float _radius = 13;

            public BirdDistance(P3S module)
            {
                _module = module;
            }

            public override void Reset() => WatchedBirds = null;

            public override void Update()
            {
                _birdsAtRisk = 0;
                if (WatchedBirds == null)
                    return;

                for (int i = 0; i < WatchedBirds.Count; ++i)
                {
                    var bird = WatchedBirds[i];
                    if (!bird.IsDead && WatchedBirds.Where(other => other.IsDead && (bird.Position - other.Position).LengthSquared() <= _radius * _radius).Any())
                    {
                        BitVector.SetVector64Bit(ref _birdsAtRisk, i);
                    }
                }
            }

            public override void AddHints(int slot, WorldState.Actor actor, List<string> hints)
            {
                if (WatchedBirds == null)
                    return;

                for (int i = 0; i < WatchedBirds.Count; ++i)
                {
                    var bird = WatchedBirds[i];
                    if (!bird.IsDead && bird.TargetID == actor.InstanceID && BitVector.IsVector64BitSet(_birdsAtRisk, i))
                    {
                        hints.Add("Drag bird away!");
                        return;
                    }
                }
            }

            public override void DrawArenaForeground(MiniArena arena)
            {
                if (WatchedBirds == null)
                    return;

                // draw alive birds tanked by PC and circles around dead birds
                for (int i = 0; i < WatchedBirds.Count; ++i)
                {
                    var bird = WatchedBirds[i];
                    if (bird.IsDead)
                    {
                        arena.AddCircle(bird.Position, _radius, arena.ColorDanger);
                    }
                    else if (bird.TargetID == _module.WorldState.PlayerActorID)
                    {
                        arena.Actor(bird, BitVector.IsVector64BitSet(_birdsAtRisk, i) ? arena.ColorEnemy : arena.ColorPlayerGeneric);
                    }
                }
            }
        }

        // state related to large bird tethers
        private class BirdTether : Component
        {
            public bool Active = false;

            private P3S _module;
            private List<WorldState.Actor> _birdsLarge;
            private (WorldState.Actor?, WorldState.Actor?, int)[] _chains = new (WorldState.Actor?, WorldState.Actor?, int)[4]; // actor1, actor2, num-charges

            private static float _chargeHalfWidth = 3;

            public BirdTether(P3S module)
            {
                _module = module;
                _birdsLarge = module.Enemies(OID.SunbirdLarge);
            }

            public override void Reset()
            {
                Active = false;
                Array.Fill(_chains, (null, null, 0));
            }

            public override void Update()
            {
                if (!Active)
                    return;

                for (int i = 0; i < Math.Min(_birdsLarge.Count, 4); ++i)
                {
                    if (_chains[i].Item3 == 2)
                        continue; // this is finished

                    var bird = _birdsLarge[i];
                    if (_chains[i].Item1 == null && bird.Tether.Target != 0)
                    {
                        Service.Log($"[P3S] Debug: assign chain bird {bird.InstanceID:X} -> {bird.Tether.Target:X}");
                        _chains[i].Item1 = _module.WorldState.FindActor(bird.Tether.Target);
                    }
                    if (_chains[i].Item2 == null && (_chains[i].Item1?.Tether.Target ?? 0) != 0)
                    {
                        Service.Log($"[P3S] Debug: assign chain bird {bird.InstanceID:X} -> ... -> {_chains[i].Item1!.Tether.Target:X}");
                        _chains[i].Item2 = _module.WorldState.FindActor(_chains[i].Item1!.Tether.Target);
                    }
                    if (_chains[i].Item3 == 0 && _chains[i].Item1 != null && bird.Tether.Target == 0)
                    {
                        Service.Log($"[P3S] Debug: first charge for {bird.InstanceID:X}");
                        _chains[i].Item3 = 1;
                    }
                    if (_chains[i].Item3 == 1 && (_chains[i].Item1 == null || _chains[i].Item1!.Tether.Target == 0))
                    {
                        Service.Log($"[P3S] Debug: second charge for {bird.InstanceID:X}");
                        _chains[i].Item3 = 2;
                    }
                }
            }

            public override void AddHints(int slot, WorldState.Actor actor, List<string> hints)
            {
                if (!Active)
                    return;

                bool hitByCharge = false;
                foreach ((var bird, (var p1, var p2, int numCharges)) in _birdsLarge.Zip(_chains))
                {
                    if (numCharges == 2)
                        continue;

                    var nextTarget = numCharges > 0 ? p2 : p1;
                    if (actor != nextTarget)
                    {
                        // check that player won't be hit by this charge
                        if (nextTarget != null && bird.Position != nextTarget.Position)
                        {
                            var fromTo = nextTarget.Position - bird.Position;
                            float len = fromTo.Length();
                            hitByCharge |= GeometryUtils.PointInRect(actor.Position - bird.Position, fromTo / len, len, 0, _chargeHalfWidth);
                        }
                    }
                    else
                    {
                        // check that tether is 'safe'
                        var tetherSource = numCharges > 0 ? p1 : bird;
                        if (tetherSource?.Tether.ID != (uint)TetherID.LargeBirdFar)
                        {
                            hints.Add("Too close!");
                        }
                    }
                }

                if (hitByCharge)
                {
                    hints.Add("GTFO from charge zone!");
                }
            }

            public override void DrawArenaBackground(MiniArena arena)
            {
                var pc = _module.Player();
                if (!Active || pc == null)
                    return;

                // draw aoe zones for imminent charges, except one towards player
                foreach ((var bird, (var p1, var p2, int numCharges)) in _birdsLarge.Zip(_chains))
                {
                    if (numCharges == 2)
                        continue;

                    var nextTarget = numCharges > 0 ? p2 : p1;
                    if (nextTarget != null && nextTarget != pc && nextTarget.Position != bird.Position)
                    {
                        var fromTo = nextTarget.Position - bird.Position;
                        float len = fromTo.Length();
                        arena.ZoneQuad(bird.Position, fromTo / len, len, 0, _chargeHalfWidth, arena.ColorAOE);
                    }
                }
            }

            public override void DrawArenaForeground(MiniArena arena)
            {
                var pc = _module.Player();
                if (!Active || pc == null)
                    return;

                // draw chains containing player
                foreach ((var bird, (var p1, var p2, int numCharges)) in _birdsLarge.Zip(_chains))
                {
                    if (numCharges == 2)
                        continue;

                    if (p1 == pc)
                    {
                        // bird -> pc -> other
                        if (numCharges == 0)
                        {
                            arena.AddLine(bird.Position, pc.Position, (bird.Tether.ID == (uint)TetherID.LargeBirdFar) ? arena.ColorSafe : arena.ColorDanger);
                            arena.Actor(bird, arena.ColorEnemy);
                            if (p2 != null)
                            {
                                arena.AddLine(pc.Position, p2.Position, (pc.Tether.ID == (uint)TetherID.LargeBirdFar) ? arena.ColorSafe : arena.ColorDanger);
                                arena.Actor(p2, arena.ColorPlayerGeneric);
                            }
                        }
                        // else: don't care, charge to pc already happened
                    }
                    else if (p2 == pc && p1 != null)
                    {
                        // bird -> other -> pc
                        if (numCharges == 0)
                        {
                            arena.AddLine(bird.Position, p1.Position, (bird.Tether.ID == (uint)TetherID.LargeBirdFar) ? arena.ColorSafe : arena.ColorDanger);
                            arena.AddLine(p1.Position, pc.Position, (p1.Tether.ID == (uint)TetherID.LargeBirdFar) ? arena.ColorSafe : arena.ColorDanger);
                            arena.Actor(bird, arena.ColorEnemy);
                            arena.Actor(p1, arena.ColorPlayerGeneric);
                        }
                        else
                        {
                            arena.AddLine(bird.Position, pc.Position, (p1.Tether.ID == (uint)TetherID.LargeBirdFar) ? arena.ColorSafe : arena.ColorDanger);
                            arena.Actor(bird, arena.ColorEnemy);
                        }
                    }
                }
            }
        }

        // state related to sunshadow tethers during fountain of fire mechanics
        private class SunshadowTether : Component
        {
            private P3S _module;
            private List<WorldState.Actor> _sunshadows;

            private static float _chargeHalfWidth = 3;

            public SunshadowTether(P3S module)
            {
                _module = module;
                _sunshadows = module.Enemies(OID.Sunshadow);
            }

            public override void AddHints(int slot, WorldState.Actor actor, List<string> hints)
            {
                foreach (var bird in _sunshadows)
                {
                    if (bird.Tether.Target == 0 || bird.Tether.Target == actor.InstanceID)
                        continue;

                    var target = _module.WorldState.FindActor(bird.Tether.Target);
                    if (target == null || bird.Position == target.Position)
                        continue;

                    if (GeometryUtils.PointInRect(actor.Position - bird.Position, Vector3.Normalize(target.Position - bird.Position), 50, 0, _chargeHalfWidth))
                    {
                        hints.Add("GTFO from charge zone!");
                        break;
                    }
                }
            }

            public override void DrawArenaBackground(MiniArena arena)
            {
                foreach (var bird in _sunshadows)
                {
                    if (bird.Tether.Target == 0 || bird.Tether.Target == _module.WorldState.PlayerActorID)
                        continue;

                    var target = _module.WorldState.FindActor(bird.Tether.Target);
                    if (target == null || bird.Position == target.Position)
                        continue;

                    var dir = Vector3.Normalize(target.Position - bird.Position);
                    arena.ZoneQuad(bird.Position, dir, 50, 0, _chargeHalfWidth, arena.ColorAOE);
                }
            }

            public override void DrawArenaForeground(MiniArena arena)
            {
                var pc = _module.Player();
                var myBird = _sunshadows.Find(bird => bird.Tether.Target == _module.WorldState.PlayerActorID);
                if (pc == null || myBird == null)
                    return;

                arena.AddLine(myBird.Position, pc.Position, arena.ColorSafe);
                arena.Actor(myBird, arena.ColorEnemy);
            }
        }

        // state related to flames of asphodelos mechanic (note that it is activated and deactivated based on helper casts rather than states due to tricky overlaps)
        private class FlamesOfAsphodelos : Component
        {
            private P3S _module;
            private float?[] _directions = new float?[3];

            public FlamesOfAsphodelos(P3S module)
            {
                _module = module;
            }

            public override void Reset()
            {
                Array.Fill(_directions, null);
            }

            public override void AddHints(int slot, WorldState.Actor actor, List<string> hints)
            {
                if (InAOE(_directions[1], actor.Position) || InAOE(_directions[0] != null ? _directions[0] : _directions[2], actor.Position))
                {
                    hints.Add("GTFO from cone!");
                }
            }

            public override void DrawArenaBackground(MiniArena arena)
            {
                if (_directions[0] != null)
                {
                    DrawZone(arena, _directions[0], arena.ColorDanger);
                    DrawZone(arena, _directions[1], arena.ColorAOE);
                }
                else
                {
                    DrawZone(arena, _directions[1], arena.ColorAOE);
                    DrawZone(arena, _directions[2], arena.ColorAOE);
                }
            }

            public override void OnCastStarted(WorldState.Actor actor)
            {
                switch ((AID)actor.CastInfo!.ActionID)
                {
                    case AID.FlamesOfAsphodelosAOE1:
                        _directions[0] = actor.Rotation;
                        break;
                    case AID.FlamesOfAsphodelosAOE2:
                        _directions[1] = actor.Rotation;
                        break;
                    case AID.FlamesOfAsphodelosAOE3:
                        _directions[2] = actor.Rotation;
                        break;
                }
            }

            public override void OnCastFinished(WorldState.Actor actor)
            {
                switch ((AID)actor.CastInfo!.ActionID)
                {
                    case AID.FlamesOfAsphodelosAOE1:
                        _directions[0] = null;
                        break;
                    case AID.FlamesOfAsphodelosAOE2:
                        _directions[1] = null;
                        break;
                    case AID.FlamesOfAsphodelosAOE3:
                        _directions[2] = null;
                        break;
                }
            }

            private void DrawZone(MiniArena arena, float? dir, uint color)
            {
                if (dir != null)
                {
                    arena.ZoneIsoscelesTri(arena.WorldCenter, dir.Value, MathF.PI / 6, 50, color);
                    arena.ZoneIsoscelesTri(arena.WorldCenter, dir.Value + MathF.PI, MathF.PI / 6, 50, color);
                }
            }

            private bool InAOE(float? dir, Vector3 pos)
            {
                if (dir == null)
                    return false;

                var toPos = Vector3.Normalize(pos - _module.Arena.WorldCenter);
                var toDir = GeometryUtils.DirectionToVec3(dir.Value);
                return MathF.Abs(Vector3.Dot(toPos, toDir)) >= MathF.Cos(MathF.PI / 6);
            }
        }

        // state related to storms of asphodelos mechanics
        private class StormsOfAsphodelos : Component
        {
            public bool Active = false;

            private P3S _module;
            private List<WorldState.Actor> _twisters;
            private List<int> _twisterTargets = new();
            private ulong _tetherTargets = 0;
            private ulong _bossTargets = 0;
            private ulong _closeToTetherTarget = 0;
            private ulong _hitByMultipleAOEs = 0;

            private static float _coneHalfAngle = MathF.PI / 12; // not sure about this!!!
            private static float _beaconRadius = 6;

            public StormsOfAsphodelos(P3S module)
            {
                _module = module;
                _twisters = module.Enemies(OID.DarkblazeTwister);
            }

            public override void Reset() => Active = false;

            public override void Update()
            {
                _twisterTargets.Clear();
                _tetherTargets = _bossTargets = _closeToTetherTarget = _hitByMultipleAOEs = 0;
                var boss = _module.Boss();
                if (!Active || boss == null)
                    return;

                // we determine failing players, trying to take two reasonable tactics in account:
                // either two tanks immune and soak everything, or each player is hit by one mechanic
                // for now, we consider tether target to be a "tank"
                int[] aoesPerPlayer = new int[_module.RaidMembers.Length];

                foreach ((int i, var player) in _module.IterateRaidMembers(true).Where(indexPlayer => indexPlayer.Item2.Tether.Target == boss.InstanceID))
                {
                    BitVector.SetVector64Bit(ref _tetherTargets, i);

                    ++aoesPerPlayer[i];
                    foreach ((int j, var other) in _module.IterateRaidMembersInRange(i, _beaconRadius))
                    {
                        ++aoesPerPlayer[j];
                        BitVector.SetVector64Bit(ref _closeToTetherTarget, j);
                    }
                }

                float cosHalfAngle = MathF.Cos(_coneHalfAngle);
                foreach ((int i, var player) in FindClosest(boss.Position).Take(3))
                {
                    BitVector.SetVector64Bit(ref _bossTargets, i);
                    foreach ((int j, var other) in FindPlayersInWinds(boss.Position, player, cosHalfAngle))
                    {
                        ++aoesPerPlayer[j];
                    }
                }

                foreach (var twister in _twisters)
                {
                    (var i, var player) = FindClosest(twister.Position).FirstOrDefault();
                    if (player == null)
                    {
                        _twisterTargets.Add(-1);
                        continue;
                    }

                    _twisterTargets.Add(i);
                    foreach ((int j, var other) in FindPlayersInWinds(twister.Position, player, cosHalfAngle))
                    {
                        ++aoesPerPlayer[j];
                    }
                }

                for (int i = 0; i < aoesPerPlayer.Length; ++i)
                    if (aoesPerPlayer[i] > 1)
                        BitVector.SetVector64Bit(ref _hitByMultipleAOEs, i);
            }

            public override void AddHints(int slot, WorldState.Actor actor, List<string> hints)
            {
                bool tethered = BitVector.IsVector64BitSet(_tetherTargets, slot);
                bool hitByMultipleAOEs = BitVector.IsVector64BitSet(_hitByMultipleAOEs, slot);
                if (actor.Role == WorldState.ActorRole.Tank)
                {
                    if (!tethered)
                    {
                        hints.Add("Intercept tether!");
                    }
                    if (hitByMultipleAOEs)
                    {
                        hints.Add("Press invul!");
                    }
                }
                else
                {
                    if (tethered)
                    {
                        hints.Add("Pass the tether!");
                    }
                    if (hitByMultipleAOEs)
                    {
                        hints.Add("GTFO from aoes!");
                    }
                }
                if (BitVector.IsVector64BitSet(_closeToTetherTarget, slot))
                {
                    hints.Add("GTFO from tether!");
                }
            }

            public override void DrawArenaBackground(MiniArena arena)
            {
                var boss = _module.Boss();
                if (!Active || boss == null)
                    return;

                foreach ((int i, var player) in _module.IterateRaidMembers())
                {
                    if (BitVector.IsVector64BitSet(_tetherTargets, i))
                    {
                        arena.ZoneCircle(player.Position, _beaconRadius, arena.ColorAOE);
                    }
                    if (BitVector.IsVector64BitSet(_bossTargets, i) && player.Position != boss.Position)
                    {
                        var offset = player.Position - boss.Position;
                        float phi = MathF.Atan2(offset.X, offset.Z);
                        arena.ZoneCone(boss.Position, 0, 50, phi - _coneHalfAngle, phi + _coneHalfAngle, arena.ColorAOE);
                    }
                }

                foreach ((var twister, int i) in _twisters.Zip(_twisterTargets))
                {
                    var player = _module.RaidMember(i); // not sure if twister could really have invalid target, but let's be safe...
                    if (player == null || player.Position == twister.Position)
                        continue;

                    var offset = player.Position - twister.Position;
                    float phi = MathF.Atan2(offset.X, offset.Z);
                    arena.ZoneCone(twister.Position, 0, 50, phi - _coneHalfAngle, phi + _coneHalfAngle, arena.ColorAOE);
                }
            }

            public override void DrawArenaForeground(MiniArena arena)
            {
                if (!Active)
                    return;

                foreach ((int i, var player) in _module.IterateRaidMembers())
                {
                    if (i != _module.PlayerSlot)
                    {
                        bool active = BitVector.IsVector64BitSet(_tetherTargets | _bossTargets, i) || _twisterTargets.Contains(i);
                        bool failing = BitVector.IsVector64BitSet(_hitByMultipleAOEs | _closeToTetherTarget, i);
                        arena.Actor(player, active ? arena.ColorDanger : (failing ? arena.ColorPlayerInteresting : arena.ColorPlayerGeneric));
                    }
                }
            }

            private IEnumerable<(int, WorldState.Actor)> FindClosest(Vector3 position)
            {
                return _module.IterateRaidMembers()
                    .Select(indexPlayer => (indexPlayer.Item1, indexPlayer.Item2, (indexPlayer.Item2.Position - position).LengthSquared()))
                    .OrderBy(indexPlayerDist => indexPlayerDist.Item3)
                    .Select(indexPlayerDist => (indexPlayerDist.Item1, indexPlayerDist.Item2));
            }

            private IEnumerable<(int, WorldState.Actor)> FindPlayersInWinds(Vector3 origin, WorldState.Actor target, float cosHalfAngle)
            {
                var dir = Vector3.Normalize(target.Position - origin);
                foreach ((int i, var player) in _module.IterateRaidMembers())
                {
                    var playerDir = Vector3.Normalize(player.Position - origin);
                    if (Vector3.Dot(dir, playerDir) >= cosHalfAngle)
                        yield return (i, player);
                }
            }
        }

        // state related to darkblaze twister mechanics
        private class DarkblazeTwister : Component
        {
            public enum State { None, Knockback, AOE }
            public State CurState = State.None;

            private P3S _module;
            private List<WorldState.Actor> _twisters;

            private static float _knockbackRange = 17;
            private static float _aoeInnerRadius = 7; // not sure about this...
            private static float _aoeOuterRadius = 20;

            public DarkblazeTwister(P3S module)
            {
                _module = module;
                _twisters = module.Enemies(OID.DarkblazeTwister);
            }

            public override void Reset() => CurState = State.None;

            public override void AddHints(int slot, WorldState.Actor actor, List<string> hints)
            {
                if (CurState == State.None)
                    return;

                var adjPos = GetAdjustedActorPosition(actor);
                if (CurState == State.Knockback && !_module.Arena.InBounds(adjPos))
                {
                    hints.Add("About to be knocked back into wall!");
                }

                foreach (var twister in _twisters.Where(twister => twister.CastInfo?.ActionID == (uint)AID.BurningTwister))
                {
                    var offset = adjPos - twister.Position;
                    if (GeometryUtils.PointInCircle(adjPos, _aoeOuterRadius) && !GeometryUtils.PointInCircle(adjPos, _aoeInnerRadius))
                    {
                        hints.Add("GTFO from aoe!");
                        break;
                    }
                }
            }

            public override void DrawArenaBackground(MiniArena arena)
            {
                if (CurState == State.None)
                    return;

                foreach (var twister in _twisters.Where(twister => twister.CastInfo?.ActionID == (uint)AID.BurningTwister))
                {
                    arena.ZoneCone(twister.Position, _aoeInnerRadius, _aoeOuterRadius, 0, 2 * MathF.PI, arena.ColorAOE);
                }
            }

            public override void DrawArenaForeground(MiniArena arena)
            {
                var pc = _module.Player();
                if (CurState != State.Knockback || pc == null)
                    return;

                var adjPos = GetAdjustedActorPosition(pc);
                if (adjPos != pc.Position)
                {
                    arena.AddLine(pc.Position, adjPos, arena.ColorDanger);
                    arena.Actor(adjPos, pc.Rotation, arena.ColorDanger);
                }
            }

            private Vector3 GetAdjustedActorPosition(WorldState.Actor actor)
            {
                return CurState == State.Knockback ? AdjustPositionForKnockback(actor.Position, _twisters.Find(twister => twister.CastInfo?.ActionID == (uint)AID.DarkTwister), _knockbackRange) : actor.Position;
            }
        }

        // state related to fledgling flight & death toll mechanics
        private class FledglingFlight : Component
        {
            public enum State { None, Players, Sparkfledgeds }

            private P3S _module;
            private State _curState = State.None;
            private List<(WorldState.Actor, float)> _sources = new(); // actor + rotation

            private static float _coneHalfAngle = MathF.PI / 8; // not sure about this

            public FledglingFlight(P3S module)
            {
                _module = module;
            }

            public override void Reset()
            {
                _curState = State.None;
                _sources.Clear();
            }

            public override void AddHints(int slot, WorldState.Actor actor, List<string> hints)
            {
                if (_sources.Count == 0)
                    return;

                int aoeCount = _sources.Where(srcRot => GeometryUtils.PointInCone(actor.Position - srcRot.Item1.Position, srcRot.Item2, _coneHalfAngle)).Count();
                int deathTollStacks = actor.FindStatus((uint)SID.DeathsToll)?.StackCount ?? 0;
                if (aoeCount < deathTollStacks)
                {
                    hints.Add($"Enter more aoes ({aoeCount}/{deathTollStacks})!");
                }
                else if (aoeCount > deathTollStacks)
                {
                    hints.Add($"GTFO from eyes ({aoeCount}/{deathTollStacks})!");
                }
            }

            public override void DrawArenaBackground(MiniArena arena)
            {
                foreach ((var source, var dir) in _sources)
                {
                    arena.ZoneIsoscelesTri(source.Position, dir, _coneHalfAngle, 50, arena.ColorAOE);
                }
            }

            public override void OnCastStarted(WorldState.Actor actor)
            {
                if ((AID)actor.CastInfo!.ActionID == AID.AshenEye)
                {
                    if (_curState != State.Sparkfledgeds)
                    {
                        _curState = State.Players;
                        _sources.Clear();
                    }
                    _sources.Add((actor, actor.Rotation));
                }
            }

            public override void OnCastFinished(WorldState.Actor actor)
            {
                if ((AID)actor.CastInfo!.ActionID == AID.AshenEye)
                {
                    int index = _sources.FindIndex(x => x.Item1 == actor);
                    if (index < 0)
                        return;

                    _sources.RemoveAt(index);
                    if (_sources.Count == 0)
                    {
                        Reset();
                    }
                }
            }

            public override void OnEventIcon(uint actorID, uint iconID)
            {
                if (iconID >= 296 && iconID <= 299)
                {
                    if (_curState != State.Players)
                    {
                        _curState = State.Players;
                        _sources.Clear();
                    }

                    var actor = _module.WorldState.FindActor(actorID);
                    if (actor != null)
                    {
                        // 296 -> E, 297 -> W, 298 -> S, 299 -> N
                        uint directionIndex = iconID - 296;
                        float dir = (directionIndex < 2 ? MathF.PI / 2 : 0) + directionIndex * MathF.PI;
                        _sources.Add((actor, dir));
                    }
                }
            }
        }

        private List<WorldState.Actor> _boss;
        private WorldState.Actor? Boss() => _boss.FirstOrDefault();

        public P3S(WorldState ws)
            : base(ws, 8)
        {
            _boss = RegisterEnemies(OID.Boss, true);
            RegisterEnemies(OID.DarkenedFire);
            RegisterEnemies(OID.SunbirdSmall);
            RegisterEnemies(OID.SunbirdLarge);
            RegisterEnemies(OID.Sunshadow);
            RegisterEnemies(OID.DarkblazeTwister);

            RegisterComponent(new HeatOfCondemnation(this));
            RegisterComponent(new Cinderwing(this));
            RegisterComponent(new DevouringBrand(this));
            RegisterComponent(new TrailOfCondemnation(this));
            RegisterComponent(new FireplumeSingle(this));
            RegisterComponent(new FireplumeMulti(this));
            RegisterComponent(new Ashplume(this));
            RegisterComponent(new DarkenedFire(this));
            RegisterComponent(new BrightenedFire(this));
            RegisterComponent(new BirdDistance(this));
            RegisterComponent(new BirdTether(this));
            RegisterComponent(new SunshadowTether(this));
            RegisterComponent(new FlamesOfAsphodelos(this));
            RegisterComponent(new StormsOfAsphodelos(this));
            RegisterComponent(new DarkblazeTwister(this));
            RegisterComponent(new FledglingFlight(this));

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

        protected override void DrawArenaForegroundPost()
        {
            Arena.Actor(Boss(), Arena.ColorEnemy);
            Arena.Actor(Player(), Arena.ColorPC);
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
            cast.Enter = () => FindComponent<HeatOfCondemnation>()!.Active = true;
            cast.Exit = () => FindComponent<HeatOfCondemnation>()!.Active = false;
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
            dispatch[AID.ExperimentalAshplumeStack] = new(null, () => FindComponent<Ashplume>()!.CurState = Ashplume.State.Stack);
            dispatch[AID.ExperimentalAshplumeSpread] = new(null, () => FindComponent<Ashplume>()!.CurState = Ashplume.State.Spread);
            var start = CommonStates.CastStart(ref link, Boss, dispatch, delay);
            var end = CommonStates.CastEnd(ref start.Next, Boss, 5, "Ashplume");
            end.EndHint |= StateMachine.StateHint.GroupWithNext;
            return end;
        }

        // note: automatically clears positioning flag
        private StateMachine.State BuildExperimentalAshplumeResolveState(ref StateMachine.State? link, float delay)
        {
            var comp = FindComponent<Ashplume>()!;
            var resolve = CommonStates.Condition(ref link, delay, () => comp.CurState == Ashplume.State.None, "Ashplume resolve");
            resolve.Exit = () => comp.CurState = Ashplume.State.None;
            resolve.EndHint |= StateMachine.StateHint.Raidwide | StateMachine.StateHint.PositioningEnd;
            return resolve;
        }

        private StateMachine.State BuildExperimentalGloryplumeMultiState(ref StateMachine.State? link, float delay)
        {
            var comp = FindComponent<Ashplume>()!;
            // first part for this mechanic always seems to be "multi-plume", works just like fireplume
            // 9 helpers teleport to position, first pair almost immediately starts casting 26315s, 1 sec stagger between pairs, 7 sec for each cast
            // ~3 sec after cast ends, boss makes an instant cast that determines stack/spread (26316/26312), ~10 sec after that hits with real AOE (26317/26313)
            // note that our helpers rely on casts rather than states
            var cast = CommonStates.Cast(ref link, Boss, AID.ExperimentalGloryplumeMulti, delay, 5, "GloryplumeMulti");
            cast.Exit = () => comp.CurState = Ashplume.State.UnknownGlory; // instant cast turns this into correct state in ~3 sec
            cast.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;

            var resolve = CommonStates.Condition(ref cast.Next, 13, () => comp.CurState == Ashplume.State.None, "Gloryplume resolve");
            resolve.Exit = () => comp.CurState = Ashplume.State.None;
            resolve.EndHint |= StateMachine.StateHint.Raidwide | StateMachine.StateHint.PositioningEnd;
            return resolve;
        }

        private StateMachine.State BuildExperimentalGloryplumeSingleState(ref StateMachine.State? link, float delay)
        {
            var comp = FindComponent<Ashplume>()!;
            // first part for this mechanic always seems to be "single-plume", works just like fireplume
            // helper teleports to position, almost immediately starts casting 26311, 6 sec for cast
            // ~3 sec after cast ends, boss makes an instant cast that determines stack/spread (26316/26312), ~4 sec after that hits with real AOE (26317/26313)
            // note that our helpers rely on casts rather than states
            var cast = CommonStates.Cast(ref link, Boss, AID.ExperimentalGloryplumeSingle, delay, 5, "GloryplumeSingle");
            cast.Exit = () => comp.CurState = Ashplume.State.UnknownGlory; // instant cast turns this into correct state in ~3 sec
            cast.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;

            var resolve = CommonStates.Condition(ref cast.Next, 7.2f, () => comp.CurState == Ashplume.State.None, "Gloryplume resolve");
            resolve.Exit = () => comp.CurState = Ashplume.State.None;
            resolve.EndHint |= StateMachine.StateHint.Raidwide | StateMachine.StateHint.PositioningEnd;
            return resolve;
        }

        private StateMachine.State BuildCinderwingState(ref StateMachine.State? link, float delay)
        {
            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.RightCinderwing] = new(null, () => FindComponent<Cinderwing>()!.CurState = Cinderwing.State.Right);
            dispatch[AID.LeftCinderwing] = new(null, () => FindComponent<Cinderwing>()!.CurState = Cinderwing.State.Left);
            var start = CommonStates.CastStart(ref link, Boss, dispatch, delay);

            var end = CommonStates.CastEnd(ref start.Next, Boss, 5, "Wing");
            end.Exit = () => FindComponent<Cinderwing>()!.CurState = Cinderwing.State.None;
            return end;
        }

        private StateMachine.State BuildDevouringBrandState(ref StateMachine.State? link, float delay)
        {
            var devouring = CommonStates.Cast(ref link, Boss, AID.DevouringBrand, delay, 3, "DevouringBrand");
            devouring.EndHint |= StateMachine.StateHint.GroupWithNext;

            var fireplume = BuildExperimentalFireplumeState(ref devouring.Next, 2); // pos-start
            fireplume.EndHint |= StateMachine.StateHint.GroupWithNext;
            fireplume.Exit = () => FindComponent<DevouringBrand>()!.Active = true;

            var breeze = CommonStates.Cast(ref fireplume.Next, Boss, AID.SearingBreeze, 7, 3, "SearingBreeze");
            breeze.EndHint |= StateMachine.StateHint.GroupWithNext;

            var wing = BuildCinderwingState(ref breeze.Next, 3);
            wing.Enter = () => FindComponent<DevouringBrand>()!.Active = false;
            wing.EndHint |= StateMachine.StateHint.PositioningEnd;
            return wing;
        }

        private StateMachine.State BuildDarkenedFireState(ref StateMachine.State? link, float delay)
        {
            // 3s after cast ends, adds start casting 26299
            var addsStart = CommonStates.CastStart(ref link, Boss, AID.DarkenedFire, delay);
            addsStart.Exit = () => FindComponent<DarkenedFire>()!.Active = true;
            addsStart.EndHint |= StateMachine.StateHint.PositioningStart;

            var addsEnd = CommonStates.CastEnd(ref addsStart.Next, Boss, 6, "DarkenedFire adds");
            addsEnd.Exit = () =>
            {
                FindComponent<DarkenedFire>()!.Active = false;
                // TODO: debug, remove...
                foreach ((_, var player) in IterateRaidMembers())
                    Service.Log($"[P3S] Debug: {player.InstanceID:X} at {Utils.Vec3String(player.Position)}");
            };
            addsEnd.EndHint |= StateMachine.StateHint.GroupWithNext;

            var brightenedComp = FindComponent<BrightenedFire>()!;
            var numbers = CommonStates.Cast(ref addsEnd.Next, Boss, AID.BrightenedFire, 5, 5, "Numbers"); // numbers appear at the beginning of the cast, at the end he starts shooting 1-8
            numbers.Enter = () => brightenedComp.Active = true;
            numbers.EndHint |= StateMachine.StateHint.GroupWithNext;

            var lastAOE = CommonStates.Condition(ref numbers.Next, 8.4f, () => brightenedComp.NumCastsHappened == 8);
            lastAOE.Exit = () => brightenedComp.Reset();

            var resolve = CommonStates.Timeout(ref lastAOE.Next, 6.6f, "DarkenedFire resolve");
            resolve.EndHint |= StateMachine.StateHint.PositioningEnd;
            return resolve;
        }

        private StateMachine.State BuildTrailOfCondemnationState(ref StateMachine.State? link, float delay)
        {
            // at this point boss teleports to one of the cardinals
            // parallel to this one of the helpers casts 26365 (actual aoe fire trails)
            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.TrailOfCondemnationCenter] = new(null, () => FindComponent<TrailOfCondemnation>()!.CurState = TrailOfCondemnation.State.Center);
            dispatch[AID.TrailOfCondemnationSides] = new(null, () => FindComponent<TrailOfCondemnation>()!.CurState = TrailOfCondemnation.State.Sides);
            var start = CommonStates.CastStart(ref link, Boss, dispatch, delay);
            var end = CommonStates.CastEnd(ref start.Next, Boss, 6);
            var resolve = CommonStates.Timeout(ref end.Next, 2, "Trail");
            resolve.Exit = () => FindComponent<TrailOfCondemnation>()!.CurState = TrailOfCondemnation.State.None;
            return resolve;
        }

        // note: expects downtime at enter, clears when birds spawn, reset when birds die
        private StateMachine.State BuildSmallBirdsState(ref StateMachine.State? link, float delay)
        {
            var birds = Enemies(OID.SunbirdSmall);

            var spawn = CommonStates.Simple(ref link, delay, "Small birds");
            spawn.Update = (_) => spawn.Done = birds.Count > 0;
            spawn.Exit = () => FindComponent<BirdDistance>()!.WatchedBirds = birds;
            spawn.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.DowntimeEnd; // adds become targetable <1sec after spawn

            var enrage = CommonStates.Simple(ref spawn.Next, 25, "Small birds enrage");
            enrage.Update = (_) => enrage.Done = birds.Find(x => !x.IsDead) == null;
            enrage.Exit = () => FindComponent<BirdDistance>()!.WatchedBirds = null;
            enrage.EndHint |= StateMachine.StateHint.Raidwide | StateMachine.StateHint.DowntimeStart; // raidwide (26326) happens ~3sec after last bird death
            return enrage;
        }

        // note: expects downtime at enter, clears when birds spawn, reset when birds die
        private StateMachine.State BuildLargeBirdsState(ref StateMachine.State? link, float delay)
        {
            var birds = Enemies(OID.SunbirdLarge);

            var spawn = CommonStates.Simple(ref link, delay, "Large birds");
            spawn.Update = (_) => spawn.Done = birds.Count > 0;
            spawn.Exit = () =>
            {
                // tethers appear ~5s after this and last for ~11s
                FindComponent<BirdDistance>()!.WatchedBirds = birds;
                FindComponent<BirdTether>()!.Active = true;
            };
            spawn.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.DowntimeEnd; // adds become targetable ~1sec after spawn

            var enrage = CommonStates.Simple(ref spawn.Next, 55, "Large birds enrage");
            enrage.Update = (_) => enrage.Done = birds.Find(x => !x.IsDead) == null;
            enrage.Exit = () =>
            {
                FindComponent<BirdDistance>()!.WatchedBirds = null;
                FindComponent<BirdTether>()!.Active = false;
            };
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
            var state = CommonStates.Cast(ref link, Boss, AID.StormsOfAsphodelos, delay, 8, "Storms");
            state.Enter = () => FindComponent<StormsOfAsphodelos>()!.Active = true;
            state.Exit = () => FindComponent<StormsOfAsphodelos>()!.Active = false;
            state.EndHint |= StateMachine.StateHint.Raidwide | StateMachine.StateHint.Tankbuster;
            return state;
        }

        private StateMachine.State BuildDarkblazeTwisterState(ref StateMachine.State? link, float delay)
        {
            var twisters = Enemies(OID.DarkblazeTwister);

            var twister = CommonStates.Cast(ref link, Boss, AID.DarkblazeTwister, delay, 4, "Twister");
            twister.Exit = () => FindComponent<DarkblazeTwister>()!.CurState = DarkblazeTwister.State.Knockback;
            twister.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;

            var breeze = CommonStates.Cast(ref twister.Next, Boss, AID.SearingBreeze, 4, 3, "SearingBreeze");
            breeze.EndHint |= StateMachine.StateHint.GroupWithNext;

            var ashplume = BuildExperimentalAshplumeCastState(ref breeze.Next, 4);

            var knockback = CommonStates.Simple(ref ashplume.Next, 3, "Knockback");
            knockback.Update = (_) => knockback.Done = !twisters.Any(twister => twister.CastInfo?.ActionID == (uint)AID.DarkTwister);
            knockback.Exit = () => FindComponent<DarkblazeTwister>()!.CurState = DarkblazeTwister.State.AOE;
            knockback.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.Knockback;

            var aoe = CommonStates.Simple(ref knockback.Next, 2, "AOE");
            aoe.Update = (_) => aoe.Done = !twisters.Any(twister => twister.CastInfo != null);
            aoe.Exit = () => FindComponent<DarkblazeTwister>()!.CurState = DarkblazeTwister.State.None;
            aoe.EndHint |= StateMachine.StateHint.GroupWithNext;

            var resolve = BuildExperimentalAshplumeResolveState(ref aoe.Next, 2);
            return resolve;
        }
    }
}
