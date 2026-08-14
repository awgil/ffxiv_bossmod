namespace BossMod.Dawntrail.Dungeon.D08StrayboroughDeadwalk.D083Träumerei;

public enum OID : uint
{
    Boss = 0x421F, // R26.0
    StrayGeist = 0x4221, // R2.0
    StrayPhantagenitrix = 0x4220, // R1.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 16764, // Boss->player, no cast, single-target

    BitterRegretVisual = 37140, // Boss->self, 6.0s cast, single-target
    BitterRegret1 = 37139, // Boss->self, 6.0+0.7s cast, range 40 width 16 rect
    BitterRegret2 = 37147, // Helper->self, 6.7s cast, range 50 width 12 rect
    BitterRegret3 = 37340, // StrayPhantagenitrix->self, 6.0s cast, range 40 width 4 rect

    Poltergeist = 37132, // Boss->self, 3.0s cast, single-target

    MemorialMarch1 = 37136, // Boss->self, 3.0s cast, single-target
    MemorialMarch2 = 37065, // Boss->self, 6.0s cast, single-target

    Impact = 37133, // Helper->self, 6.0s cast, range 40 width 4 rect

    IllIntent = 39607, // StrayGeist->player, 10.0s cast, single-target
    MaliciousMistTether = 37138, // StrayGeist->player, 10.0s cast, single-target

    GhostdusterSpreadVisual = 37145, // Boss->self, 8.0s cast, single-target
    Ghostduster = 37146, // Helper->player, 8.0s cast, range 8 circle, spread

    MaliciousMistRaidwide = 37168, // Boss->self, 5.0s cast, range 60 circle

    Fleshbuster = 37148, // Boss->self, 8.0s cast, range 60 circle

    GhostcrusherVisual = 37142, // Boss->self, 5.0s cast, single-target, line stack
    GhostcrusherMarker = 37144, // Helper->player, no cast, single-target
    Ghostcrusher = 37143 // Helper->self, no cast, range 80 width 8 rect
}

public enum SID : uint
{
    GhostlyGuise = 3949 // none->player, extra=0x0
}

sealed class ImpactArenaChange(BossModule module) : BossComponent(module)
{
    private bool active;
    private readonly GhostlyGuise ghost = module.FindComponent<GhostlyGuise>()!;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x0B)
        {
            if (state == 0x00800040u)
            {
                active = true;
            }
            else if (state == 0x00080004u)
            {
                active = false;
                Arena.Bounds = new ArenaBoundsSquare(19.5f);
            }
        }
    }
    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (active)
        {
            var isSquare = Arena.Bounds is ArenaBoundsSquare;
            var ghostly = ghost.Ghostly[pcSlot];
            Arena.Bounds = ghostly && !isSquare ? new ArenaBoundsSquare(19.5f)
            : !ghostly && isSquare ? new ArenaBoundsCustom([new Square(Arena.Center, 19.5f)], [new Cross(Arena.Center, 20f, 1.5f)])  // for some reason the obstacle cross is smaller than the AOE;
            : Arena.Bounds;
        }
    }
}

sealed class GhostlyGuise(BossModule module) : Components.GenericAOEs(module)
{
    private readonly Ghostduster _avoid = module.FindComponent<Ghostduster>()!;
    private readonly IllIntentMaliciousMist _seek = module.FindComponent<IllIntentMaliciousMist>()!;

