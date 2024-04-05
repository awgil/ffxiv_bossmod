namespace BossMod.Global.MaskedCarnivale.Stage26.Act2;

public enum OID : uint
{
    Boss = 0x2C58, //R=3.6
    Thunderhead = 0x2C59, //R=1.0
    Helper = 0x233C,
};

public enum AID : uint
{
    AutoAttack = 6499, // 2C58->player, no cast, single-target
    RawInstinct = 18604, // 2C58->self, 3,0s cast, single-target
    BodyBlow = 18601, // 2C58->player, 4,0s cast, single-target
    VoidThunderII = 18602, // 2C58->location, 3,0s cast, range 4 circle
    LightningBolt = 18606, // 2C59->self, no cast, range 8 circle
    DadJoke = 18605, // 2C58->self, no cast, range 25+R 120-degree cone, knockback 15, dir forward
    VoidThunderIII = 18603, // 2C58->player, 4,0s cast, range 20 circle
};

public enum SID : uint
{
    CriticalStrikes = 1797, // Boss->Boss, extra=0x0
    Electrocution = 271, // Boss/2C59->player, extra=0x0
};

public enum IconID : uint
{
    BaitKnockback = 23, // player
};

class Thunderhead : Components.PersistentVoidzone
{
    public Thunderhead() : base(8, m => m.Enemies(OID.Thunderhead)) { }
}

class DadJoke : Components.Knockback
{
    private DateTime _activation;

    public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
    {
        if (_activation != default)
            yield return new(module.PrimaryActor.Position, 15, _activation, Direction: Angle.FromDirection(actor.Position - module.PrimaryActor.Position), Kind: Kind.DirForward);
    }

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.BaitKnockback)
            _activation = module.WorldState.CurrentTime.AddSeconds(5);
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.DadJoke)
            _activation = default;
    }

    public override bool DestinationUnsafe(BossModule module, int slot, Actor actor, WPos pos)
    {
        if (module.FindComponent<Thunderhead>() != null && module.FindComponent<Thunderhead>()!.ActiveAOEs(module, slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)))
            return true;
        if (!module.Bounds.Contains(pos))
            return true;
        else
            return false;
    }
}

class VoidThunderII : Components.LocationTargetedAOEs
{
    public VoidThunderII() : base(ActionID.MakeSpell(AID.VoidThunderII), 4) { }
}

class RawInstinct : Components.CastHint
{
    public RawInstinct() : base(ActionID.MakeSpell(AID.RawInstinct), "Prepare to dispel buff") { }
}

class VoidThunderIII : Components.RaidwideCast
{
    public VoidThunderIII() : base(ActionID.MakeSpell(AID.VoidThunderIII), "Raidwide + Electrocution") { }
}

class BodyBlow : Components.SingleTargetCast
{
    public BodyBlow() : base(ActionID.MakeSpell(AID.BodyBlow), "Soft Tankbuster") { }
}

class Hints : BossComponent
{
    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        hints.Add($"{module.PrimaryActor.Name} will cast Raw Instinct, which causes all his hits to crit.\nUse Eerie Soundwave to dispel it.\n{module.PrimaryActor.Name} is weak against earth and strong against lightning attacks.");
    }
}

class Hints2 : BossComponent
{
    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        var critbuff = module.Enemies(OID.Boss).Where(x => x.FindStatus(SID.CriticalStrikes) != null).FirstOrDefault();
        if (critbuff != null)
            hints.Add($"Dispel {module.PrimaryActor.Name} with Eerie Soundwave!");
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
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
    public Stage26Act2(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 16))
    {
        ActivateComponent<Hints>();
    }
}
