using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod.Endwalker.HuntS.Ker
{
    public enum OID : uint
    {
        Boss = 0x35CF, // R8.000, x1
    };

    public enum AID : uint
    {
        AutoAttack = 872, // Boss->player, no cast, single-target
        MinaxGlare = 27635, // Boss->self, 6.0s cast, range 40 circle
        Heliovoid = 27636, // Boss->self, 6.0s cast, range 12 circle
        AncientBlizzard = 27637, // Boss->self, 6.0s cast, range 8?-40 donut
        ForeInterment = 27641, // Boss->self, 6.0s cast, range 40 180-degree cone
        RearInterment = 27642, // Boss->self, 6.0s cast, range 40 180-degree cone
        RightInterment = 27643, // Boss->self, 6.0s cast, range 40 180-degree cone
        LeftInterment = 27644, // Boss->self, 6.0s cast, range 40 180-degree cone
        WhisperedIncantation = 27645, // Boss->self, 5.0s cast, single-target, applies status to boss
        EternalDamnation = 27647, // Boss->self, 6.0s cast, range 40 circle gaze
        AncientFlare = 27704, // Boss->self, 6.0s cast, range 40 circle, applies pyretic
        //??? = 25698, // Boss->player, no cast, single-target
    };

    class MinaxGlare : Components.CastHint
    {
        public MinaxGlare() : base(ActionID.MakeSpell(AID.MinaxGlare), "Randomize movement") { }
    }

    class Heliovoid : Components.SelfTargetedAOEs
    {
        public Heliovoid() : base(ActionID.MakeSpell(AID.Heliovoid), new AOEShapeCircle(12)) { }
    }

    class AncientBlizzard : Components.SelfTargetedAOEs
    {
        public AncientBlizzard() : base(ActionID.MakeSpell(AID.AncientBlizzard), new AOEShapeDonut(8, 40)) { } // TODO: verify inner radius
    }

    // TODO: mirrored incantation for all interments
    class ForeInterment : Components.SelfTargetedAOEs
    {
        public ForeInterment() : base(ActionID.MakeSpell(AID.ForeInterment), new AOEShapeCone(40, 90.Degrees())) { }
    }

    class RearInterment : Components.SelfTargetedAOEs
    {
        public RearInterment() : base(ActionID.MakeSpell(AID.RearInterment), new AOEShapeCone(40, 90.Degrees())) { }
    }

    class RightInterment : Components.SelfTargetedAOEs
    {
        public RightInterment() : base(ActionID.MakeSpell(AID.RightInterment), new AOEShapeCone(40, 90.Degrees())) { }
    }

    class LeftInterment : Components.SelfTargetedAOEs
    {
        public LeftInterment() : base(ActionID.MakeSpell(AID.LeftInterment), new AOEShapeCone(40, 90.Degrees())) { }
    }

    class EternalDamnation : Components.CastGaze
    {
        public EternalDamnation() : base(ActionID.MakeSpell(AID.EternalDamnation)) { }
    }

    // TODO: pyretic hint
    class AncientFlare : Components.CastHint
    {
        public AncientFlare() : base(ActionID.MakeSpell(AID.AncientFlare), "Do nothing until buff expires") { }
    }

    // TODO: whispered incantation -> whispers manifest
    // TODO: wicked swipe
    // TODO: ancient holy
    class KerStates : StateMachineBuilder
    {
        public KerStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<MinaxGlare>()
                .ActivateOnEnter<Heliovoid>()
                .ActivateOnEnter<AncientBlizzard>()
                .ActivateOnEnter<ForeInterment>()
                .ActivateOnEnter<RearInterment>()
                .ActivateOnEnter<RightInterment>()
                .ActivateOnEnter<LeftInterment>()
                .ActivateOnEnter<EternalDamnation>()
                .ActivateOnEnter<AncientFlare>();
        }
    }

    public class Ker : SimpleBossModule
    {
        public Ker(WorldState ws, Actor primary) : base(ws, primary) { }
    }
}
