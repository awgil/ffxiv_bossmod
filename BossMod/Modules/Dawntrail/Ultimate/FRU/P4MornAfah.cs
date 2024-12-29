namespace BossMod.Dawntrail.Ultimate.FRU;

class P4MornAfah(BossModule module) : Components.UniformStackSpread(module, 4, 0, 8)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.MornAfahOracle)
        {
            // note: target is random?..
            var target = WorldState.Actors.Find(caster.TargetID);
            if (target != null)
                AddStack(target, Module.CastFinishAt(spell, 0.9f));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.MornAfahAOE) // TODO: proper spell...
            Stacks.Clear();
    }
}

class P4MornAfahHPCheck(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        var usurper = Module.Enemies(OID.UsurperOfFrostP4).FirstOrDefault();
        var oracle = Module.Enemies(OID.OracleOfDarknessP4).FirstOrDefault();
        if (usurper != null && oracle != null)
        {
            var diff = (int)(usurper.HPMP.CurHP - oracle.HPMP.CurHP) * 100.0f / usurper.HPMP.MaxHP;
            hints.Add($"Usurper HP: {(diff > 0 ? "+" : "")}{diff:f1}%");
        }
    }
}
