using System.Runtime.InteropServices;

namespace WinForms;

public static class WinApi
{
    public const uint ActivationFlag = 0x00000001;
    public const uint DeactivationFlag = 0x00000001;
    public const uint SubstitutionOnFlag = 0x00000002;
    public const uint SettingChangeFlag = 0x001A;
    public const IntPtr BroadcastFlag = 0xffff;

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr LoadKeyboardLayout(string pwszKLID, uint Flags);

    [DllImport("input.dll", CharSet = CharSet.Unicode)]
    public static extern int InstallLayoutOrTip(string psz, uint dwFlags);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, string lParam);
}
