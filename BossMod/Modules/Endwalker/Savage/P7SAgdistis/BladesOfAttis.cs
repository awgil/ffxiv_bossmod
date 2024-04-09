namespace BossMod.Endwalker.Savage.P7SAgdistis;

class BladesOfAttis(BossModule module) : Components.Exaflare(module, 7)
{
    class LineWithActor : Line
    {
        public Actor Caster;

        public LineWithActor(Actor caster)
        {
            Next = caster.Position;
            Advance = 7 * caster.Rotation.ToDirection();
            NextExplosion = caster.CastInfo!.NPCFinishAt;
            TimeToMove = 2;
            ExplosionsLeft = 8;
            MaxShownExplosions = 8;
            Caster = caster;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.BladesOfAttisFirst)
        {
            Lines.Add(new LineWithActor(caster));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.BladesOfAttisFirst or AID.BladesOfAttisRest)
        {
            int index = Lines.FindIndex(item => ((LineWithActor)item).Caster == caster);
            if (index == -1)
            {
                ReportError($"Failed to find entry for {caster.InstanceID:X}");
                return;
            }

            AdvanceLine(Lines[index], caster.Position);
            if (Lines[index].ExplosionsLeft == 0)
                Lines.RemoveAt(index);
        }
    }
}
