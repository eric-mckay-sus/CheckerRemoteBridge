// <copyright file="PiControlService.cs" company="Stanley Electric US Co. Inc.">
// Copyright (c) 2026 Stanley Electric US Co. Inc. Licensed under the MIT License.
// </copyright>

namespace CheckerRemoteBridge.Services;

using Renci.SshNet;
using Renci.SshNet.Common;
using System.Net.Sockets;

/// <summary>
/// <see cref="IPiControlService"/> implementation which connects to checker Pis over SSH or a local agent (launch, backup, reachability).
/// </summary>
public sealed class PiControlService : IPiControlService, IDisposable
{
    /// <summary>
    /// The checksum command to run if there is not one in the environment variable at <see cref="ChecksumCommandEnvOverride"/>.
    /// </summary>
    private static readonly string DefaultChecksumCommand = "cksum ./ready.sh";

    /// <summary>
    /// The name of the environment variable containing the optional override for <see cref="DefaultChecksumCommand"/>.
    /// </summary>
    private static readonly string ChecksumCommandEnvOverride = "FINAL_CHECKSUM_COMMAND";

    /// <summary>
    /// The launch command to run if there is not one in the environment variable at <see cref="LaunchCommandEnvOverride"/>.
    /// </summary>
    private static readonly string DefaultLaunchCommand = "./ready.sh";

    /// <summary>
    /// The name of the environment variable containing the optional override for <see cref="DefaultLaunchCommand"/>.
    /// </summary>
    private static readonly string LaunchCommandEnvOverride = "FINAL_LAUNCH_COMMAND";

    /// <summary>
    /// The dictionary mapping final station IDs to their <see cref="PiConnection"/> object containing connection credentials.
    /// </summary>
    private readonly IReadOnlyDictionary<int, PiConnection> connections;

    /// <summary>
    /// The dictionary mapping final station IDs to their <see cref="SshClient"/> object capable of executing SSH commands.
    /// </summary>
    private readonly Dictionary<int, SshClient> sshClients = [];

    /// <summary>
    /// The lock object used to prevent concurrency-related issues when getting or disposing a client.
    /// </summary>
    private readonly object clientSync = new ();

    /// <summary>
    /// The checksum command to be used (assigned in ctor as <see cref="ChecksumCommandEnvOverride"/> if it exists, otherwise <see cref="DefaultChecksumCommand"/>).
    /// </summary>
    private readonly string checksumCommand;

    /// <summary>
    /// The checksum command to be used (assigned in ctor as <see cref="LaunchCommandEnvOverride"/> if it exists, otherwise <see cref="DefaultLaunchCommand"/>).
    /// </summary>
    private readonly string launchCommand;

    /// <summary>
    /// Initializes a new instance of the <see cref="PiControlService"/> class by initializing connection with all checker Pis.
    /// </summary>
    public PiControlService()
    {
        this.connections = Enumerable.Range(1, 5)
            .Select(TryCreateConnection)
            .OfType<PiConnection>()
            .ToDictionary(connection => connection.finalId);

        this.checksumCommand = Environment.GetEnvironmentVariable(ChecksumCommandEnvOverride)?.Trim() ?? DefaultChecksumCommand;
        this.launchCommand = Environment.GetEnvironmentVariable(LaunchCommandEnvOverride)?.Trim() ?? DefaultLaunchCommand;
        this.IsConfigured = this.connections.Count > 0;
    }

    /// <inheritdoc/>
    public bool IsConfigured { get; }

    /// <summary>
    /// Gets a value indicating whether all configured Pis are currently reachable via SSH.
    /// </summary>
    public bool IsReady { get; private set; }

    /// <summary>
    /// Container for SSH credentials for connecting to a checker Pi.
    /// </summary>
    private sealed record PiConnection(int finalId, string host, string username, string password);

    /// <summary>
    /// Ensures SSH access to all configured Pis before the app starts.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A value indicating whether all Pis are accessible.</returns>
    public async Task<bool> InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (!this.IsConfigured)
        {
            return false;
        }

