// <copyright file="PiControlServiceStub.cs" company="Stanley Electric US Co. Inc.">
// Copyright (c) 2026 Stanley Electric US Co. Inc. Licensed under the MIT License.
// </copyright>

namespace CheckerRemoteBridge.Services;

/// <summary>
/// Placeholder Pi control service until SSH.NET integration is added.
/// </summary>
public sealed class PiControlServiceStub : IPiControlService
{
    /// <inheritdoc/>
    public bool IsConfigured => false;

    /// <inheritdoc/>
    public Task<bool> LaunchAsync(int finalId, CancellationToken cancellationToken = default) =>
        Task.FromResult(false);

    /// <inheritdoc/>
    public Task<bool> IsReachableAsync(int finalId, CancellationToken cancellationToken = default) =>
        Task.FromResult(false);
}
