using BossMod.Data;

namespace BossMod.Dawntrail.Foray.ForkedTower.FT12SwordDancer;

public enum OID : uint
{
    _Gen_DancingSword = 0x4D7C, // R2.000, x16
    _Gen_DancingSword1 = 0x4D7A, // R1.000, x5
    _Gen_DancingSword2 = 0x4D7B, // R2.000, x2
    _Gen_DancingSword3 = 0x4D79, // R2.000, x3
    _Gen_DancingSword4 = 0x4D77, // R2.000, x4
    _Gen_SwordDancer = 0x4D7D, // R1.000, x1
    Helper = 0x233C, // R0.500, x29, Helper type
    Boss = 0x4D76, // R6.000, x1

    SwordDance = 0x1EC033
}

public enum AID : uint
{
    _AutoAttack_ = 50925, // Boss->player, no cast, single-target
    _Weaponskill_SwordStorm = 49617, // Boss->self, 5.0s cast, ???
    _Ability_SwordStorm = 49684, // Helper->self, no cast, ???
    _Ability_ = 49558, // Boss->location, no cast, single-target
    _Ability_1 = 49557, // 4D7D->self, no cast, range ?-30 donut
    _Weaponskill_ThrowingSwords = 49559, // Boss->self, 2.0+1.0s cast, single-target
    _Ability_Rush = 50525, // 4D77->location, 3.0s cast, width 7 rect charge
    _Ability_Rush1 = 50526, // 4D77->location, 3.0s cast, width 7 rect charge
    _Ability_Turn = 49563, // 4D77->location, 3.5s cast, ???
    _Ability_Turn1 = 49575, // Helper->self, 3.5s cast, range 9-14 donut
    _Ability_Turn2 = 49577, // Helper->self, 3.5s cast, range 19-24 donut
    _Ability_Turn6 = 49578, // Helper->self, 3.5s cast, range 9-14 donut
    _Ability_Turn3 = 49568, // 4D77->location, 3.5s cast, ???
    _Ability_Turn4 = 49574, // 4D77->location, 3.5s cast, ???
    _Ability_Turnabout = 49889, // Helper->self, 3.5s cast, range 19-24 donut
    _Weaponskill_MartialMystique = 49584, // Boss->self, 4.0+1.5s cast, single-target
    _Ability_MartialMystique = 49585, // Helper->self, 5.5s cast, range 48 width 96 rect
    _Ability_Turn5 = 49569, // 4D77->location, 3.5s cast, ???
    _Weaponskill_MartialMystique1 = 49583, // Boss->self, 4.0+1.5s cast, single-target
    _Weaponskill_CycloswordsUnsheathed = 49586, // Boss->self, 3.0s cast, single-target
    _Weaponskill_Cycloswords = 49587, // Boss->self, 3.0s cast, single-target
    _Ability_Spin = 49592, // 4D79->self, 1.0s cast, range 15 circle
    _Ability_Spin1 = 49589, // 4D79->self, 1.0s cast, range 15-60 donut
    _Weaponskill_SwordDance = 49609, // Boss->self, 4.4+0.6s cast, single-target
    _Ability_SwordDance = 49610, // Helper->self, 5.0s cast, ???
    _Ability_SwordDance1 = 49611, // Helper->self, no cast, ???
    _Ability_SwordDance2 = 49612, // Helper->self, no cast, ???
    _Ability_SwordDance3 = 49613, // Helper->self, no cast, ???
    _Ability_SwordDance4 = 49614, // Helper->self, 1.5s cast, range 60 width 20 rect
    _Weaponskill_LeapingLift = 49594, // Boss->self, 3.0s cast, single-target
    _Ability_Pierce = 49595, // 4D7A->self, 3.6s cast, range 5 circle
    _Ability_LeapingLift = 49596, // Boss->location, no cast, ???
    _Ability_LeapingLift1 = 49597, // Boss->location, no cast, single-target
    _Ability_LeapingLift2 = 49598, // Boss->location, no cast, ???
    _Weaponskill_Swordpointe = 49685, // Boss->self, 2.0+1.0s cast, single-target
    _Ability_Steelsbreath = 49599, // 4D7A->self, 2.0s cast, ???
    _Ability_Steelsbreath1 = 50359, // Helper->self, 2.0s cast, ???
    _Weaponskill_SurgeswordsUnsheathed = 49615, // Boss->self, 3.0s cast, single-target
    _Ability_Rush2 = 49616, // 4D7C->self, 4.0s cast, range 30 width 6 rect
    _Weaponskill_ThrowingSwords1 = 49560, // Boss->self, no cast, single-target
    _Ability_Spin2 = 49590, // 4D79->self, 1.0s cast, range 20-60 donut
    Spin3 = 49593, // 4D79->self, 1.0s cast, range 20 circle
}

