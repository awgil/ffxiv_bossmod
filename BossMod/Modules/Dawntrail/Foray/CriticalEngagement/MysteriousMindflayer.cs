namespace BossMod.Dawntrail.Foray.CriticalEngagement.MysteriousMindflayer;

public enum OID : uint
{
    Boss = 0x46B5, // R3.250, x1
    Helper = 0x233C, // R0.500, x14, Helper type
    JestingJackanapes = 0x46B6, // R0.750, x4
    Tentacle = 0x46B7, // R7.200-12.024, x10
}

public enum AID : uint
{
    AutoAttack = 41173, // Boss->player, no cast, single-target
    DarkII = 41170, // Boss->self, 6.0s cast, range 65 90-degree cone
    Summon = 41168, // Boss->self, 3.0s cast, single-target
    MindBlastCast = 41167, // Boss->self, 4.0+1.0s cast, single-target
    MindBlast = 41166, // Helper->self, 5.0s cast, ???
    FireTrap = 41250, // JestingJackanapes->self, 4.0s cast, range 8 circle
    BlizzardTrap = 41251, // JestingJackanapes->self, 4.0s cast, range 8 circle
    Expand = 41255, // Tentacle->self, no cast, single-target
    Recharge = 41169, // Boss->self, 4.0s cast, single-target
    WallopFastSmall = 41314, // Tentacle->self, 7.0s cast, range 60 width 10 rect
    WallowSlowSmall = 41256, // Tentacle->self, 11.0s cast, range 60 width 10 rect
    WallopSlowBig = 41257, // Tentacle->self, 11.0s cast, range 60 width 20 rect
    VoidThunderIII = 41172, // Boss->player, 5.0s cast, single-target
    SurpriseAttackCast = 41252, // JestingJackanapes->location, 14.0s cast, single-target
    SurpriseAttackInstant = 41253, // JestingJackanapes->location, no cast, single-target
    SurpriseAttack = 41254, // Helper->location, 12.0s cast, width 6 rect charge
    ArcaneBlastCast = 41171, // Boss->self, 4.0+1.0s cast, single-target
    ArcaneBlast = 41174, // Helper->self, 5.0s cast, ???
}

public enum SID : uint
{
    ElementSelect = 2193, // none->JestingJackanapes, extra=0x344/0x345
    Seduced = 991, // JestingJackanapes->player, extra=0x3C
    PlayingWithFire = 4211, // none->player, extra=0x0
    PlayingWithIce = 4212, // none->player, extra=0x0
    Pyromania = 4213, // none->player, extra=0x0
    Cryomania = 4214, // none->player, extra=0x0
    Expand = 4215, // none->Tentacle, extra=0x1
}

public enum Element
{
    None,
    Fire,
    Ice
}

class DarkII(BossModule module) : Components.StandardAOEs(module, AID.DarkII, new AOEShapeCone(65, 45.Degrees()));
class MindBlast(BossModule module) : Components.RaidwideCast(module, AID.MindBlastCast);
class ArcaneBlast(BossModule module) : Components.RaidwideCast(module, AID.ArcaneBlastCast);
class VoidThunderIII(BossModule module) : Components.SingleTargetCast(module, AID.VoidThunderIII);
class Trap(BossModule module) : Components.GroupedAOEs(module, [AID.FireTrap, AID.BlizzardTrap], new AOEShapeCircle(8));
class WallopBig(BossModule module) : Components.StandardAOEs(module, AID.WallopSlowBig, new AOEShapeRect(60, 10));
class WallopSmall(BossModule module) : Components.GroupedAOEs(module, [AID.WallowSlowSmall, AID.WallopFastSmall], new AOEShapeRect(60, 5));

class ImpCounter(BossModule module) : BossComponent(module)
{

    public class Imp(Actor Actor, Element Element)
    {
        public readonly Actor Actor = Actor;
        public Element Element = Element;
        public WPos Source
        {
            get => Predicted > 0 ? field : Actor.Position;
            set
            {
                Predicted++;
                field = value;
            }
        } = Actor.Position;
        public int Predicted { get; private set; }
    }

