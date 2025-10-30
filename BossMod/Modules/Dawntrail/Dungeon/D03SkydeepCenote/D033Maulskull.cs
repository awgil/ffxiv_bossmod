namespace BossMod.Dawntrail.Dungeon.D03SkydeepCenote.D033Maulskull;

public enum OID : uint
{
    Boss = 0x41C7, // R19.980, x1
    Helper = 0x233C, // R0.500, x15, Helper type
}

public enum AID : uint
{
    AutoAttack = 36678, // Boss->player, no cast, single-target
    WroughtFire = 39121, // Boss->self, 4.0+1.0s cast, single-target, visual (aoe tankbuster)
    WroughtFireAOE = 39122, // Helper->player, 5.0s cast, range 6 circle tankbuster
    Ashlayer = 36711, // Boss->self, 3.0+2.0s cast, single-target, visual (raidwide)
    AshlayerAOE = 36712, // Helper->self, no cast, range 60 circle, raidwide

    StonecarverRL = 36668, // Boss->self, 8.0s cast, single-target, visual (cleave right->left)
    StonecarverLR = 36669, // Boss->self, 8.0s cast, single-target, visual (cleave left->right)
    StonecarverAOE1 = 36670, // Helper->self, 9.0s cast, range 40 width 20 rect
    StonecarverAOE2 = 36671, // Helper->self, 11.5s cast, range 40 width 20 rect
    StonecarverSecondR = 36672, // Boss->self, no cast, single-target, visual (second cleave right)
    StonecarverSecondL = 36673, // Boss->self, no cast, single-target, visual (second cleave left)
    StonecarverEnd = 36674, // Boss->self, no cast, single-target, visual (finish mechanic)

    Skullcrush = 36675, // Boss->self, 5.0+2.0s cast, single-target, visual (knockback + spread)
    SkullcrushAOE = 36676, // Helper->self, 7.0s cast, range 10 circle
    SkullcrushImpact = 36677, // Helper->self, 7.0s cast, range 60 circle knockback 18
    SkullcrushEnd = 38664, // Boss->self, no cast, single-target, visual (finish mechanic)
    Charcore = 36708, // Boss->self, no cast, single-target, visual (stack/spread resolve)
    DestructiveHeat = 36709, // Helper->players, 7.0s cast, range 6 circle spread
    BuildingHeat = 36710, // Helper->players, 7.0s cast, range 6 circle stack

    Maulwork1 = 36679, // Boss->self, 5.0s cast, single-target, visual (puddles + center/sides ?)
    Maulwork2 = 36680, // Boss->self, 5.0s cast, single-target, visual (puddles + center/sides ? - center variant)
    Maulwork3 = 36681, // Boss->self, 5.0s cast, single-target, visual (puddles + center/sides ? - sides variant)
    Maulwork4 = 36682, // Boss->self, 5.0s cast, single-target, visual (puddles + center/sides ?)
    Landing = 36683, // Helper->location, 3.0s cast, range 8 circle
    ShatterCenter = 36684, // Helper->self, 3.0s cast, range 40 width 20 rect
    ShatterSideR = 36685, // Helper->self, 3.0s cast, range 45 width 22 rect
    ShatterSideL = 36686, // Helper->self, 3.0s cast, range 45 width 22 rect

    DeepThunder = 36687, // Boss->self, 6.0s cast, single-target, visual (multi-hit tower)
    DeepThunderTowerShort = 36688, // Helper->self, 9.0s cast, range 6 circle, visual (tower vfx for 3 hits)
    DeepThunderTowerLong = 36689, // Helper->self, 11.0s cast, range 6 circle, visual (tower vfx for 5 hits)
    DeepThunderAOE = 36690, // Helper->self, no cast, range 6 circle stack
    DeepThunderRest = 36691, // Boss->self, no cast, single-target, visual (subsequent hits)
    DeepThunderEnd = 36692, // Boss->self, no cast, single-target, visual (finish mechanic)

    RingingBlowsRL = 36694, // Boss->self, 7.0+2.0s cast, single-target, visual (knockback + cleaves right->left)
    RingingBlowsLR = 36695, // Boss->self, 7.0+2.0s cast, single-target, visual (knockback + cleaves left->right)
    RingingBlowsStonecarverAOE1 = 36696, // Helper->self, 11.1s cast, range 40 width 20 rect
    RingingBlowsStonecarverAOE2 = 36697, // Helper->self, 13.6s cast, range 40 width 20 rect
    RingingBlowsStonecarverVisual1 = 36699, // Boss->self, no cast, single-target, visual (first/right? cleave)
    RingingBlowsStonecarverVisual2 = 36700, // Boss->self, no cast, single-target, visual (second/left? cleave)
    RingingBlowsSkullcrushAOE = 36666, // Helper->self, 9.0s cast, range 10 circle
    RingingBlowsSkullcrushImpact = 36667, // Helper->self, 9.0s cast, range 60 circle knockback 18

    ColossalImpact1 = 36704, // Boss->self, 6.0+2.0s cast, single-target, visual (knockback + spread/stack ?)
    ColossalImpact2 = 36705, // Boss->self, 6.0+2.0s cast, single-target, visual (knockback + spread/stack ?)
    ColossalImpactAOE = 36706, // Helper->self, 8.0s cast, range 10 circle
    ColossalImpactImpact = 36707, // Helper->self, 8.0s cast, range 60 circle knockback 20
}

