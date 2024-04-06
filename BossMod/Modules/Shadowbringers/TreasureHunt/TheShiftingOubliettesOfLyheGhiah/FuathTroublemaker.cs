namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.FuathTroublemaker;

public enum OID : uint
{
    Boss = 0x302F, //R=3.25
    BossAdd = 0x3019, //R=1.8
    BossHelper = 0x233C,
    BonusAdd_FuathTrickster = 0x3033, // R0.750
};

public enum AID : uint
{
    AutoAttack = 21733, // Boss->player, no cast, single-target
    FrigidNeedle = 21739, // Boss->self, 3,0s cast, single-target
    FrigidNeedle2 = 21740, // BossHelper->self, 3,0s cast, range 40 width 5 cross
    Spittle = 21735, // Boss->self, no cast, single-target
    Spittle2 = 21736, // BossHelper->location, 4,0s cast, range 8 circle
    CroakingChorus = 21738, // Boss->self, 3,0s cast, single-target, calls adds
    ToyHammer = 21734, // Boss->player, 4,0s cast, single-target
    Hydrocannon = 21737, // Boss->players, 5,0s cast, range 6 circle
    Telega = 9630, // BonusAdds->self, no cast, single-target, bonus adds disappear
};

class CroakingChorus : Components.CastHint
{
    public CroakingChorus() : base(ActionID.MakeSpell(AID.CroakingChorus), "Calls adds") { }
}

class FrigidNeedle : Components.SelfTargetedAOEs
{
    public FrigidNeedle() : base(ActionID.MakeSpell(AID.FrigidNeedle2), new AOEShapeCross(40, 2.5f)) { }
}

class Spittle : Components.LocationTargetedAOEs
{
    public Spittle() : base(ActionID.MakeSpell(AID.Spittle2), 8) { }
}

class ToyHammer : Components.SingleTargetCast
{
    public ToyHammer() : base(ActionID.MakeSpell(AID.ToyHammer)) { }
}

class Hydrocannon : Components.StackWithCastTargets
{
    public Hydrocannon() : base(ActionID.MakeSpell(AID.Hydrocannon), 6) { }
}

class FuathTroublemakerStates : StateMachineBuilder
{
    public FuathTroublemakerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CroakingChorus>()
            .ActivateOnEnter<ToyHammer>()
            .ActivateOnEnter<Spittle>()
            .ActivateOnEnter<Hydrocannon>()
            .ActivateOnEnter<FrigidNeedle>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BonusAdd_FuathTrickster).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 745, NameID = 9786)]
public class FuathTroublemaker : BossModule
{
    public FuathTroublemaker(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 19)) { }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.BossAdd))
            Arena.Actor(s, ArenaColor.Object);
        foreach (var s in Enemies(OID.BonusAdd_FuathTrickster))
            Arena.Actor(s, ArenaColor.Vulnerable);
    }

    public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.CalculateAIHints(slot, actor, assignment, hints);
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.BonusAdd_FuathTrickster => 3,
                OID.BossAdd => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
