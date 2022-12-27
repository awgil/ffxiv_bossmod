using System.Collections.Generic;

namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE44FamiliarFace
{
    public enum OID : uint
    {
        Boss = 0x2DD2, // R9.450, x1
        PhantomHashmal = 0x3321, // R9.450, x1
        Helper = 0x233C, // R0.500, x24
        ArenaFeatures = 0x1EA1A1, // R2.000, x9, EventObj type
        Tower = 0x1EB17E, // R0.500, EventObj type, spawn during fight
        FallingTower = 0x1EB17D, // R0.500, EventObj type, spawn during fight, rotation at spawn determines fall direction?..
        Hammer = 0x1EB17F, // R0.500, EventObj type, spawn during fight
    };

    public enum AID : uint
    {
        AutoAttack = 6497, // Boss->player, no cast, single-target
        TectonicEruption = 23826, // Helper->location, 4.0s cast, range 6 circle puddle
        RockCutter = 23827, // Boss->player, 5.0s cast, single-target, tankbuster
        AncientQuake = 23828, // Boss->self, 5.0s cast, single-target, visual
        AncientQuakeAOE = 23829, // Helper->self, no cast, ??? (x2 ~0.8s after visual)
        Sanction = 23817, // Boss->self, no cast, single-target, visual (light raidwide)
        SanctionAOE = 23832, // Helper->self, no cast, ??? (x2 ~0.8s after visual)
        Roxxor = 23823, // Helper->players, 5.0s cast, range 6 circle spread

        ControlTowerAppear = 23830, // Helper->self, 4.0s cast, range 6 circle aoe around appearing towers
        TowerRound = 23831, // Boss->self, 4.0s cast, single-target, visual (spawns 2 towers + light raidwide)
        TowerRoundAOE = 23834, // Helper->self, no cast, ??? (x2 ~0.8s after visual)
        ControlTower = 23816, // Boss->self, 4.0s cast, single-target, visual (spawns 3 towers + light raidwide)
        ControlTowerAOE = 23833, // Helper->self, no cast, ??? (x2 ~0.8s after visual)
        Towerfall = 23818, // Helper->self, 7.0s cast, range 40 width 10 rect aoe

        PhantomOrder = 24702, // Boss->self, 4.0s cast, single-target, visual
        ExtremeEdgeR = 23821, // PhantomHashmal->self, 8.0s cast, range 60 width 36 rect aoe offset to the right
        ExtremeEdgeL = 23822, // PhantomHashmal->self, 8.0s cast, range 60 width 36 rect aoe offset to the left

        IntractableLand = 24576, // Boss->self, 5.0s cast, single-target, visual (double exaflares)
        IntractableLandFirst = 23819, // Helper->self, 5.3s cast, range 8 circle
        IntractableLandRest = 23820, // Helper->location, no cast, range 8 circle

        HammerRound = 23824, // Boss->self, 5.0s cast, single-target, visual
        Hammerfall = 23825, // Helper->self, 8.0s cast, range 37 circle aoe
    };

    class TectonicEruption : Components.LocationTargetedAOEs
    {
        public TectonicEruption() : base(ActionID.MakeSpell(AID.TectonicEruption), 6) { }
    }

    class RockCutter : Components.SingleTargetCast
    {
        public RockCutter() : base(ActionID.MakeSpell(AID.RockCutter)) { }
    }

    class AncientQuake : Components.RaidwideCast
    {
        public AncientQuake() : base(ActionID.MakeSpell(AID.AncientQuake)) { }
    }

    class Roxxor : Components.SpreadFromCastTargets
    {
        public Roxxor() : base(ActionID.MakeSpell(AID.Roxxor), 6) { }
    }

    class ControlTowerAppear : Components.SelfTargetedAOEs
    {
        public ControlTowerAppear() : base(ActionID.MakeSpell(AID.ControlTowerAppear), new AOEShapeCircle(6)) { }
    }

    class Towerfall : Components.SelfTargetedAOEs
    {
        public Towerfall() : base(ActionID.MakeSpell(AID.Towerfall), new AOEShapeRect(40, 5)) { }
    }

    class ExtremeEdge : Components.GenericAOEs
    {
        private List<(Actor caster, float offset)> _casters = new();
        private static AOEShapeRect _shape = new(60, 18);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            foreach (var c in _casters)
                yield return new(_shape, c.caster.Position + c.offset * c.caster.CastInfo!.Rotation.ToDirection().OrthoL(), c.caster.CastInfo.Rotation, c.caster.CastInfo.FinishAt);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            var offset = (AID)spell.Action.ID switch
            {
                AID.ExtremeEdgeL => 15,
                AID.ExtremeEdgeR => -15,
                _ => 0
            };
            if (offset != 0)
                _casters.Add((caster, offset));
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.ExtremeEdgeL or AID.ExtremeEdgeR)
                _casters.RemoveAll(c => c.caster == caster);
        }
    }

    class IntractableLand : Components.Exaflare
    {
        public IntractableLand() : base(8) { }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.IntractableLandFirst)
            {
                Lines.Add(new() { Next = caster.Position, Advance = 8 * spell.Rotation.ToDirection(), LastExplosion = spell.FinishAt.AddSeconds(-0.8f), TimeToMove = 0.8f, ExplosionsLeft = 8, MaxShownExplosions = 4 });
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.IntractableLandFirst or AID.IntractableLandRest)
            {
                var pos = (AID)spell.Action.ID == AID.IntractableLandFirst ? caster.Position : spell.TargetXZ;
                int index = Lines.FindIndex(item => item.Next.AlmostEqual(pos, 1));
                if (index == -1)
                {
                    module.ReportError(this, $"Failed to find entry for {caster.InstanceID:X}");
                    return;
                }

                AdvanceLine(module, Lines[index], pos);
                if (Lines[index].ExplosionsLeft == 0)
                    Lines.RemoveAt(index);
            }
        }
    }

    // TODO: consider prediction - actor-create happens ~4.7s before cast start
    class Hammerfall : Components.SelfTargetedAOEs
    {
        public Hammerfall() : base(ActionID.MakeSpell(AID.Hammerfall), new AOEShapeCircle(37)) { }
    }

    class CE44FamiliarFaceStates : StateMachineBuilder
    {
        public CE44FamiliarFaceStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<TectonicEruption>()
                .ActivateOnEnter<RockCutter>()
                .ActivateOnEnter<AncientQuake>()
                .ActivateOnEnter<Roxxor>()
                .ActivateOnEnter<ControlTowerAppear>()
                .ActivateOnEnter<Towerfall>()
                .ActivateOnEnter<ExtremeEdge>()
                .ActivateOnEnter<IntractableLand>()
                .ActivateOnEnter<Hammerfall>();
        }
    }

    public class CE44FamiliarFace : BossModule
    {
        public CE44FamiliarFace(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(330, 390), 30)) { }
    }
}
