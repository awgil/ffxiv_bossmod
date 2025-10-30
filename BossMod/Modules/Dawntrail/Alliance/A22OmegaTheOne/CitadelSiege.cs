namespace BossMod.Dawntrail.Alliance.A22OmegaTheOne;

class ArenaBounds(BossModule module) : BossComponent(module)
{
    public bool Ship2 { get; private set; }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x25 && state == 0x00020001)
        {
            Ship2 = true;
            Arena.Center = new(735, 800);
            Arena.Bounds = new ArenaBoundsRect(20, 23.8f);
        }
    }
}

class MultiMissile1(BossModule module) : Components.StandardAOEs(module, AID.MultiMissileSmall, 6);
class MultiMissile2(BossModule module) : Components.StandardAOEs(module, AID.MultiMissileBig, 10);
class CitadelSiege(BossModule module) : Components.StandardAOEs(module, AID.CitadelSiegeRect, new AOEShapeRect(48, 5));
class CitadelSiegeArena(BossModule module) : Components.GenericAOEs(module)
{
    private BitMask _arena;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var b in _arena.SetBits())
        {
            var center = new WPos(815 - 10 * b, 800);
            yield return new(new AOEShapeRect(24, 5, 24), center);
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x18)
        {
            switch (state)
            {
                case 0x00020001:
                    _arena.Set(0);
                    break;
                case 0x00200010:
                    _arena.Set(1);
                    break;
                case 0x00800040:
                    _arena.Set(2);
                    break;
                case 0x02000100:
                    _arena.Set(3);
                    break;
            }
        }
    }
}

class CitadelSiegeJump(BossModule module) : BossComponent(module)
{
    private DateTime _deadline;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x19 && state == 0x00020001)
        {
            _deadline = WorldState.FutureTime(21.3f);

            var arenaRect = CurveApprox.Rect(new(20, 0), new(0, 23.8f));
            var doubleCenter = new WPos(767.5f, 800);

            var b1 = new RelSimplifiedComplexPolygon([new RelPolygonWithHoles([.. arenaRect.Select(r => r + new WDir(32.5f, 0))]), new RelPolygonWithHoles([.. arenaRect.Select(r => r - new WDir(32.5f, 0))])]);
            Arena.Center = doubleCenter;
            Arena.Bounds = new ArenaBoundsCustom(52.5f, b1);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_deadline != default && pc.Position.X >= 780)
            Arena.ZoneRect(new WPos(780, 800), new WPos(785, 800), 24, ArenaColor.SafeFromAOE);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_deadline != default && actor.Position.X >= 780)
            hints.Add("Jump!", false);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_deadline != default && actor.Position.X >= 780)
            hints.AddForbiddenZone(p => p.X > 785, _deadline);
    }
}
