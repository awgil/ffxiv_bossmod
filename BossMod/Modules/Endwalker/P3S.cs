using ImGuiNET;
using System;
using System.Collections.Generic;

namespace BossMod
{
    public class P3S : BossModule
    {
        public enum OID : uint
        {
            Boss = 0x353F,
            Sparkfledged = 0x3540, // spawned mid fight - these are "eyes"
            SunbirdSmall = 0x3541, // spawned mid fight
            SunbirdBig = 0x3543, // spawned mid fight
            Sunshadow = 0x3544, // spawned mid fight, mini birds that charge during fountains of fire
            DarkenedFire = 0x3545, // spawned mid fight
            FountainOfFire = 0x3546, // spawned mid fight, towers that healers soak
            DarkblazeTwister = 0x3547, // spawned mid fight
            SparkfledgedUnknown = 0x3800, // spawned mid fight, have weird kind...
            Helper = 0x233C, // x45
        };

        public enum AID : uint
        {
            FledglingFlight = 26282, // Boss->Boss
            DarkenedFire = 26297, // Boss->Boss
            DarkenedBlaze = 26299, // DarkenedFire->DarkenedFire, 22sec cast
            BrightenedFire = 26300, // Boss->Boss
            ExperimentalFireplumeSingle = 26302, // Boss->Boss
            ExperimentalFireplumeSingleAOE = 26303, // Helper->Helper
            ExperimentalFireplumeMulti = 26304, // Boss->Boss
            ExperimentalFireplumeMultiAOE = 26305, // Helper->Helper
            ExperimentalAshplumeStack = 26306, // Boss->Boss
            ExperimentalAshplumeStackAOE = 26307, // Helper->targets, no cast
            ExperimentalAshplumeSpread = 26308, // Boss->Boss
            ExperimentalAshplumeSpreadAOE = 26309, // Helper->targets, no cast, 7sec after cast end
            ExperimentalGloryplume = 26314, // Boss->Boss (?? this is multi+stack variant)
            ExperimentalGloryplumeAOE = 26315, // Helper->Helper
            ExperimentalGloryplumeStack = 26316, // Boss->Boss, no cast (stack variant??)
            ExperimentalGloryplumeStackAOE = 26317, // Helper->target, no cast, actual damage
            DevouringBrand = 26318, // Boss->Boss
            DevouringBrandMiniAOE = 26319, // Helper->Helper (ones standing on cardinals)
            GreatWhirlwind = 26325, // SunbirdBig->SunbirdBig (enrage)
            FlamesOfUndeath = 26326, // Boss->Boss, no cast - aoe when small or big birds all die (?)
            FireglideSweep = 26336, // SunbirdBig->SunbirdBig (charges)
            DeadRebirth = 26340, // Boss->Boss
            AshenEye = 26342, // Sparkfledged->Sparkfledged, eye cone
            FountainOfFire = 26343, // Boss->Boss
            SunsPinion = 26346, // Boss->Boss
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
            TrailOfCondemnation = 26363, // Boss->Boss (central aoe variant - spread)
            TrailOfCondemnationAOE = 26365, // Helper->Helper (actual aoe that hits those who fail the mechanic)
            FlareOfCondemnation = 26366, // Helper->target, no cast, hit and apply fire resist debuff
            HeatOfCondemnation = 26368, // Boss->Boss
            RightCinderwing = 26370, // Boss->Boss
            LeftCinderwing = 26371, // Boss->Boss
            SearingBreeze = 26372, // Boss->Boss
            SearingBreezeAOE = 36373, // Helper->Helper
            ScorchedExaltation = 26374, // Boss->Boss
            DevouringBrandAOE = 28035, // Helper->Helper (one in the center), 20sec cast
        };

        private WorldState.Actor? _boss;

