namespace BossMod.Endwalker.Alliance.A32Llymlaen;

public class FrothingSea(BossModule module) : Components.Exaflare(module, new AOEShapeRect(6, 20, 80))
{
    private static readonly Angle _rot1 = 90.Degrees();
    private static readonly Angle _rot2 = -90.Degrees();

    public override void OnEventEnvControl(byte index, uint state)
    {
        var _activation = WorldState.FutureTime(30);
        if (state == 0x00800040 && index == 0x49)
            Lines.Add(new() { Next = new(-80, -900), Advance = 2.2f * _rot1.ToDirection(), NextExplosion = _activation, TimeToMove = 0.9f, ExplosionsLeft = 13, MaxShownExplosions = 2, Rotation = _rot1 });
        if (state == 0x08000400 && index == 0x49)
            Lines.Add(new() { Next = new(80, -900), Advance = 2.2f * _rot2.ToDirection(), NextExplosion = _activation, TimeToMove = 0.9f, ExplosionsLeft = 13, MaxShownExplosions = 2, Rotation = _rot2 });
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (Lines.Count > 0 && (AID)spell.Action.ID == AID.FrothingSeaRectAOE)
        {
            ++NumCasts;
            AdvanceLine(Lines[0], Lines[0].Next + 2.2f * Lines[0].Rotation.ToDirection());
            if (Lines[0].ExplosionsLeft == 0)
                Lines.RemoveAt(0);
        }
    }
}
