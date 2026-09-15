namespace BossMod.Dawntrail.Foray.FATE.Iambe;

public enum OID : uint
{
    Boss = 0x4C41,
    Helper = 0x233C,
    WinsomeSeed = 0x4C43, // R0.240-0.528, x0 (spawn during fight)
    Iambe = 0x4C42, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 50855, // Boss->player, no cast, single-target
    DirectSeeding = 48029, // Boss->self, 3.0s cast, single-target
    GardenersHymnCast = 48031, // Boss->self, 2.5s cast, single-target
    GardenersHymn = 48032, // 4C42->location, 6.0s cast, range 5 circle
    Burst = 48033, // 4C43->self, 2.0s cast, range 15 circle
    OdeOfTheUnderfoot = 48037, // Boss->self, 5.0s cast, range 10 circle
    IambicMarch = 48035, // Boss->self, 3.0s cast, range 40 circle
}

public enum SID : uint
{
    ForwardMarch = 5142, // Boss->player, extra=0x0
    AboutFace = 5143, // Boss->player, extra=0x0
    ForcedMarch = 1257, // Boss->player, extra=0x2/0x1
    Gen = 5106, // 4C42->4C43, extra=0x1
    Gen1 = 5107, // 4C42->4C43/Boss, extra=0x1
}

class GardenersHymn(BossModule module) : Components.StandardAOEs(module, AID.GardenersHymn, 5);
class OdeOfTheUnderfoot(BossModule module) : Components.StandardAOEs(module, AID.OdeOfTheUnderfoot, 10)
{
    BitMask _marching;

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID is SID.ForwardMarch or SID.AboutFace && Raid.TryFindSlot(actor, out var slot))
            _marching.Set(slot);
    }

    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID is SID.ForwardMarch or SID.AboutFace && Raid.TryFindSlot(actor, out var slot))
            _marching.Clear(slot);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!_marching[slot])
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!_marching[slot])
            base.AddHints(slot, actor, hints);
    }
}

class Burst(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Actor seed, DateTime activation)> seeds = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => seeds.Select(s => new AOEInstance(new AOEShapeCircle(15), s.seed.Position, default, s.activation));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.GardenersHymn)
            seeds.AddRange(Module.Enemies(OID.WinsomeSeed).InRadius(spell.LocXZ, 5).Select(s => (s, Module.CastFinishAt(spell, 3.6f))));

        if ((AID)spell.Action.ID == AID.Burst)
        {
            var i = seeds.FindIndex(s => s.seed == caster);
            if (i >= 0)
                seeds.Ref(i).activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Burst)
            seeds.RemoveAll(s => s.seed == caster);
    }
}

class IambicMarch(BossModule module) : Components.StatusDrivenForcedMarch(module, 2, (uint)SID.ForwardMarch, (uint)SID.AboutFace, default, default)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (Module.PrimaryActor.CastInfo is { } ci && State.TryGetValue(actor.InstanceID, out var state) && ci.IsSpell(AID.OdeOfTheUnderfoot) && state.PendingMoves is [var move, ..])
        {
            var al = new ArcList(actor.Position, 12);
            al.ForbidCircle(ci.LocXZ, 10);
            hints.AddForbiddenDirections(al, move.activation, move.dir);
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => Module.PrimaryActor.CastInfo is { } ci && ci.IsSpell(AID.OdeOfTheUnderfoot) && pos.InCircle(ci.LocXZ, 10);
}

class IambeStates : StateMachineBuilder
{
    public IambeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GardenersHymn>()
            .ActivateOnEnter<OdeOfTheUnderfoot>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<IambicMarch>();
    }
}

[ModuleInfo(Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14765)]
public class Iambe(WorldState ws, Actor primary) : BossModule(ws, primary, new(-175, -500), new ArenaBoundsCircle(40));
