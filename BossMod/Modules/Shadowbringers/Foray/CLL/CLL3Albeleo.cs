namespace BossMod.Shadowbringers.Foray.CLL;

public enum OID : uint
{
    Boss = 0x2F06, // R0.500, x1
    AlbeleosCanisDirus = 0x2F07, // R4.200, x2
    O4ThLegionArmoredWeapon = 0x2EDF, // R4.080, x2
    O4ThLegionPredator = 0x2EDE, // R2.100, x4 (spawn during fight)
    O4ThLegionReaper = 0x2EE1, // R0.500-2.000, x0 (spawn during fight)
    O4ThLegionDuplicarius = 0x2EE2, // R0.500, x0 (spawn during fight)
    AlbeleosHrodvitnir = 0x2F0A, // R4.050, x0 (spawn during fight)
    AlbeleosMonstrosity = 0x2F09, // R4.600, x0 (spawn during fight)
    O4ThLegionColossus = 0x2EE3, // R2.500, x0 (spawn during fight)
    O4ThLegionDuplicarius1 = 0x2F8A, // R0.500, x0 (spawn during fight)
    O4ThLegionHexadrone = 0x2F8C, // R4.240, x0 (spawn during fight)
    O4ThLegionExecutioner = 0x2F8B, // R0.500, x0 (spawn during fight)
    O4ThLegionLaquearius = 0x2EEF, // R0.500, x0 (spawn during fight)
    O4ThLegionEques = 0x2EF0, // R0.500, x0 (spawn during fight)
    O4ThLegionDuplicarius2 = 0x2EDD, // R0.500, x0 (spawn during fight)
    O4ThLegionScorpion = 0x2FCA, // R6.000, x0 (spawn during fight)
}

public enum AID : uint
{
    PhotonStream = 21504, // O4ThLegionReaper->player, no cast, single-target
    MagitekRay = 21502, // O4ThLegionPredator->self, 3.0s cast, range 40 width 6 rect
    MagitekCannon = 21505, // O4ThLegionReaper->location, 3.0s cast, range 6 circle
    GrandSword = 21506, // O4ThLegionColossus->self, 5.0s cast, range 27 120-degree cone
    DiffractiveLaser = 21503, // O4ThLegionArmoredWeapon->location, 3.0s cast, range 5 circle
    BalefulGaze = 21508, // AlbeleosMonstrosity->self, 4.0s cast, range 35 circle
}

class Adds(BossModule module) : Components.AddsMulti(module, [OID.AlbeleosCanisDirus, OID.O4ThLegionArmoredWeapon, OID.O4ThLegionColossus, OID.O4ThLegionPredator, OID.O4ThLegionReaper, OID.O4ThLegionDuplicarius, OID.AlbeleosHrodvitnir, OID.AlbeleosMonstrosity, OID.O4ThLegionColossus, OID.O4ThLegionDuplicarius1, OID.O4ThLegionHexadrone, OID.O4ThLegionExecutioner, OID.O4ThLegionLaquearius, OID.O4ThLegionEques, OID.O4ThLegionDuplicarius2, OID.O4ThLegionScorpion], 1);

class MagitekRay(BossModule module) : Components.StandardAOEs(module, AID.MagitekRay, new AOEShapeRect(40, 3));
class MagitekCannon(BossModule module) : Components.StandardAOEs(module, AID.MagitekCannon, 6);
class GrandSword(BossModule module) : Components.StandardAOEs(module, AID.GrandSword, new AOEShapeCone(27, 60.Degrees()));
class DiffractiveLaser(BossModule module) : Components.StandardAOEs(module, AID.DiffractiveLaser, 5);
class BalefulGaze(BossModule module) : Components.CastGaze(module, AID.BalefulGaze);

class AlbeleoTheMaleficentStates : StateMachineBuilder
{
    public AlbeleoTheMaleficentStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<MagitekRay>()
            .ActivateOnEnter<MagitekCannon>()
            .ActivateOnEnter<GrandSword>()
            .ActivateOnEnter<DiffractiveLaser>()
            .ActivateOnEnter<BalefulGaze>()
            .Raw.Update = () => Module.PrimaryActor.HPMP.CurHP <= 1;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 735, NameID = 9433)]
public class AlbeleoTheMaleficent(WorldState ws, Actor primary) : BossModule(ws, primary, new(80, -392.4f), MakeBounds())
{
    private static ArenaBoundsCustom MakeBounds()
    {
        var b = CurveApprox.Rect(new(30, 0), new(0, 17.1f));
        var d = CurveApprox.Rect(new(11.3f, 0), new(0, 8.8f));
        return new(30, new PolygonClipper().Difference(new(b), new(d.Select(c => c + new WDir(22.5f, 13.4f)))));
    }
}
