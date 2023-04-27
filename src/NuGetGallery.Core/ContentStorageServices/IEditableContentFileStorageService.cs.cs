using System;
using System.Threading.Tasks;

namespace NuGetGallery.ContentStorageServices
{
    public interface IEditableContentFileStorageService<T> : IContentFileStorageService<T>
    {
        Task<ContentReference<T>> GetReferenceAsync(string contentFileName);

        Task<ContentSaveResult> TrySaveAsync(T content, string contentId, string contentFileName);
    }
}