    public readonly List<Imp> Imps = [];

    public IEnumerable<Imp> ActiveImps => Imps.Count > 2 ? Imps.Where(i => i.Predicted == 2) : Imps;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.ElementSelect)
        {
            if (status.Extra == 0x344)
                Imps.Add(new(actor, Element.Fire));
            if (status.Extra == 0x345)
                Imps.Add(new(actor, Element.Ice));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.BlizzardTrap or AID.FireTrap)
            Imps.RemoveAll(i => i.Actor == caster);

        if ((AID)spell.Action.ID == AID.SurpriseAttack)
            for (var i = 0; i < Imps.Count; i++)
                if (Imps[i].Source.AlmostEqual(caster.Position, 1))
                    Imps.Ref(i).Source = spell.LocXZ;
    }
}

class Seduction(BossModule module) : Components.Knockback(module)
{
    private readonly ImpCounter _impCounter = module.FindComponent<ImpCounter>()!;

    // forced march distance is about 30y, but the imp AOEs trigger about 1s before the march ends, meaning the distance we actually have to be careful about is about 24-26y depending on player movespeed
    public const float EffectiveMarchDistance = 26;

    record struct PlayerState(Element Element, DateTime Activation);

    private readonly PlayerState[] _playerStates = new PlayerState[PartyState.MaxPartySize];
    private readonly WPos[] _start = new WPos[PartyState.MaxPartySize];

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (slot >= PartyState.MaxPartySize)
            yield break;

        var closest = _impCounter.ActiveImps.Where(i => i.Element == _playerStates[slot].Element).MinBy(i => (actor.Position - i.Source).LengthSq());
        if (closest != null)
            yield return new(closest.Source, EffectiveMarchDistance, Kind: Kind.TowardsOrigin, MinDistance: 1);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        foreach (var src in Sources(slot, actor))
            if (actor.Position.InCircle(src.Origin, EffectiveMarchDistance + 8))
                hints.Add("Move away from imp!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (slot >= PartyState.MaxPartySize)
            return;

        foreach (var i in _impCounter.ActiveImps.Where(i => i.Element == _playerStates[slot].Element))
            hints.AddForbiddenZone(ShapeContains.Circle(i.Source, EffectiveMarchDistance + 8), _playerStates[slot].Activation);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.PlayingWithIce:
                if (Raid.TryFindSlot(actor, out var slot))
                    _playerStates[slot] = new(Element.Ice, status.ExpireAt);
                break;
            case SID.PlayingWithFire:
                if (Raid.TryFindSlot(actor, out var slot2))
                    _playerStates[slot2] = new(Element.Fire, status.ExpireAt);
                break;
            case SID.Seduced:
                if (Raid.TryFindSlot(actor, out var slot3))
                {
                    _playerStates[slot3] = default;
                    _start[slot3] = actor.Position;
                }
                break;
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);

        if (pcSlot >= PartyState.MaxPartySize)
            return;

        foreach (var i in _impCounter.ActiveImps.Where(i => i.Element == _playerStates[pcSlot].Element))
            Arena.AddCircle(i.Source, 8, ArenaColor.Danger);
    }
}

class SurpriseAttack(BossModule module) : Components.ChargeAOEs(module, AID.SurpriseAttack, 3);

class MysteriousMindflayerStates : StateMachineBuilder
{
    public MysteriousMindflayerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ImpCounter>()
            .ActivateOnEnter<DarkII>()
            .ActivateOnEnter<MindBlast>()
            .ActivateOnEnter<ArcaneBlast>()
            .ActivateOnEnter<VoidThunderIII>()
            .ActivateOnEnter<Trap>()
            .ActivateOnEnter<WallopBig>()
            .ActivateOnEnter<WallopSmall>()
            .ActivateOnEnter<Seduction>()
            .ActivateOnEnter<SurpriseAttack>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13646)]
public class MysteriousMindflayer(WorldState ws, Actor primary) : BossModule(ws, primary, new(300, 730), new ArenaBoundsCircle(30))
{
    public override bool DrawAllPlayers => true;
}
