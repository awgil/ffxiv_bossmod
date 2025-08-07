using BossMod.Components;

namespace BossMod.Dawntrail.Dungeon.D11MesoTerminal.D111ChirurgeonGeneral;

public enum OID : uint
{
    Boss   = 0x488F,
    Helper = 0x233C,
}

public enum AID : uint
{
    Spell_PungentAerosol     = 43807, // Helper->location, 5.5s cast, range 60 circle
    Spell_SterileSphere      = 43805, // Helper->self, 5.5s cast, range 15 circle
    Spell_SterileSphere1     = 43806, // Helper->self, 5.5s cast, range 8 circle
    Spell_BiochemicalFront   = 43802, // Boss->self, 5.0s cast, range 40 width 65 rect
}

class BiochemicalFront(BossModule module) : Components.StandardAOEs(module, AID.Spell_BiochemicalFront, new AOEShapeRect(40, 65, 0));

class PungentAerosol(BossModule module) : Components.KnockbackFromCastTarget(module, AID.Spell_PungentAerosol, 24f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        foreach (var source in Sources(slot, actor))
        {
            hints.AddForbiddenZone(ShapeContains.InvertedCone(source.Origin, 5f, Angle.FromDirection((Module.Center - source.Origin).Normalized()), 10f.Degrees()));
        }
    }
}

class SterileSphere(BossModule module) : Components.StandardAOEs(module, AID.Spell_SterileSphere, new AOEShapeCircle(15));
class SterileSphere2(BossModule module) : Components.StandardAOEs(module, AID.Spell_SterileSphere1, new AOEShapeCircle(8));

class D111ChirurgeonGeneralStates : StateMachineBuilder
{
    public D111ChirurgeonGeneralStates(BossModule module) : base(module)
    {
        TrivialPhase().
            ActivateOnEnter<PungentAerosol>().
            ActivateOnEnter<BiochemicalFront>().
            ActivateOnEnter<SterileSphere>().
            ActivateOnEnter<SterileSphere2>();
    }
}


[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1028, NameID = 13970)]
public class D111ChirurgeonGeneral(WorldState ws, Actor primary) : BossModule(ws, primary, new(270f, 12f), new ArenaBoundsSquare(20));
