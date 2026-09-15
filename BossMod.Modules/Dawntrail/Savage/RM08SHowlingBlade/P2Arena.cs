namespace BossMod.Dawntrail.Savage.RM08SHowlingBlade;

class P2Arena(BossModule module) : BossComponent(module)
{
    public bool Active;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0 && state == 0x00080004)
            Active = true;
    }
}

class DestructiblePlatforms(BossModule module) : BossComponent(module)
{
    public BitMask MissingPlatforms;

    public int DisappearCounter;
    public int ReappearCounter;

    public override void OnMapEffect(byte index, uint state)
    {
        var ix = index - 0x16;
        if (ix is >= 0 and <= 4)
        {
            if (state == 0x00080004)
            {
                DisappearCounter++;
                MissingPlatforms.Set(ix);

                // fallback: enrage ends with destroying last platform, but we can't make an arena with no bounds, framework doesn't like that
                var safePlatforms = MissingPlatforms.NumSetBits() >= 5 ? MissingPlatforms.WithoutBit(ix) : MissingPlatforms;
                Arena.Bounds = RM08SHowlingBlade.MakeBoundsP2(safePlatforms);
            }

            if (state == 0x00020001)
            {
                ReappearCounter++;
                MissingPlatforms.Clear(ix);
                Arena.Bounds = RM08SHowlingBlade.MakeBoundsP2(MissingPlatforms);
            }
        }
    }
}

static class P2Platforms
{
    public static int GetPlatform(WPos p)
    {
        for (var i = 0; i < 5; i++)
            if (p.InCone(new WPos(100, 100), 72.Degrees() * -i, 36.Degrees()))
                return i;

        return -1; // technically unreachable
    }

    public static int GetPlatform(Actor a) => GetPlatform(a.Position);
    public static bool SamePlatform(Actor a, Actor b) => GetPlatform(a) == GetPlatform(b);

    public static WPos GetPlatformCenter(int i) => new WPos(100, 100) + (72.Degrees() * -i).ToDirection() * 17.5f;

    public static void ZonePlatform(MiniArena arena, int i, uint color) => arena.ZoneCircle(GetPlatformCenter(i), 8, color);
}

static class PlatformExtensions
{
    public static IEnumerable<Actor> OnPlatform(this IEnumerable<Actor> actors, int platform) => actors.Where(a => P2Platforms.GetPlatform(a) == platform);
    public static IEnumerable<(int, Actor)> OnPlatform(this IEnumerable<(int, Actor)> actors, int platform) => actors.Where(a => P2Platforms.GetPlatform(a.Item2) == platform);
    public static IEnumerable<Actor> OnSamePlatform(this IEnumerable<Actor> actors, Actor b) => actors.Where(a => P2Platforms.SamePlatform(a, b));
    public static IEnumerable<Actor> OnDifferentPlatform(this IEnumerable<Actor> actors, Actor b) => actors.Where(a => !P2Platforms.SamePlatform(a, b));
    public static IEnumerable<(int, Actor)> OnSamePlatform(this IEnumerable<(int, Actor)> actors, Actor b) => actors.Where(a => P2Platforms.SamePlatform(a.Item2, b));
    public static bool SamePlatform(this Actor a, Actor b) => P2Platforms.GetPlatform(a) == P2Platforms.GetPlatform(b);
}
