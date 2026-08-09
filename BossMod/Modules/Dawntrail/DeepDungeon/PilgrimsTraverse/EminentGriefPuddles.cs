namespace BossMod.Endwalker.DeepDungeon.PilgrimsTraverse;

abstract class LightAndDarkBase(BossModule module) : Components.GenericAOEs(module)
{
    protected readonly DateTime[] expirations = new DateTime[4];
    protected BitMask lightBuff;
    protected BitMask darkBuff;
    protected BitMask combined;
    protected readonly List<AOEInstance> _aoes = [with(2)];
    protected readonly List<ShapeDistance> _sdfs = [with(2)];
    protected readonly bool[] wantLight = new bool[2];
    protected readonly DateTime[] activations = new DateTime[2];
    protected bool aetherdrainActive;
    protected float hpDifference;
    protected uint griefHP, eaterHP;
    protected const string either = "Get light or dark buff!";
    protected const string light = "Get light buff!";
    protected const string dark = "Get dark buff!";
    protected const string switchColor = "Switch to other color!";

    // extracted from collision data - dark material ID: 00027004, light material ID: 20007004
    private readonly WPos[] light1 = [new(-602.77661f, -288.82251f), new(-602.86578f, -288.14209f), new(-603.28021f, -288.181f), new(-603.06482f, -287.6239f),
        new(-602.72101f, -287.4534f), new(-602.63885f, -286.80841f), new(-603.35138f, -286.24081f), new(-603.75201f, -286.42239f), new(-604.20679f, -285.50009f),
        new(-604.63623f, -285.79639f), new(-604.91998f, -285.5f), new(-605.23059f, -285.63519f), new(-605.27417f, -285.89001f), new(-605.99017f, -286.302f),
        new(-605.64441f, -286.8378f), new(-605.70526f, -287.20941f), new(-606.05365f, -287.26208f), new(-606.5f, -288.02069f), new(-605.77319f, -288.67291f),
        new(-605.073f, -288.492f), new(-604.59839f, -289.13849f), new(-604.21881f, -289.07501f), new(-603.24841f, -289.5f)];
    private readonly WPos[] light2 = [new(-586.74078f, -310.64859f), new(-586.92242f, -310.24799f), new(-586f, -309.79321f), new(-586.29651f, -309.36389f),
        new(-586f, -309.0799f), new(-586.13513f, -308.76941f), new(-586.39001f, -308.72589f), new(-586.802f, -308.0098f), new(-587.33783f, -308.35559f),
        new(-587.70953f, -308.29471f), new(-587.76233f, -307.94629f), new(-588.52069f, -307.5f), new(-589.17285f, -308.22672f), new(-588.992f, -308.927f),
        new(-589.63861f, -309.40161f), new(-589.57489f, -309.78131f), new(-590f, -310.75159f), new(-589.32239f, -311.22339f), new(-588.64203f, -311.13419f),
        new(-588.68103f, -310.71979f), new(-588.12378f, -310.9353f), new(-587.95331f, -311.27911f), new(-587.30841f, -311.36111f)];
    private readonly WPos[] light3 = [new(-580.2074f, -294.15311f), new(-580.27429f, -293.3024f), new(-580.58514f, -293.35129f), new(-580.42352f, -292.65479f),
        new(-580.16559f, -292.44171f), new(-580.10419f, -291.63541f), new(-580.63861f, -290.92599f), new(-580.93903f, -291.15302f), new(-581.28021f, -290f),
        new(-581.60217f, -290.37051f), new(-581.81512f, -290f), new(-582.04791f, -290.16879f), new(-582.08063f, -290.48749f), new(-582.61761f, -291.0025f),
        new(-582.35834f, -291.67221f), new(-582.40393f, -292.13681f), new(-582.66528f, -292.20261f), new(-583f, -293.15079f), new(-582.45502f, -293.96619f),
        new(-581.92981f, -293.73999f), new(-581.57397f, -294.54919f), new(-581.28912f, -294.4686f), new(-580.56128f, -295f)];
    private readonly WPos[] light4 = [new(-595.6203f, -304.4549f), new(-595.75598f, -303.92981f), new(-595.27112f, -303.57379f), new(-595.31879f, -303.289f),
        new(-595f, -302.56131f), new(-595.50818f, -302.2074f), new(-596.01862f, -302.27429f), new(-595.9892f, -302.58511f), new(-596.4071f, -302.42349f),
        new(-596.53497f, -302.16571f), new(-597.01874f, -302.10419f), new(-597.4444f, -302.63849f), new(-597.30823f, -302.939f), new(-598f, -303.28009f),
        new(-597.77771f, -303.6022f), new(-598f, -303.81311f), new(-597.89874f, -304.04791f), new(-597.70752f, -304.0806f), new(-597.39862f, -304.6178f),
        new(-596.9967f, -304.35831f), new(-596.7179f, -304.4039f), new(-596.67847f, -304.66541f), new(-596.1095f, -305.00021f)];
    private readonly WPos[] light5 = [new(-609.2074f, -304.49191f), new(-609.27429f, -303.98141f), new(-609.58514f, -304.01071f), new(-609.42352f, -303.5929f),
        new(-609.16559f, -303.465f), new(-609.10419f, -302.98129f), new(-609.63861f, -302.5556f), new(-609.93903f, -302.6918f), new(-610.28009f, -302f),
        new(-610.60217f, -302.22229f), new(-610.81354f, -302f), new(-611.04797f, -302.10129f), new(-611.08063f, -302.29251f), new(-611.61761f, -302.6015f),
        new(-611.35834f, -303.0033f), new(-611.40393f, -303.2821f), new(-611.66528f, -303.32159f), new(-612.00006f, -303.8905f), new(-611.45514f, -304.3797f),
        new(-610.92981f, -304.24399f), new(-610.57379f, -304.72891f), new(-610.28912f, -304.68121f), new(-609.56158f, -305f)];
    private readonly WPos[] dark1 = [new(-610.82715f, -291.77328f), new(-611.008f, -291.07309f), new(-610.36139f, -290.59839f), new(-610.42511f, -290.21869f),
        new(-610f, -289.24841f), new(-610.67761f, -288.77661f), new(-611.35797f, -288.86581f), new(-611.31897f, -289.28021f), new(-611.87622f, -289.0647f),
        new(-612.04669f, -288.72089f), new(-612.69159f, -288.63889f), new(-613.25922f, -289.35141f), new(-613.07758f, -289.75201f), new(-614f, -290.20679f),
        new(-613.70361f, -290.6362f), new(-614f, -290.9201f), new(-613.86487f, -291.23059f), new(-613.60999f, -291.27411f), new(-613.19806f, -291.9902f),
        new(-612.66217f, -291.64441f), new(-612.29047f, -291.70529f), new(-612.23779f, -292.05371f), new(-611.47931f, -292.5f)];
    private readonly WPos[] dark2 = [new(-594.76941f, -314.36499f), new(-594.72583f, -314.10999f), new(-594.00983f, -313.698f), new(-594.35559f, -313.1622f),
        new(-594.29474f, -312.79059f), new(-593.94678f, -312.73819f), new(-593.5f, -311.97931f), new(-594.22681f, -311.32709f), new(-594.927f, -311.508f),
        new(-595.40161f, -310.86151f), new(-595.78125f, -310.9252f), new(-596.75159f, -310.5f), new(-597.22339f, -311.1777f), new(-597.13434f, -311.85809f),
        new(-596.71979f, -311.819f), new(-596.9353f, -312.37631f), new(-597.27911f, -312.54681f), new(-597.36115f, -313.1918f), new(-596.64862f, -313.75931f),
        new(-596.24799f, -313.57761f), new(-595.79321f, -314.5f), new(-595.36377f, -314.20361f), new(-595.08002f, -314.5f)];
    private readonly WPos[] dark3 = [new(-618.39783f, -309.62949f), new(-618.18488f, -310f), new(-617.95209f, -309.83121f), new(-617.91937f, -309.5126f),
        new(-617.38239f, -308.9975f), new(-617.64166f, -308.32791f), new(-617.59607f, -307.86319f), new(-617.33472f, -307.79739f), new(-617f, -306.84921f),
        new(-617.54498f, -306.03381f), new(-618.07019f, -306.26001f), new(-618.42657f, -305.452f), new(-618.71088f, -305.5314f), new(-619.43872f, -305f),
        new(-619.7926f, -305.84689f), new(-619.72571f, -306.6976f), new(-619.41486f, -306.64871f), new(-619.57648f, -307.34531f), new(-619.83441f, -307.55829f),
        new(-619.89594f, -308.36459f), new(-619.36139f, -309.07419f), new(-619.06097f, -308.84711f), new(-618.71991f, -310.00009f)];
    private readonly WPos[] dark4 = [new(-588.95221f, -297.8988f), new(-588.91962f, -297.70761f), new(-588.38239f, -297.3985f), new(-588.64166f, -296.9967f),
        new(-588.59607f, -296.7179f), new(-588.33472f, -296.67841f), new(-588.00006f, -296.1095f), new(-588.54523f, -295.6203f), new(-589.07019f, -295.7561f),
        new(-589.42621f, -295.27109f), new(-589.71088f, -295.31879f), new(-590.43872f, -295f), new(-590.7926f, -295.50809f), new(-590.72571f, -296.01859f),
        new(-590.41486f, -295.98929f), new(-590.57648f, -296.4071f), new(-590.83441f, -296.535f), new(-590.89581f, -297.01871f), new(-590.36139f, -297.4444f),
        new(-590.06097f, -297.3082f), new(-589.71991f, -298f), new(-589.39783f, -297.77771f), new(-589.18488f, -298f)];
    private readonly WPos[] dark5 = [new(-602.5556f, -297.36151f), new(-602.69177f, -297.061f), new(-602f, -296.71991f), new(-602.22229f, -296.39789f),
        new(-602f, -296.18491f), new(-602.10126f, -295.95209f), new(-602.29248f, -295.9194f), new(-602.6015f, -295.38239f), new(-603.0033f, -295.64169f),
        new(-603.2821f, -295.5961f), new(-603.32159f, -295.33481f), new(-603.8905f, -295.00021f), new(-604.3797f, -295.5451f), new(-604.24402f, -296.07019f),
        new(-604.72888f, -296.42621f), new(-604.68121f, -296.711f), new(-605f, -297.43869f), new(-604.49182f, -297.7926f), new(-603.98138f, -297.72571f),
        new(-604.0108f, -297.41489f), new(-603.5929f, -297.57651f), new(-603.46503f, -297.83429f), new(-602.98126f, -297.89581f)];

