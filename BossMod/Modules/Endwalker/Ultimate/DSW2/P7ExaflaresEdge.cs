using System.Linq;

namespace BossMod.Endwalker.Ultimate.DSW2
{
    class P7ExaflaresEdge : Components.Exaflare
    {
        public P7ExaflaresEdge() : base(6) { }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.ExaflaresEdgeFirst)
            {
                var advance = 7 * spell.Rotation.ToDirection();
                Lines.Add(new() { Next = caster.Position, Advance = advance, NextExplosion = spell.FinishAt, TimeToMove = 1.9f, ExplosionsLeft = 6, MaxShownExplosions = 2 });
                Lines.Add(new() { Next = caster.Position, Advance = advance.OrthoL(), NextExplosion = spell.FinishAt, TimeToMove = 1.9f, ExplosionsLeft = 6, MaxShownExplosions = 2 });
                Lines.Add(new() { Next = caster.Position, Advance = advance.OrthoR(), NextExplosion = spell.FinishAt, TimeToMove = 1.9f, ExplosionsLeft = 6, MaxShownExplosions = 2 });
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.ExaflaresEdgeFirst or AID.ExaflaresEdgeRest)
            {
                foreach (var l in Lines.Where(l => l.Next.AlmostEqual(caster.Position, 1)))
                {
                    AdvanceLine(module, l, caster.Position);
                }
                ++NumCasts;
            }
        }
    }
}
