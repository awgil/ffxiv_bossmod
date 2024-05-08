namespace BossMod.Global.MaskedCarnivale.Stage18.Act2;

public enum OID : uint
{
    Boss = 0x2725, //R=3.0
    Keg = 0x2726, //R=0.65
}

public enum AID : uint
{
    WildCharge = 15055, // 2725->players, 3.5s cast, width 8 rect charge
    Explosion = 15054, // 2726->self, 2.0s cast, range 10 circle
    Fireball = 15051, // 2725->location, 4.0s cast, range 6 circle
    RipperClaw = 15050, // 2725->self, 4.0s cast, range 5+R 90-degree cone
    TailSmash = 15052, // 2725->self, 4.0s cast, range 12+R 90-degree cone
    BoneShaker = 15053, // 2725->self, no cast, range 50 circle, harmless raidwide
}

class Explosion(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Explosion), new AOEShapeCircle(10));
class Fireball(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Fireball), 6);
class RipperClaw(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RipperClaw), new AOEShapeCone(8, 45.Degrees()));
class TailSmash(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TailSmash), new AOEShapeCone(15, 45.Degrees()));

class WildCharge(BossModule module) : Components.BaitAwayChargeCast(module, ActionID.MakeSpell(AID.WildCharge), 4)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (CurrentBaits.Count > 0 && !Module.Enemies(OID.Keg).All(e => e.IsDead))
            hints.Add("Aim charge at a keg!");
    }
}

// knockback actually delayed by 0.5s to 1s, maybe it depends on the rectangle length of the charge
class WildChargeKB(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.WildCharge), 10, kind: Kind.DirForward, stopAtWall: true);

class KegExplosion(BossModule module) : Components.GenericStackSpread(module)
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var p in Module.Enemies(OID.Keg).Where(x => !x.IsDead))
        {
            if (Arena.Config.ShowOutlinesAndShadows)
                Arena.AddCircle(p.Position, 10, 0xFF000000, 2);
            Arena.AddCircle(p.Position, 10, ArenaColor.Danger);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        foreach (var p in Module.Enemies(OID.Keg).Where(x => !x.IsDead))
            if (actor.Position.InCircle(p.Position, 10))
                hints.Add("In keg explosion radius!");
    }
}

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("Same as last stage. Make the manticores run to the kegs and their attacks\nwill make them blow up. Their attacks will also do friendly fire damage\nto each other.\nThe Ram's Voice and Ultravibration combo can be used to kill manticores.");
    }
}

class Stage18Act2States : StateMachineBuilder
{
    public Stage18Act2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .DeactivateOnEnter<Hints>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<Fireball>()
            .ActivateOnEnter<RipperClaw>()
            .ActivateOnEnter<TailSmash>()
            .ActivateOnEnter<WildCharge>()
            .ActivateOnEnter<WildChargeKB>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.Keg).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 628, NameID = 8116, SortOrder = 2)]
public class Stage18Act2 : BossModule
{
    public Stage18Act2(WorldState ws, Actor primary) : base(ws, primary, new(100, 100), new ArenaBoundsCircle(25))
    {
        ActivateComponent<Hints>();
        ActivateComponent<KegExplosion>();
    }

    protected override bool CheckPull() { return PrimaryActor.IsTargetable && PrimaryActor.InCombat || Enemies(OID.Keg).Any(e => e.InCombat); }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        foreach (var s in Enemies(OID.Boss))
            Arena.Actor(s, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.Keg))
            Arena.Actor(s, ArenaColor.Object);
    }

    public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.CalculateAIHints(slot, actor, assignment, hints);
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.Boss => 1,
                OID.Keg => 0,
                _ => 0
            };
        }
    }
}
