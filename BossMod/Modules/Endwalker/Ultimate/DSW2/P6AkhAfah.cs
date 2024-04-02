namespace BossMod.Endwalker.Ultimate.DSW2;

class P6HPCheck : BossComponent
{
    private Actor? _nidhogg;
    private Actor? _hraesvelgr;

    public override void Init(BossModule module)
    {
        _nidhogg = module.Enemies(OID.NidhoggP6).FirstOrDefault();
        _hraesvelgr = module.Enemies(OID.HraesvelgrP6).FirstOrDefault();
    }

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        if (_nidhogg != null && _hraesvelgr != null)
        {
            var diff = (int)(_nidhogg.HP.Cur - _hraesvelgr.HP.Cur) * 100.0f / _nidhogg.HP.Max;
            hints.Add($"Nidhogg HP: {(diff > 0 ? "+" : "")}{diff:f1}%");
        }
    }
}

class P6AkhAfah : Components.UniformStackSpread
{
    public bool Done { get; private set; }

    public P6AkhAfah() : base(4, 0, 4) { }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AkhAfahN)
            AddStacks(module.Raid.WithoutSlot(true).Where(p => p.Role == Role.Healer));
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.AkhAfahHAOE or AID.AkhAfahNAOE)
        {
            Stacks.Clear();
            Done = true;
        }
    }
}
