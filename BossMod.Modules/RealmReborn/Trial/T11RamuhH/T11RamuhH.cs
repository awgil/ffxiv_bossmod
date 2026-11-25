namespace BossMod.RealmReborn.Trial.T11RamuhH;

public enum OID : uint
{
    Boss = 0xBAB, // R4.200, x1
    Helper = 0xBAC, // R0.500, x12, Helper type
    GreyArbiter = 0xBAD, // R2.000, x3, spawn during fight
    LightningOrb = 0x1E948A, // R0.500, EventObj type, left behind by ground thunderstorms; event state 7 once picked up
}

public enum AID : uint
{
    AutoAttack = 2446, // Boss->player, no cast, range 5 circle
    ShockStrike = 2274, // Boss->player, no cast, range 5 circle around the target, replaces an autoattack every ~20s
    Thunderspark = 2275, // Boss->self, 3.0s cast, range 3+R circle, frees terrified players
    ThunderstormVisual = 2276, // Boss->self, 4.0s cast, single-target
    ThunderstormGround = 2277, // Helper->location, 4.0s cast, range 5 circle, leaves a lightning orb, frees terrified players
    ThunderstormPlayer = 2278, // Helper->player, 4.0s cast, range 5 circle, frees terrified players
    JudgmentBolt = 2279, // Boss->self, 3.0s cast, range 50+R circle, wipes if any arbiter is alive
    ChaoticStrike = 2280, // Boss->self, 3.0s cast, single-target, terror on the icon 1 target
    RollingThunder = 2281, // Boss->self, 2.0s cast, single-target, causality tether between two players
    Thunderbolt = 2282, // GreyArbiter->self, 3.0s cast, range 48+R width 4 rect
    IncreasedSentence = 2283, // GreyArbiter->Boss, 3.0s cast, single-target
    Unknown = 2614, // Helper->Boss, no cast, single-target
}

public enum SID : uint
{
    Terror = 66, // Boss->player, extra=0x0, can't move or act; removed by any thunderstorm or thunderspark hit
    Electroconductivity = 512, // none->player, extra=stacks, one per orb, 9.9s
    Causality = 514, // none->player, extra=0x0, damage on every action; removed by surge protection
    SurgeProtection = 517, // none->player, extra=0x0, 3 electroconductivity stacks
}

public enum IconID : uint
{
    ChaoticStrike = 1, // player
    Thunderstorm = 21, // player
}

public enum TetherID : uint
{
    RollingThunder = 21, // player->player
}

class ShockStrike(BossModule module) : Components.Cleave(module, AID.ShockStrike, new AOEShapeCircle(5), originAtTarget: true);
class Thunderspark(BossModule module) : Components.StandardAOEs(module, AID.Thunderspark, 7.2f);
class ThunderstormGround(BossModule module) : Components.StandardAOEs(module, AID.ThunderstormGround, 5);
class JudgmentBolt(BossModule module) : Components.RaidwideCast(module, AID.JudgmentBolt, "Raidwide, wipe if an arbiter is alive");
class Thunderbolt(BossModule module) : Components.StandardAOEs(module, AID.Thunderbolt, new AOEShapeRect(50, 2));
class GreyArbiter(BossModule module) : Components.Adds(module, (uint)OID.GreyArbiter, 1);

class ThunderstormPlayer(BossModule module) : Components.SpreadFromCastTargets(module, AID.ThunderstormPlayer, 5)
{
    private const float RescueRadius = 3;
    private readonly IReadOnlyList<Actor> _groundCasters = module.Enemies(OID.Helper);

    private bool Freed(Actor terrified, Actor rescuer)
    {
        if (Spreads.Any(s => s.Target == terrified || s.Target != rescuer && s.Target.Position.InCircle(terrified.Position, s.Radius)))
            return true;
        if (_groundCasters.Any(c => c.CastInfo != null && (AID)c.CastInfo.Action.ID == AID.ThunderstormGround && terrified.Position.InCircle(c.CastInfo.LocXZ, 5)))
            return true;
        return Module.PrimaryActor.CastInfo != null && (AID)Module.PrimaryActor.CastInfo.Action.ID == AID.Thunderspark && terrified.Position.InCircle(Module.PrimaryActor.Position, 7.2f);
    }

    private Actor? Rescue(Actor actor)
    {
        if (!IsSpreadTarget(actor) || actor.FindStatus(SID.Terror) != null)
            return null;
        var terrified = Raid.WithoutSlot().Where(p => p != actor && p.FindStatus(SID.Terror) != null).ToList();
        return terrified.FirstOrDefault(p => actor.Position.InCircle(p.Position, RescueRadius))
            ?? terrified.Where(p => !Freed(p, actor)).Closest(actor.Position);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Rescue(actor) is var terrified && terrified != null)
            hints.Add($"Drop thunderstorm on {terrified.Name}!", !actor.Position.InCircle(terrified.Position, RescueRadius));
        else
            base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var terrified = Rescue(actor);
        if (terrified == null)
        {
            base.AddAIHints(slot, actor, assignment, hints);
            return;
        }

        var spread = Spreads.First(s => s.Target == actor);
        foreach (var other in Spreads.Where(s => s.Target != actor))
            hints.AddForbiddenZone(ShapeDistance.Circle(other.Target.Position, other.Radius), other.Activation);
        hints.AddForbiddenZone(ShapeDistance.InvertedCircle(terrified.Position, RescueRadius), spread.Activation);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (Rescue(pc) is var terrified && terrified != null)
            Arena.AddCircle(terrified.Position, RescueRadius, ArenaColor.Safe);
    }
}

class RollingThunder(BossModule module) : BossComponent(module)
{
    private readonly IReadOnlyList<Actor> _orbs = module.Enemies(OID.LightningOrb);

    private IEnumerable<Actor> ActiveOrbs => _orbs.Where(o => o.EventState != 7);

    private static bool Tethered(Actor actor) => actor.FindStatus(SID.Causality) != null;

    private static bool Collects(Actor actor) => Tethered(actor) && actor.FindStatus(SID.Terror) == null;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!Tethered(actor))
            return;
        hints.Add("Don't use actions!", actor.CastInfo != null);
        if (Collects(actor))
            hints.Add(ActiveOrbs.Any() ? "Pick up 3 orbs!" : "No orbs left!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!Tethered(actor))
            return;

        hints.AddSpecialMode(AIHints.SpecialMode.NoActions, WorldState.CurrentTime);

        if (Collects(actor) && ActiveOrbs.Closest(actor.Position) is var orb && orb != null)
            hints.GoalZones.Add(AIHints.GoalProximity(orb.Position, 40, 100));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Collects(pc))
            foreach (var orb in ActiveOrbs)
                Arena.AddCircle(orb.Position, 1, ArenaColor.Safe);
    }
}

class T11RamuhHStates : StateMachineBuilder
{
    public T11RamuhHStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ShockStrike>()
            .ActivateOnEnter<Thunderspark>()
            .ActivateOnEnter<ThunderstormGround>()
            .ActivateOnEnter<ThunderstormPlayer>()
            .ActivateOnEnter<JudgmentBolt>()
            .ActivateOnEnter<Thunderbolt>()
            .ActivateOnEnter<GreyArbiter>()
            .ActivateOnEnter<RollingThunder>();
    }
}

[ModuleInfo(Contributors = "Nonu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 77, NameID = 2832)]
public class T11RamuhH(ModuleInit init) : BossModule(init, new(0, 0), new ArenaBoundsCircle(20));
