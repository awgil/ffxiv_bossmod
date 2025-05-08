namespace BossMod.Endwalker.Criterion.C02AMR.C020Trash1;

class Tornado(BossModule module, AID aid) : Components.StandardAOEs(module, aid, 6);
class NTornado(BossModule module) : Tornado(module, AID.NTornado);
class STornado(BossModule module) : Tornado(module, AID.STornado);

class ScytheTail(BossModule module, AID aid) : Components.StandardAOEs(module, aid, new AOEShapeCircle(10));
class NScytheTail(BossModule module) : ScytheTail(module, AID.NScytheTail);
class SScytheTail(BossModule module) : ScytheTail(module, AID.SScytheTail);

class Twister(BossModule module, AID aid) : Components.StackWithCastTargets(module, aid, 8, 4);
class NTwister(BossModule module) : Twister(module, AID.NTwister);
class STwister(BossModule module) : Twister(module, AID.STwister);

class Crosswind(BossModule module, AID aid) : Components.KnockbackFromCastTarget(module, aid, 25);
class NCrosswind(BossModule module) : Crosswind(module, AID.NCrosswind);
class SCrosswind(BossModule module) : Crosswind(module, AID.SCrosswind);

class C020FukoStates : StateMachineBuilder
{
    private readonly bool _savage;

    public C020FukoStates(BossModule module, bool savage) : base(module)
    {
        _savage = savage;
        DeathPhase(0, SinglePhase)
            .ActivateOnEnter<NTornado>(!savage)
            .ActivateOnEnter<NScytheTail>(!savage)
            .ActivateOnEnter<NTwister>(!savage)
            .ActivateOnEnter<NCrosswind>(!savage)
            .ActivateOnEnter<STornado>(savage)
            .ActivateOnEnter<SScytheTail>(savage)
            .ActivateOnEnter<STwister>(savage)
            .ActivateOnEnter<SCrosswind>(savage)
            .ActivateOnEnter<NRightSwipe>(!savage) // for yuki
            .ActivateOnEnter<NLeftSwipe>(!savage)
            .ActivateOnEnter<SRightSwipe>(savage)
            .ActivateOnEnter<SLeftSwipe>(savage);
    }

    private void SinglePhase(uint id)
    {
        ScytheTail(id, 5.7f);
        Twister(id + 0x10000, 2.1f);
        Crosswind(id + 0x20000, 12.0f);
        ScytheTail(id + 0x30000, 5.8f);
        Twister(id + 0x40000, 2.1f);
        Crosswind(id + 0x50000, 10.4f);
        ScytheTail(id + 0x60000, 6.0f);
        Twister(id + 0x70000, 4.1f);
        SimpleState(id + 0xFF0000, 10, "???");
    }

    private void ScytheTail(uint id, float delay)
    {
        Cast(id, _savage ? AID.SScytheTail : AID.NScytheTail, delay, 4, "Out");
    }

    private void Twister(uint id, float delay)
    {
        Cast(id, _savage ? AID.STwister : AID.NTwister, delay, 5, "Stack");
    }

    private void Crosswind(uint id, float delay)
    {
        Cast(id, _savage ? AID.SCrosswind : AID.NCrosswind, delay, 4, "Knockback");
    }
}
class C020NFukoStates(BossModule module) : C020FukoStates(module, false);
class C020SFukoStates(BossModule module) : C020FukoStates(module, true);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NFuko, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 946, NameID = 12399, SortOrder = 2)]
public class C020NFuko(WorldState ws, Actor primary) : C020Trash1(ws, primary)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.NPenghou), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.NYuki), ArenaColor.Object);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SFuko, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 947, NameID = 12399, SortOrder = 2)]
public class C020SFuko(WorldState ws, Actor primary) : C020Trash1(ws, primary)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.SPenghou), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.SYuki), ArenaColor.Object);
    }
}
