namespace BossMod.Shadowbringers.Alliance.A36FalseIdol;

class Distortion(BossModule module) : Components.GenericAOEs(module, AID.PlaceOfPower)
{
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
            yield return new AOEInstance(new AOEShapeCircle(6), Arena.Center, Activation: _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _activation = Module.CastFinishAt(spell);
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 1 && state == 0x00800001)
            _activation = default;
    }
}

class Dissonance(BossModule module) : Components.CastCounterMulti(module, [AID.WhiteDissonance, AID.BlackDissonance])
{
    private readonly List<(Actor Ring, A34RedGirl.Shade Color, DateTime Activation)> _rings = [];

    public override void OnActorEAnim(Actor actor, uint state)
    {
        var color = (OID)actor.OID switch
        {
            OID.WhiteDissonance => A34RedGirl.Shade.White,
            OID.BlackDissonance => A34RedGirl.Shade.Black,
            _ => default
        };
        if (color != default && state == 0x00010002)
            _rings.Add((actor, color, WorldState.FutureTime(8.1f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (WatchedActions.Contains(spell.Action))
        {
            NumCasts++;
            if (_rings.Count > 0)
                _rings.RemoveAt(0);
        }
    }

    private bool FacingCenter(Actor pc) => (Arena.Center - pc.Position).Dot(pc.Rotation.ToDirection()) > 0;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_rings.Count > 0)
        {
            var facing = pc.Rotation + (_rings[0].Color == A34RedGirl.Shade.White ? 180.Degrees() : default);
            var color = (_rings[0].Color == A34RedGirl.Shade.White) == FacingCenter(pc) ? ArenaColor.Danger : ArenaColor.Safe;
            Arena.PathArcTo(pc.Position, 1.5f, facing.Rad - MathF.PI * 0.5f, facing.Rad + MathF.PI * 0.5f);
            Arena.PathStroke(false, color);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_rings.Count > 0)
        {
            if (_rings[0].Color == A34RedGirl.Shade.White)
                hints.Add("Face away from ring!", FacingCenter(actor));
            else
                hints.Add("Face towards ring!", !FacingCenter(actor));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_rings.Count > 0)
        {
            var dangerDir = Angle.FromDirection(_rings[0].Color == A34RedGirl.Shade.White ? Arena.Center - actor.Position : actor.Position - Arena.Center);
            hints.ForbiddenDirections.Add((dangerDir, 90.Degrees(), _rings[0].Activation));
        }
    }
}