    public void AddAOE()
    {
        var center = Arena.Center;
        var shapeLight = new AOEShapeCustom(center, [new PolygonCustom(light1), new PolygonCustom(light2), new PolygonCustom(light3),
                new PolygonCustom(light4), new PolygonCustom(light5)]);
        _aoes.Add(new(shapeLight, center, color: Colors.Light, risky: false, shapeDistance: shapeLight.InvertedDistance(center, default)));
        _sdfs.Add(shapeLight.Distance(center, default));
        var shapeDark = new AOEShapeCustom(center, [new PolygonCustom(dark1), new PolygonCustom(dark2), new PolygonCustom(dark3),
                new PolygonCustom(dark4), new PolygonCustom(dark5)]);
        _aoes.Add(new(shapeDark, center, color: Colors.FutureVulnerable, risky: false, shapeDistance: shapeDark.InvertedDistance(center, default)));
        _sdfs.Add(shapeDark.Distance(center, default));
    }

    protected void UpdateStatus(ref BitMask mask, Actor actor, DateTime value)
    {
        var slot = Raid.FindSlot(actor.InstanceID);
        if (value == default)
        {
            mask.Clear(slot);
            combined.Clear(slot);
        }
        else
        {
            mask.Set(slot);
            combined.Set(slot);
        }
        if (slot >= 0)
        {
            expirations[slot] = value;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (hpDifference != default)
        {
            hints.Add($"Eater HP: {(hpDifference > 0f ? "+" : "")}{hpDifference:f1}%");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        ref var aoeLight = ref aoes[0];
        ref var aoeDark = ref aoes[1];
        aoeDark.Risky = false;
        aoeLight.Risky = false;
        if (!aetherdrainActive)
        {
            if (eaterHP <= 1u && !lightBuff[slot])
            {
                aoeLight.Risky = true;
            }
            else if (griefHP <= 1u && !darkBuff[slot])
            {
                aoeDark.Risky = true;
            }
            else if (Math.Abs(hpDifference) > 25f || !combined[slot])
            {
                if (hpDifference > 0f)
                {
                    aoeDark.Risky = true;
                }
                else
                {
                    aoeLight.Risky = true;
                }
            }
        }
        else
        {
            if (wantLight[0])
            {
                UpdateAOE(ref aoeLight, _sdfs[1], lightBuff);
            }
            else
            {
                UpdateAOE(ref aoeDark, _sdfs[0], darkBuff);
            }
        }
        base.AddAIHints(slot, actor, assignment, hints);
        void UpdateAOE(ref AOEInstance aoeWant, ShapeDistance aoeAvoid, BitMask mask)
        {
            if (!mask[slot] || expirations[slot] < aoeWant.Activation)
            {
                aoeWant.Risky = true;
                aoeWant.Activation = activations[0];
            }
            hints.TemporaryObstacles.Add(aoeAvoid);
        }
    }
}
