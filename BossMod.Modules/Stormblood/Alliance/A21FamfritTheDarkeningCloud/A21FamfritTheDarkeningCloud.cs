namespace BossMod.Stormblood.Alliance.A21FamfritTheDarkeningCloud;

public enum OID : uint
{
    Boss = 0x225A, // R6.480, x1
    Helper = 0x18D6, // R0.500, x24 (spawn during fight), Helper type
    DarkEwer = 0x225B, // R2.400, x0 (spawn during fight)
    DarkRain = 0x225C, // R2.200, x0 (spawn during fight)

    DarkeningDeluge = 0x1EA7D5,
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    TidePod = 11326, // Boss->player, 4.0s cast, single-target
    WaterIV = 11325, // Boss->self, 4.0s cast, range 80 circle
    BrinyCannonadeCast = 11329, // Boss->self, 5.0s cast, single-target
    BrinyCannonadeSpread = 11333, // 18D6->players, no cast, range 6 circle
    BossTeleport = 11335, // Boss->location, no cast, ???
    TsunamiCastBackward = 11336, // Boss->self, 3.0s cast, single-target
    TsunamiCastCCW = 11337, // Boss->self, 3.0s cast, single-target
    TsunamiCastForward = 11338, // Boss->self, 3.0s cast, single-target
    TsunamiCastCW = 11339, // Boss->self, 3.0s cast, single-target
    TsunamiRaidwideKB = 11344, // 18D6->self, 5.0s cast, range 40 circle
    TsunamiHitKB = 11345, // 18D6->self, no cast, range 36+R ?-degree cone
    TsunamiHit = 11346, // 18D6->self, no cast, range 36+R ?-degree cone
    TsunamiInstantBackward = 11340, // Boss->self, no cast, single-target
    TsunamiInstantCCW = 11341, // Boss->self, no cast, single-target
    TsunamiInstantForward = 11342, // Boss->self, no cast, single-target
    TsunamiInstantCW = 11343, // Boss->self, no cast, single-target
    DarkRainPuddle = 11334, // 18D6->self, 3.5s cast, range 8 circle
    DarkEwer = 11331, // Boss->self, 3.0s cast, single-target
    Jet = 11348, // 18D6->self, no cast, range 6 circle
    Materialize = 11347, // 225B->self, 3.0s cast, range 6 circle
    DarkRainCast = 11328, // Boss->self, 4.0s cast, single-target
    DarkeningRainfall = 11327, // Boss->self, 3.0s cast, single-target
    DarkeningDeluge = 11350, // 18D6->location, 3.0s cast, range 8 circle
    DarkCannonadeCast = 11330, // Boss->self, 6.0s cast, single-target
    DarkCannonadeStack = 11332, // 18D6->player, no cast, single-target
    Explosion = 11349, // 225C->self, 20.0s cast, range 80 circle
}

public enum SID : uint
{
    MagicVulnerabilityUp = 658, // 225C->player, extra=0x1
    Dropsy = 289, // none->player, extra=0x0
}

public enum IconID : uint
{
    Tankbuster = 198, // player->self
    Spread = 139, // player->self
    StackTogether = 55, // player->self
}

class TidePod(BossModule module) : Components.SingleTargetCast(module, AID.TidePod);
class WaterIV(BossModule module) : Components.RaidwideCast(module, AID.WaterIV);
class BrinyCannonade(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Spread, AID.BrinyCannonadeSpread, 6, 5.1f);
class TsunamiRaidwide(BossModule module) : Components.RaidwideCast(module, AID.TsunamiRaidwideKB);
class TsunamiKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, AID.TsunamiRaidwideKB, 15);
class TsunamiVoidzone(BossModule module) : Components.GenericAOEs(module, AID.TsunamiHit)
{
    public static readonly AOEShapeCone Shape = new(40, 60.Degrees());
    AOEInstance? _predicted;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_predicted);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.TsunamiCastCW:
                _predicted = new(Shape, spell.LocXZ, spell.Rotation - 90.Degrees(), Module.CastFinishAt(spell, 8.1f));
                break;
            case AID.TsunamiCastForward:
                _predicted = new(Shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, 8.1f));
                break;
            case AID.TsunamiCastCCW:
                _predicted = new(Shape, spell.LocXZ, spell.Rotation + 90.Degrees(), Module.CastFinishAt(spell, 8.1f));
                break;
            case AID.TsunamiCastBackward:
                _predicted = new(Shape, spell.LocXZ, spell.Rotation + 180.Degrees(), Module.CastFinishAt(spell, 8.1f));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            if (NumCasts >= 9)
            {
                NumCasts = 0;
                _predicted = null;
            }
        }

        switch ((AID)spell.Action.ID)
        {
            case AID.TsunamiInstantCW:
                _predicted = new(Shape, caster.Position, spell.Rotation - 90.Degrees(), WorldState.FutureTime(8.1f));
                break;
            case AID.TsunamiInstantCCW:
                _predicted = new(Shape, caster.Position, spell.Rotation + 90.Degrees(), WorldState.FutureTime(8.1f));
                break;
            case AID.TsunamiInstantBackward:
                _predicted = new(Shape, caster.Position, spell.Rotation + 180.Degrees(), WorldState.FutureTime(8.1f));
                break;
            case AID.TsunamiInstantForward:
                _predicted = new(Shape, caster.Position, spell.Rotation, WorldState.FutureTime(8.1f));
                break;
        }
    }
}

class DarkRain(BossModule module) : Components.StandardAOEs(module, AID.DarkRainPuddle, 8);
class Materialize(BossModule module) : Components.StandardAOEs(module, AID.Materialize, 6);
class DarkCannonade(BossModule module) : Components.StackTogether(module, (uint)IconID.StackTogether, 5.1f, AID.DarkCannonadeStack, radius: 1.5f);
// TODO: second pattern does spinnies, not straight line
class Jet(BossModule module) : Components.Voidzone(module, 6, OID.DarkEwer, moveHintLength: 6);
class DarkeningDeluge(BossModule module) : Components.VoidzoneAtCastTarget(module, 8, AID.DarkeningDeluge, OID.DarkeningDeluge, 2.5f);
class Slime(BossModule module) : Components.Adds(module, (uint)OID.DarkRain, forbidDots: true);
class Explosion(BossModule module) : Components.CastHint(module, AID.Explosion, "Kill slime before enrage!", true);

//class TsunamiConeDebug(BossModule module) : Components.DebugCasts(module, [AID.TsunamiHitKB, AID.TsunamiHit], new AOEShapeCone(50, 60.Degrees()), expireAfter: 10);

class A21FamfritTheDarkeningCloudStates : StateMachineBuilder
{
    public A21FamfritTheDarkeningCloudStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TidePod>()
            .ActivateOnEnter<WaterIV>()
            .ActivateOnEnter<BrinyCannonade>()
            .ActivateOnEnter<TsunamiRaidwide>()
            .ActivateOnEnter<TsunamiKnockback>()
            .ActivateOnEnter<TsunamiVoidzone>()
            .ActivateOnEnter<DarkRain>()
            .ActivateOnEnter<Materialize>()
            .ActivateOnEnter<Jet>()
            .ActivateOnEnter<DarkeningDeluge>()
            .ActivateOnEnter<Slime>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<DarkCannonade>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 550, NameID = 7245)]
public class A21FamfritTheDarkeningCloud(ModuleInit init) : BossModule(init, new(-200, 66.5f), new ArenaBoundsCircle(30));
