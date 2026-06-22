// <copyright file="EasyUAOpcClient.cs" company="Stanley Electric US Co. Inc.">
// Copyright (c) 2026 Stanley Electric US Co. Inc. Licensed under the MIT License.
// </copyright>

namespace OpcUtilities;

using OpcLabs.EasyOpc.UA;
using OpcLabs.EasyOpc.UA.Extensions;
using OpcLabs.EasyOpc.UA.OperationModel;

/// <summary>
/// OPC UA client backed by QuickOPC <see cref="EasyUAClient"/>.
/// </summary>
public sealed class EasyUAOpcClient : IOpcClient
{
    private readonly EasyUAClient client = new ();
    private readonly UAEndpointDescriptor endpointDescriptor;
    private bool disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="EasyUAOpcClient"/> class.
    /// </summary>
    /// <param name="endpointUri">The OPC UA endpoint URI.</param>
    /// <param name="username">The OPC username.</param>
    /// <param name="password">The OPC password.</param>
    public EasyUAOpcClient(string endpointUri, string username, string password)
    {
        OpcLicenseRegistrar.EnsureRegistered();
        EasyUAClientCore.SharedParameters.EngineParameters.CertificateAcceptancePolicy.AcceptAnyCertificate = true;
        this.endpointDescriptor = ((UAEndpointDescriptor)endpointUri).WithUserNameIdentity(username, password);
    }

    /// <inheritdoc/>
    public bool IsConfigured => true;

    /// <summary>
    /// Attempts to create a client from the standard OPC environment variables.
    /// </summary>
    /// <param name="client">The created client when configuration is present.</param>
    /// <returns><see langword="true"/> when all required environment variables are set.</returns>
    public static bool TryCreateFromEnvironment(out EasyUAOpcClient? client)
    {
        if (!TryGetEnvironmentValue("OPC_URI", out string? endpointUri)
            || !TryGetEnvironmentValue("OPC_USER", out string? username)
            || !TryGetEnvironmentValue("OPC_PASS", out string? password))
        {
            client = null;
            return false;
        }

        client = new EasyUAOpcClient(endpointUri, username, password);
        return true;
    }

    /// <inheritdoc/>
    public async Task<object?> ReadAsync(string nodeId, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(this.disposed, this);

        try
        {
            return await Task.Run(() => this.client.ReadValue(this.endpointDescriptor, nodeId), cancellationToken)
                .ConfigureAwait(false);
        }
        catch (UAException)
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> WriteAsync(string nodeId, object value, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(this.disposed, this);

        try
        {
            await Task.Run(() => this.client.WriteValue(this.endpointDescriptor, nodeId, value), cancellationToken)
                .ConfigureAwait(false);
            return true;
        }
        catch (UAException)
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public void SubscribeDataChange(string nodeId, int updateRateMs, Action<string, object?> onChange)
    {
        ObjectDisposedException.ThrowIf(this.disposed, this);

        this.client.SubscribeDataChange(
            this.endpointDescriptor,
            nodeId,
            updateRateMs,
            (_, eventArgs) =>
            {
                if (eventArgs.Succeeded)
                {
                    onChange(nodeId, eventArgs.AttributeData.Value);
                }
            });
    }

    /// <inheritdoc/>
    public void UnsubscribeAll()
    {
        if (this.disposed)
        {
            return;
        }

        this.client.UnsubscribeAllMonitoredItems();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (this.disposed)
        {
            return;
        }

        this.UnsubscribeAll();
        this.disposed = true;
    }

    private static bool TryGetEnvironmentValue(string key, out string value)
    {
        value = Environment.GetEnvironmentVariable(key) ?? string.Empty;
        return !string.IsNullOrWhiteSpace(value);
    }
}

/// <summary>
/// No-op OPC client used when connection settings are unavailable.
/// </summary>
public sealed class NullOpcClient : IOpcClient
{
    /// <inheritdoc/>
    public bool IsConfigured => false;

    /// <inheritdoc/>
    public Task<object?> ReadAsync(string nodeId, CancellationToken cancellationToken = default) =>
        Task.FromResult<object?>(null);

    /// <inheritdoc/>
    public Task<bool> WriteAsync(string nodeId, object value, CancellationToken cancellationToken = default) =>
        Task.FromResult(false);

    /// <inheritdoc/>
    public void SubscribeDataChange(string nodeId, int updateRateMs, Action<string, object?> onChange)
    {
    }

    /// <inheritdoc/>
    public void UnsubscribeAll()
    {
    }

    /// <inheritdoc/>
    public void Dispose()
    {
    }
}
