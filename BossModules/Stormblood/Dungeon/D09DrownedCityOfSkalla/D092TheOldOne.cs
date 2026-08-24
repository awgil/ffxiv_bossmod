namespace BossMod.Stormblood.Dungeon.D09DrownedCityOfSkalla.D092TheOldOne;

public enum OID : uint
{
    Boss = 0x1FAC, // R4.600, x1
    Helper = 0x18D6, // R0.500, x12, Helper type
    Subservient = 0x1FAD, // R1.725, x0 (spawn during fight)
    Unk = 0x204F, // R5.000, x0 (spawn during fight), Helper type
}

// shatterstone 9823
public enum AID : uint
{
    AutoAttack = 29791, // Boss->player, no cast, single-target
    MysticLight = 9815, // Boss->self, 4.0s cast, range 40+R 60-degree cone
    MysticFlameCast = 9816, // Boss->self, 3.0s cast, single-target
    MysticFlame = 9817, // Helper->self, 3.5s cast, range 8 circle
    ShiftingLight = 9818, // Boss->self, 3.0s cast, range 20+R circle
    Shatterstone = 9824, // Helper->self, 2.0s cast, range 5 circle
    OrderToDetonate = 9819, // Boss->self, 20.0s cast, single-target
}

public enum SID : uint
{
    Invincibility = 325, // none->Helper/Boss/Unk, extra=0x0
    Transfiguration = 1448, // none->player, extra=0x4A
}

public enum TetherID : uint
{
    AddsTether = 63, // Subservient->Boss
}

class MysticLight(BossModule module) : Components.StandardAOEs(module, AID.MysticLight, new AOEShapeCone(40, 30.Degrees()));
class MysticFlame(BossModule module) : Components.StandardAOEs(module, AID.MysticFlame, 8);
class ShiftingLight(BossModule module) : Components.RaidwideCast(module, AID.ShiftingLight, "Raidwide + transform");
class Invincibility(BossModule module) : Components.InvincibleStatus(module, (uint)SID.Invincibility);
class Subservient(BossModule module) : Components.Adds(module, (uint)OID.Subservient, 1, true);
class Transfiguration(BossModule module) : Components.StandardAOEs(module, AID.Shatterstone, new AOEShapeCircle(5))
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => base.ActiveAOEs(slot, actor).Select(s => s with { Risky = false, Color = ArenaColor.SafeFromAOE });

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (actor.FindStatus(SID.Transfiguration) == null)
            return;

        var livingAdds = hints.PotentialTargets.Where(t => t.Actor.OID == (uint)OID.Subservient && t.Priority >= 0).ToList();
        var casts = ActiveAOEs(slot, actor).ToList();

        livingAdds.RemoveAll(a => casts.Any(c => HitByShatter(a, c.Origin)));

        hints.GoalZones.Add(p => livingAdds.Count(a => HitByShatter(a, p)));

        if (livingAdds.Any(a => HitByShatter(a, actor.Position)))
            hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Shatterstone), actor, 3000, targetPos: actor.PosRot.XYZ());
    }

    static bool HitByShatter(AIHints.Enemy e, WPos source) => e.Actor.Position.InCircle(source, 5 + e.Actor.HitboxRadius);
}

class D092TheOldOneStates : StateMachineBuilder
{
    public D092TheOldOneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MysticLight>()
            .ActivateOnEnter<MysticFlame>()
            .ActivateOnEnter<ShiftingLight>()
            .ActivateOnEnter<Invincibility>()
            .ActivateOnEnter<Subservient>()
            .ActivateOnEnter<Transfiguration>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 279, NameID = 6908)]
public class D092TheOldOne(WorldState ws, Actor primary) : BossModule(ws, primary, new(115, 4), new ArenaBoundsCircle(19.5f));
