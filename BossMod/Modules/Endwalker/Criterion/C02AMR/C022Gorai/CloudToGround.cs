﻿namespace BossMod.Endwalker.Criterion.C02AMR.C022Gorai;

class CloudToGround(BossModule module) : Components.Exaflare(module, 6)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.NCloudToGroundAOEFirst or AID.SCloudToGroundAOEFirst)
        {
            // 4 central exaflares (+-6 along one axis, 0 along other) have 3 casts, 4 side exaflares (+-20 along one axis, +-5/15 along other) have 7 casts
            Lines.Add(new() { Next = caster.Position, Advance = 6 * spell.Rotation.ToDirection(), NextExplosion = Module.CastFinishAt(spell), TimeToMove = 1.1f, ExplosionsLeft = (caster.Position - Module.Center).LengthSq() > 100 ? 7 : 3, MaxShownExplosions = 3 });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NCloudToGroundAOEFirst or AID.SCloudToGroundAOEFirst or AID.NCloudToGroundAOERest or AID.SCloudToGroundAOERest)
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
