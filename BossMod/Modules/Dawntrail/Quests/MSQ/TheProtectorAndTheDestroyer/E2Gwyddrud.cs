namespace BossMod.Dawntrail.Quest.MSQ.TheProtectorAndTheDestroyer.Gwyddrud;

public enum OID : uint
{
    Boss = 0x4349, // R5.0    
    Gwyddrud = 0x3A5E, // R1.000, x24
    LimitBreakHelper = 0x40B5, // R1.000, x1
    WukLamat = 0x4146, // R0.500, x1
    Alisaie = 0x4339, // R0.485, x1
    OtisOathmender = 0x4348, // R1.500, x1
    Sphene = 0x4337, // R0.500, x1
    BallOfLevin = 0x434A, // R1.500, x0 (spawn during fight)
    SuperchargedLevin = 0x39C4, // R2.000, x0 (spawn during fight)
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player/WukLamat, no cast, single-target
    CracklingHowlVisual = 38211, // Boss->self, 4.3+0,7s cast, single-target
    CracklingHowl = 38212, // Helper->self, 5.0s cast, range 40 circle
    Teleport = 38213, // Boss->location, no cast, single-target
    VioletVoltageTelegraph = 38220, // Helper->self, 2.5s cast, range 20 180-degree cone
    VioletVoltage = 38221, // Helper->self, no cast, range 20 180-degree cone
    VioletVoltageVisual1 = 38214, // Boss->self, 8.3+0,7s cast, single-target
    VioletVoltageVisual2 = 38216, // Boss->self, no cast, single-target
    VioletVoltageVisual3 = 38217, // Boss->self, no cast, single-target
    VioletVoltageVisual4 = 38215, // Boss->self, 10.3+0,7s cast, single-target
    VioletVoltageVisual5 = 38218, // Boss->self, no cast, single-target
    VioletVoltageVisual6 = 38219, // Boss->self, no cast, single-target
    Gnaw = 38222, // Boss->tank, 5.0s cast, single-target
    RollingThunder = 38223, // Boss->self, 4.3+0,7s cast, single-target
    RollingThunder2 = 38224, // Helper->self, 5.0s cast, range 20 45-degree cone
    RoaringBoltKB = 38230, // Boss->self, 4.3+0,7s cast, range 20 circle, knockback 12, away from source
    RoaringBolt = 38231, // Helper->location, 7.0s cast, range 6 circle
    GatheringStorm = 38225, // Boss->self, no cast, single-target, limit break phase
    LevinStartMoving = 38226, // BallOfLevin/SuperchargedLevin->LimitBreakHelper, no cast, single-target
    GatheringSurge = 38243, // Boss->self, no cast, single-target
    UntamedCurrent = 38232, // Boss->self, 3.3+0,7s cast, range 40 circle, knockback 15, away from source
    UntamedCurrentAOE1 = 38233, // Gwyddrud->location, 3.1s cast, range 5 circle
    UntamedCurrentAOE2 = 19718, // Gwyddrud->location, 3.2s cast, range 5 circle
    UntamedCurrentAOE3 = 19719, // Gwyddrud->location, 3.3s cast, range 5 circle
    UntamedCurrentAOE4 = 19999, // Gwyddrud->location, 3.0s cast, range 5 circle
    UntamedCurrentAOE5 = 38234, // Gwyddrud->location, 3.1s cast, range 5 circle
    UntamedCurrentAOE6 = 19720, // Gwyddrud->location, 3.2s cast, range 5 circle
    UntamedCurrentAOE7 = 19721, // Gwyddrud->location, 3.3s cast, range 5 circle
    UntamedCurrentAOE8 = 19728, // Gwyddrud->location, 3.3s cast, range 5 circle
    UntamedCurrentAOE9 = 19727, // Gwyddrud->location, 3.2s cast, range 5 circle
    UntamedCurrentAOE10 = 19179, // Gwyddrud->location, 3.1s cast, range 5 circle
    UntamedCurrentSpread = 19181, // Helper->all, 5.0s cast, range 5 circle
    UntamedCurrentStack = 19276, // Helper->Alisaie, 5.0s cast, range 6 circle
}

class Gnaw(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Gnaw));
class CracklingHowl(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.CracklingHowl));
class UntamedCurrent(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.UntamedCurrent));

