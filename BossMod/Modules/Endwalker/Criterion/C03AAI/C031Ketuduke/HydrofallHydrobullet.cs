using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Criterion.C03AAI.C031Ketuduke
{
    class HydrofallHydrobullet : Components.UniformStackSpread
    {
        public struct Mechanic
        {
            public bool Spread;
            public BitMask Targets;
            public DateTime Activation;
        }

        public int ActiveMechanic { get; private set; } = -1;
        public List<Mechanic> Mechanics = new();

        public HydrofallHydrobullet() : base(6, 15) { }

        public void Activate(BossModule module, int index)
        {
            if (ActiveMechanic == index)
                return;
            ActiveMechanic = index;
            Stacks.Clear();
            Spreads.Clear();
            if (index >= 0 && index < Mechanics.Count)
            {
                ref var m = ref Mechanics.AsSpan()[index];
                if (m.Spread)
                    AddSpreads(module.Raid.WithSlot(true).IncludedInMask(m.Targets).Actors(), m.Activation);
                else
                    AddStacks(module.Raid.WithSlot(true).IncludedInMask(m.Targets).Actors(), m.Activation);
            }
        }

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            var firstMech = Math.Max(ActiveMechanic, 0);
            if (Mechanics.Count > firstMech)
                hints.Add(string.Join(" -> ", Mechanics.Skip(firstMech).Select(m => m.Spread ? "Spread" : "Stack")));
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            if ((SID)status.ID is SID.HydrofallTarget or SID.HydrobulletTarget && Mechanics.Count > 0)
            {
                ref var m = ref Mechanics.AsSpan()[Mechanics.Count - 1];
                if (m.Spread != ((SID)status.ID == SID.HydrobulletTarget))
                {
                    module.ReportError(this, $"Unexpected SID: {status.ID}");
                    return;
                }
                m.Targets.Set(module.Raid.FindSlot(actor.InstanceID));
                m.Activation = status.ExpireAt;
                if (ActiveMechanic == Mechanics.Count - 1)
                {
                    if (m.Spread)
                        AddSpread(actor, status.ExpireAt);
                    else
                        AddStack(actor, status.ExpireAt);
                }
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.Hydrofall:
                    Mechanics.Add(new() { Spread = false });
                    break;
                case AID.Hydrobullet:
                    Mechanics.Add(new() { Spread = true });
                    break;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.HydrofallSecond:
                    Mechanics.Add(new() { Spread = false });
                    break;
                case AID.HydrobulletSecond:
                    Mechanics.Add(new() { Spread = true });
                    break;
                case AID.NHydrofallAOE:
                case AID.SHydrofallAOE:
                    if (ActiveMechanic >= 0 && ActiveMechanic < Mechanics.Count && !Mechanics[ActiveMechanic].Spread)
                        Activate(module, ActiveMechanic + 1);
                    break;
                case AID.NHydrobulletAOE:
                case AID.SHydrobulletAOE:
                    if (ActiveMechanic >= 0 && ActiveMechanic < Mechanics.Count && Mechanics[ActiveMechanic].Spread)
                        Activate(module, ActiveMechanic + 1);
                    break;
            }
        }
    }
}
