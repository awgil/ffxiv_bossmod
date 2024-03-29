namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn;

// common base for lightwaves components
class LightwaveCommon : Components.CastCounter
{
    protected List<Actor> Waves = new();
    protected static readonly AOEShapeRect WaveAOE = new(50, 8); // note that actual length is 15, but we want to show aoe for full path

    private static readonly float _losRadius = 1;

    public LightwaveCommon() : base(ActionID.MakeSpell(AID.LightOfTheCrystal)) { }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(module, caster, spell);
        if ((AID)spell.Action.ID == AID.RayOfLight && !Waves.Contains(caster))
        {
            Waves.Add(caster);
        }
    }

    protected bool InSafeCone(WPos origin, WPos blocking, WPos position)
    {
        var toBlock = blocking - origin;
        var toCheck = position - origin;
        var dist = toBlock.Length();
        if (dist > toCheck.Length())
            return false;

        var center = Angle.FromDirection(toBlock);
        var halfAngle = Angle.Asin(_losRadius / dist);
        return position.InCone(origin, center, halfAngle);
    }

    protected void DrawSafeCone(MiniArena arena, WPos origin, WPos blocking)
    {
        var toBlock = blocking - origin;
        var dist = toBlock.Length();
        var center = Angle.FromDirection(toBlock);
        var halfAngle = Angle.Asin(_losRadius / dist);
        arena.ZoneCone(origin, dist, 40, center, halfAngle, ArenaColor.SafeFromAOE);
    }
}
