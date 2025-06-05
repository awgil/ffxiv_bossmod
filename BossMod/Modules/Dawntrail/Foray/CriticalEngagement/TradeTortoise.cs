namespace BossMod.Dawntrail.Foray.CriticalEngagement.TradeTortoise;

public enum OID : uint
{
    Boss = 0x46E7, // R7.000, x1
    Helper = 0x233C, // R0.500, x20, Helper type
    DeathWallHelper = 0x4865, // R0.500, x1
    RedGem = 0x46E8, // R5.000, x0 (spawn during fight)
    BlueGem = 0x46E9, // R4.000, x0 (spawn during fight)
    Turtleshell = 0x46EA, // R1.800, x0 (spawn during fight)

    Coins1 = 0x1EBD3E,
    Coins2 = 0x1EBD3F,
    Coins4 = 0x1EBD40,
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    DeathWall = 41535, // DeathWallHelper->self, no cast, range 25-30 donut
    IllGottenGoods = 41518, // Boss->self, 3.0s cast, single-target
    MaterialWorld = 41517, // Boss->self, 4.0s cast, single-target
    EarthquakeCast = 41528, // Boss->self, 5.0s cast, single-target
    Earthquake = 41529, // Helper->self, no cast, ???
    RecommendedForYou = 41993, // Boss->self, 4.0s cast, single-target
    WhatreYouBuying1 = 41515, // Boss->self, 20.0s cast, single-target
    WhatreYouBuying2 = 43452, // Boss->self, 25.0s cast, single-target
    WhatreYouBuyingHelper = 41516, // Helper->self, no cast, ???, deals damage to players with Cursed Change
    OnepennyInflation = 41521, // Boss->self, 7.0s cast, single-target
    TwopennyInflation = 43285, // Boss->self, 7.0s cast, single-target
    FourpennyInflation = 41520, // Boss->self, 7.0s cast, single-target
    CostOfLiving = 41522, // Helper->self, 7.0s cast, ???, range 30 knockback
    WaterspoutCast = 41526, // Boss->self, 5.0s cast, single-target
    Waterspout = 41527, // Helper->self, 8.3s cast, range 12 circle
    HoardWealth = 43372, // Helper->self, 6.0s cast, range 40 60-degree cone

    RubyBlaze = 41533, // player->self, no cast, pyretic fail
}

public enum SID : uint
{
    ColorOrbs = 2552, // none->player, extra=0x35E (red/pyretic) / 0x35F (blue/freeze) / 0x360 (green/kb)
    ValuedCustomer = 4384, // none->player, extra=0x0
    BuyersRemorseRed = 4342, // none->player, extra=0x0, pyretic
    BuyersRemorseBlue = 4343, // none->player, extra=0x0, deep freeze at expiration
    BuyersRemorseGreen = 4344, // none->player, extra=0x0, 35y knockback forward at expiration
    DeepFreeze = 3519, // none->player, extra=0x0
    BuyBuyBuy = 4346, // none->player, extra=0x0
    Transporting = 4376, // none->player, extra=0x29-0x31, see CoinGame
    CursedChange = 4345, // none->player, extra=0x0, coin minigame failure
}

public enum IconID : uint
{
    FireCountdown = 593, // player->self, fire countdown
    IceCountdown = 594, // player->self, ice countdown
    WindCountdown = 585, // player->self, wind countdown
    Checkmark = 503, // player->self
    X = 504, // player->self
}

public enum TetherID : uint
{
    Generic = 1, // Boss->RedGem/BlueGem/Turtleshell
    Coin1 = 328, // player->Turtleshell
    Coin2 = 329, // player->BlueGem
    Coin3 = 330, // player->RedGem/BlueGem/Turtleshell
    Coin4 = 331, // player->RedGem
    Coin5 = 332, // player->BlueGem/RedGem
    Coin6 = 333, // player->Turtleshell/BlueGem
}

class Earthquake(BossModule module) : Components.RaidwideCast(module, AID.EarthquakeCast);

class BuyersRemorseStayMove(BossModule module) : Components.StayMove(module)
{
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.BuyersRemorseRed:
                SetState(Raid.FindSlot(actor.InstanceID), new(Requirement.Stay, status.ExpireAt));
                break;
            case SID.BuyersRemorseBlue:
                SetState(Raid.FindSlot(actor.InstanceID), new(Requirement.Move, status.ExpireAt));
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.BuyersRemorseRed or SID.BuyersRemorseBlue)
            ClearState(Raid.FindSlot(actor.InstanceID));
    }
}

class BuyersRemorseTurtle(BossModule module) : Components.Knockback(module)
{
    private readonly DateTime[] _activations = new DateTime[PartyState.MaxPartySize];

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        var t = _activations.BoundSafeAt(slot);
        if (t != default)
            yield return new(actor.Position, 35, t, Direction: actor.Rotation, Kind: Kind.DirForward);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.BuyersRemorseGreen && Raid.TryFindSlot(actor, out var slot) && slot < PartyState.MaxPartySize)
            _activations[slot] = status.ExpireAt;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.BuyersRemorseGreen && Raid.TryFindSlot(actor, out var slot) && slot < PartyState.MaxPartySize)
            _activations[slot] = default;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var t = _activations.BoundSafeAt(slot);
        if (t != default)
        {
            hints.AddForbiddenZone(ShapeContains.Circle(Arena.Center, 10), t);

            var oo = Arena.Center - actor.Position;
            var center = Angle.FromDirection(oo);
            var cos = (oo.LengthSq() + 35 * 35 - 25 * 25) / (2 * oo.Length() * 35);
            if (cos is <= 1 and >= -1)
                hints.ForbiddenDirections.Add(((center + 180.Degrees()).Normalized(), Angle.Acos(-cos), t));
        }
    }
}

