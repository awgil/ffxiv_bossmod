namespace BossMod.Dawntrail.Foray.ForkedTower.FT04Magitaur;

class AssassinsDagger(BossModule module) : Components.GenericAOEs(module)
{
    class Dagger(Angle angle, DateTime d1, DateTime d2)
    {
        public Angle Angle = angle;
        public DateTime Activation1 = d1;
        public DateTime Activation2 = d2;
        public int NumCasts;
    }

    private readonly List<Dagger> _daggers = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        DateTime next = default;
        foreach (var dagger in _daggers)
        {
            if (next == default)
                next = dagger.Activation1.AddSeconds(0.5f);
            if (dagger.Activation1 < next)
                yield return new AOEInstance(new AOEShapeRect(32, 3), Arena.Center, dagger.Angle, dagger.NumCasts > 0 ? dagger.Activation2 : dagger.Activation1, Color: ArenaColor.Danger);
            else if (dagger.Activation1 < next.AddSeconds(4))
                yield return new AOEInstance(new AOEShapeRect(32, 3), Arena.Center, dagger.Angle, dagger.Activation1, Risky: false);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Ability_AssassinsDagger1)
        {
            var orientation = Angle.FromDirection(spell.LocXZ - caster.Position);
            var next = Module.CastFinishAt(spell);
            for (var i = 0; i < 6; i++)
            {
                _daggers.Add(new(orientation, next, next.AddSeconds(2)));
                next = next.AddSeconds(4);
                orientation += 70.Degrees();
            }
            _daggers.SortBy(d => d.Activation1);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Ability_AssassinsDagger1 or AID._Ability_AssassinsDagger2 or AID._Ability_AssassinsDagger3)
        {
            NumCasts++;
            var dir = Angle.FromDirection(spell.TargetXZ - caster.Position);
            if (spell.TargetXZ.AlmostEqual(Arena.Center, 1))
                dir += 180.Degrees();

            var ix = _daggers.FindIndex(d => d.Angle.AlmostEqual(dir, 0.1f));
            if (ix >= 0)
            {
                if (_daggers[ix].NumCasts == 1)
                    _daggers.RemoveAt(ix);
                else
                    _daggers[ix].NumCasts++;
            }
        }
    }
}

// 12 hits, 10 degrees of rotation between each
class Dagger1(BossModule module) : Components.ChargeAOEs(module, AID._Ability_AssassinsDagger1, 3);
class Dagger2(BossModule module) : Components.DebugCasts(module, [AID._Ability_AssassinsDagger1, AID._Ability_AssassinsDagger2], new AOEShapeRect(0, 3, 32));
