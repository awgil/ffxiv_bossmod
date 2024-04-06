namespace BossMod.Shadowbringers.Foray.Duel.Duel5Menenius;

class RuinationCross : Components.GenericAOEs
{
    private static readonly AOEShapeRect _aoeShape = new(20, 4, 20);
    private List<AOEInstance> _aoes = new();

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes;

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Ruination)
        {
            _aoes.Add(new(_aoeShape, caster.Position));
            _aoes.Add(new(_aoeShape, caster.Position, rotation: 90.Degrees()));
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Ruination)
        {
            _aoes.Clear();
        }
    }
}

class RuinationExaflare : Components.Exaflare
{
    public RuinationExaflare() : base(4) { }

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

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.RuinationExaStart)
            Lines.Add(new LineWithActor(caster));
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if (Lines.Count > 0 && (AID)spell.Action.ID is AID.RuinationExaStart or AID.RuinationExaMove)
        {
            int index = Lines.FindIndex(item => ((LineWithActor)item).Caster == caster);
            if (index < 0) return;
            AdvanceLine(module, Lines[index], caster.Position);
            if (Lines[index].ExplosionsLeft == 0)
                Lines.RemoveAt(index);
        }
    }
}
