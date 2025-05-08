namespace BossMod.Global.MaskedCarnivale.Stage24.Act1;

public enum OID : uint
{
    Boss = 0x2735, //R=1.0
    ArenaViking = 0x2734, //R=1.0
}

public enum AID : uint
{
    AutoAttack = 6497, // ArenaViking->player, no cast, single-target
    Fire = 14266, // Boss->player, 1.0s cast, single-target
    Starstorm = 15317, // Boss->location, 3.0s cast, range 5 circle
    RagingAxe = 15316, // ArenaViking->self, 3.0s cast, range 4+R 90-degree cone
    LightningSpark = 15318, // Boss->player, 6.0s cast, single-target
}

class Starstorm(BossModule module) : Components.StandardAOEs(module, AID.Starstorm, 5);
class RagingAxe(BossModule module) : Components.StandardAOEs(module, AID.RagingAxe, new AOEShapeCone(5, 45.Degrees()));
class LightningSpark(BossModule module) : Components.CastHint(module, AID.LightningSpark, "Interrupt");

class Hints2(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (!Module.PrimaryActor.IsDead)
            hints.Add($"{Module.PrimaryActor.Name} is immune to magical damage!");
        if (!Module.Enemies(OID.ArenaViking).All(e => e.IsDead))
            hints.Add($"{Module.Enemies(OID.ArenaViking).FirstOrDefault()!.Name} is immune to physical damage!");
    }
}

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"The {Module.PrimaryActor.Name} is immune to magic, the {Module.Enemies(OID.ArenaViking).FirstOrDefault()!.Name} is immune to\nphysical attacks. For the 2nd act Diamondback is highly recommended.\nFor the 3rd act a ranged physical spell such as Fire Angon\nis highly recommended.");
    }
}

class Stage24Act1States : StateMachineBuilder
{
    public Stage24Act1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .DeactivateOnEnter<Hints>()
            .ActivateOnEnter<Starstorm>()
            .ActivateOnEnter<RagingAxe>()
            .ActivateOnEnter<LightningSpark>()
            .ActivateOnEnter<Hints2>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.ArenaViking).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 634, NameID = 8127, SortOrder = 1)]
public class Stage24Act1 : BossModule
{
    public Stage24Act1(WorldState ws, Actor primary) : base(ws, primary, new(100, 100), new ArenaBoundsCircle(25))
    {
        ActivateComponent<Hints>();
    }

    protected override bool CheckPull() { return PrimaryActor.IsTargetable && PrimaryActor.InCombat || Enemies(OID.ArenaViking).Any(e => e.InCombat); }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.ArenaViking))
            Arena.Actor(s, ArenaColor.Enemy);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.Boss or OID.ArenaViking => 0, //TODO: ideally Viking should only be attacked with magical abilities and Magus should only be attacked with physical abilities
                _ => 0
            };
        }
    }
}
