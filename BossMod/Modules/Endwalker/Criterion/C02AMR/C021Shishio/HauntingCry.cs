namespace BossMod.Endwalker.Criterion.C02AMR.C021Shishio;

class HauntingCrySwipes(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _casters = [];

    private static readonly AOEShapeCone _shape = new(40, 90.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return _casters.Take(4).Select(c => new AOEInstance(_shape, c.Position, c.CastInfo!.Rotation, c.CastInfo.NPCFinishAt));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.NRightSwipe or AID.NLeftSwipe or AID.SRightSwipe or AID.SLeftSwipe)
            _casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.NRightSwipe or AID.NLeftSwipe or AID.SRightSwipe or AID.SLeftSwipe)
        {
            _casters.Remove(caster);
            ++NumCasts;
        }
    }
}

class HauntingCryReisho(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _ghosts = [];
    private DateTime _activation;
    private DateTime _ignoreBefore;

    private static readonly AOEShapeCircle _shape = new(6);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _ghosts.Select(g => new AOEInstance(_shape, g.Position, default, _activation));

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var g in _ghosts)
        {
            Arena.Actor(g, ArenaColor.Object, true);
            var target = WorldState.Actors.Find(g.Tether.Target);
            if (target != null)
                Arena.AddLine(g.Position, target.Position, ArenaColor.Danger);
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((OID)source.OID is OID.NHauntingThrall or OID.SHauntingThrall)
        {
            _ghosts.Add(source);
            _activation = WorldState.FutureTime(5.1f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NReisho or AID.SReisho && WorldState.CurrentTime > _ignoreBefore)
        {
            ++NumCasts;
            _activation = WorldState.FutureTime(2.1f);
            _ignoreBefore = WorldState.FutureTime(1);
        }
    }
}

class HauntingCryVermilionAura(BossModule module, AID aid) : Components.CastTowers(module, ActionID.MakeSpell(aid), 4);
class NHauntingCryVermilionAura(BossModule module) : HauntingCryVermilionAura(module, AID.NVermilionAura);
class SHauntingCryVermilionAura(BossModule module) : HauntingCryVermilionAura(module, AID.SVermilionAura);

class HauntingCryStygianAura(BossModule module, AID aid) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(aid), 15, true);
class NHauntingCryStygianAura(BossModule module) : HauntingCryStygianAura(module, AID.NStygianAura);
class SHauntingCryStygianAura(BossModule module) : HauntingCryStygianAura(module, AID.SStygianAura);
