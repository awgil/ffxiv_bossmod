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
            if (Raid.WithoutSlot().InRadiusExcluding(actor, _startingRadius).Any())
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
            Arena.ZoneCircle(Voidzone.Position, _growthStart == default ? _startingRadius : Math.Min(_maxRadius, _startingRadius + _growthPerSecond * (float)(WorldState.CurrentTime - _growthStart).TotalSeconds), ArenaColor.AOE);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Baiter != null)
            Arena.AddCircle(Baiter.Position, _startingRadius, ArenaColor.Danger);
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

    public override void OnEventIcon(Actor actor, uint iconID)
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

    private static readonly AOEShapeRect _shape = new(60, 4);
    private const int _maxCasts = 21;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_source == null)
            yield break;

        for (int i = NumCasts + 1; i < _maxCasts; ++i)
            yield return new(_shape, _source.Position, _startingRotation + i * _increment, _startingActivation.AddSeconds(0.5f * i));
        if (NumCasts < _maxCasts)
            yield return new(_shape, _source.Position, _startingRotation + NumCasts * _increment, _startingActivation.AddSeconds(0.5f * NumCasts), ArenaColor.Danger);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.FracturedEventideAOEFirst)
        {
            _source = caster;
            _startingRotation = spell.Rotation;
            _increment = _startingRotation.Rad > 0 ? -7.Degrees() : 7.Degrees();
            _startingActivation = spell.NPCFinishAt;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.FracturedEventideAOEFirst or AID.FracturedEventideAOERest)
            ++NumCasts;
    }
}
