using System.Linq;

namespace BossMod.Endwalker.FATE.Daivadipa
{
    public enum OID : uint
    {
        Boss = 0x356D, // R=8.0
        OrbOfImmolationBlue = 0x3570, //R=1.0
        OrbOfImmolationRed = 0x356F, //R=1.0
        OrbOfConflagrationBlue = 0x3572, //R=1.0
        OrbOfConflagrationRed = 0x3571, //R=1.0
        Helper1 = 0x3573, //R=0.5
        Helper2 = 0x3574, //R=0.5
        Helper3 = 0x3575, //R=0.5
    };

    public enum AID : uint
    {
        AutoAttack = 872, // Boss->player, no cast, single-target
        Drumbeat = 26510, // Boss->player, 5,0s cast, single-target
        LeftwardTrisula = 26508, // Boss->self, 7,0s cast, range 65 180-degree cone
        RightwardParasu = 26509, // Boss->self, 7,0s cast, range 65 180-degree cone
        Lamplight = 26497, // Boss->self, 2,0s cast, single-target
        LoyalFlame = 26499, // Boss->self, 5,0s cast, single-target, blue first
        LoyalFlame2 = 26498, // Boss->self, 5,0s cast, single-target, red first
        LitPath1 = 26501, // OrbOfImmolation->self, 1,0s cast, range 50 width 10 rect
        LitPath2 = 26500, // OrbOfImmolation2->self, 1,0s cast, range 50 width 10 rect
        CosmicWeave = 26513, // Boss->self, 4,0s cast, range 18 circle
        YawningHells = 26511, // Boss->self, no cast, single-target
        YawningHells2 = 26512, // Helper1->location, 3,0s cast, range 8 circle
        ErrantAkasa = 26514, // Boss->self, 5,0s cast, range 60 90-degree cone
        InfernalRedemption = 26517, // Boss->self, 5,0s cast, single-target
        InfernalRedemption2 = 26518, // Helper3->location, no cast, range 60 circle
        IgnitingLights = 26503, // Boss->self, 2,0s cast, single-target
        IgnitingLights2 = 26502, // Boss->self, 2,0s cast, single-target
        Burn = 26507, // OrbOfConflagration->self, 1,0s cast, range 10 circle
        Burn2 = 26506, // OrbOfConflagration2->self, 1,0s cast, range 10 circle
        KarmicFlames = 26515, // Boss->self, 5,5s cast, single-target
        KarmicFlames2 = 26516, // Helper2->location, 5,0s cast, range 50 circle, damage fall off, safe distance should be about 20
        DivineCall = 27080, // Boss->self, 4,0s cast, range 65 circle, forced backwards march
        DivineCall2 = 26520, // Boss->self, 4,0s cast, range 65 circle, forced right march
        DivineCall3 = 27079, // Boss->self, 4,0s cast, range 65 circle, forced forward march
        DivineCall4 = 26519, // Boss->self, 4,0s cast, range 65 circle, forced left march
    };

    public enum SID : uint
    {
        Hover = 1515, // none->OrbOfImmolation2, extra=0x64
        AboutFace = 1959, // Boss->player, extra=0x0
        RightFace = 1961, // Boss->player, extra=0x0
        ForwardMarch = 1958, // Boss->player, extra=0x0
        LeftFace = 1960, // Boss->player, extra=0x0
        ForcedMarch = 1257, // Boss->player, extra=0x2/0x8/0x1/0x4
    };

    class Drumbeat : Components.SingleTargetCast
    {
        public Drumbeat() : base(ActionID.MakeSpell(AID.Drumbeat)) { }
    }

    class LeftwardTrisula : Components.SelfTargetedAOEs
    {
        public LeftwardTrisula() : base(ActionID.MakeSpell(AID.LeftwardTrisula), new AOEShapeCone(65, 90.Degrees())) { }
    }

    class RightwardParasu : Components.SelfTargetedAOEs
    {
        public RightwardParasu() : base(ActionID.MakeSpell(AID.RightwardParasu), new AOEShapeCone(65, 90.Degrees())) { }
    }

    class ErrantAkasa : Components.SelfTargetedAOEs
    {
        public ErrantAkasa() : base(ActionID.MakeSpell(AID.ErrantAkasa), new AOEShapeCone(60, 45.Degrees())) { }
    }

    class CosmicWeave : Components.SelfTargetedAOEs
    {
        public CosmicWeave() : base(ActionID.MakeSpell(AID.CosmicWeave), new AOEShapeCircle(18)) { }
    }

    class YawningHells : Components.LocationTargetedAOEs
    {
        public YawningHells() : base(ActionID.MakeSpell(AID.YawningHells2), 8) { }
    }

    class InfernalRedemption : Components.RaidwideCast //Note: actual raidwide happens about 1s later by helper with 0s cast
    {
        public InfernalRedemption() : base(ActionID.MakeSpell(AID.InfernalRedemption)) { }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            foreach (var c in Casters)
                hints.PredictedDamage.Add((module.Raid.WithSlot().Mask(), c.CastInfo!.NPCFinishAt.AddSeconds(1)));
        }
    }

    class DivineCall : Components.StatusDrivenForcedMarch
    {
        public DivineCall() : base(2, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace) { }

        public override bool DestinationUnsafe(BossModule module, int slot, Actor actor, WPos pos)
        {
            if (module.FindComponent<LeftwardTrisula>() != null && module.FindComponent<LeftwardTrisula>()!.ActiveAOEs(module, slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)))
                return true;
            if (module.FindComponent<RightwardParasu>() != null && module.FindComponent<RightwardParasu>()!.ActiveAOEs(module, slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)))
                return true;
            else
                return false;
        }

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (module.PrimaryActor.CastInfo?.IsSpell(AID.DivineCall) ?? false)
                hints.Add("Apply backwards march debuff");
            if (module.PrimaryActor.CastInfo?.IsSpell(AID.DivineCall2) ?? false)
                hints.Add("Apply right march debuff");
            if (module.PrimaryActor.CastInfo?.IsSpell(AID.DivineCall3) ?? false)
                hints.Add("Apply forwards march debuff");
            if (module.PrimaryActor.CastInfo?.IsSpell(AID.DivineCall4) ?? false)
                hints.Add("Apply left march debuff");
        }
    }

    class DaivadipaStates : StateMachineBuilder
    {
        public DaivadipaStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<Drumbeat>()
                .ActivateOnEnter<LeftwardTrisula>()
                .ActivateOnEnter<RightwardParasu>()
                .ActivateOnEnter<DivineCall>()
                .ActivateOnEnter<InfernalRedemption>()
                .ActivateOnEnter<CosmicWeave>()
                .ActivateOnEnter<YawningHells>()
                .ActivateOnEnter<ErrantAkasa>();
                // .ActivateOnEnter<CivilizationBuster2>()
                // .ActivateOnEnter<Touchdown>()
                // .ActivateOnEnter<PillarImpact>()
                // .ActivateOnEnter<PillarPierce>()
                // .ActivateOnEnter<AncientAevis>()
                // .ActivateOnEnter<HeadlongRush>()
                // .ActivateOnEnter<Aether>();
        }
    }

    [ModuleInfo(FateID = 1763)]
    public class Daivadipa : BossModule
    {
        public Daivadipa(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(-608, 811), 24.5f)) { }
    }
}
