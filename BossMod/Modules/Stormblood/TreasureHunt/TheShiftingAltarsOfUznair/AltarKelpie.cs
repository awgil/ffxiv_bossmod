// CONTRIB: made by malediktus, not checked
using System.Linq;

namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.AltarKelpie
{
    public enum OID : uint
    {
        Boss = 0x2537, //R=5.4
        Hydrosphere = 0x255B, //R=1.2
        BonusAdd_AltarMatanga = 0x2545, // R3.420
        BonusAdd_GoldWhisker = 0x2544, // R0.540
        BossHelper = 0x233C,
    };

    public enum AID : uint
    {
        AutoAttack = 870, // 2544->player, no cast, single-target
        AutoAttack2 = 872, // Boss->player, no cast, single-target
        Torpedo = 13438, // Boss->player, 3,0s cast, single-target
        Innocence = 13439, // Boss->location, 3,0s cast, range 5 circle
        Gallop = 13441, // Boss->location, no cast, ???, movement ability
        RisingSeas = 13440, // Boss->self, 5,0s cast, range 50+R circle, knockback 20, away from source
        BloodyPuddle = 13443, // Hydrosphere->self, 4,0s cast, range 10+R circle
        HydroPush = 13442, // Boss->self, 6,0s cast, range 44+R width 44 rect, knockback 20, dir forward

        unknown = 9636, // BonusAdd_AltarMatanga->self, no cast, single-target
        Spin = 8599, // BonusAdd_AltarMatanga->self, no cast, range 6+R 120-degree cone
        RaucousScritch = 8598, // BonusAdd_AltarMatanga->self, 2,5s cast, range 5+R 120-degree cone
        Hurl = 5352, // BonusAdd_AltarMatanga->location, 3,0s cast, range 6 circle
        Telega = 9630, // BonusAdds->self, no cast, single-target, bonus adds disappear
    };

    class Innocence : Components.LocationTargetedAOEs
    {
        public Innocence() : base(ActionID.MakeSpell(AID.Innocence), 5) { }
    }

    class HydroPush : Components.SelfTargetedAOEs
    {
        public HydroPush() : base(ActionID.MakeSpell(AID.HydroPush), new AOEShapeRect(49.4f, 22)) { }
    }

    class BloodyPuddle : Components.SelfTargetedAOEs
    {
        public BloodyPuddle() : base(ActionID.MakeSpell(AID.BloodyPuddle), new AOEShapeCircle(11.2f)) { }
    }

    class Torpedo : Components.SingleTargetCast
    {
        public Torpedo() : base(ActionID.MakeSpell(AID.Torpedo)) { }
    }

    class RisingSeas : Components.RaidwideCast
    {
        public RisingSeas() : base(ActionID.MakeSpell(AID.RisingSeas)) { }
    }

    class HydroPushKB : Components.KnockbackFromCastTarget
    {
        public HydroPushKB() : base(ActionID.MakeSpell(AID.HydroPush), 20, shape: new AOEShapeRect(49.4f, 22), kind: Kind.DirForward)
        {
            StopAtWall = true;
        }
    }

    class RisingSeasKB : Components.KnockbackFromCastTarget
    {
        public RisingSeasKB() : base(ActionID.MakeSpell(AID.RisingSeas), 20)
        {
            StopAtWall = true;
        }
        public override bool DestinationUnsafe(BossModule module, int slot, Actor actor, WPos pos) => module.FindComponent<BloodyPuddle>()?.ActiveAOEs(module, slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false;
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

    class KelpieStates : StateMachineBuilder
    {
        public KelpieStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<Innocence>()
                .ActivateOnEnter<HydroPush>()
                .ActivateOnEnter<BloodyPuddle>()
                .ActivateOnEnter<Torpedo>()
                .ActivateOnEnter<RisingSeas>()
                .ActivateOnEnter<RisingSeasKB>()
                .ActivateOnEnter<HydroPushKB>()
                .ActivateOnEnter<Hurl>()
                .ActivateOnEnter<RaucousScritch>()
                .ActivateOnEnter<Spin>()
                .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BonusAdd_GoldWhisker).All(e => e.IsDead) && module.Enemies(OID.BonusAdd_AltarMatanga).All(e => e.IsDead);
        }
    }

    [ModuleInfo(CFCID = 586, NameID = 7589)]
    public class Kelpie : BossModule
    {
        public Kelpie(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 20)) { }

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
