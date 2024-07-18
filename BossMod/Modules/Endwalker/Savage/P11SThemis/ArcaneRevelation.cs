﻿namespace BossMod.Endwalker.Savage.P11SThemis;

// mirrors & spheres
class ArcaneRevelation(BossModule module) : Components.GenericAOEs(module)
{
    private uint _activeMirrors;
    private uint _activeSpheres;
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shapeMirror = new(50, 5);
    private static readonly AOEShapeCircle _shapeSphere = new(15);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var (mirrors, spheres) = (AID)spell.Action.ID switch
        {
            AID.ArcaneRevelationMirrorsLight => ((uint)OID.MirrorLight, 0u),
            AID.ArcaneRevelationMirrorsDark => ((uint)OID.MirrorDark, 0u),
            AID.ArcaneRevelationSpheresLight => (0u, (uint)OID.SphereLight),
            AID.ArcaneRevelationSpheresDark => (0u, (uint)OID.SphereDark),
            AID.TwofoldRevelationLight => ((uint)OID.MirrorLight, (uint)OID.SphereLight),
            AID.TwofoldRevelationDark => ((uint)OID.MirrorDark, (uint)OID.SphereDark),
            _ => (0u, 0u)
        };
        if (mirrors != 0 || spheres != 0)
        {
            _activeMirrors = mirrors;
            _activeSpheres = spheres;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ArcheLight or AID.ArcheDark or AID.UnluckyLotLight or AID.UnluckyLotDark)
        {
            ++NumCasts;
            _aoes.Clear();
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id != 0x1E46)
            return;
        if (actor.OID == _activeMirrors)
            _aoes.Add(new(_shapeMirror, actor.Position, actor.Rotation, WorldState.FutureTime(8.7f)));
        else if (actor.OID == _activeSpheres)
            _aoes.Add(new(_shapeSphere, actor.Position, actor.Rotation, WorldState.FutureTime(8.7f)));
    }
}

class DismissalOverruling(BossModule module) : Components.Knockback(module)
{
    private Actor? _source;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_source != null)
            yield return new(_source.Position, 11, Module.CastFinishAt(_source.CastInfo));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.DismissalOverrulingLightAOE or AID.DismissalOverrulingDarkAOE)
            _source = caster;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.DismissalOverrulingLightAOE or AID.DismissalOverrulingDarkAOE)
        {
            _source = null;
            ++NumCasts;
        }
    }
}

class InnerLight(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.InnerLight), new AOEShapeCircle(13));
class OuterDark(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.OuterDark), new AOEShapeDonut(8, 50));
