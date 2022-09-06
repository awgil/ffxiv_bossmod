namespace BossMod.Endwalker.Savage.P6SHegemone
{
    class Agonies : Components.StackSpread
    {
        public Agonies() : base(6, 15, 3) { }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.AgoniesDarkburst1:
                case AID.AgoniesDarkburst2:
                case AID.AgoniesDarkburst3:
                    SpreadMask.Set(module.Raid.FindSlot(spell.TargetID));
                    break;
                case AID.AgoniesUnholyDarkness1:
                case AID.AgoniesUnholyDarkness2:
                case AID.AgoniesUnholyDarkness3:
                    StackMask.Set(module.Raid.FindSlot(spell.TargetID));
                    break;
                case AID.AgoniesDarkPerimeter1:
                case AID.AgoniesDarkPerimeter2:
                    // don't really care about donuts, they auto resolve...
                    break;
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.AgoniesDarkburst1:
                case AID.AgoniesDarkburst2:
                case AID.AgoniesDarkburst3:
                    SpreadMask.Clear(module.Raid.FindSlot(spell.TargetID));
                    break;
                case AID.AgoniesUnholyDarkness1:
                case AID.AgoniesUnholyDarkness2:
                case AID.AgoniesUnholyDarkness3:
                    StackMask.Clear(module.Raid.FindSlot(spell.TargetID));
                    break;
                case AID.AgoniesDarkPerimeter1:
                case AID.AgoniesDarkPerimeter2:
                    // don't really care about donuts, they auto resolve...
                    break;
            }
        }
    }
}
