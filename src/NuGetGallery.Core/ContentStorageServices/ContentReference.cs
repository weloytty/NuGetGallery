using System;

namespace NuGetGallery.ContentStorageServices
{
    public class ContentReference<T>
    {
        public ContentReference(T content, string contentId)
        {
            if (string.IsNullOrEmpty(contentId))
            {
                throw new ArgumentException(nameof(contentId));
            }

            if (content == null)
            {
                throw new ArgumentException(nameof(content));
            }

            Content = content;
            ContentId = contentId;
        }

        public T Content { get; }

        public string ContentId { get; }
    }
}