public enum SID : uint
{
    _Gen_ = 3558, // none->4D79, extra=0x46E/0x46F
    _Gen_1 = 2056, // none->Boss/4D7A, extra=0x47A/0x47B
    _Gen_Weakness = 43, // none->player, extra=0x0
    _Gen_Transcendent = 418, // none->player, extra=0x0
}

public enum TetherID : uint
{
    _Gen_Tether_chn_sworddancer_l01t1 = 424, // 4D77->Boss
    _Gen_Tether_chn_sworddancer_r01t1 = 423, // 4D77->Boss
}

class SwordStorm(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_SwordStorm);
class Rush1(BossModule module) : Components.ChargeAOEs(module, AID._Ability_Rush, 3.5f);
class Rush2(BossModule module) : Components.ChargeAOEs(module, AID._Ability_Rush1, 3.5f);
class TurnSmall(BossModule module) : Components.StandardAOEs(module, AID._Ability_Turn1, new AOEShapeDonutSector(9, 14, 45.Degrees()));
class TurnLarge(BossModule module) : Components.StandardAOEs(module, AID._Ability_Turn2, new AOEShapeDonutSector(19, 24, 45.Degrees()));
class TurnSmallShort(BossModule module) : Components.StandardAOEs(module, AID._Ability_Turn6, new AOEShapeDonutSector(9, 14, 33.Degrees()));
class Turnabout(BossModule module) : Components.StandardAOEs(module, AID._Ability_Turnabout, new AOEShapeDonutSector(19, 24, 39.Degrees()));
class MartialMystique(BossModule module) : Components.StandardAOEs(module, AID._Ability_MartialMystique, new AOEShapeRect(48, 48));

class Spin(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<AOEInstance> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted;

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if ((OID)actor.OID == OID._Gen_DancingSword3 && animState1 == 1 && animState2 == 0)
        {
            AOEShape? shape = modelState switch
            {
                4 => new AOEShapeDonut(15, 60),
                5 => new AOEShapeDonut(20, 60),
                7 => new AOEShapeCircle(15),
                31 => new AOEShapeCircle(20),
                _ => null
            };

            if (shape != null)
                _predicted.Add(new(shape, actor.Position, default, WorldState.FutureTime(13.3f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Ability_Spin or AID._Ability_Spin1 or AID._Ability_Spin2 or AID.Spin3 && _predicted.Count > 0)
            _predicted.RemoveAt(0);
    }
}

class SwordDanceRaidwide(BossModule module) : Components.RaidwideCast(module, AID._Ability_SwordDance)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Ability_SwordDance3)
            Casters.Clear();
    }
}

// 91 -> 99.9
// 91.9 -> 102.4
// 92.7 -> 104.9
// 93.5 -> 107.4
class SwordDance(BossModule module) : Components.GenericAOEs(module, AID._Ability_SwordDance4)
{
    readonly List<AOEInstance> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        for (var i = 0; i < Math.Min(3, _predicted.Count); i++)
            yield return _predicted[i] with { Color = i == 0 ? ArenaColor.Danger : ArenaColor.AOE, Risky = i == 0 && !(actor.FindStatus(PhantomSID.Shirahadori)?.ExpireAt > _predicted[i].Activation) };
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if ((OID)actor.OID == OID.SwordDance && state == 0x00010002)
            _predicted.Add(new(new AOEShapeRect(60, 10, 60), actor.Position, actor.Rotation, _predicted.Count > 0 ? _predicted[^1].Activation.AddSeconds(2.5f) : WorldState.FutureTime(8.9f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Ability_SwordDance4 && _predicted.Count > 0)
            _predicted.RemoveAt(0);
    }
}

class FT12SwordDancerStates : StateMachineBuilder
{
    public FT12SwordDancerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SwordStorm>()
            .ActivateOnEnter<Rush1>()
            .ActivateOnEnter<Rush2>()
            .ActivateOnEnter<TurnSmall>()
            .ActivateOnEnter<TurnLarge>()
            .ActivateOnEnter<TurnSmallShort>()
            .ActivateOnEnter<Turnabout>()
            .ActivateOnEnter<MartialMystique>()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<SwordDanceRaidwide>()
            .ActivateOnEnter<SwordDance>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14820)]
public class FT12SwordDancer(WorldState ws, Actor primary) : BossModule(ws, primary, new(600, 704), new ArenaBoundsCircle(24))
{
    public override bool DrawAllPlayers => true;
}

