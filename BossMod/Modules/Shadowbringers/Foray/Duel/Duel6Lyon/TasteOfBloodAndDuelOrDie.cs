namespace BossMod.Shadowbringers.Foray.Duel.Duel6Lyon;

class TasteOfBloodAndDuelOrDie(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShape _tasteOfBloodShape = new AOEShapeCone(40, 90.Degrees());
    public readonly List<Actor> Casters = [];
    public readonly List<Actor> Duelers = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var caster in Casters)
        {
            // If the caster did Duel Or Die, the player must get hit by their attack.
            // This is represented by pointing the AOE behind the caster so their front is safe.
            Angle angle = Duelers.Contains(caster) ? caster.Rotation + 180.Degrees() : caster.Rotation;
            yield return new AOEInstance(_tasteOfBloodShape, caster.Position, angle);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.TasteOfBlood)
            Casters.Add(caster);

        if ((AID)spell.Action.ID == AID.DuelOrDie)
            Duelers.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.TasteOfBlood)
        {
            Casters.Remove(caster);
            Duelers.Remove(caster);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var caster in Casters)
        {
            bool isDueler = Duelers.Contains(caster);
            Arena.Actor(caster, isDueler ? ArenaColor.Danger : ArenaColor.Enemy, true);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Duelers.Count > 0)
            hints.Add($"Get hit by {Duelers.Count} Duel or Die Taste Of Blood");
    }
}
