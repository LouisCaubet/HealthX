using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthX.Backend {
	
    /// <summary>
    /// Interface to create/get a file from the device. Run through DependencyService.
    /// </summary>
    public interface IFileHelper {

        /// <summary>
        /// If the Local Database file exists.
        /// </summary>
        bool DatabaseExists { get; set; }

        /// <summary>
        /// Returns the path to given filename on specified platform
        /// </summary>
        /// <param name="filename">The file to get the path of.</param>
        /// <returns>The path to the given file.</returns>
        string GetLocalFilePath(string filename);

    }

}
