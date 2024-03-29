namespace BossMod.Endwalker.Savage.P4S2Hesperos;

// state related to act 2 wreath of thorns
// note: there should be four tethered helpers on activation
// note: we assume that (1) dark targets soak all towers, (2) first fire to be broken is tank-healer pair (since their debuff is slightly shorter)
class WreathOfThorns2 : BossComponent
{
    public enum State { DarkDesign, FirstSet, SecondSet, Done }

    public State CurState { get; private set; } = State.DarkDesign;
    private List<Actor> _relevantHelpers = new(); // 2 aoes -> 8 towers -> 2 aoes
    private (Actor?, Actor?) _darkTH; // first is one having tether
    private (Actor?, Actor?) _fireTH;
    private (Actor?, Actor?) _fireDD;
    private IconID[] _playerIcons = new IconID[8];
    private int _numAOECasts = 0;

    private IEnumerable<Actor> _firstSet => _relevantHelpers.Take(4);
    private IEnumerable<Actor> _secondSet => _relevantHelpers.Skip(4);

    private static readonly float _fireExplosionRadius = 6;

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        bool isTowerSoaker = actor == _darkTH.Item1 || actor == _darkTH.Item2;
        if (CurState == State.DarkDesign)
        {
            if (!isTowerSoaker)
            {
                hints.Add("Stay in center", false);
            }
            else if (_darkTH.Item1!.Tether.ID != 0) // tether not broken yet
            {
                hints.Add("Break tether!");
            }
        }
        else
        {
            var curFirePair = (_fireTH.Item1 != null && _fireTH.Item1.Tether.ID != 0) ? _fireTH : ((_fireDD.Item1 != null && _fireDD.Item1.Tether.ID != 0) ? _fireDD : (null, null));
            bool isFromCurrentPair = actor == curFirePair.Item1 || actor == curFirePair.Item2;
            if (isFromCurrentPair)
            {
                hints.Add("Break tether!");
            }
            else if (curFirePair.Item1 != null && !isTowerSoaker)
            {
                bool nearFire = actor.Position.InCircle(curFirePair.Item1!.Position, _fireExplosionRadius) || actor.Position.InCircle(curFirePair.Item2!.Position, _fireExplosionRadius);
                hints.Add("Stack with breaking tether!", !nearFire);
            }

            if (CurState != State.Done)
            {
                var relevantHelpers = CurState == State.FirstSet ? _firstSet : _secondSet;
                if (relevantHelpers.Where(IsAOE).InRadius(actor.Position, P4S2.WreathAOERadius).Any())
                {
                    hints.Add("GTFO from AOE!");
                }

                var soakedTower = relevantHelpers.Where(IsTower).InRadius(actor.Position, P4S2.WreathTowerRadius).FirstOrDefault();
                if (isTowerSoaker)
                {
                    // note: we're assuming that players with 'dark' soak all towers
                    hints.Add("Soak the tower!", soakedTower == null);
                }
                else if (soakedTower != null)
                {
                    hints.Add("GTFO from tower!");
                }
            }
        }
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (CurState == State.Done)
            return;

        foreach (var aoe in (CurState == State.SecondSet ? _secondSet : _firstSet).Where(IsAOE))
            arena.ZoneCircle(aoe.Position, P4S2.WreathAOERadius, ArenaColor.AOE);
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        // draw players
        foreach (var player in module.Raid.WithoutSlot().Exclude(pc))
            arena.Actor(player, ArenaColor.PlayerGeneric);

        // draw pc's tether
        var pcPartner = pc.Tether.Target != 0
            ? module.WorldState.Actors.Find(pc.Tether.Target)
            : module.Raid.WithoutSlot().FirstOrDefault(p => p.Tether.Target == pc.InstanceID);
        if (pcPartner != null)
        {
            var tetherColor = _playerIcons[pcSlot] switch {
                IconID.AkanthaiFire => 0xff00ffff,
                IconID.AkanthaiWind => 0xff00ff00,
                _ => 0xffff00ff
            };
            arena.AddLine(pc.Position, pcPartner.Position, tetherColor);
        }

