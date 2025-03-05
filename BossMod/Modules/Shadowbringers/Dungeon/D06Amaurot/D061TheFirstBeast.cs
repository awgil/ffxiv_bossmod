namespace BossMod.Shadowbringers.Dungeon.D06Amaurot.D061FirstBeast;

public enum OID : uint
{
    Boss = 0x27B6, // R=5.4
    FallenStar = 0x29DC, // R=2.4
    FallingTower = 0x18D6, // R=0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    VenomousBreath = 15566, // Boss->self, 3.0s cast, range 9 120-degree cone
    MeteorRainVisual = 15556, // Boss->self, 3.0s cast, single-target
    MeteorRain = 15558, // Helper->location, 3.0s cast, range 6 circle

    TheFallingSkyVisual = 15561, // Boss->self, 3.0s cast, single-target
    TheFallingSky = 15562, // Helper->location, 4.5s cast, range 10 circle
    TheFinalSky = 15563, // Boss->self, 12.0s cast, range 70 circle, meteor if failed to LoS
    CosmicKiss = 17108, // FallenStar->self, 4.5s cast, range 50 circle, meteor, damage fall off AOE
    CosmicShrapnel = 17110, // FallenStar->self, no cast, range 8 circle, meteor explodes after final sky, can be ignored since it only does like 500 dmg

    Towerfall = 15564, // FallingTower->self, 8.0s cast, range 35 width 40 rect
    Earthquake = 15565, // Boss->self, 4.0s cast, range 10 circle
    TheBurningSkyVisual = 15559, // Boss->self, 5.2s cast, single-target
    TheBurningSky1 = 13642, // FallingTower->location, 3.5s cast, range 6 circle
    TheBurningSky2 = 15560, // Helper->player, 5.2s cast, range 6 circle, spread
}

public enum IconID : uint
{
    Meteor = 57, // player
    Spreadmarker = 139, // player
}

class VenomousBreath(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.VenomousBreath), new AOEShapeCone(9, 60.Degrees()));
class MeteorRain(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.MeteorRain), 6);
class TheFallingSky(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.TheFallingSky), 10);
class CosmicKiss(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CosmicKiss), new AOEShapeCircle(10));
class Towerfall(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Towerfall), new AOEShapeRect(35, 20));
class Earthquake(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Earthquake), new AOEShapeCircle(10));
class TheBurningSky1(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.TheBurningSky1), 6);
class TheBurningSky2(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.TheBurningSky2), 6);

class Meteors(BossModule module) : Components.GenericBaitAway(module)
{
    public List<Actor> targets = [];

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Meteor)
        {
            CurrentBaits.Add(new(actor, actor, new AOEShapeCircle(10)));
            targets.Add(actor);
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.FallenStar)
        {
            CurrentBaits.Clear();
            targets.Clear();
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (targets.Contains(actor))
            hints.Add("Place meteor!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (targets.Contains(actor))
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(new(-80, 97), new(-80, 67), 15));
    }
}

class TheFinalSky(BossModule module) : Components.CastLineOfSightAOE(module, ActionID.MakeSpell(AID.TheFinalSky), 70, false)
{
    public override IEnumerable<Actor> BlockerActors() => Module.Enemies(OID.FallenStar);
}

class D061FirstBeastStates : StateMachineBuilder
{
    public D061FirstBeastStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TheFinalSky>()
            .ActivateOnEnter<Meteors>()
            .ActivateOnEnter<TheBurningSky1>()
            .ActivateOnEnter<TheBurningSky2>()
            .ActivateOnEnter<Earthquake>()
            .ActivateOnEnter<Towerfall>()
            .ActivateOnEnter<CosmicKiss>()
            .ActivateOnEnter<TheFallingSky>()
            .ActivateOnEnter<MeteorRain>()
            .ActivateOnEnter<VenomousBreath>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 652, NameID = 8201)]
public class D061FirstBeast(WorldState ws, Actor primary) : BossModule(ws, primary, new(-80, 82), new ArenaBoundsSquare(19.5f));
