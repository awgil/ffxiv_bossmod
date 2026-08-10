namespace BossMod.Endwalker.VariantCriterion.C01ASS.C011Silkie;

static class SlipperySoap
{
    public enum Color { None, Green, Blue, Yellow }

    public static Color ColorForStatus(uint sid) => sid switch
    {
        (uint)SID.BracingSudsBoss => Color.Green,
        (uint)SID.ChillingSudsBoss => Color.Blue,
        (uint)SID.FizzlingSudsBoss => Color.Yellow,
        _ => Color.None
    };
}

sealed class SlipperySoapCharge(BossModule module) : Components.GenericKnockback(module)
{
    private Actor? _chargeTarget;
    private Angle _chargeDir;
    private AOEShapeRect _chargeShape = new(default, 5f);
    private SlipperySoap.Color _color;
    private DateTime _chargeResolve;

    public bool ChargeImminent => _chargeTarget != null;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (_chargeTarget != null && _color == SlipperySoap.Color.Green)
            return new Knockback[1] { new(Module.PrimaryActor.Position, 15f, _chargeResolve, _chargeShape, _chargeDir, Kind.DirForward) };
        else
            return [];
    }

    public override void Update()
    {
        if (_chargeTarget != null)
        {
            var toTarget = _chargeTarget.Position - Module.PrimaryActor.Position;
            var len = toTarget.Length() + 0.01f; // add eps to ensure charge target is considered 'inside'
            if (_chargeShape.LengthFront != len)
                _chargeShape = new AOEShapeRect(len, 5f);
            _chargeDir = Angle.FromDirection(toTarget); // keep shape's offset zero to properly support dir-forward
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (_chargeTarget != null)
        {
            if (_chargeTarget != actor && !_chargeShape.Check(actor.Position, Module.PrimaryActor.Position, _chargeDir))
                hints.Add("Stack inside charge!");

            switch (_color)
            {
                case SlipperySoap.Color.Blue:
                    hints.Add("Move!", false);
                    break;
                case SlipperySoap.Color.Yellow:
                    hints.Add("Prepare to spread", false);
                    break;
            }
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_chargeTarget != null)
            _chargeShape.Draw(Arena, Module.PrimaryActor.Position, _chargeDir, Colors.SafeFromAOE);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        base.OnStatusGain(actor, ref status);
        if (actor != Module.PrimaryActor)
            return;
        var color = SlipperySoap.ColorForStatus(status.ID);
        if (color != SlipperySoap.Color.None)
            _color = color;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        switch (spell.Action.ID)
        {
            case (uint)AID.SlipperySoapTargetSelection:
                _chargeTarget = WorldState.Actors.Find(spell.MainTargetID);
                _chargeResolve = WorldState.FutureTime(5.5d);
                break;
            case (uint)AID.NSlipperySoapAOEBlue:
            case (uint)AID.NSlipperySoapAOEGreen:
            case (uint)AID.NSlipperySoapAOEYellow:
            case (uint)AID.SSlipperySoapAOEBlue:
            case (uint)AID.SSlipperySoapAOEGreen:
            case (uint)AID.SSlipperySoapAOEYellow:
                _chargeTarget = null;
                break;
        }
    }
}

sealed class SlipperySoapAOE(BossModule module) : Components.GenericAOEs(module)
{
    private SlipperySoap.Color _color;
    private readonly AOEShapeCross shapeBlue = new(60f, 5f);
    private readonly AOEShapeDonut shapeGreen = new(5f, 60f);
    private readonly AOEShapeCone shapeYellow = new(60f, 22.5f.Degrees());

    public bool Active => _color != SlipperySoap.Color.None;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        // TODO: activation

        switch (_color)
        {
            case SlipperySoap.Color.Green:
                return new AOEInstance[1] { new(shapeGreen, Module.PrimaryActor.Position, Module.PrimaryActor.Rotation) };
            case SlipperySoap.Color.Blue:
                return new AOEInstance[1] { new(shapeBlue, Module.PrimaryActor.Position, Module.PrimaryActor.Rotation) };
            case SlipperySoap.Color.Yellow:
                var aoes = new AOEInstance[4];
                var primaryPos = Module.PrimaryActor.Position;
                var primaryRot = Module.PrimaryActor.Rotation;
                for (var i = 0; i < 4; ++i)
                {
                    aoes[i] = new AOEInstance(shapeYellow, primaryPos, primaryRot + (45f + i * 90f).Degrees());
                }
                return aoes;
            default:
                return [];
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (actor != Module.PrimaryActor)
            return;
        var color = SlipperySoap.ColorForStatus(status.ID);
        if (color != SlipperySoap.Color.None)
            _color = color;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        _color = spell.Action.ID switch
        {
            (uint)AID.NChillingDusterBoss or
            (uint)AID.NBracingDusterBoss or
            (uint)AID.NFizzlingDusterBoss or
            (uint)AID.SChillingDusterBoss or
            (uint)AID.SBracingDusterBoss or
            (uint)AID.SFizzlingDusterBoss => SlipperySoap.Color.None,
            _ => _color
        };
    }
}

// note: we don't wait for forked lightning statuses to appear
sealed class SoapsudStatic(BossModule module) : Components.UniformStackSpread(module, default, 5f)
{
    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (actor == Module.PrimaryActor && SlipperySoap.ColorForStatus(status.ID) == SlipperySoap.Color.Yellow)
            AddSpreads(Raid.WithoutSlot(true, true, true));
    }
}
