// <copyright file="PiControlService.cs" company="Stanley Electric US Co. Inc.">
// Copyright (c) 2026 Stanley Electric US Co. Inc. Licensed under the MIT License.
// </copyright>

namespace CheckerRemoteBridge.Services;

using Renci.SshNet;

/// <summary>
/// Controls checker Pis over SSH or a local agent (launch, backup, reachability).
/// </summary>
public sealed class PiControlService : IPiControlService, IDisposable
{
    private const string DefaultChecksumCommand = "cksum ./ready.sh";
    private readonly IReadOnlyDictionary<int, PiConnection> connections;
    private readonly Dictionary<int, SshClient> sshClients = [];
    private readonly object clientSync = new ();
    private readonly string checksumCommand;

    /// <summary>
    /// Initializes a new instance of the <see cref="PiControlService"/> class by initializing connection with all checker Pis.
    /// </summary>
    public PiControlService()
    {
        this.connections = Enumerable.Range(1, 5)
            .Select(CreateConnection)
            .ToDictionary(connection => connection.finalId);

        this.checksumCommand = Environment.GetEnvironmentVariable("FINAL_CHECKSUM_COMMAND")?.Trim() ?? DefaultChecksumCommand;
        this.IsConfigured = this.connections.Count == 5;
    }

    /// <inheritdoc/>
    public bool IsConfigured { get; }

    /// <summary>
    /// Gets a value indicating whether all configured Pis are currently reachable via SSH.
    /// </summary>
    public bool IsReady { get; private set; }

    /// <summary>
    /// Ensures SSH access to all configured Pis before the app starts.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns><see langword="true"/> when all Pis are accessible.</returns>
    public async Task<bool> InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (!this.IsConfigured)
        {
            return false;
        }

        Task<bool>[] connectTasks = this.connections.Keys
            .Select(finalId => this.EnsureConnectedAsync(finalId, cancellationToken)).ToArray();

