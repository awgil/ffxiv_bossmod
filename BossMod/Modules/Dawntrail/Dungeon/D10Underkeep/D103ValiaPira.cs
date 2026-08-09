namespace BossMod.Dawntrail.Dungeon.D10Underkeep.D103ValiaPira;

public enum OID : uint
{
    Boss = 0x478E, // R4.5
    CoordinateBit1 = 0x4789, // R1.0
    CoordinateBit2 = 0x47B5, // R1.0
    CoordinateTurret = 0x4793, // R1.0
    Orb = 0x478F, // R1.6
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 42526, // Boss->player, no cast, single-target

    EntropicSphere = 42525, // Boss->self, 5.0s cast, range 40 circle
    CoordinateMarch = 42513, // Boss->self, 4.0s cast, single-target
    BitAndOrbCollide = 42514, // Orb->CoordinateBit1/CoordinateBit2, no cast, single-target
    EnforcementRay = 42737, // CoordinateBit->self, 0.5s cast, range 36 width 9 cross
    OrderedFireVisual1 = 42508, // Boss->self, no cast, single-target
    OrderedFireVisual2 = 42509, // Helper->Boss, 1.0s cast, single-target
    Electray = 43130, // CoordinateTurret->self, 7.0s cast, range 40 width 9 rect
    ElectricFieldVisual = 42519, // Boss->self, 6.6+0,7s cast, single-target
    ConcurrentField = 42521, // Helper->self, 7.3s cast, range 26 50-degree cone
    ElectricField = 43261, // Helper->self, no cast, range 26 50-degree cone
    NeutralizeFrontLines = 42738, // Boss->self, 5.0s cast, range 30 180-degree cone
    HyperchargedLight = 42524, // Helper->player, 5.0s cast, range 5 circle, spread
    Bloodmarch = 42739, // Boss->self, 5.0s cast, single-target
    DeterrentPulse = 42540 // Boss->self/players, 5.0s cast, range 40 width 8 rect, line stack
}

public enum IconID : uint
{
    ElectricField = 586, // Boss->player
    DeterrentPulse = 525 // Boss->player
}

public enum TetherID : uint
{
    OrbTeleport = 282 // CoordinateBit2->Orb
}

sealed class EntropicSphere(BossModule module) : Components.RaidwideCast(module, (uint)AID.EntropicSphere);
sealed class Electray(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Electray, new AOEShapeRect(40f, 4.5f));
sealed class ConcurrentField(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ConcurrentField, new AOEShapeCone(26f, 25f.Degrees()));
sealed class ElectricField(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(26f, 25f.Degrees()), (uint)IconID.ElectricField, (uint)AID.ElectricField, 7.4d);
sealed class NeutralizeFrontLines(BossModule module) : Components.SimpleAOEs(module, (uint)AID.NeutralizeFrontLines, new AOEShapeCone(30f, 90f.Degrees()));
sealed class HyperchargedLight(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.HyperchargedLight, 5f);
sealed class DeterrentPulse(BossModule module) : Components.LineStack(module, (uint)IconID.DeterrentPulse, (uint)AID.DeterrentPulse, 5.3d, 40f, 4f, 4, 4, 1, false);

sealed class EnforcementRay(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCross cross = new(36f, 4.5f);
    private readonly List<AOEInstance> _aoes = [with(3)];
    private bool teleported;
    private readonly List<WPos> startingpositions = [with(2)];
    private WPos center;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count != 0 ? CollectionsMarshal.AsSpan(_aoes)[..1] : [];

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x11DA && !teleported)
        {
            _aoes.Add(new(cross, actor.Position.Quantized()));
            if (center != default)
                UpdateAOEs();
        }
        else if (id == 0x11D5)
        {
            startingpositions.Add(actor.Position);
            if (startingpositions.Count == 2)
            {
                var pos0 = startingpositions[0];
                var pos1 = startingpositions[1];
                center = new((pos0.X + pos1.X) * 0.5f, (pos0.Z + pos1.Z) * 0.5f);
                UpdateAOEs();
            }
        }
        void UpdateAOEs()
        {
            var count = _aoes.Count;
            if (count > 1)
            {
                SortAOEs();
                var aoes = CollectionsMarshal.AsSpan(_aoes);

                var activationTime = NumCasts switch
                {
                    0 => 8.3d,
                    1 => 11.4d,
                    _ => 7.3d
                };

                aoes[0].Activation = WorldState.FutureTime(activationTime);

                if (NumCasts == 0)
                    aoes[1].Activation = WorldState.FutureTime(11.4d);
            }
        }
    }

    private void SortAOEs()
    {
        var count = _aoes.Count;
        if (count < 2)
            return;

        var aoes = CollectionsMarshal.AsSpan(_aoes);
        if ((aoes[0].Origin - center).LengthSq() > (aoes[1].Origin - center).LengthSq())
            (aoes[0], aoes[1]) = (aoes[1], aoes[0]);
        if (count == 3)
        {
            if ((aoes[0].Origin - center).LengthSq() > (aoes[2].Origin - center).LengthSq())
                (aoes[0], _aoes[2]) = (aoes[2], aoes[0]);

            if ((aoes[1].Origin - center).LengthSq() > (aoes[2].Origin - center).LengthSq())
                (aoes[1], aoes[2]) = (aoes[2], aoes[1]);
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (source.OID == (uint)OID.CoordinateBit2 && tether.ID == (uint)TetherID.OrbTeleport)
        {
            teleported = true;
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            var len = aoes.Length;
            var position = WorldState.Actors.Find(tether.Target)?.Position;
            if (position is not WPos pos)
                return;
            for (var i = 0; i < len; ++i)
            {
                ref var aoe = ref aoes[i];
                if (aoe.Origin.AlmostEqual(pos, 1f))
                {
                    aoe.Origin = source.Position.Quantized();
                    break;
                }
            }
            SortAOEs();

            double[] timings = NumCasts == 1 ? [2.8, 7.9] : [0.6, 7.9, 14.4];
            var lenT = timings.Length;
            var max = lenT > len ? len : lenT;
            for (var i = 0; i < max; ++i)
            {
                aoes[i].Activation = WorldState.FutureTime(timings[i]);
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.EnforcementRay)
        {
            var count = _aoes.Count;
            var pos = spell.LocXZ;
            for (var i = 0; i < count; ++i)
            {
                if (_aoes[i].Origin.AlmostEqual(pos, 1f))
                {
                    _aoes.RemoveAt(i);
                    if (_aoes.Count == 0)
                    {
                        teleported = false;
                        center = default;
                        startingpositions.Clear();
                        ++NumCasts;
                    }
                    return;
                }
            }
        }
    }
}

sealed class D103ValiaPiraStates : StateMachineBuilder
{
    public D103ValiaPiraStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<EntropicSphere>()
            .ActivateOnEnter<Electray>()
            .ActivateOnEnter<ConcurrentField>()
            .ActivateOnEnter<ElectricField>()
            .ActivateOnEnter<NeutralizeFrontLines>()
            .ActivateOnEnter<HyperchargedLight>()
            .ActivateOnEnter<DeterrentPulse>()
            .ActivateOnEnter<EnforcementRay>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1027, NameID = 13749)]
public sealed class D103ValiaPira(WorldState ws, Actor primary) : BossModule(ws, primary, new(0f, -331f), new ArenaBoundsSquare(17.5f));
