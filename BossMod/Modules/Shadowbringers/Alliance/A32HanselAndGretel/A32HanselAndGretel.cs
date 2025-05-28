namespace BossMod.Shadowbringers.Alliance.A32HanselAndGretel;

public enum OID : uint
{
    Helper1 = 0x233C, // R0.500, x11 (spawn during fight), Helper type
    Helper2 = 0x18D6, // R0.500, x3
    Unk = 0x32D4, // R0.500, x1
    Gretel = 0x31A4, // R7.000, x1
    Hansel = 0x31A5, // R7.000, x1
    MagicBullet = 0x31A7, // R1.000, x0 (spawn during fight)
    MagicalConfluence = 0x31A6, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 870, // Hansel/Gretel->player, no cast, single-target
    UpgradedLance1 = 23656, // Gretel->self, 4.0s cast, single-target
    UpgradedLance2 = 23658, // Hansel->self, 4.0s cast, single-target
    UpgradedShield1 = 23657, // Gretel->self, 4.0s cast, single-target
    UpgradedShield2 = 23659, // Hansel->self, 4.0s cast, single-target
    Repay = 23664, // Helper->player, no cast, single-target, retaliation damage when attacking directional parry
    Wail1 = 23670, // Gretel->self, 5.0s cast, range 50 circle
    Wail2 = 23671, // Hansel->self, 5.0s cast, range 50 circle
    CripplingBlow1 = 23672, // Gretel->player, 5.0s cast, single-target
    CripplingBlow2 = 23673, // Hansel->player, 5.0s cast, single-target
    TandemAssaultBloodySweep1 = 25016, // Gretel->self, 5.0s cast, single-target
    TandemAssaultBloodySweep2 = 25017, // Hansel->self, 5.0s cast, single-target
    Dash1 = 23678, // Gretel->location, no cast, single-target
    Dash2 = 23679, // Hansel->location, no cast, single-target
    BloodySweepCast1 = 23636, // Gretel->self, 8.0s cast, single-target
    BloodySweepCast2 = 23637, // Hansel->self, 8.0s cast, single-target
    BloodySweep1 = 23660, // Helper->self, 8.6s cast, range 50 width 25 rect
    BloodySweep2 = 23661, // Helper->self, 8.6s cast, range 50 width 25 rect
    RiotOfMagicCast = 23675, // Hansel->self, 5.0s cast, single-target
    RiotOfMagic = 23651, // Helper->players, 5.0s cast, range 5 circle
    SeedOfMagicAlphaCast = 23674, // Gretel->self, 5.0s cast, single-target
    SeedOfMagicAlpha = 23649, // Helper->players, 5.0s cast, range 5 circle
    TandemAssaultPassingLance1 = 25020, // Gretel->self, 5.0s cast, single-target
    TandemAssaultPassingLance2 = 25021, // Hansel->self, 5.0s cast, single-target
    PassingLanceCast1 = 23652, // Gretel->self, 8.0s cast, single-target
    PassingLanceCast2 = 23653, // Hansel->self, 8.0s cast, single-target
    PassingLance = 23654, // _Gen_HanselGretel->self, 8.4s cast, range 50 width 24 rect
    Explosion = 23655, // _Gen_MagicBullet->self, 1.0s cast, range 4 width 50 rect
    Tandem1 = 23640, // Gretel->self, no cast, single-target
    Tandem2 = 23641, // Hansel->self, no cast, single-target
    Transference1 = 23793, // Helper->location, no cast, single-target
    Transference2 = 23794, // Helper->location, no cast, single-target
    BloodySweepSlowCast1 = 23638, // Gretel->self, 13.0s cast, single-target
    BloodySweepSlowCast2 = 23639, // Hansel->self, 13.0s cast, single-target
    BloodySweepSlow1 = 23662, // Helper->self, 13.6s cast, range 50 width 25 rect
    BloodySweepSlow2 = 23663, // Helper->self, 13.6s cast, range 50 width 25 rect
    WanderingTrailCast1 = 23642, // Gretel->self, 5.0s cast, single-target
    WanderingTrailCast2 = 23643, // Hansel->self, 5.0s cast, single-target
    TandemAssaultBreakthroughCast1 = 25018, // Gretel->self, 5.0s cast, single-target
    TandemAssaultBreakthroughCast2 = 25019, // Hansel->self, 5.0s cast, single-target
    BreakthroughCast1 = 23645, // Gretel->location, 9.0s cast, single-target
    BreakthroughCast2 = 23646, // Hansel->location, 9.0s cast, single-target
    Breakthrough = 21939, // _Gen_HanselGretel->self, 9.0s cast, range 53 width 32 rect
    UnevenFooting = 23647, // _Gen_HanselGretel->self, 9.3s cast, range 50 circle
    Impact = 23644, // Helper->self, no cast, range 3 circle
    HungryLance1 = 23665, // Gretel->self, 5.0s cast, range 40 120-degree cone
    HungryLance2 = 23666, // Hansel->self, 5.0s cast, range 40 120-degree cone
    SeedOfMagicBetaCast1 = 23676, // Gretel->self, 5.0s cast, single-target
    SeedOfMagicBetaCast2 = 23677, // Hansel->self, 5.0s cast, single-target
    SeedOfMagicBeta = 23669, // Helper->location, 5.0s cast, range 5 circle
    Lamentation1 = 23667, // Gretel->self, 8.0s cast, range 50 circle
    Lamentation2 = 23668, // Hansel->self, 8.0s cast, range 50 circle
}

public enum SID : uint
{
    StrongOfSpear = 2537, // none->Hansel/Gretel, extra=0x0
    StrongOfShield = 2538, // none->Gretel/Hansel, extra=0x0
    DirectionalParry = 680, // none->Gretel/Hansel, extra=0xE
}

