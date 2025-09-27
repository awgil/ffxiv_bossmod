namespace BossMod.Endwalker.Alliance.A31Thaliak;

class RheognosisKnockback(BossModule module) : Components.Knockback(module)
{
    private Source? _knockback;

    public override IEnumerable<Source> Sources(int slot, Actor actor) => Utils.ZeroOrOne(_knockback);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Rheognosis or AID.RheognosisPetrine)
            _knockback = new(Module.Center, 25, Module.CastFinishAt(spell, 20.3f), Direction: spell.Rotation + 180.Degrees(), Kind: Kind.DirForward);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.RheognosisKnockback)
        {
            _knockback = null;
            ++NumCasts;
        }
    }
}

public class RheognosisCrash : Components.Exaflare
{
    public RheognosisCrash(BossModule module) : base(module, new AOEShapeRect(10, 12), AID.RheognosisCrash) => ImminentColor = ArenaColor.AOE;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index <= 1 && state is 0x01000001 or 0x02000001)
        {
            var west = index == 0;
            var right = state == 0x01000001;
            var south = west == right;
            var start = Module.Center + new WDir(west ? -Module.Bounds.Radius : +Module.Bounds.Radius, (south ? +Module.Bounds.Radius : -Module.Bounds.Radius) * 0.5f);
            var dir = (west ? 90 : -90).Degrees();
            Lines.Add(new() { Next = start, Advance = 10 * dir.ToDirection(), Rotation = dir, NextExplosion = WorldState.FutureTime(4), TimeToMove = 0.2f, ExplosionsLeft = 5, MaxShownExplosions = 5 });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            int index = Lines.FindIndex(item => item.Next.AlmostEqual(caster.Position, 1));
            if (index >= 0)
            {
                AdvanceLine(Lines[index], caster.Position);
                if (Lines[index].ExplosionsLeft == 0)
                    Lines.RemoveAt(index);
            }
        }
    }
}
