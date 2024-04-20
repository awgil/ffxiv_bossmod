namespace BossMod.Endwalker.Savage.P4S2Hesperos;

// state related to act 2 wreath of thorns
// note: there should be four tethered helpers on activation
// note: we assume that (1) dark targets soak all towers, (2) first fire to be broken is tank-healer pair (since their debuff is slightly shorter)
class WreathOfThorns2(BossModule module) : BossComponent(module)
{
    public enum State { DarkDesign, FirstSet, SecondSet, Done }

    public State CurState { get; private set; } = State.DarkDesign;
    private readonly List<Actor> _relevantHelpers = []; // 2 aoes -> 8 towers -> 2 aoes
    private (Actor?, Actor?) _darkTH; // first is one having tether
    private (Actor?, Actor?) _fireTH;
    private (Actor?, Actor?) _fireDD;
    private readonly IconID[] _playerIcons = new IconID[8];
    private int _numAOECasts;

    private IEnumerable<Actor> FirstSet => _relevantHelpers.Take(4);
    private IEnumerable<Actor> SecondSet => _relevantHelpers.Skip(4);

    private const float _fireExplosionRadius = 6;

    public override void AddHints(int slot, Actor actor, TextHints hints)
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
                var relevantHelpers = CurState == State.FirstSet ? FirstSet : SecondSet;
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

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (CurState == State.Done)
            return;

        foreach (var aoe in (CurState == State.SecondSet ? SecondSet : FirstSet).Where(IsAOE))
            Arena.ZoneCircle(aoe.Position, P4S2.WreathAOERadius, ArenaColor.AOE);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        // draw players
        foreach (var player in Raid.WithoutSlot().Exclude(pc))
            Arena.Actor(player, ArenaColor.PlayerGeneric);

        // draw pc's tether
        var pcPartner = pc.Tether.Target != 0
            ? WorldState.Actors.Find(pc.Tether.Target)
            : Raid.WithoutSlot().FirstOrDefault(p => p.Tether.Target == pc.InstanceID);
        if (pcPartner != null)
        {
            var tetherColor = _playerIcons[pcSlot] switch
            {
                IconID.AkanthaiFire => 0xff00ffff,
                IconID.AkanthaiWind => 0xff00ff00,
                _ => 0xffff00ff
            };
            Arena.AddLine(pc.Position, pcPartner.Position, tetherColor);
        }

        // draw towers for designated tower soakers
        bool isTowerSoaker = pc == _darkTH.Item1 || pc == _darkTH.Item2;
        if (isTowerSoaker && CurState != State.Done)
            foreach (var tower in (CurState == State.SecondSet ? SecondSet : FirstSet).Where(IsTower))
                Arena.AddCircle(tower.Position, P4S2.WreathTowerRadius, CurState == State.DarkDesign ? ArenaColor.Danger : ArenaColor.Safe);

        // draw circles around next imminent fire explosion
        if (CurState != State.DarkDesign)
        {
            var curFirePair = (_fireTH.Item1 != null && _fireTH.Item1.Tether.ID != 0) ? _fireTH : ((_fireDD.Item1 != null && _fireDD.Item1.Tether.ID != 0) ? _fireDD : (null, null));
            if (curFirePair.Item1 != null)
            {
                Arena.AddCircle(curFirePair.Item1!.Position, _fireExplosionRadius, isTowerSoaker ? ArenaColor.Danger : ArenaColor.Safe);
                Arena.AddCircle(curFirePair.Item2!.Position, _fireExplosionRadius, isTowerSoaker ? ArenaColor.Danger : ArenaColor.Safe);
            }
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (source.OID == (uint)OID.Helper && tether.ID == (uint)TetherID.WreathOfThorns)
        {
            _relevantHelpers.Add(source);
        }
        else if (source.Type == ActorType.Player)
        {
            PlayerTetherOrIconAssigned(Raid.FindSlot(source.InstanceID), source);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (CurState == State.DarkDesign && (AID)spell.Action.ID == AID.DarkDesign)
            CurState = State.FirstSet;
        else if (CurState == State.FirstSet && (AID)spell.Action.ID == AID.AkanthaiExplodeAOE && ++_numAOECasts >= 2)
            CurState = State.SecondSet;
        else if (CurState == State.SecondSet && (AID)spell.Action.ID == AID.AkanthaiExplodeAOE && ++_numAOECasts >= 4)
            CurState = State.Done;
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot == -1)
            return;

        _playerIcons[slot] = (IconID)iconID;
        PlayerTetherOrIconAssigned(slot, actor);
    }

    private void PlayerTetherOrIconAssigned(int slot, Actor actor)
    {
        if (slot == -1 || _playerIcons[slot] == IconID.None || actor.Tether.Target == 0)
            return; // icon or tether not assigned yet

        var tetherTarget = WorldState.Actors.Find(actor.Tether.Target);
        if (tetherTarget == null)
            return; // weird

        if (_playerIcons[slot] == IconID.AkanthaiDark)
        {
            _darkTH = (actor, tetherTarget);
        }
        else if (_playerIcons[slot] == IconID.AkanthaiFire)
        {
            if (actor.Role is Role.Tank or Role.Healer)
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
