using BossMod.Components;

// CONTRIB: made by malediktus, not checked
namespace BossMod.MaskedCarnivale.Stage12.Act2
{
    public enum OID : uint
    {
        Boss = 0x271B, //R=6.96
        Roselet = 0x271C, //R=0.8
    };

    public enum AID : uint
    {
        WildHorn = 14751, // 271B->self, 3,5s cast, range 10+R 120-degree cone
        SporeSac = 14752, // 271B->self, 3,0s cast, range 50 circle
        Seedvolley = 14750, // 271C->player, no cast, single-target
        Trounce = 14754, // 271B->self, 4,5s cast, range 40+R 60-degree cone
        InflammableFumes = 14753, // 271B->self, 15,0s cast, range 50 circle
    };

    class WildHorn : SelfTargetedAOEs
    {
        public WildHorn() : base(ActionID.MakeSpell(AID.WildHorn), new AOEShapeCone(16.96f, 60.Degrees())) { }
    }

    class Trounce : SelfTargetedAOEs
    {
        public Trounce() : base(ActionID.MakeSpell(AID.Trounce), new AOEShapeCone(46.96f, 30.Degrees())) { }
    }

    class SporeSac : CastHint
    {
        public SporeSac() : base(ActionID.MakeSpell(AID.SporeSac), "Calls Roselets. Prepare Ice Spikes if available.") { }
    }

    class InflammableFumes : CastHint
    {
        public InflammableFumes() : base(ActionID.MakeSpell(AID.InflammableFumes), "Stun him with Bomb Toss. High damage but suriveable.") { }
    }

    class Hints : BossComponent
    {
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            hints.Add("Use Bomb Toss to stun Hydnora when he casts Inflammable Fumes.\nUse Ice Spikes to instantly kill roselets once they become aggressive.\nHydnora is weak against water and strong against earth spells.");
        }
    }

    class Stage12Act2States : StateMachineBuilder
    {
        public Stage12Act2States(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<WildHorn>()
                .ActivateOnEnter<Trounce>()
                .ActivateOnEnter<SporeSac>()
                .ActivateOnEnter<InflammableFumes>()
                .DeactivateOnEnter<Hints>();
        }
    }

    [ModuleInfo(CFCID = 622, NameID = 8102)]
    public class Stage12Act2 : BossModule
    {
        public Stage12Act2(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 25))
        {
            ActivateComponent<Hints>();
        }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            foreach (var s in Enemies(OID.Boss))
                Arena.Actor(s, ArenaColor.Enemy, false);
            foreach (var s in Enemies(OID.Roselet))
                Arena.Actor(s, ArenaColor.Object, false);
        }

        public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.CalculateAIHints(slot, actor, assignment, hints);
            foreach (var e in hints.PotentialTargets)
            {
                e.Priority = (OID)e.Actor.OID switch
                {
                    OID.Roselet => 1, //TODO: ideally AI would use Ice Spikes when these spawn instead of attacking them directly
                    OID.Boss => 0,
                    _ => 0
                };
            }
        }
    }
}
