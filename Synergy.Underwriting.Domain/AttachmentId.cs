using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Synergy.Underwriting.Domain
{
    public class AttachmentId
    {
        private const string Pattern = @"^(?<entityType>\w+):(?<entityId>[a-z0-9]{8}-[a-z0-9]{4}-[a-z0-9]{4}-[a-z0-9]{4}-[a-z0-9]{12}):\d\d\d\d-\d\d-\d\d:[a-z0-9]{8}-[a-z0-9]{4}-[a-z0-9]{4}-[a-z0-9]{4}-[a-z0-9]{12}(-(?<friendlyName>.+))?$";

        protected AttachmentId()
        {
        }

        public string EntityType { get; protected set; }

        public Guid EntityId { get; protected set; }

        public string Id { get; protected set; }

        public string FileName { get; protected set; }

        public string FriendlyName { get; protected set; }

        public static AttachmentId Generate(string entityType, Guid entityId, string friendlyName = null)
        {
            var fileName = $"{entityType}/{entityId}/{DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}/{Guid.NewGuid()}";

            if (string.IsNullOrWhiteSpace(friendlyName) == false)
            {
                fileName += "-" + NameSerializer.Escape(friendlyName);
            }

            var id = fileName.Replace('/', ':');

            return new AttachmentId
            {
                FileName = fileName,
                Id = id,
                EntityType = entityType,
                EntityId = entityId,
                FriendlyName = friendlyName,
            };
        }

        public static bool IsValid(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            var idMatch = Regex.Match(id, Pattern);
            return idMatch.Success;
        }

        public static bool IsValid(string id, string entityType)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            var idMatch = Regex.Match(id, Pattern);
            return idMatch.Success && idMatch.Groups["entityType"].Value == entityType;
        }

        public static AttachmentId Parse(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new FormatException("AttachmentId can not be empty.");
            }

            var idMatch = Regex.Match(id, Pattern);
            if (idMatch.Success == false)
            {
                throw new FormatException("Invalid id format");
            }

            var entityType = idMatch.Groups["entityType"].Value;
            var entityId = Guid.Parse(idMatch.Groups["entityId"].Value);
            var friendlyName = idMatch.Groups["friendlyName"].Value;

            return new AttachmentId
            {
                EntityType = entityType,
                EntityId = entityId,
                Id = id,
                FileName = id.Replace(':', '/'),
                FriendlyName = string.IsNullOrWhiteSpace(friendlyName) ? null : NameSerializer.UnEscape(friendlyName),
            };
        }

        private static class NameSerializer
        {
            public static string Escape(string input)
            {
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
            }

            public static string UnEscape(string input)
            {
                try
                {
                    return Encoding.UTF8.GetString(Convert.FromBase64String(input));
                }
                catch (FormatException)
                {
                    return null;
                }
            }
        }
    }
}