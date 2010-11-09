#region copyright
/*
Copyright © 2003,
Center for Intelligent Information Retrieval,
University of Massachusetts, Amherst.
All rights reserved.

Redistribution and use in source and binary forms, with or without modification,
are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice,
this list of conditions and the following disclaimer in the documentation
and/or other materials provided with the distribution.

3. The names "Center for Intelligent Information Retrieval" and
"University of Massachusetts" must not be used to endorse or promote products
derived from this software without prior written permission. To obtain
permission, contact info@ciir.cs.umass.edu.

THIS SOFTWARE IS PROVIDED BY UNIVERSITY OF MASSACHUSETTS AND OTHER CONTRIBUTORS
"AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO,
THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDERS OR CONTRIBUTORS BE
LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE
GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
SUCH DAMAGE.*/
/// <summary> <p>Title: Kstemmer</p>
/// <p>Description: This is a java version of Bob Krovetz' kstem stemmer</p>
/// <p>Copyright: Copyright (c) 2003</p>
/// <p>Company: CIIR University of Massachusetts Amherst (http://ciir.cs.umass.edu) </p>
/// </summary>
/// <author>  Sergio Guzman-Lara
/// </author>
/// <version>  1.0
/// </version>
/// 
/// Modified to C# :: Joe Langley
/// joe_langley78@hotmail.com
/// EE 2005
#endregion

using System;

using Lucene.Net;
using TokenStream = Lucene.Net.Analysis.TokenStream;
using Token = Lucene.Net.Analysis.Token;
using TokenFilter = Lucene.Net.Analysis.TokenFilter;

namespace LuceneFilters.KStemmer
{
    /// <summary>
    /// Transforms the token stream according to the KStem stemming algorithm.
    /// For more information about KStem see <a href="http://ciir.cs.umass.edu/pubfiles/ir-35.pdf">
    /// "Viewing Morphology as an Inference Process"</a>
    /// (Krovetz, R., Proceedings of the Sixteenth Annual International ACM SIGIR
    /// Conference on Research and Development in Information Retrieval, 191-203, 1993).
    /// Note: the input to the stemming filter must already be in lower case,
    /// so you will need to use LowerCaseFilter or LowerCaseTokenizer farther
    /// down the Tokenizer chain in order for this to work properly!
    /// <P />
    /// To use this filter with other analyzers, you'll want to write an
    /// Analyzer class that sets up the TokenStream chain as you want it.
    /// To use this with LowerCaseTokenizer, for example, you'd write an
    /// analyzer like this:
    /// <P />
    /// <code>
    /// public class MyAnalyzer : StandardAnalyzer
    /// {
    /// public MyAnalyzer() : base()
    /// {}
    /// public MyAnalyzer(String[] stopWords) : base(stopWords)
    /// {
    /// //This will use STOP_WORDS in your analyzer
    /// }
    /// public override TokenStream TokenStream(string strFieldName, TextReader reader)
    /// {
    /// // Inherit from StandardAnalyzer will also lower case it
    /// return new KStemFilter(new LowerCaseTokenizer(reader));
    /// }
    /// }
    /// </code>
    /// Modified to C# :: Joe Langley
    /// joe_langley78@hotmail.com
    /// EE 2005
    /// </summary>	
    public sealed class KStemFilter : TokenFilter
    {
        private KStemmer stemmer;

        /// <summary>
        ///  Create a KStemmer with the given cache size.
        /// </summary>
        /// <param name="in_Renamed">
        /// The TokenStream whose output will be the input to KStemFilter.
        /// </param>
        /// <param name="nCacheSize">
        /// Maximum number of entries to store in the
        /// Stemmer's cache (stems stored in this cache do not need to be
        /// recomputed, speeding up the stemming process).
        ///</param>
        public KStemFilter(TokenStream in_Renamed, int nCacheSize)
            : base(in_Renamed)
        {
            stemmer = new KStemmer(nCacheSize);
        }

        /// <summary>
        /// Create a KStemmer with the default cache size of 20 000 entries.
        /// </summary>
        /// <param name="in_Renamed">The TokenStream whose output will be the input to KStemFilter.</param>
        public KStemFilter(TokenStream in_Renamed)
            : base(in_Renamed)
        {
            stemmer = new KStemmer();
        }

        /// <summary>
        /// Returns the next, stemmed, input Token.
        /// </summary>
        /// <returns>
        ///  The stemed form of a token.
        /// </returns>
        /// <throws>IOException</throws>
        public override Token Next()
        {
            Token token = input.Next();
            if (token == null)
                return null;
            else
            {
                string str = stemmer.stem(token.TermText());
                //if ((System.Object) str != token.TermText())
                if (!str.Equals(token.TermText()))
                {
                    // Yes, I mean object reference comparison here
                    //token.TermText() = str;
                    return new Token(str, token.StartOffset(), token.EndOffset(), token.Type());
                }
                return token;
            }
        }
    }
}