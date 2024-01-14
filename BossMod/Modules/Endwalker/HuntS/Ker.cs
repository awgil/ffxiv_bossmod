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
        WhisperedIncantation = 27645, // Boss->self, 5.0s cast, single-target, applies status to boss, remembers next skill
        EternalDamnation = 27647, // Boss->self, 6.0s cast, range 40 circle gaze
        AncientFlare = 27704, // Boss->self, 6.0s cast, range 40 circle, applies pyretic
        WhispersManifest = 27706, // Boss->self, 6,0s cast, range 40 circle, applies pyretc (remembered skill from Whispered Incantation)
        WhispersManifest2 = 27653, // Boss->self, 6,0s cast, range 40 circle (Ancient Holy? to be verified)
        MirroredIncantation = 27927, // Boss->self, 3,0s cast, single-target, mirrors the next 3 interments
        MirroredIncantation2 = 27928, // Boss->self, 3,0s cast, single-target, mirrors the next 4 interments
        Mirrored_RightInterment = 27663, // Boss->self, 6,0s cast, range 40 180-degree cone
        Mirrored_LeftInterment = 27664, // Boss->self, 6,0s cast, range 40 180-degree cone
        Mirrored_ForeInterment = 27661, // Boss->self, 6,0s cast, range 40 180-degree cone
        Mirrored_RearInterment = 27662, // Boss->self, 6,0s cast, range 40 180-degree cone
        unknown = 25698, // Boss->player, no cast, single-target
        AncientHoly = 27646, // Boss->self, 6,0s cast, range 40 circle
    };
    public enum SID : uint
    {
        TemporaryMisdirection = 1422, // Boss->player, extra=0x2D0
        WhisperedIncantation = 2846, // Boss->Boss, extra=0x0
        MirroredIncantation = 2848, // Boss->Boss, extra=0x3/0x2/0x1/0x4
        Pyretic = 960, // Boss->player, extra=0x0
        WhispersManifest = 2847, // Boss->Boss, extra=0x0
    };

    class MinaxGlare : Components.CastHint
    {
        public MinaxGlare() : base(ActionID.MakeSpell(AID.MinaxGlare), "Applies temporary misdirection") { }
    }

    class Heliovoid : Components.SelfTargetedAOEs
    {
        public Heliovoid() : base(ActionID.MakeSpell(AID.Heliovoid), new AOEShapeCircle(12)) { }
    }

    class AncientBlizzard : Components.SelfTargetedAOEs
    {
        public AncientBlizzard() : base(ActionID.MakeSpell(AID.AncientBlizzard), new AOEShapeDonut(8, 40)) { } // TODO: verify inner radius
    }

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
    class Mirrored_ForeInterment : Components.SelfTargetedAOEs
    {
        public Mirrored_ForeInterment() : base(ActionID.MakeSpell(AID.Mirrored_ForeInterment), new AOEShapeCone(40, 90.Degrees())) { }
    }

    class Mirrored_RearInterment : Components.SelfTargetedAOEs
    {
        public Mirrored_RearInterment() : base(ActionID.MakeSpell(AID.Mirrored_RearInterment), new AOEShapeCone(40, 90.Degrees())) { }
    }
    class Mirrored_RightInterment : Components.SelfTargetedAOEs
    {
        public Mirrored_RightInterment() : base(ActionID.MakeSpell(AID.Mirrored_RightInterment), new AOEShapeCone(40, 90.Degrees())) { }
    }

    class Mirrored_LeftInterment : Components.SelfTargetedAOEs
    {
        public Mirrored_LeftInterment() : base(ActionID.MakeSpell(AID.Mirrored_LeftInterment), new AOEShapeCone(40, 90.Degrees())) { }
    }
    class EternalDamnation : Components.CastGaze
    {
        public EternalDamnation() : base(ActionID.MakeSpell(AID.EternalDamnation)) { }
    }
    class WhisperedIncantation : Components.CastHint
    {
        public WhisperedIncantation() : base(ActionID.MakeSpell(AID.WhisperedIncantation), "Remembers the next skill and uses it again when casting Whispers Manifest") { }
    }
    class MirroredIncantation : Components.CastHint
    {
        private int Mirrorstacks;
        private bool casting;
        public MirroredIncantation() : base(ActionID.MakeSpell(AID.MirroredIncantation), "") { }
        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.MirroredIncantation)
                casting = true;
        }
        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.MirroredIncantation)
                casting = false;
        }
       public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            if (actor == module.PrimaryActor)
             switch ((SID)status.ID)
                {
                    case SID.MirroredIncantation:
                      var stacks = status.Extra switch
                    {
                        0x1 => 1,
                        0x2 => 2,
                        0x3 => 3,
                        0x4 => 4,
                        _ => 0
                    };
                    Mirrorstacks = stacks;
                    break;
            }
        }
        public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
        {
        if (actor == module.PrimaryActor)
            {if ((SID)status.ID == SID.MirroredIncantation)
                {
                    Mirrorstacks = 0;
                }
            }
        }
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (Mirrorstacks > 0)
                  hints.Add($"Mirrored interments left: {Mirrorstacks}!");
            if (casting == true)
                  hints.Add("The next three interments will be mirrored!");           
        }
    }
      class MirroredIncantation2 : Components.CastHint
    {
        public MirroredIncantation2() : base(ActionID.MakeSpell(AID.MirroredIncantation2), "The next four interments will be mirrored!") { }
    }
    class AncientFlare : Components.CastHint
    {
        private bool pyretic;
        private bool casting;
        public AncientFlare() : base(ActionID.MakeSpell(AID.AncientFlare), "") { }
        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.AncientFlare)
                casting = true;
        }
        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.AncientFlare)
                casting = false;
        }
        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            var player = module.Raid.Player();
            if (actor == player)
            {if ((SID)status.ID == SID.Pyretic)
                {
                    pyretic = true;
                }
            }
        }
        public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
        {
            var player = module.Raid.Player();
            if (actor == player)
            {if ((SID)status.ID == SID.Pyretic)
                {
                    pyretic = false;
                }
            }
        }
        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (pyretic == true)
            hints.Add("Pyretic on you! STOP everything!");
            if (casting == true)
            hints.Add("Applies Pyretic - STOP everything until it runs out!");  
        }
    }
     class AncientFlareWhispersManifest : Components.CastHint
    {
        public AncientFlareWhispersManifest() : base(ActionID.MakeSpell(AID.WhispersManifest), "Applies Pyretic - STOP everything until it runs out!") { }
    }
    class AncientHoly : Components.RaidwideCast
    {        public AncientHoly() : base(ActionID.MakeSpell(AID.AncientHoly)) { }
    }
    class AncientHolyWhispersManifest : Components.RaidwideCast
    {        public AncientHolyWhispersManifest() : base(ActionID.MakeSpell(AID.AncientHoly)) { }
    }
    // TODO: wicked swipe, check if there are even more skills missing
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
                .ActivateOnEnter<Mirrored_ForeInterment>()
                .ActivateOnEnter<Mirrored_RearInterment>()
                .ActivateOnEnter<Mirrored_RightInterment>()
                .ActivateOnEnter<Mirrored_LeftInterment>()
                .ActivateOnEnter<MirroredIncantation>()
                .ActivateOnEnter<MirroredIncantation2>()
                .ActivateOnEnter<EternalDamnation>()
                .ActivateOnEnter<AncientFlare>()
                .ActivateOnEnter<AncientHoly>()
                .ActivateOnEnter<AncientHolyWhispersManifest>()
                .ActivateOnEnter<AncientFlareWhispersManifest>();
        }
    }

    public class Ker : SimpleBossModule
    {
        public Ker(WorldState ws, Actor primary) : base(ws, primary) { }
    }
}
