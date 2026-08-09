namespace BossMod.Endwalker.DeepDungeon.PilgrimsTraverse.DD50Ogbunabali;

public enum OID : uint
{
    Ogbunabali = 0x4872, // R5.67
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 45130, // Ogbunabali->player, no cast, single-target
    Teleport = 43539, // Ogbunabali->location, no cast, single-target

    Liquefaction = 43531, // Ogbunabali->self, 3.0s cast, single-target
    FallingRock = 43532, // Helper->self, 3.0s cast, range 5 circle

    Sandpit = 43533, // Ogbunabali->self, 3.0s cast, single-target
    PitAmbushFirst = 43534, // Ogbunabali->self, 3.0s cast, range 6 circle
    PitAmbush2nd3rd = 43535, // Ogbunabali->location, no cast, range 6 circle
    PitAmbushLast = 43536, // Ogbunabali->location, no cast, range 6 circle
    Windraiser = 43537, // Ogbunabali->self, 3.0s cast, single-target
    BitingWindVoidzone = 45113, // Helper->self, no cast, range 3 circle
    BitingWind = 43538 // Helper->self, 8.0s cast, range 20 circle, knockback 20, away from source
}

public enum SID : uint
{
    SixFulmsUnder = 567, // none->player, extra=0x40E/0x410/0x412/0x414/0x400
    BreakerOfWind = 4625 // none->player, extra=0x0
}

public enum IconID : uint
{
    PitAmbush = 640 // player->self
}

[SkipLocalsInit]
sealed class Quicksand(BossModule module) : Components.GenericAOEs(module, warningText: "GTFO from quicksand!")
{
    private AOEInstance[] _aoe = [];
    private AOEInstance[] _aoeInv = [];
    private readonly DateTime[] expirations = new DateTime[4];
    private bool kbImminent;
    private DateTime kbActivation;