        bool[] results = await Task.WhenAll(connectTasks).ConfigureAwait(false);
        this.IsReady = results.All(success => success);
        return this.IsReady;
    }

    /// <inheritdoc/>
    public async Task<bool> LaunchAsync(int finalId, CancellationToken cancellationToken = default)
    {
        if (!await this.IsReachableAsync(finalId, cancellationToken).ConfigureAwait(false))
        {
            return false;
        }

        SshClient client = this.GetClient(finalId);
        string? output = await RunCommandAndCaptureAsync(client, this.checksumCommand, cancellationToken).ConfigureAwait(false);
        return !string.IsNullOrWhiteSpace(output);
    }

    /// <inheritdoc/>
    public async Task<bool> IsReachableAsync(int finalId, CancellationToken cancellationToken = default)
    {
        SshClient client = this.GetClient(finalId);
        if (client.IsConnected)
        {
            return true;
        }

        return await ConnectClientAsync(client, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Runs the configured checksum script on the specified Pi and returns the script output.
    /// </summary>
    /// <param name="finalId">The final station number (1-based).</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The checksum output when the script succeeds; otherwise <see langword="null"/>.</returns>
    public async Task<string?> RunChecksumScriptAsync(int finalId, CancellationToken cancellationToken = default)
    {
        if (!await this.IsReachableAsync(finalId, cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        SshClient client = this.GetClient(finalId);
        return await RunCommandAndCaptureAsync(client, this.checksumCommand, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        lock (this.clientSync)
        {
            foreach (SshClient client in this.sshClients.Values)
            {
                try
                {
                    if (client.IsConnected)
                    {
                        client.Disconnect();
                    }
                }
                catch
                {
                    // Ignore cleanup failures.
                }

                client.Dispose();
            }

            this.sshClients.Clear();
        }
    }

    /// <summary>
    /// Attempts to connect to a specified SSH client.
    /// </summary>
    /// <param name="client">The SshClient object to connect to.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to invalidate the connection.</param>
    /// <returns>A Task representing whether the connection was successful.</returns>
    private static async Task<bool> ConnectClientAsync(SshClient client, CancellationToken cancellationToken)
    {
        return await Task.Run(
            () =>
            {
                if (client.IsConnected)
                {
                    return true;
                }

                client.Connect();
                if (!client.IsConnected)
                {
                    return false;
                }

                SshCommand command = client.CreateCommand("bash -lc 'echo READY'");
                command.CommandTimeout = TimeSpan.FromSeconds(15);
                command.Execute();

                return command.ExitStatus == 0;
            }, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes a command on the specified SSH client and captures its output.
    /// </summary>
    /// <param name="client">The SshClient on which to run the command.</param>
    /// <param name="commandText">The command to be run.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to invalidate the command.</param>
    /// <returns>A Task representing the command output when it succeeds; otherwise <see langword="null"/>.</returns>
    private static async Task<string?> RunCommandAndCaptureAsync(SshClient client, string commandText, CancellationToken cancellationToken)
    {
        if (!client.IsConnected && !await ConnectClientAsync(client, cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        return await Task.Run(
            () =>
            {
                SshCommand command = client.CreateCommand(BuildBashCommand(commandText));
                command.CommandTimeout = TimeSpan.FromMinutes(5);
                string cmdOut = command.Execute();
                Console.WriteLine($"{commandText}: {cmdOut}");
                return command.ExitStatus == 0 ? cmdOut.Trim() : null;
            }, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates an actionable SSH client from a PiConnection object.
    /// </summary>
    /// <param name="connection">The PiConnection sourcing the connection info.</param>
    /// <returns>The new SshClient object.</returns>
    private static SshClient CreateClient(PiConnection connection)
    {
        var connectionInfo = new PasswordConnectionInfo(connection.host, connection.username, connection.password)
        {
            Timeout = TimeSpan.FromSeconds(10),
            RetryAttempts = 1,
        };

        return new SshClient(connectionInfo)
        {
            KeepAliveInterval = TimeSpan.FromSeconds(30),
        };
    }

    /// <summary>
    /// Creates a simple PiConnection record containing the IP address and login credentials for a particular checker.
    /// </summary>
    /// <param name="finalId">The number of the checker for which the PiConnection should be made.</param>
    /// <returns>The new PiConnection object.</returns>
    private static PiConnection CreateConnection(int finalId)
    {
        string host = GetRequired($"FINAL{finalId}_IP");
        string username = GetEnvironmentValue($"FINAL{finalId}_USER") ?? GetRequired("FINAL_USER");
        string password = GetEnvironmentValue($"FINAL{finalId}_PASS") ?? GetRequired("FINAL_PASS");

        return new PiConnection(finalId, host, username, password);
    }

    /// <summary>
    /// Builds a bash command from the command text only, escaping quotes as necessary.
    /// </summary>
    /// <param name="command">The command text to wrap in the bash boilerplate.</param>
    /// <returns>The command, ready for the bash shell.</returns>
    private static string BuildBashCommand(string command) =>
        $"bash -lc '{command.Replace("'", "'\\''")}'";

    private static string? GetEnvironmentValue(string key)
    {
        string? value = Environment.GetEnvironmentVariable(key);
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    /// <summary>
    /// Holds SSH credentials for a checker Pi.
    /// </summary>
    private sealed record PiConnection(int finalId, string host, string username, string password);

    /// <summary>
    /// Gets a required value from the environment.
    /// </summary>
    /// <param name="key">The key to get the environment variable.</param>
    /// <returns>The value associated with the key, or <see cref="InvalidOperationException"/> if not found.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="key"/> cannot be found in the environment.</exception>
    private static string GetRequired(string key)
    {
        string? value = Environment.GetEnvironmentVariable(key);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Required environment variable '{key}' is missing for {(key.Contains("OPC") ? "OPC" : "checker Pi")} connection.");
        }

        return value;
    }

    private async Task<bool> EnsureConnectedAsync(int finalId, CancellationToken cancellationToken)
    {
        return await this.IsReachableAsync(finalId, cancellationToken).ConfigureAwait(false);
    }

    private SshClient GetClient(int finalId)
    {
        if (!this.connections.TryGetValue(finalId, out PiConnection? connection))
        {
            throw new ArgumentOutOfRangeException(nameof(finalId), finalId, "Final ID must be between 1 and 5.");
        }

        lock (this.clientSync)
        {
            if (this.sshClients.TryGetValue(finalId, out SshClient? existingClient))
            {
                // Return it even if disconnected; IsReachableAsync will reconnect it.
                return existingClient;
            }

            SshClient newClient = CreateClient(connection);
            this.sshClients[finalId] = newClient;
            return newClient;
        }
    }
}
