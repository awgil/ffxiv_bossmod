namespace BossMod.Endwalker.Alliance.A34Eulogia;

sealed class AsAboveSoBelow(BossModule module) : Components.Exaflare(module, 6f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.EverfireFirst or (uint)AID.OnceBurnedFirst)
        {
            var advance = 6f * spell.Rotation.ToDirection();
            var pos = caster.Position;
            // outer lines have 4 explosion only, rest 5
            var numExplosions = (pos - Arena.Center).LengthSq() > 500f ? 4 : 6;
            AddLine(advance);
            AddLine(-advance);

            void AddLine(WDir dir)
            => Lines.Add(new(pos, dir, Module.CastFinishAt(spell), 1.5d, numExplosions, 5));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.EverfireFirst:
            case (uint)AID.OnceBurnedFirst:
                var dir = caster.Rotation.ToDirection();
                Advance(dir);
                Advance(-dir);
                ++NumCasts;
                break;
            case (uint)AID.EverfireRest:
            case (uint)AID.OnceBurnedRest:
                Advance(caster.Rotation.ToDirection());
                ++NumCasts;
                break;
        }
        void Advance(WDir dir)
        {
            var count = Lines.Count;
            var pos = caster.Position;
            for (var i = 0; i < count; ++i)
            {
                var line = Lines[i];
                if (line.Next.AlmostEqual(pos, 1f) && line.Advance.Dot(dir) > 5f)
                {
                    AdvanceLine(line, pos);
                    if (line.ExplosionsLeft == 0)
                        Lines.RemoveAt(i);
                    return;
                }
            }
            ReportError($"Failed to find entry for {caster.InstanceID:X}, {pos} / {dir}");
        }
    }
}
