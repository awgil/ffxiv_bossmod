namespace BossMod.Endwalker.Savage.P2SHippokampos;

// state related to channeling [over]flow mechanics
class ChannelingFlow : BossComponent
{
    public int NumStunned { get; private set; }
    private (WDir, DateTime)[] _arrows = new (WDir, DateTime)[PartyState.MaxPartySize];

    private static readonly float _typhoonHalfWidth = 2.5f;

    public bool SlotActive(BossModule module, int slot)
    {
        var (dir, expire) = _arrows[slot];
        return dir != new WDir() && (expire - module.WorldState.CurrentTime).TotalSeconds < 13;
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        Actor? partner = null;
        if (SlotActive(module, slot))
        {
            int numPartners = 0, numClipped = 0;
            var partnerDir = -_arrows[slot].Item1;
            float minDistance = 50;
            foreach (var (otherSlot, otherActor) in ActorsHitBy(module, slot, actor))
            {
                if (_arrows[otherSlot].Item1 == partnerDir)
                {
                    minDistance = MathF.Min(minDistance, partnerDir.Dot(actor.Position - otherActor.Position));
                    ++numPartners;
                    partner = otherActor;
                }
                else
                {
                    ++numClipped;
                }
            }

            if (numPartners == 0)
                hints.Add("Aim to hit partner!");
            if (numPartners > 1 || numClipped > 0)
                hints.Add("Avoid clipping irrelevant players!");
            if (minDistance < 20) // TODO: verify min range
                hints.Add("Too close to partner!");
        }

        if (ActiveArrows(module).Any(pd => pd.Item1 != actor && pd.Item1 != partner && actor.Position.InRect(pd.Item1.Position, pd.Item2, 50, 0, _typhoonHalfWidth)))
            hints.Add("GTFO from imminent flow!");
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        foreach (var (player, dir) in ActiveArrows(module))
        {
            arena.ZoneRect(player.Position, dir, 50, 0, _typhoonHalfWidth, ArenaColor.AOE);
        }
    }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.MarkFlowN:
                SetArrow(module, actor, new(0, -1), status.ExpireAt);
                break;
            case SID.MarkFlowS:
                SetArrow(module, actor, new(0, +1), status.ExpireAt);
                break;
            case SID.MarkFlowW:
                SetArrow(module, actor, new(-1, 0), status.ExpireAt);
                break;
            case SID.MarkFlowE:
                SetArrow(module, actor, new(+1, 0), status.ExpireAt);
                break;
            case SID.Stun:
                ++NumStunned;
                break;
        }
    }

    public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.MarkFlowN:
            case SID.MarkFlowS:
            case SID.MarkFlowW:
            case SID.MarkFlowE:
                SetArrow(module, actor, new(), new());
                break;
            case SID.Stun:
                --NumStunned;
                break;
        }
    }

    private void SetArrow(BossModule module, Actor actor, WDir dir, DateTime expire)
    {
        var slot = module.WorldState.Party.FindSlot(actor.InstanceID);
        if (slot >= 0)
            _arrows[slot] = (dir, expire);
    }

    private IEnumerable<(Actor, WDir)> ActiveArrows(BossModule module)
    {
        return module.Raid.WithSlot().WhereSlot(slot => SlotActive(module, slot)).Select(ia => (ia.Item2, _arrows[ia.Item1].Item1));
    }

    private IEnumerable<(int, Actor)> ActorsHitBy(BossModule module, int slot, Actor actor)
    {
        return module.Raid.WithSlot().Exclude(slot).WhereActor(a => a.Position.InRect(actor.Position, _arrows[slot].Item1, 50, 0, _typhoonHalfWidth));
    }
}
