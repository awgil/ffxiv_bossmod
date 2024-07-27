namespace BossMod.Global.MaskedCarnivale.Stage04.Act2;

public enum OID : uint
{
    Boss = 0x25D5, //R=2.5
    Beetle = 0x25D6, //R=0.6
}

public enum AID : uint
{
    AutoAttack = 6497, // 25D5->player, no cast, single-target
    GrandStrike = 14366, // 25D5->self, 1.5s cast, range 75+R width 2 rect
    Attack2 = 6499, // 25D6->player, no cast, single-target
    MagitekField = 14369, // 25D5->self, 5.0s cast, single-target
    Spoil = 14362, // 25D6->self, no cast, range 6+R circle
    MagitekRay = 14368, // 25D5->location, 3.0s cast, range 6 circle
}

class GrandStrike(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.GrandStrike), new AOEShapeRect(77.5f, 2));

class MagitekRay(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.MagitekRay), 6);

class MagitekField(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.MagitekField), "Interruptible, increases its defenses");

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("Kreios is weak to lightning spells.\nDuring the fight he will spawn 6 beetles.\nIf available use the Ram's Voice + Ultravibration combo for the instant kill.");
    }
}

class Hints2(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("Kreios is weak against lightning spells and can be frozen.");
    }
}

class Stage04Act2States : StateMachineBuilder
{
    public Stage04Act2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MagitekField>()
            .ActivateOnEnter<MagitekRay>()
            .ActivateOnEnter<GrandStrike>()
            .ActivateOnEnter<Hints2>()
            .DeactivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 614, NameID = 8087, SortOrder = 2)]
public class Stage04Act2 : BossModule
{
    public Stage04Act2(WorldState ws, Actor primary) : base(ws, primary, new(100, 100), new ArenaBoundsCircle(25))
    {
        ActivateComponent<Hints>();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.Beetle))
            Arena.Actor(s, ArenaColor.Object);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.Beetle => 1,
                OID.Boss => 0,
                _ => 0
            };
        }
    }
}
