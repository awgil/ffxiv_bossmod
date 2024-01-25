﻿using System;
using System.Collections.Generic;

namespace BossMod.RealmReborn.Trial.T03GarudaN
{
    public enum OID : uint
    {
        Boss = 0xEF, // x1
        Monolith = 0xED, // x4
        EyeOfTheStormHelper = 0x622, // x1
        RazorPlumeP1 = 0xEE, // spawn during fight
        RazorPlumeP2 = 0x2B0, // spawn during fight
    };

    public enum AID : uint
    {
        AutoAttack = 870, // Boss->player, no cast, single-target
        Friction = 656, // Boss->players, no cast, range 5 circle at random target
        Downburst = 657, // Boss->self, no cast, range 10+1.7 ?-degree cone cleave
        WickedWheel = 658, // Boss->self, no cast, range 7+1.7 circle cleave
        Slipstream = 659, // Boss->self, 2.5s cast, range 10+1.7 ?-degree cone interruptible aoe
        MistralSongP1 = 667, // Boss->self, 4.0s cast, range 30+1.7 LOSable raidwide
        AerialBlast = 662, // Boss->self, 4.0s cast, raidwide
        EyeOfTheStorm = 664, // EyeOfTheStormHelper->self, 3.0s cast, range 12-25 donut
        MistralSongP2 = 660, // Boss->self, 4.0s cast, range 30+1.7 ?-degree cone aoe
        MistralShriek = 661, // Boss->self, 4.0s cast, range 23+1.7 circle aoe
        Featherlance = 665, // RazorPlumeP1/RazorPlumeP2->self, no cast, range 8 circle, suicide attack if not killed in ~25s
    };

    // disallow clipping monoliths
    class Friction : BossComponent
    {
        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            if (module.PrimaryActor.CastInfo == null) // don't forbid standing near monoliths while boss is casting to allow avoiding aoes
                foreach (var m in module.Enemies(OID.Monolith))
                    hints.AddForbiddenZone(ShapeDistance.Circle(m.Position, 5));
        }
    }

    class Downburst : Components.Cleave
    {
        public Downburst() : base(ActionID.MakeSpell(AID.Downburst), new AOEShapeCone(11.7f, 60.Degrees())) { }
    }

    class Slipstream : Components.SelfTargetedAOEs
    {
        public Slipstream() : base(ActionID.MakeSpell(AID.Slipstream), new AOEShapeCone(11.7f, 45.Degrees())) { }
    }

    class MistralSongP1 : Components.CastLineOfSightAOE
    {
        public MistralSongP1() : base(ActionID.MakeSpell(AID.MistralSongP1), 31.7f, true) { }
        public override IEnumerable<Actor> BlockerActors(BossModule module) => module.Enemies(OID.Monolith);
    }

    // actual casts happen every ~6s after aerial blast cast
    class EyeOfTheStorm : Components.GenericAOEs
    {
        private AOEShapeDonut _shape = new(12, 25);

        public EyeOfTheStorm() : base(ActionID.MakeSpell(AID.AerialBlast)) { }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (NumCasts > 0)
                foreach (var c in module.Enemies(OID.EyeOfTheStormHelper))
                    yield return new(_shape, c.Position);
        }
    }

    class MistralSongP2 : Components.SelfTargetedAOEs
    {
        public MistralSongP2() : base(ActionID.MakeSpell(AID.MistralSongP2), new AOEShapeCone(31.7f, 60.Degrees())) { }
    }

    class MistralShriek : Components.SelfTargetedAOEs
    {
        public MistralShriek() : base(ActionID.MakeSpell(AID.MistralShriek), new AOEShapeCircle(24.7f)) { }
    }

    class T03GarudaNStates : StateMachineBuilder
    {
        public T03GarudaNStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<Friction>()
                .ActivateOnEnter<Downburst>()
                .ActivateOnEnter<Slipstream>()
                .ActivateOnEnter<MistralSongP1>()
                .ActivateOnEnter<EyeOfTheStorm>()
                .ActivateOnEnter<MistralSongP2>()
                .ActivateOnEnter<MistralShriek>();
        }
    }

    [ModuleInfo(CFCID = 58, NameID = 1644)]
    public class T03GarudaN : BossModule
    {
        public T03GarudaN(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(0, 0), 21)) { }

        public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.CalculateAIHints(slot, actor, assignment, hints);
            foreach (var e in hints.PotentialTargets)
            {
                e.Priority = (OID)e.Actor.OID switch
                {
                    OID.RazorPlumeP1 or OID.RazorPlumeP2 => 2,
                    OID.Boss => 1,
                    _ => 0
                };
            }
        }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
            foreach (var m in Enemies(OID.Monolith))
                Arena.Actor(m, ArenaColor.Danger, true);
        }
    }
}
