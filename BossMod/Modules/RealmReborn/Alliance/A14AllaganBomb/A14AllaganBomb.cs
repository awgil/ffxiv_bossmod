namespace BossMod.RealmReborn.Alliance.A14AllaganBomb;

public enum OID : uint
{
    Boss = 0x967, // R1.500-4.500, x1
    Vassago = 0x96A, // R4.600, x3
    AllaganBalloon = 0x968, // R2.400, x0 (spawn during fight)
    AllaganNapalm = 0x969, // R2.400, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 1461, // Vassago->player, no cast, single-target
    GrimHalo = 1773, // Vassago->self, no cast, range 6+R circle
    GrimCleaver = 1772, // Vassago->location, no cast, range 1 circle
    GrimFate = 1775, // Vassago->self, no cast, range 8+R ?-degree cone
    AutoAttackBalloon = 1459, // AllaganBalloon->player, no cast, single-target
    CorruptedTail = 1771, // Vassago->player, no cast, single-target
}

class Adds(BossModule module) : Components.AddsMulti(module, [OID.Vassago, OID.AllaganBalloon, OID.AllaganNapalm]);

class BossPrio(BossModule module) : Components.GenericInvincible(module)
{
    protected override IEnumerable<Actor> ForbiddenTargets(int slot, Actor actor)
    {
        if (Module.Enemies(OID.Vassago).Any(v => !v.IsDead))
            yield return Module.PrimaryActor;
    }
}

class A14AllaganBombStates : StateMachineBuilder
{
    public A14AllaganBombStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BossPrio>()
            .ActivateOnEnter<Adds>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 92, NameID = 1873)]
public class A14AllaganBomb(WorldState ws, Actor primary) : BossModule(ws, primary, new(-110, -165.6f), new ArenaBoundsCircle(33.2f));
