// <copyright file="IPiControlService.cs" company="Stanley Electric US Co. Inc.">
// Copyright (c) 2026 Stanley Electric US Co. Inc. Licensed under the MIT License.
// </copyright>

namespace CheckerRemoteBridge.Services;

/// <summary>
/// Controls checker Pis over SSH or a local agent (launch, backup, reachability).
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
    /// <returns><see langword="true"/> when the launch command was sent successfully.</returns>
    Task<bool> LaunchAsync(int finalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether the Pi is reachable over SSH.
    /// </summary>
    /// <param name="finalId">The final station number (1-based).</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns><see langword="true"/> when the Pi responds.</returns>
    Task<bool> IsReachableAsync(int finalId, CancellationToken cancellationToken = default);
}
