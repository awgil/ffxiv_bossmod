namespace BossMod.RealmReborn.Dungeon.D07Brayflox.D071GreatYellowPelican;

public enum OID : uint
{
    Boss = 0x1A7, // Great Yellow Pelican

    VioletBack = 0x1A9, // Violet Back
    SableBack = 0x1AA // Sable Back
}

public enum AID : uint
{
    AutoAttackBoss = 871, // Boss->player, no cast
    HammerBeak = 504, // Boss->self, no cast, single target
    NumbingBreath = 506, // Boss->self, 3.0s cast, range 9.2 120-degree cone aoe

    AutoAttackTrash = 871, // Trash->player, no cast
    PoisonBreath = 1393 // VioletBack->self, no cast, range 7.20 ?-degree cone cleave
}

class NumbingBreath(BossModule module) : Components.StandardAOEs(module, AID.NumbingBreath, new AOEShapeCone(9.2f, 60.Degrees()));
class PelicanAdds(BossModule module) : Components.AddsMulti(module, [OID.VioletBack, OID.SableBack], 1);

class D071GreatYellowPelicanStates : StateMachineBuilder
{
    public D071GreatYellowPelicanStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<NumbingBreath>()
            .ActivateOnEnter<PelicanAdds>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 8, NameID = 1280)]
public class D071GreatYellowPelican(WorldState ws, Actor primary) : BossModule(ws, primary, primary.Position, new ArenaBoundsCircle(20));
