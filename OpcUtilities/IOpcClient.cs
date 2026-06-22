// <copyright file="IOpcClient.cs" company="Stanley Electric US Co. Inc.">
// Copyright (c) 2026 Stanley Electric US Co. Inc. Licensed under the MIT License.
// </copyright>

namespace OpcUtilities;

/// <summary>
/// Abstraction over OPC UA read, write, and subscription operations.
/// </summary>
public interface IOpcClient : IDisposable
{
    /// <summary>
    /// Gets a value indicating whether the client has a valid endpoint configuration.
    /// </summary>
    bool IsConfigured { get; }

    /// <summary>
    /// Reads the current value of an OPC node.
    /// </summary>
    /// <param name="nodeId">The fully qualified node identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The node value, or <see langword="null"/> when the read fails.</returns>
    Task<object?> ReadAsync(string nodeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes a value to an OPC node.
    /// </summary>
    /// <param name="nodeId">The fully qualified node identifier.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns><see langword="true"/> when the write succeeds.</returns>
    Task<bool> WriteAsync(string nodeId, object value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Subscribes to data changes for a single OPC node.
    /// </summary>
    /// <param name="nodeId">The fully qualified node identifier.</param>
    /// <param name="updateRateMs">The requested update rate in milliseconds.</param>
    /// <param name="onChange">Callback invoked when the node value changes.</param>
    void SubscribeDataChange(string nodeId, int updateRateMs, Action<string, object?> onChange);

    /// <summary>
    /// Removes all active monitored-item subscriptions.
    /// </summary>
    void UnsubscribeAll();
}
