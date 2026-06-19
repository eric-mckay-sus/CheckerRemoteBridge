// <copyright file="CheckerActionService.cs" company="Stanley Electric US Co. Inc.">
// Copyright (c) 2026 Stanley Electric US Co. Inc. Licensed under the MIT License.
// </copyright>

namespace CheckerRemoteBridge.Services;

using OpcUtilities;

/// <summary>
/// Sends OPC request pulses for checker control actions.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CheckerActionService"/> class.
/// </remarks>
/// <param name="opcClient">The OPC client used to write request tags.</param>
public sealed class CheckerActionService(IOpcClient opcClient)
{
    private readonly IOpcClient opcClient = opcClient;

    /// <summary>
    /// Gets a value indicating whether OPC actions can be sent.
    /// </summary>
    public bool IsConfigured => this.opcClient.IsConfigured;

    /// <summary>
    /// Fires a reboot request for the specified final station.
    /// </summary>
    /// <param name="finalId">The final station number (1-based).</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns><see langword="true"/> when the request pulse was written.</returns>
    public Task<bool> RequestRebootAsync(int finalId, CancellationToken cancellationToken = default) =>
        this.PulseRequestAsync(finalId, "RebootRequest", cancellationToken);

    /// <summary>
    /// Fires a shutdown request for the specified final station.
    /// </summary>
    /// <param name="finalId">The final station number (1-based).</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns><see langword="true"/> when the request pulse was written.</returns>
    public Task<bool> RequestShutdownAsync(int finalId, CancellationToken cancellationToken = default) =>
        this.PulseRequestAsync(finalId, "ShutdownRequest", cancellationToken);

    /// <summary>
    /// Fires an auto-launch request for the specified final station.
    /// </summary>
    /// <param name="finalId">The final station number (1-based).</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns><see langword="true"/> when the request pulse was written.</returns>
    public Task<bool> RequestAutoAsync(int finalId, CancellationToken cancellationToken = default) =>
        this.PulseRequestAsync(finalId, "AutoRequest", cancellationToken);

    /// <summary>
    /// Fires a reset request for the specified final station.
    /// </summary>
    /// <param name="finalId">The final station number (1-based).</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns><see langword="true"/> when the request pulse was written.</returns>
    public Task<bool> RequestResetAsync(int finalId, CancellationToken cancellationToken = default) =>
        this.PulseRequestAsync(finalId, "ResetRequest", cancellationToken);

    private async Task<bool> PulseRequestAsync(int finalId, string tagName, CancellationToken cancellationToken)
    {
        if (!this.opcClient.IsConfigured)
        {
            return false;
        }

        string nodeId = OpcNodeIds.StatusTag(finalId, tagName);
        if (!await this.opcClient.WriteAsync(nodeId, true, cancellationToken).ConfigureAwait(false))
        {
            return false;
        }

        await Task.Delay(TimeSpan.FromMilliseconds(1000), cancellationToken).ConfigureAwait(false);
        return await this.opcClient.WriteAsync(nodeId, false, cancellationToken).ConfigureAwait(false);
    }
}
