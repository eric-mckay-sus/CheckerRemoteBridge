// <copyright file="IPiControlService.cs" company="Stanley Electric US Co. Inc.">
// Copyright (c) 2026 Stanley Electric US Co. Inc. Licensed under the MIT License.
// </copyright>

namespace CheckerRemoteBridge.Services;

/// <summary>
/// Interface defining the contract for controlling checker Pis over SSH or a local agent (launch, backup, reachability).
/// </summary>
public interface IPiControlService
{
    /// <summary>
    /// Gets a value indicating whether SSH/agent credentials are configured.
    /// </summary>
    bool IsConfigured { get; }

    /// <summary>
    /// Attempts to auto-launch the checker on the specified Pi.
    /// </summary>
    /// <param name="finalId">The final station number (1-based).</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>Whether the launch command was sent successfully.</returns>
    Task<bool> LaunchAsync(int finalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether the Pi is reachable over SSH. If not reachable, attempts to create the SSH connection and returns success state.
    /// </summary>
    /// <param name="finalId">The final station number (1-based).</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>Whether the Pi responds.</returns>
    Task<bool> IsReachableAsync(int finalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ensures SSH access to all configured Pis before application services are available.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>Whether all configured Pis are accessible.</returns>
    Task<bool> InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs the configured checksum script on the specified Pi and returns the script output.
    /// </summary>
    /// <param name="finalId">The final station number (1-based).</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The checksum output when the script succeeds; otherwise <see langword="null"/>.</returns>
    Task<string?> RunChecksumScriptAsync(int finalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnects from all SSH clients.
    /// </summary>
    void DisconnectAll();
}
