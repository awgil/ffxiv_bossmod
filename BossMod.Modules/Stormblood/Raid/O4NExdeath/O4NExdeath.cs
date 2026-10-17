namespace BossMod.Stormblood.Raid.O4NExdeath;

public enum OID : uint
{
    Boss = 0x1E77, // R3.800
    Head = 0x1E78, // R4.550
    DeathlyVine = 0x1E79, // R4.200
    Helper = 0x18D6, // R0.500
    BlackHole = 0x1E7A, // R1.000
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    Doom = 9399, // Boss->self, 3.0s cast, range 80
    BlizzardIII = 9403, // Boss->self, 3.5s cast, single-target
    BlizzardIIIPuddle = 9404, // Helper->location, 3.5s cast, range 6 circle
    ThunderIII = 9405, // Boss->self, 5.0s cast, single-target, tankbuster
    ThunderIIISplash = 9406, // Helper->player, no cast, range 5 circle
    FireIII = 9401, // Boss->self, 3.5s cast, single-target
    FireIIISpread = 9402, // Helper->player, 4.0s cast, range 4 circle
    VoidFire = 9407, // Boss->self, 4.0s cast, range 80+R circle
    VoidThunder = 9409, // Boss->self, 5.0s cast, range 11+R circle
    VoidBlizzard = 9408, // Boss->self, 4.0s cast, range 80+R circle
    VacuumWave = 9400, // Boss->self, 4.0s cast, knockback 11
    TheDecisiveBattle = 9224, // Boss->self, 4.0s cast, single-target
    DecisiveMeteor = 9226, // Helper->self, 4.0s cast, range 40+R circle
    Collision = 9418, // Head->self, no cast, range 80+R circle
    ZombieBreath = 9419, // Head->self, 3.0s cast, range 20+R cone
    Clearout = 9420, // DeathlyVine->self, 7.5s cast, outward cone
    Holy = 9412, // Boss->self, 5.0s cast, single-target
    HolyStack = 9413, // Helper->player, 5.0s cast, range 6 circle
    Flare = 9410, // Boss->self, 5.0s cast, single-target
    FlareAOE = 9411, // Helper->player, 5.0s cast, range 80 circle, proximity
    Meteor = 9414, // Boss->self, 7.0s cast, raidwide
    MeteorEnrage = 9415, // Boss->self, 5.0s cast
    BlackHole = 9416, // Boss->self, 4.0s cast, range 80+R circle
    BlackSpark = 9417, // BlackHole->player, no cast, single-target
}

public enum SID : uint
{
    Doom = 910, // Boss->player, extra=0x0
    Pyretic = 960, // Boss->player, extra=0x0
    DeepFreeze = 3480, // Boss->player, extra=0x0
    VulnerabilityUp = 202, // ->player, extra=0x1/0x2
    LightningResistanceDown = 898, // Helper->player, extra=0x1/0x2
}

public enum IconID : uint
{
    Holy = 62, // player
    Flare = 87, // player
}

class Doom(BossModule module) : BossComponent(module)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!actor.Class.CanEsuna())
            return;
        foreach (var member in Raid.WithoutSlot())
            if (!member.IsDead && member.FindStatus(SID.Doom) != null && member.PendingDispels.Count == 0)
                hints.Add($"Cleanse {member.Name}!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var (partySlot, member) in Raid.WithSlot())
            if (!member.IsDead && member.FindStatus(SID.Doom) != null)
                hints.ShouldCleanse.Set(partySlot);
    }
}

class BlizzardIII(BossModule module) : Components.StandardAOEs(module, AID.BlizzardIIIPuddle, 6);
class FireIII(BossModule module) : Components.SpreadFromCastTargets(module, AID.FireIIISpread, 4);
class ThunderIII(BossModule module) : Components.SingleTargetCast(module, AID.ThunderIII);

class ThunderIIISplash(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle Shape = new(5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Module.PrimaryActor.CastInfo is not { } cast || !cast.IsSpell(AID.ThunderIII))
            yield break;
        var target = WorldState.Actors.Find(Module.PrimaryActor.TargetID);
        if (target != null && target != actor)
            yield return new(Shape, target.Position, Activation: Module.CastFinishAt(cast));
    }
}

class VoidThunder(BossModule module) : Components.StandardAOEs(module, AID.VoidThunder, 14.8f); // 11 + boss radius

