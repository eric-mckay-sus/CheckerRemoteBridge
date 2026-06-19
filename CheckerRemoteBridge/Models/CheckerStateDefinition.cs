// <copyright file="CheckerStateDefinition.cs" company="Stanley Electric US Co. Inc.">
// Copyright (c) 2026 Stanley Electric US Co. Inc. Licensed under the MIT License.
// </copyright>

namespace CheckerRemoteBridge.Models;

/// <summary>
/// Connects status code to meaning and color to be used.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CheckerStateDefinition"/> class.
/// </remarks>
/// <param name="label">The human-readable label.</param>
/// <param name="color">The CSS color used when rendering the state.</param>
public sealed class CheckerStateDefinition(string label, string color)
{
    private static readonly Dictionary<int, CheckerStateDefinition> Definitions = new ()
    {
        [0] = new CheckerStateDefinition("Offline / Not Ready", "#64748b"),
        [1] = new CheckerStateDefinition("Alarm / Error", "#dc3545"),
        [2] = new CheckerStateDefinition("Idle — Manual Mode", "#fd7e14"),
        [3] = new CheckerStateDefinition("Idle — Auto Ready", "#0d6efd"),
        [4] = new CheckerStateDefinition("Test in Progress", "#6f42c1"),
        [5] = new CheckerStateDefinition("Test — Ext. Step Wait", "#d63384"),
        [6] = new CheckerStateDefinition("Test Complete", "#198754"),
        [7] = new CheckerStateDefinition("Test Aborted", "#fd7e14"),
        [100] = new CheckerStateDefinition("Starting Up", "#0dcaf0"),
        [101] = new CheckerStateDefinition("Shutting Down", "#0dcaf0"),
        [102] = new CheckerStateDefinition("Rebooting", "#0dcaf0"),
    };

    private static readonly CheckerStateDefinition Unknown = new ("Unknown", "#6c757d");

    /// <summary>
    /// Gets the human-readable label.
    /// </summary>
    public string Label { get; } = label;

    /// <summary>
    /// Gets the CSS color used when rendering the state.
    /// </summary>
    public string Color { get; } = color;

    /// <summary>
    /// Resolves display metadata for a checker state code.
    /// </summary>
    /// <param name="stateCode">The numeric checker state.</param>
    /// <returns>Display metadata for the state.</returns>
    public static CheckerStateDefinition For(int stateCode) =>
        Definitions.TryGetValue(stateCode, out CheckerStateDefinition? definition)
            ? definition
            : Unknown;
}
