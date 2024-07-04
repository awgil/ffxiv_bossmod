namespace BossMod.Endwalker.Dungeon.D02TowerOfBabil.D023Anima;

public enum OID : uint
{
    Boss = 0x33FD, // R=18.7
    LowerAnima = 0x3400, // R=18.7
    IronNail = 0x3401, // R=1.0
    LunarNail = 0x33FE, // R=1.0
    MegaGraviton = 0x33FF, // R=1.0
    Actor1eb239 = 0x1EB239, // R0.500, x0 (spawn during fight), EventObj type
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 25341, // Boss->player, no cast, single-target

    AetherialPull = 25345, // MegaGraviton->player, 8.0s cast, single-target, pull 30 between centers

    BoundlessPainPull = 26229, // Helper->self, no cast, range 60 circle, pull 60 between centers
    BoundlessPainVisual = 25347, // Boss->self, 8.0s cast, single-target, creates expanding AOE
    BoundlessPainFirst = 25348, // Helper->location, no cast, range 6 circle
    BoundlessPainRest = 25349, // Helper->location, no cast, range 6 circle

    CharnelClaw = 25357, // IronNail->self, 6.0s cast, range 40 width 5 rect

    CoffinScratchFirst = 25358, // Helper->location, 3.5s cast, range 3 circle
    CoffinScratchRest = 21239, // Helper->location, no cast, range 3 circle

    Imperatum = 25353, // Boss->self, 5.0s cast, range 60 circle, phase change
    ImperatumPull = 23929, // Helper->player, no cast, single-target, pull 60 between centers

    LunarNail = 25342, // Boss->self, 3.0s cast, single-target

    ObliviatingClaw1 = 25354, // LowerAnima->self, 3.0s cast, single-target
    ObliviatingClaw2 = 25355, // LowerAnima->self, 3.0s cast, single-target
    ObliviatingClawSpawnAOE = 25356, // IronNail->self, 6.0s cast, range 3 circle

    OblivionVisual = 25359, // LowerAnima->self, 6.0s cast, single-target
    OblivionStart = 23697, // Helper->location, no cast, range 60 circle
    OblivionLast = 23872, // Helper->location, no cast, range 60 circle

    MegaGraviton = 25344, // Boss->self, 5.0s cast, range 60 circle, tether mechanic
    GravitonSpark = 25346, // MegaGraviton->player, no cast, single-target, on touching the graviton

    PaterPatriaeAOE = 24168, // Helper->self, 3.5s cast, range 60 width 8 rect
    PaterPatriae2 = 25350, // Boss->self, 3.5s cast, single-target

    PhantomPain1 = 21182, // Boss->self, 7.0s cast, single-target
    PhantomPain2 = 25343, // Helper->self, 7.0s cast, range 20 width 20 rect

    VisualModelChange = 27228, // LowerAnima->self, no cast, single-target

    EruptingPainVisual = 25351, // Boss->self, 5.0s cast, single-target
    EruptingPain = 25352, // Helper->player, 5.0s cast, range 6 circle
}

public enum TetherID : uint
{
    PhantomPain = 162, // Helper->Helper
    AetherialPullBad = 57, // MegaGraviton->player
    AetherialPullGood = 17, // MegaGraviton->player
    AnimaDrawsPower = 22, // Helper->Boss
}

class ArenaChange(BossModule module) : BossComponent(module)
{
    private bool pausedAI;
    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x03)
        {
            if (state == 0x00020001)
                Module.Arena.Center = D023Anima.LowerArenaCenter;
            if (state == 0x00080004)
                Module.Arena.Center = D023Anima.UpperArenaCenter;
        }
    }

    // TODO: This hack is required because otherwise the game crashes on phase change if AI is on and player is following an NPC
    // can be removed if the source of the bug is found and fixed
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Imperatum && AI.AIManager.Instance?.MasterSlot != 0)
        {
            AI.AIManager.Instance?.SwitchToIdle();
            pausedAI = true;
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.LowerAnima && pausedAI)
        {
            AI.AIManager.Instance?.SwitchToFollow(Service.Config.Get<AI.AIConfig>().FollowSlot);
            pausedAI = false;
        }
    }
}

class BoundlessPain(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeCircle circle = new(18);
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.BoundlessPainPull:
                _aoe = new(circle, Module.Arena.Center);
                break;
            case AID.BoundlessPainFirst:
            case AID.BoundlessPainRest:
                if (++NumCasts == 20)
                {
                    _aoe = null;
                    NumCasts = 0;
                }
                break;
        }
    }
}

class Gravitons(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(1);
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var g in Module.Enemies(OID.MegaGraviton).Where(x => !x.IsDead))
            yield return new(circle, g.Position);
    }
}

class AetherialPull(BossModule module) : Components.StretchTetherDuo(module, (uint)TetherID.AetherialPullBad, (uint)TetherID.AetherialPullGood, 33, activationDelay: 7.9f, knockbackImmunity: true);

class CoffinScratch(BossModule module) : Components.StandardChasingAOEs(module, new AOEShapeCircle(3), ActionID.MakeSpell(AID.CoffinScratchFirst), ActionID.MakeSpell(AID.CoffinScratchRest), 6, 2, 5)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (Chasers.Count == 0)
        {
            ExcludedTargets.Reset();
            NumCasts = 0;
        }
    }
}

class PhantomPain(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PhantomPain2), new AOEShapeRect(10, 10, 10));
class PaterPatriaeAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PaterPatriaeAOE), new AOEShapeRect(60, 4));
class CharnelClaw(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CharnelClaw), new AOEShapeRect(40, 2.5f), 5);
class ErruptingPain(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.EruptingPain), 6);
class ObliviatingClawSpawnAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ObliviatingClawSpawnAOE), new AOEShapeCircle(3));
class Oblivion(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.OblivionVisual), "Raidwide x16");
class MegaGraviton(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.MegaGraviton));

class D023AnimaStates : StateMachineBuilder
{
    public D023AnimaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<Gravitons>()
            .ActivateOnEnter<BoundlessPain>()
            .ActivateOnEnter<CoffinScratch>()
            .ActivateOnEnter<PhantomPain>()
            .ActivateOnEnter<AetherialPull>()
            .ActivateOnEnter<PaterPatriaeAOE>()
            .ActivateOnEnter<CharnelClaw>()
            .ActivateOnEnter<ErruptingPain>()
            .ActivateOnEnter<ObliviatingClawSpawnAOE>()
            .ActivateOnEnter<Oblivion>()
            .ActivateOnEnter<MegaGraviton>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 785, NameID = 10285)]
public class D023Anima(WorldState ws, Actor primary) : BossModule(ws, primary, UpperArenaCenter, new ArenaBoundsSquare(19.5f))
{
    public static readonly WPos UpperArenaCenter = new(0, -180);
    public static readonly WPos LowerArenaCenter = new(0, -400);
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(OID.Boss), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.LowerAnima), ArenaColor.Enemy);
    }
}