class CoinGame(BossModule module) : BossComponent(module)
{
    private readonly TradeTortoiseConfig _config = Service.Config.Get<TradeTortoiseConfig>();

    // indexed by Transporting status param (minus 0x28)
    // player is only allowed to hold 2 coin bags, so if param >= 4, we finish the mechanic regardless of whether the total is correct
    public static readonly int[] HeldQuantity = [0, 1, 2, 4, 2, 3, 4, 5, 6, 8];

    public const float GoalSize = 7;

    record struct State(int Goal, int CarryParam, WPos Destination)
    {
        public readonly bool AtCapacity => CarryParam > 3;
        public readonly int Held => HeldQuantity.BoundSafeAt(CarryParam);
        public readonly int Deficit => Goal - Held;
    }

    private readonly State[] _playerStates = new State[PartyState.MaxPartySize];

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (!Raid.TryFindSlot(source, out var slot) || slot >= PartyState.MaxPartySize)
            return;

        var coinCount = (TetherID)tether.ID switch
        {
            TetherID.Coin1 => 1,
            TetherID.Coin2 => 2,
            TetherID.Coin3 => 3,
            TetherID.Coin4 => 4,
            TetherID.Coin5 => 5,
            TetherID.Coin6 => 6,
            _ => 0
        };
        if (coinCount > 0)
        {
            var dest = WorldState.Actors.Find(tether.Target)!;
            _playerStates[slot] = new(coinCount, 0, dest.Position);
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Transporting && Raid.TryFindSlot(actor, out var slot) && slot < PartyState.MaxPartySize)
            _playerStates[slot] = _playerStates[slot] with { CarryParam = status.Extra - 40 };
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var obj = _playerStates.BoundSafeAt(slot);
        if (obj.Goal > 0)
        {
            if (obj.AtCapacity || obj.Deficit <= 0)
                hints.Add("Drop off!", false);
            else
            {
                var s = obj.Deficit == 1 ? "" : "s";
                hints.Add($"Pick up {obj.Deficit} coin{s}!", false);
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var obj = _playerStates.BoundSafeAt(pcSlot);
        if (obj.Destination != default)
        {
            if (obj.AtCapacity || obj.Deficit <= 0)
                Arena.ZoneCircle(obj.Destination, GoalSize, ArenaColor.SafeFromAOE);
            else
                Arena.AddCircle(obj.Destination, GoalSize, ArenaColor.Safe);
        }

        var show4 = (obj.Deficit & 4) == 4;
        var show2 = (obj.Deficit & 2) == 2;
        var show1 = (obj.Deficit & 1) == 1;

        if (show4)
            Arena.Actors(Module.Enemies(OID.Coins4), ArenaColor.Object, true);
        if (show2)
            Arena.Actors(Module.Enemies(OID.Coins2), ArenaColor.Object, true);
        if (show1)
            Arena.Actors(Module.Enemies(OID.Coins1), ArenaColor.Object, true);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!_config.CoinGame)
            return;

        var obj = _playerStates.BoundSafeAt(slot);
        if (obj.Goal == 0)
            return;

        if (obj.AtCapacity || obj.Deficit <= 0)
        {
            hints.GoalZones.Add(p => p.InCircle(obj.Destination, 5) ? 10 : 0);
            return;
        }

        if ((obj.Deficit & 4) == 4)
            hints.InteractWithOID(WorldState, OID.Coins4);
        else if ((obj.Deficit & 2) == 2)
            hints.InteractWithOID(WorldState, OID.Coins2);
        else if ((obj.Deficit & 1) == 1)
            hints.InteractWithOID(WorldState, OID.Coins1);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID is IconID.Checkmark or IconID.X && Raid.TryFindSlot(actor, out var slot) && slot < PartyState.MaxPartySize)
            _playerStates[slot] = default;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.WhatreYouBuyingHelper)
            Array.Fill(_playerStates, default);
    }
}

class CostOfLiving(BossModule module) : Components.KnockbackFromCastTarget(module, AID.CostOfLiving, 30)
{
    // 3 helpers cast the spell since effectresult maxes out at 24 targets
    public override IEnumerable<Source> Sources(int slot, Actor actor) => base.Sources(slot, actor).Take(1);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var src in Sources(slot, actor))
        {
            if (!IsImmune(slot, src.Activation))
                hints.AddForbiddenZone(p =>
                {
                    var dir = (p - src.Origin).Normalized() * 30;
                    return !(p + dir).InCircle(Arena.Center, 25);
                }, src.Activation);
        }
    }
}

class Waterspout(BossModule module) : Components.StandardAOEs(module, AID.Waterspout, new AOEShapeCircle(12), maxCasts: 7);
class HoardWealth(BossModule module) : Components.StandardAOEs(module, AID.HoardWealth, new AOEShapeCone(40, 30.Degrees()), maxCasts: 3);

class TradeTortoiseStates : StateMachineBuilder
{
    public TradeTortoiseStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BuyersRemorseStayMove>()
            .ActivateOnEnter<BuyersRemorseTurtle>()
            .ActivateOnEnter<Earthquake>()
            .ActivateOnEnter<CoinGame>()
            .ActivateOnEnter<CostOfLiving>()
            .ActivateOnEnter<Waterspout>()
            .ActivateOnEnter<HoardWealth>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13695)]
public class TradeTortoise(WorldState ws, Actor primary) : BossModule(ws, primary, new(72, -545), new ArenaBoundsCircle(25))
{
    public override bool DrawAllPlayers => true;
}

[ConfigDisplay(Parent = typeof(DawntrailConfig))]
public class TradeTortoiseConfig : ConfigNode
{
    [PropertyDisplay("Automatically solve the coin minigame (if AI mode or NormalMovement is active)")]
    public bool CoinGame = true;
}
