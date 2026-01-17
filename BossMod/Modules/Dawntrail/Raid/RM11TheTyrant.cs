namespace BossMod.Dawntrail.Raid.RM11TheTyrant;

public enum OID : uint
{
    Boss = 0x4AE8, // R5.000-10.000, x?
    TheTyrant2 = 0x4AEE, // R3.000-30.000, x?
    Comet = 0x4AE9, // R2.160, x?
    Helper = 0x233C, // R0.500, x?, Helper type
    Maelstrom = 0x4AEB, // R1.000, x?
}
public enum AID : uint
{
    AutoAttack = 46005, // 4AE8->player, no cast, single-target
    CrownOfArcadia = 46006, // 4AE8->self, 5.0s cast, range 60 circle
    DrawSteel = 46008, // 4AE8->self, 2.0+4.0s cast, single-target
    DrawSteel1 = 46007, // 4AE8->self, 2.0+4.0s cast, single-target
    DrawSteel2 = 46009, // 4AE8->self, 2.0+4.0s cast, single-target

    SmashdownScytheVisual = 46012, // 4AE8->self, 2.0+1.0s cast, single-target
    SmashdownScythe = 46013, // 233C->self, 3.0s cast, range 5-60 donut
    SmashdownAxeVisual = 46010, // 4AE8->self, 2.0+1.0s cast, single-target
    SmashdownAxe = 46011, // 233C->self, 3.0s cast, range 8 circle
    SmashdownSwordVisual = 46014, // 4AE8->self, 2.0+1.0s cast, single-target
    SmashdownSword = 46015, // 233C->self, 3.0s cast, range 40 width 10 cross

    VoidStardust = 46024, // 4AE8->self, 5.0s cast, single-target
    Comet = 46025, // 233C->player, 5.0s cast, range 4 circle
    Comet1 = 46027, // 233C->players, no cast, range 4 circle
    Cometite = 46026, // 233C->location, 2.5s cast, range 4 circle

    TrophyWeapons = 46028, // 4AE8->self, 3.0s cast, single-target
    AssaultEvolvedCast = 46029, // 4AE8->self, 5.0s cast, single-target
    AssaultEvolvedSwordVisual = 46402, // 4AE8->location, no cast, single-target
    AssaultEvolvedSword = 46032, // 233C->self, 2.0s cast, range 40 width 10 cross
    AssaultEvolvedScytheVisual = 46401, // 4AE8->location, no cast, single-target
    AssaultEvolvedScythe = 46031, // 233C->self, 2.0s cast, range 5-60 donut
    AssaultEvolvedAxeVisual = 46400, // 4AE8->location, no cast, single-target
    AssaultEvolvedAxe = 46030, // 233C->self, 2.0s cast, range 8 circle

    DanceOfDominationTrophy = 47034, // 4AE8->self, 2.0+4.0s cast, single-target
    DanceOfDomination = 46033, // 4AE8->self, 4.5+0.7s cast, single-target
    DanceOfDomination1 = 46034, // 233C->self, 5.0s cast, range 60 circle
    DanceOfDomination2 = 46036, // 233C->self, no cast, range 60 circle
    DanceOfDomination3 = 47081, // 233C->self, no cast, range 60 circle
    Explosion1 = 46035, // 233C->self, 6.0s cast, range 60 width 10 rect
    Explosion2 = 47033, // 233C->self, 6.5s cast, range 60 width 10 rect
    Explosion3 = 46077, // 233C->self, 3.0s cast, range 60 width 6 rect

    RawSteelTrophy = 46037, // 4AE8->self, 2.0+4.0s cast, single-target
    RawSteel = 46016, // 4AE8->self, 5.0+1.0s cast, single-target
    RawSteel1 = 46017, // 4AE8->players, no cast, range 6 circle
    Impact = 46018, // 233C->player, 6.5s cast, range 6 circle

    Charybdistopia = 46039, // 4AE8->self, 5.0s cast, range 60 circle
    UltimateTrophyWeapons = 47083, // 4AE8->self, 3.0s cast, single-target
    AssaultApex = 47084, // 4AE8->self, 5.0s cast, single-target

    Charybdis = 46040, // 233C->player, no cast, single-target
    PowerfulGust = 46041, // 233C->self, 6.0s cast, range 60 45-degree cone
    ImmortalReign = 46042, // 4AE8->self, 3.0+1.0s cast, single-target
    OneAndOnly = 46043, // 4AE8->self, 6.0+2.0s cast, single-target
    OneAndOnly1 = 46044, // 233C->self, 9.0s cast, range 60 circle

