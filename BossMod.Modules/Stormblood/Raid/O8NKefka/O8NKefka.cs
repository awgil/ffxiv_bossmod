namespace BossMod.Stormblood.Raid.O8NKefka;

public enum OID : uint
{
    Boss = 0x2153, // R2.700
    Helper = 0x18D6, // R0.500
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Hyperdrive = 10542, // Boss->player, 4.0s cast, single-target
    BlizzardBlitz = 10519, // Boss->self, 5.0s cast, single-target
    BlizzardBlitzOut = 10520, // Helper->self, 5.0s cast, range 10 circle
    BlizzardBlitzIn = 10521, // Helper->self, 5.0s cast
    BlizzardBlitzFakeNear = 10516, // Helper->self, 5.0s cast
    BlizzardBlitzFakeDonut = 10518, // Helper->self, 5.0s cast
    BlizzardBlitzDonut = 10517, // Helper->self, 5.0s cast, range 40 circle
    BlizzardBlitzFakeCast = 10515, // Helper->self, 5.0s cast, range 10 circle
    FlagrantFire = 10526, // Boss->self, 4.2s cast, single-target
    FlagrantFireSpread = 10527, // Helper->player, 5.2s cast, range 5 circle
    FlagrantFireStack = 10528, // Helper->player, 5.2s cast, range 6 circle
    ThrummingThunder = 10522, // Boss->self, 5.0s cast, single-target
    ThrummingThunderReal = 10525, // Helper->self, 5.0s cast, range 40+R width 10 rect
    ThrummingThunderFake = 10523, // Helper->self, 5.0s cast, range 40+R width 10 rect
    ThrummingThunderFakeMirror = 10524, // Helper->self, 5.0s cast, range 40+R width 10 rect
    GravenImage = 10533, // Boss->self, 5.0s cast, single-target
    WaveCannon = 10536, // Graven Image->self, 3.0s cast, range 70+R width 6 rect
    Shockwave = 10535, // Graven Image->self, 13.0s cast, range 100+R width 40 rect
    UltimaUpsurge = 10541, // Boss->self, 4.0s cast, range 100 circle
    TimelyTeleport = 10529, // Boss->self, 4.0s cast, single-target
    TimelyTeleportCircle = 10530, // Helper->self, 4.0s cast, range 6 circle
    RevoltingRuin = 10531, // Boss->self, no cast, range 100+R 90-degree cone from the destination
    AeroAssault = 10532, // Boss->self, 3.0s cast, range 100 circle, knockback 10
    IntemperateWill = 10538, // Graven Image->self, 6.0s cast, east half
    GravitationalWave = 10537, // Graven Image->self, 6.0s cast, west half
    IndolentWill = 10540, // Graven Image->self, 6.0s cast, gaze
    AveMaria = 10539, // Graven Image->self, 6.0s cast, inverted gaze
}

