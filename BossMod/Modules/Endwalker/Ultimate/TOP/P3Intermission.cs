using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Ultimate.TOP
{
    class P3SniperCannon : Components.UniformStackSpread
    {
        public P3SniperCannon() : base(6, 6) { }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            switch ((SID)status.ID)
            {
                case SID.SniperCannonFodder:
                    AddSpread(actor, status.ExpireAt);
                    InitAssignments(module);
                    break;
                case SID.HighPoweredSniperCannonFodder:
                    AddStack(actor, status.ExpireAt);
                    InitAssignments(module);
                    break;
            }
        }

        private void InitAssignments(BossModule module)
        {
            if (Spreads.Count < 4 || Stacks.Count < 2)
                return; // too early

            // TODO: implement
        }
    }

    class P3WaveRepeater : Components.ConcentricAOEs
    {
        private static AOEShape[] _shapes = { new AOEShapeCircle(6), new AOEShapeDonut(6, 12), new AOEShapeDonut(12, 18), new AOEShapeDonut(18, 24) };

        public P3WaveRepeater() : base(_shapes) { }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.WaveRepeater1)
                AddSequence(caster.Position, spell.FinishAt);
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            var order = (AID)spell.Action.ID switch
            {
                AID.WaveRepeater1 => 0,
                AID.WaveRepeater2 => 1,
                AID.WaveRepeater3 => 2,
                AID.WaveRepeater4 => 3,
                _ => -1
            };
            if (!AdvanceSequence(order, caster.Position, module.WorldState.CurrentTime.AddSeconds(2.1f)))
                module.ReportError(this, $"Unexpected ring {order}");
        }
    }

    class P3ColossalBlow : Components.GenericAOEs
    {
        public List<AOEInstance> AOEs = new();

        private static AOEShapeCircle _shape = new(11);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => AOEs.Take(3);

        public override void OnActorPlayActionTimelineEvent(BossModule module, Actor actor, ushort id)
        {
            if ((OID)actor.OID is OID.LeftArmUnit or OID.RightArmUnit && id is 0x1E43 or 0x1E44)
                AOEs.Add(new(_shape, actor.Position, default, module.WorldState.CurrentTime.AddSeconds(13.5f)));
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.ColossalBlow)
            {
                ++NumCasts;
                AOEs.RemoveAll(aoe => aoe.Origin.AlmostEqual(caster.Position, 1));
            }
        }
    }
}
