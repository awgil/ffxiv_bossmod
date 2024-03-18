using System;

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
                //.ActivateOnEnter<D123OctomammothForeground>()
                //.ActivateOnEnter<D123OctomammothAI>()
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

    //class D123OctomammothAI : BossComponent
    //{
    //    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    //    {
    //        // Define circles
    //        var circles = new WPos[]
    //        {
    //            new WPos(-345, -368),               // Circle A
    //            new WPos(-387.678f, -350.322f),     // Circle B
    //            new WPos(-352.322f, -350.322f),     // Circle C
    //            new WPos(-370,  -343),              // Circle D
    //            new WPos(-395, -368)                // Circle E
    //        };

    //        // Define smaller circles because i failed to get rectangles working
    //        var smallCircles = new WPos[]
    //        {
    //            new WPos(-347.4326171875f, -360.63653564453f),
    //            new WPos(-347.67233276367f, -359.98715209961f),
    //            new WPos(-347.85485839844f, -359.49258422852f),
    //            new WPos(-348.21545410156f, -358.51571655273f),
    //            new WPos(-348.66912841797f, -357.2868347168f),
    //            new WPos(-359.22134399414f, -346.90921020508f),
    //            new WPos(-359.98919677734f, -346.52166748047f),
    //            new WPos(-361.35482788086f, -345.83236694336f),
    //            new WPos(-362.4801940918f, -345.26443481445f),
    //            new WPos(-377.45910644531f, -344.94708251953f),
    //            new WPos(-378.73901367188f, -345.44708251953f),
    //            new WPos(-379.94046020508f, -345.91647338867f),
    //            new WPos(-381.08489990234f, -346.36361694336f),
    //            new WPos(-391.44171142578f, -357.25454711914f),
    //            new WPos(-391.79779052734f, -357.98648071289f),
    //            new WPos(-392.10589599609f, -358.6198425293f),
    //            new WPos(-392.50811767578f, -359.44671630859f),
    //            new WPos(-392.99645996094f, -360.45059204102f)
    //        };

    //        // Define circle radius
    //        var circleRadius = 8f;
    //        var smallerCircleRadius = 2f;

    //        // Define the center and radius of the giant circle
    //        var giantCircleCenter = new WPos(-370, -368); // Adjust as needed
    //        var giantCircleRadius = 56f; // Adjust as needed

    //        // Calculate the distance from a point to the nearest circle's edge
    //        Func<WPos, float> distanceToNearestCircleEdge = p =>
    //        {
    //            var minDistance = float.MaxValue;
    //            foreach (var circle in circles)
    //            {
    //                var dx = p.X - circle.X;
    //                var dz = p.Z - circle.Z;
    //                var distance = MathF.Sqrt(dx * dx + dz * dz) - circleRadius;
    //                minDistance = Math.Min(minDistance, distance);
    //            }
    //            foreach (var circle in smallCircles)
    //            {
    //                var dx = p.X - circle.X;
    //                var dz = p.Z - circle.Z;
    //                var distance = MathF.Sqrt(dx * dx + dz * dz) - smallerCircleRadius;
    //                minDistance = Math.Min(minDistance, distance);
    //            }
    //            return minDistance;
    //        };

    //        // Calculate the distance from a point to the edge of the giant circle
    //        Func<WPos, float> distanceToGiantCircleEdge = p =>
    //        {
    //            var dx = p.X - giantCircleCenter.X;
    //            var dz = p.Z - giantCircleCenter.Z;
    //            return giantCircleRadius - MathF.Sqrt(dx * dx + dz * dz);
    //        };

    //        // Define the forbidden zone as the area outside all the circles within the giant circle
    //        Func<WPos, float> forbiddenZone = p =>
    //        {
    //            var nearestCircleDistance = distanceToNearestCircleEdge(p);
    //            var giantCircleDistance = distanceToGiantCircleEdge(p);
    //            return -Math.Min(nearestCircleDistance, giantCircleDistance); // Invert the distance
    //        };

    //        // Add forbidden zone using the calculated function with current time
    //        hints.AddForbiddenZone(forbiddenZone);
    //    }
    //}

    //class D123OctomammothForeground : BossComponent
    //{
    //    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    //    {
    //        DrawCircles(arena);
    //    }

    //    private void DrawCircles(MiniArena arena)
    //    {
    //        arena.AddCircleFilled(new WPos(-345, -368), 8, 0xFF404040);
    //        arena.AddCircleFilled(new WPos(-345, -368), 8, 0xFF404040);
    //        arena.AddCircleFilled(new WPos(-387.678f, -350.322f), 8, 0xFF404040);
    //        arena.AddCircleFilled(new WPos(-352.322f, -350.322f), 8, 0xFF404040);
    //        arena.AddCircleFilled(new WPos(-370, -343), 8, 0xFF404040);
    //        arena.AddCircleFilled(new WPos(-395, -368), 8, 0xFF404040);
    //        arena.AddCircleFilled(new WPos(-347.4326171875f, -360.63653564453f), 2, 0xFF404040);
    //        arena.AddCircleFilled(new WPos(-347.67233276367f, -359.98715209961f), 2, 0xFF404040);
    //        arena.AddCircleFilled(new WPos(-347.85485839844f, -359.49258422852f), 2, 0xFF404040);
    //        arena.AddCircleFilled(new WPos(-348.21545410156f, -358.51571655273f), 2, 0xFF404040);
    //        arena.AddCircleFilled(new WPos(-348.66912841797f, -357.2868347168f), 2, 0xFF404040);
    //        arena.AddCircleFilled(new WPos(-359.22134399414f, -346.90921020508f), 2, 0xFF404040);
    //        arena.AddCircleFilled(new WPos(-359.98919677734f, -346.52166748047f), 2, 0xFF404040);
    //        arena.AddCircleFilled(new WPos(-361.35482788086f, -345.83236694336f), 2, 0xFF404040);
    //        arena.AddCircleFilled(new WPos(-362.4801940918f, -345.26443481445f), 2, 0xFF404040);
    //        arena.AddCircleFilled(new WPos(-377.45910644531f, -344.94708251953f), 2, 0xFF404040);
    //        arena.AddCircleFilled(new WPos(-378.73901367188f, -345.44708251953f), 2, 0xFF404040);
    //        arena.AddCircleFilled(new WPos(-379.94046020508f, -345.91647338867f), 2, 0xFF404040);
    //        arena.AddCircleFilled(new WPos(-381.08489990234f, -346.36361694336f), 2, 0xFF404040);
    //        arena.AddCircleFilled(new WPos(-391.44171142578f, -357.25454711914f), 2, 0xFF404040);
    //        arena.AddCircleFilled(new WPos(-391.79779052734f, -357.98648071289f), 2, 0xFF404040);
    //        arena.AddCircleFilled(new WPos(-392.10589599609f, -358.6198425293f), 2, 0xFF404040);
    //        arena.AddCircleFilled(new WPos(-392.50811767578f, -359.44671630859f), 2, 0xFF404040);
    //        arena.AddCircleFilled(new WPos(-392.99645996094f, -360.45059204102f), 2, 0xFF404040);
    //    }
    //}

    [ModuleInfo(CFCID = 822, NameID = 12334)]
    class D123Octomammoth : BossModule
    {
        public D123Octomammoth(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(-370, -368), 36)) { }
    }
}
