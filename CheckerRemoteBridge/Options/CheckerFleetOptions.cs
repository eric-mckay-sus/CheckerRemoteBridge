// <copyright file="CheckerFleetOptions.cs" company="Stanley Electric US Co. Inc.">
// Copyright (c) 2026 Stanley Electric US Co. Inc. Licensed under the MIT License.
// </copyright>

namespace CheckerRemoteBridge.Options;

/// <summary>
/// Fleet-wide configuration for checker monitoring and actions.
/// </summary>
public sealed class CheckerFleetOptions
{
    /// <summary>
    /// Gets or sets the configuration section name.
    /// </summary>
    public const string SectionName = "CheckerFleet";

    /// <summary>
    /// Gets or sets the number of final stations to monitor.
    /// </summary>
    public int FinalCount { get; set; } = 5;

    /// <summary>
    /// Gets or sets the display name for the OPC server shown in the UI.
    /// </summary>
    public string ServerDisplayName { get; set; } = "SUS-KEPWARE-02:49320";

    /// <summary>
    /// Gets or sets the maximum watchdog age in seconds before a station is marked red.
    /// </summary>
    public int WatchdogStaleSeconds { get; set; } = 10;

    /// <summary>
    /// Gets or sets the OPC subscription update rate in milliseconds.
    /// </summary>
    public int SubscriptionUpdateRateMs { get; set; } = 1000;
}
