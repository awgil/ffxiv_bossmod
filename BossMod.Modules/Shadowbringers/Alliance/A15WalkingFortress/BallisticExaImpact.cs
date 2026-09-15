namespace BossMod.Shadowbringers.Alliance.A15WalkingFortress;

class BallisticExaImpact(BossModule module) : Components.Exaflare(module, new AOEShapeRect(15, 10))
{
    private readonly List<Line> _toAdd = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var delay = (AID)spell.Action.ID switch
        {
            AID.BallisticImpactIndicator1 => 1,
            AID.BallisticImpactIndicator2 => 3.5f,
            AID.BallisticImpactIndicator3 => 6,
            _ => 0
        };
        if (delay > 0)
            _toAdd.Add(new()
            {
                Next = caster.Position,
                Advance = new WDir(0, 8.7f).Rotate(caster.Rotation),
                Rotation = caster.Rotation,
                NextExplosion = Module.CastFinishAt(spell),
                TimeToMove = 0.7f,
                ExplosionsLeft = 7,
                MaxShownExplosions = 4
            });
    }

    public override void Update()
    {
        for (var i = _toAdd.Count - 1; i >= 0; --i)
            if (_toAdd[i].NextExplosion < WorldState.FutureTime(3))
            {
                Lines.Add(_toAdd[i]);
                _toAdd.RemoveAt(i);
            }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.BallisticImpactExa)
        {
            var ix = Lines.FindIndex(l => l.Next.AlmostEqual(caster.Position, 1));
            if (ix >= 0)
                AdvanceLine(Lines[ix], caster.Position);
        }
    }
}
