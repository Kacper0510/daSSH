using System.Diagnostics;

namespace daSSH.Data;

public static class SSH {
    public static async Task<(string, string)> RunKeyGen(int userID) {
        var path = $"/tmp/{userID}";
        var args = $"-f {path} -q -N \"\" -C {userID}";
        await LinuxHelper.RunProcessAsync("ssh-keygen", args);
        var publicKey = await File.ReadAllTextAsync($"{path}.pub");
        var privateKey = await File.ReadAllTextAsync(path);
        File.Delete($"{path}.pub");
        File.Delete(path);
        return (publicKey, privateKey);
    }
}
