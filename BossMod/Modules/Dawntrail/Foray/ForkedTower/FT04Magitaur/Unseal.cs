namespace BossMod.Dawntrail.Foray.ForkedTower.FT04Magitaur;

class UnsealAutos(BossModule module) : Components.CastCounterMulti(module, [AID._Ability_Attack1, AID._Ability_Attack3])
{
    public enum Weapon
    {
        None,
        Axe,
        Lance
    }

    public Weapon CurrentWeapon;
    private readonly List<Actor>[] _targets = Utils.MakeArray<List<Actor>>(3, []);
    private IEnumerable<Actor> AllTargets => _targets.SelectMany(t => t);

    public DateTime NextActivation;

    public void Predict(float delay) => NextActivation = WorldState.FutureTime(delay);

    public override void Update()
    {
        foreach (var t in _targets)
            t.Clear();

        if (CurrentWeapon == Weapon.None)
            return;

        foreach (var group in WorldState.Actors.Where(p => p.Type == ActorType.Player && !p.IsDead).GroupBy(FT04Magitaur.GetPlatform))
        {
            if (group.Key < 0)
                continue;

            var sorted = group.SortedByRange(Module.PrimaryActor.Position);
            if (CurrentWeapon == Weapon.Axe)
                _targets[group.Key].AddRange(sorted.Take(2));

            if (CurrentWeapon == Weapon.Lance)
                _targets[group.Key].AddRange(sorted.TakeLast(2));
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CurrentWeapon == Weapon.Axe)
            hints.Add("Autos: close");
        else if (CurrentWeapon == Weapon.Lance)
            hints.Add("Autos: far");
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (CurrentWeapon == Weapon.None)
            return;

        if (actor.Role == Role.Tank)
        {
            var platform = FT04Magitaur.GetPlatform(actor);
            if (platform < 0)
                hints.Add("Get on platform!");
            else
                hints.Add(CurrentWeapon == Weapon.Axe ? "Stay close to boss!" : "Stay away from boss!", _targets[platform].Any(a => a.Role != Role.Tank));
        }
        else if (AllTargets.Contains(actor))
            hints.Add(CurrentWeapon == Weapon.Axe ? "Stay away from boss!" : "Stay close to boss!");
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.UnsealAxe:
                CurrentWeapon = Weapon.Axe;
                NextActivation = WorldState.FutureTime(8);
                break;
            case AID.UnsealLance:
                CurrentWeapon = Weapon.Lance;
                NextActivation = WorldState.FutureTime(8);
                break;
        }

        if (WatchedActions.Contains(spell.Action))
        {
            NumCasts++;
            NextActivation = WorldState.FutureTime(3.2f);
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_Unsealed)
        {
            if (status.Extra == 0x353)
                CurrentWeapon = Weapon.Axe;
            else if (status.Extra == 0x354)
                CurrentWeapon = Weapon.Lance;
            else
                ReportError($"unrecognized status extra for Unsealed: {status.Extra}");
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => AllTargets.Contains(player) ? PlayerPriority.Danger : PlayerPriority.Normal;
}

class ForkedFury(BossModule module) : Components.CastCounter(module, AID._Ability_ForkedFury1)
{
    record struct Bait(Actor? Close, Actor? Far);

    private DateTime _activation;
    private readonly Bait[] _targets = new Bait[3];

    public override void Update()
    {
        Array.Fill(_targets, default);
        if (_activation == default)
            return;

        foreach (var group in WorldState.Actors.Where(p => p.Type == ActorType.Player && !p.IsDead).GroupBy(FT04Magitaur.GetPlatform))
        {
            if (group.Key < 0)
                continue;

            Actor? prev = null;
            foreach (var actor in group.SortedByRange(Module.PrimaryActor.Position))
            {
                if (prev == null)
                    _targets[group.Key].Close = actor;

                prev = actor;
            }
            if (prev != null)
                _targets[group.Key].Far = prev;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Ability_ForkedFury)
            _activation = Module.CastFinishAt(spell);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _activation = default;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_activation != default)
            hints.Add("Tankbuster");
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_activation == default || actor.Role == Role.Tank)
            return;

        foreach (var t in _targets)
        {
            if (t.Close == actor)
            {
                hints.Add("Too close to boss!");
                return;
            }
            if (t.Far == actor)
            {
                hints.Add("Too far from boss!");
                return;
            }
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => _targets.Any(t => t.Close == player || t.Far == player) ? PlayerPriority.Danger : PlayerPriority.Normal;
}
