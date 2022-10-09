using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.RealmReborn.Dungeon.D16Amdapor.D162DemonWall
{
    public enum OID : uint
    {
        Helper = 0x19A, // x3
        Boss = 0x283, // x1
        Pollen = 0x1E86B1, // x1, EventObj type
    };

    public enum AID : uint
    {
        MurderHole = 1044, // Boss->player, no cast, range 6 circle cleaving autoattack at random target
        LiquefyCenter = 1045, // Helper->self, 3.0s cast, range 50+R width 8 rect
        LiquefySides = 1046, // Helper->self, 2.0s cast, range 50+R width 7 rect
        Repel = 1047, // Boss->self, 3.0s cast, range 40+R 180?-degree cone knockback 20 (non-immunable)
    };

    class LiquefyCenter : Components.SelfTargetedAOEs
    {
        public LiquefyCenter() : base(ActionID.MakeSpell(AID.LiquefyCenter), new AOEShapeRect(50, 4)) { }
    }

    class LiquefySides : Components.SelfTargetedAOEs
    {
        public LiquefySides() : base(ActionID.MakeSpell(AID.LiquefySides), new AOEShapeRect(50, 3.5f)) { }
    }

    class Repel : Components.KnockbackFromCaster
    {
        public Repel() : base(ActionID.MakeSpell(AID.Repel), 20, ignoreImmunes: true) { }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            // custom hint: stay in narrow zone in center
            if (Casters.Count > 0)
            {
                var safe = ShapeDistance.Rect(module.PrimaryActor.Position, 0.Degrees(), 50, -2, 1);
                hints.AddForbiddenZone(p => -safe(p));
            }
        }
    }

    class ForbiddenZones : Components.GenericAOEs
    {
        private static AOEShapeRect _shape = new(50, 10);

        public override IEnumerable<(AOEShape shape, WPos origin, Angle rotation, DateTime time)> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            yield return (_shape, module.PrimaryActor.Position, 180.Degrees(), new()); // area behind boss

            var pollen = module.Enemies(OID.Pollen).FirstOrDefault();
            if (pollen != null && pollen.EventState == 0)
                yield return (_shape, new(200, -122), 0.Degrees(), new());
        }
    }

    class D162DemonWallStates : StateMachineBuilder
    {
        public D162DemonWallStates(BossModule module) : base(module)
        {
            TrivialPhase()
                //.ActivateOnEnter<MurderHole>() - note: no component here, there's not enough space to spread properly, and this hits for small damage
                .ActivateOnEnter<LiquefyCenter>()
                .ActivateOnEnter<LiquefySides>()
                .ActivateOnEnter<Repel>()
                .ActivateOnEnter<ForbiddenZones>();
        }
    }

    public class D162DemonWall : BossModule
    {
        public D162DemonWall(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsRect(new(200, -131), 10, 21)) { }
    }
}
