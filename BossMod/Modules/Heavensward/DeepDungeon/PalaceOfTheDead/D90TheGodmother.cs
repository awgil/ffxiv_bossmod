namespace BossMod.Heavensward.DeepDungeon.PalaceoftheDead.D90TheGodmother;

public enum OID : uint
{
    Boss = 0x1817, // R3.750, x1
    LavaBomb = 0x18E9, // R0.600, x0 (spawn during fight)
    GreyBomb = 0x18E8, // R1.200, x0 (spawn during fight)
    GiddyBomb = 0x18EA, // R1.200, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target
    Burst = 7105, // GreyBomb->self, 20.0s cast, range 50+R circle
    HypothermalCombustion = 7104, // GiddyBomb->self, 5.0s cast, range 6+R circle
    MassiveBurst = 7102, // Boss->self, 25.0s cast, range 50 circle
    Sap = 7101, // Boss->location, 3.5s cast, range 8 circle
    ScaldingScolding = 7100, // Boss->self, no cast, range 8+R ?-degree cone
    SelfDestruct = 7106, // LavaBomb->self, 3.0s cast, range 6+R circle
}

class BossAdds(BossModule module) : Components.AddsMulti(module, [(uint)OID.GreyBomb, (uint)OID.GiddyBomb]);
class Burst(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Burst), "Kill the Grey Bomb! or take 80% of your Max HP");
// future thing to do: maybe add a tether between bomb/boss to show it needs to show the aoe needs to explode on them. . . 
class HypothermalCombustion(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HypothermalCombustion), new AOEShapeCircle(7.2f));
class MassiveBurst(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.MassiveBurst), "Knock the Giddy bomb into the boss and let it explode on the boss. \n or else take 99% damage!");
class Sap(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Sap), 8);
class ScaldingScolding(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.ScaldingScolding), new AOEShapeCone(11.75f, 45.Degrees()))
{
    private readonly MassiveBurst _raidwide1 = module.FindComponent<MassiveBurst>()!;
    private readonly Sap _locationaoe1 = module.FindComponent<Sap>()!;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!_raidwide1.Active && !_locationaoe1.ActiveCasters.Any())
            base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!_raidwide1.Active && !_locationaoe1.ActiveCasters.Any())
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!_raidwide1.Active && !_locationaoe1.ActiveCasters.Any())
            base.DrawArenaForeground(pcSlot, pc);
    }
}

class SelfDestruct(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle circle = new(6.6f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.LavaBomb)
            _aoes.Add(new(circle, actor.Position, default, WorldState.FutureTime(10)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SelfDestruct)
            _aoes.Clear();
    }
}

class D90TheGodmotherStates : StateMachineBuilder
{
    public D90TheGodmotherStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BossAdds>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<HypothermalCombustion>()
            .ActivateOnEnter<MassiveBurst>()
            .ActivateOnEnter<Sap>()
            .ActivateOnEnter<ScaldingScolding>()
            .ActivateOnEnter<SelfDestruct>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 207, NameID = 5345)]
public class D90TheGodmother(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -235), new ArenaBoundsCircle(25));
