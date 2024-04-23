namespace BossMod.Shadowbringers.Foray.Duel.Duel5Menenius;

class RuinationCross(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect _aoeShape = new(20, 4, 20);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Ruination)
        {
            _aoes.Add(new(_aoeShape, caster.Position));
            _aoes.Add(new(_aoeShape, caster.Position, 90.Degrees()));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Ruination)
        {
            _aoes.Clear();
        }
    }
}

class RuinationExaflare(BossModule module) : Components.Exaflare(module, 4)
{
    class LineWithActor : Line
    {
        public Actor Caster;

        public LineWithActor(Actor caster)
        {
            Next = caster.Position;
            Advance = 4 * caster.Rotation.ToDirection();
            NextExplosion = caster.CastInfo!.NPCFinishAt;
            TimeToMove = 1.1f;
            ExplosionsLeft = 6;
            MaxShownExplosions = 7;
            Caster = caster;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.RuinationExaStart)
            Lines.Add(new LineWithActor(caster));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (Lines.Count > 0 && (AID)spell.Action.ID is AID.RuinationExaStart or AID.RuinationExaMove)
        {
            int index = Lines.FindIndex(item => ((LineWithActor)item).Caster == caster);
            if (index < 0) return;
            AdvanceLine(Lines[index], caster.Position);
            if (Lines[index].ExplosionsLeft == 0)
                Lines.RemoveAt(index);
        }
    }
}
