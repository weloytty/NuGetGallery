// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using NuGet.Services.Entities;
using NuGet.Services.FeatureFlags;
using NuGetGallery.Auditing;
using NuGetGallery.ContentStorageServices;

namespace NuGetGallery.Features 
{
    public class EditableFeatureFlagFileStorageService : EditableContentFileStorageService<FeatureFlags>, IEditableFeatureFlagStorageService, IFeatureFlagStorageService
    {
        private const int MaxRemoveUserAttempts = 3;

        private readonly ILogger<EditableFeatureFlagFileStorageService> _logger;

        public EditableFeatureFlagFileStorageService(
            ICoreFileStorageService storage,
            IAuditingService auditing,
            ILogger<EditableFeatureFlagFileStorageService> logger) : base(storage, auditing)
        {            
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<FeatureFlags> GetAsync()
        {
            return await GetAsync(CoreConstants.FeatureFlagsFileName);
        }

        public async Task RemoveUserAsync(User user)
        {
            for (var attempt = 0; attempt < MaxRemoveUserAttempts; attempt++)
            {
                var reference = await _storage.GetFileReferenceAsync(CoreConstants.Folders.ContentFolderName, CoreConstants.FeatureFlagsFileName);

                FeatureFlags flags;
                using (var stream = reference.OpenRead())
                using (var streamReader = new StreamReader(stream))
                using (var reader = new JsonTextReader(streamReader))
                {
                    flags = Serializer.Deserialize<FeatureFlags>(reader);
                }

                // Don't update the flags if the user isn't listed in any of the flights.
                if (!flags.Flights.Any(f => f.Value.Accounts.Contains(user.Username, StringComparer.OrdinalIgnoreCase)))
                {
                    return;
                }

                // The user is listed in the flights. Build a new feature flag object that
                // no longer contains the user.
                var result = new FeatureFlags(
                   flags.Features,
                   flags.Flights
                       .ToDictionary(
                           f => f.Key,
                           f => RemoveUser(f.Value, user)));

                var saveResult = await TrySaveAsync(result, reference.ContentId, CoreConstants.FeatureFlagsFileName);
                if (saveResult == ContentSaveResult.Ok)
                {
                    return;
                }

                _logger.LogWarning(
                    0,
                    "Failed to remove user from feature flags, attempt {Attempt} of {MaxAttempts}...",
                    attempt + 1,
                    MaxRemoveUserAttempts);
            }

            throw new InvalidOperationException($"Unable to remove user from feature flags after {MaxRemoveUserAttempts} attempts");
        }        

        private Flight RemoveUser(Flight flight, User user)
        {
            return new Flight(
                flight.All,
                flight.SiteAdmins,
                flight.Accounts.Where(a => !a.Equals(user.Username, StringComparison.OrdinalIgnoreCase)).ToList(),
                flight.Domains);
        }        
    }
}
