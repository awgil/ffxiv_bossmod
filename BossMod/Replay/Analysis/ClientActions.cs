namespace BossMod.ReplayAnalysis;

class ClientActions
{
    private List<(Replay r, DateTime ts, string warning)> _warnings = new();

    public ClientActions(List<Replay> replays)
    {
        foreach (var r in replays)
        {
            List<ClientState.OpActionRequest> pendingRequests = new();
            ulong playerID = 0;
            ActorCastInfo? pendingCast = null;
            foreach (var op in r.Ops)
            {
                switch (op)
                {
                    case PartyState.OpModify o:
                        if (o.Slot == PartyState.PlayerSlot)
                        {
                            playerID = o.InstanceID;
                            // TODO: is that correct?..
                            pendingRequests.Clear();
                            pendingCast = null;
                        }
                        break;

                    case ClientState.OpActionRequest o:
                        if (pendingCast != null && pendingRequests.Count > 0 && pendingCast.Action == pendingRequests[0].Request.Action)
                        {
                            // cast-end will arrive later
                            pendingCast = null;
                            pendingRequests.RemoveAt(0);
                        }
                        if (pendingRequests.Count > 0)
                        {
                            _warnings.Add((r, op.Timestamp, $"New action request {StrReq(o.Request)} while {pendingRequests.Count} are pending (first = {StrReq(pendingRequests[0].Request)}, {(op.Timestamp - pendingRequests[0].Timestamp).TotalSeconds:f3}s ago)"));
                        }
                        pendingRequests.Add(o);
                        break;

                    case ClientState.OpActionReject o:
                        int rejIndex = o.Value.SourceSequence != 0
                            ? pendingRequests.FindIndex(a => a.Request.SourceSequence == o.Value.SourceSequence)
                            : pendingRequests.FindIndex(a => a.Request.Action == o.Value.Action);
                        if (rejIndex < 0)
                        {
                            _warnings.Add((r, op.Timestamp, $"Reject {StrReject(o.Value)}: unexpected (not found); currently {pendingRequests.Count} are pending"));
                            pendingRequests.Clear();
                        }
                        else if (rejIndex > 0)
                        {
                            _warnings.Add((r, op.Timestamp, $"Reject {StrReject(o.Value)}: unexpected (index {rejIndex}); currently {pendingRequests.Count} are pending, first={StrReq(pendingRequests[0].Request)}"));
                            pendingRequests.RemoveRange(0, rejIndex + 1);
                        }
                        else if (pendingRequests[0].Request.SourceSequence != o.Value.SourceSequence || pendingRequests[0].Request.Action != o.Value.Action)
                        {
                            _warnings.Add((r, op.Timestamp, $"Reject {StrReject(o.Value)}: mismatched, expected {StrReq(pendingRequests[0].Request)}"));
                            pendingRequests.RemoveAt(0);
                        }
                        else
                        {
                            _warnings.Add((r, op.Timestamp, $"Reject {StrReject(o.Value)}: all good, but still"));
                            pendingRequests.RemoveAt(0);
                        }
                        pendingCast = null; // TODO: right?
                        break;

                    case ActorState.OpCastEvent o:
                        if (o.Value.SourceSequence != 0 && o.InstanceID == playerID)
                        {
                            var rqIndex = pendingRequests.FindIndex(r => r.Request.SourceSequence == o.Value.SourceSequence);
                            if (rqIndex < 0)
                            {
                                _warnings.Add((r, op.Timestamp, $"Unexpected action-effect {StrEvt(o.Value)}: currently {pendingRequests.Count} are pending"));
                                pendingRequests.Clear();
                            }
                            else if (rqIndex > 0)
                            {
                                _warnings.Add((r, op.Timestamp, $"Unexpected action-effect {StrEvt(o.Value)}: index={rqIndex}, first={StrReq(pendingRequests[0].Request)}, count={pendingRequests.Count}"));
                            }
                            else if (pendingRequests[0].Request.Action != o.Value.Action)
                            {
                                _warnings.Add((r, op.Timestamp, $"Request/response action mismatch: requested {StrReq(pendingRequests[0].Request)}, got {StrEvt(o.Value)}"));
                            }
                            else if ((op.Timestamp - pendingRequests[0].Timestamp).TotalSeconds >= pendingRequests[0].Request.InitialAnimationLock + pendingRequests[0].Request.InitialCastTimeTotal)
                            {
                                _warnings.Add((r, op.Timestamp, $"Response {StrEvt(o.Value)} arrived too late: {(op.Timestamp - pendingRequests[0].Timestamp).TotalSeconds:f3}s ago, initial anim lock was {pendingRequests[0].Request.InitialAnimationLock:f3}s"));
                            }
                            pendingRequests.RemoveRange(0, rqIndex + 1);
                            pendingCast = null;
                        }
                        break;

                    case ActorState.OpCastInfo o:
                        if (o.InstanceID == playerID)
                        {
                            if (o.Value != null)
                            {
                                // start new cast
                                if (pendingRequests.Count == 0)
                                {
                                    _warnings.Add((r, op.Timestamp, $"Player cast {StrCast(o.Value)} started without request"));
                                }
                                else if (pendingRequests[0].Request.Action != o.Value.Action)
                                {
                                    _warnings.Add((r, op.Timestamp, $"Player cast {StrCast(o.Value)} started with different request ({StrReq(pendingRequests[0].Request)}, count={pendingRequests.Count}"));
                                }
                                if (pendingCast != null)
                                {
                                    _warnings.Add((r, op.Timestamp, $"Player cast {StrCast(o.Value)} started when another cast {StrCast(pendingCast)} is already in progress"));
                                }
                                pendingCast = o.Value;
                            }
                            else if (pendingCast != null)
                            {
                                int index = pendingRequests.FindIndex(r => r.Request.Action == pendingCast.Action);
                                if (index < 0)
                                {
                                    _warnings.Add((r, op.Timestamp, $"Player cast {StrCast(pendingCast)} ended without request ({pendingRequests.Count} pending)"));
                                    pendingRequests.Clear();
                                }
                                else if (index > 0)
                                {
                                    _warnings.Add((r, op.Timestamp, $"Player cast {StrCast(pendingCast)} ended with index={index} request; first={StrReq(pendingRequests[0].Request)}, count={pendingRequests.Count}"));
                                    pendingRequests.RemoveRange(0, index + 1);
                                }
                                else
                                {
                                    pendingRequests.RemoveAt(0);
                                }
                                pendingCast = null;
                            }
                            // else: this was implicitly ended by different op
                        }
                        break;
                }
            }
        }
    }

    public void Draw(UITree tree)
    {
        tree.LeafNodes(_warnings, w => $"{w.r.Path} {w.ts:O}: {w.warning}");
    }

    private string StrReq(ClientActionRequest r) => $"#{r.SourceSequence} {r.Action} @ {r.TargetID:X8}";
    private string StrEvt(ActorCastEvent e) => $"#{e.SourceSequence} {e.Action} @ {e.MainTargetID:X8}";
    private string StrCast(ActorCastInfo e) => $"{e.Action} @ {e.TargetID:X8}";
    private string StrReject(ClientActionReject r) => $"#{r.SourceSequence} {r.Action} ({r.LogMessageID})";
}
