namespace BossMod.Dawntrail.Alliance.A14ShadowLord;

class CthonicFury(BossModule module) : BossComponent(module)
{
    public bool Active;

    private static readonly List<RelTriangle> _triangulation = A14ShadowLord.NormalBounds.Clipper.Difference(new(A14ShadowLord.NormalBounds.ShapeSimplified), new(A14ShadowLord.ChtonicBounds.Poly)).Triangulate();

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Active && !A14ShadowLord.ChtonicBounds.Contains(actor.Position - Module.Center))
            hints.Add("GTFO from aoe!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Active)
            Arena.Zone(_triangulation, ArenaColor.AOE);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.CthonicFuryStart)
            Active = true;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.CthonicFuryStart:
                Active = false;
                Module.Arena.Bounds = A14ShadowLord.ChtonicBounds;
                break;
            case AID.CthonicFuryEnd:
                Module.Arena.Bounds = A14ShadowLord.NormalBounds;
                break;
        }
    }
}

class BurningCourtMoatKeepBattlements(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [];

    private static readonly AOEShape _shapeC = new AOEShapeCircle(8);
    private static readonly AOEShape _shapeM = new AOEShapeDonut(5, 15);
    private static readonly AOEShape _shapeK = new AOEShapeRect(11.5f, 11.5f, 11.5f);
    private static readonly AOEShape _shapeB = new AOEShapeCustom(BuildBattlementsPolygon());

    private static RelSimplifiedComplexPolygon BuildBattlementsPolygon()
    {
        RelPolygonWithHoles poly = new([.. CurveApprox.Rect(new(100, 0), new(0, 100))]);
        poly.AddHole(CurveApprox.Rect(new(11.5f, 0), new(0, 11.5f)));
        return new([poly]);
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var shape = ShapeForAction(spell.Action);
        if (shape != null)
            AOEs.Add(new(shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var shape = ShapeForAction(spell.Action);
        if (shape != null)
            AOEs.RemoveAll(aoe => aoe.Shape == shape && aoe.Origin.AlmostEqual(caster.Position, 1));
    }

    private AOEShape? ShapeForAction(ActionID aid) => (AID)aid.ID switch
    {
        AID.BurningCourt => _shapeC,
        AID.BurningMoat => _shapeM,
        AID.BurningKeep => _shapeK,
        AID.BurningBattlements => _shapeB,
        _ => null
    };
}

class DarkNebula(BossModule module) : Components.Knockback(module)
{
    public readonly List<Actor> Casters = [];

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        foreach (var caster in Casters.Take(2))
        {
            var dir = caster.CastInfo?.Rotation ?? caster.Rotation;
            var kind = dir.ToDirection().OrthoL().Dot(actor.Position - caster.Position) > 0 ? Kind.DirLeft : Kind.DirRight;
            yield return new(caster.Position, 20, Module.CastFinishAt(caster.CastInfo), null, dir, kind);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.DarkNebulaShort or AID.DarkNebulaLong)
            Casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.DarkNebulaShort or AID.DarkNebulaLong)
        {
            ++NumCasts;
            Casters.Remove(caster);
        }
    }
}

class EchoesOfAgony(BossModule module) : Components.StackWithIcon(module, (uint)IconID.EchoesOfAgony, AID.EchoesOfAgonyAOE, 5, 9.2f, 8)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.EchoesOfAgony)
            NumFinishedStacks = 0;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == StackAction)
        {
            if (++NumFinishedStacks >= 5)
            {
                Stacks.Clear();
            }
        }
    }
}
