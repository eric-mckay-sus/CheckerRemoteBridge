// <copyright file="CheckerStateStore.cs" company="Stanley Electric US Co. Inc.">
// Copyright (c) 2026 Stanley Electric US Co. Inc. Licensed under the MIT License.
// </copyright>

namespace CheckerRemoteBridge.Services;

using System.Collections.Concurrent;
using CheckerRemoteBridge.Models;

/// <summary>
/// In-memory store of live checker status, updated by OPC subscriptions.
/// </summary>
public sealed class CheckerStateStore
{
    private readonly ConcurrentDictionary<int, CheckerStatus> states = new ();

    /// <summary>
    /// Raised when a station's status changes.
    /// </summary>
    public event Action<int>? StatusChanged;

    /// <summary>
    /// Gets the current status for a final station, creating a default entry if needed.
    /// </summary>
    /// <param name="finalId">The final station number (1-based).</param>
    /// <returns>The current status snapshot.</returns>
    public CheckerStatus Get(int finalId) => this.states.GetOrAdd(finalId, _ => new CheckerStatus());

    /// <summary>
    /// Applies a mutation to a station status and notifies subscribers.
    /// </summary>
    /// <param name="finalId">The final station number (1-based).</param>
    /// <param name="mutate">The mutation to apply.</param>
    public void Update(int finalId, Action<CheckerStatus> mutate)
    {
        CheckerStatus status = this.states.GetOrAdd(finalId, _ => new CheckerStatus());
        mutate(status);
        this.StatusChanged?.Invoke(finalId);
    }
}
