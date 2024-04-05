namespace BossMod.Endwalker.Dungeon.D13LunarSubterrane.D133Durante;

public enum OID : uint
{
    Boss = 0x4042, // R=6.0
    AethericCharge = 0x4043, // R=1.0
    Helper = 0x233C,
};

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    ArcaneEdge = 35010, // Boss->player, 5,0s cast, single-target
    OldMagic = 35011, // Boss->self, 5,0s cast, range 60 circle
    Teleport = 34991, // Boss->location, no cast, single-target
    DuplicitousBatteryTelegraph = 36058, // Helper->location, 3,5s cast, range 5 circle
    DuplicitousBattery = 36057, // Boss->self, 6,0s cast, single-target
    DuplicitousBattery2 = 34994, // Helper->location, no cast, range 5 circle
    ForsakenFount = 35003, // Boss->self, 3,0s cast, single-target
    Explosion = 35006, // 4043->self, 5,0s cast, range 11 circle
    Explosion2 = 35005, // Helper->self, 12,0s cast, range 9 circle
    FallenGrace = 35882, // Helper->player, 5,0s cast, range 6 circle
    Contrapasso = 35905, // Boss->self, 3,0s cast, range 60 circle
    Splinter = 35004, // 4043->self, no cast, single-target
    AntipodalAssaultMarker = 14588, // Helper->player, no cast, single-target
    AntipodalAssault = 35007, // Boss->self, 5,0s cast, single-target, line stack
    AntipodalAssault2 = 35008, // Boss->location, no cast, width 8 rect charge
    HardSlash = 35009, // Boss->self, 5,0s cast, range 50 90-degree cone
    TwilightPhase = 36055, // Boss->self, 6,0s cast, single-target
    TwilightPhaseA = 34997, // Boss->self, no cast, single-target
    TwilightPhaseB = 34998, // Boss->self, no cast, single-target
    TwilightPhase2 = 36056, // Helper->self, 7,3s cast, range 60 width 20 rect
    DarkImpact = 35001, // Boss->location, 7,0s cast, single-target
    DarkImpact2 = 35002, // Helper->self, 8,0s cast, range 25 circle
    DeathsJourney = 34995, // Boss->self, 6,0s cast, range 8 circle
    DeathsJourney2 = 34996, // Helper->self, 6,5s cast, range 30 30-degree cone, this does the damage
    DeathsJourney3 = 35872, // Helper->self, 6,5s cast, range 30 30-degree cone, visual
};

class Voidzone : BossComponent
{
    public override void OnEventEnvControl(BossModule module, byte index, uint state)
    {
        if (state == 0x00020001 && index == 0x0A)
            module.Arena.Bounds = new ArenaBoundsCircle(new(0, -422), 20);
    }
}

class ArcaneEdge : Components.RaidwideCast
{
    public ArcaneEdge() : base(ActionID.MakeSpell(AID.ArcaneEdge)) { }
}

class OldMagic : Components.RaidwideCast
{
    public OldMagic() : base(ActionID.MakeSpell(AID.OldMagic)) { }
}

class Contrapasso : Components.RaidwideCast
{
    public Contrapasso() : base(ActionID.MakeSpell(AID.Contrapasso)) { }
}

class DuplicitousBattery : Components.GenericAOEs
{
    private List<(WPos source, DateTime activation)> _casters = new();
    private static readonly AOEShapeCircle circle = new(5);

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        if (_casters.Count > 0)
            for (int i = 0; i < Math.Clamp(_casters.Count, 1, 16); ++i)
                yield return new(circle, _casters[i].source, activation: _casters[i].activation, color: ArenaColor.Danger);
        if (_casters.Count > 16)
            for (int i = 16; i < Math.Clamp(_casters.Count, 16, 32); ++i)
                yield return new(circle, _casters[i].source, activation: _casters[i].activation, risky: false);
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DuplicitousBatteryTelegraph)
            _casters.Add((spell.LocXZ, module.WorldState.CurrentTime.AddSeconds(6.5f)));

    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if (_casters.Count > 0 && (AID)spell.Action.ID == AID.DuplicitousBattery2)
            _casters.RemoveAt(0);
    }
}

