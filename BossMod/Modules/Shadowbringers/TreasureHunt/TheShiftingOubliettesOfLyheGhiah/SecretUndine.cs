namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.SecretUndine;

public enum OID : uint
{
    Boss = 0x3011, //R=3.60
    BossAdd = 0x3013, //R=1.12
    Bubble = 0x3012, //R=1.3, untargetable 
    BossHelper = 0x233C,
    BonusAdd_TheKeeperOfTheKeys = 0x3034, // R3.230
};

public enum AID : uint
{
    AutoAttack = 23186, // Boss/3013->player, no cast, single-target
    AutoAttack2 = 872, // BonusAdd_TheKeeperOfTheKeys->player, no cast, single-target
    Hydrowhirl = 21658, // Boss->self, 3,0s cast, range 8 circle
    Hypnowave = 21659, // Boss->self, 3,0s cast, range 30 120-degree cone, causes sleep
    Hydrotaph = 21661, // Boss->self, 4,0s cast, single-target
    Hydrotaph2 = 21662, // BossHelper->self, 4,0s cast, range 40 circle
    Hydrofan = 21663, // 3012->self, 5,0s cast, range 44 30-degree cone
    Hydropins = 21660, // Boss->self, 2,5s cast, range 12 width 4 rect
    AquaGlobe = 21664, // 3013->location, 3,0s cast, range 8 circle

    Telega = 9630, // BonusAdds->self, no cast, single-target, bonus adds disappear
    Mash = 21767, // 3034->self, 3,0s cast, range 13 width 4 rect
    Inhale = 21770, // 3034->self, no cast, range 20 120-degree cone, attract 25 between hitboxes, shortly before Spin
    Spin = 21769, // 3034->self, 4,0s cast, range 11 circle
    Scoop = 21768, // 3034->self, 4,0s cast, range 15 120-degree cone
};

class Hydrofan : Components.SelfTargetedAOEs
{
    public Hydrofan() : base(ActionID.MakeSpell(AID.Hydrofan), new AOEShapeCone(44, 15.Degrees())) { }
}

class Hypnowave : Components.SelfTargetedAOEs
{
    public Hypnowave() : base(ActionID.MakeSpell(AID.Hypnowave), new AOEShapeCone(30, 60.Degrees())) { }
}

class Hydropins : Components.SelfTargetedAOEs
{
    public Hydropins() : base(ActionID.MakeSpell(AID.Hydropins), new AOEShapeRect(12, 2)) { }
}

class AquaGlobe : Components.LocationTargetedAOEs
{
    public AquaGlobe() : base(ActionID.MakeSpell(AID.AquaGlobe), 8) { }
}

class Hydrowhirl : Components.SelfTargetedAOEs
{
    public Hydrowhirl() : base(ActionID.MakeSpell(AID.Hydrowhirl), new AOEShapeCircle(8)) { }
}

class Hydrotaph : Components.RaidwideCast
{
    public Hydrotaph() : base(ActionID.MakeSpell(AID.Hydrotaph2)) { }
}

class Spin : Components.SelfTargetedAOEs
{
    public Spin() : base(ActionID.MakeSpell(AID.Spin), new AOEShapeCircle(11)) { }
}

class Mash : Components.SelfTargetedAOEs
{
    public Mash() : base(ActionID.MakeSpell(AID.Mash), new AOEShapeRect(13, 2)) { }
}

class Scoop : Components.SelfTargetedAOEs
{
    public Scoop() : base(ActionID.MakeSpell(AID.Scoop), new AOEShapeCone(15, 60.Degrees())) { }
}

class UndineStates : StateMachineBuilder
{
    public UndineStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Hydrofan>()
            .ActivateOnEnter<Hypnowave>()
            .ActivateOnEnter<Hydropins>()
            .ActivateOnEnter<AquaGlobe>()
            .ActivateOnEnter<Hydrowhirl>()
            .ActivateOnEnter<Hydrotaph>()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<Mash>()
            .ActivateOnEnter<Scoop>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BossAdd).All(e => e.IsDead) && module.Enemies(OID.BonusAdd_TheKeeperOfTheKeys).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 745, NameID = 9790)]
public class Undine : BossModule
{
    public Undine(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 19)) { }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.BossAdd))
            Arena.Actor(s, ArenaColor.Object);
        foreach (var s in Enemies(OID.BonusAdd_TheKeeperOfTheKeys))
            Arena.Actor(s, ArenaColor.Vulnerable);
    }

    public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.CalculateAIHints(slot, actor, assignment, hints);
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.BonusAdd_TheKeeperOfTheKeys => 3,
                OID.BossAdd => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
