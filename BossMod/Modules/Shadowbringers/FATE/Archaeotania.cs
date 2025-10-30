namespace BossMod.Shadowbringers.FATE.Archaeotania;

public enum OID : uint
{
    Boss = 0x27AD, // R16.150, x1
    MegaflareHelper = 0x2AA0, // R0.500, spawn during fight
    TidalWaveHelper = 0x2BF4, // R0.500, spawn during fight
    TidalWaveVisualHelper = 0x2AB1, // R0.500, spawn during fight
    WindSlashHelper = 0x2995, // R0.500, spawn during fight
    Twister = 0x27B2, // R1.000, spawn during fight
    IceBoulder = 0x2A8D, // R2.200, spawn during fight
    AncientAevis = 0x2A8E, // R4.200, spawn during fight
    Aether = 0x2A8F, // R2.000, spawn during fight
}

public enum AID : uint
{
    AutoAttackBoss = 17085, // Boss->player, no cast, single-target
    BlizzardBreath = 16445, // Boss->self, 3.0s cast, range 60 45-degree cone
    FlamesOfTheApocalypse = 16448, // Boss->self, 2.8s cast, range 15-30 donut
    MindBlast = 16447, // Boss->self, 2.8s cast, range 21 circle
    Megaflare = 16449, // Boss->self, 5.0s cast, single-target, visual (mechanic)
    MegaflareAOE = 17483, // MegaflareHelper->players, 5.0s cast, range 6 circle stack
    TidalWave = 16440, // Boss->self, 3.5s cast, single-target, visual
    TidalWaveVisual = 16452, // TidalWaveVisualHelper->self, 8.5s cast, range 62 width 62 rect, visual
    TidalWaveKnockbackE = 18121, // TidalWaveHelper->location, 8.5s cast, range 62 width 21 rect knockback 48 (cast from W edge)
    TidalWaveKnockbackS = 18122, // TidalWaveHelper->location, 8.5s cast, range 62 width 21 rect knockback 48 (cast from N edge)
    TidalWaveKnockbackW = 18123, // TidalWaveHelper->location, 8.5s cast, range 62 width 21 rect knockback 48 (cast from E edge)
    TidalWaveKnockbackN = 16775, // TidalWaveHelper->location, 8.5s cast, range 62 width 21 rect knockback 48 (cast from S edge)
    WindSlash = 16446, // Boss->self, 3.0s cast, single-target, visual
    WindSlashAOE = 16454, // WindSlashHelper->location, 2.6s cast, range 8 circle
    FlyAway = 16443, // Boss->self, no cast, single-target, applies invincibility
    FlyVisual = 16444, // Boss->self, no cast, single-target, visual (??? during flying)
    FlyReturn = 17389, // Boss->self, no cast, single-target, visual (??? after busters)
    Windwinder = 18057, // Twister->self, no cast, range 5 circle (cast every 2.6s)
    CivilizationBuster1 = 17089, // Boss->self, 4.3s cast, range 62 width 15 rect
    CivilizationBuster2 = 16441, // Boss->self, 4.3s cast, range 62 width 15 rect
    Touchdown = 16442, // Boss->self, 7.0s cast, range 60 circle with ~30 falloff
    PillarImpact = 16455, // IceBoulder->self, 7.0s cast, range 7 circle
    PillarPierce = 16456, // IceBoulder->self, 3.0s cast, range 65 width 8 rect
    AutoAttackAdd = 870, // AncientAevis->player, no cast, single-target
    HeadlongRush = 17042, // AncientAevis->self, 3.0s cast, range 9+R width 10 rect
    Gigaflare = 16451, // Boss->self, 120.0s cast, range 80 circle
}

public enum IconID : uint
{
    Megaflare = 62, // player
}

class BlizzardBreath(BossModule module) : Components.StandardAOEs(module, AID.BlizzardBreath, new AOEShapeCone(60, 22.5f.Degrees()));
class FlamesOfTheApocalypse(BossModule module) : Components.StandardAOEs(module, AID.FlamesOfTheApocalypse, new AOEShapeDonut(15, 30));
class MindBlast(BossModule module) : Components.StandardAOEs(module, AID.MindBlast, new AOEShapeCircle(21));

// note: helpers target their corresponding target for the duration of their life
class Megaflare(BossModule module) : Components.UniformStackSpread(module, 6, 0)
{
    public override void Update()
    {
        // fallback if stack target dies before mechanic is resolved
        // sequence is boss starts casting > icons appear > helpers appear > boss finishes casting > helpers cast their aoe and die
        if (Module.PrimaryActor.CastInfo == null)
            Stacks.RemoveAll(s => !Module.Enemies(OID.MegaflareHelper).Any(h => !h.IsDead && h.TargetID == s.Target.InstanceID));
        base.Update();
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Megaflare)
            AddStack(actor, WorldState.FutureTime(5));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.MegaflareAOE)
            Stacks.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
    }
}

class TidalWave(BossModule module) : Components.KnockbackFromCastTarget(module, AID.TidalWaveVisual, 48, kind: Kind.DirForward);
class WindSlash(BossModule module) : Components.StandardAOEs(module, AID.WindSlashAOE, 8);
class Windwinder(BossModule module) : Components.PersistentVoidzone(module, 5, m => m.Enemies(OID.Twister).Where(a => !a.IsDead));
class CivilizationBuster1(BossModule module) : Components.StandardAOEs(module, AID.CivilizationBuster1, new AOEShapeRect(62, 7.5f));
class CivilizationBuster2(BossModule module) : Components.StandardAOEs(module, AID.CivilizationBuster2, new AOEShapeRect(62, 7.5f));
class Touchdown(BossModule module) : Components.StandardAOEs(module, AID.Touchdown, new AOEShapeCircle(30)); // TODO: verify falloff
class PillarImpact(BossModule module) : Components.StandardAOEs(module, AID.PillarImpact, new AOEShapeCircle(7));
class PillarPierce(BossModule module) : Components.StandardAOEs(module, AID.PillarPierce, new AOEShapeRect(65, 4));
class AncientAevis(BossModule module) : Components.Adds(module, (uint)OID.AncientAevis);
class HeadlongRush(BossModule module) : Components.StandardAOEs(module, AID.HeadlongRush, new AOEShapeRect(25.15f, 5));
class Aether(BossModule module) : Components.Adds(module, (uint)OID.Aether);

class ArchaeotaniaStates : StateMachineBuilder
{
    public ArchaeotaniaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BlizzardBreath>()
            .ActivateOnEnter<FlamesOfTheApocalypse>()
            .ActivateOnEnter<MindBlast>()
            .ActivateOnEnter<Megaflare>()
            .ActivateOnEnter<TidalWave>()
            .ActivateOnEnter<WindSlash>()
            .ActivateOnEnter<Windwinder>()
            .ActivateOnEnter<CivilizationBuster1>()
            .ActivateOnEnter<CivilizationBuster2>()
            .ActivateOnEnter<Touchdown>()
            .ActivateOnEnter<PillarImpact>()
            .ActivateOnEnter<PillarPierce>()
            .ActivateOnEnter<AncientAevis>()
            .ActivateOnEnter<HeadlongRush>()
            .ActivateOnEnter<Aether>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.Fate, GroupID = 1432, NameID = 8234)]
public class Archaeotania(WorldState ws, Actor primary) : BossModule(ws, primary, new(279, 249), new ArenaBoundsSquare(29));
