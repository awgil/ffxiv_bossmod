namespace BossMod.Global.Quest.FF15Collab.Iseultalon;

public enum OID : uint
{
    Boss = 0x252C, //R=5.0
    Helper = 0x25B4,
    Helper2 = 0x2523,
    Helper3 = 0x260C,
    Helper4 = 0x25B3,
    Helper5 = 0x273D,
    Noctis = 0x2650,
}

public enum AID : uint
{
    AutoAttack2 = 872, // Boss->player, no cast, single-target
    Stomp = 14605, // Boss->location, 3,0s cast, range 80 circle
    DeathRay = 14602, // Boss->self, 4,0s cast, single-target
    DeathRay2 = 14603, // 273D->self, 4,0s cast, range 40 width 2 rect
    DeathRay3 = 14604, // Boss->self, no cast, range 40 width 22 rect
    NeedleShot = 14606, // Boss->Noctis, 5,0s cast, range 5 circle
    Thunderbolt = 15082, // Boss->self, 5,0s cast, single-target
    Thunderbolt2 = 15083, // 260C->player/Noctis, no cast, range 5 circle
    unknown = 14608, // Boss->self, no cast, single-target
    Electrocution = 14609, // 25B3->location, 10,0s cast, range 3 circle, tower
    Electrocution2 = 14981, // 25B3->location, 10,0s cast, range 3 circle, tower
    FatalCurrent = 14610, // Helper/Helper2->self, 2,0s cast, range 80 circle, tower fail
    TailWhip = 14607, // Boss->self, 3,0s cast, range 12 270-degree cone
};

public enum IconID : uint
{
    stack = 93, // 2650
    spread = 129, // player/2650
};

class Thunderbolt : Components.UniformStackSpread
{
    public Thunderbolt() : base(0, 5, alwaysShowSpreads: true) { }
    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.spread)
            AddSpread(actor);
    }
    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Thunderbolt2)
            Spreads.Clear();
    }
}

class Electrocution : Components.GenericTowers
{ //Noctis always goes to soak this tower, except on first cast as a tutorial
    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Electrocution)
        {
            if (NumCasts > 0)
                Towers.Add(new(DeterminePosition(module, caster, spell), 3, forbiddenSoakers: module.Raid.WithSlot(true).WhereActor(p => p.InstanceID == module.Raid.Player()!.InstanceID).Mask()));
            if (NumCasts == 0)
                Towers.Add(new(DeterminePosition(module, caster, spell), 3));
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Electrocution or AID.FatalCurrent)
        {
            Towers.RemoveAll(t => t.Position.AlmostEqual(DeterminePosition(module, caster, spell), 1));
            ++NumCasts;
        }
    }

    private WPos DeterminePosition(BossModule module, Actor caster, ActorCastInfo spell) => spell.TargetID == caster.InstanceID ? caster.Position : module.WorldState.Actors.Find(spell.TargetID)?.Position ?? spell.LocXZ;

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Towers.Count > 0 && NumCasts == 0) // Noctis ignores the first tower as a tutorial
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Towers[0].Position, 3));
    }
}

class Electrocution2 : Components.CastTowers
{
    public Electrocution2() : base(ActionID.MakeSpell(AID.Electrocution2), 3) { }
    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Towers.Count > 0)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Towers[0].Position, 3));
    }
}

class Stomp : Components.RaidwideCast
{
    public Stomp() : base(ActionID.MakeSpell(AID.Stomp)) { }
}

class DeathRay : Components.SelfTargetedAOEs
{
    public DeathRay() : base(ActionID.MakeSpell(AID.DeathRay2), new AOEShapeRect(40, 1))
    {
        Color = ArenaColor.Danger;
    }
}

class TailWhip : Components.SelfTargetedAOEs
{
    public TailWhip() : base(ActionID.MakeSpell(AID.TailWhip), new AOEShapeCone(12, 135.Degrees())) { }
}

class DeathRay2 : Components.GenericAOEs
{
    private static readonly AOEShapeRect rect = new(40, 11);
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        if (_activation != default)
            yield return new(rect, module.PrimaryActor.Position, module.PrimaryActor.Rotation, _activation);
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.DeathRay)
            _activation = module.WorldState.CurrentTime.AddSeconds(6.1f);
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.DeathRay3)
            _activation = default;
    }
}

class NeedleShot : Components.StackWithCastTargets
{
    public NeedleShot() : base(ActionID.MakeSpell(AID.NeedleShot), 5) { }
}

class IseultalonStates : StateMachineBuilder
{
    public IseultalonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Thunderbolt>()
            .ActivateOnEnter<Electrocution>()
            .ActivateOnEnter<Electrocution2>()
            .ActivateOnEnter<DeathRay>()
            .ActivateOnEnter<DeathRay2>()
            .ActivateOnEnter<Stomp>()
            .ActivateOnEnter<NeedleShot>()
            .ActivateOnEnter<TailWhip>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68695, NameID = 7895)]
public class Iseultalon : BossModule
{
    public Iseultalon(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(-289, -30), 25)) { } //note the arena is actually a 6 sided polygon
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.Noctis))
            Arena.Actor(s, ArenaColor.Vulnerable);
    }
}
