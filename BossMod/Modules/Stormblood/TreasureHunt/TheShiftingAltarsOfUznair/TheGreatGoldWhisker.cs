// CONTRIB: made by malediktus, not checked
using System.Linq;

namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.TheGreatGoldWhisker
{
    public enum OID : uint
    {
        Boss = 0x2541, //R=2.4
        BossHelper = 0x233C,
        BonusAdd_GoldWhisker = 0x2544, // R0.540
    };

    public enum AID : uint
    {
        AutoAttack = 870, // Boss/BonusAdd_GoldWhisker->player, no cast, single-target
        TripleTrident = 13364, // Boss->players, 3,0s cast, single-target
        Tingle = 13365, // Boss->self, 4,0s cast, range 10+R circle
        FishOutOfWater = 13366, // Boss->self, 3,0s cast, single-target
        Telega = 9630, // BonusAdd_GoldWhisker->self, no cast, single-target
    };

    class TripleTrident : Components.SingleTargetCast
    {
        public TripleTrident() : base(ActionID.MakeSpell(AID.TripleTrident)) { }
    }

    class FishOutOfWater : Components.CastHint
    {
        public FishOutOfWater() : base(ActionID.MakeSpell(AID.FishOutOfWater), "Spawns adds") { }
    }

    class Tingle : Components.SelfTargetedAOEs
    {
        public Tingle() : base(ActionID.MakeSpell(AID.Tingle), new AOEShapeCircle(12.4f)) { }
    }

    class TheGreatGoldWhiskerStates : StateMachineBuilder
    {
        public TheGreatGoldWhiskerStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<TripleTrident>()
                .ActivateOnEnter<FishOutOfWater>()
                .ActivateOnEnter<Tingle>()
                .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BonusAdd_GoldWhisker).All(e => e.IsDead);
        }
    }

    [ModuleInfo(CFCID = 586, NameID = 7599)]
    public class TheGreatGoldWhisker : BossModule
    {
        public TheGreatGoldWhisker(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 20)) { }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
            foreach (var s in Enemies(OID.BonusAdd_GoldWhisker))
                Arena.Actor(s, ArenaColor.Vulnerable);
        }

        public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.CalculateAIHints(slot, actor, assignment, hints);
            foreach (var e in hints.PotentialTargets)
            {
                e.Priority = (OID)e.Actor.OID switch
                {
                    OID.BonusAdd_GoldWhisker => 2,
                    OID.Boss => 1,
                    _ => 0
                };
            }
        }
    }
}
