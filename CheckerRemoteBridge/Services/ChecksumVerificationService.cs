// <copyright file="ChecksumVerificationService.cs" company="Stanley Electric US Co. Inc.">
// Copyright (c) 2026 Stanley Electric US Co. Inc. Licensed under the MIT License.
// </copyright>

namespace CheckerRemoteBridge.Services;

using CheckerRemoteBridge.Models;

/// <summary>
/// Service responsible for comparing expected and actual checksum values on a checker station.
/// </summary>
public static class ChecksumVerificationService
{
    /// <summary>
    /// Enumerates the possible checksum comparison results.
    /// </summary>
    public enum ChecksumResult
    {
        /// <summary>
        /// No actual checksum has been reported yet, or no expected checksum value registered.
        /// </summary>
        Pending,

        /// <summary>
        /// Actual and expected checksums match.
        /// </summary>
        Match,

        /// <summary>
        /// Actual and expected checksums differ.
        /// </summary>
        Mismatch,
    }

    /// <summary>
    /// Evaluates checksum status for a checker station.
    /// </summary>
    /// <param name="status">The live checker status.</param>
    /// <returns>The <see cref="ChecksumResult"/>.</returns>
    public static ChecksumResult Evaluate(CheckerStatus status)
    {
        if (string.IsNullOrWhiteSpace(status.ActualChecksum))
        {
            return ChecksumResult.Pending;
        }

        return string.Equals(status.ActualChecksum, status.ExpectedChecksum, StringComparison.OrdinalIgnoreCase)
            ? ChecksumResult.Match
            : ChecksumResult.Mismatch;
    }
}