        public P3S(WorldState ws)
            : base(ws, 8)
        {
            StateMachine.State? s;
            s = BuildScorchedExaltationState(ref InitialState, 8);
            s = BuildHeatOfCondemnationState(ref s.Next, 3);

            s = BuildExperimentalFireplumeState(ref s.Next, 6);
            s.EndHint |= StateMachine.StateHint.GroupWithNext;
            s = BuildCinderwingState(ref s.Next, 6);
            s.EndHint |= StateMachine.StateHint.PositioningEnd; // TODO: should it be set for whole duration?..

            s = BuildDarkenedFireState(ref s.Next, 8);
            s = BuildHeatOfCondemnationState(ref s.Next, 7);
            s = BuildScorchedExaltationState(ref s.Next, 3);

            s = CommonStates.Cast(ref s.Next, () => _boss, AID.DevouringBrand, 7, 3, "DevouringBrand");
            s.EndHint |= StateMachine.StateHint.GroupWithNext;
            s = BuildExperimentalFireplumeState(ref s.Next, 2);
            s.EndHint |= StateMachine.StateHint.GroupWithNext;
            s = CommonStates.Cast(ref s.Next, () => _boss, AID.SearingBreeze, 7, 3, "SearingBreeze");
            s.EndHint |= StateMachine.StateHint.GroupWithNext;
            s = BuildCinderwingState(ref s.Next, 3);
            s.EndHint |= StateMachine.StateHint.PositioningEnd;

            s = BuildHeatOfCondemnationState(ref s.Next, 3);

            s = BuildExperimentalFireplumeState(ref s.Next, 3);
            s = CommonStates.Timeout(ref s.Next, 4);
            s.EndHint |= StateMachine.StateHint.DowntimeStart; // flies away

            // at this point boss teleports to one of the cardinals
            // TODO: determine stack/spread
            // parallel to this one of the helpers casts 26365 (actual aoe for those who fail?)
            s = CommonStates.Cast(ref s.Next, () => _boss, AID.TrailOfCondemnation, 4, 6, "Trail");

            s = CommonStates.Timeout(ref s.Next, 9, "Adds");
            s.EndHint |= StateMachine.StateHint.DowntimeEnd; // adds become targetable - this is ~1sec after spawn
            // big adds spawn ~25sec after small adds -or- ~3sec after last small dies (?), then ~1sec later become targetable, then ~3sec later start casting 26336 (11 sec cast)
            // tethers: sunbird gets ActorControl_Tether with param3=mid player, mid player gets ActorControl_Tether with param3=end player
            // 44 sec after 26336 cast ends, live adds start casting 26325 (3 sec cast), at the end if still alive wipe the raid
            // TODO: add states showing 'enrages' here, transition by add deaths, with raidwide AOE flags...
            // TODO: tether helper !!!

            s = CommonStates.Cast(ref s.Next, () => _boss, AID.DeadRebirth, 90, 10, "DeadRebirth");
            s.EndHint |= StateMachine.StateHint.Raidwide | StateMachine.StateHint.PositioningEnd; // TODO: move positioning-end somewhere above...
            s = BuildHeatOfCondemnationState(ref s.Next, 9);
            s = BuildFledglingFlightState(ref s.Next, 8);
            s = BuildExperimentalGloryplumeState(ref s.Next, 14);

            // TODO: complex mechanic, think what kind of helper is needed here...
            s = CommonStates.Cast(ref s.Next, () => _boss, AID.FountainOfFire, 12, 6, "FountainOfFire");
            s.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;
            s = CommonStates.Cast(ref s.Next, () => _boss, AID.SunsPinion, 2, 6, 13, "Charges");
            s.EndHint |= StateMachine.StateHint.PositioningEnd;

            s = BuildScorchedExaltationState(ref s.Next, 5);
            s = BuildScorchedExaltationState(ref s.Next, 2);
            s = BuildHeatOfCondemnationState(ref s.Next, 5);

            s = CommonStates.Cast(ref s.Next, () => _boss, AID.FirestormsOfAsphodelos, 10, 5, "Firestorm");
            s.EndHint |= StateMachine.StateHint.Raidwide;
            // 1s after cast end: 3 tornadoes spawn

            // flames mechanics: ~1s after cast ends, 6 helpers start casting their cones at the same time, since they have different cast time, they finish staggered - just look at caster rotation, ez
            s = CommonStates.Cast(ref s.Next, () => _boss, AID.FlamesOfAsphodelos, 3, 3, "Cones");
            s.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;
            s = BuildExperimentalAshplumeState(ref s.Next, 2);
            s.EndHint |= StateMachine.StateHint.PositioningEnd;

            s = CommonStates.Cast(ref s.Next, () => _boss, AID.FlamesOfAsphodelos, 2, 3, 9, "Cones");

            // the most complex mechanic, helper definitely needed...
            s = CommonStates.Cast(ref s.Next, () => _boss, AID.StormsOfAsphodelos, 1, 8, "Storms");
            s.EndHint |= StateMachine.StateHint.Raidwide | StateMachine.StateHint.Tankbuster;

            // twister: ~2 sec after cast ends, 3 twisters start their casts: 1x dark (17sec cast) and 2x burning (19 sec cast)
            s = CommonStates.Cast(ref s.Next, () => _boss, AID.DarkblazeTwister, 2, 4, "Twister");
            s.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;
            s = CommonStates.Cast(ref s.Next, () => _boss, AID.SearingBreeze, 4, 3, "SearingBreeze");
            s.EndHint |= StateMachine.StateHint.GroupWithNext;
            s = BuildExperimentalAshplumeState(ref s.Next, 4); // note: there is twister resolve before ashplume resolve...
            s.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningEnd;

            s = BuildScorchedExaltationState(ref s.Next, 3);
            s = CommonStates.Simple(ref s.Next, 2, "?????");
        }

        protected override void DrawArena()
        {
            Arena.Border();
        }

        protected override void NonPlayerCreated(WorldState.Actor actor)
        {
            if ((OID)actor.OID == OID.Boss)
            {
                _boss = actor;
            }
        }

        protected override void NonPlayerDestroyed(WorldState.Actor actor)
        {
            if (_boss == actor)
            {
                _boss = null;
            }
        }

        protected override void Reset()
        {
        }

        private StateMachine.State BuildScorchedExaltationState(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, () => _boss, AID.ScorchedExaltation, delay, 5, "AOE");
            s.EndHint |= StateMachine.StateHint.Raidwide;
            return s;
        }

