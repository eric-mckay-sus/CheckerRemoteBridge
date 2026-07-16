// <copyright file="FakePiControlService.cs" company="Stanley Electric US Co. Inc.">
// Copyright (c) 2026 Stanley Electric US Co. Inc. Licensed under the MIT License.
// </copyright>

namespace CheckerRemoteBridge.Services;

/// <summary>
/// <see cref="IPiControlService"/> implementation that pretends to use SSH to access the target checker Pis. Designed for developing SSH flow off the network.
/// </summary>
public class FakePiControlService : IPiControlService
{
    /// <summary>
    /// The output from running the checksum.
    /// </summary>
    private static readonly string ChecksumOutput = "103485781052109";

    /// <inheritdoc />
    public bool IsConfigured { get; } = true;

    /// <inheritdoc />
    public async Task<bool> InitializeAsync(CancellationToken cancellationToken = default) => this.IsConfigured;

    /// <inheritdoc />
    public async Task<bool> IsReachableAsync(int finalId, CancellationToken cancellationToken = default) => this.IsConfigured;

    /// <inheritdoc />
    public async Task<bool> LaunchAsync(int finalId, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Launched checker program for final {finalId}.");
        return true;
    }

    /// <inheritdoc />
    public async Task<string?> RunChecksumScriptAsync(int finalId, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Final {finalId} ran checksum.");
        return ChecksumOutput;
    }
}
