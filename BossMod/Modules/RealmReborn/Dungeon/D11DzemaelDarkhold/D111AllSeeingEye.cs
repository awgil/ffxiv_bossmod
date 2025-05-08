namespace BossMod.RealmReborn.Dungeon.D11DzemaelDarkhold.D111AllSeeingEye;

public enum OID : uint
{
    Boss = 0x605, // x1
    MoucheVolante = 0x606, // spawn during fight
    Amanuensis = 0x607, // spawn during fight
    Crystal = 0x1E8594, // x6, EventObj type
}

public enum AID : uint
{
    AutoAttack = 870, // Boss/Amanuensis->player, no cast, single-target
    CursedGaze = 512, // Boss->self, 2.5s cast, range 6+2.7 90-degree cone
    DreadGaze = 513, // Boss->self, 3.0s cast, range 6+2.7 90-degree cone
    EyesOnMe = 951, // Boss->self, no cast, raidwide

    AutoAttackAdd = 878, // MoucheVolante->player, no cast, single-target
    Thunderstrike = 1097, // MoucheVolante->self, 2.0s cast, range 10+1.2 width 3 rect
    Condemnation = 1100, // Amanuensis->self, 2.5s cast, range 6+1.3 90-degree cone
}

public enum SID : uint
{
    Invincibility = 325, // none->Boss, extra=0x0
}

class CursedGaze(BossModule module) : Components.StandardAOEs(module, AID.CursedGaze, new AOEShapeCone(8.7f, 45.Degrees()));
class DreadGaze(BossModule module) : Components.StandardAOEs(module, AID.DreadGaze, new AOEShapeCone(8.7f, 45.Degrees()));
class Thunderstrike(BossModule module) : Components.StandardAOEs(module, AID.Thunderstrike, new AOEShapeRect(11.2f, 1.5f));
class Condemnation(BossModule module) : Components.StandardAOEs(module, AID.Condemnation, new AOEShapeCone(7.3f, 45.Degrees()));

// try to always stay in active crystal closest to boss
class Positioning(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Module.PrimaryActor.CastInfo == null) // do not restrict zone while boss is casting, to allow avoiding aoe, even if it means temporarily leaving crystal veil
        {
            var closestCrystal = Module.Enemies(OID.Crystal).Closest(Module.PrimaryActor.Position);
            if (closestCrystal != null)
                hints.AddForbiddenZone(ShapeContains.InvertedCircle(closestCrystal.Position, 8)); // TODO: verify range
        }
    }
}

class D111AllSeeingEyeStates : StateMachineBuilder
{
    public D111AllSeeingEyeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CursedGaze>()
            .ActivateOnEnter<DreadGaze>()
            .ActivateOnEnter<Thunderstrike>()
            .ActivateOnEnter<Condemnation>()
            .ActivateOnEnter<Positioning>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 13, NameID = 1397)]
public class D111AllSeeingEye(WorldState ws, Actor primary) : BossModule(ws, primary, new(40, 70), new ArenaBoundsSquare(30))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (PrimaryActor.FindStatus(SID.Invincibility) != null)
            hints.PotentialTargets.RemoveAll(e => e.Actor == PrimaryActor);
    }
}
