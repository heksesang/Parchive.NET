using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Parchive.Library.PAR2
{
    /// <summary>
    /// PAR2 file metadata.
    /// </summary>
    public struct RecoveryFile
    {
        #region Properties
        /// <summary>
        /// The path of the file.
        /// </summary>
        public string Filename { get; set; }
        
        /// <summary>
        /// The set name. This is the base filename of the file.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The exponents that are present in this file.
        /// </summary>
        public Range<long> Exponents { get; set; }
        #endregion

        #region Constants
        private const string _Regex = @"(.+?)(?:\.vol(\d+)\+(\d+))*(\.PAR2|\.par2)";
        #endregion

        #region Static Methods
        /// <summary>
        /// Creates a RecoveryFile object from a PAR2 file.
        /// </summary>
        /// <param name="filename">The path of the PAR2 file.</param>
        /// <returns>A RecoveryFile object containing information from the PAR2 file.</returns>
        public static RecoveryFile FromFilename(string filename)
        {
            var fi = new FileInfo(filename);

            var m = Regex.Match(fi.Name, _Regex);

            long start;
            long count;

            Range<long> range = null;

            if (long.TryParse(m.Groups[2].Value, out start) && long.TryParse(m.Groups[3].Value, out count))
            {
                range = new Range<long>
                {
                    Minimum = start,
                    Maximum = start + count - 1
                };
            }

            return new RecoveryFile
            {
                Filename = fi.FullName,
                Name = m.Groups[1].Value,
                Exponents = range
            };
        }
        #endregion
    }
}
