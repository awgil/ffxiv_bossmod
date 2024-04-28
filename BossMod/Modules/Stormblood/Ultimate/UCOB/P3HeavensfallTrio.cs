namespace BossMod.Stormblood.Ultimate.UCOB;

class P3HeavensfallTrio(BossModule module) : BossComponent(module)
{
    private Actor? _nael;
    private Actor? _twin;
    private Actor? _baha;
    private readonly WPos[] _safeSpots = new WPos[PartyState.MaxPartySize];

    public bool Active => _nael != null;

    private static readonly Angle[] _offsetsNaelCenter = [10.Degrees(), 80.Degrees(), 100.Degrees(), 170.Degrees()];
    private static readonly Angle[] _offsetsNaelSide = [60.Degrees(), 80.Degrees(), 100.Degrees(), 120.Degrees()];

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actor(_nael, ArenaColor.Object, true);
        var safespot = _safeSpots[pcSlot];
        if (safespot != default)
            Arena.AddCircle(safespot, 1, ArenaColor.Safe);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.NaelDeusDarnus && id == 0x1E43)
        {
            _nael = actor;
            InitIfReady();
        }
        else if ((OID)actor.OID == OID.Twintania && id == 0x1E44)
        {
            _twin = actor;
            InitIfReady();
        }
        else if ((OID)actor.OID == OID.BahamutPrime && id == 0x1E43)
        {
            _baha = actor;
            InitIfReady();
        }
    }

    private void InitIfReady()
    {
        if (_nael == null || _twin == null || _baha == null)
            return;

        var dirToNael = Angle.FromDirection(_nael.Position - Module.Center);
        var dirToTwin = Angle.FromDirection(_twin.Position - Module.Center);
        var dirToBaha = Angle.FromDirection(_baha.Position - Module.Center);

        var twinRel = (dirToTwin - dirToNael).Normalized();
        var bahaRel = (dirToBaha - dirToNael).Normalized();
        var (offsetSymmetry, offsets) = twinRel.Rad * bahaRel.Rad < 0 // twintania & bahamut are on different sides => nael is in center
            ? (0.Degrees(), _offsetsNaelCenter)
            : ((twinRel + bahaRel) * 0.5f, _offsetsNaelSide);
        var dirSymmetry = dirToNael + offsetSymmetry;
        foreach (var p in Service.Config.Get<UCOBConfig>().P3QuickmarchTrioAssignments.Resolve(Raid))
        {
            bool left = p.group < 4;
            int order = p.group & 3;
            var offset = offsets[order];
            var dir = dirSymmetry + (left ? offset : -offset);
            _safeSpots[p.slot] = Module.Center + 20 * dir.ToDirection();
        }
    }
}

class P3HeavensfallTowers(BossModule module) : Components.CastTowers(module, ActionID.MakeSpell(AID.MegaflareTower), 3)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (spell.Action == WatchedAction && Towers.Count == 8)
        {
            var nael = Module.Enemies(OID.NaelDeusDarnus).FirstOrDefault();
            if (nael != null)
            {
                var dirToNael = Angle.FromDirection(nael.Position - Module.Center);
                var orders = Towers.Select(t => TowerSortKey(Angle.FromDirection(t.Position - Module.Center), dirToNael)).ToList();
                MemoryExtensions.Sort(orders.AsSpan(), Towers.AsSpan());
                foreach (var p in Service.Config.Get<UCOBConfig>().P3HeavensfallTrioTowers.Resolve(Raid))
                {
                    Towers.Ref(p.group).ForbiddenSoakers = new(~(1ul << p.slot));
                }
            }
        }
    }

    // order towers from nael's position CW
    private float TowerSortKey(Angle tower, Angle reference)
    {
        var cwDist = (reference - tower).Normalized().Deg;
        if (cwDist < -5f) // towers are ~22.5 degrees apart
            cwDist += 360;
        return cwDist;
    }
}

class P3HeavensfallFireball(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Fireball, ActionID.MakeSpell(AID.Fireball), 4, 5.3f, 8);
