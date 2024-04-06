namespace BossMod.Shadowbringers.Foray.Duel.Duel5Menenius;

abstract class GigaTempest : Components.Exaflare
{
    public GigaTempest(AOEShapeRect shape) : base(shape) { }

    public abstract bool IsStart(AID aid);
    public abstract bool IsMove(AID aid);

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if (IsStart((AID)spell.Action.ID))
        {
            WDir? advance = GetExaDirection(caster);
            if (advance == null) return;
            Lines.Add(new()
            {
                Next = caster.Position,
                Advance = advance.Value,
                NextExplosion = caster.CastInfo!.NPCFinishAt,
                TimeToMove = 0.9f,
                ExplosionsLeft = 5,
                MaxShownExplosions = 5,
                Rotation = caster.Rotation,
            });
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if (Lines.Count > 0 && IsStart((AID)spell.Action.ID) || IsMove((AID)spell.Action.ID))
        {
            int index = Lines.FindIndex(item => item.Next.AlmostEqual(caster.Position, 1));
            if (index < 0) return;
            AdvanceLine(module, Lines[index], caster.Position);
            if (Lines[index].ExplosionsLeft == 0)
                Lines.RemoveAt(index);
        }
    }

    // The Gigatempest caster's heading is only used for rotating the AOE shape.
    // The exaflare direction must be derived from the caster's location.
    private WDir? GetExaDirection(Actor caster)
    {
        Angle? forwardAngle = null;
        if (caster.Position.Z == 536)
        {
            forwardAngle = 180.Degrees();
        }
        if (caster.Position.Z == 504)
        {
            forwardAngle = 0.Degrees();
        }
        if (caster.Position.X == -826)
        {
            forwardAngle = 90.Degrees();
        }
        if (caster.Position.X == -794)
        {
            forwardAngle = 270.Degrees();
        }

        if (forwardAngle == null) return null;

        const float _advanceDistance = 8;
        return _advanceDistance * forwardAngle.Value.ToDirection();
    }
}

class SmallGigaTempest : GigaTempest
{
    private static readonly AOEShapeRect _aoeShapeSmall = new(10, 6.5f, 0);

    public SmallGigaTempest() : base(_aoeShapeSmall) { }

    public override bool IsStart(AID aid)
    {
        return aid is AID.GigaTempestSmallStart;
    }
    public override bool IsMove(AID aid)
    {
        return aid is AID.GigaTempestSmallMove;
    }

}

class LargeGigaTempest : GigaTempest
{
    private static readonly AOEShapeRect _aoeShapeLarge = new(35, 6.5f, 0);

    public LargeGigaTempest() : base(_aoeShapeLarge) { }

    public override bool IsStart(AID aid)
    {
        return aid is AID.GigaTempestLargeStart;
    }
    public override bool IsMove(AID aid)
    {
        return aid is AID.GigaTempestLargeMove;
    }
}
