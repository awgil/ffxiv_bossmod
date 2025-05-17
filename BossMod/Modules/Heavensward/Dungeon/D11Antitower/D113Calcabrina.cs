namespace BossMod.Heavensward.Dungeon.D11Antitower.D113Calcabrina;

public enum OID : uint
{
    Boss = 0x1502, // R2.250, x1
    Helper = 0x233C, // R0.500, x10, Helper type
    Brina = 0x15E1, // R0.900, x0 (spawn during fight)
    Calca = 0x15E0, // R0.900, x0 (spawn during fight)
    Brina1 = 0x15E3, // R0.900, x0 (spawn during fight)
    Calca1 = 0x15E2, // R0.900, x0 (spawn during fight)
}

public enum AID : uint
{
    TerrifyingGlance = 5559, // Boss->self, no cast, range 40+R ?-degree cone
    Knockout = 5556, // Boss->player, 4.0s cast, single-target
    Brace = 5557, // Boss->self, 3.0s cast, single-target
    Breach = 5558, // Helper->player, no cast, single-target
    Dollhouse = 5561, // Boss->self, 3.0s cast, ???
    Slapstick = 5560, // Boss->self, no cast, range 40 circle
    HeatGazeDonut = 5552, // 15E1/15E3->self, 3.0s cast, range 10 circle
    HeatGazeCone = 5551, // 15E0/15E2->self, 3.0s cast, range 19+R 60-degree cone
}

public enum IconID : uint
{
    TerrifyingGlance = 73, // player
}

class HeatGazeDonut(BossModule module) : Components.StandardAOEs(module, AID.HeatGazeDonut, new AOEShapeDonut(5, 10));
class HeatGazeCone(BossModule module) : Components.StandardAOEs(module, AID.HeatGazeCone, new AOEShapeCone(19.9f, 30.Degrees()));
class Knockout(BossModule module) : Components.SingleTargetCast(module, AID.Knockout);
class Brace(BossModule module) : Components.DirectionalParry(module, (uint)OID.Boss)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Brace)
            PredictParrySide(caster.InstanceID, Side.All ^ Side.Front);
    }
}

class TerrifyingGlance(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(40, 60.Degrees()), (uint)IconID.TerrifyingGlance, AID.TerrifyingGlance, 3.5f)
{
    private bool WillBeHit(Actor actor) => CurrentBaits.Any(b => b.Target == actor || IsClippedBy(actor, b));
    private bool WillBeGazed(Actor actor) => WillBeHit(actor) && actor.Rotation.ToDirection().Dot((CurrentBaits[0].Source.Position - actor.Position).Normalized()) >= 0.707107f;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (WillBeHit(actor))
            hints.ForbiddenDirections.Add((actor.AngleTo(CurrentBaits[0].Source), 45.Degrees(), CurrentBaits[0].Activation));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (WillBeGazed(actor))
            hints.Add("Turn away from gaze!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (CurrentBaits.Count > 0)
        {
            Components.GenericGaze.DrawEye(Module.Arena.WorldPositionToScreenPosition(CurrentBaits[0].Source.Position), WillBeGazed(pc));

            if (WillBeHit(pc))
            {
                Arena.PathArcTo(pc.Position, 1, (pc.Rotation + 45.Degrees()).Rad, (pc.Rotation - 45.Degrees()).Rad);
                Arena.PathStroke(false, ArenaColor.Enemy);
            }
        }
    }
}

class CalcabrinaStates : StateMachineBuilder
{
    public CalcabrinaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Brace>()
            .ActivateOnEnter<TerrifyingGlance>()
            .ActivateOnEnter<Knockout>()
            .ActivateOnEnter<HeatGazeDonut>()
            .ActivateOnEnter<HeatGazeCone>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 141, NameID = 4813, Contributors = "xan")]
public class Calcabrina(WorldState ws, Actor primary) : BossModule(ws, primary, new(232, -182), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
            h.Priority = (OID)h.Actor.OID == OID.Boss ? 0 : 1;
    }

    protected override bool CheckPull() => PrimaryActor.InCombat;
}