class VioletVoltage(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCone cone = new(20, 90.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
            yield return new(_aoes[0].Shape, _aoes[0].Origin, _aoes[0].Rotation, _aoes[0].Activation, ArenaColor.Danger);
        if (_aoes.Count > 1)
            yield return new(_aoes[1].Shape, _aoes[1].Origin, _aoes[1].Rotation, _aoes[1].Activation, Risky: !_aoes[1].Rotation.AlmostEqual(_aoes[0].Rotation + 180.Degrees(), Helpers.RadianConversion));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.VioletVoltageTelegraph)
            _aoes.Add(new(cone, caster.Position, spell.Rotation, spell.NPCFinishAt.AddSeconds(6)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID == AID.VioletVoltage)
            _aoes.RemoveAt(0);
    }
}

class RoaringBoltKB(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.RoaringBoltKB), 12, stopAtWall: true)
{
    public DateTime Activation;

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => Module.FindComponent<RoaringBolt>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action == WatchedAction)
            Activation = spell.NPCFinishAt.AddSeconds(1.2f);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var forbidden = new List<Func<WPos, float>>();
        var component = Module.FindComponent<RoaringBolt>()?.ActiveAOEs(slot, actor)?.ToList();
        if (component != null && component.Count != 0 && Sources(slot, actor).Any() || Activation > Module.WorldState.CurrentTime) // 1.2s delay to wait for action effect
        {
            foreach (var c in component!)
            {
                forbidden.Add(ShapeDistance.Cone(Module.Center, 19.5f, Angle.FromDirection(c.Origin - Module.Center), 25.Degrees()));
            }
            if (forbidden.Count > 0)
                hints.AddForbiddenZone(p => forbidden.Select(f => f(p)).Min(), Activation);
        }
    }
}

class RollingThunder(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RollingThunder2), new AOEShapeCone(20, 22.5f.Degrees()), 6)
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => ActiveCasters.Select((c, i) => new AOEInstance(Shape, c.Position, c.CastInfo!.Rotation, c.CastInfo.NPCFinishAt, i < 2 ? ArenaColor.Danger : ArenaColor.AOE));
}

class RoaringBolt(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.RoaringBolt), 6);
class UntamedCurrentSpread(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.UntamedCurrentSpread), 5);
class UntamedCurrentStack(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.UntamedCurrentStack), 6);
class UntamedCurrentAOE1(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.UntamedCurrentAOE1), 5);
class UntamedCurrentAOE2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.UntamedCurrentAOE2), 5);
class UntamedCurrentAOE3(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.UntamedCurrentAOE3), 5);
class UntamedCurrentAOE4(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.UntamedCurrentAOE4), 5);
class UntamedCurrentAOE5(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.UntamedCurrentAOE5), 5);
class UntamedCurrentAOE6(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.UntamedCurrentAOE6), 5);
class UntamedCurrentAOE7(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.UntamedCurrentAOE7), 5);
class UntamedCurrentAOE8(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.UntamedCurrentAOE8), 5);
class UntamedCurrentAOE9(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.UntamedCurrentAOE9), 5);
class UntamedCurrentAOE10(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.UntamedCurrentAOE10), 5);

class GwyddrudStates : StateMachineBuilder
{
    public GwyddrudStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CracklingHowl>()
            .ActivateOnEnter<UntamedCurrent>()
            .ActivateOnEnter<UntamedCurrentAOE1>()
            .ActivateOnEnter<UntamedCurrentAOE2>()
            .ActivateOnEnter<UntamedCurrentAOE3>()
            .ActivateOnEnter<UntamedCurrentAOE4>()
            .ActivateOnEnter<UntamedCurrentAOE5>()
            .ActivateOnEnter<UntamedCurrentAOE6>()
            .ActivateOnEnter<UntamedCurrentAOE7>()
            .ActivateOnEnter<UntamedCurrentAOE8>()
            .ActivateOnEnter<UntamedCurrentAOE9>()
            .ActivateOnEnter<UntamedCurrentAOE10>()
            .ActivateOnEnter<UntamedCurrentSpread>()
            .ActivateOnEnter<UntamedCurrentStack>()
            .ActivateOnEnter<RoaringBolt>()
            .ActivateOnEnter<RoaringBoltKB>()
            .ActivateOnEnter<RollingThunder>()
            .ActivateOnEnter<VioletVoltage>()
            .ActivateOnEnter<Gnaw>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70478, NameID = 13170)]
public class Gwyddrud(WorldState ws, Actor primary) : BossModule(ws, primary, new(349, -14), new ArenaBoundsCircle(19.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.BallOfLevin))
            Arena.Actor(s, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.SuperchargedLevin))
            Arena.Actor(s, ArenaColor.Enemy);
    }
}
