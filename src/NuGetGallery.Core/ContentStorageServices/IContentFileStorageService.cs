using System;
using System.Threading.Tasks;

namespace NuGetGallery.ContentStorageServices
{
    public interface IContentFileStorageService<T>
    {
        Task<T> GetAsync(string contentFileName);
    }
}
