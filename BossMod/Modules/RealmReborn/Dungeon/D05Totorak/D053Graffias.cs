using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod.RealmReborn.Dungeon.D05Totorak.D053Graffias
{
    public enum OID : uint
    {
        Boss = 0x103, // x1
        FleshyPod = 0x23C, // spawn during fight
        Comesmite = 0x104, // spawn during fight
        GraffiasTail = 0x10A, // spawn during fight
        PollenZone = 0x1E8614, // spawn during fight
    };

    public enum AID : uint
    {
        AutoAttack = 870, // Boss/Comesmite->player, no cast
        RealmShaker = 697, // Boss->self, no cast, range 9 raidwide around boss
        Silkscreen = 701, // Boss->self, 2.5s cast, range 18 width 4 rect aoe
        StickyWeb = 698, // Boss->player, 1.5s cast, visual (spawns fleshy pod at target)
        PodBurst = 730, // FleshyPod->self, 3.0s cast, range 7.050 aoe
        TailMolt = 704, // Boss->self, no cast, visual (spawns tail)
        DeadlyThrust = 702, // Boss->self, 2.0s cast, visual (spawns pollen zone)
    };

    class Silkscreen : Components.SelfTargetedLegacyRotationAOEs
    {
        public Silkscreen() : base(ActionID.MakeSpell(AID.Silkscreen), new AOEShapeRect(18, 2)) { }
    }

    class StickyWeb : Components.CastHint
    {
        public StickyWeb() : base(ActionID.MakeSpell(AID.StickyWeb), "Delayed AOE at target") { }
    }

    class PodBurst : Components.SelfTargetedAOEs
    {
        public PodBurst() : base(ActionID.MakeSpell(AID.PodBurst), new AOEShapeCircle(7.050f)) { }
    }

    class DeadlyThrust : Components.CastHint
    {
        public DeadlyThrust() : base(ActionID.MakeSpell(AID.DeadlyThrust), "Persistent voidzone at target") { }
    }

    class PollenZone : Components.PersistentVoidzone
    {
        public PollenZone() : base(10, m => m.Enemies(OID.PollenZone)) { }
    }

    class D053GraffiasStates : StateMachineBuilder
    {
        public D053GraffiasStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<Silkscreen>()
                .ActivateOnEnter<StickyWeb>()
                .ActivateOnEnter<PodBurst>()
                .ActivateOnEnter<DeadlyThrust>()
                .ActivateOnEnter<PollenZone>();
        }
    }

    public class D053Graffias : BossModule
    {
        public D053Graffias(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(215, -145), 20)) { }

        public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.CalculateAIHints(slot, actor, assignment, hints);

            bool haveTail = Enemies(OID.GraffiasTail).Count > 0;
            foreach (var e in hints.PotentialTargets)
            {
                e.Priority = (OID)e.Actor.OID switch
                {
                    OID.GraffiasTail => 2,
                    OID.Comesmite => haveTail ? 2 : 1,
                    OID.Boss => 1,
                    _ => 0,
                };
            }
        }
    }
}
