using System.Diagnostics;
using System.Text;

namespace Core;

public static class PowerShellService
{
    public static string RunScript(string script)
    {
        // Настраиваем процесс powershell.exe
        // -NoProfile: не загружать профиль пользователя (быстрее)
        // -NonInteractive: не ждать ввода
        // -Command: выполнить команду
        // Устанавливаем кодировку консоли в UTF8, чтобы корректно читать названия языков и JSON
        var command = $"$OutputEncoding = [Console]::OutputEncoding = [System.Text.Encoding]::UTF8; {script}";

        var startInfo = new ProcessStartInfo
        {
            FileName = "powershell",
            Arguments = $"-NoProfile -NonInteractive -Command \"{command.Replace("\"", "\\\"")}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8
        };

        using var process = Process.Start(startInfo)
                          ?? throw new Exception("Failed to start PowerShell process.");

        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();

        process.WaitForExit();

        if (process.ExitCode != 0)
            if (!string.IsNullOrWhiteSpace(error))
                throw new Exception($"PowerShell Error: {error}");

        return output;
    }
}

