namespace BossMod.Dawntrail.Extreme.Ex6GuardianArkveld;

class ChainbladeTail(BossModule module) : Components.GroupedAOEs(module, [AID.ChainbladeTail1, AID.ChainbladeTail2, AID.ChainbladeTail3, AID.ChainbladeTail4, AID.ChainbladeTail5, AID.ChainbladeTail6, AID.ChainbladeTail7, AID.ChainbladeTail8], new AOEShapeRect(40, 2));

class ChainbladeSide(BossModule module) : Components.GroupedAOEs(module, [AID.ChainbladeSide2, AID.ChainbladeSide1, AID.ChainbladeSide4, AID.ChainbladeSide3], new AOEShapeRect(80, 14));

class ChainbladeRepeat(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AID[] TailsCast = [AID.ChainbladeTail1, AID.ChainbladeTail2, AID.ChainbladeTail3, AID.ChainbladeTail4, AID.ChainbladeTail5, AID.ChainbladeTail6, AID.ChainbladeTail7, AID.ChainbladeTail8];
    private static readonly AID[] BossCast = [AID.ChainbladeSide1, AID.ChainbladeSide2, AID.ChainbladeSide3, AID.ChainbladeSide4];

    private static readonly AID[] TailsFast = [AID.ChainbladeTailFast1, AID.ChainbladeTailFast2, AID.ChainbladeTailFast3, AID.ChainbladeTailFast4, AID.ChainbladeTailFast5, AID.ChainbladeTailFast6, AID.ChainbladeTailFast7, AID.ChainbladeTailFast8];
    private static readonly AID[] BossFast = [AID.ChainbladeSideFast1, AID.ChainbladeSideFast2, AID.ChainblideSideFast3, AID.ChainbladeSideFast4];

    private static readonly AOEShape TailShape = new AOEShapeRect(40, 2);
    private static readonly AOEShape CleaveShape = new AOEShapeRect(80, 14);

    public bool Draw;

    private readonly List<AOEInstance> _predicted = [];
    private readonly List<(Actor Caster, ActorCastInfo Spell, AOEShape Shape)> _pending = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Draw ? _predicted : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ChainbladeBossCast2 or AID.ChainbladeBossCast1 or AID.ChainbladeBossCast4 or AID.ChainbladeBossCast3)
        {
            foreach (var p in _pending)
                HandleCastStart(p.Caster, p.Spell, p.Shape);
            _pending.Clear();
            return;
        }

        var id = (AID)spell.Action.ID;
        var shape = TailsCast.Contains(id)
            ? TailShape
            : BossCast.Contains(id)
                ? CleaveShape
                : null;

        if (shape == null)
            return;

        HandleCastStart(caster, spell, shape);
    }

    private void HandleCastStart(Actor caster, ActorCastInfo spell, AOEShape shape)
    {
        if (Module.PrimaryActor.CastInfo is not { } ci)
        {
            _pending.Add((caster, spell, shape));
            return;
        }

        var n = ci.Rotation.ToDirection().OrthoR();
        var d0 = spell.Rotation.ToDirection();
        var angle = d0 - d0.Dot(n) * n * 2;

        var d1 = spell.LocXZ - Module.PrimaryActor.Position;
        var src = d1 - d1.Dot(n) * n * 2;

        _predicted.Add(new(shape, Module.PrimaryActor.Position + src, angle.ToAngle(), Module.CastFinishAt(spell, 4)));
        _predicted.SortBy(p => p.Activation);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var id = (AID)spell.Action.ID;

        if (BossCast.Contains(id))
            Draw = true;

        if (TailsFast.Contains(id))
            if (_predicted.Count > 0)
                _predicted.RemoveAt(0);

        if (BossFast.Contains(id))
        {
            NumCasts++;
            Draw = false;
            if (_predicted.Count > 0)
                _predicted.RemoveAt(0);
        }
    }
}
