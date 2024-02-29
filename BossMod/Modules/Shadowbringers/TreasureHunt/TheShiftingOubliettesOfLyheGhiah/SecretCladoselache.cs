// CONTRIB: made by malediktus, not checked
using System.Linq;

namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.SecretCladoselache
{
    public enum OID : uint
    {
        Boss = 0x3027, //R=2.47
        BossAdd = 0x3028, //R=3.0 
        BossHelper = 0x233C,
    };

    public enum AID : uint
    {
        AutoAttack = 870, // BossAdd->player, no cast, single-target
        AutoAttack2 = 872, // Boss->player, no cast, single-target
        PelagicCleaver = 21705, // Boss->self, 3,5s cast, range 40 60-degree cone
        TidalGuillotine = 21704, // Boss->self, 4,0s cast, range 13 circle
        ProtolithicPuncture = 21703, // Boss->player, 4,0s cast, single-target
        PelagicCleaverRotationStart = 21706, // Boss->self, 5,0s cast, range 40 60-degree cone
        PelagicCleaverDuringRotation = 21707, // Boss->self, no cast, range 40 60-degree cone
        BiteAndRun = 21709, // BossAdd->player, 5,0s cast, width 5 rect charge
        AquaticLance = 21708, // Boss->player, 5,0s cast, range 8 circle
    };

    public enum IconID : uint
    {
        spreadmarker = 135, // player
        RotateCCW = 168, // Boss
        RotateCW = 167, // Boss
    };

    class PelagicCleaverRotation : Components.GenericRotatingAOE
    {
        private Angle _increment;
        private static readonly AOEShapeCone _shape = new(40, 30.Degrees());

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == (uint)IconID.RotateCW)
                _increment = -60.Degrees();
            if (iconID == (uint)IconID.RotateCCW)
                _increment = 60.Degrees();
        }
        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.PelagicCleaverRotationStart)
                Sequences.Add(new(_shape, caster.Position, spell.Rotation, _increment, spell.NPCFinishAt, 2.1f, 6));
        }
        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if (Sequences.Count > 0 && (AID)spell.Action.ID is AID.PelagicCleaverRotationStart or AID.PelagicCleaverDuringRotation)
                AdvanceSequence(0, module.WorldState.CurrentTime);
        }
    }

    class PelagicCleaver : Components.SelfTargetedAOEs
    {
        public PelagicCleaver() : base(ActionID.MakeSpell(AID.PelagicCleaver), new AOEShapeCone(40, 30.Degrees())) { }
    }

    class TidalGuillotine : Components.SelfTargetedAOEs
    {
        public TidalGuillotine() : base(ActionID.MakeSpell(AID.TidalGuillotine), new AOEShapeCircle(13)) { }
    }

    class ProtolithicPuncture : Components.SingleTargetCast
    {
        public ProtolithicPuncture() : base(ActionID.MakeSpell(AID.ProtolithicPuncture)) { }
    }

    class BiteAndRun : Components.BaitAwayChargeCast
    {
        public BiteAndRun() : base(ActionID.MakeSpell(AID.BiteAndRun), 2.5f) { }
    }

    class AquaticLance : Components.UniformStackSpread
    {
        public AquaticLance() : base(0, 8, alwaysShowSpreads: true) { }
        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == (uint)IconID.spreadmarker)
            {
                AddSpread(actor);
            }
        }
        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.AquaticLance)
            {
                Spreads.Clear();
            }
        }
    }

    class CladoselacheStates : StateMachineBuilder
    {
        public CladoselacheStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<PelagicCleaver>()
                .ActivateOnEnter<PelagicCleaverRotation>()
                .ActivateOnEnter<TidalGuillotine>()
                .ActivateOnEnter<ProtolithicPuncture>()
                .ActivateOnEnter<BiteAndRun>()
                .ActivateOnEnter<AquaticLance>()
                .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BossAdd).All(e => e.IsDead);
        }
    }

    [ModuleInfo(CFCID = 745, NameID = 9778)]
    public class Cladoselache : BossModule
    {
        public Cladoselache(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 20)) { }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
            foreach (var s in Enemies(OID.BossAdd))
                Arena.Actor(s, ArenaColor.Object);
        }

        public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.CalculateAIHints(slot, actor, assignment, hints);
            foreach (var e in hints.PotentialTargets)
            {
                e.Priority = (OID)e.Actor.OID switch
                {
                    OID.BossAdd => 2,
                    OID.Boss => 1,
                    _ => 0
                };
            }
        }
    }
}
