namespace BossMod.Endwalker.Extreme.Ex7Zeromus;

// TODO: find out starting/ending radius, growth speed, etc
class BlackHole : BossComponent
{
    public Actor? Baiter;
    public Actor? Voidzone;
    private DateTime _growthStart;

    // TODO: verify...
    private static readonly float _startingRadius = 5;
    private static readonly float _maxRadius = 35;
    private static readonly float _growthPerSecond = 3.3f;

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (Baiter == actor)
        {
            if (module.Raid.WithoutSlot().InRadiusExcluding(actor, _startingRadius).Any())
                hints.Add("GTFO from raid!");
        }
        else if (Baiter != null)
        {
            if (actor.Position.InCircle(Baiter.Position, _startingRadius))
                hints.Add("GTFO from black hole baiter!");
        }
    }

    public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return player == Baiter ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (Voidzone != null)
            arena.ZoneCircle(Voidzone.Position, _growthStart == default ? _startingRadius : Math.Min(_maxRadius, _startingRadius + _growthPerSecond * (float)(module.WorldState.CurrentTime - _growthStart).TotalSeconds), ArenaColor.AOE);
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (Baiter != null)
            arena.AddCircle(Baiter.Position, _startingRadius, ArenaColor.Danger);
    }

    public override void OnActorCreated(BossModule module, Actor actor)
    {
        if ((OID)actor.OID == OID.BlackHole)
        {
            Baiter = null;
            Voidzone = actor;
        }
    }

    public override void OnActorEAnim(BossModule module, Actor actor, uint state)
    {
        if (actor == Voidzone)
        {
            switch (state)
            {
                // 00010002 - appear
                case 0x00100008:
                    _growthStart = module.WorldState.CurrentTime;
                    break;
                case 0x00040020:
                    Voidzone = null;
                    break;
            }
        }
    }

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.BlackHole)
            Baiter = actor;
    }
}

class FracturedEventide : Components.GenericAOEs
{
    private Actor? _source;
    private Angle _startingRotation;
    private Angle _increment;
    private DateTime _startingActivation;

    private static readonly AOEShapeRect _shape = new(60, 4);
    private static readonly int _maxCasts = 21;

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        if (_source == null)
            yield break;

        for (int i = NumCasts + 1; i < _maxCasts; ++i)
            yield return new(_shape, _source.Position, _startingRotation + i * _increment, _startingActivation.AddSeconds(0.5f * i));
        if (NumCasts < _maxCasts)
            yield return new(_shape, _source.Position, _startingRotation + NumCasts * _increment, _startingActivation.AddSeconds(0.5f * NumCasts), ArenaColor.Danger);
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.FracturedEventideAOEFirst)
        {
            _source = caster;
            _startingRotation = spell.Rotation;
            _increment = _startingRotation.Rad > 0 ? -7.Degrees() : 7.Degrees();
            _startingActivation = spell.NPCFinishAt;
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.FracturedEventideAOEFirst or AID.FracturedEventideAOERest)
            ++NumCasts;
    }
}
