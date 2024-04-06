namespace BossMod.Endwalker.Quest.Endwalker;

class EndwalkerStates : StateMachineBuilder
{
    private Endwalker _module;

    public EndwalkerStates(Endwalker module) : base(module)
    {
        _module = module;
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
            .Raw.Update = () => _module.ZenosP2() is var ZenosP2 && ZenosP2 != null && !ZenosP2.IsTargetable && ZenosP2.HP.Cur <= 1;
    }
}

class Megaflare : Components.LocationTargetedAOEs
{
    public Megaflare() : base(ActionID.MakeSpell(AID.Megaflare), 6) { }
}

class Puddles : Components.PersistentInvertibleVoidzoneByCast
{
    public Puddles() : base(5, m => m.Enemies(OID.Puddles).Where(e => e.EventState != 7), ActionID.MakeSpell(AID.Hellfire)) { }
}

class JudgementBolt : Components.RaidwideCast
{
    public JudgementBolt() : base(ActionID.MakeSpell(AID.JudgementBoltVisual)) { }
}

class Hellfire : Components.RaidwideCast
{
    public Hellfire() : base(ActionID.MakeSpell(AID.HellfireVisual)) { }
}

class StarBeyondStars : Components.SelfTargetedAOEs
{
    public StarBeyondStars() : base(ActionID.MakeSpell(AID.StarBeyondStarsHelper), new AOEShapeCone(50, 15.Degrees()), 6) { }
}

class TheEdgeUnbound : Components.SelfTargetedAOEs
{
    public TheEdgeUnbound() : base(ActionID.MakeSpell(AID.TheEdgeUnbound), new AOEShapeCircle(10)) { }
}

class WyrmsTongue : Components.SelfTargetedAOEs
{
    public WyrmsTongue() : base(ActionID.MakeSpell(AID.WyrmsTongueHelper), new AOEShapeCone(40, 30.Degrees())) { }
}

class NineNightsAvatar : Components.SelfTargetedAOEs
{
    public NineNightsAvatar() : base(ActionID.MakeSpell(AID.NineNightsAvatar), new AOEShapeCircle(10)) { Color = ArenaColor.Danger; }
}

class NineNightsHelpers : Components.SelfTargetedAOEs
{
    public NineNightsHelpers() : base(ActionID.MakeSpell(AID.NineNightsHelpers), new AOEShapeCircle(10), 6) { }

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        => ActiveCasters.Select((c, i) => new AOEInstance(Shape, c.Position, c.CastInfo!.Rotation, c.CastInfo.NPCFinishAt, i < 2 ? ArenaColor.Danger : ArenaColor.AOE));
}

class VeilAsunder : Components.LocationTargetedAOEs
{
    public VeilAsunder() : base(ActionID.MakeSpell(AID.VeilAsunderHelper), 6) { }
}

class MortalCoil : Components.SelfTargetedAOEs
{
    public MortalCoil() : base(ActionID.MakeSpell(AID.MortalCoilVisual), new AOEShapeDonut(8, 20)) { }
}

class DiamondDust : Components.RaidwideCast
{
    public DiamondDust() : base(ActionID.MakeSpell(AID.DiamondDustVisual), "Raidwide. Turns floor to ice.") { }
}

class DeadGaze : Components.CastGaze
{
    public DeadGaze() : base(ActionID.MakeSpell(AID.DeadGazeVisual)) { }
}

class TidalWave2 : Components.KnockbackFromCastTarget
{
    public TidalWave2() : base(ActionID.MakeSpell(AID.TidalWaveVisual2), 25, kind: Kind.DirForward) { StopAtWall = true; }
}

class SwiftAsShadow : Components.ChargeAOEs
{
    public SwiftAsShadow() : base(ActionID.MakeSpell(AID.SwiftAsShadow), 1) { }
}

class Extinguishment : Components.SelfTargetedAOEs
{
    public Extinguishment() : base(ActionID.MakeSpell(AID.ExtinguishmentVisual), new AOEShapeDonut(10, 30)) { }
}

class TheEdgeUnbound2 : Components.SelfTargetedAOEs
{
    public TheEdgeUnbound2() : base(ActionID.MakeSpell(AID.TheEdgeUnbound2), new AOEShapeCircle(10)) { }
}

class UnmovingDvenadkatik : Components.SelfTargetedAOEs
{
    public UnmovingDvenadkatik() : base(ActionID.MakeSpell(AID.UnmovingDvenadkatikVisual), new AOEShapeCone(50, 15.Degrees()), 10) { }

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        => ActiveCasters.Select((c, i) => new AOEInstance(Shape, c.Position, c.CastInfo!.Rotation, c.CastInfo.NPCFinishAt, i < 2 ? ArenaColor.Danger : ArenaColor.AOE));
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "croizat", PrimaryActorOID = (uint)OID.ZenosP1, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70000, NameID = 10393)]
public class Endwalker : BossModule
{
    private IReadOnlyList<Actor> _zenosP2;

    public Actor? ZenosP1() => PrimaryActor.IsDestroyed ? null : PrimaryActor;
    public Actor? ZenosP2() => _zenosP2.FirstOrDefault();

    public Endwalker(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(100, 100), 20))
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
