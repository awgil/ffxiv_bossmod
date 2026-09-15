namespace BossMod.Dawntrail.Extreme.Ex3Sphene;

class TyrannysGraspAOE(BossModule module) : Components.StandardAOEs(module, AID.TyrannysGraspAOE, new AOEShapeRect(20, 20));

class TyrannysGraspTowers(BossModule module) : Components.GenericTowers(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.TyrannysGraspTower1 or AID.TyrannysGraspTower2)
        {
            Towers.Add(new(caster.Position, 4, 1, 1, Raid.WithSlot(true).WhereActor(p => p.Role != Role.Tank).Mask(), Module.CastFinishAt(spell)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.TyrannysGraspTower1 or AID.TyrannysGraspTower2)
        {
            ++NumCasts;
            Towers.RemoveAll(t => t.Position.AlmostEqual(caster.Position, 1));
            foreach (ref var t in Towers.AsSpan())
                foreach (var target in spell.Targets)
                    t.ForbiddenSoakers.Set(Raid.FindSlot(target.ID));
        }
    }
}