    // extracted from collision data - material ID: 00007004 (only exact matches inside arena)
    private readonly WPos[] vertices1 = [new(-288.4317f, -303.03119f), new(-287.71945f, -302.6756f), new(-287.43317f, -302.2095f), new(-287.44647f, -302.14667f),
        new(-287.30231f, -302.01669f), new(-287.09418f, -301.97656f), new(-286.89252f, -302.23557f), new(-286.58737f, -302.08163f), new(-285.8587f, -300.55795f),
        new(-286.05823f, -299.90787f), new(-286.3049f, -299.59317f), new(-287.18127f, -299.4119f), new(-287.30084f, -299.21179f), new(-288.17279f, -298.67078f),
        new(-288.36728f, -298.69333f), new(-288.87677f, -298.93207f), new(-288.96548f, -299.10803f), new(-288.90317f, -299.53339f), new(-289.05612f, -299.94833f),
        new(-289.18024f, -299.96606f), new(-289.43982f, -299.81265f), new(-289.62378f, -299.86508f), new(-289.87756f, -300.69382f), new(-290.05569f, -301.87811f),
        new(-289.67661f, -302.00513f), new(-289.46793f, -301.96176f), new(-289.2081f, -302.57965f), new(-289.0137f, -302.75156f), new(-288.63535f, -302.84525f),
        new(-288.56879f, -303.02283f)];
    private readonly WPos[] vertices2 = [new(-295.20569f, -292.67392f), new(-295.02234f, -292.91083f), new(-294.832f, -293.00104f), new(-294.98102f, -293.86533f),
        new(-294.72113f, -294.18573f), new(-294.43561f, -294.40491f), new(-294.45911f, -294.51047f), new(-294.38611f, -294.60773f), new(-293.79474f, -294.686f),
        new(-293.25281f, -294.47614f), new(-293.21313f, -294.39664f), new(-293.00272f, -294.38794f), new(-292.78644f, -294.4791f), new(-292.82199f, -294.83334f),
        new(-292.39703f, -294.84351f), new(-291.85657f, -294.54321f), new(-290.90948f, -293.94849f), new(-290.70325f, -293.29169f), new(-290.74677f, -292.7804f),
        new(-291.37762f, -292.2099f), new(-291.36325f, -291.93622f), new(-291.82977f, -290.89917f), new(-292.34402f, -290.7951f), new(-292.52057f, -290.88879f),
        new(-292.64224f, -291.14462f), new(-293.26077f, -291.62848f), new(-293.42859f, -291.6048f), new(-293.58514f, -291.26938f), new(-293.68817f, -291.20142f),
        new(-294.3847f, -291.77203f)];
    private readonly WPos[] vertices3 = [new(-301.41266f, -291.07758f), new(-301.23645f, -290.12909f), new(-300.84222f, -290.14087f), new(-300.69684f, -289.86188f),
        new(-300.78195f, -289.48346f), new(-301.00806f, -289.35162f), new(-301.1665f, -289.17627f), new(-301.75739f, -288.79446f), new(-301.89417f, -288.7905f),
        new(-302.21039f, -288.27911f), new(-302.55627f, -288.02054f), new(-302.95609f, -288.59207f), new(-303.08368f, -288.63992f), new(-303.62994f, -288.40625f),
        new(-303.93433f, -288.74771f), new(-304.34106f, -289.26077f), new(-304.6853f, -290.04431f), new(-304.7478f, -290.35974f), new(-304.59521f, -290.57001f),
        new(-303.90121f, -291.31622f), new(-303.55197f, -291.68842f), new(-303.11902f, -291.80606f), new(-302.70911f, -291.82919f), new(-302.52576f, -291.78522f),
        new(-302.2457f, -291.76172f), new(-302.08765f, -291.3674f), new(-301.69373f, -291.35962f)];
    private readonly WPos[] vertices4 = [new(-309.51031f, -300.15561f), new(-309.00595f, -300.86011f), new(-308.16318f, -300.8847f), new(-308.0719f, -300.92792f),
        new(-307.65497f, -300.64471f), new(-307.37997f, -300.11935f), new(-307.40134f, -300.07141f), new(-307.15408f, -299.86621f), new(-307.1543f, -300.32251f),
        new(-306.51807f, -299.83875f), new(-305.92398f, -298.06927f), new(-306.05652f, -297.81061f), new(-306.57684f, -297.28815f), new(-307.57767f, -297.2829f),
        new(-307.71603f, -297.13535f), new(-308.6944f, -296.8443f), new(-309.22073f, -297.18103f), new(-309.25909f, -297.34991f), new(-309.16742f, -297.60641f),
        new(-309.23749f, -298.32706f), new(-309.35419f, -298.46393f), new(-309.69113f, -298.36557f), new(-309.79086f, -298.40546f), new(-309.80579f, -299.14432f),
        new(-309.74261f, -300.17142f)];
    private readonly WPos[] vertices5 = [new(-309.2475f, -306.25641f), new(-309.54453f, -306.18964f), new(-309.83493f, -306.50693f), new(-310.21802f, -306.98056f),
        new(-310.48584f, -307.58716f), new(-310.58539f, -307.81638f), new(-310.62747f, -308.02795f), new(-310.45197f, -308.22934f), new(-309.72821f, -309.00775f),
        new(-309.46875f, -308.99362f), new(-309.25238f, -309.36426f), new(-309.08911f, -309.50275f), new(-308.62982f, -309.52539f), new(-308.44843f, -309.45135f),
        new(-308.23322f, -309.45419f), new(-308.09125f, -309.06931f), new(-307.69055f, -309.06757f), new(-308.09125f, -309.06931f), new(-307.69055f, -309.06757f),
        new(-307.41281f, -308.78857f), new(-307.22461f, -307.82846f), new(-306.83005f, -307.80823f), new(-306.71408f, -307.58908f), new(-306.79498f, -307.22855f),
        new(-307.03909f, -307.07196f), new(-307.12927f, -306.8898f), new(-307.58661f, -306.55521f), new(-307.90103f, -306.5488f), new(-308.19858f, -306.06815f),
        new(-308.45395f, -305.84329f), new(-308.90588f, -306.41437f)];
    private readonly WPos[] vertices6 = [new(-300.466f, -303.29657f), new(-301.11261f, -304.14236f), new(-301.25662f, -304.17093f), new(-301.54715f, -304.44144f),
        new(-301.96213f, -304.8725f), new(-302.00766f, -305.09674f), new(-301.95236f, -305.42624f), new(-301.8338f, -305.51096f), new(-301.67285f, -305.53525f),
        new(-301.34521f, -305.77173f), new(-300.98407f, -305.94089f), new(-300.71155f, -306.41711f), new(-299.83105f, -306.41742f), new(-299.66541f, -306.32587f),
        new(-298.96906f, -306.28461f), new(-298.86328f, -306.18683f), new(-298.70569f, -305.80841f), new(-298.5809f, -305.2124f), new(-298.28488f, -305.06406f),
        new(-298.27136f, -304.72949f), new(-298.72882f, -303.95312f), new(-299.23779f, -303.21857f), new(-299.55338f, -303.06668f)];
    private readonly WPos[] vertices7 = [new(-296.15662f, -309.29065f), new(-295.35419f, -308.40924f), new(-294.70901f, -307.85321f), new(-294.60776f, -307.91599f),
        new(-294.44226f, -308.27478f), new(-294.25229f, -308.28009f), new(-293.95331f, -308.18393f), new(-293.52103f, -307.698f), new(-293.43533f, -307.51782f),
        new(-293.2616f, -307.4285f), new(-292.8602f, -307.50412f), new(-292.40048f, -308.55029f), new(-292.41571f, -308.83606f), new(-291.80374f, -309.35443f),
        new(-291.75861f, -309.91306f), new(-291.96002f, -310.54529f), new(-292.89001f, -311.13153f), new(-293.41718f, -311.42416f), new(-293.7475f, -311.41589f),
        new(-293.77054f, -311.0885f), new(-294.24588f, -310.97299f), new(-294.30276f, -311.06641f), new(-294.83328f, -311.2652f), new(-295.3074f, -311.20007f),
        new(-295.3595f, -311.09863f), new(-295.61853f, -310.79724f), new(-295.75922f, -310.71832f), new(-295.97998f, -310.48044f), new(-295.82977f, -309.57254f),
        new(-296.01053f, -309.47983f)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoe.Length == 0)
        {
            return [];
        }
        if (kbImminent && WorldState.CurrentTime > kbActivation)
        {
            return _aoeInv;
        }
        ref var aoe = ref _aoe[0];
        aoe.Activation = expirations[slot];
        return _aoe;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.Liquefaction:
                var center = Arena.Center;
                var shape = new AOEShapeCustom(center, [new Square(center, 20f)], [new PolygonCustom(vertices1), new PolygonCustom(vertices2), new PolygonCustom(vertices3),
                new PolygonCustom(vertices4), new PolygonCustom(vertices5), new PolygonCustom(vertices6),new PolygonCustom(vertices7)]);
                _aoe = [new(shape, center, shapeDistance: shape.Distance(center, default))];
                _aoeInv = [new(shape, center, shapeDistance: shape.InvertedDistance(center, default), color: Colors.SafeFromAOE)];
                Array.Fill(expirations, DateTime.MaxValue);
                break;
            case (uint)AID.BitingWind:
                var act = Module.CastFinishAt(spell);
                kbActivation = act.AddSeconds(-2d);
                kbImminent = true;
                if (_aoeInv.Length != 0)
                {
                    ref var aoeInv = ref _aoeInv[0];
                    aoeInv.Activation = act;
                }
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BitingWind)
        {
            kbActivation = default;
            kbImminent = false;
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.SixFulmsUnder)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
            {
                expirations[slot] = status.ExpireAt.AddSeconds(-1d); // small safety margin because of latency
            }
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.SixFulmsUnder)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
            {
                expirations[slot] = DateTime.MaxValue;
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var isImminent = kbImminent && WorldState.CurrentTime > kbActivation;
        if (!isImminent || !kbImminent)
        {
            base.AddHints(slot, actor, hints);
            if (!kbImminent)
            {
                return;
            }
        }
        if (_aoeInv.Length != 0)
        {
            ref var aoe = ref _aoeInv[0];
            if (!aoe.Check(actor.Position))
            {
                hints.Add(isImminent ? "Go into quicksand to avoid knockback!" : "Prepare to go into quicksand to avoid knockback!");
                return;
            }
        }
    }
}

