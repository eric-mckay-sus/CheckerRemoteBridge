// <copyright file="FakePiControlService.cs" company="Stanley Electric US Co. Inc.">
// Copyright (c) 2026 Stanley Electric US Co. Inc. Licensed under the MIT License.
// </copyright>

namespace CheckerRemoteBridge.Services;

/// <summary>
/// A service that pretends to use SSH to access the target checker Pis. Designed for developing SSH functionality.
/// </summary>
public class FakePiControlService : IPiControlService
{
    private static readonly string DefaultChecksumCommand = "cksum ./ready.sh";

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
        return DefaultChecksumCommand;
    }
}
