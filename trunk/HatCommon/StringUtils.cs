using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Hatfield.Web.Portal
{
    /// <summary>
    /// Summary description for StringUtils.
    /// </summary>
    public class StringUtils
    {
        public StringUtils()
        { }

        /// <summary>
        /// Return the string with HTML BR tag at the end of each line.
        /// Similar to http://php.net/manual/en/function.nl2br.php
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string nl2br(string s)
        {
            return s.Replace(Environment.NewLine, "<br />");
        }

        public static string ReplaceNonAscii(string s)
        {
            byte[] asciiChar = Encoding.ASCII.GetBytes(s);
            string newResult = Encoding.ASCII.GetString(asciiChar);
            return newResult;
        }

        public static string ToTitleCase(string s)
        {
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
        }

        /// <summary>
        /// Converts a string array full of integers into an integer array.
        /// </summary>
        /// <param name="stringArray"></param>
        /// <returns></returns>
        public static int[] ToIntArray(string[] stringArray)
        {            
            List<int> ret = new List<int>();
            foreach (string s in stringArray)
            {
                int i = Int32.MinValue;
                if (Int32.TryParse(s, out i))
                    ret.Add(i);
            } // foreach
            return ret.ToArray();
        }

        public static string AddSlashes(string toAddSlashes)
        {
            string ret = toAddSlashes.Replace("'", "\'"); // single quotes
            char doubleQuote = '"';
            ret = ret.Replace(doubleQuote.ToString(), "\\" + doubleQuote);
            return ret;
        }

        /// <summary>
        /// encode a string in Base64 format
        /// </summary>
        /// <param name="toEncode"></param>
        /// <returns></returns>
        public static string Base64Encode(string toEncode)
        {
            byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(toEncode);
            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }

        /// <summary>
        /// decode a string that is in Base64 format
        /// </summary>
        /// <param name="encodedData"></param>
        /// <returns></returns>
        static public string Base64Decode(string encodedData)
        {
            byte[] encodedDataAsBytes = System.Convert.FromBase64String(encodedData);
            string returnValue = System.Text.ASCIIEncoding.ASCII.GetString(encodedDataAsBytes);
            return returnValue;

        }

        static public string Base36Encode(Int64 value)
        {
            char[] base36Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            string returnValue = "";
            if (value < 0)
            {
                value *= -1;
            }
            do
            {
                returnValue = base36Chars[value % base36Chars.Length] + returnValue;
                value /= 36;
            } while (value != 0);
            return returnValue;
        }

        public static System.IO.MemoryStream WriteTextToMemoryStream(string textToWrite, System.Text.Encoding encoding)
        {
            byte[] a = encoding.GetBytes(textToWrite);
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            ms.Write(a, 0, a.Length);
            ms.Seek(0, System.IO.SeekOrigin.Begin); // seek to the beginning.
            return ms;
        }

        /// <summary>
        /// returns the array index of value in the specified array.
        /// Utilizes comparisonType for each array/value comparison.
        /// returns -1 if the value is not found in the array.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="value"></param>
        /// <param name="comparisonType"></param>
        /// <returns></returns>
        public static int IndexOf(string[] array, string value, StringComparison comparisonType)
        {
            for (int index = 0; index < array.Length; index++)
            {
                if (String.Compare(array[index], value, comparisonType) == 0)
                    return index;
            } // for
            return -1;

        }

        /// <summary>
        /// uses the UTF8 encoding
        /// </summary>
        /// <param name="textToWrite"></param>
        /// <returns></returns>
        public static System.IO.MemoryStream WriteTextToMemoryStream(string textToWrite)
        {
            return WriteTextToMemoryStream(textToWrite, System.Text.Encoding.UTF8);
        }

        /// <summary>
        /// The same as String.Join(), but only joins non-blank strings 
        /// Concatenates a specified separator String between each element of a specified String array, yielding a single concatenated string.		
        /// </summary>
        /// <param name="seperator"></param>
        /// <param name="strings"></param>
        /// <returns></returns>
        public static string JoinNonBlanks(string seperator, string[] strings)
        {
            ArrayList sArr = new ArrayList();
            foreach (string s in strings)
            {
                if (s.Trim() != "")
                    sArr.Add(s);
            }
            string[] r = new string[sArr.Count];
            sArr.CopyTo(r);
            return String.Join(seperator, r);
        } // JoinNonBlanks

       

        /// <summary>
        /// the same as String.Join(), but the last seperator can be different.
        /// Useful for getting outputs like "1, 2 and 3".
        /// </summary>
        /// <param name="mainSeperator"></param>
        /// <param name="lastSeperator"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        public static string Join(string mainSeperator, string lastSeperator, string[] items)
        {
            StringBuilder ret = new StringBuilder();
            for (int i = 0; i < items.Length; i++)
            {
                ret.Append(items[i]);
                if (i == items.Length - 2)
                    ret.Append(lastSeperator);
                else if (i < items.Length - 2)
                    ret.Append(mainSeperator);
            }
            return ret.ToString();
        }

        /// <summary>
        /// the same as String.Join(), but the last seperator can be different.
        /// Useful for getting outputs like "1, 2 and 3".
        /// </summary>
        /// <param name="mainSeperator"></param>
        /// <param name="lastSeperator"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        public static string Join(string mainSeperator, string lastSeperator, int[] items)
        {
            List<string> arr = new List<string>();
            foreach (int i in items)
                arr.Add(i.ToString());

            return Join(mainSeperator, lastSeperator, arr.ToArray());
        }

        /// <summary>
        /// Join each items together, seperated with the seperator. Before joining, each item has the prefix and suffix added.
        /// End result: prefix+item[0]+suffix+seperator + prefix+item[1]+suffix+seperator + ...
        /// </summary>
        /// <param name="seperator"></param>
        /// <param name="items"></param>
        /// <param name="prefix"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public static string Join(string seperator, string[] items, string prefix, string suffix)
        {
            List<string> a = new List<string>();
            foreach (string i in items)
            {
                a.Add(prefix + i + suffix);
            }

            return String.Join(seperator, a.ToArray());
        }

        public static string Join(string seperator, int[] integers)
        {
            ArrayList a = new ArrayList();
            foreach (int i in integers)
                a.Add(i.ToString());
            string[] s = (string[])a.ToArray(typeof(string));
            return String.Join(seperator, s);
        }

        public static string Join(string seperator, long[] integers)
        {
            ArrayList a = new ArrayList();
            foreach (long i in integers)
                a.Add(i.ToString());
            string[] s = (string[])a.ToArray(typeof(string));
            return String.Join(seperator, s);
        }

        public static string RemoveEnding(string str, string ending)
        {
            return RemoveEnding(str, ending, false);
        }

        public static string RemoveEnding(string str, string ending, bool ignoreCase)
        {
            string strToCompare = str;
            if (ignoreCase)
                strToCompare = str.ToLower();
            string endingToCompare = ending;
            if (ignoreCase)
                endingToCompare = ending.ToLower();

            if (strToCompare.EndsWith(endingToCompare))
            {
                string s = str.Substring(0, str.Length - ending.Length);
                return s;
            }
            return str;
        }

        public static string Replace(string haystack, string searchFor, string replaceWith, bool ignoreCase)
        {
            if (haystack == null)
                return null;

            if (String.IsNullOrEmpty(searchFor))
                return haystack;

            if (!ignoreCase)
            {
                return haystack.Replace(searchFor, replaceWith);
            }
            else
            {
                // source: http://www.codeproject.com/KB/string/fastestcscaseinsstringrep.aspx?msg=1835929#xx1835929xx				

                int posCurrent = 0;
                int lenPattern = searchFor.Length;
                int idxNext = haystack.IndexOf(searchFor, StringComparison.CurrentCultureIgnoreCase);

                int sbSize = Math.Min(4096, haystack.Length);

                StringBuilder result = new StringBuilder(sbSize);

                while (idxNext >= 0)
                {
                    result.Append(haystack, posCurrent, idxNext - posCurrent);
                    result.Append(replaceWith);

                    posCurrent = idxNext + lenPattern;

                    idxNext = haystack.IndexOf(searchFor, posCurrent, StringComparison.CurrentCultureIgnoreCase);
                }

                result.Append(haystack, posCurrent, haystack.Length - posCurrent);

                return result.ToString();

            }
        } // Replace

        public static string Surround(string match, string head, string tail, string original)
        {
            if (string.IsNullOrEmpty(original) || string.IsNullOrEmpty(match) || (string.IsNullOrEmpty(head) && string.IsNullOrEmpty(tail)))
                return original;

            StringComparison comparisonType = StringComparison.CurrentCultureIgnoreCase;

            StringBuilder resultBuilder = new StringBuilder(original.Length);
            int matchLength = match.Length;
            int lastIdx = 0;

            for (; ; )
            {
                int curIdx = original.IndexOf(match, lastIdx, comparisonType);

                if (curIdx > -1)
                    resultBuilder
                      .Append(original, lastIdx, curIdx - lastIdx)
                      .Append(head)
                      .Append(original, curIdx, matchLength)
                      .Append(tail);
                else
                    return resultBuilder.Append(original.Substring(lastIdx)).ToString();

                lastIdx = curIdx + matchLength;
            }
        }


        /// <summary>
        /// Surrounds all occurances of <em>word</em> in haystack with pefix and suffixes. The haystack object is modified to contain the replaced values 
        /// </summary>
        /// <param name="wordToSurround"></param>
        /// <param name="prefix"></param>
        /// <param name="suffix"></param>
        /// <param name="haystack"></param>
        public static void SurroundTest(string wordToSurround, string prefix, string suffix, StringBuilder haystack)
        {
            int index = haystack.ToString().IndexOf(wordToSurround, StringComparison.CurrentCultureIgnoreCase);

            while (index >= 0)
            {
                // -- insert suffix first                    
                haystack.Insert(index + wordToSurround.Length, suffix);
                haystack.Insert(index, prefix);
                int nextIndex = index + prefix.Length + wordToSurround.Length + suffix.Length;
                index = haystack.ToString().IndexOf(wordToSurround, nextIndex, StringComparison.CurrentCultureIgnoreCase);
            }
        }

        public static string TrimLeft(string str)
        {
            if (str.Length == 0)
                return str;

            char c = str[0];
            while (Char.IsWhiteSpace(c))
            {
                str = str.Substring(1); // remove this char
                if (str.Length == 0)
                    return str;
                c = str[0];
            }
            return str;
        } // TrimLeft

        public static string TrimRight(string str)
        {
            if (str.Length == 0)
                return str;

            char c = str[str.Length - 1];
            while (Char.IsWhiteSpace(c))
            {
                str = StringUtils.RemoveEnding(str, c.ToString());
                if (str.Length == 0)
                    return str;
                c = str[str.Length - 1];
            }
            return str;
        } // TrimLeft

        public static string DoubleToString(double d, int numDecimalsToDisplay)
        {
            return DoubleToString(d, numDecimalsToDisplay, false);
        }

        public static string DoubleToString(double d, int numDecimalsToDisplay, bool seperateThousandsWithComma)
        {
            System.Globalization.NumberFormatInfo outputFormat = new System.Globalization.CultureInfo("en-US", false).NumberFormat;

            outputFormat.NumberDecimalDigits = numDecimalsToDisplay;
            if (seperateThousandsWithComma)
            {
                outputFormat.NumberGroupSeparator = ",";
                outputFormat.NumberGroupSizes = new int[] { 3 };
            }
            return d.ToString("N", outputFormat);

        }

        public static string formatFileSize(long fileSizeBytes)
        {
            if (fileSizeBytes >= (1073741824))
            {
                return (fileSizeBytes / 1073741824).ToString("N").Replace(".00", "") + " GB";
            }
            else if (fileSizeBytes >= (1048576))
            {
                return (fileSizeBytes / 1048576).ToString("N").Replace(".00", "") + " MB";
            }
            else if (fileSizeBytes >= (1024))
            {
                return (fileSizeBytes / 1024).ToString("N").Replace(".00", "") + " KB";
            }
            else
            {
                return fileSizeBytes.ToString("N").Replace(".00", "") + " bytes";
            }

        }

        public static string StripHTMLTags(string text)
        {
            // http://www.codeproject.com/asp/removehtml.asp?df=100&forumid=882&select=2242417#xx2242417xx
            return System.Text.RegularExpressions.Regex.Replace(text, @"<(.|\n)*?>", string.Empty);
        }

        [DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
        static extern bool PathCompactPathEx([Out] StringBuilder pszOut, string szPath, int cchMax, int dwFlags);

        public static string compactPath(string longPathName, int maxCharacters)
        {
            // source: http://www.pinvoke.net/default.aspx/shlwapi/PathCompactPathEx.html
            StringBuilder sb = new StringBuilder();
            PathCompactPathEx(sb, longPathName, maxCharacters + 1, 0);
            return sb.ToString();

        }

        public static string HtmlDiff(string oldHtml, string newHtml)
        {
            HtmlDiffClass d = new HtmlDiffClass(oldHtml, newHtml);
            return d.Build();
        }

        #region HtmlDiff class
        /// <summary>
        /// Origionally downloaded from http://htmldiff.codeplex.com/
        /// Modified to work with .Net 2 (origional used C# v3 statements)
        /// </summary>
        private class HtmlDiffClass
        {

            private StringBuilder content;
            private string oldText, newText;
            private string[] oldWords, newWords;
            Dictionary<string, List<int>> wordIndices;
            private string[] specialCaseOpeningTags = new string[] { "<strong[\\>\\s]+", "<b[\\>\\s]+", "<i[\\>\\s]+", "<big[\\>\\s]+", "<small[\\>\\s]+", "<u[\\>\\s]+", "<sub[\\>\\s]+", "<sup[\\>\\s]+", "<strike[\\>\\s]+", "<s[\\>\\s]+" };
            private string[] specialCaseClosingTags = new string[] { "</strong>", "</b>", "</i>", "</big>", "</small>", "</u>", "</sub>", "</sup>", "</strike>", "</s>" };


            /// <summary>
            /// Initializes a new instance of the <see cref="Diff"/> class.
            /// </summary>
            /// <param name="oldText">The old text.</param>
            /// <param name="newText">The new text.</param>
            public HtmlDiffClass(string oldText, string newText)
            {
                this.oldText = oldText;
                this.newText = newText;

                this.content = new StringBuilder();
            }

            /// <summary>
            /// Builds the HTML diff output
            /// </summary>
            /// <returns>HTML diff markup</returns>
            public string Build()
            {
                this.SplitInputsToWords();

                this.IndexNewWords();

                List<Operation> operations = this.Operations();

                foreach (Operation item in operations)
                {
                    this.PerformOperation(item);
                }

                return this.content.ToString();
            }

            private void IndexNewWords()
            {
                this.wordIndices = new Dictionary<string, List<int>>();
                for (int i = 0; i < this.newWords.Length; i++)
                {
                    string word = this.newWords[i];

                    if (this.wordIndices.ContainsKey(word))
                    {
                        this.wordIndices[word].Add(i);
                    }
                    else
                    {
                        this.wordIndices[word] = new List<int>();
                        this.wordIndices[word].Add(i);
                    }
                }
            }

            private void SplitInputsToWords()
            {
                this.oldWords = ConvertHtmlToListOfWords(this.Explode(this.oldText));
                this.newWords = ConvertHtmlToListOfWords(this.Explode(this.newText));
            }

            private string[] ConvertHtmlToListOfWords(string[] characterString)
            {
                Mode mode = Mode.character;
                string current_word = String.Empty;
                List<string> words = new List<string>();

                foreach (string character in characterString)
                {
                    switch (mode)
                    {
                        case Mode.character:

                            if (this.IsStartOfTag(character))
                            {
                                if (current_word != String.Empty)
                                {
                                    words.Add(current_word);
                                }

                                current_word = "<";
                                mode = Mode.tag;
                            }
                            else if (Regex.IsMatch(character, "\\s"))
                            {
                                if (current_word != String.Empty)
                                {
                                    words.Add(current_word);
                                }
                                current_word = character;
                                mode = Mode.whitespace;
                            }
                            else
                            {
                                current_word += character;
                            }

                            break;
                        case Mode.tag:

                            if (this.IsEndOfTag(character))
                            {
                                current_word += ">";
                                words.Add(current_word);
                                current_word = "";

                                if (IsWhiteSpace(character))
                                {
                                    mode = Mode.whitespace;
                                }
                                else
                                {
                                    mode = Mode.character;
                                }
                            }
                            else
                            {
                                current_word += character;
                            }

                            break;
                        case Mode.whitespace:

                            if (this.IsStartOfTag(character))
                            {
                                if (current_word != String.Empty)
                                {
                                    words.Add(current_word);
                                }
                                current_word = "<";
                                mode = Mode.tag;
                            }
                            else if (Regex.IsMatch(character, "\\s"))
                            {
                                current_word += character;
                            }
                            else
                            {
                                if (current_word != String.Empty)
                                {
                                    words.Add(current_word);
                                }

                                current_word = character;
                                mode = Mode.character;
                            }

                            break;
                        default:
                            break;
                    }


                }
                if (current_word != string.Empty)
                {
                    words.Add(current_word);
                }

                return words.ToArray();
            }

            private bool IsStartOfTag(string val)
            {
                return val == "<";
            }

            private bool IsEndOfTag(string val)
            {
                return val == ">";
            }

            private bool IsWhiteSpace(string value)
            {
                return Regex.IsMatch(value, "\\s");
            }

            private string[] Explode(string value)
            {
                return Regex.Split(value, "");
            }

            private void PerformOperation(Operation operation)
            {
                switch (operation.Action)
                {
                    case Action.equal:
                        this.ProcessEqualOperation(operation);
                        break;
                    case Action.delete:
                        this.ProcessDeleteOperation(operation, "diffdel");
                        break;
                    case Action.insert:
                        this.ProcessInsertOperation(operation, "diffins");
                        break;
                    case Action.none:
                        break;
                    case Action.replace:
                        this.ProcessReplaceOperation(operation);
                        break;
                    default:
                        break;
                }
            }

            private void ProcessReplaceOperation(Operation operation)
            {
                this.ProcessDeleteOperation(operation, "diffmod");
                this.ProcessInsertOperation(operation, "diffmod");
            }

            private void ProcessInsertOperation(Operation operation, string cssClass)
            {
                // words = this.newWords.Where((s, pos) => pos >= operation.StartInNew && pos < operation.EndInNew).ToList()
                List<string> words = new List<string>();
                for (int pos = operation.StartInNew; pos < operation.EndInNew; pos++)
                {
                    words.Add(newWords[pos]);
                }

                this.InsertTag("ins", cssClass, words);
            }

            private void ProcessDeleteOperation(Operation operation, string cssClass)
            {
                // var text = this.oldWords.Where((s, pos) => pos >= operation.StartInOld && pos < operation.EndInOld).ToList();
                List<string> text = new List<string>();
                for (int pos = operation.StartInOld; pos < operation.EndInOld; pos++)
                {
                    text.Add(oldWords[pos]);
                }
                this.InsertTag("del", cssClass, text);
            }

            private void ProcessEqualOperation(Operation operation)
            {
                // var result = this.newWords.Where((s, pos) => pos >= operation.StartInNew && pos < operation.EndInNew).ToArray();
                List<string> result = new List<string>();
                for (int pos = operation.StartInNew; pos < operation.EndInNew; pos++)
                {
                    result.Add(newWords[pos]);
                }

                this.content.Append(String.Join("", result.ToArray()));
            }


            /// <summary>
            /// This method encloses words within a specified tag (ins or del), and adds this into "content", 
            /// with a twist: if there are words contain tags, it actually creates multiple ins or del, 
            /// so that they don't include any ins or del. This handles cases like
            /// old: '<p>a</p>'
            /// new: '<p>ab</p><p>c</b>'
            /// diff result: '<p>a<ins>b</ins></p><p><ins>c</ins></p>'
            /// this still doesn't guarantee valid HTML (hint: think about diffing a text containing ins or
            /// del tags), but handles correctly more cases than the earlier version.
            /// 
            /// P.S.: Spare a thought for people who write HTML browsers. They live in this ... every day.
            /// </summary>
            /// <param name="tag"></param>
            /// <param name="cssClass"></param>
            /// <param name="words"></param>
            private void InsertTag(string tag, string cssClass, List<string> words)
            {
                while (true)
                {
                    if (words.Count == 0)
                    {
                        break;
                    }

                    // var nonTags = ExtractConsecutiveWords(words, x => !this.IsTag(x));
                    string[] nonTags = ExtractConsecutiveWords(words, IsNotTag);

                    string specialCaseTagInjection = string.Empty;
                    bool specialCaseTagInjectionIsBefore = false;

                    if (nonTags.Length != 0)
                    {
                        string text = this.WrapText(string.Join("", nonTags), tag, cssClass);

                        this.content.Append(text);
                    }
                    else
                    {
                        // Check if strong tag

                        // if (this.specialCaseOpeningTags.FirstOrDefault(x => Regex.IsMatch(words[0], x)) != null)                                        
                        if (Regex.IsMatch(words[0], specialCaseOpeningTags[0]))
                        {
                            specialCaseTagInjection = "<ins class=\"mod\">";
                            if (tag == "del")
                            {
                                words.RemoveAt(0);
                            }
                        }
                        // else if (this.specialCaseClosingTags.Contains(words[0]))
                        else if (Array.IndexOf(this.specialCaseClosingTags, words[0]) >= 0)
                        {
                            specialCaseTagInjection = "</ins>";
                            specialCaseTagInjectionIsBefore = true;
                            if (tag == "del")
                            {
                                words.RemoveAt(0);
                            }
                        }

                    }

                    if (words.Count == 0 && specialCaseTagInjection.Length == 0)
                    {
                        break;
                    }

                    if (specialCaseTagInjectionIsBefore)
                    {
                        // this.content.Append(specialCaseTagInjection + String.Join("", this.ExtractConsecutiveWords(words, x => this.IsTag(x))));
                        this.content.Append(specialCaseTagInjection);
                        this.content.Append(String.Join("", this.ExtractConsecutiveWords(words, IsTag)));
                    }
                    else
                    {
                        // this.content.Append(String.Join("", this.ExtractConsecutiveWords(words, x => this.IsTag(x))) + specialCaseTagInjection);
                        this.content.Append(String.Join("", this.ExtractConsecutiveWords(words, IsTag)) + specialCaseTagInjection);
                    }
                }
            }

            private string WrapText(string text, string tagName, string cssClass)
            {
                return string.Format("<{0} class=\"{1}\">{2}</{0}>", tagName, cssClass, text);
            }

            public delegate bool ProcessCondition(string item);


            private string[] ExtractConsecutiveWords(List<string> words, ProcessCondition condition)
            {
                int indexOfFirstTag = Int32.MinValue;

                for (int i = 0; i < words.Count; i++)
                {
                    string word = words[i];

                    if (!condition(word))
                    {
                        indexOfFirstTag = i;
                        break;
                    }
                }

                if (indexOfFirstTag != Int32.MinValue)
                {
                    // var items = words.Where((s, pos) => pos >= 0 && pos < indexOfFirstTag).ToArray();
                    List<string> items = new List<string>();
                    for (int pos = 0; pos < indexOfFirstTag; pos++)
                        items.Add(words[pos]);

                    if (indexOfFirstTag > 0)
                    {
                        words.RemoveRange(0, indexOfFirstTag);
                    }
                    return items.ToArray();
                }
                else
                {
                    // var items = words.Where((s, pos) => pos >= 0 && pos <= words.Count).ToArray();
                    List<string> items = new List<string>();
                    for (int pos = 0; pos < words.Count; pos++)
                        items.Add(words[pos]);

                    words.RemoveRange(0, words.Count);
                    return items.ToArray();
                }
            }

            private static bool IsNotTag(string item)
            {
                return !IsTag(item);
            }

            private static bool IsTag(string item)
            {
                bool isTag = IsOpeningTag(item) || IsClosingTag(item);
                return isTag;
            }

            private static bool IsOpeningTag(string item)
            {
                return Regex.IsMatch(item, "^\\s*<[^>]+>\\s*$");
            }

            private static bool IsClosingTag(string item)
            {
                return Regex.IsMatch(item, "^\\s*</[^>]+>\\s*$");
            }


            private List<Operation> Operations()
            {
                int positionInOld = 0;
                int positionInNew = 0;
                List<Operation> operations = new List<Operation>();

                List<Match> matches = this.MatchingBlocks();

                matches.Add(new Match(this.oldWords.Length, this.newWords.Length, 0));

                for (int i = 0; i < matches.Count; i++)
                {
                    Match match = matches[i];

                    bool matchStartsAtCurrentPositionInOld = (positionInOld == match.StartInOld);
                    bool matchStartsAtCurrentPositionInNew = (positionInNew == match.StartInNew);

                    Action action = Action.none;

                    if (matchStartsAtCurrentPositionInOld == false
                        && matchStartsAtCurrentPositionInNew == false)
                    {
                        action = Action.replace;
                    }
                    else if (matchStartsAtCurrentPositionInOld == true
                        && matchStartsAtCurrentPositionInNew == false)
                    {
                        action = Action.insert;
                    }
                    else if (matchStartsAtCurrentPositionInOld == false
                        && matchStartsAtCurrentPositionInNew == true)
                    {
                        action = Action.delete;
                    }
                    else // This occurs if the first few words are the same in both versions
                    {
                        action = Action.none;
                    }

                    if (action != Action.none)
                    {
                        operations.Add(
                            new Operation(action,
                                positionInOld,
                                match.StartInOld,
                                positionInNew,
                                match.StartInNew));
                    }

                    if (match.Size != 0)
                    {
                        operations.Add(new Operation(
                            Action.equal,
                            match.StartInOld,
                            match.EndInOld,
                            match.StartInNew,
                            match.EndInNew));

                    }

                    positionInOld = match.EndInOld;
                    positionInNew = match.EndInNew;
                }

                return operations;

            }

            private List<Match> MatchingBlocks()
            {
                List<Match> matchingBlocks = new List<Match>();
                this.FindMatchingBlocks(0, this.oldWords.Length, 0, this.newWords.Length, matchingBlocks);
                return matchingBlocks;
            }


            private void FindMatchingBlocks(int startInOld, int endInOld, int startInNew, int endInNew, List<Match> matchingBlocks)
            {
                Match match = this.FindMatch(startInOld, endInOld, startInNew, endInNew);

                if (match != null)
                {
                    if (startInOld < match.StartInOld && startInNew < match.StartInNew)
                    {
                        this.FindMatchingBlocks(startInOld, match.StartInOld, startInNew, match.StartInNew, matchingBlocks);
                    }

                    matchingBlocks.Add(match);

                    if (match.EndInOld < endInOld && match.EndInNew < endInNew)
                    {
                        this.FindMatchingBlocks(match.EndInOld, endInOld, match.EndInNew, endInNew, matchingBlocks);
                    }

                }
            }


            private Match FindMatch(int startInOld, int endInOld, int startInNew, int endInNew)
            {
                int bestMatchInOld = startInOld;
                int bestMatchInNew = startInNew;
                int bestMatchSize = 0;

                Dictionary<int, int> matchLengthAt = new Dictionary<int, int>();

                for (int indexInOld = startInOld; indexInOld < endInOld; indexInOld++)
                {
                    Dictionary<int, int> newMatchLengthAt = new Dictionary<int, int>();

                    string index = this.oldWords[indexInOld];

                    if (!this.wordIndices.ContainsKey(index))
                    {
                        matchLengthAt = newMatchLengthAt;
                        continue;
                    }

                    foreach (int indexInNew in this.wordIndices[index])
                    {
                        if (indexInNew < startInNew)
                        {
                            continue;
                        }

                        if (indexInNew >= endInNew)
                        {
                            break;
                        }


                        int newMatchLength = (matchLengthAt.ContainsKey(indexInNew - 1) ? matchLengthAt[indexInNew - 1] : 0) + 1;
                        newMatchLengthAt[indexInNew] = newMatchLength;

                        if (newMatchLength > bestMatchSize)
                        {
                            bestMatchInOld = indexInOld - newMatchLength + 1;
                            bestMatchInNew = indexInNew - newMatchLength + 1;
                            bestMatchSize = newMatchLength;
                        }
                    }

                    matchLengthAt = newMatchLengthAt;
                }

                return bestMatchSize != 0 ? new Match(bestMatchInOld, bestMatchInNew, bestMatchSize) : null;
            }

            public class Match
            {
                public Match(int startInOld, int startInNew, int size)
                {
                    this.StartInOld = startInOld;
                    this.StartInNew = startInNew;
                    this.Size = size;
                }

                public int StartInOld;
                public int StartInNew;
                public int Size;

                public int EndInOld
                {
                    get
                    {
                        return this.StartInOld + this.Size;
                    }
                }

                public int EndInNew
                {
                    get
                    {
                        return this.StartInNew + this.Size;
                    }
                }

            } // Match

            public class Operation
            {
                public Action Action;
                public int StartInOld;
                public int EndInOld;
                public int StartInNew;
                public int EndInNew;

                public Operation(Action action, int startInOld, int endInOld, int startInNew, int endInNew)
                {
                    this.Action = action;
                    this.StartInOld = startInOld;
                    this.EndInOld = endInOld;
                    this.StartInNew = startInNew;
                    this.EndInNew = endInNew;
                }
            }

            public enum Mode
            {
                character,
                tag,
                whitespace,
            }

            public enum Action
            {
                equal,
                delete,
                insert,
                none,
                replace
            }

        }
        #endregion

    } // class
}