    private readonly AOEShapeCircle circle = new(3f);
    private bool activated;
    private bool isFleshbuster;
    private DateTime activationFleshbuster;
    private readonly AOEInstance[] circles = new AOEInstance[4];
    public BitMask Ghostly;
    private bool risky;
    private DateTime activation;
    private SDIntersection? shapeDistances;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!activated)
        {
            return [];
        }

        if (_avoid.Spreads.Count != 0)
        {
            risky = !Ghostly[slot];
            activation = _avoid.Spreads.Ref(0).Activation;
        }
        else if (isFleshbuster)
        {
            risky = Ghostly[slot];
            activation = activationFleshbuster;
        }
        else if (_seek.CurrentBaits.Count != 0)
        {
            risky = Ghostly[slot];
        }
        else
        {
            risky = true;
        }
        return circles;
    }

    public override void Update()
    {
        if (!activated)
        {
            return;
        }
        var color = risky ? default : Colors.SafeFromAOE;
        for (var i = 0; i < 4; ++i)
        {
            ref var aoe = ref circles[i];
            aoe.Color = color;
            aoe.Activation = activation;
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (state == 0x00020001u && index == 0x0Cu) // 0x0C, 0x0D, 0x0E, 0xOF happen at the same time, one for each platform
        {
            activated = true;
            WPos[] positions = [new(137.5f, -443.5f), new(158.5f, -443.5f), new(137.5f, -422.5f), new(158.5f, -422.5f)];
            var distances = new ShapeDistance[4];
            for (var i = 0; i < 4; ++i)
            {
                var pos = positions[i];
                circles[i] = new(circle, pos);

                distances[i] = new SDInvertedCircle(pos, 3f);
            }
            shapeDistances = new SDIntersection(distances);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Fleshbuster)
        {
            isFleshbuster = true;
            activationFleshbuster = Module.CastFinishAt(spell);
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.GhostlyGuise)
        {
            Ghostly.Set(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.GhostlyGuise)
        {
            Ghostly.Clear(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Fleshbuster)
        {
            isFleshbuster = false;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (isFleshbuster || _seek.CurrentBaits.Count != 0)
        {
            hints.Add("Turn into a ghost!", !Ghostly[slot]);
        }
        else if (_avoid.Spreads.Count != 0)
        {
            hints.Add("Turn into flesh!", Ghostly[slot]);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!risky && shapeDistances != null)
        {
            hints.AddForbiddenZone(shapeDistances, activation);
        }
        else
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }
}

sealed class MaliciousMistRaidwide(BossModule module) : Components.RaidwideCast(module, (uint)AID.MaliciousMistRaidwide);
sealed class IllIntentMaliciousMist(BossModule module) : Components.StretchTetherDuo(module, 20f, 10f)
{
    private GhostlyGuise? ghost;

    // ill intent seems to break after 17, malicious mist after 20, not worth the effort to differentiate
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        ghost ??= Module.FindComponent<GhostlyGuise>();
        if (ghost!.Ghostly[slot])
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }
}

sealed class BitterRegret1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BitterRegret1, new AOEShapeRect(50f, 8f));
sealed class BitterRegret2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BitterRegret2, new AOEShapeRect(50f, 6f));
sealed class BitterRegret3(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BitterRegret3, new AOEShapeRect(40f, 2f), 5);
sealed class Impact(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Impact, new AOEShapeRect(40f, 2f));
sealed class Ghostcrusher(BossModule module) : Components.LineStack(module, aidMarker: (uint)AID.GhostcrusherMarker, (uint)AID.Ghostcrusher, 5d, 80f, maxStackSize: 4);
sealed class Ghostduster(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Ghostduster, 8f)
{
    private GhostlyGuise? ghost;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        ghost ??= Module.FindComponent<GhostlyGuise>();
        if (!ghost!.Ghostly[slot])
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }
}

sealed class D083TräumereiStates : StateMachineBuilder
{
    public D083TräumereiStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<IllIntentMaliciousMist>()
            .ActivateOnEnter<Ghostduster>()
            .ActivateOnEnter<GhostlyGuise>()
            .ActivateOnEnter<ImpactArenaChange>()
            .ActivateOnEnter<Ghostcrusher>()
            .ActivateOnEnter<MaliciousMistRaidwide>()
            .ActivateOnEnter<Impact>()
            .ActivateOnEnter<BitterRegret1>()
            .ActivateOnEnter<BitterRegret2>()
            .ActivateOnEnter<BitterRegret3>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 981u, NameID = 12763u)]
public sealed class D083Träumerei(WorldState ws, Actor primary) : BossModule(ws, primary, new(148f, -433f), new ArenaBoundsSquare(19.5f));