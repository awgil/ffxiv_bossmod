// CONTRIB: made by malediktus, not checked
using System.Linq;

namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.Hati
{
    public enum OID : uint
    {
        Boss = 0x2538, //R=5.4
        BossAdd = 0x2569, //R=3.0
        BossHelper = 0x233C,
        IceVoidzone = 0x1E8D9C,
    };

    public enum AID : uint
    {
        AutoAttack = 870, // Boss->player, no cast, single-target
        AutoAttack2 = 6499, // BossAdd->player, no cast, single-target
        GlassyNova = 13362, // Boss->self, 3,0s cast, range 40+R width 8 rect
        Hellstorm = 13359, // Boss->self, 3,0s cast, single-target
        Hellstorm2 = 13363, // BossHelper->location, 3,5s cast, range 10 circle
        Netherwind = 13741, // BossAdd->self, 3,0s cast, range 15+R width 4 rect
        BrainFreeze = 13361, // Boss->self, 4,0s cast, range 10+R circle, turns player into Imp
        PolarRoar = 13360, // Boss->self, 3,0s cast, range 9-40 donut
    };

    class PolarRoar : Components.SelfTargetedAOEs
    {
        public PolarRoar() : base(ActionID.MakeSpell(AID.PolarRoar), new AOEShapeDonut(9, 40)) { }
    }

    class Hellstorm : Components.LocationTargetedAOEs
    {
        public Hellstorm() : base(ActionID.MakeSpell(AID.Hellstorm2), 10) { }
    }

    class Netherwind : Components.SelfTargetedAOEs
    {
        public Netherwind() : base(ActionID.MakeSpell(AID.Netherwind), new AOEShapeRect(18, 2)) { }
    }

    class GlassyNova : Components.SelfTargetedAOEs
    {
        public GlassyNova() : base(ActionID.MakeSpell(AID.GlassyNova), new AOEShapeRect(45.4f, 4)) { }
    }

    class BrainFreeze : Components.SelfTargetedAOEs
    {
        public BrainFreeze() : base(ActionID.MakeSpell(AID.BrainFreeze), new AOEShapeCircle(15.4f)) { }
    }

    class HatiStates : StateMachineBuilder
    {
        public HatiStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<PolarRoar>()
                .ActivateOnEnter<Hellstorm>()
                .ActivateOnEnter<Netherwind>()
                .ActivateOnEnter<BrainFreeze>()
                .ActivateOnEnter<GlassyNova>()
                .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BossAdd).All(e => e.IsDead);
        }
    }

    [ModuleInfo(CFCID = 586, NameID = 7590)]
    public class Hati : BossModule
    {
        public Hati(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 20)) { }

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
