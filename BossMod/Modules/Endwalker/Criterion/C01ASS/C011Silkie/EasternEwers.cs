namespace BossMod.Endwalker.Criterion.C01ASS.C011Silkie;

class EasternEwers(BossModule module) : Components.Exaflare(module, 4)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.NBrimOver or AID.SBrimOver)
        {
            Lines.Add(new() { Next = caster.Position, Advance = new(0, 5.1f), NextExplosion = spell.NPCFinishAt, TimeToMove = 0.8f, ExplosionsLeft = 11, MaxShownExplosions = int.MaxValue });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NBrimOver or AID.SBrimOver or AID.NRinse or AID.SRinse)
        {
            int index = Lines.FindIndex(item => Math.Abs(item.Next.X - caster.Position.X) < 1);
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
