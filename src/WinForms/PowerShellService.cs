using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace WinForms;

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

    public static List<T> ParseJsonAsArray<T>(string json, JsonSerializerOptions options)
    {
        var trimmed = json.TrimStart();
        if (trimmed.StartsWith("["))
        {
            return JsonSerializer.Deserialize<List<T>>(json, options) ?? [];
        }
        else
        {
            var single = JsonSerializer.Deserialize<T>(json, options);
            return single != null ? [single] : [];
        }
    }
}

