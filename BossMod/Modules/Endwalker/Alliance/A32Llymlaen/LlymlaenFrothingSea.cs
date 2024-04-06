namespace BossMod.Endwalker.Alliance.A32Llymlaen;

public class FrothingSea : Components.Exaflare
{
    private static readonly Angle _rot1 = 90.Degrees();
    private static readonly Angle _rot2 = -90.Degrees();

    public FrothingSea() : base(new AOEShapeRect(6, 20, 80)) { }

    public override void OnEventEnvControl(BossModule module, byte index, uint state)
    {
        var _activation = module.WorldState.CurrentTime.AddSeconds(0.15f);
        if (state == 0x00800040 && index == 0x49)
            Lines.Add(new() { Next = new(-80, -900), Advance = 2.2f * _rot1.ToDirection(), NextExplosion = _activation, TimeToMove = 0.9f, ExplosionsLeft = 13, MaxShownExplosions = 2, Rotation = _rot1 });
        if (state == 0x08000400 && index == 0x49)
            Lines.Add(new() { Next = new(80, -900), Advance = 2.2f * _rot2.ToDirection(), NextExplosion = _activation, TimeToMove = 0.9f, ExplosionsLeft = 13, MaxShownExplosions = 2, Rotation = _rot2 });
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if (Lines.Count > 0 && (AID)spell.Action.ID == AID.FrothingSeaRectAOE)
        {
            ++NumCasts;
            AdvanceLine(module, Lines[0], Lines[0].Next + 2.2f * Lines[0].Rotation.ToDirection());
            if (Lines[0].ExplosionsLeft == 0)
                Lines.RemoveAt(0);
        }
    }
}
