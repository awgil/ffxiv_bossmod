namespace BossMod.Dawntrail.Savage.RM04WickedThunder;

class Sabertail(BossModule module) : Components.Exaflare(module, new AOEShapeCircle(6))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.SabertailFirst)
        {
            var dir = spell.Rotation.ToDirection();
            var startOffset = caster.Position - Module.Center;
            startOffset.Z *= 0.99f; // handle exaflares right on N/S borders
            var distanceToBorder = Module.Bounds.IntersectRay(startOffset, dir);
            Lines.Add(new() { Next = caster.Position, Advance = 6.5f * dir, NextExplosion = Module.CastFinishAt(spell), TimeToMove = 0.7f, ExplosionsLeft = (int)(distanceToBorder / 6.5f) + 1, MaxShownExplosions = 5 });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.SabertailFirst or AID.SabertailRest)
        {
            ++NumCasts;
            int index = Lines.FindIndex(item => item.Next.AlmostEqual(caster.Position, 1));
            if (index == -1)
            {
                ReportError($"Failed to find entry for {caster.InstanceID:X}");
                return;
            }

            AdvanceLine(Lines[index], caster.Position);
            if (Lines[index].ExplosionsLeft == 0)
                Lines.RemoveAt(index);
        }
    }
}
