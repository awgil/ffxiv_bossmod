//namespace BossMod.Shadowbringers.Foray.Dalriada.D5DiabloArmament;

//public enum OID : uint
//{
//    Boss = 0x31B3, // R28.500, x1
//    Helper = 0x233C, // R0.500, x33, mixed types
//    DiabolicBit = 0x31B4, // R1.200, x0 (spawn during fight)
//    Aether = 0x31B5, // R1.500, x0 (spawn during fight)

//    Gate1 = 0x1EB1D6,
//    Gate2 = 0x1EB1D7,
//    Gate3 = 0x1EB1D8,
//    Gate4 = 0x1EB1D9,
//}

//public enum AID : uint
//{
//    _Weaponskill_BrutalCamisado = 24899, // Boss->player, no cast, single-target
//    _Weaponskill_AethericExplosion = 23751, // Helper->self, 5.0s cast, ???
//    _Weaponskill_AethericExplosion1 = 23750, // Boss->self, 5.0s cast, single-target
//    _Weaponskill_AetherochemicalLaser = 23717, // Boss->self, no cast, range 60 width 60 rect
//    _Weaponskill_AetherochemicalLaser1 = 23716, // Boss->self, no cast, range 60 width 22 rect
//    _Weaponskill_Explosion = 24721, // Helper->self, 10.0s cast, range 60 width 22 rect
//    _Weaponskill_Explosion1 = 23719, // Helper->self, 10.0s cast, range 60 width 22 rect
//    _Weaponskill_Explosion2 = 23720, // Helper->self, 8.0s cast, range 60 width 22 rect
//    _Weaponskill_Explosion3 = 23718, // Helper->self, 10.0s cast, range 60 width 22 rect
//    _Weaponskill_Explosion4 = 24722, // Helper->self, 8.0s cast, range 60 width 22 rect
//    _Weaponskill_Explosion5 = 23721, // Helper->self, 8.0s cast, range 60 width 22 rect
//    _Weaponskill_Explosion6 = 23722, // Helper->self, 6.0s cast, range 60 width 22 rect
//    _Weaponskill_AdvancedDeathRay = 23748, // Boss->self, 5.0s cast, single-target
//    _Weaponskill_AdvancedDeathRay1 = 23749, // Helper->players, no cast, range 70 width 8 rect
//    _Weaponskill_DiabolicGate = 23711, // Boss->self, 4.0s cast, single-target
//    _Weaponskill_DiabolicGate1 = 25028, // Helper->self, 5.0s cast, ???
//    DiabolicGateDeathwall = 24994, // Helper->self, no cast, range ?-60 donut
//    _Weaponskill_RuinousPseudomen = 23712, // Boss->self, 15.0s cast, single-target
//    _Weaponskill_RuinousPseudomen1 = 24995, // Helper->self, 1.5s cast, range 80 width 24 rect
//    _Weaponskill_RuinousPseudomen2 = 23713, // Helper->self, 1.0s cast, single-target
//    _Weaponskill_RuinousPseudomen3 = 24908, // Helper->self, 1.5s cast, range 100 width 24 rect
//    _Weaponskill_RuinousPseudomen4 = 23714, // Boss->self, no cast, single-target
//    _Weaponskill_RuinousPseudomen5 = 24911, // Helper->self, 4.5s cast, range 80 width 24 rect
//    _Weaponskill_UltimatePseudoterror = 23715, // Boss->self, 4.0s cast, range 15-70 donut
//    _Weaponskill_MagitekBit = 23724, // Boss->self, 4.0s cast, single-target
//    DiabolicBitMove = 23725, // 31B4->location, no cast, single-target
//    _Weaponskill_AssaultCannon = 23726, // 31B4->self, 7.0s cast, range 100 width 6 rect
//    _Weaponskill_AdvancedDeathIV = 23727, // Boss->self, 4.0s cast, single-target
//    _Weaponskill_AdvancedDeathIV1 = 23728, // Helper->location, 7.0s cast, range 1 circle (but actually 10?)
//    _Weaponskill_LightPseudopillar = 23729, // Boss->self, 3.0s cast, single-target
//    _Weaponskill_LightPseudopillar1 = 23730, // Helper->location, 4.0s cast, range 10 circle
//    _Weaponskill_AethericBoom = 23732, // Helper->self, 5.0s cast, ???
//    _Weaponskill_AethericBoom1 = 23731, // Boss->self, 5.0s cast, single-target
//    _Weaponskill_Aetheroplasm = 23733, // 31B5->self, no cast, range 6 circle
//    _Weaponskill_DeadlyDealing = 23746, // Boss->location, 7.0s cast, range 6 circle
//    _Weaponskill_DeadlyDealing1 = 23747, // Helper->self, 7.5s cast, ???
//    _Weaponskill_VoidSystemsOverload = 23736, // Helper->self, 5.0s cast, ???
//    _Weaponskill_VoidSystemsOverload1 = 23735, // Boss->self, 5.0s cast, single-target
//    _Weaponskill_PillarOfShamash = 23737, // Helper->self, 8.0s cast, range 70 20-degree cone
//    _Weaponskill_PillarOfShamash1 = 23738, // Helper->self, 9.5s cast, range 70 20-degree cone
//    _Weaponskill_PillarOfShamash2 = 23739, // Helper->self, 11.0s cast, range 70 20-degree cone
//    _Ability_PillarOfShamash = 23741, // Helper->player, no cast, single-target
//    _Weaponskill_PillarOfShamash3 = 23740, // Helper->player, no cast, range 70 width 4 rect
//    _Weaponskill_PillarOfShamash4 = 23742, // Helper->players, no cast, range 70 width 8 rect
//    _Weaponskill_VoidSystemsOverload2 = 25364, // Boss->self, 5.0s cast, single-target
//}

