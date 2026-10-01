namespace BossMod.RealmReborn.Dungeon.D11DzemaelDarkhold.D113Batraal;

public enum OID : uint
{
    Boss = 0x4A8D, // R4.600, Batraal (Duty Support / remaster)
    CorruptedCrystal = 0x4A8E, // R1.000
    CorruptedCrystalOld = 0x60B, // R1.000, legacy crystal casts still seen
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    Hit45577 = 45577, // Boss->self, no cast // TODO: identify from replay
    Hit45578 = 45578, // Boss->self, no cast // TODO: identify from replay
    GrimAura = 45579, // Boss->self, 4.7s cast, range 12 circle
    HellsSend = 45580, // Boss->self, 2.7s cast, single-target visual
    Hit45581 = 45581, // Boss->self, no cast // TODO: identify from replay
    Desolation = 45582, // Boss->self, 2.7s cast, range 6-60 donut
    // TODO: remaster crystal may use a different AID than legacy 1167 — confirm CorruptedCrystal casts
    AetherialSurge = 1167, // CorruptedCrystalOld->self, 2.7s cast, range 6 circle
}

class GrimAura(BossModule module) : Components.StandardAOEs(module, AID.GrimAura, 12);
class Desolation(BossModule module) : Components.StandardAOEs(module, AID.Desolation, new AOEShapeDonut(6, 60));
class AetherialSurge(BossModule module) : Components.StandardAOEs(module, AID.AetherialSurge, 6);
class Crystals(BossModule module) : Components.AddsMulti(module, [OID.CorruptedCrystal, OID.CorruptedCrystalOld], 2);

class D113BatraalStates : StateMachineBuilder
{
    public D113BatraalStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GrimAura>()
            .ActivateOnEnter<Desolation>()
            .ActivateOnEnter<AetherialSurge>()
            .ActivateOnEnter<Crystals>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 13, NameID = 1396)] // TODO: clear after remaster crystal AID + unnamed hits
public class D113Batraal(ModuleInit init) : BossModule(init, new(85, -175), new ArenaBoundsSquare(25));
