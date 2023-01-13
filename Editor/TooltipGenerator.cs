using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace TooltipGenerator {
    /// <summary>
    /// A class containing a set of methods for generating Unity's tooltips from existing code comments.
    /// </summary>
    public class TooltipGenerator {
        private readonly StringBuilder tooltipTagBuilder;
        private readonly StringBuilder documentationBuilder;

        private readonly List<CodeProcessingConfiguration> codeProcessors;
        private readonly string escapedNewLineInGeneratedCode;
        private readonly string newLineInGeneratedCode;

        private const string validFieldsKey = "<" + nameof(validFieldsKey) + ">";
        private HashSet<string> validClassNames = new HashSet<string>();

        /// <summary>
        /// base method for collecting and storing valid types/ class names
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="lookup"></param>
        private void ExtractTypesBase(TypeCache.TypeCollection collection, ref HashSet<string> lookup) {
            string currentTypeName;
            int indexOfGenericNotation;

            foreach (Type type in collection) {
                currentTypeName = type.Name;
                indexOfGenericNotation = currentTypeName.IndexOf("`");

                if (indexOfGenericNotation != -1) {
                    currentTypeName = currentTypeName.Substring(0, indexOfGenericNotation);
                }

                if (!validClassNames.Contains(currentTypeName)) {
                    validClassNames.Add(currentTypeName);
                }
            }
        }

        /// <summary>
        /// Extract relevant types including all derived types
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lookup"></param>
        private void ExtractTypesDerived<T>(ref HashSet<string> lookup) {
            lookup.Add(typeof(T).Name);
            ExtractTypesBase(TypeCache.GetTypesDerivedFrom<T>(), ref lookup);
        }

        /// <summary>
        /// Extract relevant types based on a given attribute
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lookup"></param>
        private void ExtractTypesByAttribute<T>(ref HashSet<string> lookup) where T : Attribute {
            ExtractTypesBase(TypeCache.GetTypesWithAttribute<T>(), ref lookup);
        }

        /// <summary>
        /// Update the valid classes that support [Tooltip] by using Unitys TypeCache
        /// </summary>
        public void UpdateValidClasses() {
            validClassNames.Clear();
            ExtractTypesDerived<MonoBehaviour>(ref validClassNames);
            ExtractTypesDerived<ScriptableObject>(ref validClassNames);
            ExtractTypesByAttribute<SerializableAttribute>(ref validClassNames);
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public TooltipGenerator() {
            UpdateValidClasses();
            tooltipTagBuilder = new StringBuilder();
            documentationBuilder = new StringBuilder();

            const RegexOptions regexOptions = RegexOptions.Multiline;

            newLineInGeneratedCode = Environment.NewLine;

            escapedNewLineInGeneratedCode = newLineInGeneratedCode.Replace("\r", @"\r");
            escapedNewLineInGeneratedCode = escapedNewLineInGeneratedCode.Replace("\n", @"\n");

            string doubleEscapedNewLine = newLineInGeneratedCode.Replace("\r", @"\\r");
            doubleEscapedNewLine = doubleEscapedNewLine.Replace("\n", @"\\n");

            Regex commentExtractingRegexp = new Regex(@"\s*<summary>\s*(?:" + doubleEscapedNewLine + @")?(?<comment>.*?(?=(?:" + doubleEscapedNewLine + @")?\s*</summary\s*))", regexOptions);

            const string nonTooltipAttributes = @"(?<attributes>^[ \t]*\[(?!([ \t]*(?:UnityEngine.)?Tooltip))[^]]+\]\s*(?=^))*";
            const string nonCommentRegexPartSerialize = @"\s*(?=^)" + nonTooltipAttributes + @"(?<tooltip>^[ \t]*\[(?:UnityEngine.)?Tooltip\(""(?<tooltipContent>[^""]*)""\)\]\s*(?=^))?" + nonTooltipAttributes + @"(?<field>(?<beginning>^[ \t]*)" + validFieldsKey + @"\s+[^\s;=]+\s+[^\s;=]+\s*(?>=[^;]+)?;)";
            const string newLineRegex = @"(?:\r)?\n";

            string[] validFieldTypes = new string[] {
                "public",
                @"\[SerializeField\] private",
            };

            CodeProcessingConfiguration processingConfiguration = new CodeProcessingConfiguration(validFieldTypes, @"(?>^[ \t]*///[ \t]?(?>(?<documentation>[^\r\n]*))" + newLineRegex + @")+" + nonCommentRegexPartSerialize, commentExtractingRegexp, "cs");
            codeProcessors = new List<CodeProcessingConfiguration>();
            codeProcessors.Add(processingConfiguration);
        }

        /// <summary>
        /// Processes the file's content, and if any tooltip was generated, updates the file.
        /// </summary>
        /// <param name="filePath"> The path of the file to update. </param>
        /// <param name="commentTypes"> The <see cref="CommentTypes"/> to be considered while generating the tooltips. </param>
        /// <returns> True if the file was updated. </returns>
        public bool TryProcessFile(string filePath) {
            return TryProcessFile(filePath, filePath, Encoding.Default);
        }

        /// <summary>
        /// Processes the file's content, and if any tooltip was generated, updates the file.
        /// </summary>
        /// <param name="filePath"> The path of the file to update. </param>
        /// <param name="fileEncoding"> The <see cref="Encoding"/> of the file. </param>
        /// <returns> True if an output file with updated content was created. </returns>
        public bool TryProcessFile(string filePath, Encoding fileEncoding) {
            return TryProcessFile(filePath, filePath, fileEncoding);
        }

        /// <summary>
        /// Processes the input file's content, and if any tooltip was generated, an output file will be created containing the updated content.
        /// </summary>
        /// <param name="inputFilePath"> The path of the input file. </param>
        /// <param name="outputFilePath"> The path of the output file. </param>
        /// <returns> True if an output file with updated content was created. </returns>
        public bool TryProcessFile(string inputFilePath, string outputFilePath) {
            return TryProcessFile(inputFilePath, outputFilePath, Encoding.Default);
        }

        /// <summary>
        /// Processes the input file's content, and if any tooltip was generated, an output file will be created containing the updated content.
        /// </summary>
        /// <param name="inputFilePath"> The path of the input file. </param>
        /// <param name="outputFilePath"> The path of the output file. </param>
        /// <param name="fileEncoding"> The <see cref="Encoding"/> of the input file and output file. </param>
        /// <returns> True if an output file with updated content was created. </returns>
        public bool TryProcessFile(string inputFilePath, string outputFilePath, Encoding fileEncoding) {
            try {
                string inputFileContent;
                using (StreamReader streamReader = new StreamReader(inputFilePath, fileEncoding)) {
                    inputFileContent = streamReader.ReadToEnd();
                }

                string outputFileContent;
                bool fileWasModified = TryProcessText(inputFileContent, out outputFileContent);

                if (fileWasModified) {
                    using (StreamWriter writer = new StreamWriter(outputFilePath, false, fileEncoding)) {
                        writer.Write(outputFileContent);
                    }
                }

                return fileWasModified;

            } catch (UnauthorizedAccessException) {
                return false;
            }
        }

        /// <summary>
        /// Checks if the given .cs file content contains a valid class declaration that can work with [Tooltip] attributes/ uses them.
        /// </summary>
        /// <param name="textToProcess">The input text.</param>
        /// <returns>Wether the given .cs file content contains valid classes.</returns>
        private bool IsValidClass(string textToProcess) {
            string[] classes = textToProcess.Split("class ");
            int indexOfFirstCurlyBracket = -1;
            bool isValid = false;
            string classFragment = "";

            if (classes.Length > 1) {
                for (int i = 1; i < classes.Length; i++) {
                    classFragment = classes[i];
                    indexOfFirstCurlyBracket = classFragment.IndexOf("{");
                    if (indexOfFirstCurlyBracket != -1) {
                        classFragment = classFragment.Substring(0, indexOfFirstCurlyBracket);

                        foreach (string validClassName in validClassNames) {
                            if (classFragment.IndexOf(validClassName) != -1) {
                                isValid = true;
                                break;
                            }
                        }
                    }
                }
            }
            return isValid;
        }

        /// <summary>
        /// Processes the given text by updating it with tooltips generated from valid comments.
        /// </summary>
        /// <param name="textToProcess"> The input text. </param>
        /// <param name="processedText"> The output text. If method returns false, this text will be equal to <paramref name="textToProcess"/>. </param>
        /// <returns> True if the text was updated.</returns>
        public Boolean TryProcessText(string textToProcess, out string processedText) {
            processedText = textToProcess;

            // return early, if classes contained in the given .cs file are not valid for [Tooltip] attribute
            if (!IsValidClass(textToProcess)) {
                return false;
            }

            bool fileWasModified = false;

            string tooltipAttribute = "[Tooltip(\"";
            if (textToProcess.IndexOf("using UnityEngine;") < 0) {
                tooltipAttribute = "[UnityEngine.Tooltip(\"";
            }

            foreach (CodeProcessingConfiguration codeProcessor in codeProcessors) {
                Debug.Log("test ");
                for (int i=0; i<codeProcessor.ValidFieldTypes.Length; i++) {
                    Debug.Log("test 2");
                    int insertedTextLength = 0;
                    string parserString = codeProcessor.Parser.Replace(validFieldsKey, codeProcessor.ValidFieldTypes[i]);
                    Regex regex = new Regex(parserString, RegexOptions.Multiline);
                    MatchCollection matches = regex.Matches(processedText);
                    int matchesCount = matches.Count;
                    for (int matchIndex = 0; matchIndex < matchesCount; matchIndex++) {
                        Debug.Log("test 3");
                        Match match = matches[matchIndex];
                        GroupCollection groups = match.Groups;
                        string tooltipContent = BuildTooltipContent(groups["documentation"].Captures, codeProcessor.CommentExtractor);

                        //if existing tooltip is different than the generated one
                        if (tooltipContent != groups["tooltipContent"].ToString()) {
                            tooltipTagBuilder.Append(groups["beginning"]);
                            //tooltip attribute beginning
                            tooltipTagBuilder.Append(tooltipAttribute);
                            tooltipTagBuilder.Append(tooltipContent);
                            //tooltip attribute end
                            tooltipTagBuilder.Append("\")]");
                            tooltipTagBuilder.Append(newLineInGeneratedCode);
                            string tooltip = tooltipTagBuilder.ToString();
                            tooltipTagBuilder.Length = 0;
                            tooltipTagBuilder.Capacity = 0;

                            //remove possible old tooltip
                            Group oldTooltipGroup = groups["tooltip"];
                            int oldTooltipLength = oldTooltipGroup.Length;
                            if (oldTooltipLength > 0) {
                                processedText = processedText.Remove(insertedTextLength + oldTooltipGroup.Index,
                                    oldTooltipLength);
                                insertedTextLength -= oldTooltipLength;
                            }
                            //insert tooltip in text
                            processedText = processedText.Insert(insertedTextLength + groups["field"].Index, tooltip);
                            insertedTextLength += tooltip.Length;

                            fileWasModified = true;
                        }
                    }
                }
            }
            return fileWasModified;
        }

        private string BuildTooltipContent(CaptureCollection documentationCaptures, Regex commentExtractor) {
            //constructing the documentation text
            int capturesCount = documentationCaptures.Count;
            for (int captureIndex = 0; captureIndex < capturesCount; captureIndex++) {
                Capture capturedLine = documentationCaptures[captureIndex];
                documentationBuilder.Append(capturedLine);

                //new line if there is other lines to add
                if (captureIndex != capturesCount - 1)
                    documentationBuilder.Append(escapedNewLineInGeneratedCode);
            }

            string documentation = documentationBuilder.ToString();
            documentationBuilder.Length = 0;
            documentationBuilder.Capacity = 0;

            string tooltipContent;
            if (commentExtractor != null) {
                //extracting the significant meaningful part of the documentation 
                Match match = commentExtractor.Match(documentation);
                //if (match.Success == false)
                    //throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "Could not parse the following documentation xml '{0}'", documentation));
                tooltipContent = match.Groups["comment"].ToString();
            } else
                tooltipContent = documentation;

            // replace double quotes
            tooltipContent = tooltipContent.Replace("\"", "'");

            // replace illegal backslashes
            foreach (Match m in Regex.Matches(tooltipContent, @"\\[^nratfvbn]", RegexOptions.Multiline)) {
                tooltipContent = tooltipContent.Remove(m.Index, 1).Insert(m.Index,"/");
            }

            return tooltipContent;
        }
    }
}
