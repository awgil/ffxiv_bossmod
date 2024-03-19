// CONTRIB: made by malediktus, not checked
using System.Linq;

namespace BossMod.MaskedCarnivale.Stage25.Act2
{
    public enum OID : uint
    {
        Boss = 0x267F, //R=1.2
        BlazingAngon = 0x2682, //R=0.6
        Helper = 0x233C,
    };

    public enum AID : uint
    {
        RepellingSpray = 14768, // Boss->self, 2,0s cast, single-target, boss reflectss magic attacks
        ApocalypticBolt = 14766, // 267F->self, 3,0s cast, range 50+R width 8 rect
        BlazingAngon = 14769, // 267F->location, 1,0s cast, single-target
        Burn = 14776, // 2682->self, 6,0s cast, range 50+R circle
        TheRamsVoice = 14763, // 267F->self, 3,5s cast, range 8 circle
        TheDragonsVoice = 14764, // 267F->self, 3,5s cast, range 6-30 donut
        ApocalypticRoar = 14767, // 267F->self, 5,0s cast, range 35+R 120-degree cone
    };

    public enum SID : uint
    {
        RepellingSpray = 556, // Boss->Boss, extra=0x64
        Doom = 910, // Boss->player, extra=0x0
    };

    class ApocalypticBolt : Components.SelfTargetedAOEs
    {
        public ApocalypticBolt() : base(ActionID.MakeSpell(AID.ApocalypticBolt), new AOEShapeRect(51.2f, 4)) { }
    }

    class ApocalypticRoar : Components.SelfTargetedAOEs
    {
        public ApocalypticRoar() : base(ActionID.MakeSpell(AID.ApocalypticRoar), new AOEShapeCone(36.2f, 60.Degrees())) { }
    }

    class TheRamsVoice : Components.SelfTargetedAOEs
    {
        public TheRamsVoice() : base(ActionID.MakeSpell(AID.TheRamsVoice), new AOEShapeCircle(8)) { }
    }

    class TheDragonsVoice : Components.SelfTargetedAOEs
    {
        public TheDragonsVoice() : base(ActionID.MakeSpell(AID.TheDragonsVoice), new AOEShapeDonut(6, 30)) { }
    }

    class Hints : BossComponent
    {
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            hints.Add($"In this act {module.PrimaryActor.Name} will reflect all magic attacks.\nHe will also spawn adds that need to be dealed with swiftly\nsince they will spam raidwides. The adds are immune against magic\nand fire attacks.");
        }
    }

    class Hints2 : BossComponent
    {
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (!module.Enemies(OID.BlazingAngon).All(e => e.IsDead))
                hints.Add($"Kill {module.Enemies(OID.BlazingAngon).FirstOrDefault()!.Name}! Use physical attacks except fire aspected.");
            var magicreflect = module.Enemies(OID.Boss).Where(x => x.FindStatus(SID.RepellingSpray) != null).FirstOrDefault();
            if (magicreflect != null)
                hints.Add($"{module.PrimaryActor.Name} will reflect all magic damage!");
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            var doomed = actor.FindStatus(SID.Doom);
            if (doomed != null)
                hints.Add("You were doomed! Cleanse it with Exuviation or finish the act fast.");
        }
    }

    class Stage25Act2States : StateMachineBuilder
    {
        public Stage25Act2States(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<ApocalypticBolt>()
                .ActivateOnEnter<ApocalypticRoar>()
                .ActivateOnEnter<TheRamsVoice>()
                .ActivateOnEnter<TheDragonsVoice>()
                .ActivateOnEnter<Hints2>()
                .DeactivateOnEnter<Hints>();
        }
    }

    [ModuleInfo(CFCID = 635, NameID = 8129)]
    public class Stage25Act2 : BossModule
    {
        public Stage25Act2(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 25))
        {
            ActivateComponent<Hints>();
        }

        public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.CalculateAIHints(slot, actor, assignment, hints);
            foreach (var e in hints.PotentialTargets)
            {
                e.Priority = (OID)e.Actor.OID switch
                {
                    OID.BlazingAngon => 1, //TODO: ideally Magus should only be attacked with ranged physical abilities
                    OID.Boss => 0, //TODO: ideally Azulmagia should only be attacked with physical abilities in this act
                    _ => 0
                };
            }
        }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
            foreach (var s in Enemies(OID.BlazingAngon))
                Arena.Actor(s, ArenaColor.Object);
        }

    }
}
