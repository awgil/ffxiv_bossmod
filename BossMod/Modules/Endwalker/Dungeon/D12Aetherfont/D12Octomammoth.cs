using System;
using System.Collections.Generic;
using System.Linq;
using Lumina.Data.Parsing.Layer;
using static BossMod.ActorCastEvent;

/*
everything we need to understand the mechanics is in here
https://www.thegamer.com/final-fantasy-14-the-aetherfont-dungeon-guide-walkthrough/
https://ffxiv.consolegameswiki.com/wiki/The_Aetherfont
https://gamerescape.com/2023/05/25/ffxiv-endwalker-guide-the-aetherfont/
*/

namespace BossMod.Endwalker.Dungeon.D12Aetherfont.D12Octomammoth
{
    /*
    notes to self bnpcname has nameID, contentfindercondition has the CFC
    iconid is a row in lockon sheet
    tetherid is a row in channeling or w/e sheet
    */
    public enum OID : uint
    {
        Boss = 0x3EAA, // R26.000, x1
        MammothTentacle = 0x3EAB, // R6.000, x8, 523 type
        Actor3eac = 0x3EAC, // R0.500, x8
        Helper = 0x233C , // ???
    };

    public enum AID : uint
    {
        BossSelfUKN = 33350,      //done.(ignored) Boss->self, no cast, single-target
        Breathstroke = 34551,     //done. Boss->self, 16.5s cast, range 35 180-degree cone
        Clearout = 33348,         //done. MammothTentacle->self, 9.0s cast, range 16 120-degree cone
        Octostroke = 33347,       //done.(GUESSING SIZE) Boss->self, 16.0s cast, single-target
        SalineSpit1 = 33352,      //done. Boss->self, 3.0s cast, single-target
        SalineSpit2 = 33353,      //done. Helper->self, 6.0s cast, range 8 circle
        Telekinesis1 = 33349,     //done.(ignored. the helper does the telegraph) Boss->self, 5.0s cast, single-target
        Telekinesis2 = 33351,     //done. Helper->self, 10.0s cast, range 12 circle
        TidalBreath = 33354,      //done. Boss->self, 10.0s cast, range 35 180-degree cone
        TidalRoar = 33356,        //done. Boss->self, 5.0s cast, range 60 circle
        VividEyes = 33355,        //done. Boss->self, 4.0s cast, range ?-26 donut
        WaterDrop = 34436,        //done. Helper->player, 5.0s cast, range 6 circle
        Wallop = 33346,           //done. MammothTentacle->self, 3.0s cast, range 22 width 8 rect //idk if i did this one right
    };
    public enum IconID : uint
    {
        Icon_300 = 300, // Helper
        Icon_139 = 139, // player/33CE/33CF/3DC2
    };
    public enum TetherID : uint
    {
        Tether_167 = 167, // Actor3eac->Boss -> Telekinesis
    };

    class Wallop : Components.SelfTargetedAOEs
    {
        public Wallop() : base(ActionID.MakeSpell(AID.Wallop), new AOEShapeRect(11,5.5f,16)) { }
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
    class D12OctomammothStates : StateMachineBuilder
    {
        public D12OctomammothStates(BossModule module) : base(module)
        {
            TrivialPhase()
            .ActivateOnEnter<D12OctomammothAI>()
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

    /*    notes to self bnpcname has nameID, contentfindercondition has the CFC
    */

     class D12OctomammothAI : BossComponent
    {
        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            // Define circles
            var circles = new WPos[]
            {
                new WPos(-345, -368),               // Circle A
                new WPos(-387.678f, -350.322f),     // Circle B
                new WPos(-352.322f, -350.322f),     // Circle C
                new WPos(-370,  -343),              // Circle D
                new WPos(-395, -368)                // Circle E
            };

            // Define smaller circles because i failed to get rectangles working
            var smallCircles = new WPos[]
            {
             /*   new WPos(-348, -358),               // Small Circle A
                new WPos(-360, -346),               // Small Circle B
                new WPos(-379, -346),               // Small Circle C
                new WPos(-392, -360)                // Small Circle D*/
            new WPos(-347.4326171875f, -360.63653564453f),
            new WPos(-347.67233276367f, -359.98715209961f),
            new WPos(-347.85485839844f, -359.49258422852f),
            new WPos(-348.21545410156f, -358.51571655273f),
            new WPos(-348.66912841797f, -357.2868347168f),
            new WPos(-359.22134399414f, -346.90921020508f),
            new WPos(-359.98919677734f, -346.52166748047f),
            new WPos(-361.35482788086f, -345.83236694336f),
            new WPos(-362.4801940918f, -345.26443481445f),
            new WPos(-377.45910644531f, -344.94708251953f),
            new WPos(-378.73901367188f, -345.44708251953f),
            new WPos(-379.94046020508f, -345.91647338867f),
            new WPos(-381.08489990234f, -346.36361694336f),
            new WPos(-391.44171142578f, -357.25454711914f),
            new WPos(-391.79779052734f, -357.98648071289f),
            new WPos(-392.10589599609f, -358.6198425293f),
            new WPos(-392.50811767578f, -359.44671630859f),
            new WPos(-392.99645996094f, -360.45059204102f)
            };

            // Define circle radius
            var circleRadius = 8f;
            var smallerCircleRadius = 2f;

            // Define the center and radius of the giant circle
            var giantCircleCenter = new WPos(-370, -368); // Adjust as needed
            var giantCircleRadius = 56f;          // Adjust as needed

            // Calculate the distance from a point to the nearest circle's edge
            Func<WPos, float> distanceToNearestCircleEdge = p =>
            {
                var minDistance = float.MaxValue;
                foreach (var circle in circles)
                {
                    var dx = p.X - circle.X;
                    var dz = p.Z - circle.Z;
                    var distance = MathF.Sqrt(dx * dx + dz * dz) - circleRadius;
                    minDistance = Math.Min(minDistance, distance);
                }
                foreach (var circle in smallCircles)
                {
                    var dx = p.X - circle.X;
                    var dz = p.Z - circle.Z;
                    var distance = MathF.Sqrt(dx * dx + dz * dz) - smallerCircleRadius;
                    minDistance = Math.Min(minDistance, distance);
                }
                return minDistance;
            };

            // Calculate the distance from a point to the edge of the giant circle
            Func<WPos, float> distanceToGiantCircleEdge = p =>
            {
                var dx = p.X - giantCircleCenter.X;
                var dz = p.Z - giantCircleCenter.Z;
                return giantCircleRadius - MathF.Sqrt(dx * dx + dz * dz);
            };

            // Define the forbidden zone as the area outside all the circles within the giant circle
            Func<WPos, float> forbiddenZone = p =>
            {
                var nearestCircleDistance = distanceToNearestCircleEdge(p);
                var giantCircleDistance = distanceToGiantCircleEdge(p);
                return -Math.Min(nearestCircleDistance, giantCircleDistance); // Invert the distance
            };

            // Add forbidden zone using the calculated function with current time
            hints.AddForbiddenZone(forbiddenZone);
        }
    }

    [ModuleInfo(CFCID = 822, NameID = 12334)]

    class D12Octomammoth : BossModule
    {
        public D12Octomammoth(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(-370, -368), 36)) { }

        public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.CalculateAIHints(slot, actor, assignment, hints);
        }

    }
    

}