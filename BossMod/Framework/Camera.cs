using System;
using System.Runtime.InteropServices;

namespace BossMod
{
    class Camera
    {
        public static Camera? Instance;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr GetMatrixSingletonDelegate();

        private GetMatrixSingletonDelegate _getMatrixSingleton { get; init; }

        public SharpDX.Matrix ViewProj { get; private set; }
        public SharpDX.Matrix Proj { get; private set; }
        public SharpDX.Matrix View { get; private set; }
        public SharpDX.Matrix CameraWorld { get; private set; }
        public float CameraAzimuth { get; private set; } // facing north = 0, facing west = pi/4, facing south = +-pi/2, facing east = -pi/4
        public float CameraAltitude { get; private set; } // facing horizontally = 0, facing down = pi/4, facing up = -pi/4

        public Camera()
        {
            var funcAddress = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 48 8D 4C 24 ?? 48 89 4c 24 ?? 4C 8D 4D ?? 4C 8D 44 24 ??");
            _getMatrixSingleton = Marshal.GetDelegateForFunctionPointer<GetMatrixSingletonDelegate>(funcAddress);
        }

        public void Update()
        {
            var matrixSingleton = _getMatrixSingleton();
            ViewProj = ReadMatrix(matrixSingleton + 0x1b4);
            Proj = ReadMatrix(matrixSingleton + 0x174);
            View = ViewProj * SharpDX.Matrix.Invert(Proj);
            CameraWorld = SharpDX.Matrix.Invert(View);
            CameraAzimuth = MathF.Atan2(View.Column3.X, View.Column3.Z);
            CameraAltitude = MathF.Asin(View.Column3.Y);
        }

        private unsafe SharpDX.Matrix ReadMatrix(IntPtr address)
        {
            var p = (float*)address;
            SharpDX.Matrix mtx = new();
            for (var i = 0; i < 16; i++)
                mtx[i] = *p++;
            return mtx;
        }
    }
}
