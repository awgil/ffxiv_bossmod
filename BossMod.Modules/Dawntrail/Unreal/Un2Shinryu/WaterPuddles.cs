namespace BossMod.Dawntrail.Unreal.Un2Shinryu;

class WaterPuddles(BossModule module) : BossComponent(module)
{
    private const float Radius = 5;
    private DateTime _avoid;
    private DateTime _seek;
    private DateTime _stack;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.LevinboltVisual:
            case AID.JudgmentBolt:
                _avoid = Module.CastFinishAt(spell);
                break;
            case AID.Hellfire:
                _seek = Module.CastFinishAt(spell);
                break;
            case AID.HypernovaVisual:
                _stack = Module.CastFinishAt(spell, 0.1f);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.LevinboltVisual:
            case AID.JudgmentBolt:
                _avoid = default;
                break;
            case AID.Hellfire:
                _seek = default;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Hypernova)
            _stack = default;
    }

    private IEnumerable<Actor> Puddles() => Module.Enemies(OID.WaterPuddle).Where(p => !p.IsDead);
    private Actor? StackPuddle() => Puddles().MinBy(p => (p.Position - Arena.Center).LengthSq());

    private Actor? Target(Actor actor) => _stack != default ? StackPuddle() : _seek != default ? Puddles().MinBy(p => (p.Position - actor.Position).LengthSq()) : null;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_avoid != default)
        {
            foreach (var p in Puddles())
                hints.AddForbiddenZone(ShapeDistance.Circle(p.Position, Radius + 1), _avoid);
        }
        else if (Target(actor) is { } target)
        {
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(target.Position, Radius - 1.5f), _stack != default ? _stack : _seek);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var inWater = Puddles().Any(p => actor.Position.InCircle(p.Position, Radius));
        if (_avoid != default && inWater)
            hints.Add("Get out of water!");
        else if (_stack != default && StackPuddle() is { } puddle && !actor.Position.InCircle(puddle.Position, Radius))
            hints.Add("Stack in the water!");
        else if (_seek != default && !inWater && Puddles().Any())
            hints.Add("Stand in water!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var stackPuddle = _stack != default ? StackPuddle() : null;
        foreach (var p in Puddles())
        {
            if (_avoid != default)
                Arena.ZoneCircle(p.Position, Radius, ArenaColor.AOE);
            else if (_seek != default || p == stackPuddle)
                Arena.ZoneCircle(p.Position, Radius, ArenaColor.SafeFromAOE);
            else
                Arena.AddCircle(p.Position, Radius, ArenaColor.Object);
        }
    }
}
