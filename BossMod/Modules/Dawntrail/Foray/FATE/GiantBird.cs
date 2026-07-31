namespace BossMod.Modules.Dawntrail.Foray.FATE.GaintBird;

public enum OID : uint {
    Boss = 0x46C1,
    Helper = 0x233C,
    Petrifog = 0x46C2, // R1.300, x0 (spawn during fight)
    Petrifog1 = 0x4821, // R1.300, x0 (spawn during fight)
    Petrifog2 = 0x4822, // R1.300, x0 (spawn during fight)
    Petrifog3 = 0x481F, // R1.300, x0 (spawn during fight)
    Petrifog4 = 0x481E, // R1.300, x0 (spawn during fight)
    Petrifog5 = 0x4820, // R1.300, x0 (spawn during fight)
    Petrifog6 = 0x481D, // R1.300, x0 (spawn during fight)
}

public enum AID : uint {
    AutoAttack = 42900, // Boss->player, no cast, single-target
    Teleport = 44481, // Boss->location, no cast, single-target

    GaleCannon = 41274, // Boss->self, 5.0s cast, range 40 width 10 rect
    Petrisphere = 41272, // Boss->self, 4.0s cast, single-target
    SphereShatter = 41273, // 46C2/481F/481E/481D/4822/4821/4820->self, 2.0s cast, range 7 circle
}

class GaleCannon(BossModule module) : Components.StandardAOEs(module, AID.GaleCannon, new AOEShapeRect(40.0f, 5.0f));

class SphereShatter(BossModule module) : Components.StandardAOEs(module, AID.SphereShatter, new AOEShapeCircle(7.0f)) {
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        base.AddAIHints(slot, actor, assignment, hints);

        // This isn't a perfect solution, but the idea is to just keep the player away from the orb radius and direction its moving to prevent
        // it being caught in the cast. If players are still getting hit can try increasing the radius size to keep them even further out.
        var petrifogs = WorldState.Actors.Where(a => a.OID is (uint)OID.Petrifog or
            (uint)OID.Petrifog1 or (uint)OID.Petrifog2 or (uint)OID.Petrifog3 or
            (uint)OID.Petrifog4 or (uint)OID.Petrifog5 or (uint)OID.Petrifog6).ToList();
        foreach (var petrifog in petrifogs.Where(a => !a.IsDead && a.CastInfo == null)) {
            hints.AddForbiddenZone(ShapeContains.Capsule(petrifog.Position, petrifog.Rotation.ToDirection(), 7.0f, 4.0f), WorldState.FutureTime(5.0f));
        }
    }
}

class GiantBirdStates : StateMachineBuilder {
    public GiantBirdStates(BossModule module) : base(module) {
        TrivialPhase()
            .ActivateOnEnter<GaleCannon>()
            .ActivateOnEnter<SphereShatter>();
    }
}

[ModuleInfo(Incomplete = true, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13739)]
public class GiantBird(WorldState ws, Actor primary) : BossModule(ws, primary, new(-547.0f, -600.0f), new ArenaBoundsCircle(40));
