using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using NuGetGallery.Auditing;
using NuGetGallery.ContentStorageServices;

namespace NuGetGallery.ContentStorageServices
{
    public class EditableContentFileStorageService<T> : ContentFileStorageService<T>, IEditableContentFileStorageService<T>
    {
        // Implement auditing when updating a content file
        private readonly IAuditingService _auditing;

        public EditableContentFileStorageService(ICoreFileStorageService storage, IAuditingService auditing) : base(storage)
        {
            _auditing = auditing ?? throw new ArgumentNullException(nameof(auditing));
        }

        public async Task<ContentReference<T>> GetReferenceAsync(string contentFileName)
        {
            var reference = await _storage.GetFileReferenceAsync(CoreConstants.Folders.ContentFolderName, contentFileName);

            return new ContentReference<T>(
                ReadContentFromStream(reference.OpenRead()),
                reference.ContentId);
        }

        public async Task<ContentSaveResult> TrySaveAsync(T content, string contentId, string contentFileName)
        {
            var result = await TrySaveInternalAsync(content, contentId, contentFileName);

            return result;
        }

        private async Task<ContentSaveResult> TrySaveInternalAsync(T content, string contentId, string contentFileName)
        {
            var accessCondition = AccessConditionWrapper.GenerateIfMatchCondition(contentId);

            try
            {
                using (var stream = new MemoryStream())
                using (var writer = new StreamWriter(stream))
                using (var jsonWriter = new JsonTextWriter(writer))
                {
                    Serializer.Serialize(jsonWriter, content);
                    jsonWriter.Flush();
                    stream.Position = 0;

                    await _storage.SaveFileAsync(CoreConstants.Folders.ContentFolderName, contentFileName, stream, accessCondition);

                    return ContentSaveResult.Ok;
                }
            }
            catch (StorageException e) when (e.IsPreconditionFailedException())
            {
                return ContentSaveResult.Conflict;
            }
        }
    }
}
