namespace BossMod.RealmReborn.Dungeon.D01Sastasha.D012CaptainMadison;

public enum OID : uint
{
    // TODO: confirm add OIDs + hitbox radius from live Sastasha run
    Boss = 0x566, // R?, Captain Madison (NameID 1382)
    ShallowtailReaver = 0x156, // R?, NameID 342 Shallowtail Reaver
    ScurvyDog = 0x4B5, // R?, NameID 1205
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    Sandslinger = 496, // Boss->self, no cast, range 6 cone (accuracy down)
}

class Sandslinger(BossModule module) : Components.Cleave(module, AID.Sandslinger, new AOEShapeCone(6, 45.Degrees())); // TODO: verify angle
class MadisonAdds(BossModule module) : Components.AddsMulti(module, [OID.ShallowtailReaver, OID.ScurvyDog], 1);

class D012CaptainMadisonStates : StateMachineBuilder
{
    public D012CaptainMadisonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Sandslinger>()
            .ActivateOnEnter<MadisonAdds>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 4, NameID = 1382)] // TODO: clear after add OIDs + Sandslinger angle
public class D012CaptainMadison(WorldState ws, Actor primary) : BossModule(ws, primary, primary.Position, new ArenaBoundsCircle(20));
