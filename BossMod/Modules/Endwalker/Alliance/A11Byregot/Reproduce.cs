using System;

namespace BossMod.Endwalker.Alliance.A11Byregot
{
    class Reproduce : Components.Exaflare
    {
        public Reproduce() : base(7) { }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.CloudToGroundFast or AID.CloudToGroundSlow)
            {
                var fast = (AID)spell.Action.ID == AID.CloudToGroundFast;
                Lines.Add(new() { Next = caster.Position, Advance = new(-8.5f, 0), NextExplosion = spell.FinishAt, TimeToMove = fast ? 0.6f : 1.4f, ExplosionsLeft = 6, MaxShownExplosions = fast ? 5 : 2 });
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.CloudToGroundFast or AID.CloudToGroundSlow or AID.CloudToGroundFastAOE or AID.CloudToGroundSlowAOE)
            {
                ++NumCasts;

                int index = Lines.FindIndex(item => MathF.Abs(item.Next.Z - caster.Position.Z) < 1);
                if (index == -1)
                {
                    module.ReportError(this, $"Failed to find entry for {caster.InstanceID:X}");
                    return;
                }

                AdvanceLine(module, Lines[index], caster.Position);
                if (Lines[index].Next.X < module.Bounds.Center.X - module.Bounds.HalfSize)
                    Lines.RemoveAt(index);
            }
        }
    }
}
