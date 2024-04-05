namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.AltarKelpie;

public enum OID : uint
{
    Boss = 0x2537, //R=5.4
    Hydrosphere = 0x255B, //R=1.2
    BossHelper = 0x233C,
    BonusAdd_AltarMatanga = 0x2545, // R3.420
    BonusAdd_GoldWhisker = 0x2544, // R0.540
    AltarQueen = 0x254A, // R0,840, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    AltarGarlic = 0x2548, // R0,840, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    AltarTomato = 0x2549, // R0,840, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    AltarOnion = 0x2546, // R0,840, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    AltarEgg = 0x2547, // R0,840, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
};

public enum AID : uint
{
    AutoAttack = 870, // 2544->player, no cast, single-target
    AutoAttack2 = 872, // Boss/BonusAdd_AltarMatanga->player, no cast, single-target
    Torpedo = 13438, // Boss->player, 3,0s cast, single-target
    Innocence = 13439, // Boss->location, 3,0s cast, range 5 circle
    Gallop = 13441, // Boss->location, no cast, ???, movement ability
    RisingSeas = 13440, // Boss->self, 5,0s cast, range 50+R circle, knockback 20, away from source
    BloodyPuddle = 13443, // Hydrosphere->self, 4,0s cast, range 10+R circle
    HydroPush = 13442, // Boss->self, 6,0s cast, range 44+R width 44 rect, knockback 20, dir forward

    unknown = 9636, // BonusAdd_AltarMatanga->self, no cast, single-target
    Spin = 8599, // BonusAdd_AltarMatanga->self, no cast, range 6+R 120-degree cone
    RaucousScritch = 8598, // BonusAdd_AltarMatanga->self, 2,5s cast, range 5+R 120-degree cone
    Hurl = 5352, // BonusAdd_AltarMatanga->location, 3,0s cast, range 6 circle
    PluckAndPrune = 6449, // AltarEgg->self, 3,5s cast, range 6+R circle
    PungentPirouette = 6450, // AltarGarlic->self, 3,5s cast, range 6+R circle
    TearyTwirl = 6448, // AltarOnion->self, 3,5s cast, range 6+R circle
    Pollen = 6452, // AltarQueen->self, 3,5s cast, range 6+R circle
    HeirloomScream = 6451, // AltarTomato->self, 3,5s cast, range 6+R circle
    Telega = 9630, // BonusAdds->self, no cast, single-target, bonus adds disappear
};

class Innocence : Components.LocationTargetedAOEs
{
    public Innocence() : base(ActionID.MakeSpell(AID.Innocence), 5) { }
}

class HydroPush : Components.SelfTargetedAOEs
{
    public HydroPush() : base(ActionID.MakeSpell(AID.HydroPush), new AOEShapeRect(49.4f, 22, 5)) { }
}

class BloodyPuddle : Components.GenericAOEs
{
    private static readonly AOEShapeCircle circle = new(11.2f);
    private readonly List<Actor> _spheres = [];
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        foreach (var s in _spheres)
            yield return new(circle, s.Position, activation: _activation);
    }

    public override void OnActorCreated(BossModule module, Actor actor)
    {
        if ((OID)actor.OID == OID.Hydrosphere)
        {
            _spheres.Add(actor);
            _activation = module.WorldState.CurrentTime.AddSeconds(8.6f);
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.BloodyPuddle)
            _spheres.Clear();
    }
}

class Torpedo : Components.SingleTargetDelayableCast
{
    public Torpedo() : base(ActionID.MakeSpell(AID.Torpedo)) { }
}

class RisingSeas : Components.RaidwideCast
{
    public RisingSeas() : base(ActionID.MakeSpell(AID.RisingSeas)) { }
}

class HydroPushKB : Components.KnockbackFromCastTarget
{
    public HydroPushKB() : base(ActionID.MakeSpell(AID.HydroPush), 20, shape: new AOEShapeRect(49.4f, 22, 5), kind: Kind.DirForward)
    {
        StopAtWall = true;
    }
}

class RisingSeasKB : Components.KnockbackFromCastTarget
{
    public RisingSeasKB() : base(ActionID.MakeSpell(AID.RisingSeas), 20)
    {
        StopAtWall = true;
    }
    public override bool DestinationUnsafe(BossModule module, int slot, Actor actor, WPos pos) => module.FindComponent<BloodyPuddle>()?.ActiveAOEs(module, slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false;
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

class KelpieStates : StateMachineBuilder
{
    public KelpieStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Innocence>()
            .ActivateOnEnter<HydroPush>()
            .ActivateOnEnter<BloodyPuddle>()
            .ActivateOnEnter<Torpedo>()
            .ActivateOnEnter<RisingSeas>()
            .ActivateOnEnter<RisingSeasKB>()
            .ActivateOnEnter<HydroPushKB>()
            .ActivateOnEnter<Hurl>()
            .ActivateOnEnter<RaucousScritch>()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<PluckAndPrune>()
            .ActivateOnEnter<TearyTwirl>()
            .ActivateOnEnter<HeirloomScream>()
            .ActivateOnEnter<PungentPirouette>()
            .ActivateOnEnter<Pollen>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BonusAdd_GoldWhisker).All(e => e.IsDead) && module.Enemies(OID.BonusAdd_AltarMatanga).All(e => e.IsDead) && module.Enemies(OID.AltarEgg).All(e => e.IsDead) && module.Enemies(OID.AltarQueen).All(e => e.IsDead) && module.Enemies(OID.AltarOnion).All(e => e.IsDead) && module.Enemies(OID.AltarGarlic).All(e => e.IsDead) && module.Enemies(OID.AltarTomato).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 586, NameID = 7589)]
public class Kelpie : BossModule
{
    public Kelpie(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 20)) { }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.BonusAdd_GoldWhisker))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.BonusAdd_AltarMatanga))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.AltarEgg))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.AltarTomato))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.AltarQueen))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.AltarGarlic))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.AltarOnion))
            Arena.Actor(s, ArenaColor.Vulnerable);
    }

    public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.CalculateAIHints(slot, actor, assignment, hints);
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.AltarOnion => 7,
                OID.AltarEgg => 6,
                OID.AltarGarlic => 5,
                OID.AltarTomato => 4,
                OID.AltarQueen or OID.BonusAdd_GoldWhisker => 3,
                OID.BonusAdd_AltarMatanga => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
