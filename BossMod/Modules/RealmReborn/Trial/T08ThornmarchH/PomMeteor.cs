namespace BossMod.RealmReborn.Trial.T08ThornmarchH;

class PomMeteor(BossModule module) : BossComponent(module)
{
    private BitMask _activeTowers;
    private BitMask _soakedTowers;
    private DateTime _towerActivation;
    private int _cometsLeft;
    private float _activationDelay = 8; // 8s for first set of towers, then 16s for others

    private const float _towerRadius = 5;
    private const float _cometAvoidRadius = 6;
    private static readonly Angle[] _towerAngles = [180.Degrees(), 90.Degrees(), 0.Degrees(), -90.Degrees(), 135.Degrees(), 45.Degrees(), -45.Degrees(), -135.Degrees()];
    private static readonly WDir[] _towerOffsets = [.. _towerAngles.Select(a => 10 * a.ToDirection())];

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_activeTowers.None())
            return;

        if (_cometsLeft > 0)
        {
            foreach (int i in _activeTowers.SetBits())
                hints.AddForbiddenZone(ShapeContains.Circle(Module.Center + _towerOffsets[i], _cometAvoidRadius));
        }
        else
        {
            // assume H1/H2/R1/R2 soak towers
            int soakedTower = assignment switch
            {
                PartyRolesConfig.Assignment.H1 => 0,
                PartyRolesConfig.Assignment.H2 => 1,
                PartyRolesConfig.Assignment.R1 => 2,
                PartyRolesConfig.Assignment.R2 => 3,
                _ => -1,
            };
            if (soakedTower >= 0)
            {
                if (!_activeTowers[soakedTower])
                    soakedTower += 4;
                hints.AddForbiddenZone(ShapeContains.InvertedCircle(Module.Center + _towerOffsets[soakedTower], _towerRadius), _towerActivation);
            }
            else
            {
                foreach (int i in _activeTowers.SetBits())
                    hints.AddForbiddenZone(ShapeContains.Circle(Module.Center + _towerOffsets[i], _towerRadius), _towerActivation);
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (int i in _activeTowers.SetBits())
            Arena.AddCircle(Module.Center + _towerOffsets[i], _towerRadius, _soakedTowers[i] ? ArenaColor.Safe : ArenaColor.Danger);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.GoodKingsDecree3:
                _activationDelay = 16;
                _cometsLeft = 3;
                break;
            case AID.MogCometAOE:
                --_cometsLeft;
                break;
        }
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index is >= 8 and < 16)
        {
            var towerIndex = index - 8;
            switch (state)
            {
                case 0x00020001:
                    _activeTowers.Set(towerIndex);
                    _towerActivation = WorldState.FutureTime(_activationDelay);
                    break;
                case 0x00200010:
                    _soakedTowers.Set(towerIndex);
                    break;
                case 0x00400001:
                    _soakedTowers.Clear(towerIndex);
                    break;
                case 0x00080004:
                    _activeTowers.Clear(towerIndex);
                    _soakedTowers.Clear(towerIndex);
                    break;
            }
        }
    }
}
