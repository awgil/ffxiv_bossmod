namespace BossMod.RealmReborn.Dungeon.D11DzemaelDarkhold.D111AllSeeingEye;

public enum OID : uint
{
    // TODO: remaster AID kit inferred from Lumina — confirm OIDs/shapes in a live Duty Support run
    Boss = 0x4A8A, // R2.700, All-seeing Eye (Duty Support / remaster)
    Helper = 0x233C, // R0.500
    CrystalOld = 0x60B, // R1.000, Corrupted Crystal (trash/legacy)
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    VoidMatterVisual = 45567, // Boss->self, 7.2s cast, single-target visual
    VoidMatter = 45568, // Helper->location, 7.7s cast, range 10 circle
    EvilEye = 45569, // Boss->self, 4.7s cast, range 33 circle gaze
    IntimidatingFlash = 45570, // Boss->self, 2.7s cast, range 10-55 donut
}

class VoidMatter(BossModule module) : Components.StandardAOEs(module, AID.VoidMatter, 10);
class EvilEye(BossModule module) : Components.CastGaze(module, AID.EvilEye);
class IntimidatingFlash(BossModule module) : Components.StandardAOEs(module, AID.IntimidatingFlash, new AOEShapeDonut(10, 55));

class D111AllSeeingEyeStates : StateMachineBuilder
{
    public D111AllSeeingEyeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<VoidMatter>()
            .ActivateOnEnter<EvilEye>()
            .ActivateOnEnter<IntimidatingFlash>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 13, NameID = 1397)] // TODO: clear after remaster AID kit live verify
public class D111AllSeeingEye(ModuleInit init) : BossModule(init, new(48, 78), new ArenaBoundsSquare(25));
