using System;
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
        Soundstorm = 27230, // Boss->self, 5.0s cast, range 30 circle
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

    class SoundstormMiniLightDevour : Components.GenericAOEs
    {
        private static AOEShapeCircle _miniLight = new(18);
        private static float _marchDistance = 12;

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if ((module.PrimaryActor.CastInfo?.IsSpell(AID.MiniLight) ?? false) || MarchDirection(actor) != SID.None)
                yield return new(_miniLight, module.PrimaryActor.Position); // TODO: activation
        }

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (!(module.PrimaryActor.CastInfo?.IsSpell() ?? false))
                return;

            string hint = (AID)module.PrimaryActor.CastInfo.Action.ID switch
            {
                AID.Soundstorm => "Apply march debuffs",
                AID.Devour => "Harmless unless failed",
                _ => "",
            };
            if (hint.Length > 0)
                hints.Add(hint);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            var marchDirection = MarchDirection(pc);
            if (marchDirection != SID.None)
            {
                var dir = marchDirection switch
                {
                    SID.AboutFace => 180.Degrees(),
                    SID.LeftFace => 90.Degrees(),
                    SID.RightFace => -90.Degrees(),
                    _ => 0.Degrees()
                };
                var target = pc.Position + _marchDistance * (pc.Rotation + dir).ToDirection();
                arena.AddLine(pc.Position, target, ArenaColor.Danger);
                arena.Actor(target, pc.Rotation, ArenaColor.Danger);
            }
        }

        private SID MarchDirection(Actor actor)
        {
            foreach (var s in actor.Statuses)
                if ((SID)s.ID is SID.ForwardMarch or SID.AboutFace or SID.LeftFace or SID.RightFace)
                    return (SID)s.ID;
            return SID.None;
        }
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
                .ActivateOnEnter<SoundstormMiniLightDevour>()
                .ActivateOnEnter<BogBomb>()
                .ActivateOnEnter<BrackishRain>();
        }
    }

    public class Yilan : SimpleBossModule
    {
        public Yilan(WorldState ws, Actor primary) : base(ws, primary) { }
    }
}
