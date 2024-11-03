namespace BossMod.Shadowbringers.Dungeon.D11HeroesGauntlet.D112SpectralNecromancer;

public enum OID : uint
{

    Boss = 0x2DF1, // R2.3
    Necrobomb1 = 0x2DF2, // R0.75
    Necrobomb2 = 0x2DF3, // R0.75
    Necrobomb3 = 0x2DF4, // R0.75
    Necrobomb4 = 0x2DF5, // R0.75
    Necrobomb5 = 0x2DF6, // R0.75
    Necrobomb6 = 0x2DF7, // R0.75
    Necrobomb7 = 0x2DF8, // R0.75
    Necrobomb8 = 0x2DF9, // R0.75
    BleedVoidzone = 0x1EB02C,
    NecroPortal = 0x1EB07A,
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6499, // Necrobomb3/Necrobomb4/Necrobomb1/Necrobomb2->player, no cast, single-target
    FellForces = 20305, // Boss->player, no cast, single-target

    AbsoluteDarkII = 20321, // Boss->self, 5.0s cast, range 40 120-degree cone

    TwistedTouch = 20318, // Boss->player, 4.0s cast, single-target
    Necromancy1 = 20311, // Boss->self, 3.0s cast, single-target
    Necromancy2 = 20312, // Boss->self, 3.0s cast, single-target
    Necroburst1 = 20313, // Boss->self, 4.3s cast, single-target
    Necroburst2 = 20314, // Boss->self, 4.3s cast, single-target

    Burst1 = 20322, // Necrobomb1->self, 4.0s cast, range 8 circle
    Burst2 = 21429, // Necrobomb2->self, 4.0s cast, range 8 circle
    Burst3 = 21430, // Necrobomb3->self, 4.0s cast, range 8 circle
    Burst4 = 21431, // Necrobomb4->self, 4.0s cast, range 8 circle
    Burst5 = 20324, // Necrobomb5->self, 4.0s cast, range 8 circle
    Burst6 = 21432, // Necrobomb6->self, 4.0s cast, range 8 circle
    Burst7 = 21433, // Necrobomb7->self, 4.0s cast, range 8 circle
    Burst8 = 21434, // Necrobomb8->self, 4.0s cast, range 8 circle

    PainMireVisual = 20387, // Boss->self, no cast, single-target
    PainMire = 20388, // Helper->location, 5.5s cast, range 9 circle, spawns voidzone smaller than AOE
    DeathThroes = 20323, // Necrobomb5/Necrobomb6/Necrobomb7/Necrobomb8->player, no cast, single-target

    ChaosStorm = 20320, // Boss->self, 4.0s cast, range 40 circle, raidwide
    DarkDelugeVisual = 20316, // Boss->self, 4.0s cast, single-target
    DarkDeluge = 20317 // Helper->location, 5.0s cast, range 5 circle
}

public enum IconID : uint
{
    Baitaway = 23, // player
    Tankbuster = 198 // player
}

public enum SID : uint
{
    Doom = 910 // Boss->player, extra=0x0
}

public enum TetherID : uint
{
    WalkingNecrobombs = 17, // Necrobomb3/Necrobomb1/Necrobomb2/Necrobomb4->2753/player/2757/2752
    CrawlingNecrobombs = 79 // Necrobomb7/Necrobomb8/Necrobomb5/Necrobomb6->player/2753/2757/2752
}

class AbsoluteDarkII(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AbsoluteDarkII), new AOEShapeCone(40, 60.Degrees()));
class PainMire(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.PainMire), 9)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Module.Enemies(OID.BleedVoidzone).Any(x => x.EventState != 7))
        { }
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class BleedVoidzone(BossModule module) : Components.PersistentVoidzone(module, 8, m => m.Enemies(OID.BleedVoidzone).Where(x => x.EventState != 7));
class TwistedTouch(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.TwistedTouch));
class ChaosStorm(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ChaosStorm));
class DarkDeluge(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.DarkDeluge), 5);
class NecrobombBaitAway(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(9.25f), (uint)IconID.Baitaway, ActionID.MakeSpell(AID.DeathThroes), centerAtTarget: true); // note: explosion is not always exactly the position of player, if zombie teleports to player it is player + zombie hitboxradius = 1.25 away

class Necrobombs(BossModule module) : BossComponent(module)
{
    private readonly NecrobombBaitAway _ba = module.FindComponent<NecrobombBaitAway>()!;
    private static readonly AOEShapeCircle circle = new(8);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_ba.ActiveBaits.Any())
            return;
        var forbidden = new List<Func<WPos, float>>();
        foreach (var e in WorldState.Actors.Where(x => !x.IsAlly && x.Tether.ID == (uint)TetherID.CrawlingNecrobombs))
            forbidden.Add(circle.Distance(e.Position, default));
        if (forbidden.Count > 0)
            hints.AddForbiddenZone(p => forbidden.Min(f => f(p)));
    }
}

class Burst(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle circle = new(8);
    private static readonly HashSet<AID> casts = [AID.Burst1, AID.Burst2, AID.Burst3, AID.Burst4, AID.Burst5, AID.Burst6, AID.Burst7, AID.Burst8];
    // Note: Burst5 to Burst8 locations are unknown until players unable to move, so they are irrelevant and not drawn
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (modelState == 54)
            _aoes.Add(new(circle, actor.Position, default, WorldState.FutureTime(6))); // activation time can be vastly different, even twice as high so we take a conservative delay
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (casts.Contains((AID)spell.Action.ID))
            _aoes.Clear();
    }
}

class Doom(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _doomed = [];

    public static bool CanActorCureDoom(Actor actor) => actor.Role == Role.Healer || actor.Class == Class.BRD;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Doom)
            _doomed.Add(actor);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Doom)
            _doomed.Remove(actor);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_doomed.Count > 0 && CanActorCureDoom(actor))
            for (var i = 0; i < _doomed.Count; ++i)
            {
                var doomed = _doomed[i];
                if (actor.Role == Role.Healer)
                    hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Esuna), doomed, ActionQueue.Priority.High);
                else if (actor.Class == Class.BRD)
                    hints.ActionsToExecute.Push(ActionID.MakeSpell(BRD.AID.WardensPaean), doomed, ActionQueue.Priority.High);
            }
    }
}

class D112SpectralNecromancerStates : StateMachineBuilder
{
    public D112SpectralNecromancerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AbsoluteDarkII>()
            .ActivateOnEnter<PainMire>()
            .ActivateOnEnter<BleedVoidzone>()
            .ActivateOnEnter<TwistedTouch>()
            .ActivateOnEnter<ChaosStorm>()
            .ActivateOnEnter<DarkDeluge>()
            .ActivateOnEnter<NecrobombBaitAway>()
            .ActivateOnEnter<Necrobombs>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<Doom>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 737, NameID = 9508)]
public class D112SpectralNecromancer(WorldState ws, Actor primary) : BossModule(ws, primary, new(-450, -531), new ArenaBoundsCircle(19.5f));
