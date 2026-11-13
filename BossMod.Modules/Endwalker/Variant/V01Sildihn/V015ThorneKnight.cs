namespace BossMod.Endwalker.Variant.V01Sildihn.V015ThorneKnight;

public enum OID : uint
{
    Boss = 0x3917, // R7.200, x1
    MagickedPuppet = 0x3949, // R0.500
    AmaljaaArtilleryCarriage = 0x394A, // R0.500
    BallOfFire = 0x3989, // R1.500, x0 (spawn during fight)
    Helper = 0x233C, // R0.500, Helper type
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target
    Slashburn1 = 28911, // Boss->self, 8.5s cast, single-target
    Slashburn2 = 28912, // Boss->self, 8.5s cast, single-target
    Slashburn3 = 28913, // Boss->self, 8.5s cast, single-target
    Slashburn4 = 28930, // Boss->self, 8.5s cast, single-target
    BlazingBeacon1 = 28921, // Helper->self, 8.5s cast, range 50 width 16 rect
    BlazingBeacon2 = 28928, // Helper->self, 8.5s cast, range 50 width 16 rect
    BlazingBeaconFake = 28926, // Helper->self, 6.5s cast, range 50 width 16 rect
    SacredFlay1 = 28922, // Helper->self, 8.5s cast, range 50 45-degree cone
    SacredFlay2 = 28929, // Helper->self, 8.5s cast, range 50 45-degree cone
    SacredFlayFake = 28927, // Helper->self, 6.5s cast, range 50 45-degree cone
    BlisteringBlow = 28906, // Boss->player, 5.0s cast, single-target, tankbuster
    Cogwheel = 28907, // Boss->self, 5.0s cast, range 50 circle, raidwide
    Explosion = 28925, // BallOfFire->self, 7.0s cast, range 50 width 6 cross
    ForeHonor = 28908, // Boss->self, 6.0s cast, range 50 180-degree cone
    SpringToLife1 = 28909, // Boss->self, 7.5s cast, single-target
    SpringToLife2 = 28910, // Boss->self, 7.5s cast, single-target
    MagicCannon = 28919, // MagickedPuppet->self, no cast, range 45 width 6 rect
    AmaljaaArtillery = 28920, // AmaljaaArtilleryCarriage->self, no cast, range 45 width 6 rect
    ActivatePuppetVisual = 28966, // MagickedPuppet->self, no cast, single-target
    DeactivatePuppetVisual = 28967, // MagickedPuppet->self, no cast, single-target
    BlazeOfGlory = 28916, // Boss->self, 3.0s cast, single-target
    SignalFlareVisual = 28917, // Boss->self, 5.5s cast, single-target
    SignalFlare = 28923, // Helper->self, 7.0s cast, range 10 circle
}

class SpringToLife(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly (WPos[] Carriages, Angle Facing)[] _walls =
    [
        ([new(263, -235), new(271.5f, -243.5f), new(280, -252)], 45.Degrees()),
        ([new(294, -256), new(302.5f, -247.5f), new(311, -239)], -45.Degrees()),
        ([new(267, -220.5f), new(275.5f, -212), new(284, -203.5f)], 135.Degrees()),
        ([new(298, -208), new(306.5f, -216.5f), new(315, -225)], -135.Degrees()),
    ];

    private readonly Dictionary<ulong, AOEInstance> _puppets = [];
    private readonly List<AOEInstance> _carriages = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _puppets.Values.Concat(_carriages);

    public override void OnMapEffect(byte index, uint state)
    {
        if (state != 0x00020001 || index >= _walls.Length)
            return;
        var (positions, facing) = _walls[index];
        var activation = WorldState.FutureTime(3.7f);
        foreach (var p in positions)
            _carriages.Add(new(new AOEShapeRect(45, 3), p, facing, activation));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ActivatePuppetVisual:
                _puppets[caster.InstanceID] = new(new AOEShapeRect(45, 3), caster.Position, spell.Rotation, WorldState.FutureTime(2.9f));
                break;
            case AID.MagicCannon:
                ++NumCasts;
                if (!_puppets.Remove(caster.InstanceID) && _puppets.Count > 0)
                    _puppets.Remove(_puppets.Keys.First());
                break;
            case AID.AmaljaaArtillery:
                ++NumCasts;
                var index = _carriages.FindIndex(c => c.Origin.AlmostEqual(caster.Position, 1));
                if (index >= 0 || _carriages.Count > 0)
                    _carriages.RemoveAt(index >= 0 ? index : 0);
                break;
        }
    }
}

class BlisteringBlow(BossModule module) : Components.SingleTargetDelayableCast(module, AID.BlisteringBlow);
class BlazingBeacon(BossModule module) : Components.GroupedAOEs(module, [AID.BlazingBeacon1, AID.BlazingBeacon2], new AOEShapeRect(50, 8));
class SacredFlay(BossModule module) : Components.GroupedAOEs(module, [AID.SacredFlay1, AID.SacredFlay2], new AOEShapeCone(50, 45.Degrees()));
class SignalFlare(BossModule module) : Components.StandardAOEs(module, AID.SignalFlare, new AOEShapeCircle(10), 6);
class Explosion(BossModule module) : Components.StandardAOEs(module, AID.Explosion, new AOEShapeCross(50, 3));
class ForeHonor(BossModule module) : Components.StandardAOEs(module, AID.ForeHonor, new AOEShapeCone(50, 90.Degrees()));
class Cogwheel(BossModule module) : Components.RaidwideCast(module, AID.Cogwheel);

class V015ThorneKnightStates : StateMachineBuilder
{
    public V015ThorneKnightStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SpringToLife>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<SignalFlare>()
            .ActivateOnEnter<ForeHonor>()
            .ActivateOnEnter<SacredFlay>()
            .ActivateOnEnter<BlazingBeacon>()
            .ActivateOnEnter<Cogwheel>()
            .ActivateOnEnter<BlisteringBlow>();
    }
}

[ModuleInfo(Incomplete = true, Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 868, NameID = 11419, SortOrder = 6)]
public class V015ThorneKnight(ModuleInit init) : BossModule(init, new(289, -230), new ArenaBoundsRect(17.5f, 17.5f, 45.Degrees()));
