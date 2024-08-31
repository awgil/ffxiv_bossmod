namespace BossMod.Heavensward.Dungeon.D05GreatGubalLibrary.D053EverlivingBibliotaph;

public enum OID : uint
{
    Boss = 0xE84, // R2.850, x?
    TheEverlivingBibliotaph = 0xEA4, // R0.500, x?
    Voidsphere = 0xEC5, // R1.000, x?
    Bibliophile = 0xE87, // R0.450, x0 (spawn during fight)
    Bibliomancer = 0xE86, // R1.690, x0 (spawn during fight)
    Biblioklept = 0xE85, // R1.500, x0 (spawn during fight)
    AbyssalLance = 0xEA5, // R1.000, x0 (spawn during fight)

    SummonPad = 0x1E99F3, // x6 / x3 / x2
    SummonPadLights = 0x1E991D, // x1 / x2 / x3
    Helper = 0x233C, // x3
}
public enum AID : uint
{
    Attack = 872, // ED4/ECE/ED1/ED0/E84->player, no cast, single-target
    Thrub = 3527, // E84->player, no cast, single-target
    VoidSpark = 3526, // E84->player, no cast, single-target
    VoidSpark2 = 3780, // EC5->self, 4.0s cast, range 7+R circle
    VoidCall = 3524, // E84->self, 18.0s cast, single-target
    DeepDarkness = 3528, // E84->self, 3.0s cast, range -25 donut
    MagicBurst = 3529, // E84->self, 4.0s cast, range 15 circle

    VoidBlizzardIII = 3535, // Bibliomancer->location, 3.0s cast, range 5 circle
    AbyssalSwing = 3532, // Biblioklept->self, no cast, range 6+R ?-degree cone
    AbyssalCharge = 3530, // Biblioklept->self, 1.0s cast, single-target
    AbyssalCharge2 = 3531, // AbyssalLance->self, 3.0s cast, range 40+R width 4 rect

}

class VoidSpark(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.VoidSpark));
class VoidSpark2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.VoidSpark2), new AOEShapeCircle(7f + 1f));
//class VoidCall(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.VoidCall));
class DeepDarkness(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.DeepDarkness), new AOEShapeDonut(10.5f + 1f, 25));
class MagicBurst(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MagicBurst), new AOEShapeCircle(15));
class VoidBlizzardIII(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.VoidBlizzardIII), 5);
class AbyssalSwing(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.AbyssalSwing), new AOEShapeCone(6f + 1.5f, 45.Degrees()))
{
    private readonly List<Actor> _biblioklepts = [];
    private IEnumerable<(Actor origin, Actor target, Angle angle)> OriginsAndTargets()
    {
        foreach (var b in _biblioklepts)
        {
            if (b.IsDead || !ActiveForUntargetable && !b.IsTargetable || !ActiveWhileCasting && b.CastInfo != null)
                continue;

            var target = WorldState.Actors.Find(b.TargetID);
            if (target != null)
            {
                yield return (OriginAtTarget ? target : b, target, Angle.FromDirection(target.Position - b.Position));
            }
        }
    }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var (origin, target, angle) in OriginsAndTargets())
        {
            if (actor != target)
            {
                hints.AddForbiddenZone(Shape, origin.Position, angle, NextExpected);
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var e in OriginsAndTargets())
        {
            Shape.Outline(Arena, e.origin.Position, e.angle);
        }
    }
    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.Biblioklept)
        {
            _biblioklepts.Add(actor);
        }
    }
    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID is OID.Biblioklept)
        {
            _biblioklepts.Remove(actor);
        }
    }
};

class AbyssalCharge(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AbyssalCharge), new AOEShapeRect(40f + 1f, 2));
class AbyssalCharge2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AbyssalCharge2), new AOEShapeRect(40f + 1f, 2));

class VoidCall(BossModule module) : BossComponent(module)
{
    public List<Actor> _pads = [];
    public List<Components.GenericTowers.Tower> _towers = [];
    public float Radius = 3f;
    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.SummonPad)
        {
            _pads.Add(actor);
            _towers.Add(new(actor.Position, Radius));
        }
    }
    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID is OID.SummonPad)
        {
            _pads.Remove(actor);
            _towers.RemoveAll(t => t.Position.AlmostEqual(actor.Position, 1));
        }
    }

    public static void DrawTower(MiniArena arena, WPos pos, float radius, bool safe)
    {
        if (arena.Config.ShowOutlinesAndShadows)
            arena.AddCircle(pos, radius, 0xFF000000, 3);
        arena.AddCircle(pos, radius, safe ? ArenaColor.Safe : ArenaColor.Danger, 2);
    }
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var t in _towers)
            DrawTower(Arena, t.Position, t.Radius, !t.ForbiddenSoakers[pcSlot]);
    }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_towers.Count == 0)
            return;
        var minSoakers = _pads.Count switch
        {
            6 => 1,
            3 => 2,
            2 => 3,
            _ => 0
        };
        var bestTower = _towers[0];
        var maxEval = -1;
        foreach (var t in _towers)
        {
            var curEval = Raid.WithSlot().InRadius(t.Position, 2).Count();
            if (curEval > maxEval)
            {
                maxEval = curEval;
                bestTower = t;
            }
        }
        var curSoakers = Raid.WithSlot().InRadius(bestTower.Position, 2).Count();
        if (minSoakers == 3 && curSoakers == 2 ||
            minSoakers == 2 && curSoakers == 1 ||
            minSoakers == 1 && curSoakers == 0)
        {
            hints.AddForbiddenZone(new AOEShapeDonut(2, 50), bestTower.Position);
        }
        else if (curSoakers < minSoakers)
        {
            hints.AddForbiddenZone(new AOEShapeDonut(2, 50), bestTower.Position);
        }
        else
            _towers.Remove(bestTower);
    }
}
class MultiAddsModule(BossModule module) : Components.AddsMulti(module, [(uint)OID.Voidsphere, (uint)OID.Bibliophile, (uint)OID.Bibliomancer, (uint)OID.Biblioklept, (uint)OID.AbyssalLance])
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.Biblioklept => 4,
                OID.Bibliomancer => 3,
                OID.Bibliophile => 2,
                OID.Boss => 1,
                _ => 0
            };
    }
};

class D053EverlivingBibliotaphStates : StateMachineBuilder
{
    public D053EverlivingBibliotaphStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<VoidSpark>()
            .ActivateOnEnter<VoidSpark2>()
            .ActivateOnEnter<DeepDarkness>()
            .ActivateOnEnter<MagicBurst>()
            .ActivateOnEnter<VoidBlizzardIII>()
            .ActivateOnEnter<AbyssalSwing>()
            .ActivateOnEnter<AbyssalCharge>()
            .ActivateOnEnter<AbyssalCharge2>()
            .ActivateOnEnter<VoidCall>()
            .ActivateOnEnter<MultiAddsModule>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 31, NameID = 3930)]
public class D053EverlivingBibliotaph(WorldState ws, Actor primary) : BossModule(ws, primary, new(377.4f, -59.7f), new ArenaBoundsCircle(25));
