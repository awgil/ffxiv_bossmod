namespace BossMod.RealmReborn.Trial.T04PortaDecumana.Phase2;

public enum OID : uint
{
    Boss = 0x3900, // R=6.0
    Helper = 0x233C,
    Aetheroplasm = 0x3902, // R=1.0
    MagitekBit = 0x3901, // R=0.6
}

public enum AID : uint
{
    AutoAttack = 29004, // Boss->player, no cast, single-target
    Teleport = 28628, // Boss->location, no cast, single-target
    TankPurge = 29022, // Boss->self, 5.0s cast, raidwide
    HomingLasers = 29023, // Boss->player, 5.0s cast, single-target, tankbuster

    MagitekRayForward = 29005, // Boss->self, no cast, single-target, visual
    MagitekRayRight = 29006, // Boss->self, no cast, single-target, visual
    MagitekRayLeft = 29007, // Boss->self, no cast, single-target, visual
    MagitekRayAOEForward = 29008, // Helper->self, 2.2s cast, range 40 width 6 rect aoe
    MagitekRayAOERight = 29009, // Helper->self, 2.2s cast, range 40 width 6 rect aoe
    MagitekRayAOELeft = 29010, // Helper->self, 2.2s cast, range 40 width 6 rect aoe

    HomingRay = 29011, // Boss->self, 4.0s cast, single-target, visual
    HomingRayAOE = 29012, // Helper->player, 5.0s cast, range 6 circle spread
    LaserFocus = 29013, // Boss->self, 4.0s cast, single-target, visual
    LaserFocusAOE = 29014, // Helper->player, 5.0s cast, range 6 circle stack

    AethericBoom = 29015, // Boss->self, 4.0s cast, knockback 30
    AetheroplasmSoak = 29016, // Aetheroplasm->self, no cast, range 8 circle aoe
    AetheroplasmCollide = 29017, // Aetheroplasm->self, no cast, raidwide

    BitTeleport = 29018, // MagitekBit->location, no cast, single-target
    AssaultCannon = 29019, // MagitekBit->self, 4.0s cast, range 40 width 4 rect

    CitadelBuster = 29020, // Boss->self, 5.0s cast, range 40 width 12 rect aoe
    Explosion = 29021, // Helper->self, 7.0s cast, raidwide with ? falloff

    LimitBreakRefill = 28542, // Helper->self, no cast, range 40 circle - probably limit break refill
    Ultima = 29024, // Boss->self, 71.0s cast, enrage
}

class TankPurge(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.TankPurge));
class HomingLasers(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.HomingLasers));
class MagitekRayF(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MagitekRayAOEForward), new AOEShapeRect(40, 3));
class MagitekRayR(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MagitekRayAOERight), new AOEShapeRect(40, 3));
class MagitekRayL(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MagitekRayAOELeft), new AOEShapeRect(40, 3));
class HomingRay(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.HomingRayAOE), 6);
class LaserFocus(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.LaserFocusAOE), 6)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // requested by LTS, not sure if this is really needed (it is supposed to improve DPS uptime?)
        if (Stacks.Count > 0)
        {
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Module.Center, 1.5f), Stacks[0].Activation);
        }
    }
}

class AethericBoom(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.AethericBoom), 30, stopAtWall: true)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Casters.Count > 0)
            hints.Add("Prepare to soak the orbs!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count > 0)
        {
            hints.PlannedActions.Add((ActionID.MakeSpell(WAR.AID.ArmsLength), actor, 1, false));
            hints.PlannedActions.Add((ActionID.MakeSpell(WHM.AID.Surecast), actor, 1, false));
        }
    }
}

class Aetheroplasm(BossModule module) : BossComponent(module)
{
    private readonly IReadOnlyList<Actor> _orbs = module.Enemies(OID.Aetheroplasm);

    public IEnumerable<Actor> ActiveOrbs => _orbs.Where(orb => !orb.IsDead);

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (ActiveOrbs.Any())
            hints.Add("Soak the orbs!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // orbs spawn with rotation 0°, checking for a different angle makes sure the AI doesn't run into the wall trying to catch them
        // since orbs are outside of the arena until they start rotating
        var orb = ActiveOrbs.FirstOrDefault(x => !x.Rotation.AlmostEqual(0.Degrees(), Helpers.RadianConversion));
        var orbs = new List<Func<WPos, float>>();
        if (orb != null)
        {
            hints.PlannedActions.Add((ActionID.MakeSpell(WAR.AID.Sprint), actor, 1, false));
            foreach (var o in ActiveOrbs)
                orbs.Add(ShapeDistance.InvertedCircle(o.Position + 0.5f * o.Rotation.ToDirection(), 0.5f));
        }
        if (orbs.Count > 0)
            hints.AddForbiddenZone(p => orbs.Select(f => f(p)).Max());
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var orb in ActiveOrbs)
            Arena.AddCircle(orb.Position, 1.4f, ArenaColor.Safe);
    }
}

class AssaultCannon(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AssaultCannon), new AOEShapeRect(40, 2));
class CitadelBuster(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CitadelBuster), new AOEShapeRect(40, 6));

class Explosion(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Explosion), new AOEShapeCircle(16)) // TODO: verify falloff
{
    // there is an overlap with another mechanic which has to be resolved first
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Module.FindComponent<AssaultCannon>()!.Casters.Count == 0)
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class Ultima(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.Ultima), "Enrage!", true);

class T04PortaDecumana2States : StateMachineBuilder
{
    public T04PortaDecumana2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TankPurge>()
            .ActivateOnEnter<HomingLasers>()
            .ActivateOnEnter<MagitekRayF>()
            .ActivateOnEnter<MagitekRayR>()
            .ActivateOnEnter<MagitekRayL>()
            .ActivateOnEnter<HomingRay>()
            .ActivateOnEnter<LaserFocus>()
            .ActivateOnEnter<AethericBoom>()
            .ActivateOnEnter<Aetheroplasm>()
            .ActivateOnEnter<AssaultCannon>()
            .ActivateOnEnter<CitadelBuster>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<Ultima>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "veyn, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 830, NameID = 2137, SortOrder = 2)]
public class T04PortaDecumana2(WorldState ws, Actor primary) : BossModule(ws, primary, new(-704, 480), new ArenaBoundsCircle(19.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy, true);
        foreach (var s in Enemies(OID.Aetheroplasm).Where(x => !x.IsDead))
            Arena.Actor(s, ArenaColor.Object, true);
    }
}
