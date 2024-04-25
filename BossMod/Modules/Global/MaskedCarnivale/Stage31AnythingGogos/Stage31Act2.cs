namespace BossMod.Global.MaskedCarnivale.Stage31.Act2;

public enum OID : uint
{
    Boss = 0x30F7, //R=2.0
    Maelstrom = 0x30F9, //R=1.0
    Voidzone = 0x1E9684,
    Helper = 0x233C,
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target
    Teleport = 23108, // Boss->location, no cast, single-target
    GogoFireIII = 23117, // Boss->self, 3,0s cast, range 60 circle, applies pyretic
    GogoBlizzardIII = 23118, // Boss->self, 3,0s cast, range 6+R circle
    GogoThunderIII = 23119, // Boss->location, 3,0s cast, range 6 circle
    GogoFlare = 23128, // Boss->player, 6,0s cast, range 80 circle
    GogoHoly = 23130, // Boss->player, 6,0s cast, range 6 circle, unavoidable, basically a raidwide
    GogoMeteorVisual = 23120, // Boss->self, 3,0s cast, single-target
    GogoMeteorVisual2 = 22023, // Boss->self, no cast, single-target
    GogoMeteor1 = 23121, // Helper->location, 3,0s cast, range 5 circle
    GogoMeteor2 = 23123, // Helper->location, 12,0s cast, range 100 circle, damage fall off AOE, ca. 16 distance seems fine
    GogoMeteor3 = 23131, // Helper->location, 4,0s cast, range 8 circle
    GogoMeteor4 = 23122, // Helper->location, 11,0s cast, range 8 circle
    GogoMeteor5 = 23129, // Helper->location, 7,0s cast, range 100 circle, damage fall off AOE, ca. 16 distance seems fine
    GogoMeteor6 = 23124, // Helper->location, 10,0s cast, range 100 circle, wipe without diamondback
    Charybdis = 20055, // Boss->self, 3,0s cast, single-target
    Charybdis2 = 20056, // Helper->self, 4,0s cast, range 8 circle
    Icestorm = 23126, // Boss->self, 3,0s cast, single-target
    Icestorm2 = 23127, // Helper->self, no cast, range 60 circle, applies frostbite + heavy, should be removed with exuviation
    AetherialPull = 23125, // Maelstrom->self, 1,0s cast, range 8 circle, pull 40 between centers
}

public enum SID : uint
{
    Pyretic = 960, // Boss->player, extra=0x0
    Frostbite = 268, // Helper->player, extra=0x0
    Heavy = 1107, // Helper->player, extra=0x50
}

class Charybdis(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Charybdis), 8);
class Maelstrom(BossModule module) : Components.PersistentVoidzone(module, 8, m => m.Enemies(OID.Maelstrom));
class GogoFlare(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.GogoFlare));
class GogoHoly(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.GogoHoly));
class GogoMeteor1(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.GogoMeteor1), 5);
class GogoMeteor2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.GogoMeteor2), 16);
class GogoMeteor3(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.GogoMeteor3), 8);
class GogoMeteor4(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.GogoMeteor4), 8);
class GogoMeteor5(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.GogoMeteor5), 16);
class GogoMeteorBig(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.GogoMeteor6), "Use Diamondback!");
class Icestorm(BossModule module) : Components.RaidwideCastDelay(module, ActionID.MakeSpell(AID.Icestorm), ActionID.MakeSpell(AID.Icestorm2), 0.9f, "Raidwide + Frostbite + Heavy");
class ThunderIII(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 6, ActionID.MakeSpell(AID.GogoThunderIII), m => m.Enemies(OID.Voidzone).Where(e => e.EventState != 7), 0.8f);

class GogoBlizzardIII(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(8);
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
            yield return new(circle, Module.PrimaryActor.Position, default, _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.GogoFireIII)
            _activation = spell.NPCFinishAt.AddSeconds(5.1f);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.GogoBlizzardIII)
            _activation = default;
    }
}

class GogoFireIIIHint(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.GogoFireIII), "Pyretic, dodge AOE then stop everything!");

class Pyretic(BossModule module) : Components.StayMove(module)
{
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.Pyretic)
        {
            if (Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0 && slot < Requirements.Length)
                Requirements[slot] = Requirement.Stay;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.Pyretic)
        {
            if (Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0 && slot < Requirements.Length)
                Requirements[slot] = Requirement.None;
        }
    }
}

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var debuff = actor.FindStatus(SID.Heavy) != null || actor.FindStatus(SID.Frostbite) != null;
        if (debuff)
            hints.Add($"Cleanse debuffs!");
    }
}

class Stage31Act2States : StateMachineBuilder
{
    public Stage31Act2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Charybdis>()
            .ActivateOnEnter<Maelstrom>()
            .ActivateOnEnter<GogoFlare>()
            .ActivateOnEnter<GogoHoly>()
            .ActivateOnEnter<GogoMeteor1>()
            .ActivateOnEnter<GogoMeteor2>()
            .ActivateOnEnter<GogoMeteor3>()
            .ActivateOnEnter<GogoMeteor4>()
            .ActivateOnEnter<GogoMeteor5>()
            .ActivateOnEnter<GogoMeteorBig>()
            .ActivateOnEnter<ThunderIII>()
            .ActivateOnEnter<Icestorm>()
            .ActivateOnEnter<GogoBlizzardIII>()
            .ActivateOnEnter<GogoFireIIIHint>()
            .ActivateOnEnter<Pyretic>()
            .ActivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 754, NameID = 9908, SortOrder = 2)]
public class Stage31Act2(WorldState ws, Actor primary) : BossModule(ws, primary, new ArenaBoundsCircle(new(100, 100), 16));
