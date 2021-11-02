using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Synergy.Common.Exceptions;

namespace Synergy.Underwriting.Domain
{
    public class FileId
    {
        protected FileId()
        {
        }

        public string EntityType { get; protected set; }

        public string Id { get; protected set; }

        public Guid EventId { get; protected set; }

        public string FileName { get; protected set; }

        public static FileId Generate(Guid eventId, string entityType, string friendlyName = null)
        {
            var fileName = $"{entityType}/event_{eventId}/{DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}/{Guid.NewGuid()}";

            if (string.IsNullOrWhiteSpace(friendlyName) == false)
            {
                fileName += "/" + friendlyName;
            }

            var id = fileName.Replace('/', ':');
            return new FileId()
            {
                EventId = eventId,
                FileName = fileName,
                Id = id,
            };
        }

        public static FileId Parse(string id)
        {
            var idMatch = Regex.Match(id, @"^(?<entityType>\w+):event_(?<eventId>.{36}):\d\d\d\d-\d\d-\d\d:.{36}(?<friendlyName>:.+)?$");
            if (idMatch.Success == false)
            {
                throw new ModelStateException("id", "Invalid id format");
            }

            var eventIdString = idMatch.Groups["eventId"].Value;

            if (Guid.TryParse(eventIdString, out var eventId) == false)
            {
                throw new ModelStateException("id", "Invalid id format");
            }

            var entityType = idMatch.Groups["entityType"].Value;

            return new FileId
            {
                EntityType = entityType,
                Id = id,
                EventId = eventId,
                FileName = id.Replace(':', '/'),
            };
        }
    }
}