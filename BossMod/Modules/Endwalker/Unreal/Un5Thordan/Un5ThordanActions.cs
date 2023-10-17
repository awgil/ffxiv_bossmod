using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BossMod.Endwalker.Unreal.Un5Thordan;

class AscalonsMight : Components.Cleave
{
    public AscalonsMight() : base(ActionID.MakeSpell(AID.AscalonsMight), new AOEShapeCone(10f, 45.Degrees()), (uint)OID.Thordan) { }
}

class Meteorain : Components.LocationTargetedAOEs
{
    public Meteorain() : base(ActionID.MakeSpell(AID.MeteorainAOE), 6) { }
}

class AscalonsMercy : Components.SelfTargetedAOEs
{
    public AscalonsMercy() : base(ActionID.MakeSpell(AID.AscalonsMercy), new AOEShapeCone(55.27f, 10.Degrees())) { }
}

class AscalonsMercyHelper : Components.SelfTargetedAOEs
{
    public AscalonsMercyHelper() : base(ActionID.MakeSpell(AID.AscalonsMercyHelper), new AOEShapeCone(55.27f, 10.Degrees())) { }
}

class DragonsGaze : Components.CastGaze
{
    public DragonsGaze() : base(ActionID.MakeSpell(AID.DragonsGaze)) { }
}

class LightningStorm : Components.UniformStackSpread
{
    public LightningStorm() : base(0, 5, 0, 1, alwaysShowSpreads: true) { }

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        if ((IconID)iconID == IconID.LightningStorm)
        {
            AddSpread(actor, module.WorldState.CurrentTime.AddSeconds(5.1f));
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.LightningStorm)
        {
            Spreads.Clear();
        }
    }
}

class BurningChains : BossComponent
{
    public Dictionary<Actor, Actor> Tethers = new();
    
    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (Tethers.Keys.Contains(actor) || Tethers.Values.Contains(actor))
            hints.Add("Break chains!");
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        foreach (var (source, target) in Tethers)
        {
            arena.AddLine(source.Position, target.Position, ArenaColor.Danger);
        }
    }

    public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.BurningChains)
        {
            Actor? target = module.WorldState.Actors.Find(tether.Target);
            if (target is not null)
            {
                Tethers.Add(source, target);
            }
        }
    }

    public override void OnUntethered(BossModule module, Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.BurningChains)
        {
            Tethers.Remove(source);
        }
    }
}

class SwordShieldOfTheHeavens : BossComponent
{
    public bool Active => !adelphel.IsDead || !janlenoux.IsDead;
    
    private Actor adelphel;
    private Actor janlenoux;
    
    public override void Init(BossModule module)
    {
        adelphel = module.Enemies(OID.Adelphel)[0];
        janlenoux = module.Enemies(OID.Janlenoux)[0];
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (adelphel.CastInfo != null
            && adelphel.CastInfo.TargetID == actor.InstanceID 
            && adelphel.CastInfo.IsSpell(AID.HolyBladedance)
            && adelphel.Statuses.Select(x => x.ID).Contains((uint)StatusID.SwordOfTheHeavens))
            hints.Add("Mitigate NOW!");
        if (janlenoux.CastInfo != null
            && janlenoux.CastInfo.TargetID == actor.InstanceID 
            && janlenoux.CastInfo.IsSpell(AID.HolyBladedance)
            && janlenoux.Statuses.Select(x => x.ID).Contains((uint)StatusID.SwordOfTheHeavens))
            hints.Add("Mitigate NOW!");
    }

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        if (!adelphel.IsDead && !janlenoux.IsDead && adelphel.Position.InCircle(janlenoux.Position, 10f))
        {
            hints.Add("Separate Adds!");
        }
        else if (adelphel.Statuses.Select(x => x.ID).Contains((uint)StatusID.SwordOfTheHeavens))
        {
            hints.Add("Focus on Ser Adelphel!");
        }
        else if (janlenoux.Statuses.Select(x => x.ID).Contains((uint)StatusID.SwordOfTheHeavens))
        {
            hints.Add("Focus on Ser Janlenoux!");
        }
    }
}

class Heavensflame : Components.LocationTargetedAOEs
{
    public Heavensflame() : base(ActionID.MakeSpell(AID.HeavensflameAOE), 6) { }
}

class Conviction : Components.CastTowers
{
    public Conviction() : base(ActionID.MakeSpell(AID.ConvictionAOE), 3f) { }
}

class SpiralThrust : Components.SelfTargetedAOEs
{
    public SpiralThrust() : base(ActionID.MakeSpell(AID.SpiralThrust), new AOEShapeRect(55.27f, 6f)) { }
}

class SkywardLeap : Components.UniformStackSpread
{
    // not sure about the spread radius, 15 seems to be enough but damage goes up to 20
    public SkywardLeap() : base(0, 20f, 0, 1, alwaysShowSpreads: true) { } 

    private Queue<Actor> baits = new();

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.SkywardLeap)
        {
            baits.Enqueue(actor);
            AddSpread(actor);
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SkywardLeap)
        {
            baits.Dequeue();
            Spreads.RemoveAll(x => !baits.Contains(x.Target));
        }
    }
}

class HoliestOfHoly : Components.RaidwideCast
{
    public HoliestOfHoly() : base(ActionID.MakeSpell(AID.HoliestOfHoly)) { }
}

class SpiralPierce : Components.BaitAwayTethers
{
    public SpiralPierce() : base(new AOEShapeRect(50f,6f), (uint)TetherID.SpiralPierce, ActionID.MakeSpell(AID.SpiralPierce))
    {
    }
}

class HiemalStorm : Components.PersistentVoidzone
{
    public HiemalStorm() : base(6,  m => m.Enemies(OID.HiemalStorm).Where(x => !x.IsDestroyed)) { }
}

class FaithUnmoving : Components.Knockback
{
    public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
    {
        return new[]
        {
            new Source(
                new WPos(0, 0),
                16f
            )
        };
    }
}