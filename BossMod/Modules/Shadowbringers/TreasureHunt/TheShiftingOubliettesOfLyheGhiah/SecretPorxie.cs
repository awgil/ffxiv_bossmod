// CONTRIB: made by malediktus, not checked
using System.Linq;

namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.SecretPorxie
{
    public enum OID : uint
    {
        Boss = 0x3014, //R=1.2
        BossHelper = 0x233C,
        MagickedBroomHelper = 0x310A, // R=0.5
        MagickedBroom1 = 0x30F3, // R=3.125
        MagickedBroom2 = 0x30F2, // R=3.125
        MagickedBroom3 = 0x30F4, // R=3.125
        MagickedBroom4 = 0x30F1, // R=3.125
        MagickedBroom5 = 0x3015, // R=3.125
        MagickedBroom6 = 0x30F0, // R=3.125
        BonusAdd_TheKeeperOfTheKeys = 0x3034, // R3.230
    };

    public enum AID : uint
    {
        AutoAttack = 872, // Boss/BonusAdd_TheKeeperOfTheKeys->player, no cast, single-target
        BrewingStorm = 21670, // Boss->self, 2,5s cast, range 5 60-degree cone
        HarrowingDream = 21671, // Boss->self, 3,0s cast, range 6 circle, applies sleep
        BecloudingDust = 22935, // Boss->self, 3,0s cast, single-target
        BecloudingDust2 = 22936, // BossHelper->location, 3,0s cast, range 6 circle
        SweepStart = 22937, // 30F4/30F3/30F2/30F1/3015/30F0->self, 4,0s cast, range 6 circle
        SweepRest = 21672, // 30F4/30F3/30F2/30F1/3015/30F0->self, no cast, range 6 circle
        SweepVisual = 22508, // 30F4/30F3/30F2/30F1/3015/30F0->self, no cast, single-target, visual
        SweepVisual2 = 22509, // 30F4/30F3/30F2/30F1/3015/30F0->self, no cast, single-target

        Telega = 9630, // BonusAdds->self, no cast, single-target, bonus adds disappear
        Mash = 21767, // 3034->self, 3,0s cast, range 13 width 4 rect
        Inhale = 21770, // 3034->self, no cast, range 20 120-degree cone, attract 25 between hitboxes, shortly before Spin
        Spin = 21769, // 3034->self, 4,0s cast, range 11 circle
        Scoop = 21768, // 3034->self, 4,0s cast, range 15 120-degree cone
    };


    class BrewingStorm : Components.SelfTargetedAOEs
    {
        public BrewingStorm() : base(ActionID.MakeSpell(AID.BrewingStorm), new AOEShapeCone(5, 30.Degrees())) { }
    }

    class HarrowingDream : Components.SelfTargetedAOEs
    {
        public HarrowingDream() : base(ActionID.MakeSpell(AID.HarrowingDream), new AOEShapeCircle(6)) { }
    }

    class BecloudingDust : Components.LocationTargetedAOEs
    {
        public BecloudingDust() : base(ActionID.MakeSpell(AID.BecloudingDust2), 6) { }
    }

    class Sweep : Components.Exaflare
    {
        public Sweep() : base(6) { }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.SweepStart)
                Lines.Add(new() { Next = caster.Position, Advance = 12 * spell.Rotation.ToDirection(), NextExplosion = spell.NPCFinishAt.AddSeconds(0.9f), TimeToMove = 4.5f, ExplosionsLeft = 4, MaxShownExplosions = 3 });
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if (Lines.Count > 0 && (AID)spell.Action.ID == AID.SweepRest)
            {
                int index = Lines.FindIndex(item => item.Next.AlmostEqual(caster.Position, 1));
                AdvanceLine(module, Lines[index], caster.Position);
                if (Lines[index].ExplosionsLeft == 0)
                    Lines.RemoveAt(index);
            }
        }
    }

    class Spin : Components.SelfTargetedAOEs
    {
        public Spin() : base(ActionID.MakeSpell(AID.Spin), new AOEShapeCircle(11)) { }
    }

    class Mash : Components.SelfTargetedAOEs
    {
        public Mash() : base(ActionID.MakeSpell(AID.Mash), new AOEShapeRect(13, 2)) { }
    }

    class Scoop : Components.SelfTargetedAOEs
    {
        public Scoop() : base(ActionID.MakeSpell(AID.Scoop), new AOEShapeCone(15, 60.Degrees())) { }
    }

    class PorxieStates : StateMachineBuilder
    {
        public PorxieStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<BrewingStorm>()
                .ActivateOnEnter<HarrowingDream>()
                .ActivateOnEnter<BecloudingDust>()
                .ActivateOnEnter<Sweep>()
                .ActivateOnEnter<Spin>()
                .ActivateOnEnter<Mash>()
                .ActivateOnEnter<Scoop>()
                .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BonusAdd_TheKeeperOfTheKeys).All(e => e.IsDead);
        }
    }

    [ModuleInfo(CFCID = 745, NameID = 9795)]
    public class Porxie : BossModule
    {
        public Porxie(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 20)) { }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
            foreach (var s in Enemies(OID.BonusAdd_TheKeeperOfTheKeys))
                Arena.Actor(s, ArenaColor.Vulnerable);
        }

        public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.CalculateAIHints(slot, actor, assignment, hints);
            foreach (var e in hints.PotentialTargets)
            {
                e.Priority = (OID)e.Actor.OID switch
                {
                    OID.BonusAdd_TheKeeperOfTheKeys => 2,
                    OID.Boss => 1,
                    _ => 0
                };
            }
        }
    }
}
