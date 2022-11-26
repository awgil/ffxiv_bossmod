namespace BossMod.Endwalker.Savage.P7SAgdistis
{
    class BladesOfAttis : Components.Exaflare
    {
        class LineWithActor : Line
        {
            public Actor Caster;

            public LineWithActor(Actor caster)
            {
                Next = caster.Position;
                Advance = 7 * caster.Rotation.ToDirection();
                LastExplosion = caster.CastInfo!.FinishAt.AddSeconds(-2);
                TimeToMove = 2;
                ExplosionsLeft = 8;
                MaxShownExplosions = 8;
                Caster = caster;
            }
        }

        public BladesOfAttis() : base(7) { }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.BladesOfAttisFirst)
            {
                Lines.Add(new LineWithActor(caster));
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.BladesOfAttisFirst or AID.BladesOfAttisRest)
            {
                int index = Lines.FindIndex(item => ((LineWithActor)item).Caster == caster);
                if (index == -1)
                {
                    module.ReportError(this, $"Failed to find entry for {caster.InstanceID:X}");
                    return;
                }

                AdvanceLine(module, Lines[index], caster.Position);
                if (Lines[index].ExplosionsLeft == 0)
                    Lines.RemoveAt(index);
            }
        }
    }
}
