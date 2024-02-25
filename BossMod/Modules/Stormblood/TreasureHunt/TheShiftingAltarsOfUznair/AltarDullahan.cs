// CONTRIB: made by malediktus, not checked
using System.Linq;

namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.AltarDullahan
{
    public enum OID : uint
    {
        Boss = 0x2533, //R=3.8
        BossAdd = 0x2563, //R=1.8
        BossHelper = 0x233C,
    };

    public enum AID : uint
    {
        AutoAttack = 870, // 2533->player, no cast, single-target
        AutoAttack2 = 6497, // 2563->player, no cast, single-target
        IronJustice = 13316, // 2533->self, 3,0s cast, range 8+R 120-degree cone
        Cloudcover = 13477, // 2533->location, 3,0s cast, range 6 circle
        TerrorEye = 13644, // 2563->location, 3,5s cast, range 6 circle
        StygianRelease = 13314, // 2533->self, 3,5s cast, range 50+R circle, small raidwide dmg, knockback 20 from source
        VillainousRebuke = 13315, // 2533->players, 4,5s cast, range 6 circle
    };

    class IronJustice : Components.SelfTargetedAOEs
    {
        public IronJustice() : base(ActionID.MakeSpell(AID.IronJustice), new AOEShapeCone(11.8f, 60.Degrees())) { }
    }

    class Cloudcover : Components.LocationTargetedAOEs
    {
        public Cloudcover() : base(ActionID.MakeSpell(AID.Cloudcover), 6) { }
    }

    class TerrorEye : Components.LocationTargetedAOEs
    {
        public TerrorEye() : base(ActionID.MakeSpell(AID.TerrorEye), 6) { }
    }

    class VillainousRebuke : Components.StackWithCastTargets
    {
        public VillainousRebuke() : base(ActionID.MakeSpell(AID.VillainousRebuke), 6) { }
    }

    class StygianRelease : Components.RaidwideCast
    {
        public StygianRelease() : base(ActionID.MakeSpell(AID.StygianRelease)) { }
    }

    class StygianReleaseKB : Components.KnockbackFromCastTarget
    {
        public StygianReleaseKB() : base(ActionID.MakeSpell(AID.StygianRelease), 20)
        {
            StopAtWall = true;
        }
        public override bool DestinationUnsafe(BossModule module, int slot, Actor actor, WPos pos) => module.FindComponent<TerrorEye>()?.ActiveAOEs(module, slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false;
    }

    class DullahanStates : StateMachineBuilder
    {
        public DullahanStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<IronJustice>()
                .ActivateOnEnter<Cloudcover>()
                .ActivateOnEnter<TerrorEye>()
                .ActivateOnEnter<VillainousRebuke>()
                .ActivateOnEnter<StygianRelease>()
                .ActivateOnEnter<StygianReleaseKB>()
                .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BossAdd).All(e => e.IsDead);
        }
    }

    [ModuleInfo(CFCID = 586, NameID = 7585)]
    public class Dullahan : BossModule
    {
        public Dullahan(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 20)) { }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
            foreach (var s in Enemies(OID.BossAdd))
                Arena.Actor(s, ArenaColor.Object);
        }

        public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.CalculateAIHints(slot, actor, assignment, hints);
            foreach (var e in hints.PotentialTargets)
            {
                e.Priority = (OID)e.Actor.OID switch
                {
                    OID.BossAdd => 2,
                    OID.Boss => 1,
                    _ => 0
                };
            }
        }
    }
}
