namespace BossMod.Modules.Dawntrail.Advanced.Ad01TheMerchantsTale.Ad011PariofPlenty;

sealed class FireFlight(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private WPos _nextLanding = default;
    private DateTime _firstActivation = default;
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);
    private bool left;
    private Actor _who;
    private bool _active = false;
    // This isn't a *good* way of doing this but it's much simpler today than painstakingly figuring out which rides belong to which mechanics today.
    private readonly List<uint> _carpetrides = [(uint)AID.CarpetRide, (uint)AID.CarpetRide1, (uint)AID.CarpetRide2, (uint)AID.CarpetRide3, (uint)AID.CarpetRide4, (uint)AID.CarpetRide5, (uint)AID.CarpetRide6, (uint)AID.CarpetRide7, (uint)AID.CarpetRide8, (uint)AID.CarpetRide9, (uint)AID.CarpetRide10];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is ((uint)AID.LeftFireflight))
        {
            left = true;
            _who = caster;
            _active = true;
        }
        else if (spell.Action.ID is (uint)AID.RightFireflight)
        {
            left = false;
            _who = caster;
            _active = true;
        }
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        if (_active)
        {
            if (tether.ID == (uint)TetherID.Fireflight)
            {
                var target = WorldState.Actors.Find(tether.Target);
                if (target == null)
                {
                    return;
                }
                var aoes = CollectionsMarshal.AsSpan(_aoes);
                var len = aoes.Length;
                var initial = len == 0 ? _who.Position : _nextLanding;
                _nextLanding = target.Position;
                if (_firstActivation == default)
                {
                    _firstActivation = WorldState.FutureTime(11.25d);
                }

                var rot = (_nextLanding - initial).ToAngle();
                var activation = _firstActivation.AddSeconds(2d * (len - 1));
                var conerot = left ? rot + 90.Degrees() : rot + 270.Degrees();
                //var conepos = initial + conerot.ToDirection() * 5f;
                _aoes.Add(new(new AOEShapeCone(60f, 90f.Degrees()), initial, conerot, activation));
                if (_aoes.Count == 3)
                {
                    // inner slighly larger but untethered helper location not exactly where boss lands
                    _aoes.Add(new(new AOEShapeDonut(7.7f, 60f), _nextLanding, activation: activation.AddSeconds(2.75d)));
                }
            }
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_active)
        {
            if (_carpetrides.Contains(spell.Action.ID))
            {
                if (_aoes.Count > 0)
                {
                    _aoes.RemoveAt(0);
                    //if (_mech is Mechanic.fireFlight)
                    //{
                    //    _aoes.RemoveAt(0);
                    //}
                }
            }
            else if (spell.Action.ID == (uint)AID.SunCirclet)
            {
                _aoes.Clear();
                _nextLanding = default;
                _firstActivation = default;
                _active = false;
            }
        }
    }
}

