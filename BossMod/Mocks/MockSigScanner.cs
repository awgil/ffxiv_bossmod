using DalaMock.Core.Mocks;
using Dalamud.Plugin.Services;
using System.Diagnostics;
using System.Threading;

namespace BossMod.Mocks;

internal class MockSigScanner : ISigScanner, IMockService
{
    public string ServiceName => "MockSigScanner";

    public bool IsCopy => throw new NotImplementedException();

    public bool Is32BitProcess => throw new NotImplementedException();

    public nint SearchBase => throw new NotImplementedException();

    public nint TextSectionBase => throw new NotImplementedException();

    public long TextSectionOffset => throw new NotImplementedException();

    public int TextSectionSize => throw new NotImplementedException();

    public nint DataSectionBase => throw new NotImplementedException();

    public long DataSectionOffset => throw new NotImplementedException();

    public int DataSectionSize => throw new NotImplementedException();

    public nint RDataSectionBase => throw new NotImplementedException();

    public long RDataSectionOffset => throw new NotImplementedException();

    public int RDataSectionSize => throw new NotImplementedException();

    public ProcessModule Module => throw new NotImplementedException();

    public nint GetStaticAddressFromSig(string signature, int offset = 0) => throw new NotImplementedException();
    public nint ResolveRelativeAddress(nint nextInstAddr, int relOffset) => throw new NotImplementedException();
    public nint[] ScanAllText(string signature) => throw new NotImplementedException();
    public IEnumerable<nint> ScanAllText(string signature, CancellationToken cancellationToken) => throw new NotImplementedException();
    public nint ScanData(string signature) => throw new NotImplementedException();
    public nint ScanModule(string signature) => throw new NotImplementedException();
    public nint ScanText(string signature) => throw new NotImplementedException();
    public bool TryGetStaticAddressFromSig(string signature, out nint result, int offset = 0) => throw new NotImplementedException();
    public bool TryScanData(string signature, out nint result) => throw new NotImplementedException();
    public bool TryScanModule(string signature, out nint result) => throw new NotImplementedException();
    public bool TryScanText(string signature, out nint result) => throw new NotImplementedException();
}
