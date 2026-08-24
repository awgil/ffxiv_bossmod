namespace BossMod.Dawntrail.Extreme.Ex1Valigarmanda;

class RuinfallTower(BossModule module) : Components.GenericTowers(module, AID.RuinfallTower)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Towers.Add(new(caster.Position, 6, 2, 2, Module.Raid.WithSlot(true).WhereActor(p => p.Role != Role.Tank).Mask()));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Towers.Clear();
    }
}

class RuinfallKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, AID.RuinfallKnockback, 25, kind: Kind.DirForward);
class RuinfallAOE(BossModule module) : Components.StandardAOEs(module, AID.RuinfallAOE, 6);
