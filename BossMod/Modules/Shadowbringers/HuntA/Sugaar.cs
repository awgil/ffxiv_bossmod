using System;
using System.Collections.Generic;
using System.Linq;


namespace BossMod.Shadowbringers.HuntA.Sugaar
{
    public enum OID : uint
    {
        Boss = 0x2875, 
    };

    public enum AID : uint
    {
        AutoAttack = 870, // Boss->player, no cast, single-target
        NumbingNoise = 18015, // Boss->self (front), 5.0s cast, range 13, 120 degree cone
        TailSnap = 18016, // Boss->self (behind), 5.0s cast, range 18, 120 degree cone
        BodySlam = 18018, // Boss->self, 5.0s cast, range 11 circle
        NumbingNoiseRotation = 18100, // rotation, pulls player in from 30 units, 3 attacks of NumbingNoise2
        NumbingNoiseDuringRotation = 18098, // during rotation, 0.3s cast time, 13 range, 120 degree cone
        TailSnapRotation = 18101, // rotation, pulls player in from 30 units, 3 attacks of TailSnap2
        TailSnapDuringRotation = 18099, // Boss->self (behind), 0.3s cast, range 18, 120 degree cone
    }

    class NumbingNoise : Components.SelfTargetedAOEs
    {
        public NumbingNoise() : base(ActionID.MakeSpell(AID.NumbingNoise), new AOEShapeCone(13,60.Degrees())) { } 
    }
    class NumbingNoiseRotation : Components.SelfTargetedAOEs
    {
        public NumbingNoiseRotation() : base(ActionID.MakeSpell(AID.NumbingNoiseRotation), new AOEShapeCone(13,60.Degrees())) { }
    }

   class NumbingNoiseRotationKB : Components.KnockbackFromCastTarget //TODO: pulls/attracts should probably have their own component to make this easier in future
    {
        public float PullDistance;
        public Angle Direction;
        public DateTime Activation;
        public float DistanceToBoss;
        public int active;
        public NumbingNoiseRotationKB() : base(ActionID.MakeSpell(AID.NumbingNoiseRotation),default) { }
        public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
        {
               var Boss = module.Enemies(OID.Boss).FirstOrDefault();
               var player = module.Raid.Player();
             if (Boss != null && player != null) 
                DistanceToBoss = (player.Position - Boss.Position).Length();
             if (Boss != null && player != null) 
                PullDistance = 30 - (Boss.HitboxRadius + player.HitboxRadius + (30-DistanceToBoss));
              if (Boss != null && player != null) 
              Direction=Angle.FromDirection(player.Position - Boss.Position);
             if (active>=1 && PullDistance > 0 && PullDistance <=25)
                yield return new(new(), PullDistance, Activation = default, null, Direction, Kind.TowardsOrigin);
            }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            base.OnCastStarted(module, caster, spell);
            if ((AID)spell.Action.ID == AID.NumbingNoiseRotation)
            active=1;
        }
                public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            base.OnCastStarted(module, caster, spell);
            if ((AID)spell.Action.ID == AID.NumbingNoiseRotation)
            active=0;
        }

            public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
             if (active>=1 && DistanceToBoss >= 13 && DistanceToBoss <=30)
            {
                hints.Add("Use anti knockback ability or get pulled into danger zone!");
            }
        }
    }

class TailSnapRotationKB : Components.KnockbackFromCastTarget //TODO: pulls/attracts should probably have their own component to make this easier in future
    {
        public float PullDistance;
        public Angle Direction;
        public DateTime Activation;
        public float DistanceToBoss;
        public int active;
        public TailSnapRotationKB() : base(ActionID.MakeSpell(AID.TailSnapRotation),default) { }
        public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
        {
               var Boss = module.Enemies(OID.Boss).FirstOrDefault();
               var player = module.Raid.Player();
             if (Boss != null && player != null) 
                DistanceToBoss = (player.Position - Boss.Position).Length();
             if (Boss != null && player != null) 
                PullDistance = 30 - (Boss.HitboxRadius + player.HitboxRadius + (30-DistanceToBoss));
              if (Boss != null && player != null) 
              Direction=Angle.FromDirection(player.Position - Boss.Position);
             if (active>=1 && PullDistance > 0 && PullDistance <=25)
                yield return new(new(), PullDistance, Activation = default, null, Direction, Kind.TowardsOrigin);
            }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            base.OnCastStarted(module, caster, spell);
            if ((AID)spell.Action.ID == AID.TailSnapRotation)
            active=1;
        }
                public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            base.OnCastStarted(module, caster, spell);
            if ((AID)spell.Action.ID == AID.TailSnapRotation)
            active=0;
        }

            public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
             if (active>=1 && DistanceToBoss >= 18 && DistanceToBoss <=30)
            {
                hints.Add("Use anti knockback ability or get pulled into danger zone!");
            }
        }
    }
    class NumbingNoiseDuringRotation : Components.SelfTargetedAOEs
    {
        public NumbingNoiseDuringRotation() : base(ActionID.MakeSpell(AID.NumbingNoiseDuringRotation), new AOEShapeCone(13,60.Degrees()),3) { }
    }
    class TailSnap : Components.SelfTargetedAOEs
    {
        public TailSnap() : base(ActionID.MakeSpell(AID.TailSnap), new AOEShapeCone(18,60.Degrees())) { }
    }
    class TailSnapRotation : Components.SelfTargetedAOEs
    
    {
        public TailSnapRotation() : base(ActionID.MakeSpell(AID.TailSnap), new AOEShapeCone(18,60.Degrees(), directionOffset: 180.Degrees())) { }
    }
    class TailSnapDuringRotation : Components.SelfTargetedAOEs
    {
        public TailSnapDuringRotation() : base(ActionID.MakeSpell(AID.TailSnapRotation), new AOEShapeCone(18,60.Degrees()),3) { }
    }
    class BodySlam : Components.SelfTargetedAOEs
    {
        public BodySlam() : base(ActionID.MakeSpell(AID.BodySlam), new AOEShapeCircle(11)) { }
    }
    class SugaarStates : StateMachineBuilder
    {
        public SugaarStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<NumbingNoise>()
                .ActivateOnEnter<TailSnap>()
                .ActivateOnEnter<TailSnapRotation>()
                .ActivateOnEnter<TailSnapRotationKB>()
                .ActivateOnEnter<NumbingNoiseRotation>()
                .ActivateOnEnter<NumbingNoiseRotationKB>()
                .ActivateOnEnter<NumbingNoiseDuringRotation>()
                .ActivateOnEnter<TailSnapDuringRotation>()
                .ActivateOnEnter<BodySlam>();
        }
    }

    public class Sugaar : SimpleBossModule
    {
        public Sugaar(WorldState ws, Actor primary) : base(ws, primary) { }
    }
}