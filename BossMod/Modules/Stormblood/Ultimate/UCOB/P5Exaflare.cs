namespace BossMod.Stormblood.Ultimate.UCOB;

class P5Exaflare(BossModule module) : Components.Exaflare(module, 6)
{
    public void Reset() => NumCasts = 0;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ExaflareFirst)
        {
            Lines.Add(new() { Next = caster.Position, Advance = 8 * spell.Rotation.ToDirection(), NextExplosion = spell.NPCFinishAt, TimeToMove = 1.5f, ExplosionsLeft = 6, MaxShownExplosions = 4 });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ExaflareFirst or AID.ExaflareRest)
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
