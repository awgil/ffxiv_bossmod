namespace BossMod.Heavensward.DeepDungeon.PalaceOfTheDead.D190TheGodfather;

public enum OID : uint
{
    Boss = 0x1820, // R3.750, x1
    GiddyBomb = 0x18F3, // R0.600, x0 (spawn during fight), small bombs that are untargetable, multiple spawn up and explode at once
    LavaBomb = 0x18F2, // R1.200, x0 (spawn during fight), (also known as greybomb) cast a pbaoe that stuns needs to be on top of the boss's hitbox to stun
    RemedyBomb = 0x18F1, // R1.200, x0 (spawn during fight), cast a roomwide aoe that will hit for 80% of max hp + inflicts a dot
    Actor1E86E0 = 0x1E86E0, // R2.000, x1, EventObj type
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target
    Flashthoom = 7170, // LavaBomb->self, 5.0s cast, range 6+R(7.2f) circle, needs to be on boss's hitbox and finish the cast to stun the boss
    HypothermalCombustionMinionCast = 7172, // GiddyBomb->self, 1.5s cast, range 6+R(6.6f) circle, pbaoe's. Should be considered voidzones while they're alive
    HypothermalCombustionRemedyBomb = 7171, // RemedyBomb->self, 15.0s cast, range 50+R(51.2f) circle, roomwide cast, will take 80% of your hp and give a dot (15 potency for 5 seconds) if not killed
    MassiveBurst = 7103, // Boss->self, 25.0s cast, range 50 circle, roomwide cast, needs to be stunned by Flashthoom (Lavabomb/GreyBomb) cast
    Sap = 7169, // Boss->location, 3.5s cast, range 8 circle, location targeted aoe's
    ScaldingScolding = 7168, // Boss->self, no cast, range 8+R(11.75f) ?-degree cone, think might be a 90 degree's, needs checked
}

//TODO: Make the boss's hitbox show up potentially/maybe add 2 circle indicators to show how close the stun bomb needs to be, add indicators to where the stun bomb will spawn next, to allow pre-positioning
//spawn locations for stun bomb are as follows: 1:(-288.626, -300.256) 2:(-297.465, -297.525) 3:(-288.837, -305.537) 4:(-309.132, -303.739) 5:(-298.355, -293.630) 6:(-301.954, -314.289) 7:(-299.119, -297.563)
class BossAdds(BossModule module) : Components.AddsMulti(module, [OID.LavaBomb, OID.RemedyBomb]);
class Flashthoom(BossModule module) : Components.StandardAOEs(module, AID.Flashthoom, new AOEShapeCircle(7.2f))
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (Module.Enemies(OID.LavaBomb).FirstOrDefault() is Actor g && Module.PrimaryActor.Position.InCircle(g.Position, 7.2f))
            hints.SetPriority(g, AIHints.Enemy.PriorityForbidden);
    }
}
class Sap(BossModule module) : Components.StandardAOEs(module, AID.Sap, 8);
class ScaldingScoldingCleave(BossModule module) : Components.Cleave(module, AID.ScaldingScolding, new AOEShapeCone(11.75f, 45.Degrees()), activeWhileCasting: false);

class RemedyBombEnrage(BossModule module) : BossComponent(module)
{
    private DateTime _enrage;

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_enrage != default)
            hints.Add($"Remedy bomb is enraging in {(_enrage - WorldState.CurrentTime).TotalSeconds:f1}s, kill it!");
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        {
            if ((AID)spell.Action.ID == AID.HypothermalCombustionRemedyBomb)
                _enrage = WorldState.FutureTime(16);
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        {
            if ((AID)spell.Action.ID == AID.HypothermalCombustionRemedyBomb)
                _enrage = default;
        }
    }
}

class MassiveBurstEnrage(BossModule module) : BossComponent(module)
{
    private DateTime _enrage;

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_enrage != default)
            hints.Add($"Boss will cast a roomwide in {(_enrage - WorldState.CurrentTime).TotalSeconds:f1}s, stun it with the Lavabomb!");
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        {
            if ((AID)spell.Action.ID == AID.MassiveBurst)
                _enrage = WorldState.FutureTime(26);
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        {
            if ((AID)spell.Action.ID == AID.MassiveBurst)
                _enrage = default;
        }
    }
}

class HypothermalMinion(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle Circle = new(6.6f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.GiddyBomb)
            _aoes.Add(new(Circle, actor.Position, default, WorldState.FutureTime(10)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.HypothermalCombustionMinionCast)
            _aoes.Clear();
    }
}

class D190TheGodfatherStates : StateMachineBuilder
{
    public D190TheGodfatherStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BossAdds>()
            .ActivateOnEnter<Flashthoom>()
            .ActivateOnEnter<Sap>()
            .ActivateOnEnter<ScaldingScoldingCleave>()
            .ActivateOnEnter<RemedyBombEnrage>()
            .ActivateOnEnter<MassiveBurstEnrage>()
            .ActivateOnEnter<HypothermalMinion>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 217, NameID = 5471)]
public class D190TheGodfather(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -300), new ArenaBoundsCircle(25));
