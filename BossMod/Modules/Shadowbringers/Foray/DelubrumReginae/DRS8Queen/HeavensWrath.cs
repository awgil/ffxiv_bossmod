namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS8Queen;

class HeavensWrathAOE(BossModule module) : Components.StandardAOEs(module, AID.HeavensWrathVisual, new AOEShapeRect(25, 5, 25));

// TODO: generalize
class HeavensWrathKnockback(BossModule module) : Components.Knockback(module)
{
    private readonly List<Source> _sources = [];
    private static readonly AOEShapeCone _shape = new(30, 90.Degrees());

    public override IEnumerable<Source> Sources(int slot, Actor actor) => _sources;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.HeavensWrathVisual)
        {
            _sources.Clear();
            _sources.Add(new(caster.Position, 15, Module.CastFinishAt(spell), _shape, spell.Rotation + 90.Degrees(), Kind.DirForward));
            _sources.Add(new(caster.Position, 15, Module.CastFinishAt(spell), _shape, spell.Rotation - 90.Degrees(), Kind.DirForward));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.HeavensWrathVisual)
        {
            _sources.Clear();
            ++NumCasts;
        }
    }
}
