using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Synergy.Underwriting.Domain
{
    public class PropertyFileId
    {
        public PropertyFileId()
        {
        }

        public string EntityType { get; protected set; }

        public string Id { get; protected set; }

        public Guid PropertyId { get; protected set; }

        public string FileName { get; protected set; }

        public static PropertyFileId Generate(Guid propertyId, string entityType, string friendlyName = null)
        {
            var fileName = $"{entityType}/property_{propertyId}/{DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}/{Guid.NewGuid()}";

            if (string.IsNullOrWhiteSpace(friendlyName) == false)
            {
                fileName += "/" + friendlyName;
            }

            var id = fileName.Replace('/', ':');
            return new PropertyFileId()
            {
                PropertyId = propertyId,
                FileName = fileName,
                Id = id,
            };
        }
    }
}
