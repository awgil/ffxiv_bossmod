namespace BossMod.Endwalker.Alliance.A33Oschon;

public class ArrowTrail : Components.Exaflare
{
    public ArrowTrail() : base(new AOEShapeRect(5, 5, 5)) { }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ArrowTrailTelegraph)
            Lines.Add(new() { Next = caster.Position, Advance = 5 * caster.Rotation.ToDirection(), NextExplosion = spell.NPCFinishAt.AddSeconds(2.3f), TimeToMove = 0.5f, ExplosionsLeft = 8, MaxShownExplosions = 3 });
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if (Lines.Count > 0 && (AID)spell.Action.ID == AID.ArrowTrailExa)
        {
            ++NumCasts;
            int index = Lines.FindIndex(item => item.Next.AlmostEqual(caster.Position, 1));
            AdvanceLine(module, Lines[index], caster.Position);
            if (Lines[index].ExplosionsLeft == 0)
                Lines.RemoveAt(index);
        }
    }
}
