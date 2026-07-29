namespace BossMod.Endwalker.Extreme.Ex7Zeromus;

// TODO: find out starting/ending radius, growth speed, etc
class BlackHole(BossModule module) : BossComponent(module)
{
    public Actor? Baiter;
    public Actor? Voidzone;
    private DateTime _growthStart;

    // TODO: verify...
    private const float _startingRadius = 5;
    private const float _maxRadius = 35;
    private const float _growthPerSecond = 3.3f;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Baiter == actor)
        {
            if (Raid.WithoutSlot(false, true, true).InRadiusExcluding(actor, _startingRadius).Any())
                hints.Add("GTFO from raid!");
        }
        else if (Baiter != null)
        {
            if (actor.Position.InCircle(Baiter.Position, _startingRadius))
                hints.Add("GTFO from black hole baiter!");
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return player == Baiter ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Voidzone != null)
            Arena.ZoneCircle(Voidzone.Position, _growthStart == default ? _startingRadius : Math.Min(_maxRadius, _startingRadius + _growthPerSecond * (float)(WorldState.CurrentTime - _growthStart).TotalSeconds), Colors.AOE);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Baiter != null)
            Arena.ZoneCircleOutline(Baiter.Position, _startingRadius, Colors.Danger);
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.BlackHole)
        {
            Baiter = null;
            Voidzone = actor;
        }
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor == Voidzone)
        {
            switch (state)
            {
                // 00010002 - appear
                case 0x00100008:
                    _growthStart = WorldState.CurrentTime;
                    break;
                case 0x00040020:
                    Voidzone = null;
                    break;
            }
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.BlackHole)
            Baiter = actor;
    }
}

class FracturedEventide(BossModule module) : Components.GenericAOEs(module)
{
    private Actor? _source;
    private Angle _startingRotation;
    private Angle _increment;
    private DateTime _startingActivation;

    private static readonly AOEShapeRect _shape = new(60f, 4f);
    private const int _maxCasts = 21;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_source == null)
            return [];

        var count = _maxCasts - (NumCasts + 1) + (NumCasts < _maxCasts ? 1 : 0);
        if (count <= 0)
            return [];

        var aoes = new AOEInstance[count];
        var index = 0;

        for (var i = NumCasts + 1; i < _maxCasts; ++i)
            aoes[index++] = new(_shape, _source.Position, _startingRotation + i * _increment, _startingActivation.AddSeconds(0.5d * i));

        if (NumCasts < _maxCasts)
            aoes[index++] = new(_shape, _source.Position, _startingRotation + NumCasts * _increment, _startingActivation.AddSeconds(0.5d * NumCasts), Colors.Danger);
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FracturedEventideAOEFirst)
        {
            _source = caster;
            _startingRotation = spell.Rotation;
            _increment = _startingRotation.Rad > 0f ? -7f.Degrees() : 7f.Degrees();
            _startingActivation = Module.CastFinishAt(spell);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.FracturedEventideAOEFirst or (uint)AID.FracturedEventideAOERest)
            ++NumCasts;
    }
}
