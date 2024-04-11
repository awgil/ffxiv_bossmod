namespace BossMod.Endwalker.Alliance.A33Oschon;

public class ArrowTrail(BossModule module) : Components.Exaflare(module, new AOEShapeRect(5, 5, 5))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ArrowTrailTelegraph)
            Lines.Add(new() { Next = caster.Position, Advance = 5 * caster.Rotation.ToDirection(), NextExplosion = spell.NPCFinishAt.AddSeconds(2.3f), TimeToMove = 0.5f, ExplosionsLeft = 8, MaxShownExplosions = 3 });
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (Lines.Count > 0 && (AID)spell.Action.ID == AID.ArrowTrailExa)
        {
            ++NumCasts;
            int index = Lines.FindIndex(item => item.Next.AlmostEqual(caster.Position, 1));
            AdvanceLine(Lines[index], caster.Position);
            if (Lines[index].ExplosionsLeft == 0)
                Lines.RemoveAt(index);
        }
    }
}
