namespace BossMod.Endwalker.Quest.Endwalker;

class SilveredEdge : Components.GenericAOEs
{
    private DateTime _activation;
    private bool active;
    private bool casting;
    private Angle _rotation;
    private static readonly AOEShapeRect rect = new(40, 3);

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        if (active && casting)
        {
            yield return new(rect, module.PrimaryActor.Position, Angle.FromDirection(actor.Position - module.PrimaryActor.Position), _activation);
            yield return new(rect, module.PrimaryActor.Position, Angle.FromDirection(actor.Position - module.PrimaryActor.Position) + 120.Degrees(), _activation);
            yield return new(rect, module.PrimaryActor.Position, Angle.FromDirection(actor.Position - module.PrimaryActor.Position) + 240.Degrees(), _activation);
        }
        if (active && !casting)
        {
            yield return new(rect, module.PrimaryActor.Position, _rotation, _activation);
            yield return new(rect, module.PrimaryActor.Position, _rotation + 120.Degrees(), _activation);
            yield return new(rect, module.PrimaryActor.Position, _rotation + 240.Degrees(), _activation);
        }
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SilveredEdge)
        {
            active = true;
            casting = true;
            _activation = spell.NPCFinishAt.AddSeconds(1.4f);
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SilveredEdge)
        {
            casting = false;
            _rotation = Angle.FromDirection(module.Raid.Player()!.Position - caster.Position);
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
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

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!casting)
            base.AddAIHints(module, slot, actor, assignment, hints);
    }
}
