using System.Linq;

namespace BossMod.Stormblood.Ultimate.UWU
{
    // TODO: is it baited on farthest dps or any roles? can subsequent eruptions bait on other targets?
    // casts are 3s long and 2s apart (overlapping)
    class P2Eruption : Components.LocationTargetedAOEs
    {
        public int NumCastsStarted { get; private set; }
        private BitMask _baiters;

        public P2Eruption() : base(ActionID.MakeSpell(AID.EruptionAOE), 8) { }

        public override void Update(BossModule module)
        {
            if (NumCastsStarted == 0)
            {
                var source = ((UWU)module).Ifrit();
                if (source != null)
                    _baiters = module.Raid.WithSlot().WhereActor(a => a.Class.IsDD()).SortedByRange(source.Position).TakeLast(2).Mask();
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_baiters[pcSlot])
                arena.AddCircle(pc.Position, Shape.Radius, ArenaColor.Safe);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            base.OnCastStarted(module, caster, spell);
            if ((AID)spell.Action.ID == AID.EruptionAOE)
            {
                if (NumCastsStarted < 2)
                {
                    if (NumCastsStarted == 0)
                        _baiters.Reset();
                    var (baiterSlot, baiter) = module.Raid.WithSlot().ExcludedFromMask(_baiters).Closest(spell.LocXZ);
                    if (baiter != null)
                        _baiters.Set(baiterSlot);
                }
                ++NumCastsStarted;
                if (NumCastsStarted >= 8)
                    _baiters.Reset();
            }
        }
    }
}
