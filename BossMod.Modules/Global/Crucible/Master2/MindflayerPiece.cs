#pragma warning disable CA1707 // Identifiers should not contain underscores

namespace BossMod.Global.Crucible.MindflayerPiece;

public enum OID : uint
{
    Helper = 0x233C, // R0.500, x5 (spawn during fight), Helper type
    Boss = 0x4CDD, // R2.340, x1
    _Gen_MyconidPiece = 0x4CDE, // R0.600, x0 (spawn during fight)
    _Gen_ArcaneSphere = 0x4CE1, // R1.000-1.860, x0 (spawn during fight)
    _Gen_Shroombed = 0x4CDF, // R6.000, x0 (spawn during fight)
    _Gen_Spore = 0x4CE0, // R3.000, x0 (spawn during fight)

    VoidWater = 0x1E9998, // R7.500
}

public enum AID : uint
{
    _Spell_Thunder = 48623, // Boss->player, no cast, single-target
    _Spell_VoidWaterIII = 49199, // Boss->self, 5.0s cast, single-target
    _Spell_VoidWaterIII1 = 49200, // Helper->players, no cast, range 8 circle
    _AutoAttack_ = 49682, // 4CDE->player, no cast, single-target
    _Spell_VoidThunderIII = 49205, // Boss->self, 5.0s cast, single-target
    _Spell_VoidThunderIII1 = 49206, // Helper->player, no cast, range 6 circle
    _Spell_ = 50939, // Helper->self, 4.0s cast, range 50 width 10 cross
    _Spell_VoidThunderIII2 = 49207, // Helper->self, 4.0s cast, range 50 width 10 cross
    _Weaponskill_SporeSpill = 49196, // Helper->self, 1.0s cast, range 6 circle
    _Weaponskill_ArcaneUtterance = 49201, // Boss->self, 5.0s cast, single-target
    _Weaponskill_ = 49197, // 4CDF->self, no cast, range 6 circle
    _Weaponskill_ArcaneEnhancement = 49202, // Boss->self, 5.0s cast, single-target
    _Weaponskill_DarkCurrent1 = 49203, // 4CE1->self, 2.0s cast, range 100 width 4 rect
    _Weaponskill_DarkCurrent = 49204, // 4CE1->self, 2.0s cast, range 100 width 10 rect
    _Spell_VoidParalyzeIII = 49208, // Boss->self, 7.0s cast, range 60 circle
    _Weaponskill_BigBurst = 49198, // 4CE0->self, no cast, range 60 circle
}

public enum SID : uint
{
    _Gen_WaterResistanceDown = 5021, // Helper->player, extra=0x1/0x2
    _Gen_LightningResistanceDownII = 4456, // none->player, extra=0x0
    _Gen_SustainedDamage = 3795, // none->4CE1, extra=0x1
    _Gen_Paralysis = 5382, // Boss->player, extra=0x0
    _Gen_Poison = 5140, // Helper/4CDF/4CE0->player, extra=0x0
    _Gen_Bleeding = 3077, // none->player, extra=0x0
    _Gen_Bleeding1 = 3078, // none->player, extra=0x0
}

public enum IconID : uint
{
    _Gen_Icon_m0311trg_t0v = 135, // player/49E4/4A15->self
    _Gen_Icon_tank_lockonae_6m_5s_01t = 344, // player->self
}

public enum TetherID : uint
{
    _Gen_Tether_chn_magic_supply_01y2 = 426, // 4CE1->Boss
}

