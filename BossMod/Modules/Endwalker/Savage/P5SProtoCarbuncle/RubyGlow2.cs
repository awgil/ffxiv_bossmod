namespace BossMod.Endwalker.Savage.P5SProtoCarbuncle;

// note: we start showing magic aoe only after double rush resolve
class RubyGlow2(BossModule module) : RubyGlowCommon(module, ActionID.MakeSpell(AID.DoubleRush))
{
    private string _hint = "";

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        // TODO: correct explosion time
        var magic = NumCasts > 0 ? MagicStones.FirstOrDefault() : null;
        if (magic != null)
            yield return new(ShapeHalf, Module.Center, Angle.FromDirection(QuadrantDir(QuadrantForPosition(magic.Position))));
        foreach (var p in ActivePoisonAOEs())
            yield return p;
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_hint.Length > 0)
            hints.Add(_hint);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            var magic = MagicStones.FirstOrDefault();
            if (magic != null)
            {
                _hint = QuadrantDir(QuadrantForPosition(magic.Position)).Dot(spell.LocXZ - caster.Position) > 0 ? "Stay after charge" : "Swap sides after charge";
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _hint = "";
    }
}
