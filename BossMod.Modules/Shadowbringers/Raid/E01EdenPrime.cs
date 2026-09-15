namespace BossMod.Shadowbringers.Raid.E01EdenPrime;

public enum OID : uint
{
    Boss = 0x2499, //Eden Prime
    GravityPuddle = 0x1EAE20, //GravityPuddle
    Helper = 0x233C, // R0.500, x16, Helper type
    GuardianOfParadise = 0x249A, // R3.000, x0 (Adds Phase)
    BlackHole = 0x249b, //Actually white
}

public enum AID : uint
{
    AutoAttack = 871, // Boss/249A->player, no cast, single-target
    SpearOfParadise = 15777, // Boss->player, 5.0s cast, single-target tankbuster
    Heavensunder = 15778, // Boss->player, no cast, single-target (follow-up tankbuster second hit, intended swap?)
    EdensGravity = 15764, // Boss->self, 4.0s cast, range 60 circle
    ViceOfApathy = 17640, // 233C->players, no cast, range 6 circle - actual damage of ViceAndVirtueGravity
    ViceAndVirtueGravity = 17637, // Boss->self, 6.0s cast, single-target - prepares gravity spread aoe on 4 players (role priority). Players not targetted take higher damage if hit.
    SunderPressure = 17663, // 233C->self, no cast, range 5 circle
    EdensFlare = 15767, // Boss->self, 4.0s cast, range 5-60 donut
    ViceAndVirtueLines = 17636, // Boss->self, 6.0s cast, single-target
    ViceOfVanity = 17639, // 233C->self, no cast, range 100+R width 6 rect - actual damage of ViceAndVirtueLines
    _Ability_ = 18051, // Boss->location, no cast, ???
    PureLight = 15779, // Boss->self, 10.5s cast, range 60 width 60 rect
    DeltaAttack = 17642, // Boss->self, 4.5s cast, single-target, casts several mechanics at once, which are all telegraphed individually.
    EdensBlizzardIII = 17644, // 233C->self, 5.0s cast, range 10 circle
    EdensFireIII = 17643, // 233C->players, 5.0s cast, range 5 circle
    EdensThunderIII = 17645, // 233C->self, 5.0s cast, range 40+R width 10 rect
    DimensionalShift = 15772, // Boss->self, 4.0s cast, range 60 circle
    PureBeam = 15774, // 233C->self, 3.0s cast, range 60 width 4 rect
    ParadiseLost = 15776, // 233C->location, 3.0s cast, range 6 circle
    FragorMaximus = 15780, // Boss->self, 5.0s cast, range 60 circle
    ParadisalDive = 15785, // 249A->self, 5.0s cast, range 60 circle
    ManaSlice = 15782, // 249A->self, no cast, range 6+R ?-degree cone - todo: figure out how to telegraph this.
    ManaBurst = 15783, // 249A->self, 4.0s cast, range 60 circle
    PrimevalStasis = 15930, // 249D->location, no cast, range 80 circle
    EternalBreath = 15781, // 233C->location, no cast, range 60 circle - unmitigable cutscene
}
public enum IconID : uint
{
    GravitySpread = 28, // player->self, Vice and Virtue damages players without this icon within spread radius
    FireIIISpread = 96, //5 spread during delta attack
}

public enum TetherID : uint
{
    ViceAndVirtue = 17, // player->Boss
}
class FragorMaximus(BossModule module) : Components.RaidwideCast(module, AID.FragorMaximus);
class ManaBurst(BossModule module) : Components.RaidwideCast(module, AID.ManaBurst);
class EdensGravity(BossModule module) : Components.RaidwideCast(module, AID.EdensGravity);
class DimensionalShift(BossModule module) : Components.RaidwideCast(module, AID.DimensionalShift);
class ParadisalDive(BossModule module) : Components.ProximityAOEs(module, AID.ParadisalDive, 20f);
class EdensFlare(BossModule module) : Components.StandardAOEs(module, AID.EdensFlare, new AOEShapeDonut(5f, 60f));
class EdensBlizzardIII(BossModule module) : Components.StandardAOEs(module, AID.EdensBlizzardIII, new AOEShapeCircle(10f));
class EdensFireIII(BossModule module) : Components.IconStackSpread(module, 0, (uint)IconID.FireIIISpread, null, AID.EdensFireIII, 0, 5, 0);
class EdensThunderIII(BossModule module) : Components.StandardAOEs(module, AID.EdensThunderIII, new AOEShapeRect(40f, 5f));
class PureBeam(BossModule module) : Components.StandardAOEs(module, AID.PureBeam, new AOEShapeRect(60f, 2f));
class ParadiseLost(BossModule module) : Components.StandardAOEs(module, AID.ParadiseLost, new AOEShapeCircle(6));
class ViceAndVirtueGravity(BossModule module) : Components.IconStackSpread(module, 0, (uint)IconID.GravitySpread, null, AID.ViceOfApathy, 0, 5, 3.2f)
{
    public DateTime Activation;
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        //Spread finishes when Vice and Virtue cast finishes.
        if (spell.Action.ID == (uint)AID.ViceAndVirtueGravity)
        {
            Activation = Module.CastFinishAt(spell);
        }
        base.OnCastStarted(caster, spell);
    }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (WorldState.CurrentTime < Activation)
        {
            //Don't drop void zone under boss.
            hints.AddForbiddenZone(ShapeDistance.Circle(Module.PrimaryActor.Position, 6f), Activation);
            //Spread further than the minimum 5 units to ensure there is a path to run between puddles.
            foreach (var spreadTarget in ActiveSpreadTargets)
            {
                hints.AddForbiddenZone(ShapeDistance.Circle(spreadTarget.Position, 6f), Activation);
            }
        }
        base.AddAIHints(slot, actor, assignment, hints);
    }
}