sealed class FireFlightFactOrFiction(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private WPos _nextLanding = default;
    private DateTime _firstActivation = default;
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);
    private bool left;
    private Actor real;
    private bool _active = false;
    private readonly List<uint> _carpetrides = [(uint)AID.CarpetRide, (uint)AID.CarpetRide1, (uint)AID.CarpetRide2, (uint)AID.CarpetRide3, (uint)AID.CarpetRide4, (uint)AID.CarpetRide5, (uint)AID.CarpetRide6, (uint)AID.CarpetRide7, (uint)AID.CarpetRide8, (uint)AID.CarpetRide9, (uint)AID.CarpetRide10];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.LeftFireflightFactAndFiction)
        {
            left = true;
            _active = true;
        }
        else if (spell.Action.ID is (uint)AID.RightFireflightFactAndFiction)
        {
            left = false;
            _active = true;
        }
    }
    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        if (_active)
        {
            if (_aoes.Count == 0)
            {
                if (source.Position.AlmostEqual(Module.PrimaryActor.Position, 0.5f))
                {
                    real = source;
                }
            }
            else
            {
                if (source.Position.AlmostEqual(_nextLanding, 0.5f))
                {
                    real = source;
                }
            }
            if (source == real)
            {
                var target = WorldState.Actors.Find(tether.Target);
                if (target == null)
                {
                    return;
                }
                var aoes = CollectionsMarshal.AsSpan(_aoes);
                var len = aoes.Length;
                var initial = len == 0 ? Module.PrimaryActor.Position : _nextLanding;
                _nextLanding = target.Position;
                if (_firstActivation == default)
                {
                    _firstActivation = WorldState.FutureTime(11.25d);
                }

                var rot = (_nextLanding - initial).ToAngle();
                var activation = _firstActivation.AddSeconds(2d * (len - 1));
                var conerot = left ? rot + 90.Degrees() : rot + 270.Degrees();
                //var conepos = initial + conerot.ToDirection() * 5f;
                _aoes.Add(new(new AOEShapeCone(60f, 90f.Degrees()), initial, conerot, activation));
                if (_aoes.Count == 3)
                {
                    // inner slighly larger but untethered helper location not exactly where boss lands
                    _aoes.Add(new(new AOEShapeDonut(7.7f, 60f), _nextLanding, activation: activation.AddSeconds(2.75d)));
                }
            }
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_active)
        {
            if (_carpetrides.Contains(spell.Action.ID))
            {
                if (_aoes.Count > 0)
                {
                    _aoes.RemoveAt(0);
                    //if (_mech is Mechanic.fireFlight)
                    //{
                    //    _aoes.RemoveAt(0);
                    //}
                }
            }
            else if (spell.Action.ID == (uint)AID.SunCirclet)
            {
                _aoes.Clear();
                _nextLanding = default;
                _firstActivation = default;
                _active = false;
            }
        }
    }
}

sealed class DoubleFableFlight(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Activecasters;
    private WPos _nextLanding = default;
    private DateTime _firstActivation = default;
    private bool left;
    private bool _active = false;
    private readonly List<uint> _carpetrides = [(uint)AID.CarpetRide, (uint)AID.CarpetRide1, (uint)AID.CarpetRide2, (uint)AID.CarpetRide3, (uint)AID.CarpetRide4, (uint)AID.CarpetRide5, (uint)AID.CarpetRide6, (uint)AID.CarpetRide7, (uint)AID.CarpetRide8, (uint)AID.CarpetRide9, (uint)AID.CarpetRide10];
    private readonly List<AOEInstance> _casters = [];

    public ReadOnlySpan<AOEInstance> Activecasters
    {
        get
        {
            var count = _casters.Count;
            var max = count > 2 ? 2 : count;
            return CollectionsMarshal.AsSpan(_casters)[..max];
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is ((uint)AID.LeftFableflight1))
        {
            left = true;
            _active = true;
        }
        else if (spell.Action.ID is (uint)AID.RightFableflight1)
        {
            left = false;
            _active = true;
        }
        else if (spell.Action.ID is (uint)AID.CharmdFableflight)
        {
            _active = true;
        }
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        if (_active)
        {
            if (tether.ID == (uint)TetherID.Fireflight)
            {

                var target = WorldState.Actors.Find(tether.Target);
                if (target == null)
                {
                    return;
                }
                var aoes = CollectionsMarshal.AsSpan(_aoes);
                var len = aoes.Length;
                var initial = source.Position;
                _nextLanding = target.Position;
                if (_firstActivation == default)
                {
                    _firstActivation = WorldState.FutureTime(11.25d);
                }

                var rot = (_nextLanding - initial).ToAngle();
                var activation = _firstActivation.AddSeconds(2d * (len - 1));
                var conerot = left ? rot + 90.Degrees() : rot + 270.Degrees();
                //var conepos = initial + conerot.ToDirection() * 5f;
                _casters.Add(new(new AOEShapeCone(60f, 90f.Degrees()), initial, conerot, activation));
                //_casters.Add(new(new AOEShapeCone(60f, 90f.Degrees()), initial, rot + 180.Degrees(), activation.AddSeconds(2d)));
            }
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_active)
        {
            if (_carpetrides.Contains(spell.Action.ID))
            {
                if (_casters.Count > 0)
                {
                    _casters.RemoveAt(0);
                    //if (_mech is Mechanic.fireFlight)
                    //{
                    //    _aoes.RemoveAt(0);
                    //}
                    if (_casters.Count == 0)
                    {
                        _active = false;
                    }
                }
            }
        }
    }
}
sealed class FalseFlameDisplay(BossModule module) : Components.AddsPointless(module, (uint)OID.FalseFlame);
