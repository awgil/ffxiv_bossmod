namespace BossMod.Endwalker.Savage.P11SThemis
{
    class HeartOfJudgment : Components.GenericTowers
    {
        public override void Init(BossModule module)
        {
            for (int i = 0; i < 4; ++i)
                Towers.Add(new(module.Bounds.Center + 11.5f * (45 + i * 90).Degrees().ToDirection(), 4, 2, 2));
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.Explosion or AID.MassiveExplosion)
            {
                ++NumCasts;
                Towers.Clear();
            }
        }
    }
}
