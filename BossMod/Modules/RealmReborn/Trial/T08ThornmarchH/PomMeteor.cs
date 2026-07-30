namespace BossMod.RealmReborn.Trial.T08ThornmarchH;

class PomMeteor(BossModule module) : BossComponent(module)
{
    private BitMask _activeTowers;
    private BitMask _soakedTowers;
    private DateTime _towerActivation;
    private int _cometsLeft;
    private float _activationDelay = 8f; // 8s for first set of towers, then 16s for others

    private const float _towerRadius = 5f;
    private const float _cometAvoidRadius = 6f;
    private static readonly WDir[] _towerOffsets = GetTowerOffsets();
    private static WDir[] GetTowerOffsets()
    {
        var towerOffsets = new WDir[8];
        for (var i = 0; i < 8; ++i)
            towerOffsets[i] = 10f * (45f * i).Degrees().ToDirection();
        return towerOffsets;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_activeTowers.None())
            return;

        if (_cometsLeft > 0)
        {
            foreach (var i in _activeTowers.SetBits())
                hints.AddForbiddenZone(new SDCircle(Arena.Center + _towerOffsets[i], _cometAvoidRadius));
        }
        else
        {
            // assume H1/H2/R1/R2 soak towers
            var soakedTower = assignment switch
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
                hints.AddForbiddenZone(new SDInvertedCircle(Arena.Center + _towerOffsets[soakedTower], _towerRadius), _towerActivation);
            }
            else
            {
                foreach (var i in _activeTowers.SetBits())
                    hints.AddForbiddenZone(new SDCircle(Arena.Center + _towerOffsets[i], _towerRadius), _towerActivation);
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var i in _activeTowers.SetBits())
            Arena.ZoneCircleOutline(Arena.Center + _towerOffsets[i], _towerRadius, _soakedTowers[i] ? Colors.Safe : 0);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.GoodKingsDecree3:
                _activationDelay = 16;
                _cometsLeft = 3;
                break;
            case (uint)AID.MogCometAOE:
                --_cometsLeft;
                break;
        }
    }

    public override void OnMapEffect(byte index, uint state)
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
