namespace BossMod.RealmReborn.Extreme.Ex1Ultima;

// AI idea: we want to run as a group and pop all orbs, without necessarily involving tanks
// for 1/2 casts, we first try to stack S of boss, since everyone is somewhat close to that point, to get knocked back to the south edge; we then pop S orb and E orb(s) in order S->N
// for 3 cast, we immune knockbacks and stack where two south orbs spawn to immediately handle two pairs; we then run to pop N orbs
class AethericBoom(BossModule module) : Components.CastHint(module, AID.AethericBoom, "Knockback + orbs")
{
    private bool _waitingForOrbs;
    private readonly List<Actor> _activeOrbs = [];
    private readonly List<Actor> _orbsToPop = [];
    public bool OrbsActive => _waitingForOrbs || _orbsToPop.Count > 0;

    private const float _explosionRadius = 8;

    public override void Update()
    {
        // cleanup
        _orbsToPop.RemoveAll(a => a.IsDestroyed);
        _activeOrbs.RemoveAll(a => a.IsDestroyed);

        if (!_waitingForOrbs && Active)
            _waitingForOrbs = true;

        if (_waitingForOrbs)
        {
            var orbs = Module.Enemies(OID.Ultimaplasm);
            if (orbs.Count == 2 * (NumCasts + 1)) // 4/6/8 orbs should spawn after 1/2/3 casts
            {
                _activeOrbs.AddRange(orbs);
                switch (NumCasts)
                {
                    case 1:
                        _orbsToPop.AddRange(orbs.Where(a => a.PosRot.Z > 17)); // S orb
                        _orbsToPop.AddRange(orbs.Where(a => a.PosRot.X > 17)); // E orb
                        break;
                    case 2:
                        _orbsToPop.AddRange(orbs.Where(a => a.PosRot.Z > 8)); // S orb
                        _orbsToPop.AddRange(orbs.Where(a => a.PosRot.X > 15 && a.PosRot.Z > 0)); // E/S orb
                        _orbsToPop.AddRange(orbs.Where(a => a.PosRot.X > 15 && a.PosRot.Z < 0)); // E/N orb
                        break;
                    case 3:
                        _orbsToPop.AddRange(orbs.Where(a => a.PosRot.Z > 9 && a.PosRot.X < 0)); // S/W orb
                        _orbsToPop.AddRange(orbs.Where(a => a.PosRot.Z > 9 && a.PosRot.X > 0)); // S/E orb
                        _orbsToPop.AddRange(orbs.Where(a => a.PosRot.Z < -9 && a.PosRot.X > 0)); // N/E orb
                        _orbsToPop.AddRange(orbs.Where(a => a.PosRot.Z < -9 && a.PosRot.X < 0)); // N/W orb
                        break;
                }
                _waitingForOrbs = false;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!OrbsActive)
            return;

        if (Module.PrimaryActor.TargetID == actor.InstanceID)
        {
            // current MT should not be doing this mechanic
            foreach (var orb in _activeOrbs)
                hints.AddForbiddenZone(ShapeContains.Circle(orb.Position, 3));
            return;
        }

        if (Active)
        {
            if (NumCasts < 2)
            {
                // first or second cast in progress => stack S of boss to be knocked back roughly in same direction
                hints.AddForbiddenZone(ShapeContains.Cone(Module.PrimaryActor.Position, 50, 180.Degrees(), 170.Degrees()));
            }
            else
            {
                // third cast in progress => immune knockback and go to resolve positions
                hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.ArmsLength), actor, ActionQueue.Priority.High);
                hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Surecast), actor, ActionQueue.Priority.High);
                PrepositionForOrbs(hints, assignment, 3);
            }
        }
        else if (_waitingForOrbs || NumCasts == 3)
        {
            // preposition while waiting for orbs (or for static positions at third orbs)
            PrepositionForOrbs(hints, assignment, NumCasts);
        }
        else
        {
            // run to pop next orb
            var nextOrb = _orbsToPop[0];
            if (actor.Role is Role.Melee or Role.Tank && Raid.WithoutSlot().InRadius(nextOrb.Position, _explosionRadius).Count() > 5)
            {
                // pop the orb
                hints.AddForbiddenZone(ShapeContains.InvertedCircle(nextOrb.Position, 1.5f));
            }
            else
            {
                // run closer to the orb
                hints.AddForbiddenZone(ShapeContains.InvertedCircle(nextOrb.Position + nextOrb.Rotation.ToDirection(), _explosionRadius - 2));
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var orb in _activeOrbs)
        {
            Arena.Actor(orb, ArenaColor.Object, true);
            Arena.AddCircle(orb.Position, _explosionRadius, ArenaColor.Danger);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if ((AID)spell.Action.ID == AID.AetheroplasmBoom)
        {
            _activeOrbs.Remove(caster);
            _orbsToPop.Remove(caster);
        }
    }

    private void PrepositionForOrbs(AIHints hints, PartyRolesConfig.Assignment assignment, int orbsCount)
    {
        float x = assignment is PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.H1 or PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.R1 ? 1 : -1;
        if (orbsCount == 3 && assignment is PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.M2)
        {
            // sacrifice melees on side orbs, this sucks but whatever
            hints.AddForbiddenZone(ShapeContains.InvertedCircle(new(10 * x, -2), 1.5f));
        }
        else
        {
            hints.AddForbiddenZone(ShapeContains.InvertedCircle(new(2 * x, 10), 1.5f));
        }
    }
}