        private StateMachine.State BuildHeatOfCondemnationState(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, () => _boss, AID.HeatOfCondemnation, delay, 6, "Tether");
            s.EndHint |= StateMachine.StateHint.Tankbuster;
            return s;
        }

        // note - positioning state is set at the end, make sure to clear later
        private StateMachine.State BuildExperimentalFireplumeState(ref StateMachine.State? link, float delay)
        {
            // TODO: helpers
            // 1. single-plume version: immediately after cast end, 1 helper teleports to position and starts casting 26303, which takes 6s
            // 2. multi-plume version: immediately after cast end, 9 helpers teleport to positions and start casting 26305
            //    first pair starts cast almost immediately, then pairs 2-4 and finally central start their cast with 1 sec between them
            //    each cast lasts 2 secs - ez to use caster coords to determine 'safespots' (midpoint between 1 and 2 pairs)
            // so center (last/only) plume hits around 6s after cast end
            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.ExperimentalFireplumeSingle] = new(null, () => { });
            dispatch[AID.ExperimentalFireplumeMulti] = new(null, () => { });
            var start = CommonStates.CastStart(ref link, () => _boss, dispatch, delay);
            start.EndHint |= StateMachine.StateHint.PositioningStart;

            var end = CommonStates.CastEnd(ref start.Next, () => _boss, 5, "Fireplume");
            return end;
        }

        // note - no positioning flags, since this is part of mechanics that manage it themselves
        private StateMachine.State BuildExperimentalAshplumeState(ref StateMachine.State? link, float delay)
        {
            // TODO: helpers
            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.ExperimentalAshplumeStack] = new(null, () => { });
            dispatch[AID.ExperimentalAshplumeSpread] = new(null, () => { });
            var start = CommonStates.CastStart(ref link, () => _boss, dispatch, delay);
            var end = CommonStates.CastEnd(ref start.Next, () => _boss, 3, "Ashplume");
            var resolve = CommonStates.Timeout(ref end.Next, 6); // resolve = 26307/26309 instant cast
            resolve.EndHint |= StateMachine.StateHint.Raidwide;
            return resolve;
        }

        // note - positioning state is set at the end, make sure to clear later
        private StateMachine.State BuildExperimentalGloryplumeState(ref StateMachine.State? link, float delay)
        {
            // TODO: variants?..
            // first part always seems to be "multi-plume", works just like fireplume
            // 9 helpers teleport to position, first pair almost immediately starts casting 26315s, 1 sec stagger between pairs, 7 sec for each cast
            // !!! helper should show position much earlier than graphic!
            // ~3 sec after cast ends, boss makes an instant cast that determines stack/spread - TODO detect...
            var cast = CommonStates.Cast(ref link, () => _boss, AID.ExperimentalGloryplume, delay, 5, "Gloryplume");
            cast.EndHint |= StateMachine.StateHint.PositioningStart;
            var resolve = CommonStates.Timeout(ref cast.Next, 13, "Gloryplume resolve");
            resolve.EndHint |= StateMachine.StateHint.Raidwide | StateMachine.StateHint.PositioningEnd;
            return resolve;
        }

        private StateMachine.State BuildCinderwingState(ref StateMachine.State? link, float delay)
        {
            // TODO: left/right
            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.RightCinderwing] = new(null, () => { });
            dispatch[AID.LeftCinderwing] = new(null, () => { });
            var start = CommonStates.CastStart(ref link, () => _boss, dispatch, delay);
            var end = CommonStates.CastEnd(ref start.Next, () => _boss, 5, "Wing");
            return end;
        }

        private StateMachine.State BuildDarkenedFireState(ref StateMachine.State? link, float delay)
        {
            // TODO: helper
            // 3s after cast ends, adds start casting 26299
            var addsStart = CommonStates.CastStart(ref link, () => _boss, AID.DarkenedFire, delay);
            addsStart.EndHint |= StateMachine.StateHint.PositioningStart;
            var addsEnd = CommonStates.CastEnd(ref addsStart.Next, () => _boss, 6, "DarkenedFire adds");
            addsEnd.EndHint |= StateMachine.StateHint.GroupWithNext;
            var numbers = CommonStates.Cast(ref addsEnd.Next, () => _boss, AID.BrightenedFire, 5, 5, "Numbers"); // numbers appear at the beginning of the cast, at the end he starts shooting 1-8
            numbers.EndHint |= StateMachine.StateHint.GroupWithNext;
            var resolve = CommonStates.Timeout(ref numbers.Next, 15, "DarkenedFire resolve");
            resolve.EndHint |= StateMachine.StateHint.PositioningEnd;
            return resolve;
        }

        private StateMachine.State BuildFledglingFlightState(ref StateMachine.State? link, float delay)
        {
            // TODO: helper?.. it's really a trivial mechanic...
            // at resolve, sparkfledgeds teleport to targets, 2 sec later start casting 26342 (4 sec cast)
            return CommonStates.Cast(ref link, () => _boss, AID.FledglingFlight, delay, 3, 8, "Eyes");
        }
    }
}
