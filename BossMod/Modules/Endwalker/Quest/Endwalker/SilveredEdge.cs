namespace BossMod.Endwalker.Quest.Endwalker;

class SilveredEdge(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime _activation;
    private bool active;
    private bool casting;
    private Angle _rotation;
    private static readonly AOEShapeRect rect = new(40, 3);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (active && casting)
        {
            yield return new(rect, Module.PrimaryActor.Position, Angle.FromDirection(actor.Position - Module.PrimaryActor.Position), _activation);
            yield return new(rect, Module.PrimaryActor.Position, Angle.FromDirection(actor.Position - Module.PrimaryActor.Position) + 120.Degrees(), _activation);
            yield return new(rect, Module.PrimaryActor.Position, Angle.FromDirection(actor.Position - Module.PrimaryActor.Position) + 240.Degrees(), _activation);
        }
        if (active && !casting)
        {
            yield return new(rect, Module.PrimaryActor.Position, _rotation, _activation);
            yield return new(rect, Module.PrimaryActor.Position, _rotation + 120.Degrees(), _activation);
            yield return new(rect, Module.PrimaryActor.Position, _rotation + 240.Degrees(), _activation);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SilveredEdge)
        {
            active = true;
            casting = true;
            _activation = spell.NPCFinishAt.AddSeconds(1.4f);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SilveredEdge)
        {
            casting = false;
            _rotation = Angle.FromDirection(Raid.Player()!.Position - caster.Position);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SilveredEdgeVisual)
        {
            ++NumCasts;
            if (NumCasts == 3)
            {
                NumCasts = 0;
                active = false;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!casting)
            base.AddAIHints(slot, actor, assignment, hints);
    }
}
