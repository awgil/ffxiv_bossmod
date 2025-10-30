namespace BossMod.RealmReborn.Alliance.A15KingBehemoth;

public enum OID : uint
{
    Boss = 0x932, // R8.700, x1
    Helper = 0x933, // R0.500, x1
    IronGiant = 0x935, // R3.000, x0 (spawn during fight)
    Puroboros = 0x934, // R2.400, x0 (spawn during fight)

    Comet = 0x936
}

public enum AID : uint
{
    AutoAttack = 1461, // Boss/IronGiant->player, no cast, single-target
    GrandSword = 1785, // IronGiant->self, no cast, range 12+R 120-degree cone
    MagitekRay = 1787, // IronGiant->location, no cast, range 6 circle
    EclipticMeteor = 1756, // Boss->self, 10.0s cast, ???
    SelfDestruct = 1789, // Puroboros->self, 3.0s cast, range 6+R circle
}

class Comet(BossModule module) : BossComponent(module)
{
    private readonly DateTime[] _comets = new DateTime[PartyState.MaxAllies];

    public override void OnEventVFX(Actor actor, uint vfxID, ulong targetID)
    {
        if (vfxID == 298 && Raid.TryFindSlot(actor, out var slot))
            _comets[slot] = WorldState.FutureTime(12.1f);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_comets[pcSlot] > WorldState.CurrentTime)
            Arena.AddCircle(Module.PrimaryActor.Position, Module.PrimaryActor.HitboxRadius + 2.4f, ArenaColor.Danger);

        Arena.Actors(Module.Enemies(OID.Comet).Where(e => !e.IsDead), ArenaColor.Object, true);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_comets[slot] > WorldState.CurrentTime)
            hints.Add("Drop comet outside boss!", actor.Position.InCircle(Module.PrimaryActor.Position, Module.PrimaryActor.HitboxRadius + 2.4f));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_comets[slot] > WorldState.CurrentTime)
            hints.AddForbiddenZone(ShapeContains.Circle(Module.PrimaryActor.Position, Module.PrimaryActor.HitboxRadius + 2.4f), _comets[slot]);
    }
}

class EclipticMeteor(BossModule module) : Components.CastLineOfSightAOE(module, AID.EclipticMeteor, 100, false)
{
    public override IEnumerable<Actor> BlockerActors() => Module.Enemies(OID.Comet).Where(a => !a.Position.InCircle(Module.PrimaryActor.Position, Module.PrimaryActor.HitboxRadius + a.HitboxRadius));

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Comet)
            Refresh();
    }
}

class SelfDestruct(BossModule module) : Components.StandardAOEs(module, AID.SelfDestruct, 8.4f);

class A15KingBehemothStates : StateMachineBuilder
{
    public A15KingBehemothStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<EclipticMeteor>()
            .ActivateOnEnter<Comet>()
            .ActivateOnEnter<SelfDestruct>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 92, NameID = 727)]
public class A15KingBehemoth(WorldState ws, Actor primary) : BossModule(ws, primary, new(-110, -368.35f), new ArenaBoundsCircle(29.5f));