class VoidWaterIII(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID._Gen_Icon_m0311trg_t0v, AID._Spell_VoidWaterIII1, 8, 6);
class VoidPuddle(BossModule module) : Components.Voidzone(module, 7.5f, OID.VoidWater)
{
    public override void AddHints(int slot, Actor actor, TextHints hints) { }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!Module.Enemies(OID._Gen_MyconidPiece).Any(p => !p.IsDead))
            foreach (var s in Sources)
                hints.AddForbiddenZone(ShapeDistance.Circle(s.Position, 7.5f), DateTime.MaxValue);
    }
}
class VoidThunderIIIBuster(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(6), (uint)IconID._Gen_Icon_tank_lockonae_6m_5s_01t, AID._Spell_VoidThunderIII1, 6, true, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (actor.FindStatus(SID._Gen_WaterResistanceDown) != null)
            hints.Add("GTFO from puddle!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var b in ActiveBaitsOn(actor))
            foreach (var p in Module.Enemies(OID.VoidWater))
                hints.AddForbiddenZone(ShapeDistance.Circle(p.Position, 7.5f), b.Activation);
    }
}

class VoidThunderIIICross(BossModule module) : Components.StandardAOEs(module, AID._Spell_VoidThunderIII2, new AOEShapeCross(50, 5));

class MyconidPiece(BossModule module) : Components.Adds(module, (uint)OID._Gen_MyconidPiece, 1)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var puddles = Module.Enemies(OID.VoidWater);

        foreach (var target in hints.PotentialTargets.Where(t => t.Actor.OID == (uint)OID._Gen_MyconidPiece))
        {
            var midpoint = puddles.Aggregate(default(WDir), (a, p) => a + p.Position.ToWDir()) / puddles.Count;
            target.DesiredPosition = midpoint.ToWPos();

            // standing in puddle -> kill
            if (puddles.Any(p => target.Actor.Position.InCircle(p.Position, 7.5f)))
                target.Priority = 1;

            // not in puddle, targeting us -> pull to puddle and don't kill it early
            else if (target.Actor.TargetID == actor.InstanceID)
            {
                // TODO: this is a hack to make normalmove pull the mushroom to the right position
                target.ShouldBeTargeted = true;
                target.Priority = AIHints.Enemy.PriorityForbidden;
            }

            // not in puddle, not targeting us -> aggro it
            else
            {
                target.ShouldBeTargeted = true;
                target.ShouldBeTanked = true;
            }
        }
    }
}

class Shroombed(BossModule module) : Components.Voidzone(module, 6, OID._Gen_Shroombed);

class SporeSpillPre(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_SporeSpill)
{
    readonly List<Actor> _alive = [];
    readonly List<AOEInstance> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _predicted.RemoveAll(p => p.Origin.AlmostEqual(spell.LocXZ, 0.5f));
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID._Gen_MyconidPiece)
            _alive.Add(actor);
    }

    public override void Update()
    {
        for (var i = _alive.Count - 1; i >= 0; i--)
            if (_alive[i].IsDead)
            {
                _predicted.Add(new(new AOEShapeCircle(6), _alive[i].Position, default, WorldState.FutureTime(3)));
                _alive.RemoveAt(i);
            }
    }
}
class SporeSpill(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_SporeSpill, 6);

class DarkCurrentSmall(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_DarkCurrent1, new AOEShapeRect(100, 2));
class DarkCurrentLarge(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_DarkCurrent, new AOEShapeRect(100, 5));

class VoidParalyzeIII(BossModule module) : Components.RaidwideCast(module, AID._Spell_VoidParalyzeIII, "Raidwide + paralysis");

class MindflayerPieceStates : StateMachineBuilder
{
    public MindflayerPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<VoidWaterIII>()
            .ActivateOnEnter<VoidPuddle>()
            .ActivateOnEnter<VoidThunderIIIBuster>()
            .ActivateOnEnter<VoidThunderIIICross>()
            .ActivateOnEnter<MyconidPiece>()
            .ActivateOnEnter<Shroombed>()
            .ActivateOnEnter<SporeSpill>()
            .ActivateOnEnter<SporeSpillPre>()
            .ActivateOnEnter<DarkCurrentSmall>()
            .ActivateOnEnter<DarkCurrentLarge>()
            .ActivateOnEnter<VoidParalyzeIII>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1092, NameID = 14633)]
public class MindflayerPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120, 0), new ArenaBoundsSquare(20));

