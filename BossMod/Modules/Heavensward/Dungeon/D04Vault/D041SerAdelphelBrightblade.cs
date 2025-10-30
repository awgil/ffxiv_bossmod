namespace BossMod.Heavensward.Dungeon.D04Vault.D041SerAdelphelBrightblade;

public enum OID : uint
{
    Boss = 0x104E, // R0.500
    VaultOstiary = 0x104F, // R0.500, x2
    VaultDeacon = 0x1050, // R0.500, x1
    BossP2 = 0x1051, // R2.200, x1
    Brightsphere = 0x1052, // R1.000, x?
    SerAdelphel = 0xD25,
}

public enum AID : uint
{
    Fire = 966, // VaultDeacon->player, 1.0s cast, single-target
    Attack = 870, // VaultOstiary/Boss/BossP2->player, no cast, single-target
    FastBladeBoss = 717, // Boss->player, no cast, single-target
    FastBlade = 9, // VaultOstiary->player, no cast, single-target
    Bloodstain = 1099, // Boss->self, 2.5s cast, range 5 circle
    Advent = 4979, // Boss->self, no cast, single-target
    Advent2 = 4980, // SerAdelphel->self, no cast, range 80 circle
    Advent3 = 4122, // Boss->self, no cast, single-target
    HoliestOfHoly = 4126, // BossP2->self, 3.0s cast, range 80+R circle
    HeavenlySlash = 4125, // BossP2->self, no cast, range 8+R 90-degree cone
    HolyShieldBash = 4127, // BossP2->player, 4.0s cast, single-target
    SolidAscension1 = 4128, // BossP2->player, no cast, single-target
    SolidAscension2 = 4129, // SerAdelphel->player, no cast, single-target
    ShiningBlade = 4130, // BossP2->location, no cast, width 6 rect charge
    BrightFlare = 4132, // Brightsphere->self, no cast, range 5+R circle
    Execution = 4131, // BossP2->location, no cast, range 5 circle
    Retreat = 4257, // Boss->self, no cast, single-target
}

public enum IconID : uint
{
    HolyShieldBash = 16, // player
    Execution = 32, // player
}

class HoliestOfHoly(BossModule module) : Components.RaidwideCast(module, AID.HoliestOfHoly);
class HeavenlySlash(BossModule module) : Components.Cleave(module, AID.HeavenlySlash, new AOEShapeCone(10.2f, 45.Degrees()), enemyOID: (uint)OID.BossP2, activeWhileCasting: false);
class HolyShieldBash(BossModule module) : Components.SingleTargetCast(module, AID.HolyShieldBash, "Stun", damageType: AIHints.PredictedDamageType.Raidwide);
class Bloodstain(BossModule module) : Components.StandardAOEs(module, AID.Bloodstain, new AOEShapeCircle(5));
class Execution(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Execution, AID.Execution, 5, 7.1f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action == SpreadAction)
            Spreads.Clear();
    }
}
class BrightFlare(BossModule module) : Components.GenericAOEs(module, AID.BrightFlare)
{
    private readonly List<(Actor, DateTime)> _balls = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _balls.Select(b => new AOEInstance(new AOEShapeCircle(6), b.Item1.Position, Activation: b.Item2.AddSeconds(4.7f)));

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Brightsphere)
            _balls.Add((actor, WorldState.CurrentTime));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
            _balls.RemoveAll(b => b.Item1 == caster);
    }
}
class Adds(BossModule module) : Components.AddsMulti(module, [OID.VaultOstiary, OID.VaultDeacon]);
class D041SerAdelphelBrightbladeStates : StateMachineBuilder
{
    public D041SerAdelphelBrightbladeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HoliestOfHoly>()
            .ActivateOnEnter<HeavenlySlash>()
            .ActivateOnEnter<HolyShieldBash>()
            .ActivateOnEnter<Execution>()
            .ActivateOnEnter<BrightFlare>()
            .ActivateOnEnter<Bloodstain>()
            .ActivateOnEnter<Adds>();

    }
}
[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 34, NameID = 3849)]
public class D041SerAdelphelBrightblade(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -100), new ArenaBoundsCircle(19.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.BossP2), ArenaColor.Enemy);
    }
}
