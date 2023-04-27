// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NuGetGallery.ContentStorageServices;

namespace NuGetGallery.Login
{
    public interface IEditableLoginConfigurationFileStorageService: IEditableContentFileStorageService<LoginDiscontinuation>
    {
        Task AddUserEmailAddressforPasswordAuthenticationAsync(string emailAddress, bool add);

        Task<IReadOnlyList<string>> GetListOfExceptionEmailList();
    }
}
