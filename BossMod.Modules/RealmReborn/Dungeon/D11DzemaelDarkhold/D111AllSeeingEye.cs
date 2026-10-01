namespace BossMod.RealmReborn.Dungeon.D11DzemaelDarkhold.D111AllSeeingEye;

public enum OID : uint
{
    Boss = 0x4A8A, // R2.700, All-seeing Eye (Duty Support / remaster)
    Helper = 0x233C, // R0.500
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    VoidMatterVisual = 45567, // Boss->self, 7.2s cast, single-target visual
    VoidMatter = 45568, // Helper->location, 7.7s cast, range 10 circle
    EyesOnMe = 45569, // Boss->self, 5.0s cast, range 33 circle gaze
    BlusteringBlink = 45570, // Boss->self, 3.0s cast, range 55 width 10 rect
}

class VoidMatter(BossModule module) : Components.StandardAOEs(module, AID.VoidMatter, 10);
class EyesOnMe(BossModule module) : Components.CastGaze(module, AID.EyesOnMe);
class BlusteringBlink(BossModule module) : Components.StandardAOEs(module, AID.BlusteringBlink, new AOEShapeRect(55, 5));

class D111AllSeeingEyeStates : StateMachineBuilder
{
    public D111AllSeeingEyeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<VoidMatter>()
            .ActivateOnEnter<EyesOnMe>()
            .ActivateOnEnter<BlusteringBlink>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 13, NameID = 1397)]
public class D111AllSeeingEye(ModuleInit init) : BossModule(init, new(48, 78), new ArenaBoundsCircle(19.5f));