class VoidElements(BossModule module) : Components.StayMove(module)
{
    private DateTime _moveUntil;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.IsSpell(AID.VoidBlizzard))
        {
            Array.Fill(PlayerStates, new(Requirement.Move, Module.CastFinishAt(spell)));
            _moveUntil = Module.CastFinishAt(spell, 2);
        }
        else if (spell.IsSpell(AID.VoidFire))
            Array.Fill(PlayerStates, new(Requirement.Stay, Module.CastFinishAt(spell, 3)));
    }

    public override void Update()
    {
        if (_moveUntil == default || WorldState.CurrentTime < _moveUntil)
            return;
        _moveUntil = default;
        for (var i = 0; i < PlayerStates.Length; ++i)
            if (PlayerStates[i].Requirement == Requirement.Move)
                PlayerStates[i] = default;
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.Pyretic)
            SetState(Raid.FindSlot(actor.InstanceID), new(Requirement.Stay, status.ExpireAt, 1));
        else if ((SID)status.ID == SID.DeepFreeze)
            ClearState(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.Pyretic)
            ClearState(Raid.FindSlot(actor.InstanceID), 1);
    }
}

class VacuumWave(BossModule module) : Components.KnockbackFromCastTarget(module, AID.VacuumWave, 11);
class Holy(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Holy, AID.HolyStack, 6, 5.1f);

class Flare(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(12), (uint)IconID.Flare, AID.FlareAOE, 5.1f, true)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (ActiveBaitsOn(actor).Any() && (actor.Position - Arena.Center).LengthSq() < 17.5f * 17.5f)
            hints.Add("Bait to the edge!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        foreach (var b in ActiveBaitsOn(actor))
            hints.AddForbiddenZone(ShapeDistance.Circle(Arena.Center, 17.5f), b.Activation);
    }
}

class Meteor(BossModule module) : Components.RaidwideCast(module, AID.Meteor);

class Clearout(BossModule module) : Components.StandardAOEs(module, AID.Clearout, new AOEShapeCone(24.2f, 60.Degrees()));
class MeteorEnrage(BossModule module) : Components.RaidwideCast(module, AID.MeteorEnrage, "Enrage");
class BlackHoles(BossModule module) : Components.Voidzone(module, 4, (uint)OID.BlackHole);

class DecisiveBattle(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle Meteor = new(16);
    private static readonly AOEShapeCircle HeadHazard = new(6);
    private static readonly AOEShapeCone Breath = new(24.5f, 60.Degrees());
    private AOEInstance? _meteor;
    private AOEInstance? _head;
    private AOEInstance? _breath;
    private int _count;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_meteor is { } meteor)
            yield return meteor;
        if (_head is { } head)
            yield return head;
        if (_breath is { } breath)
            yield return breath;
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_count >= 2 && (_meteor != null || Module.PrimaryActor.CastInfo?.IsSpell(AID.TheDecisiveBattle) == true))
            hints.Add("Stand in the vine gap");
        else if (_meteor != null)
            hints.Add("Go to the edge");
        else if (_breath != null)
            hints.Add("Behind the head");
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.IsSpell(AID.TheDecisiveBattle))
            ++_count;
        else if (spell.IsSpell(AID.DecisiveMeteor))
            _meteor = new(Meteor, Arena.Center, Activation: Module.CastFinishAt(spell));
        else if (spell.IsSpell(AID.ZombieBreath))
            _breath = new(Breath, caster.Position, spell.Rotation, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.DecisiveMeteor)
            _meteor = null;
        else if (spell.Action.ID == (uint)AID.Collision)
            _head = new(HeadHazard, Arena.Center, Activation: WorldState.FutureTime(5));
        else if (spell.Action.ID == (uint)AID.ZombieBreath)
        {
            _breath = null;
            _head = null;
            ++NumCasts;
        }
    }
}

class O4NExdeathStates : StateMachineBuilder
{
    public O4NExdeathStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Doom>()
            .ActivateOnEnter<BlizzardIII>()
            .ActivateOnEnter<FireIII>()
            .ActivateOnEnter<ThunderIII>()
            .ActivateOnEnter<ThunderIIISplash>()
            .ActivateOnEnter<VoidThunder>()
            .ActivateOnEnter<VoidElements>()
            .ActivateOnEnter<VacuumWave>()
            .ActivateOnEnter<Holy>()
            .ActivateOnEnter<Flare>()
            .ActivateOnEnter<Meteor>()
            .ActivateOnEnter<MeteorEnrage>()
            .ActivateOnEnter<Clearout>()
            .ActivateOnEnter<BlackHoles>()
            .ActivateOnEnter<DecisiveBattle>();
    }
}

[ModuleInfo(Contributors = "croizat", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 255, NameID = 6052)]
public class O4NExdeath(ModuleInit init) : BossModule(init, new(0, 0), new ArenaBoundsCircle(20));
