// <copyright file="OpcNodeIds.cs" company="Stanley Electric US Co. Inc.">
// Copyright (c) 2026 Stanley Electric US Co. Inc. Licensed under the MIT License.
// </copyright>

namespace CheckerRemoteBridge.Services;

/// <summary>
/// Builds OPC node identifiers for checker status tags.
/// </summary>
public static class OpcNodeIds
{
    /// <summary>
    /// Status tags subscribed by <see cref="OpcMonitorService"/>.
    /// </summary>
    public static readonly string[] StatusTags =
    [
        "CheckerState",
        "CheckerStatusMessage",
        "CheckerAlarmMessage",
        "AutoRequest",
        "AutoRequestACK",
        "ResetRequest",
        "ResetRequestACK",
        "ShutdownRequest",
        "ShutdownRequestACK",
        "RebootRequest",
        "RebootRequestACK",
        "WatchdogDateTime",
        "ExpectedChecksum",
        "ActualChecksum",
    ];

    /// <summary>
    /// Builds the node identifier for a status tag on a final station.
    /// </summary>
    /// <param name="finalId">The final station number (1-based).</param>
    /// <param name="tagName">The tag name within the Status group.</param>
    /// <returns>The fully qualified OPC node identifier.</returns>
    public static string StatusTag(int finalId, string tagName) =>
        $"ns=2;s=ANT1.Final{finalId}.Status.{tagName}";

    /// <summary>
    /// Parses a final station id from a subscribed node identifier.
    /// </summary>
    /// <param name="nodeId">The fully qualified OPC node identifier.</param>
    /// <param name="finalId">The parsed final id when successful.</param>
    /// <param name="tagName">The parsed tag name when successful.</param>
    /// <returns><see langword="true"/> when the node id matches the expected pattern.</returns>
    public static bool TryParseStatusTag(string nodeId, out int finalId, out string tagName)
    {
        finalId = 0;
        tagName = string.Empty;

        const string prefix = "ns=2;s=ANT1.Final";
        if (!nodeId.StartsWith(prefix, StringComparison.Ordinal))
        {
            return false;
        }

        int dotIndex = nodeId.IndexOf('.', prefix.Length);
        if (dotIndex < 0 || !int.TryParse(nodeId[prefix.Length..dotIndex], out finalId))
        {
            return false;
        }

        string suffix = $".Final{finalId}.Status.";
        int statusIndex = nodeId.IndexOf(suffix, StringComparison.Ordinal);
        if (statusIndex < 0)
        {
            return false;
        }

        tagName = nodeId[(statusIndex + suffix.Length)..];
        return tagName.Length > 0;
    }
}
