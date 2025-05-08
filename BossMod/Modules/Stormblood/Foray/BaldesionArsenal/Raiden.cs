using BossMod.Modules.Stormblood.Foray;

namespace BossMod.Stormblood.Foray.BaldesionArsenal.Raiden;

public enum OID : uint
{
    Boss = 0x2605,
    Helper = 0x261B, // R0.500, x24, Helper type
    BallLightning = 0x2606, // R1.000, x0 (spawn during fight)
    StreakLightning = 0x2607, // R1.200, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 14777, // Boss->player, no cast, single-target
    SpiritsOfTheFallen = 14458, // Boss->self, 4.0s cast, range 35+R circle
    Shingan = 14459, // Boss->player, 4.0s cast, single-target
    Thundercall = 14463, // Boss->self, 3.0s cast, single-target
    AmeNoSakahoko = 14440, // Boss->self, 4.5s cast, single-target
    AmeNoSakahoko1 = 14441, // Helper->self, 7.5s cast, range 25 circle
    WhirlingZantetsuken = 14442, // Boss->self, 5.5s cast, range ?-60 donut
    Shock = 14445, // BallLightning->self, 3.0s cast, range 8 circle
    LateralZantetsukenRight = 14443, // Boss->self, 6.5s cast, range 70+R width 39 rect
    LateralZantetsukenLeft = 14444, // Boss->self, 6.5s cast, range 70+R width 39 rect
    LancingBolt = 14454, // Boss->self, 3.0s cast, single-target
    LancingBlow = 14455, // StreakLightning->self, no cast, range 10 circle
    BoomingLament = 14461, // Boss->location, 4.0s cast, range 10 circle
    UltimateZantetsuken = 14456, // Boss->self, 18.0s cast, range 80+R circle
    CloudToGround = 14448, // Boss->self, 4.0s cast, single-target
    CloudToGroundFirst = 14449, // Helper->self, 5.0s cast, range 6 circle
    CloudToGroundRest = 14450, // Helper->self, no cast, range 6 circle
}

public enum IconID : uint
{
    StreakLightning = 138, // player->self
}

class BoomingLament(BossModule module) : Components.StandardAOEs(module, AID.BoomingLament, 10);
class SpiritsOfTheFallen(BossModule module) : Components.RaidwideCast(module, AID.SpiritsOfTheFallen);
class AmeNoSakahoko(BossModule module) : Components.StandardAOEs(module, AID.AmeNoSakahoko1, new AOEShapeCircle(25));
class WhirlingZantetsuken(BossModule module) : Components.StandardAOEs(module, AID.WhirlingZantetsuken, new AOEShapeDonut(5, 60));
class Shock(BossModule module) : Components.StandardAOEs(module, AID.Shock, new AOEShapeCircle(8));
class LateralZantetsuken(BossModule module) : Components.GroupedAOEs(module, [AID.LateralZantetsukenLeft, AID.LateralZantetsukenRight], new AOEShapeRect(75, 19.5f));
class StreakLightning(BossModule module) : Components.GenericStackSpread(module, true)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.StreakLightning)
            Spreads.Add(new(actor, 10, WorldState.FutureTime(6.1f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.LancingBlow && Spreads.Count > 0)
            Spreads.Remove(Spreads.MinBy(s => (s.Target.Position - spell.TargetXZ).LengthSq()));
    }
}
class LightningAdds(BossModule module) : Components.Adds(module, (uint)OID.StreakLightning, 1);
class UltimateZantetsuken(BossModule module) : Components.CastHint(module, AID.UltimateZantetsuken, "Kill adds!", true);

class CloudToGround(BossModule module) : Components.Exaflare(module, new AOEShapeCircle(6), AID.CloudToGroundFirst)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Lines.Add(new()
            {
                Next = caster.Position,
                Advance = caster.Rotation.ToDirection() * 8,
                NextExplosion = Module.CastFinishAt(spell),
                TimeToMove = 1,
                ExplosionsLeft = 8,
                MaxShownExplosions = 4
            });
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.CloudToGroundFirst or AID.CloudToGroundRest)
        {
            for (var i = 0; i < Lines.Count; ++i)
            {
                if (!Lines[i].Next.AlmostEqual(caster.Position, 1))
                    continue;
                AdvanceLine(Lines[i], caster.Position);
                if (Lines[i].ExplosionsLeft == 0)
                    Lines.RemoveAt(i--);
            }
        }
    }
}

class RaidenStates : StateMachineBuilder
{
    public RaidenStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SpiritsOfTheFallen>()
            .ActivateOnEnter<AmeNoSakahoko>()
            .ActivateOnEnter<WhirlingZantetsuken>()
            .ActivateOnEnter<LateralZantetsuken>()
            .ActivateOnEnter<Shock>()
            .ActivateOnEnter<StreakLightning>()
            .ActivateOnEnter<LightningAdds>()
            .ActivateOnEnter<BoomingLament>()
            .ActivateOnEnter<UltimateZantetsuken>()
            .ActivateOnEnter<CloudToGround>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 639, NameID = 7973)]
public class Raiden(WorldState ws, Actor primary) : BAModule(ws, primary, new(0, 458), new ArenaBoundsCircle(35));
