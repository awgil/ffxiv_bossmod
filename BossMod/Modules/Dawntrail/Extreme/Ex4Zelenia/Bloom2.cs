namespace BossMod.Dawntrail.Extreme.Ex4Zelenia;

class AlexandrianThunderIV(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<Actor> Casters = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.AlexandrianThunderIVCircle:
            case AID.AlexandrianThunderIVDonut:
                Casters.Add(caster);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.AlexandrianThunderIVCircle or AID.AlexandrianThunderIVDonut)
        {
            NumCasts++;
            Casters.Remove(caster);
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Casters.Take(1).Select(csr => new AOEInstance((AID)csr.CastInfo!.Action.ID == AID.AlexandrianThunderIVCircle ? new AOEShapeCircle(8) : new AOEShapeDonut(8, 20), csr.Position, Activation: Module.CastFinishAt(csr.CastInfo)));
}

class ThunderSlash(BossModule module) : Components.StandardAOEs(module, AID.ThunderSlash, new AOEShapeCone(24, 30.Degrees()))
{
    private readonly Tiles Tiles = module.FindComponent<Tiles>()!;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        for (var i = 0; i < Math.Min(2, Casters.Count); i++)
        {
            var caster = Casters[i];
            yield return new AOEInstance(Shape, caster.CastInfo!.LocXZ, caster.CastInfo!.Rotation, Module.CastFinishAt(caster.CastInfo), Color: i == 0 ? ArenaColor.Danger : ArenaColor.AOE);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (Casters.Count > 0 && Tiles is { } t)
            hints.AddForbiddenZone(t.TileShape(), NumCasts > 0 ? default : Module.CastFinishAt(Casters[0].CastInfo));
    }
}

class ThunderHints(BossModule module) : Components.CastCounter(module, null)
{
    private readonly Tiles Tiles = module.FindComponent<Tiles>()!;

    private bool? OutFirst;

    private static readonly BitMask TilesOut = BitMask.Build(9, 10, 13, 14);
    private static readonly BitMask TilesIn = BitMask.Build(1, 2, 5, 6);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.AlexandrianThunderIVCircle:
                OutFirst ??= true;
                break;
            case AID.AlexandrianThunderIVDonut:
                OutFirst ??= false;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.AlexandrianThunderIVCircle or AID.AlexandrianThunderIVDonut)
            NumCasts++;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (NumCasts == 0 && OutFirst is { } b)
        {
            var safeTiles = (b ? TilesOut : TilesIn) & ~Tiles.Mask;
            var safeTile = safeTiles.SetBits().First(bit => !safeTiles[bit - 1] && !safeTiles[bit + 1]);
            if (safeTile == 0)
            {
                ReportError("Unable to find safe tile for pattern");
                return;
            }
            var angle = Tiles.GetTileOrientation(safeTile);
            if ((safeTile % 8) < 4)
                angle += 15.Degrees();
            else
                angle -= 15.Degrees();

            var safeDir = angle.ToDirection() * (safeTile < 8 ? 6.7f : 9.3f);

            Arena.AddCircle(Arena.Center + safeDir, 0.6f, ArenaColor.Safe);
        }
    }
}
