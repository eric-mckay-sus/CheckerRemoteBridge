// <copyright file="HealthEvaluator.cs" company="Stanley Electric US Co. Inc.">
// Copyright (c) 2026 Stanley Electric US Co. Inc. Licensed under the MIT License.
// </copyright>

namespace CheckerRemoteBridge.Services;

using CheckerRemoteBridge.Models;

/// <summary>
/// Derives checker health from live status and the current clock.
/// </summary>
public static class HealthEvaluator
{
    /// <summary>
    /// Evaluates the health state for a checker station.
    /// </summary>
    /// <param name="status">The live checker status.</param>
    /// <param name="now">The current time used for watchdog age calculation.</param>
    /// <param name="watchdogStaleSeconds">Maximum allowed watchdog age in seconds.</param>
    /// <returns>The derived health state.</returns>
    public static HealthState Evaluate(CheckerStatus status, DateTime now, int watchdogStaleSeconds)
    {
        if (status.CheckerState == 0)
        {
            return HealthState.Offline;
        }

        if (status.CheckerState is 100 or 101 or 102)
        {
            return HealthState.Transitioning;
        }

        bool watchdogStale = status.WatchdogDateTime is null
            || (now - status.WatchdogDateTime.Value).TotalSeconds > watchdogStaleSeconds;
        bool hasAlarm = !string.IsNullOrWhiteSpace(status.AlarmMessage);
        return watchdogStale || hasAlarm ? HealthState.Red : HealthState.Green;
    }

    /// <summary>
    /// Gets the CSS color associated with a health state.
    /// </summary>
    /// <param name="health">The health state.</param>
    /// <returns>A CSS color string.</returns>
    public static string GetColor(HealthState health) => health switch
    {
        HealthState.Green => "#198754",
        HealthState.Red => "#dc3545",
        HealthState.Transitioning => "#0dcaf0",
        _ => "#64748b",
    };

    /// <summary>
    /// Gets the display label associated with a health state.
    /// </summary>
    /// <param name="health">The health state.</param>
    /// <returns>A short uppercase label.</returns>
    public static string GetLabel(HealthState health) => health switch
    {
        HealthState.Green => "GREEN",
        HealthState.Red => "RED",
        HealthState.Transitioning => "BUSY",
        _ => "OFFLINE",
    };

    /// <summary>
    /// Formats watchdog age for display.
    /// </summary>
    /// <param name="watchdogDateTime">The latest watchdog timestamp.</param>
    /// <param name="now">The current time.</param>
    /// <returns>A human-readable age string.</returns>
    public static string FormatWatchdogAge(DateTime? watchdogDateTime, DateTime now)
    {
        if (watchdogDateTime is null)
        {
            return "no signal";
        }

        int seconds = Math.Max(0, (int)(now - watchdogDateTime.Value).TotalSeconds);
        if (seconds < 60)
        {
            return $"{seconds}s ago";
        }

        if (seconds < 3600)
        {
            return $"{seconds / 60}m ago";
        }

        return $"{seconds / 3600}h ago";
    }
}
