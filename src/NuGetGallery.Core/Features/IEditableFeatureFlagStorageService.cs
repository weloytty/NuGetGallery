// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using NuGet.Services.Entities;
using NuGet.Services.FeatureFlags;
using NuGetGallery.ContentStorageServices;

namespace NuGetGallery.Features
{
    public interface IEditableFeatureFlagStorageService : IEditableContentFileStorageService<FeatureFlags>
    {
        /// <summary>
        /// Remove the user from the feature flags if needed. This may throw on failure.
        /// </summary>
        /// <param name="user">The user to remove from feature flags.</param>
        Task RemoveUserAsync(User user);
    }
}
