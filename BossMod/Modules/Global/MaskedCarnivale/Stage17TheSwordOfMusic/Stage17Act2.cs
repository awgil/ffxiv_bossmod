namespace BossMod.Global.MaskedCarnivale.Stage17.Act2;

public enum OID : uint
{
    Boss = 0x2721, // R=2.5
    LeftClaw = 0x2722, //R=2.0
    RightClaw = 0x2723, //R=2.0
    MagitekRayVoidzone = 0x1E8D9B, //R=0.5
}

public enum AID : uint
{
    AutoAttack = 6497, // 2721->player, no cast, single-target
    GrandStrike = 15047, // 2721->self, 1.5s cast, range 75+R width 2 rect
    MagitekField = 15049, // 2721->self, 5.0s cast, single-target, buffs defenses, interruptible
    AutoAttack2 = 6499, // 2723/2722->player, no cast, single-target
    TheHand = 14760, // 2722/2723->self, 3.0s cast, range 6+R 120-degree cone
    Shred = 14759, // 2723/2722->self, 2.5s cast, range 4+R width 4 rect
    MagitekRay = 15048, // 2721->location, 3.0s cast, range 6 circle, voidzone, interruptible
}

class GrandStrike(BossModule module) : Components.StandardAOEs(module, AID.GrandStrike, new AOEShapeRect(77.5f, 2));
class MagitekField(BossModule module) : Components.CastHint(module, AID.MagitekField, "Interruptible, increases its defenses");
class MagitekRay(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 6, AID.MagitekRay, m => m.Enemies(OID.MagitekRayVoidzone), 0);
class TheHand(BossModule module) : Components.StandardAOEs(module, AID.TheHand, new AOEShapeCone(8, 60.Degrees()));
class Shred(BossModule module) : Components.StandardAOEs(module, AID.Shred, new AOEShapeRect(6, 2));
class TheHandKB(BossModule module) : Components.KnockbackFromCastTarget(module, AID.TheHand, 10, shape: new AOEShapeCone(8, 60.Degrees())); // actual knockback happens a whole 0.9s after snapshot

class Hints2(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (!Module.Enemies(OID.LeftClaw).All(e => e.IsDead))
            hints.Add($"{Module.Enemies(OID.LeftClaw).FirstOrDefault()!.Name} counters magical damage!");
        if (!Module.Enemies(OID.RightClaw).All(e => e.IsDead))
            hints.Add($"{Module.Enemies(OID.RightClaw).FirstOrDefault()!.Name} counters physical damage!");
    }
}

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"{Module.PrimaryActor.Name} is weak to lightning spells.\nDuring the fight he will spawn one of each claws as known from act 1.\nIf available use the Ram's Voice + Ultravibration combo for instant kill.");
    }
}

class Stage17Act2States : StateMachineBuilder
{
    public Stage17Act2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MagitekField>()
            .ActivateOnEnter<MagitekRay>()
            .ActivateOnEnter<TheHand>()
            .ActivateOnEnter<TheHandKB>()
            .ActivateOnEnter<GrandStrike>()
            .ActivateOnEnter<Shred>()
            .ActivateOnEnter<Hints2>()
            .DeactivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 627, NameID = 8087, SortOrder = 2)]
public class Stage17Act2 : BossModule
{
    public Stage17Act2(WorldState ws, Actor primary) : base(ws, primary, new(100, 100), new ArenaBoundsCircle(16))
    {
        ActivateComponent<Hints>();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.LeftClaw))
            Arena.Actor(s, ArenaColor.Object);
        foreach (var s in Enemies(OID.RightClaw))
            Arena.Actor(s, ArenaColor.Object);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.LeftClaw or OID.RightClaw => 1, //TODO: ideally left claw should only be attacked with magical abilities and right claw should only be attacked with physical abilities
                OID.Boss => 0,
                _ => 0
            };
        }
    }
}
