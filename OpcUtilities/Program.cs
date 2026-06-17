// <copyright file="Program.cs" company="Stanley Electric US Co. Inc.">
// Copyright (c) 2026 Stanley Electric US Co. Inc. Licensed under the MIT License.
// </copyright>

namespace OpcUtilities;

using System.Reflection;
using OpcLabs.BaseLib.ComponentModel;
using OpcLabs.EasyOpc.UA;
using OpcLabs.EasyOpc.UA.Extensions;
using OpcLabs.EasyOpc.UA.OperationModel;

/// <summary>
/// Utilities for interfacing with OPC software using the EasyOpc SDK from OPC Labs.
/// </summary>
public static class OpcUtilities
{
    /// <summary>
    /// Describes the connection to the OPC server.
    /// </summary>
    private static readonly UAEndpointDescriptor EndpointDescriptor = ((UAEndpointDescriptor)GetRequired("OPC_URI")).WithUserNameIdentity(GetRequired("OPC_USER"), GetRequired("OPC_PASS"));

    /// <summary>
    /// Describes this particular OPC client.
    /// </summary>
    private static readonly EasyUAClient Client = new ();

    /// <summary>
    /// Entry point for the program. Builds a node ID from its constituent parts, then performs the desired read/write.
    /// </summary>
    /// <returns>A Task representing that the program is complete.</returns>
    public static async Task Main()
    {
        RegisterLicense();
        bool repeat = true;
        while (repeat)
        {
            Console.Write("Please indicate whether you would like to read or write to the target tag: ");
            string response = Console.ReadLine() ?? string.Empty;
            bool isRead = response.Equals("read", StringComparison.OrdinalIgnoreCase);

            string?[] nodeIdBuilder = new string?[4];
            Console.Write("Please enter channel name: ");
            nodeIdBuilder[0] = Console.ReadLine();
            Console.Write("Please enter device name: ");
            nodeIdBuilder[1] = Console.ReadLine();
            Console.Write("Please enter tag group (if applicable): ");
            nodeIdBuilder[2] = Console.ReadLine();
            Console.Write("Please enter tag name: ");
            nodeIdBuilder[3] = Console.ReadLine();

            string nodeId = "ns=2;s=" + string.Join('.', nodeIdBuilder.Where(s => !string.IsNullOrEmpty(s)));

            if (isRead)
            {
                await ReadOpcTag(nodeId);
            }
            else
            {
                object? toWrite = null;
                while (toWrite is null)
                {
                    Console.Write("Please enter the desired value for this tag: ");
                    toWrite = Console.ReadLine();
                }

                await WriteOpcTag(nodeId, toWrite);
            }

            Console.Write("Do you wish to process another tag? (y/n): ");
            char confirmation = (char)Console.ReadKey().Key;
            Console.WriteLine();
            repeat = confirmation.Equals('y') || confirmation.Equals('Y');
        }
    }

    /// <summary>
    /// Registers the license binary (must be in the solution at OpcUtilities/license.bin).
    /// </summary>
    private static void RegisterLicense()
    {
        try
            {
                LicensingManagement.Instance.RegisterManagedResource(
                    "QuickOPC",
                    "Multipurpose",
                    Assembly.GetExecutingAssembly(),
                    "OpcUtilities.license.bin");

                Console.WriteLine("License applied successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to apply license: {ex.Message}");
            }
    }

    /// <summary>
    /// Attempts to read the value from the OPC node identified by <paramref name="nodeId"/>.
    /// Does NOT validate new <paramref name="nodeId"/>. Self-reports exceptions without throwing.
    /// </summary>
    /// <param name="nodeId">The OPC ID of the tag to write to.</param>
    /// <returns>A value indicating whether the write was successful.</returns>
    private static async Task<string> ReadOpcTag(string nodeId)
    {
        EasyUAClientCore.SharedParameters.EngineParameters.CertificateAcceptancePolicy.AcceptAnyCertificate = true;

        try
        {
            Console.WriteLine("Reading OPC node value...");

            // Connection is implicitly opened and managed during the operation
            object value = IEasyUAClientExtension.ReadValue(Client, EndpointDescriptor, nodeId);

            Console.WriteLine($"Successfully read {nodeId.Split('.')[^1]} tag: {value}");
            return value.ToString() ?? string.Empty;
        }
        catch (UAException ex)
        {
            Console.WriteLine($"OPC Error: {ex.Message}");
            return ex.Message;
        }
    }

    /// <summary>
    /// Attempts to write the specified <paramref name="value"/> to the OPC node identified by <paramref name="nodeId"/>.
    /// Does NOT validate <paramref name="nodeId"/> or new <paramref name="value"/>. Self-reports exceptions without throwing.
    /// </summary>
    /// <param name="nodeId">The OPC ID of the tag to write to.</param>
    /// <param name="value">The value to write.</param>
    /// <returns>A value indicating whether the write was successful.</returns>
    private static async Task<bool> WriteOpcTag(string nodeId, object value)
    {
        EasyUAClientCore.SharedParameters.EngineParameters.CertificateAcceptancePolicy.AcceptAnyCertificate = true;

        try
        {
            Console.WriteLine("Writing OPC node value...");

            // Connection is implicitly opened and managed during the operation
            Client.WriteValue(EndpointDescriptor, nodeId, value);

            Console.WriteLine($"Successfully wrote {value} to {nodeId.Split('.')[^1]} tag");
            return true;
        }
        catch (UAException ex)
        {
            Console.WriteLine($"OPC Error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Gets a required value from the environment.
    /// </summary>
    /// <param name="key">The key to get the environment variable.</param>
    /// <returns>The value associated with the key, or <see cref="InvalidOperationException"/> if not found.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="key"/> cannot be found in the environment.</exception>
    private static string GetRequired(string key)
    {
        string? value = Environment.GetEnvironmentVariable(key);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Required environment variable '{key}' is missing for OPC connection.");
        }

        return value;
    }
}
