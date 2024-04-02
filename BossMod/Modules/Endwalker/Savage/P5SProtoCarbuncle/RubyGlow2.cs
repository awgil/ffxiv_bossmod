namespace BossMod.Endwalker.Savage.P5SProtoCarbuncle;

class RubyGlow2 : RubyGlowCommon
{
    private string _hint = "";

    // note: we start showing magic aoe only after double rush resolve
    public RubyGlow2() : base(ActionID.MakeSpell(AID.DoubleRush)) { }

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        // TODO: correct explosion time
        var magic = NumCasts > 0 ? MagicStones.FirstOrDefault() : null;
        if (magic != null)
            yield return new(ShapeHalf, module.Bounds.Center, Angle.FromDirection(QuadrantDir(QuadrantForPosition(module, magic.Position))));
        foreach (var p in ActivePoisonAOEs(module))
            yield return p;
    }

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        if (_hint.Length > 0)
            hints.Add(_hint);
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            var magic = MagicStones.FirstOrDefault();
            if (magic != null)
            {
                _hint = QuadrantDir(QuadrantForPosition(module, magic.Position)).Dot(spell.LocXZ - caster.Position) > 0 ? "Stay after charge" : "Swap sides after charge";
            }
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _hint = "";
    }
}