class ViceAndVirtueLines(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeRect(100f, 3f), (uint)TetherID.ViceAndVirtue, AID.ViceAndVirtueLines);
class GravityPuddle(BossModule module) : Components.VoidzoneAtCastTarget(module, 5, AID.ViceOfApathy, OID.GravityPuddle, 0);
class PureLight(BossModule module) : Components.StandardAOEs(module, AID.PureLight, new AOEShapeRect(60f, 60f));
class GuardianOfParadise(BossModule module) : Components.Adds(module, (uint)OID.GuardianOfParadise);
class ManaSlice(BossModule module) : Components.Cleave(module, AID.ManaSlice, new AOEShapeCone(15, 45.Degrees()), (uint)OID.GuardianOfParadise); // TODO: verify angle. Radius definitely larger than 6y, likely 10. 15 for safety
class SpearOfParadise(BossModule module) : Components.SingleTargetCast(module, AID.SpearOfParadise);

class DeltaAttack(BossModule module) : BossComponent(module)
{
    public DateTime activation;
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action.ID == (uint)AID.DeltaAttack)
        {
            activation = Module.CastFinishAt(spell);
        }
    }
    //AI tries to spread 8 people into one tiny corner of the arena. Someone needs to run out. This leaves 4 people in a northern corner where the boss will likely be, while the ranged head to the southern corners.
    //Sprint is necessary to make it; it's a long run.
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (WorldState.CurrentTime < activation)
        {
            hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Sprint), actor, ActionQueue.Priority.VeryHigh);
            switch (assignment)
            {
                case PartyRolesConfig.Assignment.MT:
                case PartyRolesConfig.Assignment.OT:
                case PartyRolesConfig.Assignment.M1:
                case PartyRolesConfig.Assignment.M2:
                    hints.AddForbiddenZone(ShapeDistance.Rect(new WPos(80, 110), new WPos(120, 110), 10f), activation); //don't go to south side.
                    break;
                case PartyRolesConfig.Assignment.R1:
                case PartyRolesConfig.Assignment.H1:
                    hints.AddForbiddenZone(ShapeDistance.Rect(new WPos(110, 80), new WPos(110, 120), 10f), activation); //don't go to east side.
                    hints.AddForbiddenZone(ShapeDistance.Rect(new WPos(80, 90), new WPos(120, 90), 10f), activation); //don't go to north side.
                    break;
                default:
                    hints.AddForbiddenZone(ShapeDistance.Rect(new WPos(90, 80), new WPos(90, 120), 10f), activation); //don't go to west side.
                    hints.AddForbiddenZone(ShapeDistance.Rect(new WPos(80, 90), new WPos(120, 90), 10f), activation); //don't go to north side.
                    break;
            }
        }
        base.AddAIHints(slot, actor, assignment, hints);
    }
}

class E01EdenPrimeStates : StateMachineBuilder
{
    public E01EdenPrimeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ViceAndVirtueGravity>()
            .ActivateOnEnter<GravityPuddle>()
            .ActivateOnEnter<ViceAndVirtueLines>()
            .ActivateOnEnter<PureLight>()
            .ActivateOnEnter<DeltaAttack>()
            .ActivateOnEnter<GuardianOfParadise>()
            .ActivateOnEnter<FragorMaximus>()
            .ActivateOnEnter<EdensGravity>()
            .ActivateOnEnter<ManaBurst>()
            .ActivateOnEnter<ManaSlice>()
            .ActivateOnEnter<DimensionalShift>()
            .ActivateOnEnter<ParadisalDive>()
            .ActivateOnEnter<EdensFlare>()
            .ActivateOnEnter<EdensBlizzardIII>()
            .ActivateOnEnter<EdensFireIII>()
            .ActivateOnEnter<EdensThunderIII>()
            .ActivateOnEnter<PureBeam>()
            .ActivateOnEnter<ParadiseLost>()
            .ActivateOnEnter<SpearOfParadise>();
    }
}

[ModuleInfo(Incomplete = false, Contributors = "AndMyAxe", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 653, NameID = 8345)]
public class E01EdenPrime(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(19.5f)); //death wall at ~20
