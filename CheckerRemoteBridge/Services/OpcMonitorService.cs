// <copyright file="OpcMonitorService.cs" company="Stanley Electric US Co. Inc.">
// Copyright (c) 2026 Stanley Electric US Co. Inc. Licensed under the MIT License.
// </copyright>

namespace CheckerRemoteBridge.Services;

using System.Globalization;
using CheckerRemoteBridge.Models;
using CheckerRemoteBridge.Options;
using Microsoft.Extensions.Options;
using OpcUtilities;

/// <summary>
/// Service responsible for subscribing to and monitoring checker status OPC tags, updating the local store.
/// </summary>
/// <param name="opcClient">The OPC client used for subscriptions.</param>
/// <param name="stateStore">The shared checker state store.</param>
/// <param name="fleetOptions">Fleet configuration options.</param>
/// <param name="logger">The logger instance.</param>
public sealed class OpcMonitorService(
    IOpcClient opcClient,
    CheckerStateStore stateStore,
    IOptions<CheckerFleetOptions> fleetOptions,
    ILogger<OpcMonitorService> logger) : BackgroundService
{
    private readonly IOpcClient opcClient = opcClient;
    private readonly CheckerStateStore stateStore = stateStore;
    private readonly CheckerFleetOptions fleetOptions = fleetOptions.Value;
    private readonly ILogger<OpcMonitorService> logger = logger;

    /// <inheritdoc/>
    public override Task StopAsync(CancellationToken cancellationToken)
    {
        this.opcClient.UnsubscribeAll();
        return base.StopAsync(cancellationToken);
    }

    /// <inheritdoc/>
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!this.opcClient.IsConfigured)
        {
            this.logger.LogWarning(
                "OPC connection is not configured. Set OPC_URI, OPC_USER, and OPC_PASS to enable live monitoring.");
            return Task.CompletedTask;
        }

        this.logger.LogInformation(
            "Subscribing to checker status tags for {FinalCount} finals at {UpdateRateMs}ms.",
            this.fleetOptions.FinalCount,
            this.fleetOptions.SubscriptionUpdateRateMs);

        for (int finalId = 1; finalId <= this.fleetOptions.FinalCount; finalId++)
        {
            foreach (string tagName in OpcNodeIds.StatusTags)
            {
                string nodeId = OpcNodeIds.StatusTag(finalId, tagName);
                this.opcClient.SubscribeDataChange(
                    nodeId,
                    this.fleetOptions.SubscriptionUpdateRateMs,
                    (_, value) => this.HandleTagChange(nodeId, value));
            }
        }

        return Task.Delay(Timeout.Infinite, stoppingToken);
    }

    /// <summary>
    /// Updates the <paramref name="tagName"/> entry in the <paramref name="status"/> object to <paramref name="value"/>.
    /// </summary>
    /// <param name="status">The <see cref="CheckerStatus"/> object to update.</param>
    /// <param name="tagName">The property of the <paramref name="status"/> object to update.</param>
    /// <param name="value">The new value for <paramref name="status"/>.<paramref name="tagName"/>.</param>
    private static void ApplyTagValue(CheckerStatus status, string tagName, object? value)
    {
        switch (tagName)
        {
            case "CheckerState":
                status.CheckerState = Convert.ToInt32(value);
                status.HasCheckerState = true;
                break;
            case "CheckerStatusMessage":
                status.StatusMessage = value?.ToString() ?? string.Empty;
                break;
            case "CheckerAlarmMessage":
                status.AlarmMessage = value?.ToString() ?? string.Empty;
                break;
            case "WatchdogDateTime":
                status.WatchdogDateTime = ConvertToDateTime(value);
                break;
            case "ExpectedChecksum":
                status.ExpectedChecksum = value?.ToString() ?? string.Empty;
                break;
            case "ActualChecksum":
                status.ActualChecksum = value?.ToString() ?? string.Empty;
                break;
            case "ShutdownRequestACK":
                status.ShutdownRequestAck = Convert.ToBoolean(value);
                break;
            case "RebootRequestACK":
                status.RebootRequestAck = Convert.ToBoolean(value);
                break;
        }
    }

    /// <summary>
    /// Attempts to convert the <paramref name="value"/> object to a datetime.
    /// </summary>
    /// <param name="value">The onject to convert to a datetime.</param>
    /// <returns>A new <see cref="DateTime"/> representing <paramref name="value"/>, or null if conversion failed.</returns>
    private static DateTime? ConvertToDateTime(object? value)
    {
        if (value is null)
        {
            return null;
        }

        if (value is DateTime dateTime)
        {
            return dateTime;
        }

        if (value is DateTimeOffset dateTimeOffset)
        {
            return dateTimeOffset.LocalDateTime;
        }

        return DateTime.TryParse(value.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsed)
            ? parsed
            : null;
    }

    /// <summary>
    /// Updates the local state store with the new <paramref name="value"/> of <paramref name="nodeId"/>.
    /// </summary>
    /// <param name="nodeId">The OPC tag that was changed.</param>
    /// <param name="value">The new value stored in <paramref name="nodeId"/>.</param>
    private void HandleTagChange(string nodeId, object? value)
    {
        if (!OpcNodeIds.TryParseStatusTag(nodeId, out int finalId, out string tagName))
        {
            return;
        }

        this.stateStore.Update(finalId, status => ApplyTagValue(status, tagName, value));
    }
}
