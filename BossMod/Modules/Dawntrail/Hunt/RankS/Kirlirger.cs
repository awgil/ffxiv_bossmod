namespace BossMod.Dawntrail.Hunt.RankS.Kirlirger;

public enum OID : uint
{
    Boss = 0x452A, // R6.250, x1
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    FullmoonFury = 39464, // Boss->self, 6.0s cast, range 20 circle
    DiscordantMoon = 39465, // Boss->self, 6.0s cast, range 10-40 donut
    FightersFlourish = 39466, // Boss->self, 5.0s cast, range 40 270-degree frontal cone
    DiscordantFlourish = 39467, // Boss->self, 5.0s cast, range 40 270-degree backward cone
    FullmoonFuryHonor = 39459, // Boss->self, 6.0s cast, range 20 circle
    FullmoonFuryDishonor = 39463, // Boss->self, 6.0s cast, range 10-40 donut
    DiscordantMoonHonor = 39875, // Boss->self, 6.0s cast, range 10-40 donut
    DiscordantMoonDishonor = 39484, // Boss->self, 6.0s cast, range 20 circle
    FightersFlourishHonor = 39473, // Boss->self, 5.0s cast, range 40 270-degree cone
    FightersFlourishDishonor = 39477, // Boss->self, 5.0s cast, range 40 270-degree cone
    DiscordantFlourishHonor = 39877, // Boss->self, 5.0s cast, range 40 270-degree cone
    DiscordantFlourishDishonor = 39538, // Boss->self, 5.0s cast, range 40 270-degree cone
    DishonorsDiscord1 = 39456, // Boss->self, 3.0s cast, single-target, visual (negate next cast)
    DishonorsDiscord2 = 39457, // Boss->self, 3.0s cast, single-target, visual (negate second cast)
    DishonorsDiscord3 = 39458, // Boss->self, 3.0s cast, single-target, visual (negate third cast)
    HonorsAccord1 = 39453, // Boss->self, 3.0s cast, single-target, visual (literal next cast)
    HonorsAccord2 = 39454, // Boss->self, 3.0s cast, single-target, visual (literal second cast)
    HonorsAccord3 = 39455, // Boss->self, 3.0s cast, single-target, visual (literal third cast)
    EnervatingGloom = 39480, // Boss->players, 5.0s cast, range 6 circle stack
    FlyingFist = 39524, // Boss->self, 2.5s cast, range 40 width 8 rect
    OdiousUproar = 39481, // Boss->self, 5.0s cast, range 40 circle, raidwide
}

public enum IconID : uint
{
    EnervatingGloom = 161, // player
}

class FullmoonFury(BossModule module) : Components.StandardAOEs(module, AID.FullmoonFury, new AOEShapeCircle(20));
class FullmoonFuryHonor(BossModule module) : Components.StandardAOEs(module, AID.FullmoonFuryHonor, new AOEShapeCircle(20));
class FullmoonFuryDishonor(BossModule module) : Components.StandardAOEs(module, AID.FullmoonFuryDishonor, new AOEShapeDonut(10, 40));
class DiscordantMoon(BossModule module) : Components.StandardAOEs(module, AID.DiscordantMoon, new AOEShapeDonut(10, 40));
class DiscordantMoonHonor(BossModule module) : Components.StandardAOEs(module, AID.DiscordantMoonHonor, new AOEShapeDonut(10, 40));
class DiscordantMoonDishonor(BossModule module) : Components.StandardAOEs(module, AID.DiscordantMoonDishonor, new AOEShapeCircle(20));
class FightersFlourish(BossModule module) : Components.StandardAOEs(module, AID.FightersFlourish, new AOEShapeCone(40, 135.Degrees()));
class FightersFlourishHonor(BossModule module) : Components.StandardAOEs(module, AID.FightersFlourishHonor, new AOEShapeCone(40, 135.Degrees()));
class FightersFlourishDishonor(BossModule module) : Components.StandardAOEs(module, AID.FightersFlourishDishonor, new AOEShapeCone(40, 135.Degrees()));
class DiscordantFlourish(BossModule module) : Components.StandardAOEs(module, AID.DiscordantFlourish, new AOEShapeCone(40, 135.Degrees()));
class DiscordantFlourishHonor(BossModule module) : Components.StandardAOEs(module, AID.DiscordantFlourishHonor, new AOEShapeCone(40, 135.Degrees()));
class DiscordantFlourishDishonor(BossModule module) : Components.StandardAOEs(module, AID.DiscordantFlourishDishonor, new AOEShapeCone(40, 135.Degrees()));
class EnervatingGloom(BossModule module) : Components.StackWithCastTargets(module, AID.EnervatingGloom, 6, 4);
class FlyingFist(BossModule module) : Components.StandardAOEs(module, AID.FlyingFist, new AOEShapeRect(40, 4));
class OdiousUproar(BossModule module) : Components.RaidwideCast(module, AID.OdiousUproar);

class KirlirgerStates : StateMachineBuilder
{
    public KirlirgerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FullmoonFury>()
            .ActivateOnEnter<FullmoonFuryHonor>()
            .ActivateOnEnter<FullmoonFuryDishonor>()
            .ActivateOnEnter<DiscordantMoon>()
            .ActivateOnEnter<DiscordantMoonHonor>()
            .ActivateOnEnter<DiscordantMoonDishonor>()
            .ActivateOnEnter<FightersFlourish>()
            .ActivateOnEnter<FightersFlourishHonor>()
            .ActivateOnEnter<FightersFlourishDishonor>()
            .ActivateOnEnter<DiscordantFlourish>()
            .ActivateOnEnter<DiscordantFlourishHonor>()
            .ActivateOnEnter<DiscordantFlourishDishonor>()
            .ActivateOnEnter<EnervatingGloom>()
            .ActivateOnEnter<FlyingFist>()
            .ActivateOnEnter<OdiousUproar>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.S, NameID = 13360)]
public class Kirlirger(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
