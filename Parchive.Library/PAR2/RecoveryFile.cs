using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Parchive.Library.PAR2
{
    public struct RecoveryFile
    {
        #region Properties
        public string Filename { get; set; }
        
        public string Name { get; set; }

        public Range<long> Exponents { get; set; }
        #endregion

        #region Constants
        private const string _Regex = @"(.+?)(?:\.vol(\d+)\+(\d+))*(\.PAR2|\.par2)";
        #endregion

        #region Static Methods
        public static RecoveryFile FromFilename(string filename)
        {
            var m = Regex.Match(filename, _Regex);

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
                Filename = filename,
                Name = m.Groups[1].Value,
                Exponents = range
            };
        }

        public static IEnumerable<IGrouping<string, RecoveryFile>> FromDirectory(string directory)
        {
            var recoveryFiles = new List<RecoveryFile>();

            foreach (var file in Directory.EnumerateFiles(directory))
            {
                var fi = new FileInfo(file);

                if (fi.Extension == ".par2")
                {
                    recoveryFiles.Add(RecoveryFile.FromFilename(fi.Name));
                }
            }

            return recoveryFiles.GroupBy(x => x.Name);
        }
        #endregion
    }
}
