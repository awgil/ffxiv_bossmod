namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS8Queen;

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    public override bool KeepOnPhaseChange => true;
    public readonly AOEShapeDonut donut = new(25f, 43f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.EmpyreanIniquityAOE && Arena.Bounds.Radius > 26f)
        {
            var center = Arena.Center;
            _aoe = [new(donut, center, default, Module.CastFinishAt(spell, 4.8d), shapeDistance: donut.Distance(center, default))];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x19)
        {
            if (state is 0x00020001u or 0x00400001u)
            {
                Arena.Bounds = Queen.GetDefaultArena();
                _aoe = [];
            }
            else if (state == 0x00200010u)
            {
                Arena.Bounds = new ArenaBoundsSquare(25f);
                _aoe = [];
            }
        }
    }
}
