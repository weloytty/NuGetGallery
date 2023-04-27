using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NuGetGallery.ContentStorageServices
{
    public class ContentFileStorageService<T> : IContentFileStorageService<T>
    {
        protected static readonly JsonSerializer Serializer;

        protected readonly ICoreFileStorageService _storage;
        
        static ContentFileStorageService()
        {
            Serializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                MissingMemberHandling = MissingMemberHandling.Error,
                Converters = new List<JsonConverter>
                {
                    new StringEnumConverter()
                }
            });
        }

        public ContentFileStorageService(ICoreFileStorageService storage)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        public async Task<T> GetAsync(string contentFileName)
        {
            using (var stream = await _storage.GetFileAsync(CoreConstants.Folders.ContentFolderName, contentFileName))
            {
                return ReadContentFromStream(stream);
            }
        }

        protected T ReadContentFromStream(Stream stream)
        {
            using (var streamReader = new StreamReader(stream))
            using (var reader = new JsonTextReader(streamReader))
            {
                return Serializer.Deserialize<T>(reader);
            }
        }
    }
}