[SkipLocalsInit]
sealed class WindraiserArena(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private readonly AOEShapeDonut donut = new(15f, 20f);
    private readonly WPos center = new(-300f, -300f);
    private static Polygon[] GetBaseCircle() => [new Polygon(new(-300f, -300f), 15f, 72)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x01)
        {
            switch (state)
            {
                case 0x00100004u:
                    Arena.Center = center;
                    Arena.Bounds = new ArenaBoundsCustom(GetBaseCircle());
                    break;
                case 0x00020001u:
                    Arena.Center = center;
                    Arena.Bounds = new ArenaBoundsCustom(GetBaseCircle(), [new Polygon(center.Quantized(), 3f, 64)]);
                    break;
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Windraiser)
        {
            _aoe = [new(donut, center, default, Module.CastFinishAt(spell, 0.8d), shapeDistance: donut.Distance(center, default))];
        }
    }
}

[SkipLocalsInit]
sealed class BitingWindKB(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.BitingWind, 20f)
{
    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.BreakerOfWind)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
            {
                PlayerImmunes[slot].DutyBuffExpire = DateTime.MaxValue;
            }
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.BreakerOfWind)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
            {
                PlayerImmunes[slot].DutyBuffExpire = default;
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints) { } // hint handled by quicksand component
}

[SkipLocalsInit]
sealed class FallingRock(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FallingRock, 5f);

[SkipLocalsInit]
sealed class PitAmbush(BossModule module) : Components.StandardChasingAOEs(module, 5f, (uint)AID.PitAmbushFirst, default, 5f, 2.1d, 4, true, (uint)IconID.PitAmbush)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.PitAmbushFirst or (uint)AID.PitAmbush2nd3rd or (uint)AID.PitAmbushLast)
        {
            var pos = spell.MainTargetID == caster.InstanceID ? caster.Position.Quantized() : WorldState.Actors.Find(spell.MainTargetID)?.Position ?? spell.TargetXZ;
            Advance(pos, MoveDistance, WorldState.CurrentTime);
            if (Chasers.Count == 0 && ResetTargets)
            {
                Targets.Clear();
                NumCasts = 0;
            }
        }
    }
}

