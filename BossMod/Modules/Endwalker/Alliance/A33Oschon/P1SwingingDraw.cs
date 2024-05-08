namespace BossMod.Endwalker.Alliance.A33Oschon;

class P1SwingingDraw(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.SwingingDrawAOE))
{
    public readonly List<AOEInstance> AOEs = [];
    private static readonly AOEShapeCone _shape = new(60, 60.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var dir = (AID)spell.Action.ID switch
        {
            AID.SwingingDrawCW => -45.Degrees(),
            AID.SwingingDrawCCW => 45.Degrees(),
            _ => default
        };
        if (dir != default)
        {
            dir += Angle.FromDirection(caster.Position - Module.Center);
            AOEs.Add(new(_shape, Module.Center + 25 * dir.ToDirection(), dir + 180.Degrees(), spell.NPCFinishAt.AddSeconds(6.2f)));
        }
    }
}
