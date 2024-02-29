﻿namespace BossMod.Endwalker.Alliance.A14Naldthal
{
    class OnceAboveEverBelow : Components.Exaflare
    {
        public OnceAboveEverBelow() : base(6) { }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.EverfireFirst or AID.OnceBurnedFirst)
            {
                var advance = 6 * spell.Rotation.ToDirection();
                // lines are offset by 6/18/30; outer have 1 explosion only, mid have 4 or 5, inner 5
                var numExplosions = (caster.Position - module.Bounds.Center).LengthSq() > 500 ? 1 : 5;
                Lines.Add(new() { Next = caster.Position, Advance = advance, NextExplosion = spell.NPCFinishAt, TimeToMove = 1.5f, ExplosionsLeft = numExplosions, MaxShownExplosions = 5 });
                Lines.Add(new() { Next = caster.Position, Advance = -advance, NextExplosion = spell.NPCFinishAt, TimeToMove = 1.5f, ExplosionsLeft = numExplosions, MaxShownExplosions = 5 });
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.EverfireFirst:
                case AID.OnceBurnedFirst:
                    var dir = caster.Rotation.ToDirection();
                    Advance(module, caster.Position, dir);
                    Advance(module, caster.Position, -dir);
                    ++NumCasts;
                    break;
                case AID.EverfireRest:
                case AID.OnceBurnedRest:
                    Advance(module, caster.Position, caster.Rotation.ToDirection());
                    ++NumCasts;
                    break;
            }
        }

        private void Advance(BossModule module, WPos position, WDir dir)
        {
            int index = Lines.FindIndex(item => item.Next.AlmostEqual(position, 1) && item.Advance.Dot(dir) > 5);
            if (index == -1)
            {
                module.ReportError(this, $"Failed to find entry for {position} / {dir}");
                return;
            }

            AdvanceLine(module, Lines[index], position);
            if (Lines[index].ExplosionsLeft == 0)
                Lines.RemoveAt(index);
        }
    }
}
