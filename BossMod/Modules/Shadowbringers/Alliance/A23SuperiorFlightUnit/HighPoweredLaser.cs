namespace BossMod.Shadowbringers.Alliance.A23SuperiorFlightUnit;

class HighPoweredLaser(BossModule module) : Components.CastCounter(module, AID._Weaponskill_ManeuverHighPoweredLaser1)
{
    record struct Stack(Actor Source, Actor Target, DateTime Activation)
    {
        public readonly bool Check(Actor player) => player.Position.InRect(Source.Position, Source.DirectionTo(Target).Normalized(), 80, 0, 7);
    }
    private readonly List<Stack> _stacks = [];

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _stacks.RemoveAll(c => c.Target.InstanceID == spell.MainTargetID);
        }

        if ((AID)spell.Action.ID == AID._Ability_ && WorldState.Actors.Find(spell.MainTargetID) is { } tar)
            _stacks.Add(new(caster, tar, WorldState.FutureTime(5.4f)));
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var s in _stacks)
            Arena.ZoneRect(s.Source.Position, s.Source.DirectionTo(s.Target), 80, 0, 7, ArenaColor.SafeFromAOE);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_stacks.Count == 0)
            return;
        else if (_stacks.Any(s => s.Target == actor))
            hints.Add("Stack with party!", false);
        else
        {
            var count = _stacks.Count(c => c.Check(actor));
            if (count == 0)
                hints.Add("Stack!");
            else
            {
                hints.Add("Stack!", false);
                if (count > 1)
                    hints.Add("GTFO from other stacks!");
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_stacks.Count == 0)
            return;

        if (_stacks.All(s => s.Target != actor))
        {
            var rects = _stacks.Select(s => ShapeContains.Rect(s.Source.Position, s.Source.DirectionTo(s.Target), 80, 0, 7)).ToList();
            hints.AddForbiddenZone(p => rects.Count(f => f(p)) != 1, _stacks[0].Activation);
        }

        hints.PredictedDamage.Add((Raid.WithSlot().Where(p => _stacks.Any(s => s.Check(p.Item2))).Mask(), _stacks[0].Activation));
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => _stacks.Any(s => s.Target == player) ? PlayerPriority.Interesting : PlayerPriority.Normal;
}
