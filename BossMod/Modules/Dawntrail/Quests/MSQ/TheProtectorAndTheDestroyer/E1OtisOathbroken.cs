namespace BossMod.Dawntrail.Quest.MSQ.TheProtectorAndTheDestroyer.Otis;

public enum OID : uint
{
    Boss = 0x4342, // R3.0    
    EverkeepAerostat = 0x4344, // R2.3
    EverkeepAerostat2 = 0x4345, // R2.3
    EverkeepTurret = 0x4346, // R0.6
    EverkeepSentryG10 = 0x4343, // R0.9
    EverkeepSentryR10 = 0x4347, // R1.999
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->tank, no cast, single-target
    AutoAttack2 = 872, // EverkeepAerostat2->tank, no cast, single-target
    AutoAttack3 = 28538, // EverkeepTurret->tank, no cast, single-target
    AutoAttack4 = 36403, // EverkeepSentryR10->tank, no cast, single-target
    Teleport = 38193, // Boss->location, no cast, single-target
    FormationAlpha = 38194, // Boss->self, 5.0s cast, single-target
    ThrownFlames = 38205, // EverkeepAerostat2->self, 6.0s cast, range 8 circle
    BastionBreaker = 38198, // Helper->all, 6.0s cast, range 6 circle, spread
    SearingSlash = 38197, // Boss->self, 6.0s cast, range 8 circle
    StormlitShockwave = 38202, // Boss->self, 5.0s cast, range 40 circle
    SelfDestruct = 38206, // EverkeepAerostat2->self, 8.0s cast, range 40 circle
    FormationBeta = 38195, // Boss->self, 5.0s cast, single-target
    Electrobeam = 38207, // EverkeepTurret->self, 6.0s cast, range 40 width 4 rect
    HolyBlade = 38199, // Helper->Alisaie, 6.0s cast, range 6 circle, stack
    SteadfastWill = 38201, // Boss->tank, 5.0s cast, single-target
    SelfDestruct2 = 38208, // EverkeepTurret->self, 8.0s cast, range 40 circle
    FormationGamma = 38196, // Boss->self, 5.0s cast, single-target
    Rush = 38209, // EverkeepSentryR10->location, 5.0s cast, width 5 rect charge
    SelfDestruct23 = 38210, // EverkeepSentryR10->self, 8.0s cast, range 40 circle
    ValorousAscension = 38203, // Boss->self, 8.0s cast, range 40 circle
    RendPower = 38200, // Helper->self, 4.5s cast, range 40 30-degree cone
    ModelChange = 38204 // Boss->self, no cast, single-target
}

class StormlitShockwave(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.StormlitShockwave));
class ValorousAscension(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ValorousAscension));
class RendPower(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RendPower), new AOEShapeCone(40, 15.Degrees()), 6);
class ThrownFlames(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ThrownFlames), new AOEShapeCircle(8));
class BastionBreaker(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.BastionBreaker), 6);
class HolyBlade(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.HolyBlade), 6);
class SearingSlash(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SearingSlash), new AOEShapeCircle(8));
class Electrobeam(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Electrobeam), new AOEShapeRect(40, 2));

class Rush(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
            for (var i = 0; i < Math.Clamp(_aoes.Count, 0, 2); ++i)
                yield return new(_aoes[i].Shape, _aoes[i].Origin, _aoes[i].Rotation, _aoes[i].Activation, ArenaColor.Danger);
        if (_aoes.Count > 2)
            for (var i = 2; i < 4; ++i)
                yield return new(_aoes[i].Shape, _aoes[i].Origin, _aoes[i].Rotation, _aoes[i].Activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Rush)
        {
            var dir = spell.LocXZ - caster.Position;
            _aoes.Add(new(new AOEShapeRect(dir.Length(), 2.5f), caster.Position, Angle.FromDirection(dir), spell.NPCFinishAt));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID == AID.Rush)
            _aoes.RemoveAt(0);
    }
}
class SteadfastWill(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.SteadfastWill));

class OtisOathbrokenStates : StateMachineBuilder
{
    public OtisOathbrokenStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<StormlitShockwave>()
            .ActivateOnEnter<ValorousAscension>()
            .ActivateOnEnter<RendPower>()
            .ActivateOnEnter<ThrownFlames>()
            .ActivateOnEnter<BastionBreaker>()
            .ActivateOnEnter<SearingSlash>()
            .ActivateOnEnter<Electrobeam>()
            .ActivateOnEnter<SteadfastWill>()
            .ActivateOnEnter<HolyBlade>()
            .ActivateOnEnter<Rush>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70478, NameID = 13168)]
public class OtisOathbroken(WorldState ws, Actor primary) : BossModule(ws, primary, new(349, -14), new ArenaBoundsCircle(19.5f))
{
    protected override bool CheckPull() => PrimaryActor.IsTargetable && PrimaryActor.InCombat || Enemies(OID.EverkeepSentryG10).Any(e => e.InCombat) || Enemies(OID.EverkeepAerostat).Any(e => e.InCombat);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.EverkeepAerostat))
            Arena.Actor(s, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.EverkeepAerostat2))
            Arena.Actor(s, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.EverkeepSentryG10))
            Arena.Actor(s, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.EverkeepSentryR10))
            Arena.Actor(s, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.EverkeepTurret))
            Arena.Actor(s, ArenaColor.Enemy);
    }
}
