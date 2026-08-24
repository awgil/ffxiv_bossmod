namespace BossMod.Stormblood.Ultimate.UWU;

// TODO: is it baited on farthest dps or any roles? can subsequent eruptions bait on other targets?
// casts are 3s long and 2s apart (overlapping)
class P2Eruption(BossModule module) : Components.StandardAOEs(module, AID.EruptionAOE, 8)
{
    public int NumCastsStarted { get; private set; }
    private BitMask _baiters;

    public override void Update()
    {
        if (NumCastsStarted == 0)
        {
            var source = ((UWU)Module).Ifrit();
            if (source != null)
                _baiters = Raid.WithSlot().WhereActor(a => a.Class.IsDD()).SortedByRange(source.Position).TakeLast(2).Mask();
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_baiters[pcSlot])
            Arena.AddCircle(pc.Position, ((AOEShapeCircle)Shape).Radius, ArenaColor.Safe);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if ((AID)spell.Action.ID == AID.EruptionAOE)
        {
            if (NumCastsStarted < 2)
            {
                if (NumCastsStarted == 0)
                    _baiters.Reset();
                var (baiterSlot, baiter) = Raid.WithSlot().ExcludedFromMask(_baiters).Closest(spell.LocXZ);
                if (baiter != null)
                    _baiters.Set(baiterSlot);
            }
            ++NumCastsStarted;
            if (NumCastsStarted >= 8)
                _baiters.Reset();
        }
    }
}