class Hyperdrive(BossModule module) : Components.SingleTargetCast(module, AID.Hyperdrive);
class UltimaUpsurge(BossModule module) : Components.RaidwideCast(module, AID.UltimaUpsurge);
class FlagrantFireSpread(BossModule module) : Components.SpreadFromCastTargets(module, AID.FlagrantFireSpread, 5);
class FlagrantFireStack(BossModule module) : Components.StackWithCastTargets(module, AID.FlagrantFireStack, 6);
class BlizzardBlitzOut(BossModule module) : Components.StandardAOEs(module, AID.BlizzardBlitzOut, 10);
class BlizzardBlitzIn(BossModule module) : Components.StandardAOEs(module, AID.BlizzardBlitzIn, new AOEShapeDonut(10, 40));
class BlizzardBlitzFakeDonut(BossModule module) : Components.StandardAOEs(module, AID.BlizzardBlitzFakeDonut, 10);
class BlizzardBlitzFakeNear(BossModule module) : Components.StandardAOEs(module, AID.BlizzardBlitzFakeNear, new AOEShapeDonut(10, 40));
class ThrummingThunderReal(BossModule module) : Components.StandardAOEs(module, AID.ThrummingThunderReal, new AOEShapeRect(40.5f, 5));
class ThrummingThunderFake(BossModule module) : Components.StandardAOEs(module, AID.ThrummingThunderFakeMirror, new AOEShapeRect(40.5f, 5));
class WaveCannon(BossModule module) : Components.StandardAOEs(module, AID.WaveCannon, new AOEShapeRect(70.5f, 3));
class IntemperateWill(BossModule module) : Components.StandardAOEs(module, AID.IntemperateWill, new AOEShapeCone(100, 90.Degrees()));
class GravitationalWave(BossModule module) : Components.StandardAOEs(module, AID.GravitationalWave, new AOEShapeCone(100, 90.Degrees()));
class Shockwave(BossModule module) : Components.KnockbackFromCastTarget(module, AID.Shockwave, 15, shape: new AOEShapeRect(100.5f, 20), kind: Kind.DirForward);
class AeroAssault(BossModule module) : Components.KnockbackFromCastTarget(module, AID.AeroAssault, 10, shape: new AOEShapeCircle(100));
class IndolentWill(BossModule module) : Components.CastGaze(module, AID.IndolentWill, range: 100);
class AveMaria(BossModule module) : Components.CastGaze(module, AID.AveMaria, true, 100);

class RevoltingRuin(BossModule module) : Components.GenericAOEs(module, AID.RevoltingRuin)
{
    private static readonly AOEShapeCone Shape = new(102.7f, 45.Degrees());
    private WPos _origin;
    private Angle _rotation;
    private bool _active;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_active)
            yield return new(Shape, _origin, _rotation, WorldState.CurrentTime);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!_active)
            return;

        hints.AddForbiddenZone(Shape, _origin, _rotation, WorldState.CurrentTime);
        var behind = Arena.ClampToBounds(_origin - 4 * _rotation.ToDirection());
        var off = behind - Arena.Center;
        var len = off.Length();
        if (len > 18)
            behind = Arena.Center + off * (18 / len);
        hints.GoalZones.Add(p => p.InCircle(behind, 1) ? 10 : 0);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc) { }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_active)
            Shape.Outline(Arena, _origin, _rotation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.IsSpell(AID.TimelyTeleportCircle) && (spell.LocXZ - Module.PrimaryActor.Position).LengthSq() > 25)
        {
            _origin = spell.LocXZ;
            _rotation = spell.Rotation;
            _active = true;
        }
        else if (spell.IsSpell(AID.AeroAssault) || spell.IsSpell(AID.AveMaria))
            _active = false;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.IsSpell(AID.RevoltingRuin) || spell.IsSpell(AID.AveMaria))
            _active = false;
    }
}

class KefkaStates : StateMachineBuilder
{
    public KefkaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Hyperdrive>()
            .ActivateOnEnter<UltimaUpsurge>()
            .ActivateOnEnter<FlagrantFireSpread>()
            .ActivateOnEnter<FlagrantFireStack>()
            .ActivateOnEnter<BlizzardBlitzOut>()
            .ActivateOnEnter<BlizzardBlitzIn>()
            .ActivateOnEnter<BlizzardBlitzFakeDonut>()
            .ActivateOnEnter<BlizzardBlitzFakeNear>()
            .ActivateOnEnter<ThrummingThunderReal>()
            .ActivateOnEnter<ThrummingThunderFake>()
            .ActivateOnEnter<WaveCannon>()
            .ActivateOnEnter<IntemperateWill>()
            .ActivateOnEnter<GravitationalWave>()
            .ActivateOnEnter<Shockwave>()
            .ActivateOnEnter<AeroAssault>()
            .ActivateOnEnter<IndolentWill>()
            .ActivateOnEnter<AveMaria>()
            .ActivateOnEnter<RevoltingRuin>();
    }
}

[ModuleInfo(Contributors = "croizat", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 289, NameID = 7131)]
public class Kefka(ModuleInit init) : BossModule(init, new(0, 0), new ArenaBoundsCircle(20));
