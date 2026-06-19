// <copyright file="OpcLicenseRegistrar.cs" company="Stanley Electric US Co. Inc.">
// Copyright (c) 2026 Stanley Electric US Co. Inc. Licensed under the MIT License.
// </copyright>

namespace OpcUtilities;

using System.Reflection;
using OpcLabs.BaseLib.ComponentModel;

/// <summary>
/// Registers the embedded QuickOPC license once per process.
/// </summary>
internal static class OpcLicenseRegistrar
{
    private static int registered;

    /// <summary>
    /// Ensures the QuickOPC license has been applied for this process.
    /// </summary>
    public static void EnsureRegistered()
    {
        if (Interlocked.CompareExchange(ref registered, 1, 0) != 0)
        {
            return;
        }

        LicensingManagement.Instance.RegisterManagedResource(
            "QuickOPC",
            "Multipurpose",
            Assembly.GetExecutingAssembly(),
            "OpcUtilities.license.bin");
    }
}
