// <copyright file="HealthState.cs" company="Stanley Electric US Co. Inc.">
// Copyright (c) 2026 Stanley Electric US Co. Inc. Licensed under the MIT License.
// </copyright>

namespace CheckerRemoteBridge.Models;

/// <summary>
/// Enum to represent the "health" classification for a checker station (based on time from last response/state).
/// </summary>
public enum HealthState
{
    /// <summary>
    /// Checker is offline or not ready.
    /// </summary>
    Offline,

    /// <summary>
    /// Checker is healthy — watchdog fresh and no active alarm.
    /// </summary>
    Green,

    /// <summary>
    /// Checker has a stale watchdog or active alarm.
    /// </summary>
    Red,

    /// <summary>
    /// Checker is starting up, shutting down, or rebooting.
    /// </summary>
    Transitioning,
}
