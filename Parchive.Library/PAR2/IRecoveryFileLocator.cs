using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parchive.Library.PAR2
{
    public interface IRecoveryFileLocator
    {
        /// <summary>
        /// Locates all recovery files in the location.
        /// </summary>
        /// <param name="location">The location to search.</param>
        /// <param name="additionalParams">Might be used by derived classes where additional parameters are required to locate the files.</param>
        /// <returns>A collection of RecoveryFile objects, grouped by set name.</returns>
        IEnumerable<IGrouping<string, RecoveryFile>> Locate(string location, params object[] additionalParams);
    }

    public class LocalRecoveryFileLocator : IRecoveryFileLocator
    {
        /// <summary>
        /// Creates RecoveryFile objects from all the PAR2 files in a directory.
        /// </summary>
        /// <param name="directory">The directory to search.</param>
        /// <returns>A collection of RecoveryFile objects, grouped by set name.</returns>
        public IEnumerable<IGrouping<string, RecoveryFile>> Locate(string directory, params object[] additionalParams)
        {
            var recoveryFiles = new List<RecoveryFile>();

            foreach (var file in Directory.EnumerateFiles(directory))
            {
                var fi = new FileInfo(file);

                if (fi.Extension == ".par2")
                {
                    recoveryFiles.Add(RecoveryFile.FromFilename(fi.FullName));
                }
            }

            return recoveryFiles.GroupBy(x => x.Name);
        }
    }
}
