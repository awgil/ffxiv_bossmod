namespace BossMod.Shadowbringers.Alliance.A32HanselAndGretel;

public enum OID : uint
{
    Helper = 0x233C, // R0.500, x11 (spawn during fight), Helper type
    _Gen_HanselGretel = 0x18D6, // R0.500, x3
    _Gen_ = 0x32D4, // R0.500, x1
    Gretel = 0x31A4, // R7.000, x1
    Hansel = 0x31A5, // R7.000, x1
    _Gen_MagicBullet = 0x31A7, // R1.000, x0 (spawn during fight)
    _Gen_MagicalConfluence = 0x31A6, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_Attack = 870, // Hansel/Gretel->player, no cast, single-target
    _Weaponskill_UpgradedLance1 = 23656, // Gretel->self, 4.0s cast, single-target
    _Weaponskill_UpgradedLance = 23658, // Hansel->self, 4.0s cast, single-target
    _Weaponskill_UpgradedShield = 23657, // Gretel->self, 4.0s cast, single-target
    _Weaponskill_UpgradedShield1 = 23659, // Hansel->self, 4.0s cast, single-target
    _Weaponskill_Repay = 23664, // Helper->player, no cast, single-target
    _Weaponskill_Wail = 23671, // Hansel->self, 5.0s cast, range 50 circle
    _Weaponskill_CripplingBlow = 23672, // Gretel->player, 5.0s cast, single-target
    _Weaponskill_Wail1 = 23670, // Gretel->self, 5.0s cast, range 50 circle
    _Weaponskill_CripplingBlow1 = 23673, // Hansel->player, 5.0s cast, single-target
    _Weaponskill_TandemAssaultBloodySweep = 25017, // Hansel->self, 5.0s cast, single-target
    _Weaponskill_TandemAssaultBloodySweep1 = 25016, // Gretel->self, 5.0s cast, single-target
    _Weaponskill_ = 23679, // Hansel->location, no cast, single-target
    _Weaponskill_1 = 23678, // Gretel->location, no cast, single-target
    _Weaponskill_BloodySweep = 23637, // Hansel->self, 8.0s cast, single-target
    _Weaponskill_BloodySweep1 = 23636, // Gretel->self, 8.0s cast, single-target
    _Weaponskill_BloodySweep2 = 23660, // Helper->self, 8.6s cast, range 50 width 25 rect
    _Weaponskill_BloodySweep3 = 23661, // Helper->self, 8.6s cast, range 50 width 25 rect
    _Weaponskill_RiotOfMagic = 23675, // Hansel->self, 5.0s cast, single-target
    _Weaponskill_SeedOfMagicAlpha = 23674, // Gretel->self, 5.0s cast, single-target
    _Weaponskill_SeedOfMagicAlpha1 = 23649, // Helper->players, 5.0s cast, range 5 circle
    _Weaponskill_RiotOfMagic1 = 23651, // Helper->players, 5.0s cast, range 5 circle
    _Weaponskill_TandemAssaultPassingLance = 25021, // Hansel->self, 5.0s cast, single-target
    _Weaponskill_TandemAssaultPassingLance1 = 25020, // Gretel->self, 5.0s cast, single-target
    _Weaponskill_PassingLance = 23653, // Hansel->self, 8.0s cast, single-target
    _Weaponskill_PassingLance1 = 23652, // Gretel->self, 8.0s cast, single-target
    _Weaponskill_PassingLance2 = 23654, // _Gen_HanselGretel->self, 8.4s cast, range 50 width 24 rect
    _Weaponskill_Explosion = 23655, // _Gen_MagicBullet->self, 1.0s cast, range 4 width 50 rect
    _Weaponskill_Tandem = 23641, // Hansel->self, no cast, single-target
    _Weaponskill_Tandem1 = 23640, // Gretel->self, no cast, single-target
    _Weaponskill_Transference = 23794, // Helper->location, no cast, single-target
    _Weaponskill_Transference1 = 23793, // Helper->location, no cast, single-target
    _Weaponskill_BloodySweep4 = 23639, // Hansel->self, 13.0s cast, single-target
    _Weaponskill_BloodySweep5 = 23638, // Gretel->self, 13.0s cast, single-target
    _Weaponskill_BloodySweep6 = 23662, // Helper->self, 13.6s cast, range 50 width 25 rect
    _Weaponskill_BloodySweep7 = 23663, // Helper->self, 13.6s cast, range 50 width 25 rect
    _Weaponskill_WanderingTrail = 23643, // Hansel->self, 5.0s cast, single-target
    _Weaponskill_WanderingTrail1 = 23642, // Gretel->self, 5.0s cast, single-target
    _Weaponskill_TandemAssaultBreakthrough = 25019, // Hansel->self, 5.0s cast, single-target
    _Weaponskill_TandemAssaultBreakthrough1 = 25018, // Gretel->self, 5.0s cast, single-target
    _Weaponskill_Breakthrough = 21939, // _Gen_HanselGretel->self, 9.0s cast, range 53 width 32 rect
    _Weaponskill_Breakthrough1 = 23646, // Hansel->location, 9.0s cast, single-target
    _Weaponskill_Breakthrough2 = 23645, // Gretel->location, 9.0s cast, single-target
    _Weaponskill_UnevenFooting = 23647, // _Gen_HanselGretel->self, 9.3s cast, range 50 circle
    _Weaponskill_Impact = 23644, // Helper->self, no cast, range 3 circle
    _Weaponskill_HungryLance = 23666, // Hansel->self, 5.0s cast, range 40 120-degree cone
    _Weaponskill_HungryLance1 = 23665, // Gretel->self, 5.0s cast, range 40 120-degree cone
    _Weaponskill_SeedOfMagicBeta = 23677, // Hansel->self, 5.0s cast, single-target
    _Weaponskill_SeedOfMagicBeta1 = 23676, // Gretel->self, 5.0s cast, single-target
    _Weaponskill_SeedOfMagicBeta2 = 23669, // Helper->location, 5.0s cast, range 5 circle
    _Weaponskill_Lamentation = 23668, // Hansel->self, 8.0s cast, range 50 circle
    _Weaponskill_Lamentation1 = 23667, // Gretel->self, 8.0s cast, range 50 circle
}

