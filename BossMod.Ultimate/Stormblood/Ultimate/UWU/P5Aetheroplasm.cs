namespace BossMod.Stormblood.Ultimate.UWU;

class P5AethericBoom : Components.KnockbackFromCastTarget
{
    readonly DateTime _deadlineDefault;

    public P5AethericBoom(BossModule module) : base(module, AID.AethericBoom, 10)
    {
        _deadlineDefault = WorldState.FutureTime(6.1f);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var side = actor.Role == Role.Tank ? -1 : 1;
        hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Arena.Center + new WDir(0, 3).Rotate((side * 45).Degrees()), 1), Module.CastFinishAt(Casters.FirstOrDefault()?.CastInfo, fallback: _deadlineDefault));
    }
}
class P5Aetheroplasm(BossModule module) : Components.CastCounter(module, AID.AetheroplasmUltima)
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(Module.Enemies(OID.Ultimaplasm).Where(p => !p.IsDead), ArenaColor.Object, true);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (NumCasts < 2)
        {
            var p1 = ShapeDistance.InvertedCircle(new(108, 108), 1);
            var p2 = ShapeDistance.InvertedCircle(new(92, 108), 1);
            hints.AddForbiddenZone(ShapeDistance.Intersection([p1, p2]));
        }
        else if (NumCasts < 4)
        {
            var p3 = ShapeDistance.InvertedCircle(new(108, 92), 1);
            var p4 = ShapeDistance.InvertedCircle(new(92, 92), 1);
            hints.AddForbiddenZone(ShapeDistance.Intersection([p3, p4]));
        }
    }
}
