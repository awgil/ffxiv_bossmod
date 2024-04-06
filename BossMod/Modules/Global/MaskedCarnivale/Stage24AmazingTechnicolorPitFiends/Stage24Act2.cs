namespace BossMod.Global.MaskedCarnivale.Stage24.Act2;

public enum OID : uint
{
    Boss = 0x2736, //R=2.0
    ArenaViking = 0x2737, //R=1.0
    ArenaMagus = 0x2738, //R=1.0
};

public enum AID : uint
{
    AutoAttack = 6499, // 2736->player, no cast, single-target
    AutoAttack2 = 6497, // 2737->player, no cast, single-target
    CondensedLibra = 15319, // 2736->player, 5,0s cast, single-target
    TripleHit = 15320, // 2736->players, 3,0s cast, single-target
    Mechanogravity = 15322, // 2736->location, 3,0s cast, range 6 circle
    Fire = 14266, // 2738->player, 1,0s cast, single-target
    Starstorm = 15317, // 2738->location, 3,0s cast, range 5 circle
    RagingAxe = 15316, // 2737->self, 3,0s cast, range 4+R 90-degree cone
    Silence = 15321, // 2736->player, 5,0s cast, single-target
};

class Starstorm : Components.LocationTargetedAOEs
{
    public Starstorm() : base(ActionID.MakeSpell(AID.Starstorm), 5) { }
}

class Mechanogravity : Components.LocationTargetedAOEs
{
    public Mechanogravity() : base(ActionID.MakeSpell(AID.Mechanogravity), 6) { }
}

class RagingAxe : Components.SelfTargetedAOEs
{
    public RagingAxe() : base(ActionID.MakeSpell(AID.RagingAxe), new AOEShapeCone(5, 45.Degrees())) { }
}

class CondensedLibra : Components.SingleTargetCast
{
    public CondensedLibra() : base(ActionID.MakeSpell(AID.CondensedLibra), "Use Diamondback!") { }
}
class TripleHit : Components.SingleTargetCast
{
    public TripleHit() : base(ActionID.MakeSpell(AID.TripleHit), "Use Diamondback!") { }
}

class Silence : Components.CastHint
{
    public Silence() : base(ActionID.MakeSpell(AID.Silence), "Interrupt") { }
}

class Hints2 : BossComponent
{
    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        if (!module.Enemies(OID.ArenaMagus).All(e => e.IsDead))
            hints.Add($"{module.Enemies(OID.ArenaMagus).FirstOrDefault()!.Name} is immune to magical damage!");
        if (!module.Enemies(OID.ArenaViking).All(e => e.IsDead))
            hints.Add($"{module.Enemies(OID.ArenaViking).FirstOrDefault()!.Name} is immune to physical damage!");
    }
}

class Hints : BossComponent
{
    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        hints.Add($"The {module.PrimaryActor.Name} casts Silence which should be interrupted.\nCondensed Libra puts a debuff on you. Use Diamondback to survive the\nfollowing attack. Alternatively you can cleanse the debuff with Exuviation.");
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
    public Stage24Act2(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 25))
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

    public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.CalculateAIHints(slot, actor, assignment, hints);
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