//public enum IconID : uint
//{
//    AdvancedDeathRay = 230, // player->self
//    _Gen_Icon_267 = 267, // player->self
//    _Gen_Icon_23 = 23, // player->self
//}

//public enum SID : uint
//{
//    AccelerationBomb = 2657, // none->player, extra=0x0
//}

//class AdvancedDeathRay(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(70, 4), (uint)IconID.AdvancedDeathRay, ActionID.MakeSpell(AID._Weaponskill_AdvancedDeathRay1));
//class AethericExplosion(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Weaponskill_AethericExplosion));

//class Explosion1 : Components.GroupedAOEs
//{
//    public Explosion1(BossModule module) : base(module, [AID._Weaponskill_Explosion, AID._Weaponskill_Explosion1, AID._Weaponskill_Explosion3], new AOEShapeRect(60, 11))
//    {
//        Color = ArenaColor.Danger;
//    }
//}
//class Explosion2(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_Explosion2, AID._Weaponskill_Explosion4, AID._Weaponskill_Explosion5], new AOEShapeRect(60, 11))
//{
//    private Explosion3? ex3;
//    public override void Update()
//    {
//        ex3 ??= Module.FindComponent<Explosion3>();
//    }

//    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => base.ActiveAOEs(slot, actor).Select(a => a with { Color = ex3?.ActiveCasters.Count() > 0 ? ArenaColor.Danger : ArenaColor.AOE });
//}
//class Explosion3(BossModule module) : Components.StandardAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Explosion6), new AOEShapeRect(60, 11));

//class DiabolicGate(BossModule module) : Components.GenericAOEs(module)
//{
//    private readonly List<(WPos Source, Angle Rotation, DateTime Activation)> Charges = [];
//    private static readonly float[] Delays = [3.94f, 3.05f, 4.26f];

//    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Charges.Take(2).Select((r, i) => new AOEInstance(new AOEShapeRect(100, 12), r.Source, r.Rotation, r.Activation, Color: i == 0 ? ArenaColor.Danger : ArenaColor.AOE)).Reverse();

//    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
//    {
//        if ((AID)spell.Action.ID == AID._Weaponskill_RuinousPseudomen)
//        {
//            var gates = WorldState.Actors.Where(a => (OID)a.OID is OID.Gate1 or OID.Gate2 or OID.Gate3 or OID.Gate4);

//            Actor? nextGate(Actor caster)
//            {
//                var across = gates.Exclude(caster).First(g => g.Position.InRect(caster.Position, caster.Rotation.ToDirection() * 1000, 12));
//                return gates.Exclude(across).First(g => g.OID == across.OID);
//            }

//            Charges.Add((caster.Position, caster.Rotation, Module.CastFinishAt(spell, 1.5f)));
//            var src = caster;
//            for (var i = 0; i < 3; i++)
//            {
//                var next = nextGate(src);
//                if (next == null)
//                {
//                    ReportError($"Missing gate for {src}");
//                    return;
//                }
//                src = next;
//                Charges.Add((next.Position, next.Rotation, Charges[^1].Activation.AddSeconds(Delays[i])));
//            }
//        }
//    }

//    public override void AddHints(int slot, Actor actor, TextHints hints)
//    {
//        foreach (var aoe in ActiveAOEs(slot, actor).Skip(1))
//            if (aoe.Shape.Check(actor.Position, aoe.Origin, aoe.Rotation))
//                hints.Add("GTFO from aoe!");
//    }

//    public override void OnEventCast(Actor caster, ActorCastEvent spell)
//    {
//        if ((AID)spell.Action.ID is AID._Weaponskill_RuinousPseudomen1 or AID._Weaponskill_RuinousPseudomen3 or AID._Weaponskill_RuinousPseudomen5)
//            Charges.RemoveAt(0);
//    }
//}

//class PseudomenBounds(BossModule module) : Components.GenericAOEs(module)
//{
//    // TODO this is wrong
//    private const float SmallArenaRadius = 17f;

//    private DateTime Activation;

//    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Activation == default ? [] : [new AOEInstance(new AOEShapeDonut(SmallArenaRadius, 60), Arena.Center, default, Activation)];

//    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
//    {
//        if ((AID)spell.Action.ID == AID._Weaponskill_DiabolicGate1)
//            Activation = Module.CastFinishAt(spell);
//    }