[SkipLocalsInit]
sealed class DD50OgbunabaliStates : StateMachineBuilder
{
    public DD50OgbunabaliStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WindraiserArena>()
            .ActivateOnEnter<Quicksand>()
            .ActivateOnEnter<PitAmbush>()
            .ActivateOnEnter<FallingRock>()
            .ActivateOnEnter<BitingWindKB>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified,
StatesType = typeof(DD50OgbunabaliStates),
ConfigType = null,
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = typeof(SID),
TetherIDType = null,
IconIDType = typeof(IconID),
PrimaryActorOID = (uint)OID.Ogbunabali,
Contributors = "The Combat Reborn Team (Malediktus)",
Expansion = BossModuleInfo.Expansion.Dawntrail,
Category = BossModuleInfo.Category.DeepDungeon,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 1036u,
NameID = 14263u,
SortOrder = 1,
PlanLevel = 0)]
[SkipLocalsInit]
public sealed class DD50Ogbunabali : BossModule
{
    public DD50Ogbunabali(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private DD50Ogbunabali(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        WPos[] vertices = [new(-295.2f, -317.95f), new(-293.93f, -317.72f), new(-293.32f, -317.45f), new(-292.28f, -316.67f), new(-291.93f, -315.37f),
        new(-290.14f, -313.47f), new(-289.58f, -313.35f), new(-289f, -313.36f), new(-288.38f, -313.43f), new(-287.76f, -313.22f),
        new(-287.18f, -312.87f), new(-287.04f, -311.6f), new(-286.89f, -310.97f), new(-286.63f, -310.34f), new(-286.34f, -309.76f),
        new(-285.36f, -308.89f), new(-283.63f, -308.22f), new(-282.93f, -307.15f), new(-282.6f, -305.91f), new(-282.5f, -305.25f),
        new(-282.41f, -302.05f), new(-282.52f, -301.4f), new(-283.13f, -300.34f), new(-283.56f, -297.13f), new(-283.58f, -294.5f),
        new(-283.69f, -293.85f), new(-284.46f, -291.31f), new(-284.73f, -290.69f), new(-285.38f, -289.51f), new(-285.95f, -289.17f),
        new(-289.46f, -287.29f), new(-289.8f, -286.8f), new(-290.36f, -285.63f), new(-290.35f, -285.05f), new(-290.45f, -284.39f),
        new(-292.15f, -281.54f), new(-293.91f, -281.79f), new(-294.59f, -281.84f), new(-295.85f, -281.81f), new(-297.1f, -281.53f),
        new(-297.58f, -281.2f), new(-298.05f, -280.82f), new(-298.71f, -280.76f), new(-299.35f, -280.93f), new(-300.03f, -281.2f),
        new(-300.7f, -281.38f), new(-301.37f, -281.5f), new(-301.93f, -281.51f), new(-303.06f, -280.87f), new(-303.73f, -280.95f),
        new(-304.6f, -282.04f), new(-306.46f, -282.1f), new(-306.92f, -282.63f), new(-307.47f, -283.91f), new(-307.8f, -284.42f),
        new(-308.33f, -284.77f), new(-309.45f, -285.28f), new(-310.01f, -285.63f), new(-310.94f, -286.66f), new(-311.71f, -287.78f),
        new(-312.37f, -288.87f), new(-314.23f, -289.63f), new(-314.58f, -290f), new(-315.65f, -291.63f), new(-316.48f, -292.62f),
        new(-316.42f, -293.33f), new(-316.23f, -294.01f), new(-316.23f, -294.63f), new(-316.3f, -295.28f), new(-316.48f, -295.92f),
        new(-317.68f, -298.2f), new(-317.82f, -299.51f), new(-317.69f, -300.16f), new(-317.09f, -301.28f), new(-316.53f, -303.12f),
        new(-316.67f, -304.37f), new(-316.51f, -305.07f), new(-315.62f, -306.18f), new(-315.43f, -306.72f), new(-315.33f, -307.33f),
        new(-314.98f, -307.88f), new(-314.46f, -308.19f), new(-311.94f, -311.87f), new(-311.68f, -312.44f), new(-311.48f, -313.04f),
        new(-311f, -313.5f), new(-308.67f, -314.81f), new(-307.58f, -315.49f), new(-301.01f, -316.41f), new(-297.96f, -317.92f)];
        var arena = new ArenaBoundsCustom([new PolygonCustom(vertices)]);
        return (arena.Center, arena);
    }
}
