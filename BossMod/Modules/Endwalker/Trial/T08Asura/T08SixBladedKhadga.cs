using BossMod.Endwalker.Alliance.A30Trash2;
using BossMod.Shadowbringers.Foray.DelubrumReginae.DRS4QueensGuard;

namespace BossMod.Endwalker.Trial.T08Asura;

class SixBladedKhadga : Components.GenericAOEs
{
    private readonly List<ActorCastInfo> _spell = [];
    private DateTime _start;
    private static readonly AOEShapeCone Cone = new(20, 90.Degrees());
    private const float MaxError = MathF.PI / 180;

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        if (_spell.Count > 0)
            yield return new(Cone, module.PrimaryActor.Position, _spell[0].Rotation, _start.AddSeconds(NumCasts * 2), ArenaColor.Danger);
        if (_spell.Count > 1)
            yield return new(Cone, module.PrimaryActor.Position, _spell[1].Rotation, _start.AddSeconds(2 + NumCasts * 2), risky: !_spell[1].Rotation.AlmostEqual(_spell[0].Rotation + 180.Degrees(), MaxError));
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.KhadgaTelegraph1 or AID.KhadgaTelegraph2 or AID.KhadgaTelegraph3)
        {
            _spell.Add(spell);
            if (_start == default)
                _start = module.WorldState.CurrentTime.AddSeconds(12.9f);
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Khadga1 or AID.Khadga2 or AID.Khadga3 or AID.Khadga4 or AID.Khadga5 or AID.Khadga6)
        {
            _spell.RemoveAt(0);
            _start.AddSeconds(2);
            if (++NumCasts == 6)
            {
                NumCasts = 0;
                _start = default;
            }
        }
    }
}