//    public override void OnEventCast(Actor caster, ActorCastEvent spell)
//    {
//        if ((AID)spell.Action.ID == AID.DiabolicGateDeathwall)
//        {
//            Activation = default;
//            Arena.Bounds = new ArenaBoundsCircle(SmallArenaRadius);
//        }

//        if ((AID)spell.Action.ID == AID._Weaponskill_RuinousPseudomen5)
//            Arena.Bounds = new ArenaBoundsCircle(29.5f);
//    }
//}

//class UltimatePseudoterror(BossModule module) : Components.StandardAOEs(module, ActionID.MakeSpell(AID._Weaponskill_UltimatePseudoterror), new AOEShapeDonut(15, 70));
//class AssaultCannon(BossModule module) : Components.StandardAOEs(module, ActionID.MakeSpell(AID._Weaponskill_AssaultCannon), new AOEShapeRect(100, 3));
//class AdvancedDeath(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_AdvancedDeathIV1), 10);
//class LightPseudopillar(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_LightPseudopillar1), 10);
//class DeadlyDealing(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID._Weaponskill_DeadlyDealing1), 30, stopAtWall: true);
//class VoidSystemsOverload(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Weaponskill_VoidSystemsOverload));
//class PillarOfShamashCone(BossModule module) : Components.GenericAOEs(module)
//{
//    private readonly List<Actor> Short = [];
//    private readonly List<Actor> Mid = [];
//    private readonly List<Actor> Long = [];

//    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
//    {
//        foreach (var s in Short)
//            yield return Cone(s, Mid.Count > 0 ? ArenaColor.Danger : ArenaColor.AOE);

//        foreach (var m in Mid)
//            yield return Cone(m, Short.Count == 0 ? ArenaColor.Danger : ArenaColor.AOE);

//        if (Short.Count == 0)
//            foreach (var l in Long)
//                yield return Cone(l, Mid.Count == 0 ? ArenaColor.Danger : ArenaColor.AOE);
//    }

//    private AOEInstance Cone(Actor caster, uint color = 0) => new(new AOEShapeCone(70, 10.Degrees()), caster.Position, caster.Rotation, Module.CastFinishAt(caster.CastInfo), color == 0 ? ArenaColor.AOE : color);

//    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
//    {
//        switch ((AID)spell.Action.ID)
//        {
//            case AID._Weaponskill_PillarOfShamash:
//                Short.Add(caster);
//                break;
//            case AID._Weaponskill_PillarOfShamash1:
//                Mid.Add(caster);
//                break;
//            case AID._Weaponskill_PillarOfShamash2:
//                Long.Add(caster);
//                break;
//        }
//    }

//    public override void OnEventCast(Actor caster, ActorCastEvent spell)
//    {
//        switch ((AID)spell.Action.ID)
//        {
//            case AID._Weaponskill_PillarOfShamash:
//                Short.Remove(caster);
//                break;
//            case AID._Weaponskill_PillarOfShamash1:
//                Mid.Remove(caster);
//                break;
//            case AID._Weaponskill_PillarOfShamash2:
//                Long.Remove(caster);
//                break;
//        }
//    }
//}

//class AccelerationBomb(BossModule module) : Components.StayMove(module)
//{
//    public override void OnStatusGain(Actor actor, ActorStatus status)
//    {
//        if ((SID)status.ID == SID.AccelerationBomb)
//            SetState(Raid.FindSlot(actor.InstanceID), new(Requirement.Stay, status.ExpireAt));
//    }

//    public override void OnStatusLose(Actor actor, ActorStatus status)
//    {
//        if ((SID)status.ID == SID.AccelerationBomb)
//            ClearState(Raid.FindSlot(actor.InstanceID));
//    }
//}

//class TheDiabloArmamentStates : StateMachineBuilder
//{
//    public TheDiabloArmamentStates(BossModule module) : base(module)
//    {
//        // TODO add pillar of shamash line stack
//        TrivialPhase()
//            .ActivateOnEnter<AdvancedDeathRay>()
//            .ActivateOnEnter<AethericExplosion>()
//            .ActivateOnEnter<Explosion1>()
//            .ActivateOnEnter<Explosion2>()
//            .ActivateOnEnter<Explosion3>()
//            .ActivateOnEnter<PseudomenBounds>()
//            .ActivateOnEnter<DiabolicGate>()
//            .ActivateOnEnter<UltimatePseudoterror>()
//            .ActivateOnEnter<AssaultCannon>()
//            .ActivateOnEnter<AdvancedDeath>()
//            .ActivateOnEnter<LightPseudopillar>()
//            .ActivateOnEnter<DeadlyDealing>()
//            .ActivateOnEnter<VoidSystemsOverload>()
//            .ActivateOnEnter<PillarOfShamashCone>()
//            .ActivateOnEnter<AccelerationBomb>()
//            ;
//    }
//}

//[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 778, NameID = 10007)]
//public class TheDiabloArmament(WorldState ws, Actor primary) : BossModule(ws, primary, new(-720, -760), new ArenaBoundsCircle(29.5f))
//{
//    public override bool DrawAllPlayers => true;
//}

