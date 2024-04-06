namespace BossMod.Endwalker.Criterion.C02AMR.C020Trash1;

class Tornado : Components.LocationTargetedAOEs
{
    public Tornado(AID aid) : base(ActionID.MakeSpell(aid), 6) { }
}
class NTornado : Tornado { public NTornado() : base(AID.NTornado) { } }
class STornado : Tornado { public STornado() : base(AID.STornado) { } }

class ScytheTail : Components.SelfTargetedAOEs
{
    public ScytheTail(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeCircle(10)) { }
}
class NScytheTail : ScytheTail { public NScytheTail() : base(AID.NScytheTail) { } }
class SScytheTail : ScytheTail { public SScytheTail() : base(AID.SScytheTail) { } }

class Twister : Components.StackWithCastTargets
{
    public Twister(AID aid) : base(ActionID.MakeSpell(aid), 8, 4) { }
}
class NTwister : Twister { public NTwister() : base(AID.NTwister) { } }
class STwister : Twister { public STwister() : base(AID.STwister) { } }

class Crosswind : Components.KnockbackFromCastTarget
{
    public Crosswind(AID aid) : base(ActionID.MakeSpell(aid), 25) { }
}
class NCrosswind : Crosswind { public NCrosswind() : base(AID.NCrosswind) { } }
class SCrosswind : Crosswind { public SCrosswind() : base(AID.SCrosswind) { } }

class C020FukoStates : StateMachineBuilder
{
    private bool _savage;

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
            // for yuki
            .ActivateOnEnter<NRightSwipe>(!savage)
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
class C020NFukoStates : C020FukoStates { public C020NFukoStates(BossModule module) : base(module, false) { } }
class C020SFukoStates : C020FukoStates { public C020SFukoStates(BossModule module) : base(module, true) { } }

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NFuko, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 946, NameID = 12399, SortOrder = 2)]
public class C020NFuko : C020Trash1
{
    public C020NFuko(WorldState ws, Actor primary) : base(ws, primary) { }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.NPenghou), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.NYuki), ArenaColor.Object);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SFuko, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 947, NameID = 12399, SortOrder = 2)]
public class C020SFuko : C020Trash1
{
    public C020SFuko(WorldState ws, Actor primary) : base(ws, primary) { }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.SPenghou), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.SYuki), ArenaColor.Object);
    }
}
