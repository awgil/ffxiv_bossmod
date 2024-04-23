namespace BossMod.Endwalker.Ultimate.DSW2;

class P2BroadSwing(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.BroadSwingAOE))
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCone _aoe = new(40, 60.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(2);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var rot = (AID)spell.Action.ID switch
        {
            AID.BroadSwingRL => -60.Degrees(),
            AID.BroadSwingLR => 60.Degrees(),
            _ => default
        };
        if (rot != default)
        {
            _aoes.Add(new(_aoe, caster.Position, spell.Rotation + rot, spell.NPCFinishAt.AddSeconds(0.8f), ArenaColor.Danger));
            _aoes.Add(new(_aoe, caster.Position, spell.Rotation - rot, spell.NPCFinishAt.AddSeconds(1.8f)));
            _aoes.Add(new(_aoe, caster.Position, spell.Rotation + 180.Degrees(), spell.NPCFinishAt.AddSeconds(2.8f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            if (_aoes.Count > 0)
                _aoes.RemoveAt(0);
            if (_aoes.Count > 0)
                _aoes.AsSpan()[0].Color = ArenaColor.Danger;
        }
    }
}
