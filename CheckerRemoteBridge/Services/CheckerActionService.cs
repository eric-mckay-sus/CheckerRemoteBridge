// <copyright file="CheckerActionService.cs" company="Stanley Electric US Co. Inc.">
// Copyright (c) 2026 Stanley Electric US Co. Inc. Licensed under the MIT License.
// </copyright>

namespace CheckerRemoteBridge.Services;

using static CheckerRemoteBridge.Services.ChecksumVerificationService;

using OpcUtilities;
using CheckerRemoteBridge.Models;

/// <summary>
/// Sends OPC request pulses for checker control actions and triggers Pi-side checksum execution.
/// </summary>
/// <param name="opcClient">The OPC client used to write request tags.</param>
/// <param name="piControlService">The Pi control service used to run the checksum script.</param>
/// <param name="stateStore">The checker state store used to publish checksum results.</param>
public sealed class CheckerActionService(IOpcClient opcClient, IPiControlService piControlService, CheckerStateStore stateStore)
{
    private readonly IOpcClient opcClient = opcClient;
    private readonly IPiControlService piControlService = piControlService;
    private readonly CheckerStateStore stateStore = stateStore;

    /// <summary>
    /// Gets a value indicating whether OPC actions can be sent.
    /// </summary>
    public bool OpcConfigured => this.opcClient.IsConfigured;

    /// <summary>
    /// Gets a value indicating whether SSH actions can be sent.
    /// </summary>
    public bool SshConfigured => this.piControlService.IsConfigured;

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
    /// <returns>Whether the launch was successful.</returns>
    public async Task<bool> CheckAndLaunchAsync(int finalId, CancellationToken cancellationToken = default)
    {
        await this.RequestChecksumAsync(finalId, cancellationToken);

        CheckerStatus currentStatus = this.stateStore.Get(finalId);
        ChecksumResult result = Evaluate(currentStatus);

        Console.WriteLine($"[Station {finalId}] Evaluation Result: {result}");
        Console.WriteLine($"[Station {finalId}] Expected: '{currentStatus.ExpectedChecksum}'");
        Console.WriteLine($"[Station {finalId}] Actual  : '{currentStatus.ActualChecksum}'");

        if (!Evaluate(this.stateStore.Get(finalId)).Equals(ChecksumResult.Match))
        {
            return false;
        }

        bool isRunning = await this.piControlService.LaunchAsync(finalId, cancellationToken);
        this.stateStore.Update(finalId, status => status.CheckerRunning = isRunning);

        return isRunning;
    }

    /// <summary>
    /// Fires a reset request for the specified final station.
    /// </summary>
    /// <param name="finalId">The final station number (1-based).</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns><see langword="true"/> when the request pulse was written.</returns>
    public Task<bool> RequestResetAsync(int finalId, CancellationToken cancellationToken = default) =>
        this.PulseRequestAsync(finalId, "ResetRequest", cancellationToken);

    /// <summary>
    /// Requests a checksum on the checker program on the specified Pi.
    /// </summary>
    /// <remarks>
    /// This method is private because it is run internally by <see cref="CheckAndLaunchAsync"/> and has no real reason to be run elsewhere.
    /// </remarks>
    /// <param name="finalId">The final station number (1-based).</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The SSH output from the checksum command.</returns>
    private async Task<string?> RequestChecksumAsync(int finalId, CancellationToken cancellationToken = default)
    {
        string? checksum = await this.piControlService.RunChecksumScriptAsync(finalId, cancellationToken);

        if (!string.IsNullOrWhiteSpace(checksum))
        {
            this.stateStore.Update(finalId, status => status.ActualChecksum = checksum.Trim());
        }

        return checksum;
    }

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