        // draw towers for designated tower soakers
        bool isTowerSoaker = pc == _darkTH.Item1 || pc == _darkTH.Item2;
        if (isTowerSoaker && CurState != State.Done)
            foreach (var tower in (CurState == State.SecondSet ? _secondSet : _firstSet).Where(IsTower))
                arena.AddCircle(tower.Position, P4S2.WreathTowerRadius, CurState == State.DarkDesign ?  ArenaColor.Danger : ArenaColor.Safe);

        // draw circles around next imminent fire explosion
        if (CurState != State.DarkDesign)
        {
            var curFirePair = (_fireTH.Item1 != null && _fireTH.Item1.Tether.ID != 0) ? _fireTH : ((_fireDD.Item1 != null && _fireDD.Item1.Tether.ID != 0) ? _fireDD : (null, null));
            if (curFirePair.Item1 != null)
            {
                arena.AddCircle(curFirePair.Item1!.Position, _fireExplosionRadius, isTowerSoaker ? ArenaColor.Danger : ArenaColor.Safe);
                arena.AddCircle(curFirePair.Item2!.Position, _fireExplosionRadius, isTowerSoaker ? ArenaColor.Danger : ArenaColor.Safe);
            }
        }
    }

    public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
    {
        if (source.OID == (uint)OID.Helper && tether.ID == (uint)TetherID.WreathOfThorns)
        {
            _relevantHelpers.Add(source);
        }
        else if (source.Type == ActorType.Player)
        {
            PlayerTetherOrIconAssigned(module, module.Raid.FindSlot(source.InstanceID), source);
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if (CurState == State.DarkDesign && (AID)spell.Action.ID == AID.DarkDesign)
            CurState = State.FirstSet;
        else if (CurState == State.FirstSet && (AID)spell.Action.ID == AID.AkanthaiExplodeAOE && ++_numAOECasts >= 2)
            CurState = State.SecondSet;
        else if (CurState == State.SecondSet && (AID)spell.Action.ID == AID.AkanthaiExplodeAOE && ++_numAOECasts >= 4)
            CurState = State.Done;
    }

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        var slot = module.Raid.FindSlot(actor.InstanceID);
        if (slot == -1)
            return;

        _playerIcons[slot] = (IconID)iconID;
        PlayerTetherOrIconAssigned(module, slot, actor);
    }

    private void PlayerTetherOrIconAssigned(BossModule module, int slot, Actor actor)
    {
        if (slot == -1 || _playerIcons[slot] == IconID.None || actor.Tether.Target == 0)
            return; // icon or tether not assigned yet

        var tetherTarget = module.WorldState.Actors.Find(actor.Tether.Target);
        if (tetherTarget == null)
            return; // weird

        if (_playerIcons[slot] == IconID.AkanthaiDark)
        {
            _darkTH = (actor, tetherTarget);
        }
        else if (_playerIcons[slot] == IconID.AkanthaiFire)
        {
            if (actor.Role == Role.Tank || actor.Role == Role.Healer)
                _fireTH = (actor, tetherTarget);
            else
                _fireDD = (actor, tetherTarget);
        }
    }

    private bool IsTower(Actor actor)
    {
        if (actor.Position.X < 90)
            return actor.Position.Z > 100;
        else if (actor.Position.Z < 90)
            return actor.Position.X < 100;
        else if (actor.Position.X > 110)
            return actor.Position.Z < 100;
        else if (actor.Position.Z > 110)
            return actor.Position.X > 100;
        else
            return false;
    }

    private bool IsAOE(Actor actor)
    {
        if (actor.Position.X < 90)
            return actor.Position.Z < 100;
        else if (actor.Position.Z < 90)
            return actor.Position.X > 100;
        else if (actor.Position.X > 110)
            return actor.Position.Z > 100;
        else if (actor.Position.Z > 110)
            return actor.Position.X < 100;
        else
            return false;
    }
}
