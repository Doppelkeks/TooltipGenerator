using System.Text.RegularExpressions;

namespace TooltipGenerator {
    /// <summary>
    /// Defines the parameters for processing (parsing) code to extract the comments to be put in the tooltips
    /// </summary>
    internal class CodeProcessingConfiguration {
        /// <summary>
        /// Specifics for setting up the soecific field types we want to detect
        /// </summary>
        internal string[] ValidFieldTypes { get; private set; }

        /// <summary>
        /// Regex that extracts from the code the documentation lines
        /// </summary>
        internal string Parser { get; private set; }

        /// <summary>
        /// Regex that transform documentation lines to comment lines.
        /// Is optional: If no documentation to comment transformation needed, set this to null
        /// </summary>
        internal Regex CommentExtractor { get; private set; }

        internal CodeProcessingConfiguration(string[] validFieldTypes, string parser, Regex commentExtractor, string compatibleFileExtensions) {
            ValidFieldTypes = validFieldTypes;
            Parser = parser;
            CommentExtractor = commentExtractor;
        }
    }
}