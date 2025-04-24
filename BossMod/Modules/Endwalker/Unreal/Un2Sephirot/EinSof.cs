﻿namespace BossMod.Endwalker.Unreal.Un2Sephirot;

class EinSof(BossModule module) : Components.GenericAOEs(module, AID.EinSofAOE)
{
    private readonly List<Actor> _active = [];

    private static readonly AOEShape _shape = new AOEShapeCircle(10); // TODO: verify radius

    public bool Active => _active.Count > 0;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return _active.Select(p => new AOEInstance(_shape, p.Position));
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        switch (state)
        {
            // 0x00100020 - become harmful, happens ~2.5s after appear and ~1.5s before first aoe
            // 0x00400080 - ??? (after 5th aoe)
            case 0x00040008: // appear as harmless
                _active.Add(actor);
                break;
            case 0x00010200: // disappear (happens ~1.2s after last aoe)
                _active.Remove(actor);
                break;
        }
    }
}
