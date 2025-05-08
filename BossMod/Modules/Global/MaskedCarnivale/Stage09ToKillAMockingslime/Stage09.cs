namespace BossMod.Global.MaskedCarnivale.Stage09;

public enum OID : uint
{
    Boss = 0x242D, //R=2.4
    Pudding = 0x2711, //R=1.8
    Gelato = 0x2712, //R=1.8
    Marshmallow = 0x2713, //R=1.8
    Licorice = 0x2714, //R=1.8
    Bavarois = 0x2715, //R=1.8
    Flan = 0x2716, //R=1.8
    DarkVoidzone = 0x1E9C9D, //R=0.5
}

public enum AID : uint
{
    DeathRay = 15056, // 242D->player, 1.0s cast, single-target
    Dark = 15057, // 242D->location, 3.0s cast, range 5 circle, creates a voidzone with radius 4
    GoldenTongue = 14265, // 242D/2713/2714/2715->self, 5.0s cast, single-target
    Fire = 14266, // 2711->player, 1.0s cast, single-target
    Blizzard = 14267, // 2712->player, 1.0s cast, single-target
    Aero = 14269, // 2713->player, 1.0s cast, single-target
    Stone = 14270, // 2714->player, 1.0s cast, single-target
    Thunder = 14268, // 2715->player, 1.0s cast, single-target
    Water = 14271, // 2716->player, 1.0s cast, single-target
}

class GoldenTongue(BossModule module) : Components.CastHint(module, AID.GoldenTongue, "Can be interrupted, increase its magic damage");

class DarkVoidzone(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 4, AID.Dark, m => m.Enemies(OID.DarkVoidzone), 0);

class Dark(BossModule module) : Components.StandardAOEs(module, AID.Dark, 5);

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("Guimauve summons a total of 6 adds during the fight, one of each element.\nHealer mimikry can be helpful if you have trouble surviving.");
    }
}

class Stage09States : StateMachineBuilder
{
    public Stage09States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Dark>()
            .ActivateOnEnter<DarkVoidzone>()
            .ActivateOnEnter<GoldenTongue>()
            .DeactivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 619, NameID = 8099)]
public class Stage09 : BossModule
{
    public Stage09(WorldState ws, Actor primary) : base(ws, primary, new(100, 100), new ArenaBoundsCircle(16))
    {
        ActivateComponent<Hints>();
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.Licorice or OID.Flan or OID.Marshmallow or OID.Pudding or OID.Bavarois or OID.Gelato => 1,
                OID.Boss => 0,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.Licorice))
            Arena.Actor(s, ArenaColor.Object);
        foreach (var s in Enemies(OID.Flan))
            Arena.Actor(s, ArenaColor.Object);
        foreach (var s in Enemies(OID.Pudding))
            Arena.Actor(s, ArenaColor.Object);
        foreach (var s in Enemies(OID.Marshmallow))
            Arena.Actor(s, ArenaColor.Object);
        foreach (var s in Enemies(OID.Bavarois))
            Arena.Actor(s, ArenaColor.Object);
        foreach (var s in Enemies(OID.Gelato))
            Arena.Actor(s, ArenaColor.Object);
    }
}
