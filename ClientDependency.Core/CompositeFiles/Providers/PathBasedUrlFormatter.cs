using System;
using System.Linq;
using System.Text;
using ClientDependency.Core;

namespace ClientDependency.Core.CompositeFiles.Providers
{
    internal static class PathBasedUrlFormatter
    {

        public static bool Parse(string pathBasedUrlFormat, string path, out string fileKey, out ClientDependencyType type, out int version)
        {
            //start parsing from the end
            var typeIndex = pathBasedUrlFormat.IndexOf("{type}");
            var versionIndex = pathBasedUrlFormat.IndexOf("{version}");

            var typeDelimiter = pathBasedUrlFormat.Substring(versionIndex + "{version}".Length, typeIndex - (versionIndex + "{version}".Length));
            var typeAsString = "";            
            for (var i = path.Length - 1; i > path.LastIndexOf(typeDelimiter); i--)
            {
                typeAsString += path[i];
            }
            typeAsString = typeAsString.ReverseString();

            var versionDelimiter = pathBasedUrlFormat.Substring("{dependencyId}".Length, versionIndex - ("{dependencyId}".Length));
            var versionAsString = "";
            for (var i = path.Length - 1; i > path.LastIndexOf(versionDelimiter); i--)
            {
                versionAsString += path[i];
            }
            versionAsString = versionAsString.ReverseString();

            fileKey = "";
            type = typeAsString == "js" ? ClientDependencyType.Javascript : ClientDependencyType.Css;
            version = 10;
            return true;

        }

        /// <summary>
        /// Creates a path based on the format, fileKey, type and version specified.
        /// </summary>
        /// <param name="pathBasedUrlFormat"></param>
        /// <param name="fileKey"></param>
        /// <param name="type"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static string CreatePath(string pathBasedUrlFormat, string fileKey, ClientDependencyType type, int version)
        {            
            var pathUrl = pathBasedUrlFormat;
            var dependencyId = new StringBuilder();
            int pos = 0;
            //split paths at a max of 240 chars to not exceed the max path length of a URL
            while (fileKey.Length > pos)
            {
                if (dependencyId.Length > 0)
                {
                    dependencyId.Append('/');
                }
                var len = Math.Min(fileKey.Length - pos, 240);
                dependencyId.Append(fileKey.Substring(pos, len));
                pos += 240;
            }
            pathUrl = pathUrl.Replace("{dependencyId}", dependencyId.ToString());
            pathUrl = pathUrl.Replace("{version}", version.ToString());
            switch (type)
            {
                case ClientDependencyType.Css:
                    pathUrl = pathUrl.Replace("{type}", "css");
                    break;
                case ClientDependencyType.Javascript:
                    pathUrl = pathUrl.Replace("{type}", "js");
                    break;
            }
            return pathUrl;
        }

        /// <summary>
        /// Ensures the url format is valid, if not an exception is thrown
        /// </summary>
        /// <param name="pathBasedUrl"></param>
        public static void Validate(string pathBasedUrl)
        {
            if (string.IsNullOrEmpty(pathBasedUrl))
            {
                throw new FormatException("The value specified for pathUrlFormat cannot be null or empty");
            }
            var pathBasedUrlFormat = pathBasedUrl;
            var pathChars = pathBasedUrlFormat.ToCharArray();
            //now we need to validate it:
            var requiredTokens = new[] { "{dependencyId}", "{version}", "{type}" };
            var latestIndex = -1;
            foreach (var r in requiredTokens)
            {
                var newIndex = pathBasedUrlFormat.IndexOf(r);
                if (latestIndex > -1 && newIndex < latestIndex)
                {
                    throw new FormatException("The ordering of the tokens in the pathUrlFormat must be in the order: dependencyId, version, type");
                }
                latestIndex = newIndex;
                if (!pathBasedUrlFormat.Contains(r))
                    throw new FormatException("The value specified for pathUrlFormat does not contain an " + r + " token");
            }
            if (!pathBasedUrl.EndsWith("{type}"))
            {
                throw new FormatException("The pathUrlFormat must end with the {type} token");
            }
            if (!pathBasedUrl.StartsWith("{dependencyId}"))
            {
                throw new FormatException("The pathUrlFormat must start with the {dependencyId} token");
            }
            if (pathChars.Count(x => x == '{') > 3)
            {
                throw new FormatException("The value specified for pathUrlFormat contains a '{' character outside of the token declaration which is invalid");
            }
            if (pathChars.Count(x => x == '}') > 3)
            {
                throw new FormatException("The value specified for pathUrlFormat contains a '}' character outside of the token declaration which is invalid");
            }
            //ensure that each token is delimited by something
            if (pathChars[pathBasedUrlFormat.IndexOf('}') + 1] == '{' || pathChars[pathBasedUrlFormat.IndexOf('}') + 1] == '}')
            {
                throw new FormatException("The {dependencyId} and {version} tokens must be seperated by a valid character");
            }
            if (pathChars[pathBasedUrlFormat.IndexOf('}', pathBasedUrlFormat.IndexOf('}') + 1) + 1] == '{' || pathChars[pathBasedUrlFormat.IndexOf('}', pathBasedUrlFormat.IndexOf('}') + 1) + 1] == '}')
            {
                throw new FormatException("The {version} and {type} tokens must be seperated by a valid character");
            }
        }
    }
}