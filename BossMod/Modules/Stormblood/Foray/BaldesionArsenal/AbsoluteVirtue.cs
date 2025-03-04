namespace BossMod.Stormblood.Foray.BaldesionArsenal.AbsoluteVirtue;

public enum OID : uint
{
    Boss = 0x25DC, // R5.400, x1
    Helper = 0x2628, // R0.500, x18
    RelativeVirtue = 0x25DD, // R5.400, x3
    DarkAurora = 0x25E0, // R1.000, x0 (spawn during fight)
    BrightAurora = 0x25DF, // R1.000, x0 (spawn during fight)
    DarkHole = 0x1EAA49,
    BrightHole = 0x1EAA4A,
}

public enum AID : uint
{
    AutoAttack = 14532, // Boss->player, no cast, single-target
    Meteor = 14233, // Boss->self, 4.0s cast, range 60 circle
    Eidos = 14214, // Boss->self, 2.0s cast, single-target, changes aspect (light/dark)
    AstralRays = 14221, // Helper->self, 8.0s cast, range 8 circle
    UmbralRays = 14222, // Helper->self, 8.0s cast, range 8 circle
    HostileAspect = 14219, // Boss->self, 8.0s cast, single-target
    MedusaJavelin = 14235, // Boss->self, 3.0s cast, range 60+R 90-degree cone
    BrightAurora = 14217, // Helper->self, 3.0s cast, range 30 width 100 rect
    DarkAurora = 14218, // Helper->self, 3.0s cast, range 30 width 100 rect
    ImpactStream = 14216, // Boss->self, 3.0s cast, single-target
    AuroralWind = 14234, // Boss->players, 5.0s cast, range 5 circle
    TurbulentAether = 14224, // Boss->self, 3.0s cast, single-target
    BrightExplosion = 14225, // BrightAurora->self, no cast, range 6 circle
    DarkExplosion = 14226, // DarkAurora->self, no cast, range 6 circle
}

public enum SID : uint
{
    AstralEssence = 1710, // Boss->Helper/Boss, extra=0x0
    UmbralEssence = 1711, // Boss->Helper/Boss, extra=0x0
    AlteredStates = 1387, // none->Helper, extra=0x46
    UmbralCloak = 1713, // none->player, extra=0x0
    AstralCloak = 1712, // none->player, extra=0x0
}

public enum TetherID : uint
{
    DarkTether = 1, // DarkAurora->player
    BrightTether = 2, // BrightAurora->player
}

class Meteor(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Meteor));
class AuroralWind(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.AuroralWind), new AOEShapeCircle(5), centerAtTarget: true, endsOnCastEvent: true);
class MedusaJavelin(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MedusaJavelin), new AOEShapeCone(65, 45.Degrees()));

class Aurora(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> Casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Casters.Select(c => new AOEInstance(new AOEShapeRect(30, 50), c.Position, c.Rotation, Module.CastFinishAt(c.CastInfo)));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.BrightAurora:
                if (caster.FindStatus(SID.AstralEssence) != null)
                    Casters.Add(caster);
                break;
            case AID.DarkAurora:
                if (caster.FindStatus(SID.UmbralEssence) != null)
                    Casters.Add(caster);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.BrightAurora or AID.DarkAurora)
            Casters.Remove(caster);
    }
}
class Rays(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Actor Actor, bool Big)> Casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Casters.Select(c => new AOEInstance(new AOEShapeCircle(c.Big ? 15 : 8), c.Actor.Position, default, Module.CastFinishAt(c.Actor.CastInfo)));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.UmbralRays:
                Casters.Add((caster, caster.FindStatus(SID.UmbralEssence) != null));
                break;
            case AID.AstralRays:
                Casters.Add((caster, caster.FindStatus(SID.AstralEssence) != null));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.UmbralRays or AID.AstralRays)
            Casters.RemoveAll(c => c.Actor == caster);
    }
}

class Balls(BossModule module) : BossComponent(module)
{
    // EObjAnim 00040008 when hole disappears

    private readonly List<(Actor Source, Actor Target, uint Color)> Tethers = [];

    private const uint DarkColor = 0xFFB3198F;
    private const uint LightColor = 0xFFB087E6;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID is TetherID.DarkTether or TetherID.BrightTether)
        {
            var actor = WorldState.Actors.Find(tether.Target);
            if (actor != null)
                Tethers.Add((source, actor, tether.ID == (uint)TetherID.DarkTether ? DarkColor : LightColor));
        }
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        Tethers.RemoveAll(t => t.Source == source);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var (src, tar, col) in Tethers)
            Arena.AddLine(src.Position, tar.Position, col);

        foreach (var h in Module.Enemies(OID.DarkHole))
            Arena.AddCircle(h.Position, 1.5f, DarkColor);
        foreach (var h in Module.Enemies(OID.BrightHole))
            Arena.AddCircle(h.Position, 1.5f, LightColor);
    }
}

class AIPreposition(BossModule module) : BossComponent(module)
{
    enum Mechanic
    {
        None,
        Javelin, // stay close to boss, cone is very large
        Aurora, // stay on boss's vertical axis, room is split
    }

    private Mechanic NextMechanic = Mechanic.None;

    private void StartMechanic(Mechanic m)
    {
        if (NextMechanic == m)
            NextMechanic = Mechanic.None;
    }

    private int HostileAspectCounter;
    private int EidosCounter;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.HostileAspect:
                HostileAspectCounter++;
                if (HostileAspectCounter == 1)
                    NextMechanic = Mechanic.Javelin;
                break;
            case AID.Eidos:
                EidosCounter++;
                if (EidosCounter == 2)
                    NextMechanic = Mechanic.Aurora;
                break;
            case AID.TurbulentAether:
                NextMechanic = Mechanic.Javelin;
                break;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        switch (NextMechanic)
        {
            case Mechanic.Javelin:
                hints.AddForbiddenZone(ShapeDistance.Donut(Module.PrimaryActor.Position, 6, 100), DateTime.MaxValue);
                break;
            case Mechanic.Aurora:
                hints.AddForbiddenZone(ShapeDistance.InvertedRect(Module.PrimaryActor.Position, 90.Degrees(), 50, 50, 3), DateTime.MaxValue);
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.MedusaJavelin:
                StartMechanic(Mechanic.Javelin);
                break;
            case AID.BrightAurora:
            case AID.DarkAurora:
                StartMechanic(Mechanic.Aurora);
                break;
        }
    }

#if DEBUG
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        hints.Add($"Prepositioning for mechanic: {NextMechanic}", false);
    }
#endif
}

class AbsoluteVirtueStates : StateMachineBuilder
{
    public AbsoluteVirtueStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Meteor>()
            .ActivateOnEnter<Rays>()
            .ActivateOnEnter<MedusaJavelin>()
            .ActivateOnEnter<AuroralWind>()
            .ActivateOnEnter<Aurora>()
            .ActivateOnEnter<AIPreposition>()
            .ActivateOnEnter<Balls>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 639, NameID = 7976)]
public class AbsoluteVirtue(WorldState ws, Actor primary) : BossModule(ws, primary, new(-175, 314), new ArenaBoundsCircle(28));

