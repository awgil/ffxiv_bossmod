namespace BossMod.Endwalker.Savage.P1SErichthonios;

// state related to [aether]flails mechanics
class Flails : BossComponent
{
    public int NumCasts { get; private set; } = 0;
    private AOEShape? _first;
    private AOEShape? _second;
    private bool _detectSecond;
    private bool _showSecond;

    private static readonly AOEShape _aoeLeft = new AOEShapeCone(60, 135.Degrees(), 90.Degrees());
    private static readonly AOEShape _aoeRight = new AOEShapeCone(60, 135.Degrees(), -90.Degrees());
    private static readonly AOEShape _aoeInner = new AOEShapeCircle(P1S.InnerCircleRadius);
    private static readonly AOEShape _aoeOuter = new AOEShapeDonut(P1S.InnerCircleRadius, 60);

    public override void Init(BossModule module)
    {
        (_first, _second) = (AID)(module.PrimaryActor.CastInfo?.Action.ID ?? 0) switch
        {
            AID.GaolerFlailRL => (_aoeRight, _aoeLeft),
            AID.GaolerFlailLR => (_aoeLeft, _aoeRight),
            AID.GaolerFlailIO1 => (_aoeInner, _aoeOuter),
            AID.GaolerFlailIO2 => (_aoeInner, _aoeOuter),
            AID.GaolerFlailOI1 => (_aoeOuter, _aoeInner),
            AID.GaolerFlailOI2 => (_aoeOuter, _aoeInner),
            AID.AetherflailRX => (_aoeRight, null),
            AID.AetherflailLX => (_aoeLeft, null),
            AID.AetherflailIL => (_aoeInner, _aoeLeft),
            AID.AetherflailIR => (_aoeInner, _aoeRight),
            AID.AetherflailOL => (_aoeOuter, _aoeLeft),
            AID.AetherflailOR => (_aoeOuter, _aoeRight),
            _ => (null, null)
        };

        if (_first == null)
            module.ReportError(this, "Failed to detect flail zones");

        _detectSecond = _first != null && _second == null;
        _showSecond = _first is AOEShapeCone != _second is AOEShapeCone;
    }

    public override void Update(BossModule module)
    {
        // currently the best way i've found to determine secondary aetherflail attack if first attack is a cone is to watch spawned npcs
        // these can appear few frames later...
        if (!_detectSecond)
            return;

        var weaponsBall = module.Enemies(OID.FlailI);
        var weaponsChakram = module.Enemies(OID.FlailO);
        if (weaponsBall.Count + weaponsChakram.Count > 0)
        {
            _detectSecond = false;
            if (weaponsBall.Count > 0 && weaponsChakram.Count > 0)
            {
                module.ReportError(this, $"Failed to determine second aetherflail: there are {weaponsBall.Count} balls and {weaponsChakram.Count} chakrams");
            }
            else
            {
                _second = weaponsBall.Count > 0 ? _aoeInner : _aoeOuter;
            }
        }
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (_first?.Check(actor.Position, module.PrimaryActor) ?? false)
            hints.Add("Hit by first flail!");
        if (_showSecond && _second != null && _second.Check(actor.Position, module.PrimaryActor))
            hints.Add("Hit by second flail!");
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        _first?.Draw(arena, module.PrimaryActor);
        if (_showSecond)
            _second?.Draw(arena, module.PrimaryActor);
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.GaolerFlailR1:
            case AID.GaolerFlailL1:
            case AID.GaolerFlailI1:
            case AID.GaolerFlailO1:
                ++NumCasts;
                _first = null;
                _showSecond = true;
                break;
            case AID.GaolerFlailR2:
            case AID.GaolerFlailL2:
            case AID.GaolerFlailI2:
            case AID.GaolerFlailO2:
                ++NumCasts;
                _second = null;
                break;
        }
    }
}
