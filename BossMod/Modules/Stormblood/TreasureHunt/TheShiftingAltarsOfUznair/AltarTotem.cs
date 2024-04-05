namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.AltarTotem;

public enum OID : uint
{
    Boss = 0x2534, //R=5.06
    BossAdd = 0x2566, //R=2.2
    BossHelper = 0x233C,
    FireVoidzone = 0x1EA8BB,
    BonusAdd_AltarMatanga = 0x2545, // R3.420
};

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 872, // Boss->player, no cast, single-target
    AutoAttack3 = 6499, // BossAdd->player, no cast, single-target
    FlurryOfRage = 13451, // Boss->self, 3,0s cast, range 8+R 120-degree cone
    WhorlOfFrenzy = 13453, // Boss->self, 3,0s cast, range 6+R circle
    WaveOfMalice = 13454, // Boss->location, 2,5s cast, range 5 circle
    TheWardensVerdict = 13739, // BossAdd->self, 3,0s cast, range 40+R width 4 rect
    FlamesOfFury = 13452, // Boss->location, 4,0s cast, range 10 circle

    unknown = 9636, // BonusAdd_AltarMatanga->self, no cast, single-target
    Spin = 8599, // BonusAdd_AltarMatanga->self, no cast, range 6+R 120-degree cone
    RaucousScritch = 8598, // BonusAdd_AltarMatanga->self, 2,5s cast, range 5+R 120-degree cone
    Hurl = 5352, // BonusAdd_AltarMatanga->location, 3,0s cast, range 6 circle
    Telega = 9630, // BonusAdds->self, no cast, single-target, bonus adds disappear
};

public enum IconID : uint
{
    Baitaway = 23, // player
};

class FlurryOfRage : Components.SelfTargetedAOEs
{
    public FlurryOfRage() : base(ActionID.MakeSpell(AID.FlurryOfRage), new AOEShapeCone(13.06f, 60.Degrees())) { }
}

class WaveOfMalice : Components.LocationTargetedAOEs
{
    public WaveOfMalice() : base(ActionID.MakeSpell(AID.WaveOfMalice), 5) { }
}

class WhorlOfFrenzy : Components.SelfTargetedAOEs
{
    public WhorlOfFrenzy() : base(ActionID.MakeSpell(AID.WhorlOfFrenzy), new AOEShapeCircle(11.06f)) { }
}

class TheWardensVerdict : Components.SelfTargetedAOEs
{
    public TheWardensVerdict() : base(ActionID.MakeSpell(AID.TheWardensVerdict), new AOEShapeRect(45.06f, 2)) { }
}

class FlamesOfFury : Components.LocationTargetedAOEs
{
    public FlamesOfFury() : base(ActionID.MakeSpell(AID.FlamesOfFury), 10) { }
}

class FlamesOfFuryBait : Components.GenericBaitAway
{
    private bool targeted;
    private Actor? target;

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.Baitaway)
        {
            CurrentBaits.Add(new(actor, actor, new AOEShapeCircle(10)));
            targeted = true;
            target = actor;
        }
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.FlamesOfFury)
            ++NumCasts;
        if (NumCasts == 3)
        {
            CurrentBaits.Clear();
            NumCasts = 0;
            targeted = false;
        }
    }

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(module, slot, actor, assignment, hints);
        if (target == actor && targeted)
            hints.AddForbiddenZone(ShapeDistance.Circle(module.Bounds.Center, 18));
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (target == actor && targeted)
            hints.Add("Bait voidzone away! (3 times)");
    }
}

class FlamesOfFuryVoidzone : Components.PersistentVoidzone
{
    public FlamesOfFuryVoidzone() : base(10, m => m.Enemies(OID.FireVoidzone).Where(z => z.EventState != 7)) { }
}

class RaucousScritch : Components.SelfTargetedAOEs
{
    public RaucousScritch() : base(ActionID.MakeSpell(AID.RaucousScritch), new AOEShapeCone(8.42f, 30.Degrees())) { }
}

class Hurl : Components.LocationTargetedAOEs
{
    public Hurl() : base(ActionID.MakeSpell(AID.Hurl), 6) { }
}

class Spin : Components.Cleave
{
    public Spin() : base(ActionID.MakeSpell(AID.Spin), new AOEShapeCone(9.42f, 60.Degrees()), (uint)OID.BonusAdd_AltarMatanga) { }
}

class TotemStates : StateMachineBuilder
{
    public TotemStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FlurryOfRage>()
            .ActivateOnEnter<WaveOfMalice>()
            .ActivateOnEnter<WhorlOfFrenzy>()
            .ActivateOnEnter<TheWardensVerdict>()
            .ActivateOnEnter<FlamesOfFury>()
            .ActivateOnEnter<FlamesOfFuryBait>()
            .ActivateOnEnter<FlamesOfFuryVoidzone>()
            .ActivateOnEnter<Hurl>()
            .ActivateOnEnter<RaucousScritch>()
            .ActivateOnEnter<Spin>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BossAdd).All(e => e.IsDead) && module.Enemies(OID.BonusAdd_AltarMatanga).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 586, NameID = 7586)]
public class Totem : BossModule
{
    public Totem(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 20)) { }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.BossAdd))
            Arena.Actor(s, ArenaColor.Object);
        foreach (var s in Enemies(OID.BonusAdd_AltarMatanga))
            Arena.Actor(s, ArenaColor.Vulnerable);
    }

    public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.CalculateAIHints(slot, actor, assignment, hints);
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.BonusAdd_AltarMatanga => 3,
                OID.BossAdd => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