    Meteorain = 46045, // 4AE8->self, 5.0s cast, single-target
    CosmicKiss = 46046, // 4AE9->self, 8.0s cast, range 4 circle
    ForegoneFatality = 46048, // 4AEE->player, no cast, single-target
    MassiveMeteor = 46051, // 233C->players, 6.0s cast, range 6 circle
    DoubleTyrannhilation = 46052, // 4AE8->self, no cast, single-target
    DoubleTyrannhilation1 = 46053, // 4AE8->self, 8.0+1.0s cast, range 30 circle
    HiddenTyrannhilation = 46215,

    Flatliner = 46056, // 4AE8->self, 4.0+2.0s cast, single-target
    FlatlinerKnockUp = 47759, // 233C->self, 6.0s cast, range 60 circle
    MajesticMeteor = 46057, // 4AE8->self, 4.0s cast, single-target
    MajesticMeteor1 = 46058, // 233C->location, 4.0s cast, range 6 circle
    MajesticMeteorain = 46059, // 233C->self, 6.0s cast, range 60 width 10 rect

    MammothMeteor = 46060, // 233C->location, 6.5s cast, range 60 circle
    FireAndFuryVisual = 46071, // 4AE8->self, 5.0+1.0s cast, single-target
    FireAndFuryCone1 = 46073, // 233C->self, 6.0s cast, range 60 90-degree cone
    FireAndFuryCone2 = 46072, // 233C->self, 6.0s cast, range 60 90-degree cone
    ExplosionKnockUp = 46061, // 233C->self, 10.0s cast, range 4 circle
    ArcadionAvalanche = 46069, // 4AE8->self, 8.0+9.5s cast, single-target
    ArcadionAvalancheToss = 46070, // 233C->self, 17.5s cast, range 40 width 40 rect

