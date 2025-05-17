namespace BossMod.Endwalker.Quest.Endwalker;

class EndwalkerStates : StateMachineBuilder
{
    public EndwalkerStates(Endwalker module) : base(module)
    {
        DeathPhase(0, id => { SimpleState(id, 10000, "Enrage"); })
            .ActivateOnEnter<TidalWave>()
            .ActivateOnEnter<Megaflare>()
            .ActivateOnEnter<Puddles>()
            .ActivateOnEnter<JudgementBolt>()
            .ActivateOnEnter<Hellfire>()
            .ActivateOnEnter<AkhMorn>()
            .ActivateOnEnter<StarBeyondStars>()
            .ActivateOnEnter<TheEdgeUnbound>()
            .ActivateOnEnter<WyrmsTongue>()
            .ActivateOnEnter<NineNightsAvatar>()
            .ActivateOnEnter<NineNightsHelpers>()
            .ActivateOnEnter<VeilAsunder>()
            .ActivateOnEnter<Exaflare>()
            .ActivateOnEnter<DiamondDust>()
            .ActivateOnEnter<DeadGaze>()
            .ActivateOnEnter<MortalCoil>()
            .ActivateOnEnter<TidalWave2>();

        SimplePhase(1, id => { SimpleState(id, 10000, "Enrage"); }, "P2")
            .ActivateOnEnter<AetherialRay>()
            .ActivateOnEnter<SilveredEdge>()
            .ActivateOnEnter<VeilAsunder>()
            .ActivateOnEnter<SwiftAsShadow>()
            .ActivateOnEnter<Candlewick>()
            .ActivateOnEnter<AkhMorn>()
            .ActivateOnEnter<Extinguishment>()
            .ActivateOnEnter<WyrmsTongue>()
            .ActivateOnEnter<UnmovingDvenadkatik>()
            .ActivateOnEnter<TheEdgeUnbound2>()
            .Raw.Update = () => module.ZenosP2() is var ZenosP2 && ZenosP2 != null && !ZenosP2.IsTargetable && ZenosP2.HPMP.CurHP <= 1;
    }
}

class Megaflare(BossModule module) : Components.StandardAOEs(module, AID.Megaflare, 6);
class Puddles(BossModule module) : Components.PersistentInvertibleVoidzoneByCast(module, 5, m => m.Enemies(OID.Puddles).Where(e => e.EventState != 7), AID.Hellfire);
class JudgementBolt(BossModule module) : Components.RaidwideCast(module, AID.JudgementBoltVisual);
class Hellfire(BossModule module) : Components.RaidwideCast(module, AID.HellfireVisual);
class StarBeyondStars(BossModule module) : Components.StandardAOEs(module, AID.StarBeyondStarsHelper, new AOEShapeCone(50, 15.Degrees()), 6);
class TheEdgeUnbound(BossModule module) : Components.StandardAOEs(module, AID.TheEdgeUnbound, new AOEShapeCircle(10));
class WyrmsTongue(BossModule module) : Components.StandardAOEs(module, AID.WyrmsTongueHelper, new AOEShapeCone(40, 30.Degrees()));

class NineNightsAvatar : Components.StandardAOEs
{
    public NineNightsAvatar(BossModule module) : base(module, AID.NineNightsAvatar, new AOEShapeCircle(10)) { Color = ArenaColor.Danger; }
}

class NineNightsHelpers(BossModule module) : Components.StandardAOEs(module, AID.NineNightsHelpers, new AOEShapeCircle(10), 6)
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => ActiveCasters.Select((c, i) => new AOEInstance(Shape, c.Position, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo), i < 2 ? ArenaColor.Danger : ArenaColor.AOE));
}

class VeilAsunder(BossModule module) : Components.StandardAOEs(module, AID.VeilAsunderHelper, 6);
class MortalCoil(BossModule module) : Components.StandardAOEs(module, AID.MortalCoilVisual, new AOEShapeDonut(8, 20));
class DiamondDust(BossModule module) : Components.RaidwideCast(module, AID.DiamondDustVisual, "Raidwide. Turns floor to ice.");
class DeadGaze(BossModule module) : Components.CastGaze(module, AID.DeadGazeVisual);
class TidalWave2(BossModule module) : Components.KnockbackFromCastTarget(module, AID.TidalWaveVisual2, 25, kind: Kind.DirForward, stopAtWall: true);
class SwiftAsShadow(BossModule module) : Components.ChargeAOEs(module, AID.SwiftAsShadow, 1);
class Extinguishment(BossModule module) : Components.StandardAOEs(module, AID.ExtinguishmentVisual, new AOEShapeDonut(10, 30));
class TheEdgeUnbound2(BossModule module) : Components.StandardAOEs(module, AID.TheEdgeUnbound2, new AOEShapeCircle(10));

class UnmovingDvenadkatik(BossModule module) : Components.StandardAOEs(module, AID.UnmovingDvenadkatikVisual, new AOEShapeCone(50, 15.Degrees()), 10)
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => ActiveCasters.Select((c, i) => new AOEInstance(Shape, c.Position, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo), i < 2 ? ArenaColor.Danger : ArenaColor.AOE));
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "croizat", PrimaryActorOID = (uint)OID.ZenosP1, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70000, NameID = 10393)]
public class Endwalker : BossModule
{
    private readonly IReadOnlyList<Actor> _zenosP2;

    public Actor? ZenosP1() => PrimaryActor.IsDestroyed ? null : PrimaryActor;
    public Actor? ZenosP2() => _zenosP2.FirstOrDefault();

    public Endwalker(WorldState ws, Actor primary) : base(ws, primary, new(100, 100), new ArenaBoundsSquare(20))
    {
        _zenosP2 = Enemies(OID.ZenosP2);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        switch (StateMachine.ActivePhaseIndex)
        {
            case 0:
                Arena.Actor(ZenosP1(), ArenaColor.Enemy);
                break;
            case 1:
                Arena.Actor(ZenosP2(), ArenaColor.Enemy);
                break;
        }
    }
}
