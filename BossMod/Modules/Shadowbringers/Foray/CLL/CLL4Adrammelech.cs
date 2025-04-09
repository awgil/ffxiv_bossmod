#if DEBUG
namespace BossMod.Shadowbringers.Foray.CLL.CLL4Adrammelech;

public enum OID : uint
{
    Boss = 0x2E01,
    Helper = 0x233C,
}

public enum AID : uint
{
    AutoAttack = 20372, // Boss->player, no cast, single-target
    HolyIV = 20374, // Boss->self, 4.0s cast, range 60 circle
    CurseOfTheFiend = 20346, // Boss->self, 3.0s cast, single-target
    AccursedBecoming = 20347, // Boss->self, 4.0s cast, single-target
    WaterIVVisual = 21464, // Helper->self, 6.5s cast, range 60 circle
    WaterIV1 = 20350, // Helper->self, 6.5s cast, range 60 distance 12 knockback
    WaterIV2 = 20556, // Helper->self, 6.5s cast, range 60 distance 12 knockback
    WaterIV3 = 20557, // Helper->self, 6.5s cast, range 60 distance 12 knockback
    WaterIV4 = 20356, // Helper->self, 6.5s cast, range 60 distance 12 knockback
    BlizzardIV1 = 21595, // Helper->self, 6.5s cast, deep freeze effect
    BlizzardIV2 = 21597, // Helper->self, 6.5s cast, deep freeze effect
    BlizzardIV3 = 20555, // Helper->self, 6.5s cast, deep freeze effect
    BlizzardIV4 = 20349, // Helper->self, 6.5s cast, deep freeze effect
    AeroIV = 20352, // Helper->self, 6.5s cast, range 15-30 donut
    AeroIV2 = 20358, // Helper->self, 6.5s cast, range 15-30 donut
    BurstII = 20363, // Helper->location, 4.0s cast, range 6 circle
    BurstIIBoss = 20362, // Boss->self, 4.0s cast, single-target
    WarpedLightBoss = 20364, // Boss->self, 8.0s cast, single-target
    WarpedLight1 = 20365, // Helper->location, 8.0s cast, width 3 rect charge
    WarpedLight2 = 20558, // Helper->location, 8.5s cast, width 3 rect charge
    WarpedLight3 = 20559, // Helper->location, 9.0s cast, width 3 rect charge
    WarpedLight4 = 20560, // Helper->location, 9.5s cast, width 3 rect charge
    WarpedLight5 = 20561, // Helper->location, 10.0s cast, width 3 rect charge
    WarpedLight6 = 20562, // Helper->location, 10.5s cast, width 3 rect charge
    Shock = 20366, // Helper->self, 11.0s cast, range 35 circle
    Flare = 20373, // Boss->player, 4.0s cast, single-target
    TornadoBoss = 20367, // Boss->self, 4.0s cast, single-target
    Tornado = 20368, // Helper->location, 4.0s cast, range 6 circle
}

class AeroIV(BossModule module) : Components.StandardAOEs(module, ActionID.MakeSpell(AID.AeroIV), new AOEShapeDonut(15, 30));
class AeroIV2(BossModule module) : Components.StandardAOEs(module, ActionID.MakeSpell(AID.AeroIV2), new AOEShapeDonut(15, 30));
class BurstII(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.BurstII), 6);
class BlizzardIV(BossModule module) : Components.StayMove(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.BlizzardIV1:
            case AID.BlizzardIV2:
            case AID.BlizzardIV3:
            case AID.BlizzardIV4:
                Array.Fill(PlayerStates, new(Requirement.Move, Module.CastFinishAt(spell)));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.BlizzardIV1:
            case AID.BlizzardIV2:
            case AID.BlizzardIV3:
            case AID.BlizzardIV4:
                Array.Fill(PlayerStates, default);
                break;
        }
    }
}

class WaterIV(BossModule module) : Components.KnockbackFromCastTarget(module, default, 12)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.WaterIV1:
            case AID.WaterIV2:
            case AID.WaterIV3:
            case AID.WaterIV4:
                if (Casters.Count == 0)
                    Casters.Add(caster);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.WaterIV1:
            case AID.WaterIV2:
            case AID.WaterIV3:
            case AID.WaterIV4:
                Casters.Remove(caster);
                break;
        }
    }
}

class WarpedLight(BossModule module) : Components.ChargeAOEs(module, default, 1.5f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.WarpedLight1:
            case AID.WarpedLight2:
            case AID.WarpedLight3:
            case AID.WarpedLight4:
            case AID.WarpedLight5:
            case AID.WarpedLight6:
                var dir = spell.LocXZ - caster.Position;
                Casters.Add((caster, new AOEShapeRect(dir.Length(), HalfWidth), Angle.FromDirection(dir)));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.WarpedLight1:
            case AID.WarpedLight2:
            case AID.WarpedLight3:
            case AID.WarpedLight4:
            case AID.WarpedLight5:
            case AID.WarpedLight6:
                Casters.RemoveAll(e => e.caster == caster);
                break;
        }
    }
}

class Shock(BossModule module) : Components.StandardAOEs(module, ActionID.MakeSpell(AID.Shock), new AOEShapeCircle(35));
class Flare(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Flare));

class AdrammelechStates : StateMachineBuilder
{
    public AdrammelechStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WarpedLight>()
            .ActivateOnEnter<Shock>()
            .ActivateOnEnter<Flare>()
            .ActivateOnEnter<AeroIV>()
            .ActivateOnEnter<AeroIV2>()
            .ActivateOnEnter<WaterIV>()
            .ActivateOnEnter<BlizzardIV>()
            .ActivateOnEnter<BurstII>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 735, NameID = 9442)]
public class Adrammelech(WorldState ws, Actor primary) : BossModule(ws, primary, new(80, -606), new ArenaBoundsCircle(29.5f))
{
    public override bool DrawAllPlayers => true;
}
#endif
