namespace BossMod.RealmReborn.Dungeon.D16Amdapor.D162DemonWall;

public enum OID : uint
{
    Helper = 0x19A, // x3
    Boss = 0x283, // x1
    Pollen = 0x1E86B1, // x1, EventObj type
}

public enum AID : uint
{
    MurderHole = 1044, // Boss->player, no cast, range 6 circle cleaving autoattack at random target
    LiquefyCenter = 1045, // Helper->self, 3.0s cast, range 50+R width 8 rect
    LiquefySides = 1046, // Helper->self, 2.0s cast, range 50+R width 7 rect
    Repel = 1047, // Boss->self, 3.0s cast, range 40+R 180?-degree cone knockback 20 (non-immunable)
}

class LiquefyCenter(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LiquefyCenter), new AOEShapeRect(50, 4));
class LiquefySides(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LiquefySides), new AOEShapeRect(50, 3.5f));

class Repel(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.Repel), 20, true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // custom hint: stay in narrow zone in center
        if (Casters.Count > 0)
        {
            var safe = ShapeDistance.Rect(Module.PrimaryActor.Position, 0.Degrees(), 50, -2, 1);
            hints.AddForbiddenZone(p => -safe(p));
        }
    }
}

class ForbiddenZones(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect _shape = new(50, 10);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        yield return new(_shape, Module.PrimaryActor.Position, 180.Degrees()); // area behind boss

        var pollen = Module.Enemies(OID.Pollen).FirstOrDefault();
        if (pollen != null && pollen.EventState == 0)
            yield return new(_shape, new(200, -122));
    }
}

class D162DemonWallStates : StateMachineBuilder
{
    public D162DemonWallStates(BossModule module) : base(module)
    {
        // note: no component for Murder Hole - there's not enough space to spread properly, and this hits for small damage
        TrivialPhase()
            .ActivateOnEnter<LiquefyCenter>()
            .ActivateOnEnter<LiquefySides>()
            .ActivateOnEnter<Repel>()
            .ActivateOnEnter<ForbiddenZones>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 14, NameID = 1694)]
public class D162DemonWall(WorldState ws, Actor primary) : BossModule(ws, primary, new(200, -131), new ArenaBoundsRect(10, 21));