public enum SID : uint
{
    _Gen_Jog = 4209, // none->player, extra=0x14
    _Gen_StrongOfSpear = 2537, // none->Hansel/Gretel, extra=0x0
    _Gen_ = 2056, // none->Hansel/Gretel, extra=0x123/0x124/0x125/0x122
    _Gen_StrongOfShield = 2538, // none->Gretel/Hansel, extra=0x0
    _Gen_DirectionalParry = 680, // none->Gretel/Hansel, extra=0xE
    _Gen_VulnerabilityUp = 1789, // Helper/_Gen_HanselGretel/Hansel->player, extra=0x1
    _Gen_Weakness = 43, // none->player, extra=0x0
    _Gen_Transcendent = 418, // none->player, extra=0x0
    _Gen_BrinkOfDeath = 44, // none->player, extra=0x0
}

public enum IconID : uint
{
    _Gen_Icon_tank_lockon02k1 = 218, // player->self
    _Gen_Icon_loc05sp_05af = 96, // player->self
    _Gen_Icon_com_share0c = 62, // player->self
}

public enum TetherID : uint
{
    _Gen_Tether_chn_pchange_t0a1 = 151, // Gretel->Hansel
}

abstract class UpgradedShield(BossModule module, OID oid, AID cast) : Components.DirectionalParry(module, (uint)oid)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == cast)
            PredictParrySide(caster.InstanceID, Side.All ^ Side.Front);
    }
}

class UpgradedShield1(BossModule module) : UpgradedShield(module, OID.Gretel, AID._Weaponskill_UpgradedShield);
class UpgradedShield2(BossModule module) : UpgradedShield(module, OID.Hansel, AID._Weaponskill_UpgradedShield1);

class Wail1(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_Wail1);
class Wail2(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_Wail);

class CripplingBlow1(BossModule module) : Components.SingleTargetCast(module, AID._Weaponskill_CripplingBlow);
class CripplingBlow2(BossModule module) : Components.SingleTargetCast(module, AID._Weaponskill_CripplingBlow1);

class BloodySweep(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_BloodySweep2, AID._Weaponskill_BloodySweep3], new AOEShapeRect(50, 12.5f));

class SeedOfRiotOfMagic(BossModule module) : Components.CastStackSpread(module, AID._Weaponskill_RiotOfMagic1, AID._Weaponskill_SeedOfMagicAlpha1, 5, 5, alwaysShowSpreads: true);

class PassingLance(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_PassingLance2, new AOEShapeRect(50, 12));

class MagicBulletExplosion(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_Explosion)
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
        if ((AID)spell.Action.ID == AID._Weaponskill_PassingLance2)
            _activation = Module.CastFinishAt(spell, 1);
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID._Gen_MagicBullet)
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

class BloodySweep2(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_BloodySweep6, AID._Weaponskill_BloodySweep7], new AOEShapeRect(50, 12.5f));

class MagicalConfluence(BossModule module) : Components.PersistentVoidzone(module, 4, m => m.Enemies(OID._Gen_MagicalConfluence).Where(e => e.EventState != 7), 8);
class Breakthrough(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Breakthrough, new AOEShapeRect(53, 16));
// estimate of falloff
class UnevenFooting(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_UnevenFooting, new AOEShapeCircle(20));
class HungryLance(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_HungryLance, AID._Weaponskill_HungryLance1], new AOEShapeCone(40, 60.Degrees()));
class SeedOfMagicBeta(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_SeedOfMagicBeta2, 5);
class Lamentation1(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_Lamentation);
class Lamentation2(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_Lamentation1);

class A32HanselAndGretelStates : StateMachineBuilder
{
    public A32HanselAndGretelStates(A32HanselAndGretel module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<UpgradedShield1>()
            .ActivateOnEnter<UpgradedShield2>()
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

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 779, NameID = 9989, PrimaryActorOID = (uint)OID.Gretel)]
public class A32HanselAndGretel(WorldState ws, Actor primary) : BossModule(ws, primary, new(-800, -951), new ArenaBoundsCircle(24.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.Hansel), ArenaColor.Enemy);
    }
}
