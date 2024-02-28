using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Unreal.Un3Sophia
{
    // shows all three demiurges + handles directional parry from first; the reason is to simplify condition checks
    class Demiurges : Components.DirectionalParry
    {
        private IReadOnlyList<Actor> _second = ActorEnumeration.EmptyList;
        private IReadOnlyList<Actor> _third = ActorEnumeration.EmptyList;

        public bool AddsActive => ActiveActors.Any() || _second.Any(a => a.IsTargetable && !a.IsDead) || _third.Any(a => a.IsTargetable && !a.IsDead);

        public Demiurges() : base((uint)OID.Demiurge1) { }

        public override void Init(BossModule module)
        {
            base.Init(module);
            _second = module.Enemies(OID.Demiurge2);
            _third = module.Enemies(OID.Demiurge3);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            base.DrawArenaForeground(module, pcSlot, pc, arena);
            arena.Actors(_second, ArenaColor.Enemy);
            arena.Actors(_third, ArenaColor.Enemy);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            var sides = (AID)spell.Action.ID switch
            {
                AID.VerticalKenoma => Side.Front | Side.Back,
                AID.HorizontalKenoma => Side.Left | Side.Right,
                _ => Side.None
            };
            if (sides != Side.None)
                PredictParrySide(caster.InstanceID, sides);
        }
    }

    class DivineSpark : Components.CastGaze
    {
        public DivineSpark() : base(ActionID.MakeSpell(AID.DivineSpark)) { }
    }

    class GnosticRant : Components.SelfTargetedAOEs
    {
        public GnosticRant() : base(ActionID.MakeSpell(AID.GnosticRant), new AOEShapeCone(40, 135.Degrees())) { }
    }

    class GnosticSpear : Components.SelfTargetedAOEs
    {
        public GnosticSpear() : base(ActionID.MakeSpell(AID.GnosticSpear), new AOEShapeRect(20.75f, 2, 0.75f)) { }
    }

    class RingOfPain : Components.PersistentVoidzoneAtCastTarget
    {
        public RingOfPain() : base(5, ActionID.MakeSpell(AID.RingOfPain), m => m.Enemies(OID.RingOfPain).Where(z => z.EventState != 7), 1.7f) { }
    }

    class Infusion : Components.GenericWildCharge
    {
        public Infusion() : base(5, ActionID.MakeSpell(AID.Infusion)) { }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
            {
                Source = caster;
                foreach (var (slot, player) in module.Raid.WithSlot())
                {
                    PlayerRoles[slot] = player.InstanceID == spell.TargetID ? PlayerRole.Target : PlayerRole.Share;
                }
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                Source = null;
        }
    }
}