class Explosion : Components.SelfTargetedAOEs
{
    public Explosion() : base(ActionID.MakeSpell(AID.Explosion), new AOEShapeCircle(11)) { }
}

class Explosion2 : Components.SelfTargetedAOEs
{
    public Explosion2() : base(ActionID.MakeSpell(AID.Explosion2), new AOEShapeCircle(9)) { }
}
class FallenGrace : Components.SpreadFromCastTargets
{
    public FallenGrace() : base(ActionID.MakeSpell(AID.FallenGrace), 6) { }
}

// TODO: create and use generic 'line stack' component
class AntipodalAssault : Components.GenericBaitAway
{
    private Actor? target;

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.AntipodalAssaultMarker)
        {
            target = module.WorldState.Actors.Find(spell.MainTargetID);
            CurrentBaits.Add(new(module.PrimaryActor, target!, new AOEShapeRect(50, 4))); // the actual range is not 50, but just a charge of 8 width, but always goes until the edge of the arena, so we can simplify it
        }
        if ((AID)spell.Action.ID == AID.AntipodalAssault2)
            CurrentBaits.Clear();
    }

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (CurrentBaits.Count > 0 && actor != target)
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(module.PrimaryActor.Position, (target!.Position - module.PrimaryActor.Position).Normalized(), 50, 0, 4));
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (CurrentBaits.Count > 0)
        {
            if (!actor.Position.InRect(module.PrimaryActor.Position, (target!.Position - module.PrimaryActor.Position).Normalized(), 50, 0, 4))
                hints.Add("Stack!");
            else
                hints.Add("Stack!", false);
        }
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        foreach (var bait in CurrentBaits)
            bait.Shape.Draw(arena, BaitOrigin(bait), bait.Rotation, ArenaColor.SafeFromAOE);
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena) { }
}

class HardSlash : Components.SelfTargetedAOEs
{
    public HardSlash() : base(ActionID.MakeSpell(AID.HardSlash), new AOEShapeCone(50, 45.Degrees())) { }
}

class TwilightPhase : Components.SelfTargetedAOEs
{
    public TwilightPhase() : base(ActionID.MakeSpell(AID.TwilightPhase2), new AOEShapeRect(30, 10, 30)) { }
}

class DarkImpact : Components.SelfTargetedAOEs
{
    public DarkImpact() : base(ActionID.MakeSpell(AID.DarkImpact2), new AOEShapeCircle(25)) { }
}

class DeathsJourney : Components.SelfTargetedAOEs
{
    public DeathsJourney() : base(ActionID.MakeSpell(AID.DeathsJourney), new AOEShapeCircle(8)) { }
}

class DeathsJourney2 : Components.SelfTargetedAOEs
{
    public DeathsJourney2() : base(ActionID.MakeSpell(AID.DeathsJourney2), new AOEShapeCone(30, 15.Degrees())) { }
}

class D133DuranteStates : StateMachineBuilder
{
    public D133DuranteStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Voidzone>()
            .ActivateOnEnter<OldMagic>()
            .ActivateOnEnter<ArcaneEdge>()
            .ActivateOnEnter<Contrapasso>()
            .ActivateOnEnter<DuplicitousBattery>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<Explosion2>()
            .ActivateOnEnter<FallenGrace>()
            .ActivateOnEnter<AntipodalAssault>()
            .ActivateOnEnter<HardSlash>()
            .ActivateOnEnter<TwilightPhase>()
            .ActivateOnEnter<DarkImpact>()
            .ActivateOnEnter<DeathsJourney>()
            .ActivateOnEnter<DeathsJourney2>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 823, NameID = 12584)]
class D133Durante : BossModule
{
    public D133Durante(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(0, -422), 23)) { }
}
