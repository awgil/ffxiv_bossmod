using System.Collections.Generic;

namespace BossMod.Endwalker.HuntA.Yilan
{
    public enum OID : uint
    {
        Boss = 0x35BF, // R5.400, x1
    };

    public enum AID : uint
    {
        AutoAttack = 872, // Boss->player, no cast, single-target
        Soundstorm = 27230, // Boss->self, 5.0s cast, range 30 circle, applies march debuffs
        MiniLight = 27231, // Boss->self, 6.0s cast, range 18 circle
        Devour = 27232, // Boss->self, 1.0s cast, range 10 ?-degree cone, kills seduced and deals very small damage otherwise
        BogBomb = 27233, // Boss->location, 4.0s cast, range 6 circle
        BrackishRain = 27234, // Boss->self, 4.0s cast, range 10 90-degree cone
    };

    public enum SID : uint
    {
        None = 0,
        ForwardMarch = 1958,
        AboutFace = 1959,
        LeftFace = 1960,
        RightFace = 1961,
    }

    class Soundstorm : Components.StatusDrivenForcedMarch
    {
        public Soundstorm() : base(2, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace) { }

        public override bool DestinationUnsafe(BossModule module, int slot, Actor actor, WPos pos) => MiniLight.Shape.Check(pos, module.PrimaryActor);

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (module.PrimaryActor.CastInfo?.IsSpell(AID.Soundstorm) ?? false)
                hints.Add("Apply march debuffs");
        }
    }

    class MiniLight : Components.GenericAOEs
    {
        private bool _active;
        public static AOEShapeCircle Shape = new(18);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_active)
                yield return new(Shape, module.PrimaryActor.Position); // TODO: activation
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.Soundstorm or AID.MiniLight)
                _active = true;
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.MiniLight)
                _active = false;
        }
    }

    class Devour : Components.CastHint
    {
        public Devour() : base(ActionID.MakeSpell(AID.Devour), "Harmless until failed previous mechanic") { }
    }

    class BogBomb : Components.LocationTargetedAOEs
    {
        public BogBomb() : base(ActionID.MakeSpell(AID.BogBomb), 6) { }
    }

    class BrackishRain : Components.SelfTargetedAOEs
    {
        public BrackishRain() : base(ActionID.MakeSpell(AID.BrackishRain), new AOEShapeCone(10, 45.Degrees())) { }
    }

    class YilanStates : StateMachineBuilder
    {
        public YilanStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<Soundstorm>()
                .ActivateOnEnter<MiniLight>()
                .ActivateOnEnter<Devour>()
                .ActivateOnEnter<BogBomb>()
                .ActivateOnEnter<BrackishRain>();
        }
    }

    public class Yilan : SimpleBossModule
    {
        public Yilan(WorldState ws, Actor primary) : base(ws, primary) { }
    }
}
