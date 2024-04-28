namespace BossMod.Endwalker.Unreal.Un4Zurvan;

class P2SoarTwinSpirit(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Actor caster, AOEInstance aoe)> _pending = [];

    private readonly AOEShapeRect _shape = new(50, 5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _pending.Select(p => p.aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.TwinSpiritFirst)
        {
            _pending.Add((caster, new(_shape, caster.Position, Angle.FromDirection(spell.LocXZ - caster.Position), spell.NPCFinishAt)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.TwinSpiritFirst:
                var index = _pending.FindIndex(p => p.caster == caster);
                if (index >= 0)
                    _pending[index] = (caster, new(_shape, spell.LocXZ, Angle.FromDirection(Module.Center - spell.LocXZ), WorldState.FutureTime(9.2f)));
                break;
            case AID.TwinSpiritSecond:
                _pending.RemoveAll(p => p.caster == caster);
                ++NumCasts;
                break;
        }
    }
}

class P2SoarFlamingHalberd(BossModule module) : Components.UniformStackSpread(module, 0, 12, alwaysShowSpreads: true)
{
    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.FlamingHalberd)
            AddSpread(actor, WorldState.FutureTime(5.1f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.FlamingHalberd)
            Spreads.Clear(); // don't bother finding proper target, they all happen at the same time
    }
}

class P2SoarFlamingHalberdVoidzone(BossModule module) : Components.PersistentVoidzone(module, 8, m => m.Enemies(OID.FlamingHalberdVoidzone).Where(z => z.EventState != 7));

class P2SoarDemonicDiveCoolFlame(BossModule module) : Components.UniformStackSpread(module, 7, 8, 7, alwaysShowSpreads: true)
{
    public override void OnEventIcon(Actor actor, uint iconID)
    {
        switch ((IconID)iconID)
        {
            case IconID.DemonicDive:
                AddStack(actor, WorldState.FutureTime(5.1f));
                break;
            case IconID.CoolFlame:
                AddSpread(actor, WorldState.FutureTime(5.1f));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.DemonicDive:
                Stacks.Clear();
                break;
            case AID.CoolFlame:
                Spreads.Clear();
                break;
        }
    }
}
