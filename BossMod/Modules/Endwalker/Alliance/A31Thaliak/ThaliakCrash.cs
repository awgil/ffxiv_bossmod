namespace BossMod.Endwalker.Alliance.A31Thaliak;
public class RheognosisCrash : Components.Exaflare
{
    private static readonly Angle _rot1 = 90.Degrees();
    private static readonly Angle _rot2 = -90.Degrees();
    public RheognosisCrash() : base(new AOEShapeRect(24.01f, 12))  { } //actually the rect is only 24, but there seem to be a few thousandth of variation in the location

    public override void OnEventEnvControl(BossModule module, byte index, uint state)
    {
        var _activation = module.WorldState.CurrentTime.AddSeconds(0.15f);
        if (state == 0x01000001)
        {
            if (index == 0x00)
                Lines.Add(new() { Next = new(-969, 957), Advance = 10 * _rot1.ToDirection(), NextExplosion = _activation, TimeToMove = 0.15f, ExplosionsLeft = 5, MaxShownExplosions = 5, Rotation = _rot1 });
            if (index == 0x01)
                Lines.Add(new() { Next = new(-921, 933), Advance = 10 * _rot2.ToDirection(), NextExplosion = _activation, TimeToMove = 0.15f, ExplosionsLeft = 5, MaxShownExplosions = 5, Rotation = _rot2 });
        }
        if (state == 0x02000001)
        {
            if (index == 0x00)
                Lines.Add(new() { Next = new(-969, 933), Advance = 10 * _rot1.ToDirection(), NextExplosion = _activation, TimeToMove = 0.15f, ExplosionsLeft = 5, MaxShownExplosions = 5, Rotation = _rot1 });
            if (index == 0x01)
                Lines.Add(new() { Next = new(-921, 957), Advance = 10 * _rot2.ToDirection(), NextExplosion = _activation, TimeToMove = 0.15f, ExplosionsLeft = 5, MaxShownExplosions = 5, Rotation = _rot2 });
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.RheognosisCrashExaflare)
        {
            ++NumCasts;
            int index = Lines.FindIndex(item => item.Next.AlmostEqual(caster.Position, 1));
            AdvanceLine(module, Lines[index], caster.Position);
            if (Lines[index].ExplosionsLeft == 0)
                Lines.RemoveAt(index);
        }
    }
}
