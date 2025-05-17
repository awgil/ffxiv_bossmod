namespace BossMod.Global.MaskedCarnivale.Stage25.Act2;

public enum OID : uint
{
    Boss = 0x267F, //R=1.2
    BlazingAngon = 0x2682, //R=0.6
    Helper = 0x233C,
}

public enum AID : uint
{
    RepellingSpray = 14768, // Boss->self, 2.0s cast, single-target, boss reflectss magic attacks
    ApocalypticBolt = 14766, // 267F->self, 3.0s cast, range 50+R width 8 rect
    BlazingAngon = 14769, // 267F->location, 1.0s cast, single-target
    Burn = 14776, // 2682->self, 6.0s cast, range 50+R circle
    TheRamsVoice = 14763, // 267F->self, 3.5s cast, range 8 circle
    TheDragonsVoice = 14764, // 267F->self, 3.5s cast, range 6-30 donut
    ApocalypticRoar = 14767, // 267F->self, 5.0s cast, range 35+R 120-degree cone
}

public enum SID : uint
{
    RepellingSpray = 556, // Boss->Boss, extra=0x64
    Doom = 910, // Boss->player, extra=0x0
}

class ApocalypticBolt(BossModule module) : Components.StandardAOEs(module, AID.ApocalypticBolt, new AOEShapeRect(51.2f, 4));
class ApocalypticRoar(BossModule module) : Components.StandardAOEs(module, AID.ApocalypticRoar, new AOEShapeCone(36.2f, 60.Degrees()));
class TheRamsVoice(BossModule module) : Components.StandardAOEs(module, AID.TheRamsVoice, new AOEShapeCircle(8));
class TheDragonsVoice(BossModule module) : Components.StandardAOEs(module, AID.TheDragonsVoice, new AOEShapeDonut(6, 30));

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"In this act {Module.PrimaryActor.Name} will reflect all magic attacks.\nHe will also spawn adds that need to be dealed with swiftly\nsince they will spam raidwides. The adds are immune against magic\nand fire attacks.");
    }
}

class Hints2(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (!Module.Enemies(OID.BlazingAngon).All(e => e.IsDead))
            hints.Add($"Kill {Module.Enemies(OID.BlazingAngon).FirstOrDefault()!.Name}! Use physical attacks except fire aspected.");
        var magicreflect = Module.Enemies(OID.Boss).FirstOrDefault(x => x.FindStatus(SID.RepellingSpray) != null);
        if (magicreflect != null)
            hints.Add($"{Module.PrimaryActor.Name} will reflect all magic damage!");
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
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

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 635, NameID = 8129, SortOrder = 2)]
public class Stage25Act2 : BossModule
{
    public Stage25Act2(WorldState ws, Actor primary) : base(ws, primary, new(100, 100), new ArenaBoundsCircle(25))
    {
        ActivateComponent<Hints>();
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
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
