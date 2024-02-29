// CONTRIB: made by malediktus, not checked
using System.Linq;

namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.SecretPegasus
{
    public enum OID : uint
    {
        Boss = 0x3016, //R=2.5
        Thunderhead = 0x3017, //R=1.0, untargetable
        BossHelper = 0x233C,
        BonusAdd_TheKeeperOfTheKeys = 0x3034, // R3.230
    };

    public enum AID : uint
    {
        AutoAttack = 872, // Boss/BonusAdd_TheKeeperOfTheKeys->player, no cast, single-target
        BurningBright = 21667, // Boss->self, 3,0s cast, range 47 width 6 rect
        Nicker = 21668, // Boss->self, 4,0s cast, range 12 circle
        CloudCall = 21666, // Boss->self, 3,0s cast, single-target, calls clouds
        Gallop = 21665, // Boss->players, no cast, width 10 rect charge
        LightningBolt = 21669, // Thunderhead->self, 3,0s cast, range 8 circle

        Telega = 9630, // BonusAdds->self, no cast, single-target, bonus adds disappear
        Mash = 21767, // 3034->self, 3,0s cast, range 13 width 4 rect
        Inhale = 21770, // 3034->self, no cast, range 20 120-degree cone, attract 25 between hitboxes, shortly before Spin
        Spin = 21769, // 3034->self, 4,0s cast, range 11 circle
        Scoop = 21768, // 3034->self, 4,0s cast, range 15 120-degree cone
    };

    class BurningBright : Components.SelfTargetedAOEs
    {
        public BurningBright() : base(ActionID.MakeSpell(AID.BurningBright), new AOEShapeRect(47, 3)) { }
    }

    class Nicker : Components.SelfTargetedAOEs
    {
        public Nicker() : base(ActionID.MakeSpell(AID.Nicker), new AOEShapeCircle(12)) { }
    }

    class CloudCall : Components.CastHint
    {
        public CloudCall() : base(ActionID.MakeSpell(AID.CloudCall), "Calls thunderclouds") { }
    }

    class LightningBolt : Components.SelfTargetedAOEs
    {
        public LightningBolt() : base(ActionID.MakeSpell(AID.LightningBolt), new AOEShapeCircle(8)) { }
    }

    class Spin : Components.SelfTargetedAOEs
    {
        public Spin() : base(ActionID.MakeSpell(AID.Spin), new AOEShapeCircle(11)) { }
    }

    class Mash : Components.SelfTargetedAOEs
    {
        public Mash() : base(ActionID.MakeSpell(AID.Mash), new AOEShapeRect(13, 2)) { }
    }

    class Scoop : Components.SelfTargetedAOEs
    {
        public Scoop() : base(ActionID.MakeSpell(AID.Scoop), new AOEShapeCone(15, 60.Degrees())) { }
    }

    class PegasusStates : StateMachineBuilder
    {
        public PegasusStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<BurningBright>()
                .ActivateOnEnter<Nicker>()
                .ActivateOnEnter<CloudCall>()
                .ActivateOnEnter<LightningBolt>()
                .ActivateOnEnter<Spin>()
                .ActivateOnEnter<Mash>()
                .ActivateOnEnter<Scoop>()
                .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BonusAdd_TheKeeperOfTheKeys).All(e => e.IsDead);
        }
    }

    [ModuleInfo(CFCID = 745, NameID = 9793)]
    public class Pegasus : BossModule
    {
        public Pegasus(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 20)) { }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
            foreach (var s in Enemies(OID.Thunderhead).Where(x => !x.IsDead))
                Arena.Actor(s, ArenaColor.Object, true);
            foreach (var s in Enemies(OID.BonusAdd_TheKeeperOfTheKeys))
                Arena.Actor(s, ArenaColor.Vulnerable);
        }

        public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.CalculateAIHints(slot, actor, assignment, hints);
            foreach (var e in hints.PotentialTargets)
            {
                e.Priority = (OID)e.Actor.OID switch
                {
                    OID.BonusAdd_TheKeeperOfTheKeys => 2,
                    OID.Boss => 1,
                    _ => 0
                };
            }
        }
    }
}
