namespace BossMod.Dawntrail.Savage.RM10STheXtremes;

enum Color
{
    None,
    Red,
    Blue
}

enum Mechanic
{
    None,
    Stack,
    Spread,
    Tankbuster
}

record struct Source(Color Color, Mechanic Mechanic, WPos Origin);

static class AirHelpers
{
    public static Source? ProcessEffect(byte index, uint state)
    {
        if (index is >= 0x0E and <= 0x16)
        {
            var cm = state switch
            {
                0x00020001 => (Color.Blue, Mechanic.Spread),
                0x00200010 => (Color.Blue, Mechanic.Stack),
                0x00800040 => (Color.Blue, Mechanic.Tankbuster),
                0x02000100 => (Color.Red, Mechanic.Spread),
                0x08000400 => (Color.Red, Mechanic.Stack),
                0x20001000 => (Color.Red, Mechanic.Tankbuster),
                _ => default
            };
            if (cm.Item1 != default)
            {
                var ix = index - 0x0E;
                var col = ix % 3;
                var row = ix / 3;
                var pos = new WPos(87 + col * 13, 87 + row * 13);

                return new(cm.Item1, cm.Item2, pos);
            }
        }

        return null;
    }
}

class AirBaits(BossModule module) : Components.UntelegraphedBait(module)
{
    private int _numIcons;
    public record struct Source(WPos Origin, DateTime Activation, Color Color, Mechanic Mechanic);
    protected readonly List<Source> Sources = [];

    public static readonly AOEShapeCone Cone = new(60, 22.5f.Degrees());

    private int _blueCounter;
    private int _redCounter;

    public int NumRedCasts { get; private set; }
    public int NumBlueCasts { get; private set; }

    public override void OnMapEffect(byte index, uint state)
    {
        if (AirHelpers.ProcessEffect(index, state) is { } source)
        {
            _numIcons++;

            Source s = new(source.Origin, WorldState.FutureTime(8.6f), source.Color, source.Mechanic);
            if (_numIcons <= 2)
                CurrentBaits.Add(DetermineBait(s));
            else
                Sources.Add(s);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Weaponskill_ReEntryBlast1:
            case AID._Weaponskill_VerticalBlast1:
                NumCasts++;
                NumRedCasts++;
                Advance();
                break;
            case AID._Weaponskill_ReEntryPlunge1:
            case AID._Weaponskill_VerticalPlunge1:
                NumCasts++;
                NumBlueCasts++;
                Advance();
                break;
            case AID._Weaponskill_PlungingSnap1:
                if (++_blueCounter > 3)
                {
                    NumCasts++;
                    NumBlueCasts++;
                    Advance();
                    _blueCounter = 0;
                }
                break;
            case AID._Weaponskill_BlastingSnap1:
                if (++_redCounter > 3)
                {
                    NumCasts++;
                    NumRedCasts++;
                    Advance();
                    _redCounter = 0;
                }
                break;
        }
    }

    void Advance()
    {
        if (NumCasts % 2 == 0)
        {
            CurrentBaits.Clear();
            CurrentBaits.AddRange(Sources.Take(2).Select(DetermineBait));
            if (Sources.Count > 1)
                Sources.RemoveRange(0, 2);
        }
    }

    protected virtual Bait DetermineBait(Source src)
    {
        return src.Mechanic switch
        {
            Mechanic.Spread => new(src.Origin, default, Cone, src.Activation, count: 4, damageType: AIHints.PredictedDamageType.Raidwide, isProximity: true),
            Mechanic.Stack => new(src.Origin, default, Cone, src.Activation, count: 1, stackSize: 4, damageType: AIHints.PredictedDamageType.Shared, isProximity: true),
            Mechanic.Tankbuster => new(src.Origin, default, new AOEShapeCircle(6), src.Activation, count: 1, damageType: AIHints.PredictedDamageType.Tankbuster, forbiddenTargets: Raid.WithSlot().WhereActor(a => a.Role != Role.Tank).Mask(), isProximity: true, centerAtTarget: true),
            _ => throw new Exception($"unknown mechanic type {src.Mechanic}"),
        };
    }
}

class AirPuddleCone(BossModule module) : FlamePuddle(module, [AID._Weaponskill_BlastingSnap1, AID._Weaponskill_ReEntryBlast1], new AOEShapeCone(60, 22.5f.Degrees()), OID.FlameCone);
class AirPuddleCircle(BossModule module) : FlamePuddle(module, AID._Weaponskill_VerticalBlast1, new AOEShapeCircle(6), OID.FlamePuddle6, originAtTarget: true);

class Air2Baits(BossModule module) : AirBaits(module)
{
    public BitMask Red;
    public BitMask Blue;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_XtremeFiresnaking)
            Red.Set(Raid.FindSlot(actor.InstanceID));
        if ((SID)status.ID == SID._Gen_XtremeWatersnaking)
            Blue.Set(Raid.FindSlot(actor.InstanceID));
    }
}