    HeartbreakKick = 46079, // 4AE8->self, 5.0+1.0s cast, single-target
    HeartbreakKick1 = 46080, // 233C->self, no cast, range 4 circle
    GreatWallOfFire = 46074, // 4AE8->self, 5.0s cast, single-target
    GreatWallOfFire1 = 46075, // 4AE8->self, no cast, range 60 width 6 rect
    GreatWallOfFire2 = 46076, // 233C->self, no cast, single-target
}
public enum IconID : uint
{
    VoidStardustSpread = 630, // player->self
    RawSteelSpread = 311, // player->self
    RawSteelSharedTankbuster = 600, // player->self
    MassiveMeteorStack = 318, // player->self
    WallOfFireTankbuster = 598, // player->self
}
public enum TetherID : uint
{
    TankInterceptTether = 356, // TheTyrant2->Comet/player
}
class CrownOfArcadia(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect rect = new(20, 6, 20);
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
        {
            foreach (var e in _aoes)
            {
                yield return e with { Color = ArenaColor.AOE };
            }
        }
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.CrownOfArcadia)
        {
            _aoes.Add(new(rect, new WPos(74, 100)));
            _aoes.Add(new(rect, new WPos(126, 100)));
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.CrownOfArcadia)
        {
            _aoes.Clear();
        }
    }
}
class Smashdown1(BossModule module) : Components.StandardAOEs(module, AID.SmashdownScythe, new AOEShapeDonut(5, 60));
class Smashdown2(BossModule module) : Components.StandardAOEs(module, AID.SmashdownAxe, 8f);
class Smashdown3(BossModule module) : Components.StandardAOEs(module, AID.SmashdownSword, new AOEShapeCross(40, 5f));
class VoidStardust(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.VoidStardustSpread, AID.VoidStardust, 4f, 4.7f, true)
{
    public int _numCasts;
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.VoidStardust)
            _numCasts = 0;
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Cometite or AID.Comet1)
        {
            _numCasts++;
        }
        if (_numCasts >= 16)
        {
            Spreads.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
            ++NumFinishedSpreads;
        }
    }
}
class Cometite(BossModule module) : Components.StandardAOEs(module, AID.Cometite, 4f);
class AssaultEvolved1(BossModule module) : Components.StandardAOEs(module, AID.AssaultEvolvedSword, new AOEShapeCross(40, 5f));
class AssaultEvolved2(BossModule module) : Components.StandardAOEs(module, AID.AssaultEvolvedScythe, new AOEShapeDonut(5, 60));
class AssaultEvolved3(BossModule module) : Components.StandardAOEs(module, AID.AssaultEvolvedAxe, 8f);
class DanceOfDomination(BossModule module) : Components.RaidwideCast(module, AID.DanceOfDomination);
class Explosion1(BossModule module) : Components.StandardAOEs(module, AID.Explosion1, new AOEShapeRect(60f, 5f));
class Explosion2(BossModule module) : Components.StandardAOEs(module, AID.Explosion1, new AOEShapeRect(60f, 5f));
class Explosion3(BossModule module) : Components.StandardAOEs(module, AID.Explosion1, new AOEShapeRect(60f, 3f));
class RawSteelTankBuster(BossModule module) : Components.IconSharedTankbuster(module, (uint)IconID.RawSteelSharedTankbuster, AID.RawSteel1, 6f);
class RawSteelSpreads(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.RawSteelSpread, AID.Impact, 6, 0, true);
class Charybdistopia(BossModule module) : Components.RaidwideCast(module, AID.Charybdistopia);
class Maelstrom(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle circ = new(5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
        {
            foreach (var e in _aoes)
            {
                yield return e with { Color = ArenaColor.AOE };
            }
        }
    }
    public override void OnActorCreated(Actor actor)
    {
        base.OnActorCreated(actor);
        if ((OID)actor.OID is OID.Maelstrom)
        {
            _aoes.Add(new(circ, actor.Position));
        }
    }
    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID is OID.Maelstrom)
        {
            _aoes.Clear();
        }
    }
}
class PowerfulGust(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCone cone = new(60f, 22.5f.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
        {
            foreach (var e in _aoes.Take(8))
            {
                yield return e with { Color = ArenaColor.AOE };
            }
        }
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.PowerfulGust)
        {
            _aoes.Add(new(cone, caster.CastInfo!.LocXZ, caster.CastInfo!.Rotation));

        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID is AID.PowerfulGust)
        {
            _aoes.RemoveAt(0);
        }
    }
}
class OneAndOnly(BossModule module) : Components.RaidwideCast(module, AID.OneAndOnly);
class CosmicKiss(BossModule module) : Components.CastTowers(module, AID.CosmicKiss, 4f);
class MassiveMeteor(BossModule module) : Components.StackWithIcon(module, (uint)IconID.MassiveMeteorStack, AID.MassiveMeteor, 6, 0);
class ForegoneFatality(BossModule module) : Components.TankbusterTether(module, AID.ForegoneFatality, (uint)TetherID.TankInterceptTether, 6f);
class DoubleTyrannhilation(BossModule module) : Components.CastLineOfSightAOE(module, AID.DoubleTyrannhilation1, 60f, false)
{
    public override IEnumerable<Actor> BlockerActors() => Module.Enemies(OID.Comet).Where(a => !a.Position.InCircle(Module.PrimaryActor.Position, Module.PrimaryActor.HitboxRadius + a.HitboxRadius + 9));

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Comet)
            Refresh();
    }
}
class HiddenTyrannhilation(BossModule module) : Components.CastLineOfSightAOE(module, AID.HiddenTyrannhilation, 60f, false)
{
    public override IEnumerable<Actor> BlockerActors() => Module.Enemies(OID.Comet).Where(a => !a.Position.InCircle(Module.PrimaryActor.Position, Module.PrimaryActor.HitboxRadius + a.HitboxRadius + 9));

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Comet)
            Refresh();
    }
}
class Flatliner(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect rect = new(20, 6, 20);
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
        {
            foreach (var e in _aoes)
            {
                yield return e with { Color = ArenaColor.AOE };
            }
        }
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Flatliner)
        {
            _aoes.Add(new(rect, caster.CastInfo!.LocXZ, caster.CastInfo!.Rotation));
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Flatliner)
        {
            Arena.Bounds = new ArenaBoundsRect(26, 20);
        }
        if ((AID)spell.Action.ID is AID.CrownOfArcadia)
        {
            Arena.Bounds = new ArenaBoundsRect(20, 20);
            _aoes.Clear();
        }
    }
}
class FlatlinerKnockUp(BossModule module) : Components.KnockbackFromCastTarget(module, AID.FlatlinerKnockUp, 15, true);
class MajesticMeteor(BossModule module) : Components.StandardAOEs(module, AID.MajesticMeteor1, 6f);
class MajesticMeteorain(BossModule module) : Components.StandardAOEs(module, AID.MajesticMeteorain, new AOEShapeRect(60f, 5f));
class MammothMeteor(BossModule module) : Components.StandardAOEs(module, AID.MammothMeteor, new AOEShapeCircle(22));
class FireAndFury1(BossModule module) : Components.StandardAOEs(module, AID.FireAndFuryCone1, new AOEShapeCone(60f, 45.Degrees()));
class FireAndFury2(BossModule module) : Components.StandardAOEs(module, AID.FireAndFuryCone2, new AOEShapeCone(60f, 45.Degrees()));
class ExplosionTower(BossModule module) : Components.CastTowers(module, AID.ExplosionKnockUp, 4f, 1, 8);
class ExplosionKnockUp(BossModule module) : Components.KnockbackFromCastTarget(module, AID.ExplosionKnockUp, 25, true)
{
    private const float TowerRadius = 4f;
    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        Source? best = null;
        float bestDistSq = float.MaxValue;

