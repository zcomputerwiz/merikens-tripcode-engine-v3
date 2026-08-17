using System.Runtime.InteropServices;

namespace TripEngine.Core
{
    [StructLayout(LayoutKind.Sequential)]
    public struct EngineConfig
    {
        public uint DeviceType; // 0 = CPU, 1 = CUDA, 2 = Vulkan
        public uint VectorWidth;
    }

    public static partial class NativeMethods
    {
        [LibraryImport("tripengine", EntryPoint = "InitEngine")]
        public static partial int InitEngine(ref EngineConfig config);

        [LibraryImport("tripengine", EntryPoint = "ShutdownEngine")]
        public static partial void ShutdownEngine();

        [LibraryImport("tripengine", EntryPoint = "ProcessBatch")]
        public static partial ulong ProcessBatch(
            [MarshalAs(UnmanagedType.LPStr)] string salt,
            ulong startIndex,
            ulong count,
            IntPtr outMatches
        );
    }
}
