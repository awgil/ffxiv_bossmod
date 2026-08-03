namespace BossMod.RealmReborn.Dungeon.D07Brayflox.D073Hellbender;

public enum OID : uint
{
    // Boss
    Boss = 0x1AB, // Hellbender

    // Aiatar
    Aiatar = 0x1AD, // Aiatar

    // Trash
    QueerBubble = 0x428 // Queer Bubble
}

public enum AID : uint
{
    // Boss
    AutoAttack = 872, // Boss->player, no cast, single target
    BogBubble = 980, // Boss->player, no cast, range 6 circle aoe
    PeculiarLight = 982, // Boss->self, 3.5s cast, range 8 circle aoe
    StagnantSpray = 448, // Boss->self, 2.5s cast, range 8 120-degree cone aoe
    Effluvium = 979, // Boss->player, no cast, single target

    // Aiater
    AutoAttackAiatar = 870, // Boss-player, no cast, single target
    Touchdown = 564 // Boss->self, no cast, range 10 circle aoe
}

public enum SID : uint
{
    Bind = 280 // none->player, extra = 0x0
}

class BogBubble(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BogBubble, 6);

class PeculiarLight(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PeculiarLight, 8);

class StagnantSpray(BossModule module) : Components.SimpleAOEs(module, (uint)AID.StagnantSpray, new AOEShapeCone(8, 60.Degrees()));

class PlayerBound(BossModule module) : BossComponent(module)
{
    private BitMask _bound;

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => _bound[playerSlot] ? PlayerPriority.Interesting : PlayerPriority.Normal;

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {

        foreach (var slot in _bound.SetBits())
        {
            // assign actor and add circle around them.
            var actor = Raid[slot];
            if (actor != null)
                Arena.ZoneCircleOutline(actor.Position, 1.5f, Colors.PlayerInteresting, 1.5f);
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        // Add global hint text
        if (!_bound.None())
            hints.Add("Kill Queer Bubble to free bound player!");
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        // True if player has bind status
        if (status.ID == (uint)SID.Bind)
            _bound[Raid.FindSlot(actor.InstanceID)] = true;
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        // False if player no longer has bind status
        if (status.ID == (uint)SID.Bind)
            _bound[Raid.FindSlot(actor.InstanceID)] = false;
    }
}

class D120HellbenderStates : StateMachineBuilder
{
    public D120HellbenderStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BogBubble>()
            .ActivateOnEnter<PeculiarLight>()
            .ActivateOnEnter<StagnantSpray>()
            .ActivateOnEnter<PlayerBound>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Chuggalo", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 8, NameID = 1286)]
public class D120Hellbender(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] shape = [new(-118.14f, -57.46f), new(-116.88f, -56.86f), new(-116.30f, -56.76f), new(-115.77f, -57.08f), new(-115.12f, -57.40f),
    new(-112.37f, -57.15f), new(-112.00f, -56.79f), new(-111.28f, -55.52f), new(-109.68f, -53.20f), new(-109.16f, -53.06f),
    new(-108.68f, -53.47f), new(-108.32f, -53.83f), new(-108.62f, -54.43f), new(-109.97f, -56.02f), new(-110.17f, -56.56f),
    new(-108.21f, -56.57f), new(-107.53f, -56.46f), new(-105.35f, -53.79f), new(-104.82f, -53.60f), new(-102.90f, -53.24f),
    new(-102.27f, -52.97f), new(-94.97f, -48.62f), new(-94.65f, -48.23f), new(-89.57f, -38.75f), new(-90.91f, -26.39f),
    new(-89.51f, -22.63f), new(-89.60f, -21.93f), new(-93.91f, -15.04f), new(-94.41f, -14.96f), new(-97.26f, -16.44f),
    new(-97.86f, -16.66f), new(-98.47f, -16.75f), new(-98.85f, -16.36f), new(-101.13f, -12.32f), new(-101.81f, -12.12f),
    new(-110.24f, -11.58f), new(-110.62f, -11.92f), new(-111.16f, -12.79f), new(-112.00f, -13.77f), new(-112.57f, -14.04f),
    new(-114.50f, -14.32f), new(-115.21f, -14.55f), new(-115.87f, -14.48f), new(-117.61f, -14.66f), new(-118.17f, -14.52f),
    new(-121.39f, -11.42f), new(-121.87f, -11.21f), new(-124.70f, -10.53f), new(-126.08f, -10.43f), new(-126.66f, -10.80f),
    new(-130.67f, -15.45f), new(-132.57f, -35.03f), new(-132.68f, -35.61f), new(-134.25f, -40.97f), new(-134.25f, -41.64f),
    new(-121.34f, -56.68f), new(-120.78f, -57.12f), new(-118.82f, -57.42f), new(-118.14f, -57.46f)];
    // Centroid of the polygon is at: (-111.924f, -33.284f)

    public static readonly ArenaBoundsCustom arena = new([new PolygonCustom(shape)]);

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.QueerBubble => 2,
                (uint)OID.Boss => 1,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Aiatar));
    }
}
