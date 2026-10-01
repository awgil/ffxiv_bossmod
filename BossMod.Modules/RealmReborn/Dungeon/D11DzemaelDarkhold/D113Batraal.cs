namespace BossMod.RealmReborn.Dungeon.D11DzemaelDarkhold.D113Batraal;

public enum OID : uint
{
    Boss = 0x4A8D, // R4.600, Batraal (Duty Support / remaster)
    CorruptedCrystal = 0x4A8E, // R1.000
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    GrimCleaver = 45577, // Boss->player, no cast, single-target
    GrimFate = 45578, // Boss->player, no cast, single-target
    GrimHalo = 45579, // Boss->self, 4.7s cast, range 12 circle
    Hellssend = 45580, // Boss->self, 2.7s cast, single-target visual
    CorruptedCrystals = 45581, // Boss->CorruptedCrystal, no cast, single-target
    Desolation = 45582, // Boss->self, 3.0s cast, range 60 width 6 rect
}

class GrimHalo(BossModule module) : Components.StandardAOEs(module, AID.GrimHalo, 12);
class Desolation(BossModule module) : Components.StandardAOEs(module, AID.Desolation, new AOEShapeRect(60, 3));
class Crystals(BossModule module) : Components.Adds(module, (uint)OID.CorruptedCrystal, 2);

class D113BatraalStates : StateMachineBuilder
{
    public D113BatraalStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GrimHalo>()
            .ActivateOnEnter<Desolation>()
            .ActivateOnEnter<Crystals>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 13, NameID = 1396)]
public class D113Batraal(ModuleInit init) : BossModule(init, new(85, -175), new ArenaBoundsSquare(25));
