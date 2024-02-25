namespace BossMod.Endwalker.Alliance.A21Nophica
{
    class SowingCircle : Components.Exaflare
    {
        public SowingCircle() : base(5) { }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.SowingCircleFirst)
            {
                Lines.Add(new() { Next = spell.LocXZ, Advance = 5 * caster.Rotation.ToDirection(), NextExplosion = spell.NPCFinishAt, TimeToMove = 1, ExplosionsLeft = 10, MaxShownExplosions = 2 });
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.SowingCircleFirst or AID.SowingCircleRest)
            {
                ++NumCasts;
                int index = Lines.FindIndex(item => item.Next.AlmostEqual(spell.TargetXZ, 1));
                if (index == -1)
                {
                    module.ReportError(this, $"Failed to find entry for {caster.InstanceID:X}");
                    return;
                }

                AdvanceLine(module, Lines[index], spell.TargetXZ);
                if (Lines[index].ExplosionsLeft == 0)
                    Lines.RemoveAt(index);
            }
        }
    }
}