public enum IconID : uint
{
    DestructiveHeat = 375, // player
    WroughtFire = 344, // player
    BuildingHeat = 317, // player
}

class Stonecarver(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [];

    private static readonly AOEShapeRect _shape = new(40, 10);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs.Take(1);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.StonecarverAOE1 or AID.StonecarverAOE2 or AID.RingingBlowsStonecarverAOE1 or AID.RingingBlowsStonecarverAOE2)
        {
            AOEs.Add(new(_shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
            AOEs.SortBy(aoe => aoe.Activation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.StonecarverAOE1 or AID.StonecarverAOE2 or AID.RingingBlowsStonecarverAOE1 or AID.RingingBlowsStonecarverAOE2 && AOEs.Count > 0)
            AOEs.RemoveAt(0);
    }
}

class ImpactAOE(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCircle _shape = new(10);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.SkullcrushAOE or AID.RingingBlowsSkullcrushAOE or AID.ColossalImpactAOE)
            _aoes.Add(new(_shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.SkullcrushAOE or AID.RingingBlowsSkullcrushAOE or AID.ColossalImpactAOE)
            _aoes.Clear();
    }
}

class Impact(BossModule module) : Components.Knockback(module)
{
    public readonly List<Source> AOEs = [];
    private readonly Stonecarver? _cleaves = module.FindComponent<Stonecarver>();

    public override IEnumerable<Source> Sources(int slot, Actor actor) => AOEs;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (AOEs.Count == 0)
            return;

        var origin = AOEs[0].Origin;
        if (Math.Abs(origin.X - Module.Center.X) > 5)
        {
            // knockback from corner -> aim to opposite corner
            hints.AddForbiddenZone(SafeSpotInDirection(origin, (Module.Center - origin).Normalized()), AOEs[0].Activation);
        }
        else if (_cleaves?.AOEs.Count > 0)
        {
            // knockback to the corner that won't be cleaved, but not too far to easily move to other side
            hints.AddForbiddenZone(SafeSpotInDirection(origin, (_cleaves.AOEs[0].Origin.X > Module.Center.X ? -15.Degrees() : 15.Degrees()).ToDirection()), AOEs[0].Activation);
        }
        else
        {
            // knockback to any of the corners
            var c1 = SafeSpotInDirection(origin, (-35).Degrees().ToDirection());
            var c2 = SafeSpotInDirection(origin, 35.Degrees().ToDirection());
            hints.AddForbiddenZone(p => c1(p) && c2(p), AOEs[0].Activation);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var distance = (AID)spell.Action.ID switch
        {
            AID.SkullcrushImpact or AID.RingingBlowsSkullcrushImpact => 18,
            AID.ColossalImpactImpact => 20,
            _ => 0
        };
        if (distance > 0)
            AOEs.Add(new(caster.Position, distance, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.SkullcrushImpact or AID.RingingBlowsSkullcrushImpact or AID.ColossalImpactImpact)
            AOEs.Clear();
    }

    private Func<WPos, bool> SafeSpotInDirection(WPos origin, WDir dir) => ShapeContains.InvertedCircle(origin + dir * 11, 1);
}

class DestructiveBuildingHeat(BossModule module) : Components.CastStackSpread(module, AID.BuildingHeat, AID.DestructiveHeat, 6, 6, 4, alwaysShowSpreads: true)
{
    private readonly Impact? impact = module.FindComponent<Impact>();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // only start spreading after knockback is done
        if (impact == null || impact.AOEs.Count == 0)
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class Landing(BossModule module) : Components.StandardAOEs(module, AID.Landing, 8);
class ShatterCenter(BossModule module) : Components.StandardAOEs(module, AID.ShatterCenter, new AOEShapeRect(40, 10));
class ShatterSideR(BossModule module) : Components.StandardAOEs(module, AID.ShatterSideR, new AOEShapeRect(45, 11));
class ShatterSideL(BossModule module) : Components.StandardAOEs(module, AID.ShatterSideL, new AOEShapeRect(45, 11));

class DeepThunder(BossModule module) : Components.GenericTowers(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var t in Towers)
            hints.AddForbiddenZone(ShapeContains.InvertedCircle(t.Position, t.Radius));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.DeepThunderTowerShort or AID.DeepThunderTowerLong)
            Towers.Add(new(caster.Position, 6, 4, 4));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.DeepThunderTowerShort or AID.DeepThunderTowerLong)
            Towers.Clear();
    }
}

class WroughtFire(BossModule module) : Components.BaitAwayCast(module, AID.WroughtFireAOE, new AOEShapeCircle(6), true);
class Ashlayer(BossModule module) : Components.RaidwideCast(module, AID.Ashlayer);

class D033MaulskullStates : StateMachineBuilder
{
    public D033MaulskullStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Stonecarver>()
            .ActivateOnEnter<ImpactAOE>()
            .ActivateOnEnter<Impact>()
            .ActivateOnEnter<DestructiveBuildingHeat>()
            .ActivateOnEnter<Landing>()
            .ActivateOnEnter<ShatterCenter>()
            .ActivateOnEnter<ShatterSideR>()
            .ActivateOnEnter<ShatterSideL>()
            .ActivateOnEnter<DeepThunder>()
            .ActivateOnEnter<WroughtFire>()
            .ActivateOnEnter<Ashlayer>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 829, NameID = 12728)]
public class D033Maulskull(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, -430), new ArenaBoundsSquare(20));
