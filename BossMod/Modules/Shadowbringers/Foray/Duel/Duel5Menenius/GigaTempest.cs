namespace BossMod.Shadowbringers.Foray.Duel.Duel5Menenius;

abstract class GigaTempest(BossModule module, AOEShapeRect shape, AID aidFirst, AID aidRest) : Components.Exaflare(module, shape)
{
    private readonly AID _aidStart = aidFirst;
    private readonly AID _aidRest = aidRest;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == _aidStart)
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

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (Lines.Count > 0 && (AID)spell.Action.ID == _aidStart || (AID)spell.Action.ID == _aidRest)
        {
            int index = Lines.FindIndex(item => item.Next.AlmostEqual(caster.Position, 1));
            if (index < 0)
                return;
            AdvanceLine(Lines[index], caster.Position);
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

class SmallGigaTempest(BossModule module) : GigaTempest(module, new AOEShapeRect(10, 6.5f), AID.GigaTempestSmallStart, AID.GigaTempestSmallMove);
class LargeGigaTempest(BossModule module) : GigaTempest(module, new AOEShapeRect(35, 6.5f), AID.GigaTempestLargeStart, AID.GigaTempestLargeMove);
