namespace BossMod.Endwalker.Criterion.C03AAI.C033Statice;

// dartboard layout:
// - inner/outer rings split is at radius 12
// - there are 12 segments total, meaning 30 degrees per segment
// - starting from S (0deg) and CCW (increasing angle), colors are red->blue->yellow on outer segments and yellow->red->blue on inner segments
class Dartboard(BossModule module) : BossComponent(module)
{
    public enum Color { None, Red, Blue, Yellow }

    public int NumCasts { get; private set; }
    public Color ForbiddenColor { get; private set; }
    public BitMask Bullseye;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Bullseye[slot] || ForbiddenColor != Color.None)
        {
            var color = PosToColor(actor.Position);
            if (color == ForbiddenColor)
                hints.Add("GTFO from forbidden color!");
            else if (Bullseye[slot] && Raid.WithSlot(true).Exclude(actor).WhereSlot(i => Bullseye[i]).WhereActor(p => PosToColor(p.Position) == color).Any())
                hints.Add("Stay on different segments!");
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => Bullseye[playerSlot] ? PlayerPriority.Danger : PlayerPriority.Interesting;

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (ForbiddenColor != Color.None)
            DrawSegmentsOfColor(ForbiddenColor, ArenaColor.AOE);
        if (Bullseye[pcSlot])
        {
            var color = PosToColor(pc.Position);
            if (color != ForbiddenColor)
                DrawSegmentsOfColor(color, ArenaColor.SafeFromAOE);
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.NHomingPattern or OID.SHomingPattern)
            ForbiddenColor = PosToColor(actor.Position);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.BullsEye)
            Bullseye.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NUncommonGroundSuccess or AID.NUncommonGroundFail or AID.SUncommonGroundSuccess or AID.SUncommonGroundFail)
        {
            Bullseye.Reset();
            ++NumCasts;
        }
    }

    public Color DirToColor(Angle dir, bool inner)
    {
        var segIndex = (int)Math.Floor(dir.Deg / 30);
        if (inner)
            --segIndex;
        if (segIndex < 0)
            segIndex += 9;
        return Color.Red + segIndex % 3;
    }

    private Color PosToColor(WPos pos)
    {
        var off = pos - Module.Center;
        return DirToColor(Angle.FromDirection(off), off.LengthSq() < 144);
    }

    private void DrawSegmentsOfColor(Color color, uint zoneColor)
    {
        int index = (int)color - 1;
        var dirOut = (15 + index * 30).Degrees();
        for (int i = 0; i < 4; ++i)
        {
            Arena.ZoneCone(Module.Center, 0, 12, dirOut + 30.Degrees(), 15.Degrees(), zoneColor);
            Arena.ZoneCone(Module.Center, 12, Module.Bounds.Radius, dirOut, 15.Degrees(), zoneColor);
            dirOut += 90.Degrees();
        }
    }
}
