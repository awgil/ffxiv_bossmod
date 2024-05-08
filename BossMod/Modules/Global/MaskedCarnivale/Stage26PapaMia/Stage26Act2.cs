namespace BossMod.Global.MaskedCarnivale.Stage26.Act2;

public enum OID : uint
{
    Boss = 0x2C58, //R=3.6
    Thunderhead = 0x2C59, //R=1.0
    Helper = 0x233C,
}

public enum AID : uint
{
    AutoAttack = 6499, // 2C58->player, no cast, single-target
    RawInstinct = 18604, // 2C58->self, 3.0s cast, single-target
    BodyBlow = 18601, // 2C58->player, 4.0s cast, single-target
    VoidThunderII = 18602, // 2C58->location, 3.0s cast, range 4 circle
    LightningBolt = 18606, // 2C59->self, no cast, range 8 circle
    DadJoke = 18605, // 2C58->self, no cast, range 25+R 120-degree cone, knockback 15, dir forward
    VoidThunderIII = 18603, // 2C58->player, 4.0s cast, range 20 circle
}

public enum SID : uint
{
    CriticalStrikes = 1797, // Boss->Boss, extra=0x0
    Electrocution = 271, // Boss/2C59->player, extra=0x0
}

public enum IconID : uint
{
    BaitKnockback = 23, // player
}

class Thunderhead(BossModule module) : Components.PersistentVoidzone(module, 8, m => m.Enemies(OID.Thunderhead));

class DadJoke(BossModule module) : Components.Knockback(module)
{
    private DateTime _activation;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_activation != default)
            yield return new(Module.PrimaryActor.Position, 15, _activation, Direction: Angle.FromDirection(actor.Position - Module.PrimaryActor.Position), Kind: Kind.DirForward);
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.BaitKnockback)
            _activation = WorldState.FutureTime(5);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.DadJoke)
            _activation = default;
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        if (Module.FindComponent<Thunderhead>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false)
            return true;
        if (!Module.InBounds(pos))
            return true;
        else
            return false;
    }
}

class VoidThunderII(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.VoidThunderII), 4);
class RawInstinct(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.RawInstinct), "Prepare to dispel buff");
class VoidThunderIII(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.VoidThunderIII), "Raidwide + Electrocution");
class BodyBlow(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.BodyBlow), "Soft Tankbuster");

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"{Module.PrimaryActor.Name} will cast Raw Instinct, which causes all his hits to crit.\nUse Eerie Soundwave to dispel it.\n{Module.PrimaryActor.Name} is weak against earth and strong against lightning attacks.");
    }
}

class Hints2(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        var critbuff = Module.Enemies(OID.Boss).FirstOrDefault(x => x.FindStatus(SID.CriticalStrikes) != null);
        if (critbuff != null)
            hints.Add($"Dispel {Module.PrimaryActor.Name} with Eerie Soundwave!");
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var electrocution = actor.FindStatus(SID.Electrocution);
        if (electrocution != null)
            hints.Add("Electrocution on you! Cleanse it with Exuviation.");
    }
}

class Stage26Act2States : StateMachineBuilder
{
    public Stage26Act2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RawInstinct>()
            .ActivateOnEnter<VoidThunderII>()
            .ActivateOnEnter<VoidThunderIII>()
            .ActivateOnEnter<BodyBlow>()
            .ActivateOnEnter<DadJoke>()
            .ActivateOnEnter<Thunderhead>()
            .ActivateOnEnter<Hints2>()
            .DeactivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 695, NameID = 9231, SortOrder = 2)]
public class Stage26Act2 : BossModule
{
    public Stage26Act2(WorldState ws, Actor primary) : base(ws, primary, new(100, 100), new ArenaBoundsCircle(16))
    {
        ActivateComponent<Hints>();
    }
}
