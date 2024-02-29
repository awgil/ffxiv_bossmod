﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE52TimeToBurn
{
    public enum OID : uint
    {
        Boss = 0x31C8, // R9.000, x4
        Helper = 0x233C, // R0.500, x23
        Clock = 0x1EB17A, // R0.500, x9, EventObj type
        TimeBomb1 = 0x1EB17B, // R0.500, EventObj type, spawn during fight
        TimeBomb2 = 0x1EB1D4, // R0.500, EventObj type, spawn during fight
    };

    public enum AID : uint
    {
        AutoAttack = 6499, // Boss->player, no cast, single-target
        TimeEruption = 23953, // Boss->self, 3.0s cast, single-target, visual
        PauseTime = 23954, // Boss->self, 3.0s cast, single-target, visual
        StartTime = 23955, // Boss->self, 3.0s cast, single-target, visual
        TimeEruptionAOE = 23956, // Helper->location, no cast, range 20 width 20 rect aoe
        TimeBomb = 23957, // Boss->self, 3.0s cast, single-target, visual
        TimeBombAOE = 23958, // Helper->self, 1.0s cast, range 60 90-degree cone
        Reproduce = 24809, // Boss->self, 3.0s cast, single-target, visual
        CrimsonCyclone = 23959, // Boss->self, 10.0s cast, range 60 width 20 rect aoe
        Eruption = 23960, // Helper->location, 3.0s cast, range 8 circle aoe
        FireTankbuster = 23961, // Boss->player, 5.0s cast, single-target
        FireRaidwide = 23962, // Boss->self, 5.0s cast, single-target, visual
        FireRaidwideAOE = 23963, // Helper->self, no cast, ???
    };

    // these three main mechanics can overlap in a complex way, so we have a single component to handle them. Potential options:
    // - eruption only (fast/slow clocks): visual cast -> 9 eobjanims -> pause cast -> start cast -> resolve
    // - bombs only (cones): visual cast -> spawn bombs -> countdown eobjanims -> resolve
    // - reproduce only (clone charges): visual cast -> resolve
    // - bombs only (x3 instead of x2)
    // - complex: eruption visual -> 9 eruption eobjanims -> pause cast -> bomb visual -> spawn bombs -> reproduce visual & bomb countdown -> cyclone cast start -> bomb resolve -> cyclone resolve -> start cast -> eruption resolve
    // => rules: show bombs if active (activate by visual, deactivate by resolve, show for each object); otherwise show cyclone cast if active; otherwise show eruptions
    class TimeEruptionBombReproduce : Components.GenericAOEs
    {
        private DateTime _bombsActivation;
        private DateTime _eruptionStart; // timestamp of StartTime cast start
        private IReadOnlyList<Actor> _bombs = ActorEnumeration.EmptyList;
        private List<Actor> _cycloneCasters = new();
        private List<(WPos pos, TimeSpan delay)> _clocks = new();
        private List<WPos> _eruptionSafeSpots = new();

        private static AOEShapeCone _shapeBomb = new(60, 45.Degrees());
        private static AOEShapeRect _shapeCyclone = new(60, 10);
        private static AOEShapeRect _shapeEruption = new(10, 10, 10);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_bombsActivation != default)
            {
                foreach (var b in _bombs)
                    yield return new(_shapeBomb, b.Position, b.Rotation, _bombsActivation);
            }
            else if (_cycloneCasters.Count > 0)
            {
                foreach (var c in _cycloneCasters)
                    yield return new(_shapeCyclone, c.Position, c.CastInfo!.Rotation, c.CastInfo.NPCFinishAt);
            }
            else if (_eruptionStart != default)
            {
                foreach (var e in _clocks.Count > 2 ? _clocks.Take(_clocks.Count - 2) : _clocks)
                    yield return new(_shapeEruption, e.pos, new(), _eruptionStart + e.delay);
            }
        }

        public override void Init(BossModule module)
        {
            _bombs = module.Enemies(OID.TimeBomb1); // either 1 or 2 works, dunno what's the difference
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var p in _eruptionSafeSpots)
                _shapeEruption.Draw(arena, p, new(), ArenaColor.SafeFromAOE);
            base.DrawArenaBackground(module, pcSlot, pc, arena);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.TimeBomb:
                    if (caster == module.PrimaryActor)
                        _bombsActivation = module.WorldState.CurrentTime.AddSeconds(23.2f);
                    break;
                case AID.CrimsonCyclone:
                    _cycloneCasters.Add(caster);
                    break;
                case AID.StartTime:
                    _eruptionStart = module.WorldState.CurrentTime;
                    break;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.TimeBombAOE:
                    _bombsActivation = new();
                    break;
                case AID.CrimsonCyclone:
                    _cycloneCasters.Remove(caster);
                    break;
                case AID.TimeEruptionAOE:
                    _eruptionSafeSpots.Clear();
                    _clocks.RemoveAll(c => c.pos.AlmostEqual(caster.Position, 1));
                    if (_clocks.Count == 0)
                        _eruptionStart = new();
                    break;
            }
        }

        public override void OnActorEAnim(BossModule module, Actor actor, uint state)
        {
            if ((OID)actor.OID != OID.Clock)
                return;

            var delay = state switch
            {
                0x00010002 => TimeSpan.FromSeconds(9.7),
                0x00010020 => TimeSpan.FromSeconds(11.7),
                _ => TimeSpan.Zero
            };
            if (delay == TimeSpan.Zero)
                return;

            _clocks.Add((actor.Position, delay));
            if (_clocks.Count == 9)
            {
                _clocks.SortBy(e => e.delay);
                _eruptionSafeSpots.AddRange(_clocks.TakeLast(2).Select(e => e.pos));
            }
        }
    }

    class Eruption : Components.LocationTargetedAOEs
    {
        public Eruption() : base(ActionID.MakeSpell(AID.Eruption), 8) { }
    }

    class FireTankbuster : Components.SingleTargetCast
    {
        public FireTankbuster() : base(ActionID.MakeSpell(AID.FireTankbuster)) { }
    }

    class FireRaidwide : Components.RaidwideCast
    {
        public FireRaidwide() : base(ActionID.MakeSpell(AID.FireRaidwide)) { }
    }

    class CE52TimeToBurnStates : StateMachineBuilder
    {
        public CE52TimeToBurnStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<TimeEruptionBombReproduce>()
                .ActivateOnEnter<Eruption>()
                .ActivateOnEnter<FireTankbuster>()
                .ActivateOnEnter<FireRaidwide>();
        }
    }

    [ModuleInfo(CFCID = 778, DynamicEventID = 26)]
    public class CE52TimeToBurn : BossModule
    {
        public CE52TimeToBurn(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(-550, 0), 30)) { }
    }
}
