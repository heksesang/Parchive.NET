using Parchive.Library.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parchive.Library
{
    public static class FileInfoExtensions
    {
        /// <summary>
        /// Formats the file path as a relative Unix path.
        /// </summary>
        /// <param name="fi">The FileInfo object.</param>
        /// <param name="di">The base directory.</param>
        /// <returns>A Unix path to the file.</returns>
        /// <exception cref="Parchive.Library.Exceptions.PathError">
        /// The base directory isn't valid for this file.
        /// </exception>
        public static string GetUnixPath(this FileInfo fi, DirectoryInfo di)
        {
            var dir = fi.Directory;

            while (dir != null)
            {
                if (dir.FullName == di.FullName)
                {
                    break;
                }

                dir = dir.Parent;

                if (dir == null)
                {
                    throw new PathError("Invalid base directory");
                }
            }

            return fi.FullName.Replace(dir.FullName, string.Empty).Replace('\\', '/').TrimStart('/');
        }
    }
}
