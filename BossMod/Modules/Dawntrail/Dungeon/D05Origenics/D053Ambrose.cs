namespace BossMod.Dawntrail.Dungeon.D05Origenics.D053Ambrose;

public enum OID : uint
{
    Boss = 0x417D, // R4.998, x1
    OrigenicsEyeborg = 0x417E, // R4.000, x3
    Superfluity = 0x417F, // R1.800, x6
    Electrolance = 0x4180, // R1.380, x1
    Helper = 0x233C, // R0.500, x53, Helper type
}

public enum AID : uint
{
    AutoAttackBoss = 870, // Boss->player, no cast, single-target
    AutoAttackAdd = 872, // Superfluity/OrigenicsEyeborg->player, no cast, single-target
    Teleport = 36439, // Boss->location, no cast, single-target
    PsychicWave = 36436, // Boss->self, 5.0s cast, range 80 circle, raidwide
    VoltaicSlash = 36437, // Boss->player, 5.0s cast, single-target, tankbuster
    OverwhelmingChargeSolo = 39233, // Boss->self, 5.0s cast, range 26 180-degree cone
    PsychokinesisDoors = 36427, // Boss->self, 10.0s cast, single-target, visual (door aoes)
    PsychokinesisDoorsAOE = 36428, // Helper->self, 10.0s cast, range 70 width 13 rect
    ExtrasensoryField = 36432, // Boss->self, 7.0s cast, single-target, visual (square knockbacks)
    ExtrasensoryExpulsionAOEV = 36433, // Helper->self, 7.0s cast, range 20 width 15 rect, knock forward 20
    ExtrasensoryExpulsionAOEH = 36434, // Helper->self, 7.0s cast, range 15 width 20 rect, knock forward 20
    PsychokineticCharge = 39055, // Boss->self, 7.0s cast, single-target, visual (knockbacks + cleave)
    OverwhelmingChargeKnockback = 36435, // Boss->self, no cast, single-target, visual (cleave after knockback)
    OverwhelmingChargeKnockbackAOE = 39072, // Helper->self, 9.8s cast, range 26 180-degree cone
    Electrolance = 36429, // Boss->location, 6.0s cast, range 22 circle
    PsychokinesisLance = 38929, // Boss->self, 8.0s cast, single-target, visual (line aoes)
    PsychokinesisLanceVisual = 38953, // Helper->location, 2.5s cast, width 10 rect charge
    PsychokinesisLanceRush = 38954, // Electrolance->location, no cast, width 10 rect charge
    ElectrolanceAssimilation = 36430, // Boss->self, 0.5s cast, single-target, visual (final lance charge)
    ElectrolanceAssimilationAOE = 36431, // Helper->self, 1.0s cast, range 33 width 10 rect
    WhorlOfTheMind = 36438, // Helper->player, 5.0s cast, range 5 circle spread
}

public enum IconID : uint
{
    VoltaicSlash = 218, // player
    WhorlOfTheMind = 376, // player
}

public enum TetherID : uint
{
    Psychokinesis = 266, // Boss->Electrolance
}

class PsychicWave(BossModule module) : Components.RaidwideCast(module, AID.PsychicWave);
class VoltaicSlash(BossModule module) : Components.SingleTargetCast(module, AID.VoltaicSlash);
class OverwhelmingChargeSolo(BossModule module) : Components.StandardAOEs(module, AID.OverwhelmingChargeSolo, new AOEShapeCone(26, 90.Degrees()));
class PsychokinesisDoors(BossModule module) : Components.StandardAOEs(module, AID.PsychokinesisDoorsAOE, new AOEShapeRect(70, 6.5f));
class Adds(BossModule module) : Components.AddsMulti(module, [OID.OrigenicsEyeborg, OID.Superfluity]);

class ExtrasensoryExpulsion(BossModule module) : Components.Knockback(module)
{
    public readonly List<Source> AOEs = [];
    private WDir _cleaveDir;

    private static readonly AOEShapeRect _shape = new(10, 10, 10);

    public override IEnumerable<Source> Sources(int slot, Actor actor) => AOEs;
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => !Module.InBounds(pos) || _cleaveDir.Dot(pos - Module.Center) > 0;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var s in AOEs)
            if (DestinationUnsafe(slot, actor, s.Origin + s.Distance * s.Direction.ToDirection()))
                hints.AddForbiddenZone(ShapeContains.Rect(s.Origin, s.Direction, 15, 15, 15), s.Activation); // note: bigger than actual aoe, to prevent ai from standing too close to the border and being knocked across safe tile
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var s in AOEs)
            if (!DestinationUnsafe(pcSlot, pc, s.Origin + s.Distance * s.Direction.ToDirection()))
                _shape.Draw(Arena, s.Origin, s.Direction, ArenaColor.SafeFromAOE);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ExtrasensoryExpulsionAOEV:
            case AID.ExtrasensoryExpulsionAOEH:
                var origin = Module.Center + new WDir(caster.Position.X < Module.Center.X ? -10 : +10, caster.Position.Z < Module.Center.Z ? -10 : +10);
                AOEs.Add(new(origin, 20, Module.CastFinishAt(spell), _shape, spell.Rotation, Kind.DirForward));
                break;
            case AID.OverwhelmingChargeKnockbackAOE:
                _cleaveDir = spell.Rotation.ToDirection();
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ExtrasensoryExpulsionAOEV or AID.ExtrasensoryExpulsionAOEH)
        {
            AOEs.Clear();
            _cleaveDir = default;
        }
    }
}

class OverwhelmingChargeKnockback(BossModule module) : Components.StandardAOEs(module, AID.OverwhelmingChargeKnockbackAOE, new AOEShapeCone(26, 90.Degrees()))
{
    private readonly ExtrasensoryExpulsion? _knockback = module.FindComponent<ExtrasensoryExpulsion>();

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _knockback?.AOEs.Count > 0 ? [] : base.ActiveAOEs(slot, actor);
}

class Electrolance(BossModule module) : Components.StandardAOEs(module, AID.Electrolance, 22);

class PsychokinesisLance(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.PsychokinesisLanceVisual)
        {
            var dir = spell.LocXZ - caster.Position;
            _aoes.Add(new(new AOEShapeRect(dir.Length(), 5), caster.Position, Angle.FromDirection(dir), _aoes.Count == 0 ? Module.CastFinishAt(spell, 5.1f) : _aoes[^1].Activation.AddSeconds(0.8f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.PsychokinesisLanceRush or AID.ElectrolanceAssimilationAOE && _aoes.Count > 0)
            _aoes.RemoveAt(0);
    }
}

class WhorlOfTheMind(BossModule module) : Components.SpreadFromCastTargets(module, AID.WhorlOfTheMind, 5);

class D053AmbroseStates : StateMachineBuilder
{
    public D053AmbroseStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PsychicWave>()
            .ActivateOnEnter<VoltaicSlash>()
            .ActivateOnEnter<OverwhelmingChargeSolo>()
            .ActivateOnEnter<PsychokinesisDoors>()
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<ExtrasensoryExpulsion>()
            .ActivateOnEnter<OverwhelmingChargeKnockback>()
            .ActivateOnEnter<Electrolance>()
            .ActivateOnEnter<PsychokinesisLance>()
            .ActivateOnEnter<WhorlOfTheMind>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 825, NameID = 12695)]
public class D053Ambrose(WorldState ws, Actor primary) : BossModule(ws, primary, new(190, 0), new ArenaBoundsRect(15, 19.5f));
