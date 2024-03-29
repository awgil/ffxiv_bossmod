namespace BossMod.Endwalker.Savage.P8S1Hephaistos;

class SnakingKick : Components.GenericAOEs
{
    private static readonly AOEShapeCircle _shape = new(10);

    public SnakingKick() : base(ActionID.MakeSpell(AID.SnakingKick)) { }

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        yield return new(_shape, module.PrimaryActor.Position);
    }
}

// snake 'priority' depends on position, CW from N: N is 0, NE is 1, and so on
abstract class PetrifactionCommon : Components.GenericGaze
{
    public int NumEyeCasts { get; private set; }
    public int NumBloodCasts { get; private set; }
    public int NumCrownCasts { get; private set; }
    public int NumBreathCasts { get; private set; }
    protected List<(Actor caster, DateTime activation, int priority)> ActiveGorgons = new();

    public PetrifactionCommon() : base(ActionID.MakeSpell(AID.PetrifactionAOE)) { }

    public override IEnumerable<Eye> ActiveEyes(BossModule module, int slot, Actor actor)
    {
        if (ActiveGorgons.Count > NumCasts)
        {
            var maxActivation = ActiveGorgons[NumCasts].activation.AddSeconds(1);
            foreach (var g in ActiveGorgons.Skip(NumCasts).TakeWhile(g => g.activation < maxActivation))
                yield return new(g.caster.Position, g.activation);
        }
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        foreach (var g in module.Enemies(OID.Gorgon).Where(g => !g.IsDead))
            arena.Actor(g, ArenaColor.Enemy, true);
        base.DrawArenaForeground(module, pcSlot, pc, arena);
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Petrifaction)
        {
            var dir = Angle.FromDirection(caster.Position - module.Bounds.Center);
            var priority = (int)MathF.Round((180 - dir.Deg) / 45) % 8;
            ActiveGorgons.Add((caster, spell.NPCFinishAt.AddSeconds(1.1f), priority));
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(module, caster, spell);
        switch ((AID)spell.Action.ID)
        {
            case AID.EyeOfTheGorgon:
                ++NumEyeCasts;
                break;
            case AID.BloodOfTheGorgon:
                ++NumBloodCasts;
                break;
            case AID.CrownOfTheGorgon:
                ++NumCrownCasts;
                break;
            case AID.BreathOfTheGorgon:
                ++NumBreathCasts;
                break;
        }
    }

    public void DrawPetrify(Actor source, bool delayed, MiniArena arena) => arena.AddCone(source.Position, 25, source.Rotation, 45.Degrees(), delayed ? ArenaColor.Safe : ArenaColor.Danger);
    public void DrawExplode(Actor source, bool delayed, MiniArena arena) => arena.AddCircle(source.Position, 5, delayed ? ArenaColor.Safe : ArenaColor.Danger);
}
