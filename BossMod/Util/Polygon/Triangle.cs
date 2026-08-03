// currently we use Clipper2 library (based on Vatti algorithm) for boolean operations and a custom Earcut implementation for triangulating
// note: the major user of these primitives is bounds clipper; since they operate in 'local' coordinates, we use WDir everywhere (offsets from center) and call that 'relative polygons' - i'm not quite happy with that, it's not very intuitive
namespace BossMod;

// a triangle; as basic as it gets
[SkipLocalsInit]
public readonly struct RelTriangle(WDir a, WDir b, WDir c)
{
    public readonly WDir A = a;
    public readonly WDir B = b;
    public readonly WDir C = c;
}
