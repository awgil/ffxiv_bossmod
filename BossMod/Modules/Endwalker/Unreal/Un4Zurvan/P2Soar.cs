using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Unreal.Un4Zurvan
{
    class P2SoarTwinSpirit : Components.GenericAOEs
    {
        private List<(Actor caster, AOEInstance aoe)> _pending = new();

        private AOEShapeRect _shape = new(50, 5);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _pending.Select(p => p.aoe);

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.TwinSpiritFirst)
            {
                _pending.Add((caster, new(_shape, caster.Position, Angle.FromDirection(spell.LocXZ - caster.Position), spell.NPCFinishAt)));
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.TwinSpiritFirst:
                    var index = _pending.FindIndex(p => p.caster == caster);
                    if (index >= 0)
                        _pending[index] = (caster, new(_shape, spell.LocXZ, Angle.FromDirection(module.Bounds.Center - spell.LocXZ), module.WorldState.CurrentTime.AddSeconds(9.2f)));
                    break;
                case AID.TwinSpiritSecond:
                    _pending.RemoveAll(p => p.caster == caster);
                    ++NumCasts;
                    break;
            }
        }
    }

    class P2SoarFlamingHalberd : Components.UniformStackSpread
    {
        public P2SoarFlamingHalberd() : base(0, 12, alwaysShowSpreads: true) { }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == (uint)IconID.FlamingHalberd)
                AddSpread(actor, module.WorldState.CurrentTime.AddSeconds(5.1f));
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.FlamingHalberd)
                Spreads.Clear(); // don't bother finding proper target, they all happen at the same time
        }
    }

    class P2SoarFlamingHalberdVoidzone : Components.PersistentVoidzone
    {
        public P2SoarFlamingHalberdVoidzone() : base(8, m => m.Enemies(OID.FlamingHalberdVoidzone).Where(z => z.EventState != 7)) { }
    }

    class P2SoarDemonicDiveCoolFlame : Components.UniformStackSpread
    {
        public P2SoarDemonicDiveCoolFlame() : base(7, 8, 7, alwaysShowSpreads: true) { }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            switch ((IconID)iconID)
            {
                case IconID.DemonicDive:
                    AddStack(actor, module.WorldState.CurrentTime.AddSeconds(5.1f));
                    break;
                case IconID.CoolFlame:
                    AddSpread(actor, module.WorldState.CurrentTime.AddSeconds(5.1f));
                    break;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.DemonicDive:
                    Stacks.Clear();
                    break;
                case AID.CoolFlame:
                    Spreads.Clear();
                    break;
            }
        }
    }
}
