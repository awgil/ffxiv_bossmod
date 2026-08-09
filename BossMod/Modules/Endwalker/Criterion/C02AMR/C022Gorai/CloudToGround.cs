namespace BossMod.Endwalker.VariantCriterion.C02AMR.C022Gorai;

sealed class CloudToGround(BossModule module) : Components.Exaflare(module, 6f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.NCloudToGroundAOEFirst or (uint)AID.SCloudToGroundAOEFirst)
        {
            // 4 central exaflares (+-6 along one axis, 0 along other) have 3 casts, 4 side exaflares (+-20 along one axis, +-5/15 along other) have 7 casts
            Lines.Add(new(caster.Position, 6f * spell.Rotation.ToDirection(), Module.CastFinishAt(spell), 1.1d, (caster.Position - Arena.Center).LengthSq() > 100 ? 7 : 3, 3));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.NCloudToGroundAOEFirst or (uint)AID.SCloudToGroundAOEFirst or (uint)AID.NCloudToGroundAOERest or (uint)AID.SCloudToGroundAOERest)
        {
            ++NumCasts;
            var count = Lines.Count;
            var pos = caster.Position;
            for (var i = 0; i < count; ++i)
            {
                var line = Lines[i];
                if (line.Next.AlmostEqual(pos, 1f))
                {
                    AdvanceLine(line, pos);
                    if (line.ExplosionsLeft == 0)
                        Lines.RemoveAt(i);
                    return;
                }
            }
            ReportError($"Failed to find entry for {caster.InstanceID:X}");
        }
    }
}
