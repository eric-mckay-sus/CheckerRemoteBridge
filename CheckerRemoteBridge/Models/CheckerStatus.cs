// <copyright file="CheckerStatus.cs" company="Stanley Electric US Co. Inc.">
// Copyright (c) 2026 Stanley Electric US Co. Inc. Licensed under the MIT License.
// </copyright>

namespace CheckerRemoteBridge.Models;

/// <summary>
/// Live OPC-backed status for a single checker final station.
/// </summary>
public sealed class CheckerStatus
{
    /// <summary>
    /// Gets or sets the numeric checker state code from OPC.
    /// </summary>
    public int CheckerState { get; set; }

    /// <summary>
    /// Gets or sets the human-readable status message from OPC.
    /// </summary>
    public string StatusMessage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the active alarm message from OPC, if any.
    /// </summary>
    public string AlarmMessage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the latest watchdog timestamp reported by the checker.
    /// </summary>
    public DateTime? WatchdogDateTime { get; set; }

    /// <summary>
    /// Gets or sets the expected checksum from MES via OPC.
    /// </summary>
    public string ExpectedChecksum { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the actual checksum reported at startup via OPC.
    /// </summary>
    public string ActualChecksum { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the auto-launch request was acknowledged.
    /// </summary>
    public bool AutoRequestAck { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the reset request was acknowledged.
    /// </summary>
    public bool ResetRequestAck { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the shutdown request was acknowledged.
    /// </summary>
    public bool ShutdownRequestAck { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the reboot request was acknowledged.
    /// </summary>
    public bool RebootRequestAck { get; set; }
}
