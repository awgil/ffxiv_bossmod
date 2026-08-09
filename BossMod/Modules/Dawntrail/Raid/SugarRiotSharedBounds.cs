namespace BossMod.Dawntrail.Raid;

public abstract class SugarRiotSharedBounds : BossModule
{
    public SugarRiotSharedBounds(WorldState ws, Actor primary) : base(ws, primary, new(100f, 100f), new ArenaBoundsSquare(20f))
    {
        jumpEdgeSegments = GenerateSegments();
    }

    private readonly PolygonCustom polygonNorth = new([new(100.628f, 79.853f), new(100.525f, 80.108f), new(100.425f, 80.365f), new(100.327f, 80.622f),
    new(100.231f, 80.88f), new(100.138f, 81.139f), new(100.046f, 81.398f), new(99.956f, 81.658f), new(99.869f, 81.919f),
    new(99.784f, 82.181f), new(99.701f, 82.443f), new(99.62f, 82.706f), new(99.541f, 82.97f), new(99.464f, 83.234f),
    new(99.39f, 83.499f), new(99.317f, 83.765f), new(99.247f, 84.031f), new(99.179f, 84.298f), new(99.113f, 84.565f),
    new(99.05f, 84.833f), new(98.988f, 85.101f), new(98.929f, 85.37f), new(98.872f, 85.639f), new(98.817f, 85.909f),
    new(98.765f, 86.179f), new(98.714f, 86.45f), new(98.666f, 86.721f), new(98.62f, 86.992f), new(98.577f, 87.264f),
    new(98.535f, 87.536f), new(98.496f, 87.808f), new(98.459f, 88.081f), new(104.574f, 88.886f), new(104.604f, 88.663f),
    new(104.636f, 88.441f), new(104.67f, 88.219f), new(104.705f, 87.997f), new(104.743f, 87.775f), new(104.782f, 87.554f),
    new(104.823f, 87.333f), new(104.866f, 87.112f), new(104.911f, 86.892f), new(104.958f, 86.672f), new(105.006f, 86.452f),
    new(105.056f, 86.233f), new(105.108f, 86.015f), new(105.162f, 85.796f), new(105.217f, 85.578f), new(105.275f, 85.361f),
    new(105.334f, 85.144f), new(105.395f, 84.928f), new(105.457f, 84.712f), new(105.522f, 84.497f), new(105.588f, 84.282f),
    new(105.656f, 84.068f), new(105.725f, 83.854f), new(105.797f, 83.641f), new(105.87f, 83.428f), new(105.945f, 83.216f),
    new(106.021f, 83.005f), new(106.099f, 82.794f), new(106.179f, 82.584f), new(106.261f, 82.375f), new(106.345f, 82.166f),
    new(106.43f, 81.958f), new(106.517f, 81.751f), new(106.605f, 81.544f), new(106.696f, 81.338f), new(106.787f, 81.133f),
    new(106.881f, 80.929f), new(106.976f, 80.725f), new(107.073f, 80.522f), new(107.172f, 80.321f), new(107.272f, 80.119f),
    new(107.374f, 79.919f)]);
    private readonly PolygonCustom polygonMiddle = new([new(97.804f, 93.038f), new(97.766f, 93.309f), new(97.717f, 93.579f), new(97.659f, 93.847f),
    new(97.592f, 94.113f), new(97.517f, 94.376f), new(97.433f, 94.637f), new(97.341f, 94.895f), new(97.24f, 95.15f),
    new(97.131f, 95.402f), new(97.014f, 95.649f), new(96.889f, 95.893f), new(96.756f, 96.133f), new(96.615f, 96.368f),
    new(96.466f, 96.598f), new(96.31f, 96.823f), new(96.147f, 97.044f), new(95.977f, 97.258f), new(95.799f, 97.467f),
    new(95.615f, 97.67f), new(95.425f, 97.867f), new(95.228f, 98.058f), new(95.025f, 98.242f), new(94.816f, 98.419f),
    new(94.601f, 98.59f), new(94.381f, 98.753f), new(94.156f, 98.909f), new(93.925f, 99.057f), new(93.69f, 99.198f),
    new(93.451f, 99.331f), new(93.207f, 99.457f), new(92.959f, 99.574f), new(92.707f, 99.682f), new(95.068f, 105.382f),
    new(95.323f, 105.28f), new(95.581f, 105.188f), new(95.842f, 105.104f), new(96.105f, 105.029f), new(96.371f, 104.962f),
    new(96.639f, 104.904f), new(96.908f, 104.855f), new(97.18f, 104.815f), new(97.452f, 104.784f), new(97.725f, 104.761f),
    new(97.999f, 104.748f), new(98.273f, 104.743f), new(98.547f, 104.748f), new(98.821f, 104.761f), new(99.094f, 104.784f),
    new(99.366f, 104.815f), new(99.637f, 104.855f), new(99.907f, 104.904f), new(100.175f, 104.962f), new(100.441f, 105.029f),
    new(100.704f, 105.104f), new(100.965f, 105.188f), new(101.223f, 105.28f), new(101.478f, 105.381f), new(101.729f, 105.49f),
    new(101.977f, 105.607f), new(102.221f, 105.732f), new(102.461f, 105.865f), new(102.696f, 106.006f), new(102.926f, 106.155f),
    new(103.151f, 106.311f), new(103.371f, 106.475f), new(107.127f, 101.58f), new(106.911f, 101.411f), new(106.702f, 101.233f),
    new(106.499f, 101.049f), new(106.302f, 100.859f), new(106.112f, 100.662f), new(105.928f, 100.459f), new(105.75f, 100.25f),
    new(105.58f, 100.035f), new(105.417f, 99.815f), new(105.261f, 99.589f), new(105.112f, 99.359f), new(104.971f, 99.124f),
    new(104.838f, 98.884f), new(104.713f, 98.641f), new(104.596f, 98.393f), new(104.487f, 98.141f), new(104.386f, 97.887f),
    new(104.294f, 97.629f), new(104.21f, 97.368f), new(104.135f, 97.104f), new(104.068f, 96.838f), new(104.01f, 96.57f),
    new(103.961f, 96.301f), new(103.921f, 96.03f), new(103.89f, 95.757f), new(103.867f, 95.484f), new(103.854f, 95.21f),
    new(103.849f, 94.936f), new(103.854f, 94.662f), new(103.867f, 94.389f), new(103.89f, 94.115f), new(103.923f, 93.843f)]);
    private readonly PolygonCustom polygonEast = new([new(107.338f, 109.518f), new(107.557f, 109.684f), new(107.778f, 109.849f),
    new(107.999f, 110.012f), new(108.222f, 110.174f), new(108.447f, 110.333f), new(108.673f, 110.49f), new(108.899f, 110.646f),
    new(109.128f, 110.8f), new(109.357f, 110.952f), new(109.588f, 111.102f), new(109.82f, 111.25f), new(110.053f, 111.397f),
    new(110.287f, 111.541f), new(110.523f, 111.684f), new(110.759f, 111.824f), new(110.997f, 111.963f), new(111.236f, 112.099f),
    new(111.476f, 112.234f), new(111.717f, 112.367f), new(111.959f, 112.497f), new(112.203f, 112.626f), new(112.447f, 112.753f),
    new(112.692f, 112.878f), new(112.939f, 113f), new(113.186f, 113.121f), new(113.434f, 113.24f), new(113.684f, 113.356f),
    new(113.934f, 113.471f), new(114.185f, 113.584f), new(114.437f, 113.694f), new(114.69f, 113.803f), new(114.944f, 113.909f),
    new(115.199f, 114.013f), new(115.454f, 114.115f), new(115.711f, 114.215f), new(115.968f, 114.314f), new(116.226f, 114.409f),
    new(116.484f, 114.503f), new(116.744f, 114.595f), new(117.004f, 114.684f), new(117.265f, 114.772f), new(117.527f, 114.857f),
    new(117.789f, 114.94f), new(118.052f, 115.021f), new(118.316f, 115.1f), new(118.58f, 115.177f), new(118.845f, 115.251f),
    new(119.111f, 115.323f), new(119.377f, 115.394f), new(119.644f, 115.462f), new(119.911f, 115.527f), new(120.179f, 115.591f),
    new(120.058f, 109.184f), new(119.843f, 109.119f), new(119.628f, 109.053f), new(119.413f, 108.985f), new(119.2f, 108.916f),
    new(118.987f, 108.844f), new(118.774f, 108.771f), new(118.562f, 108.696f), new(118.351f, 108.62f), new(118.14f, 108.541f),
    new(117.93f, 108.461f), new(117.721f, 108.38f), new(117.512f, 108.296f), new(117.304f, 108.211f), new(117.097f, 108.124f),
    new(116.89f, 108.035f), new(116.684f, 107.945f), new(116.479f, 107.853f), new(116.275f, 107.76f), new(116.071f, 107.664f),
    new(115.868f, 107.567f), new(115.666f, 107.469f), new(115.465f, 107.369f), new(115.265f, 107.267f), new(115.065f, 107.163f),
    new(114.867f, 107.058f), new(114.669f, 106.951f), new(114.472f, 106.843f), new(114.276f, 106.733f), new(114.081f, 106.621f),
    new(113.887f, 106.508f), new(113.693f, 106.393f), new(113.501f, 106.277f), new(113.31f, 106.159f), new(113.119f, 106.04f),
    new(112.93f, 105.919f), new(112.742f, 105.796f), new(112.554f, 105.672f), new(112.368f, 105.546f), new(112.182f, 105.419f),
    new(111.998f, 105.291f), new(111.815f, 105.16f), new(111.633f, 105.029f), new(111.452f, 104.896f), new(111.272f, 104.761f),
    new(111.093f, 104.625f)]);
    private readonly PolygonCustom polygonWest = new([new(88.088f, 101.596f), new(87.834f, 101.703f), new(87.581f, 101.811f), new(87.329f, 101.922f),
    new(87.078f, 102.034f), new(86.828f, 102.149f), new(86.579f, 102.265f), new(86.33f, 102.384f), new(86.083f, 102.505f),
    new(85.837f, 102.628f), new(85.591f, 102.752f), new(85.347f, 102.879f), new(85.104f, 103.008f), new(84.861f, 103.138f),
    new(84.62f, 103.271f), new(84.38f, 103.406f), new(84.141f, 103.542f), new(83.904f, 103.681f), new(83.667f, 103.822f),
    new(83.431f, 103.964f), new(83.197f, 104.108f), new(82.964f, 104.255f), new(82.732f, 104.403f), new(82.501f, 104.553f),
    new(82.272f, 104.705f), new(82.044f, 104.859f), new(81.817f, 105.015f), new(81.591f, 105.172f), new(81.367f, 105.331f),
    new(81.144f, 105.493f), new(80.922f, 105.656f), new(80.701f, 105.821f), new(80.482f, 105.987f), new(80.265f, 106.156f),
    new(80.049f, 106.326f), new(79.834f, 106.498f), new(79.868f, 115.074f), new(80.013f, 114.902f), new(80.159f, 114.732f),
    new(80.306f, 114.562f), new(80.455f, 114.394f), new(80.605f, 114.227f), new(80.757f, 114.061f), new(80.91f, 113.896f),
    new(81.064f, 113.732f), new(81.22f, 113.57f), new(81.377f, 113.409f), new(81.535f, 113.25f), new(81.695f, 113.092f),
    new(81.856f, 112.935f), new(82.018f, 112.779f), new(82.181f, 112.625f), new(82.346f, 112.472f), new(82.512f, 112.32f),
    new(82.679f, 112.17f), new(82.847f, 112.021f), new(83.017f, 111.873f), new(83.188f, 111.727f), new(83.36f, 111.583f),
    new(83.533f, 111.439f), new(83.707f, 111.297f), new(83.883f, 111.157f), new(84.059f, 111.018f), new(84.237f, 110.88f),
    new(84.416f, 110.744f), new(84.596f, 110.61f), new(84.777f, 110.476f), new(84.959f, 110.345f), new(85.142f, 110.215f),
    new(85.327f, 110.086f), new(85.512f, 109.959f), new(85.698f, 109.833f), new(85.886f, 109.709f), new(86.074f, 109.586f),
    new(86.264f, 109.465f), new(86.454f, 109.346f), new(86.645f, 109.228f), new(86.838f, 109.112f), new(87.031f, 108.997f),
    new(87.225f, 108.884f), new(87.42f, 108.772f), new(87.616f, 108.662f), new(87.813f, 108.554f), new(88.011f, 108.447f),
    new(88.21f, 108.342f), new(88.409f, 108.238f), new(88.61f, 108.136f), new(88.811f, 108.036f), new(89.013f, 107.938f),
    new(89.216f, 107.841f), new(89.419f, 107.745f), new(89.623f, 107.652f), new(89.829f, 107.56f), new(90.034f, 107.469f),
    new(90.241f, 107.381f), new(90.448f, 107.294f)]);
    public PolygonCustom[] GetCombinedRiver() => [polygonNorth, polygonWest, polygonEast, polygonMiddle];
    private readonly (WPos, WPos)[] jumpEdges = [(new(97.639f, 93.016f), new(103.923f, 93.843f)), (new(104.243f, 88.842f), new(98.459f, 88.081f)),
    (new(94.940f, 105.074f), new(92.739f, 99.759f)), (new(88.216f, 101.905f), new(90.321f, 106.986f)), (new(106.924f, 101.845f), new(103.371f, 106.475f)),
    (new(107.389f, 109.451f), new(111.042f, 104.691f))];
    private readonly (WPos p, WDir d, float l)[] jumpEdgeSegments;

    public bool IntersectJumpEdge(WPos p, WDir d, float l)
    {
        for (var i = 0; i < 6; ++i)
        {
            ref readonly var e = ref jumpEdgeSegments[i];
            var n = e.d.OrthoL();
            var dirDot = d.Dot(n);
            if (dirDot < 0.05f)
            {
                continue;
            }

            var ts = n.Dot(e.p - p) / dirDot;
            if (ts < 0 || ts > l)
            {
                continue;
            }

            var te = d.OrthoL().Dot(p - e.p) / e.d.Dot(d.OrthoL());
            if (te >= 0 && te <= e.l)
            {
                return true;
            }
        }
        return false;
    }

    private (WPos p, WDir d, float l)[] GenerateSegments()
    {
        var segments = new (WPos p, WDir d, float l)[6];

        for (var i = 0; i < 6; ++i)
        {
            ref readonly var edge = ref jumpEdges[i];
            ref readonly var edge1 = ref edge.Item1;
            var edge2M1 = edge.Item2 - edge1;
            var direction = edge2M1.Normalized();
            var length = edge2M1.Length();
            segments[i] = (edge1, direction, length);
        }
        return segments;
    }
}