public enum IconID : uint
{
    Tankbuster = 218, // player->self
    Spread = 96, // player->self
    Share = 62, // player->self
}

public enum TetherID : uint
{
    StrongerTogether = 1, // Hansel->Gretel
    Swap = 151, // Gretel->Hansel
}

abstract class UpgradedShield(BossModule module, OID oid, AID cast) : Components.DirectionalParry(module, (uint)oid)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == cast)
            PredictParrySide(caster.InstanceID, Side.All ^ Side.Front);
    }
}

class UpgradedShield1(BossModule module) : UpgradedShield(module, OID.Gretel, AID.UpgradedShield1);
class UpgradedShield2(BossModule module) : UpgradedShield(module, OID.Hansel, AID.UpgradedShield2);

class Wail1(BossModule module) : Components.RaidwideCast(module, AID.Wail1);
class Wail2(BossModule module) : Components.RaidwideCast(module, AID.Wail2);

class CripplingBlow1(BossModule module) : Components.SingleTargetCast(module, AID.CripplingBlow1);
class CripplingBlow2(BossModule module) : Components.SingleTargetCast(module, AID.CripplingBlow2);

class BloodySweep(BossModule module) : Components.GroupedAOEs(module, [AID.BloodySweep1, AID.BloodySweep2], new AOEShapeRect(50, 12.5f));

class SeedOfRiotOfMagic(BossModule module) : Components.CastStackSpread(module, AID.RiotOfMagic, AID.SeedOfMagicAlpha, 5, 5, alwaysShowSpreads: true);

class PassingLance(BossModule module) : Components.StandardAOEs(module, AID.PassingLance, new AOEShapeRect(50, 12));

class MagicBulletExplosion(BossModule module) : Components.GenericAOEs(module, AID.Explosion)
{
    private readonly List<Actor> _bullets = [];
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
            foreach (var bullet in _bullets)
                yield return new AOEInstance(new AOEShapeRect(2, 25, 2), bullet.Position, bullet.Rotation, _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.PassingLance)
            _activation = Module.CastFinishAt(spell, 1);
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.MagicBullet)
            _bullets.Add(actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _bullets.Remove(caster);
            if (_bullets.Count == 0)
                _activation = default;
        }
    }
}

class BloodySweep2(BossModule module) : Components.GroupedAOEs(module, [AID.BloodySweepSlow1, AID.BloodySweepSlow2], new AOEShapeRect(50, 12.5f));

class MagicalConfluence(BossModule module) : Components.PersistentVoidzone(module, 4, m => m.Enemies(OID.MagicalConfluence).Where(e => e.EventState != 7), 8);
class Breakthrough(BossModule module) : Components.StandardAOEs(module, AID.Breakthrough, new AOEShapeRect(53, 16));
// estimate of falloff
class UnevenFooting(BossModule module) : Components.StandardAOEs(module, AID.UnevenFooting, new AOEShapeCircle(22));
class HungryLance(BossModule module) : Components.GroupedAOEs(module, [AID.HungryLance2, AID.HungryLance1], new AOEShapeCone(40, 60.Degrees()));
class SeedOfMagicBeta(BossModule module) : Components.StandardAOEs(module, AID.SeedOfMagicBeta, 5);
class Lamentation1(BossModule module) : Components.RaidwideCast(module, AID.Lamentation2);
class Lamentation2(BossModule module) : Components.RaidwideCast(module, AID.Lamentation1);

class StrongerTogether(BossModule module) : BossComponent(module)
{
    private Actor? _boss1;
    private Actor? _boss2;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.StrongerTogether && (OID)source.OID is OID.Hansel or OID.Gretel)
        {
            _boss1 = source;
            _boss2 = WorldState.Actors.Find(tether.Target);
        }
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.StrongerTogether)
        {
            _boss1 = _boss2 = null;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_boss1?.TargetID == actor.InstanceID || _boss2?.TargetID == actor.InstanceID)
            hints.Add("Separate bosses!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_boss1 is { } b1 && _boss2 is { } b2)
            Arena.AddLine(b1.Position, b2.Position, ArenaColor.Danger);
    }
}

class A32HanselAndGretelStates : StateMachineBuilder
{
    public A32HanselAndGretelStates(A32HanselAndGretel module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<UpgradedShield1>()
            .ActivateOnEnter<UpgradedShield2>()
            .ActivateOnEnter<StrongerTogether>()
            .ActivateOnEnter<Wail1>()
            .ActivateOnEnter<Wail2>()
            .ActivateOnEnter<CripplingBlow1>()
            .ActivateOnEnter<CripplingBlow2>()
            .ActivateOnEnter<BloodySweep>()
            .ActivateOnEnter<SeedOfRiotOfMagic>()
            .ActivateOnEnter<PassingLance>()
            .ActivateOnEnter<MagicBulletExplosion>()
            .ActivateOnEnter<BloodySweep2>()
            .ActivateOnEnter<MagicalConfluence>()
            .ActivateOnEnter<Breakthrough>()
            .ActivateOnEnter<UnevenFooting>()
            .ActivateOnEnter<HungryLance>()
            .ActivateOnEnter<SeedOfMagicBeta>()
            .ActivateOnEnter<Lamentation1>()
            .ActivateOnEnter<Lamentation2>()
            .Raw.Update = () => module.PrimaryActor.IsDeadOrDestroyed && module.Enemies(OID.Hansel).All(e => e.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 779, NameID = 9989, PrimaryActorOID = (uint)OID.Gretel)]
public class A32HanselAndGretel(WorldState ws, Actor primary) : BossModule(ws, primary, new(-800, -951), new ArenaBoundsCircle(24.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.Hansel), ArenaColor.Enemy);
    }
}
