using System.Diagnostics;

namespace daSSH.Data;

public static class LinuxHelper {
    // See https://stackoverflow.com/a/31492250/23240713
    public static async Task<int> RunProcessAsync(string fileName, string args) {
        using var process = new Process {
            StartInfo = {
                FileName = fileName, Arguments = args,
                UseShellExecute = false, CreateNoWindow = true,
                RedirectStandardOutput = true, RedirectStandardError = true
            },
            EnableRaisingEvents = true
        };
        return await RunProcessAsync(process).ConfigureAwait(false);
    }

    private static Task<int> RunProcessAsync(Process process) {
        var tcs = new TaskCompletionSource<int>();

        process.Exited += (s, ea) => tcs.SetResult(process.ExitCode);
        process.OutputDataReceived += (s, ea) => Console.WriteLine(ea.Data);
        process.ErrorDataReceived += (s, ea) => Console.WriteLine("ERR: " + ea.Data);

        bool started = process.Start();
        if (!started) {
            throw new InvalidOperationException("Could not start process: " + process);
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        return tcs.Task;
    }
}
