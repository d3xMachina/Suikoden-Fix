extern alias GSD1;
extern alias GSD2;

using Suikoden_Fix.Tools.Patch;

namespace Suikoden_Fix.Patches;

public class BetterSanchezPatch
{
    private static void PatchAssembly()
    {
        var address = MemoryPatcher.GetMethodAddress(typeof(GSD1.Teventf4_c), "setWorkData");
        MemoryPatcher.PatchNOP(address, 0xA0, 6); // ignore chara_no check to add both Odessa and Ted
    }
}
