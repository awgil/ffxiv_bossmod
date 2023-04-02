using System.Collections.Generic;
using System.Linq;

namespace BossMod.Shadowbringers.Ultimate.TEA
{
    class P2EarthMissileBaited : Components.PersistentVoidzoneAtCastTarget
    {
        public P2EarthMissileBaited() : base(5, ActionID.MakeSpell(AID.EarthMissileBaited), m => m.Enemies(OID.VoidzoneEarthMissileBaited).Where(z => z.EventState != 7), 0.9f) { }
    }

    class P2EarthMissileIce : Components.PersistentVoidzoneAtCastTarget
    {
        // TODO: verify larger radius...
        public P2EarthMissileIce() : base(10, ActionID.MakeSpell(AID.EarthMissileIce), Voidzones, 0.8f) { }

        private static IEnumerable<Actor> Voidzones(BossModule m)
        {
            foreach (var z in m.Enemies(OID.VoidzoneEarthMissileIceSmall).Where(z => z.EventState != 7))
            {
                yield return z;
                yield break;
            }
            foreach (var z in m.Enemies(OID.VoidzoneEarthMissileIceLarge).Where(z => z.EventState != 7))
            {
                yield return z;
                yield break;
            }
        }
    }

    // note: we use a single spread/stack component for both enumerations and ice missile spreads, since they happen at the same time
    // TODO: add hint for spread target to stay close to tornado...
    class P2Enumeration : Components.UniformStackSpread
    {
        public P2Enumeration() : base(5, 6, 3, 3, true, false) { } // TODO: verify enumeration radius

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            switch ((IconID)iconID)
            {
                case IconID.Enumeration:
                    // note: we assume tanks never share enumeration
                    AddStack(actor, module.WorldState.CurrentTime.AddSeconds(5.1f), module.Raid.WithSlot(true).WhereActor(p => p.Role == Role.Tank).Mask());
                    break;
                case IconID.EarthMissileIce:
                    AddSpread(actor, module.WorldState.CurrentTime.AddSeconds(5.1f));
                    break;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.Enumeration:
                    Stacks.Clear();
                    break;
                case AID.EarthMissileIce:
                    Spreads.Clear();
                    break;
            }
        }
    }

    class P2HiddenMinefield : Components.SelfTargetedAOEs
    {
        private List<WPos> _mines = new();

        public P2HiddenMinefield() : base(ActionID.MakeSpell(AID.HiddenMinefield), new AOEShapeCircle(5)) { }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var m in _mines)
                arena.Actor(m, default, ArenaColor.Object);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            base.OnCastStarted(module, caster, spell);
            if (spell.Action == WatchedAction)
                _mines.Add(caster.Position);
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            base.OnEventCast(module, caster, spell);
            if ((AID)spell.Action.ID is AID.HiddenMine or AID.HiddenMineShrapnel)
                _mines.RemoveAll(m => m.AlmostEqual(caster.Position, 1));
        }
    }
}