        foreach (var c in Casters)
        {
            var minDist = MinDistance + (MinDistanceBetweenHitboxes ? actor.HitboxRadius + c.HitboxRadius : 0);

            var origin = (c.CastInfo!.TargetID == c.InstanceID) ? c.Position : (WorldState.Actors.Find(c.CastInfo.TargetID)?.Position ?? c.CastInfo.LocXZ);

            var dSq = (actor.Position - origin).LengthSq();

            if (dSq > TowerRadius * TowerRadius)
                continue;

            if (dSq < bestDistSq)
            {
                bestDistSq = dSq;

                var rot = (c.CastInfo.TargetID == c.InstanceID) ? c.CastInfo.Rotation : Angle.FromDirection(origin - c.Position);

                best = new Source(origin, Distance, Module.CastFinishAt(c.CastInfo), Shape, rot, KnockbackKind, minDist);
            }
        }

        if (best != null)
            yield return best.Value;
    }
}
class ArcadionAvalanche(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect rect = new(40f, 20f);
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
        {
            foreach (var e in _aoes)
            {
                yield return e with { Color = ArenaColor.AOE };
            }
        }
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ArcadionAvalanche)
        {
            _aoes.Add(new(rect, caster.CastInfo!.LocXZ, caster.CastInfo!.Rotation));
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.CrownOfArcadia)
        {
            _aoes.Clear();
        }
    }
}
class ArcadionAvalancheToss(BossModule module) : Components.StandardAOEs(module, AID.ArcadionAvalancheToss, new AOEShapeRect(40f, 20f));
class HeartbreakKick(BossModule module) : Components.CastTowers(module, AID.HeartbreakKick, 4, 1, 8, damageType: AIHints.PredictedDamageType.Shared)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Towers.Add(new(DeterminePosition(caster, spell), Radius, MinSoakers, MaxSoakers, activation: Module.CastFinishAt(spell)));
        if ((AID)spell.Action.ID is AID.MajesticMeteor)
        {
            var pos = DeterminePosition(caster, spell);
            Towers.RemoveAll(t => t.Position.AlmostEqual(pos, 1));
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    { }
}
class GreatWallOfFire(BossModule module) : Components.IconSharedTankbuster(module, (uint)IconID.WallOfFireTankbuster, AID.GreatWallOfFire, new AOEShapeRect(60, 3f));
class RM11TheTyrantStates : StateMachineBuilder
{
    public RM11TheTyrantStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CrownOfArcadia>()
            .ActivateOnEnter<Smashdown1>()
            .ActivateOnEnter<Smashdown2>()
            .ActivateOnEnter<Smashdown3>()
            .ActivateOnEnter<VoidStardust>()
            .ActivateOnEnter<Cometite>()
            .ActivateOnEnter<AssaultEvolved1>()
            .ActivateOnEnter<AssaultEvolved2>()
            .ActivateOnEnter<AssaultEvolved3>()
            .ActivateOnEnter<DanceOfDomination>()
            .ActivateOnEnter<Explosion1>()
            .ActivateOnEnter<Explosion2>()
            .ActivateOnEnter<Explosion3>()
            .ActivateOnEnter<RawSteelTankBuster>()
            .ActivateOnEnter<RawSteelSpreads>()
            .ActivateOnEnter<Charybdistopia>()
            .ActivateOnEnter<Maelstrom>()
            .ActivateOnEnter<PowerfulGust>()
            .ActivateOnEnter<OneAndOnly>()
            .ActivateOnEnter<CosmicKiss>()
            .ActivateOnEnter<MassiveMeteor>()
            .ActivateOnEnter<ForegoneFatality>()
            .ActivateOnEnter<DoubleTyrannhilation>()
            .ActivateOnEnter<HiddenTyrannhilation>()
            .ActivateOnEnter<Flatliner>()
            .ActivateOnEnter<FlatlinerKnockUp>()
            .ActivateOnEnter<MajesticMeteor>()
            .ActivateOnEnter<MajesticMeteorain>()
            .ActivateOnEnter<FireAndFury1>()
            .ActivateOnEnter<FireAndFury2>()
            .ActivateOnEnter<MammothMeteor>()
            .ActivateOnEnter<ExplosionTower>()
            .ActivateOnEnter<ExplosionKnockUp>()
            .ActivateOnEnter<ArcadionAvalanche>()
            .ActivateOnEnter<ArcadionAvalancheToss>()
            .ActivateOnEnter<HeartbreakKick>()
            .ActivateOnEnter<GreatWallOfFire>();
    }
}
[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1072, NameID = 14305)]
public class RM11TheTyrant(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsRect(20, 20));
