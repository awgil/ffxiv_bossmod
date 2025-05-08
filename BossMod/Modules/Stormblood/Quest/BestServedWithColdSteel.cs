namespace BossMod.Stormblood.Quest.BestServedWithColdSteel;

public enum OID : uint
{
    Boss = 0x1A52, // R2.100f, x1
    Grynewaht = 0x1A53,
    Helper = 0x233C,
}

public enum AID : uint
{
    CermetPile = 8117, // 1A52->self, 3.0fs cast, range 4$1fR width 6 rect
    Firebomb = 8495, // Boss->location, 3.0fs cast, range 4 circle
    OpenFire1 = 8121, // 19D9->location, 3.0fs cast, range 6 circle
    AugmentedSuffering = 8492, // Boss->self, 3.5fs cast, range $1fR circle
    AugmentedUprising = 8493, // Boss->self, 3.0fs cast, range $1fR 120-degree cone
    SelfDetonate = 8122, // 1A56->self, no cast, range 6 circle
    SelfDetonate1 = 9169, // Boss->self, 60.0s cast, range 100 circle
}

public enum TetherID : uint
{
    Mine = 54, // 1A56->player
}

class AugmentedUprising(BossModule module) : Components.StandardAOEs(module, AID.AugmentedUprising, new AOEShapeCone(8.5f, 60.Degrees()));
class AugmentedSuffering(BossModule module) : Components.StandardAOEs(module, AID.AugmentedSuffering, new AOEShapeCircle(6.5f));
class OpenFire(BossModule module) : Components.StandardAOEs(module, AID.OpenFire1, 6);

class CermetPile(BossModule module) : Components.StandardAOEs(module, AID.CermetPile, new AOEShapeRect(42.1f, 3f));
class Firebomb(BossModule module) : Components.StandardAOEs(module, AID.Firebomb, 4);

class MagitekTurret(BossModule module) : Components.GenericAOEs(module, AID.SelfDetonate)
{
    class Mine(Actor source, Actor target, WPos sourcePosLastFrame, DateTime tethered)
    {
        public Actor source = source;
        public Actor target = target;
        public WPos sourcePosLastFrame = sourcePosLastFrame;
        public DateTime tethered = tethered;

        public float DistanceLeft(WorldState ws)
        {
            var elapsed = (float)(ws.CurrentTime - tethered).TotalSeconds;
            // approximation, turret starts moving after about 3.7s on average, but 4 is a nice round number
            return Math.Clamp(12 - elapsed, 0, 8) * 3;
        }
    }

    private readonly List<Mine> Mines = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var m in Mines.Where(m => m.target == actor))
        {
            var mineToPlayer = m.target.Position - m.source.Position;
            var projectedExplosion = mineToPlayer.Length() > m.DistanceLeft(WorldState)
                ? (m.target.Position - m.source.Position).Normalized() * m.DistanceLeft(WorldState)
                // offset danger zone slightly toward mine so that AI can dodge
                // if centered on player it doesn't know which direction to go
                : mineToPlayer * 0.9f;
            yield return new AOEInstance(new AOEShapeCircle(6), m.source.Position + projectedExplosion, default, Activation: m.tethered.AddSeconds(12));
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Mine && WorldState.Actors.Find(tether.Target) is Actor target)
            Mines.Add(new(source, target, source.Position, WorldState.CurrentTime));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SelfDetonate)
            Mines.RemoveAll(m => m.source == caster);
    }

    public override void OnActorDestroyed(Actor actor)
    {
        Mines.RemoveAll(m => m.source == actor);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var m in Mines.Where(m => m.target == pc))
            Arena.AddLine(m.source.Position, pc.Position, ArenaColor.Danger);
    }
}

class MagitekSelfDetonate(BossModule module) : Components.CastCounter(module, AID.SelfDetonate1)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            NumCasts++;
    }
}

class MagitekVanguardIPrototypeStates : StateMachineBuilder
{
    private readonly MagitekVanguardIPrototype _module;

    private float BossHPRatio => (float)_module.PrimaryActor.HPMP.CurHP / _module.PrimaryActor.HPMP.MaxHP;

    public MagitekVanguardIPrototypeStates(MagitekVanguardIPrototype module) : base(module)
    {
        _module = module;
        DeathPhase(0, P1);
    }

    private void P1(uint id)
    {
        Condition(id, 300, () => BossHPRatio < 0.9f, "Adds 1")
            .ActivateOnEnter<CermetPile>()
            .ActivateOnEnter<Firebomb>()
            .ActivateOnEnter<AugmentedSuffering>()
            .ActivateOnEnter<OpenFire>()
            .ActivateOnEnter<AugmentedUprising>();

        Condition(id + 2, 300, () => BossHPRatio < 0.75f, "Adds 2");
        Condition(id + 4, 300, () => BossHPRatio < 0.65f, "Turrets").ActivateOnEnter<MagitekTurret>();
        Condition(id + 6, 300, () => BossHPRatio < 0.55f, "Adds 3");
        Condition(id + 8, 300, () => BossHPRatio < 0.4f, "Cutscene").ActivateOnEnter<MagitekSelfDetonate>();
        ComponentCondition<MagitekSelfDetonate>(id + 10, 18, m => m.NumCasts > 0);
        CastEnd(id + 12, 60, "Self-detonate").SetHint(StateMachine.StateHint.DowntimeStart);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 67989, NameID = 5650)]
public class MagitekVanguardIPrototype(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, CustomBounds)
{
    private static readonly List<WDir> vertices = [
        new(-487.40f, -230.79f), new(-487.56f, -188.08f), new(-478.75f, -181.25f), new(-439.37f, -183.46f), new(-457.85f, -211.90f), new(-461.13f, -228.75f)
    ];

    public static readonly WPos ArenaCenter = new(-465.40f, -202.09f);
    public static readonly ArenaBoundsCustom CustomBounds = new(30, new(vertices.Select(v => v - ArenaCenter.ToWDir())));

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
        {
            if (h.Actor.OID == 0x1A52)
                h.Priority = 1;
            else if (h.Actor.TargetID == actor.InstanceID)
                h.Priority = 2;
            else
                h.Priority = 0;
        }
    }
}
