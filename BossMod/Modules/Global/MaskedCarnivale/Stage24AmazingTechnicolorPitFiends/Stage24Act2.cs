namespace BossMod.Global.MaskedCarnivale.Stage24.Act2;

public enum OID : uint
{
    Boss = 0x2736, //R=2.0
    ArenaViking = 0x2737, //R=1.0
    ArenaMagus = 0x2738, //R=1.0
}

public enum AID : uint
{
    AutoAttack = 6499, // 2736->player, no cast, single-target
    AutoAttack2 = 6497, // 2737->player, no cast, single-target
    CondensedLibra = 15319, // 2736->player, 5.0s cast, single-target
    TripleHit = 15320, // 2736->players, 3.0s cast, single-target
    Mechanogravity = 15322, // 2736->location, 3.0s cast, range 6 circle
    Fire = 14266, // 2738->player, 1.0s cast, single-target
    Starstorm = 15317, // 2738->location, 3.0s cast, range 5 circle
    RagingAxe = 15316, // 2737->self, 3.0s cast, range 4+R 90-degree cone
    Silence = 15321, // 2736->player, 5.0s cast, single-target
}

class Starstorm(BossModule module) : Components.StandardAOEs(module, AID.Starstorm, 5);
class Mechanogravity(BossModule module) : Components.StandardAOEs(module, AID.Mechanogravity, 6);
class RagingAxe(BossModule module) : Components.StandardAOEs(module, AID.RagingAxe, new AOEShapeCone(5, 45.Degrees()));
class CondensedLibra(BossModule module) : Components.SingleTargetCast(module, AID.CondensedLibra, "Use Diamondback!");
class TripleHit(BossModule module) : Components.SingleTargetCast(module, AID.TripleHit, "Use Diamondback!");
class Silence(BossModule module) : Components.CastHint(module, AID.Silence, "Interrupt");

class Hints2(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (!Module.Enemies(OID.ArenaMagus).All(e => e.IsDead))
            hints.Add($"{Module.Enemies(OID.ArenaMagus).FirstOrDefault()!.Name} is immune to magical damage!");
        if (!Module.Enemies(OID.ArenaViking).All(e => e.IsDead))
            hints.Add($"{Module.Enemies(OID.ArenaViking).FirstOrDefault()!.Name} is immune to physical damage!");
    }
}

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"The {Module.PrimaryActor.Name} casts Silence which should be interrupted.\nCondensed Libra puts a debuff on you. Use Diamondback to survive the\nfollowing attack. Alternatively you can cleanse the debuff with Exuviation.");
    }
}

class Stage24Act2States : StateMachineBuilder
{
    public Stage24Act2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .DeactivateOnEnter<Hints>()
            .ActivateOnEnter<Starstorm>()
            .ActivateOnEnter<RagingAxe>()
            .ActivateOnEnter<Silence>()
            .ActivateOnEnter<Mechanogravity>()
            .ActivateOnEnter<CondensedLibra>()
            .ActivateOnEnter<TripleHit>()
            .ActivateOnEnter<Hints2>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 634, NameID = 8128, SortOrder = 2)]
public class Stage24Act2 : BossModule
{
    public Stage24Act2(WorldState ws, Actor primary) : base(ws, primary, new(100, 100), new ArenaBoundsCircle(25))
    {
        ActivateComponent<Hints>();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.ArenaViking))
            Arena.Actor(s, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.ArenaMagus))
            Arena.Actor(s, ArenaColor.Enemy);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.ArenaMagus or OID.ArenaViking => 1, //TODO: ideally Viking should only be attacked with magical abilities and Magus should only be attacked with physical abilities
                OID.Boss => 0,
                _ => 0
            };
        }
    }
}
