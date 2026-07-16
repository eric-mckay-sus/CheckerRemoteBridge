// <copyright file="CheckerActionService.cs" company="Stanley Electric US Co. Inc.">
// Copyright (c) 2026 Stanley Electric US Co. Inc. Licensed under the MIT License.
// </copyright>

namespace CheckerRemoteBridge.Services;

using static CheckerRemoteBridge.Services.ChecksumVerificationService;

using OpcUtilities;
using CheckerRemoteBridge.Models;

/// <summary>
/// Service responsible for OPC request pulses of checker control actions and Pi-side checksum execution/client launch.
/// </summary>
/// <param name="opcClient">The OPC client used to write request tags.</param>
/// <param name="piControlService">The Pi control service used to run the checksum script and launch the checker client program.</param>
/// <param name="stateStore">The checker state store used to publish checksum results.</param>
public sealed class CheckerActionService(IOpcClient opcClient, IPiControlService piControlService, CheckerStateStore stateStore)
{
    private static readonly int PulseDurationMs = 1000;

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
    /// Executes cleanup procedures for <see cref="opcClient"/> and <see cref="piControlService"/>.
    /// </summary>
    public void EndServices()
    {
        this.opcClient.UnsubscribeAll();
        this.piControlService.DisconnectAll();
    }

    /// <summary>
    /// Fires a reboot request for the checker with ID=<paramref name="finalId"/>.
    /// </summary>
    /// <param name="finalId">The final station number (1-based).</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A value indivating whether the request pulse was written.</returns>
    public Task<bool> RequestRebootAsync(int finalId, CancellationToken cancellationToken = default) =>
        this.PulseRequestAsync(finalId, "RebootRequest", cancellationToken);

    /// <summary>
    /// Fires a shutdown request for the checker with ID=<paramref name="finalId"/>.
    /// </summary>
    /// <param name="finalId">The final station number (1-based).</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A value indivating whether the request pulse was written.</returns>
    public Task<bool> RequestShutdownAsync(int finalId, CancellationToken cancellationToken = default) =>
        this.PulseRequestAsync(finalId, "ShutdownRequest", cancellationToken);

    /// <summary>
    /// Fires an auto-launch request for the specified final station after verifying the checksum.
    /// </summary>
    /// <remarks>
    /// The bundling of checksum and launch is valid under the assumption that they will never be run separately.
    /// Every launch requires a checksum to be run immediately beforehand, and it's pointless to run a checksum on a file we aren't opening.
    /// </remarks>
    /// <param name="finalId">The final station number (1-based).</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A value indicating whether the launch was successful.</returns>
    public async Task<bool> CheckAndLaunchAsync(int finalId, CancellationToken cancellationToken = default)
    {
        await this.RequestChecksumAsync(finalId, cancellationToken);

        CheckerStatus status = this.stateStore.Get(finalId);

        if (!Evaluate(status).Equals(ChecksumResult.Match))
        {
            Console.WriteLine($"Checker {finalId} failed checksum");
            return false;
        }

        // If the checker state has not arrived from OPC yet, do not treat the default value of 0 as an offline state.
        if (!status.HasCheckerState)
        {
            Console.WriteLine($"Checker {finalId} launch skipped: checker state has not populated yet");
            return false;
        }

        // If the Pi is doing something, don't launch (it's busy, almost certainly with this program). Nothing is lost, we still have the SSH connection.
        if (status.CheckerState != 0)
        {
            this.stateStore.Update(finalId, status => status.CheckerRunning = true);
            Console.WriteLine($"Checker {finalId} already running");
            return false;
        }

        bool isRunning = await this.piControlService.LaunchAsync(finalId, cancellationToken);
        this.stateStore.Update(finalId, status => status.CheckerRunning = isRunning);
        Console.WriteLine($"Checker {finalId} running: {isRunning}");

        return isRunning;
    }

    /// <summary>
    /// Requests a checksum on the checker program on the specified Pi and puts it in <see cref="stateStore"/> .
    /// </summary>
    /// <remarks>
    /// This method is private because it is only run internally by <seealso cref="CheckAndLaunchAsync"/> and has no real reason to be run elsewhere.
    /// </remarks>
    /// <param name="finalId">The final station number (1-based).</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
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

    /// <summary>
    /// Given a valid boolean OPC tag with <paramref name="tagName"/>, sets the flag for <see cref="PulseDurationMs"/>, then unsets it.
    /// </summary>
    /// <param name="finalId">The final station number (1-based).</param>
    /// <param name="tagName">The name of the OPC tag to pulse.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A value indicating whether the request pulse was written.</returns>
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

        await Task.Delay(TimeSpan.FromMilliseconds(PulseDurationMs), cancellationToken).ConfigureAwait(false);
        return await this.opcClient.WriteAsync(nodeId, false, cancellationToken).ConfigureAwait(false);
    }
}
