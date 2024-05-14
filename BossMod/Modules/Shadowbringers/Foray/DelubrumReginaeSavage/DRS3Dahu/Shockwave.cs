namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS3Dahu;

class Shockwave(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCone _shape = new(15, 90.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(1);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.LeftSidedShockwaveFirst or AID.RightSidedShockwaveFirst)
        {
            _aoes.Add(new(_shape, caster.Position, spell.Rotation, spell.NPCFinishAt));
            _aoes.Add(new(_shape, caster.Position, spell.Rotation + 180.Degrees(), spell.NPCFinishAt.AddSeconds(2.6f)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.LeftSidedShockwaveFirst or AID.RightSidedShockwaveFirst or AID.LeftSidedShockwaveSecond or AID.RightSidedShockwaveSecond && _aoes.Count > 0)
            _aoes.RemoveAt(0);
    }
}