        Task<bool>[] connectTasks = this.connections.Keys
            .Select(finalId => this.IsReachableAsync(finalId, cancellationToken)).ToArray();

        bool[] results = await Task.WhenAll(connectTasks).ConfigureAwait(false);
        this.IsReady = results.All(success => success);
        return this.IsReady;
    }

    /// <inheritdoc/>
    public async Task<bool> LaunchAsync(int finalId, CancellationToken cancellationToken = default)
    {
        if (!await this.IsReachableAsync(finalId, cancellationToken).ConfigureAwait(false))
        {
            Console.WriteLine($"Final {finalId} not reachable");
            return false;
        }

        SshClient client = this.GetClient(finalId);
        string? output = await RunCommandAndCaptureAsync(client, this.launchCommand, cancellationToken).ConfigureAwait(false);
        Console.WriteLine($"Final {finalId} reported {output}");
        return true; // Works until script name changes
    }

    /// <inheritdoc/>
    public async Task<bool> IsReachableAsync(int finalId, CancellationToken cancellationToken = default)
    {
        if (!this.IsConfigured)
        {
            return false;
        }

        SshClient client = this.GetClient(finalId);
        if (client.IsConnected)
        {
            return true;
        }

        return await ConnectClientAsync(client, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void DisconnectAll()
    {
        foreach (KeyValuePair<int, SshClient> clientPair in this.sshClients)
        {
            clientPair.Value.Disconnect();
            Console.WriteLine($"Disconnected from final {clientPair.Key}");
        }
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
        ThreadPool.GetAvailableThreads(out int worker, out int _);
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Starting connect for thread pool avail={worker}");

        return await Task.Run(
            () =>
            {
                try
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
                }
                catch (Exception ex) when (ex is SshException or SocketException or TimeoutException)
                {
                    Console.WriteLine($"SSH connect failed: {ex.Message}");
                    return false;
                }
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
                try
                {
                    SshCommand command = client.CreateCommand(BuildBashCommand(commandText));
                    command.CommandTimeout = TimeSpan.FromSeconds(15);
                    string cmdOut = command.Execute();
                    Console.WriteLine($"{commandText}: {cmdOut}");
                    return command.ExitStatus == 0 ? cmdOut.Trim() : null;
                }
                catch (Exception ex) when (ex is SshException or SocketException or TimeoutException)
                {
                    Console.WriteLine($"SSH command failed: {ex.Message}");
                    return null;
                }
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
    private static PiConnection? TryCreateConnection(int finalId)
    {
        string? host = GetEnvironmentValue($"FINAL{finalId}_IP");
        string? username = GetEnvironmentValue($"FINAL{finalId}_USER") ?? GetEnvironmentValue("FINAL_USER");
        string? password = GetEnvironmentValue($"FINAL{finalId}_PASS") ?? GetEnvironmentValue("FINAL_PASS");

        if (host is null || username is null || password is null)
        {
            return null;
        }
        else
        {
            return new PiConnection(finalId, host, username, password);
        }
    }

    /// <summary>
    /// Builds a bash command from the command text only, escaping quotes as necessary.
    /// </summary>
    /// <param name="command">The command text to wrap in the bash boilerplate.</param>
    /// <returns>The command, ready for the bash shell.</returns>
    private static string BuildBashCommand(string command) =>
        $"bash -lc '{command.Replace("'", "'\\''")}'";

    /// <summary>
    /// Reads an environment variable if it exists.
    /// </summary>
    /// <param name="key">The name of the environment variable.</param>
    /// <returns>The value stored in the environment variable name <paramref name="key"/>, or null if it does not exist.</returns>
    private static string? GetEnvironmentValue(string key)
    {
        string? value = Environment.GetEnvironmentVariable(key);
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    /// <summary>
    /// Gets the SshClient object representing the checker for the specified Pi.
    /// </summary>
    /// <param name="finalId">The number of the checker for which to fetch the SSH client.</param>
    /// <returns>The SshClient object associate with the indicated Pi.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when he specified checker is not registered in <see cref="sshClients"/>.</exception>
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
