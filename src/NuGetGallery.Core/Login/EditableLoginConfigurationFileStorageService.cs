// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using NuGetGallery.Auditing;
using NuGetGallery.ContentStorageServices;

namespace NuGetGallery.Login
{
    public class EditableLoginConfigurationFileStorageService : EditableContentFileStorageService<LoginDiscontinuation>, IEditableLoginConfigurationFileStorageService
    {
        private const int MaxAttempts = 3;
        private readonly ILogger<EditableLoginConfigurationFileStorageService> _logger;

        public EditableLoginConfigurationFileStorageService(
            ICoreFileStorageService storage,
            IAuditingService auditing,
            ILogger<EditableLoginConfigurationFileStorageService> logger) : base(storage, auditing)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task AddUserEmailAddressforPasswordAuthenticationAsync(string emailAddress, bool add)
        {
            for (var attempt = 0; attempt < MaxAttempts; attempt++)
            {
                var reference = await _storage.GetFileReferenceAsync(CoreConstants.Folders.ContentFolderName, CoreConstants.LoginDiscontinuationConfigFileName);

                LoginDiscontinuation logins;
                using (var stream = reference.OpenRead())
                using (var streamReader = new StreamReader(stream))
                using (var reader = new JsonTextReader(streamReader))
                {
                    logins = Serializer.Deserialize<LoginDiscontinuation>(reader);
                }

                var exceptionsForEmailAddresses = logins.ExceptionsForEmailAddresses;
                if (add)
                {

                    if (logins.ExceptionsForEmailAddresses.Contains(emailAddress))
                    {
                        return;
                    }
                    exceptionsForEmailAddresses.Add(emailAddress);
                }
                else
                {
                    if (!logins.ExceptionsForEmailAddresses.Contains(emailAddress))
                    {
                        return;
                    }
                    exceptionsForEmailAddresses.Remove(emailAddress);
                }

                var result = new LoginDiscontinuation(
                   logins.DiscontinuedForEmailAddresses,
                   logins.DiscontinuedForDomains,
                   exceptionsForEmailAddresses,
                   logins.ForceTransformationToOrganizationForEmailAddresses,
                   logins.EnabledOrganizationAadTenants,
                   logins.IsPasswordDiscontinuedForAll);

                var saveResult = await TrySaveAsync(result, reference.ContentId, CoreConstants.LoginDiscontinuationConfigFileName);
                if (saveResult == ContentSaveResult.Ok)
                {
                    return;
                }

                var operation = add ? "add" : "remove";
                _logger.LogWarning(
                    "Failed to {operation} emailAddress from exception list, attempt {Attempt} of {MaxAttempts}...",
                    operation,
                    attempt + 1,
                    MaxAttempts);
            }

            throw new InvalidOperationException($"Unable to add/remove emailAddress from exception list after {MaxAttempts} attempts");
        }

        public async Task<IReadOnlyList<string>> GetListOfExceptionEmailList()
        {
            var loginDiscontinuation = await GetAsync(CoreConstants.LoginDiscontinuationConfigFileName);

            return loginDiscontinuation.ExceptionsForEmailAddresses.ToList();
        }
    }
}