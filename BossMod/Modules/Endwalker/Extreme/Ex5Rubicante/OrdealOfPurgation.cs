namespace BossMod.Endwalker.Extreme.Ex5Rubicante;

// mechanic implementation:
// 1. all three circles gain a status that determines their pattern; this pattern is relative to actor's direction
//    inner can be one of three patterns (either single straight line in actor's main direction, or two straight lines - one in actor's direction, another at -90/180 degrees)
//    middle is always one pattern (four straight lines at direction, +-45 and 180, then two curved lines starting at +-135 and leading to +-90)
//    outer is always one pattern (all 8 straight lines)
// 2. 8 actors (triangle/square) get PATE 11D1; they are all in center, but face their visual position
// 3. we get envcontrols for rotations (at least one for each mechanic instance; from this point we can start showing aoes)
// 4. at the same time, all players get 12s penance debuff, which is a deadline to resolve
// 5. right after that, boss starts casting visual cast - at this point we start showing the mechanic
// 5. penance expires and is replaced with 9s shackles debuff, this happens right before cast end
class OrdealOfPurgation : Components.GenericAOEs
{
    public enum Symbol { Unknown, Tri, Sq }

    private int _dirInner;
    private int _dirInnerExtra; // == dirInner if there is only 1 fireball
    private int _midIncrement; // inner with this index is incremented by one (rotated CCW) when passing middle ring
    private int _midDecrement; // inner with this index is decremented by one (rotated CW) when passing middle ring
    private int _rotationOuter;
    private Symbol[] _symbols = new Symbol[8];
    private DateTime _activation;

    private static readonly AOEShapeCone _shapeTri = new(60, 30.Degrees());
    private static readonly AOEShapeRect _shapeSq = new(20, 40);

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        if (_activation != default)
        {
            if (AOEFromDirection(module, _dirInner) is var aoe1 && aoe1 != null)
                yield return aoe1.Value;
            if (_dirInnerExtra != _dirInner && AOEFromDirection(module, _dirInnerExtra) is var aoe2 && aoe2 != null)
                yield return aoe2.Value;
        }
    }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Pattern)
        {
            switch ((OID)actor.OID)
            {
                case OID.CircleOfPurgatoryInner:
                    _dirInner = AngleToDirectionIndex(actor.Rotation);
                    _dirInnerExtra = status.Extra switch
                    {
                        0x21F => AngleToDirectionIndex(actor.Rotation - 90.Degrees()),
                        0x220 => AngleToDirectionIndex(actor.Rotation + 180.Degrees()),
                        _ => _dirInner
                    };
                    break;
                case OID.CircleOfPurgatoryMiddle:
                    _midIncrement = AngleToDirectionIndex(actor.Rotation - 135.Degrees());
                    _midDecrement = AngleToDirectionIndex(actor.Rotation + 135.Degrees());
                    break;
            }
        }
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.OrdealOfPurgation)
            _activation = spell.NPCFinishAt; // note: actual activation is several seconds later, but we need to finish our movements before shackles, so effective activation is around cast end
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.FieryExpiationTri or AID.FieryExpiationSq)
            ++NumCasts;
    }

    public override void OnActorPlayActionTimelineEvent(BossModule module, Actor actor, ushort id)
    {
        var symbol = (OID)actor.OID switch
        {
            OID.CircleOfPurgatoryTriange => Symbol.Tri,
            OID.CircleOfPurgatorySquare => Symbol.Sq,
            _ => Symbol.Unknown
        };
        if (symbol != Symbol.Unknown && id == 0x11D1)
        {
            var dir = AngleToDirectionIndex(actor.Rotation);
            if (_symbols[dir] != Symbol.Unknown)
                module.ReportError(this, $"Duplicate symbols at {dir}");
            _symbols[dir] = symbol;
        }
    }

    public override void OnEventEnvControl(BossModule module, byte index, uint state)
    {
        var dir = state switch
        {
            0x00020001 => -1,
            0x00200010 => +1,
            _ => 0 // 0x00080004 = remove rotation markers
        };
        if (dir != 0)
        {
            switch (index)
            {
                case 1:
                    _dirInner = NormalizeDirectionIndex(_dirInner + dir);
                    _dirInnerExtra = NormalizeDirectionIndex(_dirInnerExtra + dir);
                    break;
                case 2:
                    _midIncrement = NormalizeDirectionIndex(_midIncrement + dir);
                    _midDecrement = NormalizeDirectionIndex(_midDecrement + dir);
                    break;
                case 3:
                    _rotationOuter = dir;
                    break;
            }
        }
    }

    // 0 is N, then increases in CCW order
    private int NormalizeDirectionIndex(int index) => index & 7;
    private int AngleToDirectionIndex(Angle rotation) => NormalizeDirectionIndex((int)(Math.Round(rotation.Deg / 45) + 4));
    private Angle DirectionIndexToAngle(int index) => (index - 4) * 45.Degrees();

    private int TransformByMiddle(int index)
    {
        if (index == _midIncrement)
            return NormalizeDirectionIndex(index + 1);
        else if (index == _midDecrement)
            return NormalizeDirectionIndex(index - 1);
        else
            return index;
    }

    private AOEShape? ShapeAtDirection(int index) => _symbols[NormalizeDirectionIndex(index - _rotationOuter)] switch
    {
        Symbol.Tri => _shapeTri,
        Symbol.Sq => _shapeSq,
        _ => null
    };

    private AOEInstance? AOEFromDirection(BossModule module, int index)
    {
        index = TransformByMiddle(index);
        var shape = ShapeAtDirection(index);
        var dir = DirectionIndexToAngle(index);
        return shape != null ? new(shape, module.Bounds.Center + module.Bounds.HalfSize * dir.ToDirection(), dir + 180.Degrees(), _activation) : null;
    }
}
