
namespace BossMod.Dawntrail.Raid.M12NLindwurm;

[SkipLocalsInit]
sealed class CruelCoil(BossModule module) : Components.GenericAOEs(module, warningText: "Run out of snek!")
{
    // 1st time does skinsplitter x4 then cruel coil
    // 2nd time around same except only 3 skinsplitter
    // has mapeffect for 0x02 to 0x0A, with state 0x00010001 and 0x00040004
    // 0x0A 0x00010001 is a "reset", maybe boss still spinning around
    // 0x02-0x09 0x00010001 is the exit, with 0x03 = SE, 0x05 = SW, 0x07 = NW, 0x09 = NE?
    // exit in normal mode is the direction that boss is facing
    // mapeffects don't fire on the 1st skinsplitter after succ, only subsequent ones
    // coils have 13f radius; make 13f AOE, diff a 12.9f ring in the middle with small connecting platform to rest of arena for pathfinding

    private bool _active;
    private readonly AOEShapeCircle circle = new(13f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoe.Length != 0)
        {
            ref var aoe = ref _aoe[0];
            aoe.Risky = _active;
        }
        return _aoe;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ConstrictorVisual3)
        {
            _aoe = [new(circle, Arena.Center.Quantized(), activation: Module.CastFinishAt(spell), risky: false)];
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Bind)
        {
            UpdateArena();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var id = spell.Action.ID;
        if (id == (uint)AID.SkinsplitterVisual)
        {
            _active = false;
            UpdateArena();
        }
        else if (id == (uint)AID.Constrictor)
        {
            _active = false;
            _aoe = [];
            Arena.Bounds = new ArenaBoundsRect(20f, 15f);
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x0A)
        {
            _active = state != 0x00010001u;
        }
        else if (index is >= 0x02 and <= 0x09)
        {
            // state 0x00010001 is "open" section, or "reset" if 0x0A
            // 0x09 = NW, 0x07 = SW, 0x05 = SE, 0x03 = NE
            // N would be start at 0x02
            if (state == 0x00010001)
            {
                _active = true;
                UpdateArena(index);
            }
        }
    }

    private void UpdateArena(int state = 0)
    {
        var centerDir = (state == 0 ? 0f : (state - 0x02) * -45f).Degrees();
        Arena.Bounds = new ArenaBoundsCustom([new Rectangle(Arena.Center, 20f, 15f)], [new DonutSegmentV(Arena.Center, 9.5f, 13.5f, centerDir, (state == 0 ? 180f : 160f).Degrees(), 128)]);
    }
}
