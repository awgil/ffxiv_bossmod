// CONTRIB: made by malediktus, not checked
using System.Linq;

namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.AltarDiresaur
{
    public enum OID : uint
    {
        Boss = 0x253A, //R=6.6
        BossAdd = 0x256F, //R=4.0
        BossHelper = 0x233C,
        BonusAdd_AltarMatanga = 0x2545, // R3.420
        BonusAdd_GoldWhisker = 0x2544, // R0.540
        FireVoidzone = 0x1EA140,
    };

    public enum AID : uint
    {
        AutoAttack = 870, // Boss/2544->player, no cast, single-target
        AutoAttack2 = 872, // BonusAdd_AltarMatanga->player, no cast, single-target
        AutoAttack3 = 6497, // 256F->player, no cast, single-target
        DeadlyHold = 13217, // Boss->player, 3,0s cast, single-target
        HeatBreath = 13218, // Boss->self, 3,0s cast, range 8+R 90-degree cone
        TailSmash = 13220, // Boss->self, 3,0s cast, range 20+R 90-degree cone
        RagingInferno = 13283, // Boss->self, 3,0s cast, range 60 circle
        Comet = 13835, // BossHelper->location, 3,0s cast, range 4 circle
        HardStomp = 13743, // 256F->self, 3,0s cast, range 6+R circle
        Fireball = 13219, // Boss->location, 3,0s cast, range 6 circle

        unknown = 9636, // BonusAdd_AltarMatanga->self, no cast, single-target
        Spin = 8599, // BonusAdd_AltarMatanga->self, no cast, range 6+R 120-degree cone
        RaucousScritch = 8598, // BonusAdd_AltarMatanga->self, 2,5s cast, range 5+R 120-degree cone
        Hurl = 5352, // BonusAdd_AltarMatanga->location, 3,0s cast, range 6 circle
        Telega = 9630, // BonusAdds->self, no cast, single-target, bonus adds disappear
    };

    public enum IconID : uint
    {
        Baitaway = 23, // player
    };

    class DeadlyHold : Components.SingleTargetCast
    {
        public DeadlyHold() : base(ActionID.MakeSpell(AID.DeadlyHold)) { }
    }

    class HeatBreath : Components.SelfTargetedAOEs
    {
        public HeatBreath() : base(ActionID.MakeSpell(AID.HeatBreath), new AOEShapeCone(14.6f, 45.Degrees())) { }
    }

    class TailSmash : Components.SelfTargetedAOEs
    {
        public TailSmash() : base(ActionID.MakeSpell(AID.TailSmash), new AOEShapeCone(26.6f, 45.Degrees())) { }
    }

    class RagingInferno : Components.RaidwideCast
    {
        public RagingInferno() : base(ActionID.MakeSpell(AID.RagingInferno)) { }
    }

    class Comet : Components.LocationTargetedAOEs
    {
        public Comet() : base(ActionID.MakeSpell(AID.Comet), 4) { }
    }

    class HardStomp : Components.SelfTargetedAOEs
    {
        public HardStomp() : base(ActionID.MakeSpell(AID.HardStomp), new AOEShapeCircle(10)) { }
    }

    class Fireball : Components.UniformStackSpread
    {
        public Fireball() : base(0, 6, alwaysShowSpreads: true) { }
        private bool targeted;
        private int numcasts;
        private Actor? target;
        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == (uint)IconID.Baitaway)
            {
                AddSpread(actor);
                targeted = true;
                target = actor;
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.Fireball)
                ++numcasts;
            if (numcasts == 3)
            {
                Spreads.Clear();
                numcasts = 0;
                targeted = false;
            }
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.AddAIHints(module, slot, actor, assignment, hints);
            if (target == actor && targeted)
                hints.AddForbiddenZone(ShapeDistance.Circle(module.Bounds.Center, 18));
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (target == actor && targeted)
                hints.Add("Bait away (3 times!)");
        }
    }

    class FireballVoidzone : Components.PersistentVoidzoneAtCastTarget
    {
        public FireballVoidzone() : base(6, ActionID.MakeSpell(AID.Fireball), m => m.Enemies(OID.FireVoidzone), 0) { }
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

    class DiresaurStates : StateMachineBuilder
    {
        public DiresaurStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<DeadlyHold>()
                .ActivateOnEnter<HeatBreath>()
                .ActivateOnEnter<TailSmash>()
                .ActivateOnEnter<RagingInferno>()
                .ActivateOnEnter<Comet>()
                .ActivateOnEnter<HardStomp>()
                .ActivateOnEnter<Fireball>()
                .ActivateOnEnter<FireballVoidzone>()
                .ActivateOnEnter<Hurl>()
                .ActivateOnEnter<RaucousScritch>()
                .ActivateOnEnter<Spin>()
                .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BossAdd).All(e => e.IsDead) && module.Enemies(OID.BonusAdd_GoldWhisker).All(e => e.IsDead) && module.Enemies(OID.BonusAdd_AltarMatanga).All(e => e.IsDead);
        }
    }

    [ModuleInfo(CFCID = 586, NameID = 7627)]
    public class Diresaur : BossModule
    {
        public Diresaur(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 20)) { }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
            foreach (var s in Enemies(OID.BossAdd))
                Arena.Actor(s, ArenaColor.Object);
            foreach (var s in Enemies(OID.BonusAdd_GoldWhisker))
                Arena.Actor(s, ArenaColor.Vulnerable);
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
                    OID.BonusAdd_GoldWhisker => 4,
                    OID.BonusAdd_AltarMatanga => 3,
                    OID.BossAdd => 2,
                    OID.Boss => 1,
                    _ => 0
                };
            }
        }
    }
}
