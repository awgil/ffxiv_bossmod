namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE206DarkArtistry;

public enum OID : uint
{
    PhantomNecromancer = 0x4BC1,
    Helper = 0x233C,
    LongDeadExplorer = 0x4BC2, // R1.000, x0 (spawn during fight)
    LongDeadPirate = 0x4BC3, // R2.600, x0 (spawn during fight)
    PhantomNecromancer1 = 0x4C75, // R1.000, x1
}

public enum AID : uint
{
    AutoAttack = 50761, // PhantomNecromancer->player, no cast, single-target
    Ability = 47173, // 4C75->self, no cast, ???
    DarkII = 47181, // PhantomNecromancer->self, 5.0s cast, range 50 width 50 rect
    DarkFlareCast = 47182, // PhantomNecromancer->self, 5.0s cast, single-target
    DarkFlare = 47183, // Helper->self, no cast, ???
    ArcaneRevelation = 47179, // PhantomNecromancer->self, 3.0s cast, single-target
    Necrosurge = 47180, // Helper->self, 7.0s cast, range 70 width 12 rect

    RiseOfTheFallen = 47174, // PhantomNecromancer->self, 3.0s cast, single-target
    LongDeadExplorerExplosion = 47175, // 4BC2->self, 2.0s cast, range 8 circle
    LongDeadPirateExplosion = 47176, // 4BC3->self, 4.0s cast, range 80 width 7 cross
}

public enum SID : uint
{
    ExplosionTimer = 2056, // none->LongDeadExplorer, extra=0x26B
}

sealed class DarkII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DarkII, new AOEShapeRect(50.0f, 25.0f));
sealed class DarkFlare(BossModule module) : Components.RaidwideCast(module, (uint)AID.DarkFlareCast);
sealed class Necrosurge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Necrosurge, new AOEShapeRect(70.0f, 6.0f));

sealed class LongDeadExplorer(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LongDeadExplorerExplosion, new AOEShapeCircle(8.0f),
    riskyWithSecondsLeft: 4.0f)
{

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.LongDeadExplorer && id == 4564)
        {
            Casters.Add(new(Shape, actor.Position, actor.Rotation, WorldState.FutureTime(7.1f), actorID: actor.InstanceID));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) { }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Casters.Count;
        if (count == 0)
        {
            return [];
        }

        var aoes = CollectionsMarshal.AsSpan(Casters);
        var deadline = aoes[0].Activation.AddSeconds(1.0f);

        var index = 0;
        while (index < count)
        {
            ref var aoe = ref aoes[index];
            if (aoe.Activation >= deadline)
            {
                break;
            }

            index++;
        }

        var max = index * 2 > count ? count : index * 2;

        if (RiskyWithSecondsLeft != default)
        {
            var time = WorldState.CurrentTime;
            for (var i = 0; i < max; i++)
            {
                ref var aoe = ref aoes[i];
                aoe.Risky = aoe.Activation.AddSeconds(-RiskyWithSecondsLeft) <= time;
                aoe.Color = i < index ? Colors.Danger : Colors.AOE;
            }
        }

        return aoes[..max];
    }
}

sealed class LongDeadPirate(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LongDeadPirateExplosion, new AOEShapeCross(80.0f, 3.5f), 4,
    riskyWithSecondsLeft: 5.0f)
{

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.LongDeadPirate && id == 4561)
        {
            Casters.Add(new(Shape, actor.Position, actor.Rotation, WorldState.FutureTime(9.0f), actorID: actor.InstanceID));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) { }
}

[SkipLocalsInit]
sealed class CE206DarkArtistryStates : StateMachineBuilder
{
    public CE206DarkArtistryStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DarkII>()
            .ActivateOnEnter<DarkFlare>()
            .ActivateOnEnter<LongDeadExplorer>()
            .ActivateOnEnter<LongDeadPirate>()
            .ActivateOnEnter<Necrosurge>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified,
    StatesType = typeof(CE206DarkArtistryStates),
    ConfigType = null, // replace null with typeof(PhantomNecromancerConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = typeof(SID),
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.PhantomNecromancer,
    Contributors = "Equilius",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CriticalEngagement,
    GroupID = 1093u,
    NameID = 57u,
    SortOrder = 9,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class CE206DarkArtistry(WorldState ws, Actor primary) : BossModule(ws, primary, new(224.000f, -860.000f), new ArenaBoundsSquare(20f))
{
    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InSquare(Arena.Center, 20f);
}
