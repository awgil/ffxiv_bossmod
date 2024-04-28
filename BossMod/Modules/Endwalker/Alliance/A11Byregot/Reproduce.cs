namespace BossMod.Endwalker.Alliance.A11Byregot;

class Reproduce(BossModule module) : Components.Exaflare(module, 7)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.CloudToGroundFast or AID.CloudToGroundSlow)
        {
            var fast = (AID)spell.Action.ID == AID.CloudToGroundFast;
            Lines.Add(new() { Next = caster.Position, Advance = new(-8.5f, 0), NextExplosion = spell.NPCFinishAt, TimeToMove = fast ? 0.6f : 1.4f, ExplosionsLeft = 6, MaxShownExplosions = fast ? 5 : 2 });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.CloudToGroundFast or AID.CloudToGroundSlow or AID.CloudToGroundFastAOE or AID.CloudToGroundSlowAOE)
        {
            ++NumCasts;

            int index = Lines.FindIndex(item => MathF.Abs(item.Next.Z - caster.Position.Z) < 1);
            if (index == -1)
            {
                ReportError($"Failed to find entry for {caster.InstanceID:X}");
                return;
            }

            AdvanceLine(Lines[index], caster.Position);
            if (Lines[index].Next.X < Module.Center.X - Module.Bounds.Radius)
                Lines.RemoveAt(index);
        }
    }
}
