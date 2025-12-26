using System.Runtime.InteropServices;

namespace WinForms;

public static class WinApi
{
    public const uint ActivationFlag = 0x00000001;

    public const uint SubstitutionOnFlag = 0x00000002;

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr LoadKeyboardLayout(string pwszKLID, uint Flags);

    [DllImport("user32.dll")]
    public static extern bool UnloadKeyboardLayout(IntPtr hkl);

    [DllImport("input.dll", CharSet = CharSet.Unicode)]
    public static extern int InstallLayoutOrTip(string psz, uint dwFlags);

    [DllImport("user32.dll")]
    public static extern int GetKeyboardLayoutList(int nBuff, [Out] IntPtr[]? lpList);
}
