// CONTRIB: made by malediktus, not checked
using System.Linq;

namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.AltarAiravata
{
    public enum OID : uint
    {
        Boss = 0x2543, //R=4.75
        BonusAdd_AltarMatanga = 0x2545, // R3.420
        BonusAdd_GoldWhisker = 0x2544, // R0.540
        BossHelper = 0x233C,
    };

    public enum AID : uint
    {
        AutoAttack = 870, // 2544->player, no cast, single-target
        AutoAttack2 = 872, // Boss,Matanaga->player, no cast, single-target
        Huff = 13371, // Boss->player, 3,0s cast, single-target
        HurlBoss = 13372, // Boss->location, 3,0s cast, range 6 circle
        Buffet = 13374, // Boss->player, 3,0s cast, single-target, knockback 20 forward
        SpinBoss = 13373, // Boss->self, 4,0s cast, range 30 120-degree cone
        BarbarousScream = 13375, // Boss->self, 3,5s cast, range 14 circle

        unknown = 9636, // BonusAdd_AltarMatanga->self, no cast, single-target
        Spin = 8599, // BonusAdd_AltarMatanga->self, no cast, range 6+R 120-degree cone
        RaucousScritch = 8598, // BonusAdd_AltarMatanga->self, 2,5s cast, range 5+R 120-degree cone
        Hurl = 5352, // BonusAdd_AltarMatanga->location, 3,0s cast, range 6 circle
        Telega = 9630, // BonusAdds->self, no cast, single-target, bonus adds disappear
    };

    public enum IconID : uint
    {
        BuffetTarget = 23, // player
    };

    class HurlBoss : Components.LocationTargetedAOEs
    {
        public HurlBoss() : base(ActionID.MakeSpell(AID.HurlBoss), 6) { }
    }

    class SpinBoss : Components.SelfTargetedAOEs
    {
        public SpinBoss() : base(ActionID.MakeSpell(AID.SpinBoss), new AOEShapeCone(30, 60.Degrees())) { }
    }

    class BarbarousScream : Components.SelfTargetedAOEs
    {
        public BarbarousScream() : base(ActionID.MakeSpell(AID.BarbarousScream), new AOEShapeCircle(13)) { }
    }

    class Huff : Components.SingleTargetCast
    {
        public Huff() : base(ActionID.MakeSpell(AID.Huff)) { }
    }

    class Buffet : Components.KnockbackFromCastTarget
    {
        private bool targeted;
        private Actor? target;

        public Buffet() : base(ActionID.MakeSpell(AID.Buffet), 20, kind: Kind.DirForward)
        {
            StopAtWall = true;
        }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == (uint)IconID.BuffetTarget)
            {
                targeted = true;
                target = actor;
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            base.OnCastFinished(module, caster, spell);
            if ((AID)spell.Action.ID == AID.Buffet)
                targeted = false;
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            base.AddHints(module, slot, actor, hints, movementHints);
            if (target == actor && targeted)
            {
                hints.Add("Damage + Knockback --> Cone attack on you");
            }
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.AddAIHints(module, slot, actor, assignment, hints);
            if (target == actor && targeted)
                hints.AddForbiddenZone(ShapeDistance.Circle(module.Bounds.Center, 18));
        }
    }

    class RaucousScritch : Components.SelfTargetedAOEs
    {
        public RaucousScritch() : base(ActionID.MakeSpell(AID.RaucousScritch), new AOEShapeCone(8.42f, 30.Degrees())) { }
    }

    class Hurl : Components.LocationTargetedAOEs
    {
        public Hurl() : base(ActionID.MakeSpell(AID.Hurl), 6) { }
    }

    class Spin : Components.Cleave
    {
        public Spin() : base(ActionID.MakeSpell(AID.Spin), new AOEShapeCone(9.42f, 60.Degrees()), (uint)OID.BonusAdd_AltarMatanga) { }
    }

    class AiravataStates : StateMachineBuilder
    {
        public AiravataStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<HurlBoss>()
                .ActivateOnEnter<SpinBoss>()
                .ActivateOnEnter<BarbarousScream>()
                .ActivateOnEnter<Huff>()
                .ActivateOnEnter<Buffet>()
                .ActivateOnEnter<Hurl>()
                .ActivateOnEnter<RaucousScritch>()
                .ActivateOnEnter<Spin>()
                .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BonusAdd_GoldWhisker).All(e => e.IsDead) && module.Enemies(OID.BonusAdd_AltarMatanga).All(e => e.IsDead);
        }
    }

    [ModuleInfo(CFCID = 586, NameID = 7601)]
    public class Airavata : BossModule
    {
        public Airavata(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 20)) { }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
            foreach (var s in Enemies(OID.BonusAdd_GoldWhisker))
                Arena.Actor(s, ArenaColor.Object);
            foreach (var s in Enemies(OID.BonusAdd_AltarMatanga))
                Arena.Actor(s, ArenaColor.Vulnerable);
        }

        public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.CalculateAIHints(slot, actor, assignment, hints);
            foreach (var e in hints.PotentialTargets)
            {
                e.Priority = (OID)e.Actor.OID switch
                {
                    OID.BonusAdd_GoldWhisker => 3,
                    OID.BonusAdd_AltarMatanga => 2,
                    OID.Boss => 1,
                    _ => 0
                };
            }
        }
    }
}
