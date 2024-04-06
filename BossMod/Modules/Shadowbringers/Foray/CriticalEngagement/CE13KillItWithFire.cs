namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE13KillItWithFire;

public enum OID : uint
{
    Boss = 0x2E2F, // R2.250, x1
    Helper = 0x233C, // R0.500, x32
    RottenMandragora = 0x2E30, // R1.050, spawn during fight
    Pheromones = 0x2E31, // R1.500, spawn during fight
};

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target
    Teleport = 20513, // Boss->location, no cast, single-target, teleport

    HarvestFestival = 20511, // Boss->self, 4.0s cast, single-target, visual (summon mandragoras)
    PheromonesVisual1 = 20512, // RottenMandragora->self, no cast, single-target, visual (???)
    PheromonesVisual2 = 20514, // RottenMandragora->location, no cast, single-target, visual (???)
    RancidPheromones = 20515, // RottenMandragora->self, no cast, single-target, visual (???)
    Heartbreak = 20516, // Pheromones->self, no cast, range 4 circle when pheromone is touched
    DeadLeaves = 20517, // Boss->self, 4.0s cast, range 30 circle, visual (recolors)
    TenderAnaphylaxis = 20518, // Helper->self, 4.0s cast, range 30 90-degree cone
    JealousAnaphylaxis = 20519, // Helper->self, 4.0s cast, range 30 90-degree cone
    AnaphylacticShock = 20520, // Helper->self, 4.0s cast, range 30 width 2 rect aoe (borders)
    SplashBomb = 20521, // Boss->self, 4.0s cast, single-target, visual (puddles)
    SplashBombAOE = 20522, // Helper->self, 4.0s cast, range 6 circle puddle
    SplashGrenade = 20523, // Boss->self, 5.0s cast, single-target, visual (stack)
    SplashGrenadeAOE = 20524, // Helper->players, 5.0s cast, range 6 circle stack
    PlayfulBreeze = 20525, // Boss->self, 4.0s cast, single-target, visual (raidwide)
    PlayfulBreezeAOE = 20526, // Helper->self, 4.0s cast, range 60 circle raidwide
    Budbutt = 20527, // Boss->player, 4.0s cast, single-target, tankbuster
};

public enum SID : uint
{
    TenderAnaphylaxis = 2301, // Helper->player, extra=0x0
    JealousAnaphylaxis = 2302, // Helper->player, extra=0x0
};

class Pheromones : Components.PersistentVoidzone
{
    public Pheromones() : base(4, m => m.Enemies(OID.Pheromones)) { }
}

class DeadLeaves : Components.GenericAOEs
{
    private BitMask _tenderStatuses;
    private BitMask _jealousStatuses;
    private readonly List<Actor> _tenderCasters = [];
    private readonly List<Actor> _jealousCasters = [];

    private static readonly AOEShapeCone _shape = new(30, 45.Degrees());

    public DeadLeaves() : base(new(), "Go to different color!") { }

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        if (_tenderStatuses[slot])
            foreach (var c in _tenderCasters)
                yield return new(_shape, c.Position, c.CastInfo!.Rotation, c.CastInfo.NPCFinishAt);
        if (_jealousStatuses[slot])
            foreach (var c in _jealousCasters)
                yield return new(_shape, c.Position, c.CastInfo!.Rotation, c.CastInfo.NPCFinishAt);
    }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.TenderAnaphylaxis:
                _tenderStatuses.Set(module.Raid.FindSlot(actor.InstanceID));
                break;
            case SID.JealousAnaphylaxis:
                _jealousStatuses.Set(module.Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.TenderAnaphylaxis:
                _tenderStatuses.Clear(module.Raid.FindSlot(actor.InstanceID));
                break;
            case SID.JealousAnaphylaxis:
                _jealousStatuses.Clear(module.Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        CastersForAction(spell.Action)?.Add(caster);
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        CastersForAction(spell.Action)?.Remove(caster);
    }

    private List<Actor>? CastersForAction(ActionID action) => (AID)action.ID switch
    {
        AID.TenderAnaphylaxis => _tenderCasters,
        AID.JealousAnaphylaxis => _jealousCasters,
        _ => null
    };
}

class AnaphylacticShock : Components.SelfTargetedAOEs
{
    public AnaphylacticShock() : base(ActionID.MakeSpell(AID.AnaphylacticShock), new AOEShapeRect(30, 1)) { }
}

class SplashBomb : Components.SelfTargetedAOEs
{
    public SplashBomb() : base(ActionID.MakeSpell(AID.SplashBombAOE), new AOEShapeCircle(6)) { }
}

class SplashGrenade : Components.StackWithCastTargets
{
    public SplashGrenade() : base(ActionID.MakeSpell(AID.SplashGrenadeAOE), 6) { }
}

class PlayfulBreeze : Components.RaidwideCast
{
    public PlayfulBreeze() : base(ActionID.MakeSpell(AID.PlayfulBreeze)) { }
}

class Budbutt : Components.SingleTargetCast
{
    public Budbutt() : base(ActionID.MakeSpell(AID.Budbutt)) { }
}

class CE13KillItWithFireStates : StateMachineBuilder
{
    public CE13KillItWithFireStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Pheromones>()
            .ActivateOnEnter<DeadLeaves>()
            .ActivateOnEnter<AnaphylacticShock>()
            .ActivateOnEnter<SplashBomb>()
            .ActivateOnEnter<SplashGrenade>()
            .ActivateOnEnter<PlayfulBreeze>()
            .ActivateOnEnter<Budbutt>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.BozjaCE, GroupID = 735, NameID = 1)] // bnpcname=9391
public class CE13KillItWithFire : BossModule
{
    public CE13KillItWithFire(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(-90, 700), 25)) { }
}
