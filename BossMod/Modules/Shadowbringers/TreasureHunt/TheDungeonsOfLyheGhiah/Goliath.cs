namespace BossMod.Shadowbringers.TreasureHunt.DungeonsOfLyheGhiah.Goliath;

public enum OID : uint
{
    Boss = 0x2BA5, //R=5.25
    BossAdd = 0x2BA6, //R=2.1
    BossHelper = 0x233C,
    DungeonQueen = 0x2A0A, // R0,840, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    DungeonGarlic = 0x2A08, // R0,840, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    DungeonTomato = 0x2A09, // R0,840, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    DungeonOnion = 0x2A06, // R0,840, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    DungeonEgg = 0x2A07, // R0,840, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    BonusAdd_TheKeeperOfTheKeys = 0x2A05, // R3.230
};

public enum AID : uint
{
    AutoAttack = 872, // Boss/BossAdd->player, no cast, single-target
    MechanicalBlow = 17873, // Boss->player, 5,0s cast, single-target
    Wellbore = 17874, // Boss->location, 7,0s cast, range 15 circle
    Fount = 17875, // BossHelper->location, 3,0s cast, range 4 circle
    Incinerate = 17876, // Boss->self, 5,0s cast, range 100 circle
    Accelerate = 17877, // Boss->players, 5,0s cast, range 6 circle
    Compress = 17879, // Boss->self, 2,5s cast, range 100 width 7 cross
    Compress2 = 17878, // BossAdd->self, 2,5s cast, range 100+R width 7 rect

    Pollen = 6452, // 2A0A->self, 3,5s cast, range 6+R circle
    TearyTwirl = 6448, // 2A06->self, 3,5s cast, range 6+R circle
    HeirloomScream = 6451, // 2A09->self, 3,5s cast, range 6+R circle
    PluckAndPrune = 6449, // 2A07->self, 3,5s cast, range 6+R circle
    PungentPirouette = 6450, // 2A08->self, 3,5s cast, range 6+R circle
    Telega = 9630, // BonusAdds->self, no cast, single-target, bonus adds disappear
    Mash = 17852, // 2A05->self, 2,5s cast, range 12+R width 4 rect
    Scoop = 17853, // 2A05->self, 4,0s cast, range 15 120-degree cone
    Inhale = 17855, // 2A05->self, no cast, range 20 120-degree cone, attract 25 between hitboxes, shortly before Spin
    Spin = 17854, // 2A05->self, 2,5s cast, range 11 circle
};

class Wellbore : Components.SelfTargetedAOEs
{
    public Wellbore() : base(ActionID.MakeSpell(AID.Wellbore), new AOEShapeCircle(15)) { }
}

class Compress : Components.SelfTargetedAOEs
{
    public Compress() : base(ActionID.MakeSpell(AID.Compress), new AOEShapeCross(100, 3.5f)) { }
}

class Compress2 : Components.SelfTargetedAOEs
{
    public Compress2() : base(ActionID.MakeSpell(AID.Compress2), new AOEShapeRect(102.1f, 3.5f)) { }
}

class Accelerate : Components.StackWithCastTargets
{
    public Accelerate() : base(ActionID.MakeSpell(AID.Accelerate), 6) { }
}

class Incinerate : Components.RaidwideCast
{
    public Incinerate() : base(ActionID.MakeSpell(AID.Incinerate)) { }
}

class Fount : Components.LocationTargetedAOEs
{
    public Fount() : base(ActionID.MakeSpell(AID.Fount), 4) { }
}

class MechanicalBlow : Components.SingleTargetCast
{
    public MechanicalBlow() : base(ActionID.MakeSpell(AID.MechanicalBlow)) { }
}

class PluckAndPrune : Components.SelfTargetedAOEs
{
    public PluckAndPrune() : base(ActionID.MakeSpell(AID.PluckAndPrune), new AOEShapeCircle(6.84f)) { }
}

class TearyTwirl : Components.SelfTargetedAOEs
{
    public TearyTwirl() : base(ActionID.MakeSpell(AID.TearyTwirl), new AOEShapeCircle(6.84f)) { }
}

class HeirloomScream : Components.SelfTargetedAOEs
{
    public HeirloomScream() : base(ActionID.MakeSpell(AID.HeirloomScream), new AOEShapeCircle(6.84f)) { }
}

class PungentPirouette : Components.SelfTargetedAOEs
{
    public PungentPirouette() : base(ActionID.MakeSpell(AID.PungentPirouette), new AOEShapeCircle(6.84f)) { }
}

class Pollen : Components.SelfTargetedAOEs
{
    public Pollen() : base(ActionID.MakeSpell(AID.Pollen), new AOEShapeCircle(6.84f)) { }
}

class Spin : Components.SelfTargetedAOEs
{
    public Spin() : base(ActionID.MakeSpell(AID.Spin), new AOEShapeCircle(11)) { }
}

class Mash : Components.SelfTargetedAOEs
{
    public Mash() : base(ActionID.MakeSpell(AID.Mash), new AOEShapeRect(15.23f, 2)) { }
}

class Scoop : Components.SelfTargetedAOEs
{
    public Scoop() : base(ActionID.MakeSpell(AID.Scoop), new AOEShapeCone(15, 60.Degrees())) { }
}

class GoliathStates : StateMachineBuilder
{
    public GoliathStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Wellbore>()
            .ActivateOnEnter<Compress>()
            .ActivateOnEnter<Compress2>()
            .ActivateOnEnter<Accelerate>()
            .ActivateOnEnter<Incinerate>()
            .ActivateOnEnter<MechanicalBlow>()
            .ActivateOnEnter<PluckAndPrune>()
            .ActivateOnEnter<TearyTwirl>()
            .ActivateOnEnter<HeirloomScream>()
            .ActivateOnEnter<PungentPirouette>()
            .ActivateOnEnter<Pollen>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BossAdd).All(e => e.IsDead) && module.Enemies(OID.DungeonEgg).All(e => e.IsDead) && module.Enemies(OID.DungeonQueen).All(e => e.IsDead) && module.Enemies(OID.DungeonOnion).All(e => e.IsDead) && module.Enemies(OID.DungeonGarlic).All(e => e.IsDead) && module.Enemies(OID.DungeonTomato).All(e => e.IsDead) && module.Enemies(OID.BonusAdd_TheKeeperOfTheKeys).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 688, NameID = 8953)]
public class Goliath : BossModule
{
    public Goliath(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(0, -390), 20)) { }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.BossAdd))
            Arena.Actor(s, ArenaColor.Object);
        foreach (var s in Enemies(OID.DungeonEgg))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.DungeonTomato))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.DungeonQueen))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.DungeonGarlic))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.DungeonOnion))
            Arena.Actor(s, ArenaColor.Vulnerable);
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
                OID.DungeonOnion => 7,
                OID.DungeonEgg => 6,
                OID.DungeonGarlic => 5,
                OID.DungeonTomato => 4,
                OID.DungeonQueen or OID.BonusAdd_TheKeeperOfTheKeys => 3,
                OID.BossAdd => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
