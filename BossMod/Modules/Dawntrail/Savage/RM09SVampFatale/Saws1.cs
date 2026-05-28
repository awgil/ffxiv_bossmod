namespace BossMod.Dawntrail.Savage.RM09SVampFatale;

class BuzzsawArena(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime _activation;
    public bool Active { get; private set; }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation == default)
            yield break;

        yield return new(new AOEShapeRect(10, 20), Arena.Center + new WDir(10, 0), 90.Degrees());
        yield return new(new AOEShapeRect(10, 20), Arena.Center - new WDir(10, 0), -90.Degrees());
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x0F && state == 0x00020001)
            _activation = WorldState.FutureTime(5.7f);

        if (index == 0x00 && state == 0x00020001)
        {
            _activation = default;
            Active = true;
            Arena.Bounds = new ArenaBoundsRect(10, 20);
        }

        if (index == 0x00 && state == 0x00080004)
        {
            Arena.Center = new(100, 100);
            Arena.Bounds = new ArenaBoundsSquare(20);
        }
    }
}

class Coffinmaker(BossModule module) : Components.Adds(module, (uint)OID.Coffinmaker, 1);

class DeadWake(BossModule module) : Components.StandardAOEs(module, AID.DeadWake, new AOEShapeRect(10, 10));

class DeadWakeArena(BossModule module) : BossComponent(module)
{
    private float _height = 40;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.DeadWake)
        {
            _height -= 10;
            if (_height == 0)
                return;
            Arena.Center = new WPos(100, 100) + new WDir(0, (40 - _height) * 0.5f);
            Arena.Bounds = new ArenaBoundsRect(10, _height / 2);
        }
    }
}

class CoffinFiller(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Actor Caster, float Length)> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var act in _aoes.Take(2))
        {
            if (act.Caster.CastInfo == null)
                continue;

            yield return new AOEInstance(new AOEShapeRect(act.Length, 2.5f), act.Caster.CastInfo!.LocXZ, act.Caster.CastInfo!.Rotation, Module.CastFinishAt(act.Caster.CastInfo));
        }
    }

    public override void Update()
    {
        // FIXME
        _aoes.RemoveAll(c => c.Caster.CastInfo == null);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.CoffinfillerLong or AID.CoffinfillerMedium or AID.CoffinfillerShort)
        {
            NumCasts++;
            _aoes.RemoveAll(c => c.Caster == caster);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var len = (AID)spell.Action.ID switch
        {
            AID.CoffinfillerLong => 32,
            AID.CoffinfillerMedium => 22,
            AID.CoffinfillerShort => 12,
            _ => 0
        };
        if (len > 0)
        {
            _aoes.Add((caster, len));
            _aoes.SortBy(c => c.Caster.CastInfo!.NPCRemainingTime);
        }
    }
}
