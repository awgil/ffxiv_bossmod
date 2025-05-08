namespace BossMod.Stormblood.Quest.RhalgrsBeacon;

public enum OID : uint
{
    Boss = 0x1A88,
    Helper = 0x233C,
    TerminusEst = 0x1BCA,
    MarkXLIIIArtilleryCannon = 0x1B4A, // R2.000, x3
    SkullsSpear = 0x1A8C, // R0.500, x3
    SkullsBlade = 0x1A8B, // R0.500, x3
    MagitekTurretII = 0x1BC7, // R0.600, x0 (spawn during fight)
    ChoppingBlock = 0x1EA4D9, // R0.500, x0 (spawn during fight), voidzone event object
}

public enum AID : uint
{
    TheOrder = 8370, // Boss->self, 3.0s cast, single-target
    TerminusEst1 = 8337, // 1BCA->self, no cast, range 40+R width 4 rect
    Gunblade = 8310, // Boss->player, 5.0s cast, single-target, 10y knockback
    DiffractiveLaser = 8340, // 1BC7->self, 2.5s cast, range 18+R 60-degree cone
    ChoppingBlock1 = 8346, // 1A57->location, 3.0s cast, range 5 circle
}

class DiffractiveLaser(BossModule module) : Components.StandardAOEs(module, AID.DiffractiveLaser, new AOEShapeCone(18.6f, 30.Degrees()));

class TerminusEst(BossModule module) : Components.GenericAOEs(module, AID.TheOrder)
{
    private readonly List<Actor> Termini = [];
    private DateTime? CastFinish;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(Module.Enemies(OID.TerminusEst).Where(x => !x.IsDead), ArenaColor.Object, true);
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var t in Termini)
            yield return new AOEInstance(new AOEShapeRect(41f, 2), t.Position, t.Rotation, Activation: CastFinish ?? WorldState.FutureTime(10));
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.TerminusEst)
            Termini.Add(actor);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            CastFinish = Module.CastFinishAt(spell);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.TerminusEst1)
            Termini.Remove(caster);
    }
}

class Gunblade(BossModule module) : Components.Knockback(module, AID.Gunblade, stopAtWall: true)
{
    public readonly List<Actor> Casters = [];

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var caster = Casters.FirstOrDefault();
        if (caster == null)
            return;

        var voidzones = Module.Enemies(OID.ChoppingBlock).Where(x => x.EventState != 7).Select(v => ShapeContains.Circle(v.Position, 5)).ToList();
        if (voidzones.Count == 0)
            return;

        var combined = ShapeContains.Union(voidzones);

        bool projectedDist(WPos pos)
        {
            var direction = (pos - caster.Position).Normalized();
            var projected = pos + 10 * direction;
            return combined(projected);
        }

        hints.AddForbiddenZone(projectedDist, Module.CastFinishAt(caster.CastInfo));
    }

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        foreach (var c in Casters)
            yield return new(c.Position, 10, Module.CastFinishAt(c.CastInfo));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Casters.Remove(caster);
    }
}

class ChoppingBlock(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 5, AID.ChoppingBlock1, m => m.Enemies(OID.ChoppingBlock).Where(x => x.EventState != 7), 0);

class FordolaRemLupisStates : StateMachineBuilder
{
    public FordolaRemLupisStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Gunblade>()
            .ActivateOnEnter<TerminusEst>()
            .ActivateOnEnter<DiffractiveLaser>()
            .ActivateOnEnter<ChoppingBlock>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68064, NameID = 5953)]
public class FordolaRemLupis(WorldState ws, Actor primary) : BossModule(ws, primary, new(-195.25f, 147.5f), new ArenaBoundsCircle(20));
