﻿using System;
using System.Linq;

// CONTRIB: made by dhoggpt, not checked
namespace BossMod.Endwalker.Dungeon.D12Aetherfont.D123Octomammoth
{
    public enum OID : uint
    {
        Boss = 0x3EAA, // R26.000, x1
        MammothTentacle = 0x3EAB, // R6.000, x8, 523 type
        Helper = 0x233C, // ???
    };

    public enum AID : uint
    {
        BossSelfUKN = 33350, // Boss->self, no cast, single-target (ignored)
        Breathstroke = 34551, // Boss->self, 16.5s cast, range 35 180-degree cone
        Clearout = 33348, // MammothTentacle->self, 9.0s cast, range 16 120-degree cone
        Octostroke = 33347, // Boss->self, 16.0s cast, single-target (GUESSING SIZE)
        SalineSpit1 = 33352, // Boss->self, 3.0s cast, single-target
        SalineSpit2 = 33353, // Helper->self, 6.0s cast, range 8 circle
        Telekinesis1 = 33349, // Boss->self, 5.0s cast, single-target (ignored. the helper does the telegraph)
        Telekinesis2 = 33351, // Helper->self, 10.0s cast, range 12 circle
        TidalBreath = 33354, // Boss->self, 10.0s cast, range 35 180-degree cone
        TidalRoar = 33356, // Boss->self, 5.0s cast, range 60 circle
        VividEyes = 33355, // Boss->self, 4.0s cast, range ?-26 donut
        WaterDrop = 34436, // Helper->player, 5.0s cast, range 6 circle
        Wallop = 33346, // MammothTentacle->self, 3.0s cast, range 22 width 8 rect - idk if i did this one right
    };

    public enum TetherID : uint
    {
        Telekinesis = 167, // Actor3eac->Boss
    };

    class Border : BossComponent
    {
        private static float _platformOffset = 25;
        private static float _platformRadius = 8;
        private static Angle[] _platformDirections = [-90.Degrees(), -45.Degrees(), 0.Degrees(), 45.Degrees(), 90.Degrees()];
        private static WDir[] _platformCenters = _platformDirections.Select(d => _platformOffset * d.ToDirection()).ToArray();

        private static float _bridgeInner = 20;
        private static float _bridgeOuter = 26;
        private static Angle _offInner = DirToPointAtDistance(_bridgeInner);
        private static Angle _offOuter = DirToPointAtDistance(_bridgeOuter);

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            hints.AddForbiddenZone(p => {
                // union of platforms
                var res = _platformCenters.Select(off => ShapeDistance.Circle(module.Bounds.Center + off, _platformRadius)(p)).Min();
                // union of bridges
                for (int i = 1; i < 5; ++i)
                    res = Math.Min(res, ShapeDistance.Rect(module.Bounds.Center + _platformCenters[i - 1], module.Bounds.Center + _platformCenters[i], 3)(p));
                // invert
                return -res;
            });

            base.AddAIHints(module, slot, actor, assignment, hints);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            // draw platforms
            foreach (var c in _platformCenters)
                arena.AddCircle(module.Bounds.Center + c, _platformRadius, ArenaColor.Border);

            // draw bridges
            for (int i = 1; i < 5; ++i)
            {
                DrawBridgeLine(arena, module.Bounds.Center, _platformDirections[i - 1], _platformDirections[i], _offInner, _bridgeInner);
                DrawBridgeLine(arena, module.Bounds.Center, _platformDirections[i - 1], _platformDirections[i], _offOuter, _bridgeOuter);
            }
        }

        private static Angle DirToPointAtDistance(float d) => Angle.Acos((_platformOffset * _platformOffset + d * d - _platformRadius * _platformRadius) / (2 * _platformOffset * d));

        private void DrawBridgeLine(MiniArena arena, WPos center, Angle from, Angle to, Angle offset, float distance)
        {
            var p1 = center + distance * (from + offset).ToDirection();
            var p2 = center + distance * (to - offset).ToDirection();
            arena.AddLine(p1, p2, ArenaColor.Border);
        }
    }

    class Wallop : Components.SelfTargetedAOEs
    {
        public Wallop() : base(ActionID.MakeSpell(AID.Wallop), new AOEShapeRect(11, 5.5f, 16)) { }
    }

    class SalineSpit1 : Components.SpreadFromCastTargets
    {
        public SalineSpit1() : base(ActionID.MakeSpell(AID.SalineSpit1), 3) { }
    }

    class VividEyes : Components.SelfTargetedAOEs
    {
        public VividEyes() : base(ActionID.MakeSpell(AID.VividEyes), new AOEShapeDonut(20, 26)) { }
    }

    class Clearout : Components.SelfTargetedAOEs
    {
        public Clearout() : base(ActionID.MakeSpell(AID.Clearout), new AOEShapeCone(16, 60.Degrees())) { }
    }

    class TidalBreath : Components.SelfTargetedAOEs
    {
        public TidalBreath() : base(ActionID.MakeSpell(AID.TidalBreath), new AOEShapeCone(35, 90.Degrees())) { }
    }

    class Breathstroke : Components.SelfTargetedAOEs
    {
        public Breathstroke() : base(ActionID.MakeSpell(AID.Breathstroke), new AOEShapeCone(35, 90.Degrees())) { }
    }

    class Octostroke : Components.SelfTargetedAOEs
    {
        public Octostroke() : base(ActionID.MakeSpell(AID.Octostroke), new AOEShapeCircle(3)) { }
    }

    class TidalRoar : Components.RaidwideCast
    {
        public TidalRoar() : base(ActionID.MakeSpell(AID.TidalRoar)) { }
    }

    class WaterDrop : Components.SelfTargetedAOEs
    {
        public WaterDrop() : base(ActionID.MakeSpell(AID.WaterDrop), new AOEShapeCircle(6)) { }
    }

    class SalineSpit2 : Components.SelfTargetedAOEs
    {
        public SalineSpit2() : base(ActionID.MakeSpell(AID.SalineSpit2), new AOEShapeCircle(8)) { }
    }

    class Telekinesis2 : Components.SelfTargetedAOEs
    {
        public Telekinesis2() : base(ActionID.MakeSpell(AID.Telekinesis2), new AOEShapeCircle(12)) { }
    }

    class D123OctomammothStates : StateMachineBuilder
    {
        public D123OctomammothStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<Border>()
                .ActivateOnEnter<Wallop>()
                .ActivateOnEnter<Clearout>()
                .ActivateOnEnter<VividEyes>()
                .ActivateOnEnter<Octostroke>()
                .ActivateOnEnter<WaterDrop>()
                .ActivateOnEnter<TidalRoar>()
                .ActivateOnEnter<TidalBreath>()
                .ActivateOnEnter<Telekinesis2>()
                .ActivateOnEnter<Breathstroke>()
                .ActivateOnEnter<SalineSpit1>()
                .ActivateOnEnter<SalineSpit2>();
        }
    }

    [ModuleInfo(CFCID = 822, NameID = 12334)]
    class D123Octomammoth : BossModule
    {
        public D123Octomammoth(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(-370, -368), 36)) { }
    }
}
