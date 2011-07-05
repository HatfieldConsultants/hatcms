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
using System.Collections;
using System.IO;
using System.Text;

namespace LuceneFilters.KStemmer
{
    /// <summary>
    /// This class implements the KStem algorithm
    /// </summary>
    public class KStemmer
    {
        private String Suffix
        {
            set
            {
                setSuff(value, value.Length);
            }
        }

        /// <summary>
        /// Default size of the cache that stores <code>(word,stem)</code> pairs.
        /// This speeds up processing since Kstem works by
        /// sucessive "transformations" to the input word until a
        /// suitable stem is found.
        /// </summary>
        public static int DEFAULT_CACHE_SIZE = 20000;
        private const int nMaxWordLen = 100;

        private static readonly String[] exceptionWords = new String[] { "aide", "bathe", "caste", "cute", "dame", "dime", "doge", "done", "dune", "envelope", "gage", "grille", "grippe", "lobe", "mane", "mare", "nape", "node", "pane", "pate", "plane", "pope", "programme", "quite", "ripe", "rote", "rune", "sage", "severe", "shoppe", "sine", "slime", "snipe", "steppe", "suite", "swinge", "tare", "tine", "tope", "tripe", "twine" };
        private static readonly String[][] directConflations = new String[][] { new String[] { "aging", "age" }, new String[] { "going", "go" }, new String[] { "goes", "go" }, new String[] { "lying", "lie" }, new String[] { "using", "use" }, new String[] { "owing", "owe" }, new String[] { "suing", "sue" }, new String[] { "dying", "die" }, new String[] { "tying", "tie" }, new String[] { "vying", "vie" }, new String[] { "aged", "age" }, new String[] { "used", "use" }, new String[] { "vied", "vie" }, new String[] { "cued", "cue" }, new String[] { "died", "die" }, new String[] { "eyed", "eye" }, new String[] { "hued", "hue" }, new String[] { "iced", "ice" }, new String[] { "lied", "lie" }, new String[] { "owed", "owe" }, new String[] { "sued", "sue" }, new String[] { "toed", "toe" }, new String[] { "tied", "tie" }, new String[] { "does", "do" }, new String[] { "doing", "do" }, new String[] { "aeronautical", "aeronautics" }, new String[] { "mathematical", "mathematics" }, new String[] { "political", "politics" }, new String[] { "metaphysical", "metaphysics" }, new String[] { "cylindrical", "cylinder" }, new String[] { "nazism", "nazi" }, new String[] { "ambiguity", "ambiguous" }, new String[] { "barbarity", "barbarous" }, new String[] { "credulity", "credulous" }, new String[] { "generosity", "generous" }, new String[] { "spontaneity", "spontaneous" }, new String[] { "unanimity", "unanimous" }, new String[] { "voracity", "voracious" }, new String[] { "fled", "flee" }, new String[] { "miscarriage", "miscarry" } };
        private static readonly String[][] countryNationality = new String[][]{new String[]{"afghan", "afghanistan"}, new String[]{"african", "africa"}, new String[]{"albanian", "albania"}, new String[]{"algerian", "algeria"}, new String[]{"american", "america"}, new String[]{"andorran", "andorra"}, new String[]{"angolan", "angola"}, new String[]{"arabian", "arabia"}, new String[]{"argentine", "argentina"}, new String[]{"armenian", "armenia"}, new String[]{"asian", "asia"}, new String[]{"australian", "australia"}, new String[]{"austrian", "austria"}, new String[]{"azerbaijani", "azerbaijan"}, new String[]{"azeri", "azerbaijan"}, new String[]{"bangladeshi", "bangladesh"}, new String[]{"belgian", "belgium"}, new String[]{"bermudan", "bermuda"}, new String[]{"bolivian", "bolivia"}, new String[]{"bosnian", "bosnia"}, new String[]{"botswanan", "botswana"}, new String[]{"brazilian", "brazil"}, new String[]{"british", "britain"}, new String[]{"bulgarian", "bulgaria"}, new String[]{"burmese", "burma"}, new String[]{"californian", "california"}, new String[]{"cambodian", "cambodia"}, new String[]{"canadian", "canada"}, new String[]{"chadian", "chad"}, new String[]{"chilean", "chile"}, new String[]{"chinese", "china"}, new String[]{"colombian", "colombia"}, new String[]{"croat", "croatia"}, new String[]{"croatian", "croatia"}, new String[]{"cuban", "cuba"}, new String[]{"cypriot", "cyprus"}, new String[]{"czechoslovakian", "czechoslovakia"}, new String[]{"danish", "denmark"}, new String[]{"egyptian", "egypt"}, new String[]{"equadorian", "equador"}, new String[]{"eritrean", "eritrea"}, new String[]{"estonian", "estonia"}, new String[]{"ethiopian", "ethiopia"}, new String[]{"european", "europe"}, new 
			String[]{"fijian", "fiji"}, new String[]{"filipino", "philippines"}, new String[]{"finnish", "finland"}, new String[]{"french", "france"}, new String[]{"gambian", "gambia"}, new String[]{"georgian", "georgia"}, new String[]{"german", "germany"}, new String[]{"ghanian", "ghana"}, new String[]{"greek", "greece"}, new String[]{"grenadan", "grenada"}, new String[]{"guamian", "guam"}, new String[]{"guatemalan", "guatemala"}, new String[]{"guinean", "guinea"}, new String[]{"guyanan", "guyana"}, new String[]{"haitian", "haiti"}, new String[]{"hawaiian", "hawaii"}, new String[]{"holland", "dutch"}, new String[]{"honduran", "honduras"}, new String[]{"hungarian", "hungary"}, new String[]{"icelandic", "iceland"}, new String[]{"indonesian", "indonesia"}, new String[]{"iranian", "iran"}, new String[]{"iraqi", "iraq"}, new String[]{"iraqui", "iraq"}, new String[]{"irish", "ireland"}, new String[]{"israeli", "israel"}, new String[]{"italian", "italy"}, new String[]{"jamaican", "jamaica"}, new String[]{"japanese", "japan"}, new String[]{"jordanian", "jordan"}, new String[]{"kampuchean", "cambodia"}, new String[]{"kenyan", "kenya"}, new String[]{"korean", "korea"}, new String[]{"kuwaiti", "kuwait"}, new String[]{"lankan", "lanka"}, new String[]{"laotian", "laos"}, new String[]{"latvian", "latvia"}, new String[]{"lebanese", "lebanon"}, new String[]{"liberian", "liberia"}, new String[]{"libyan", "libya"}, new String[]{"lithuanian", "lithuania"}, new String[]{"macedonian", "macedonia"}, new String[]{"madagascan", "madagascar"}, new String[]{"malaysian", "malaysia"}, new String[]{"maltese", "malta"}, new String[]{"mauritanian", "mauritania"}, new String[]{"mexican", "mexico"}, new String
			[]{"micronesian", "micronesia"}, new String[]{"moldovan", "moldova"}, new String[]{"monacan", "monaco"}, new String[]{"mongolian", "mongolia"}, new String[]{"montenegran", "montenegro"}, new String[]{"moroccan", "morocco"}, new String[]{"myanmar", "burma"}, new String[]{"namibian", "namibia"}, new String[]{"nepalese", "nepal"}, new String[]{"nicaraguan", "nicaragua"}, new String[]{"nigerian", "nigeria"}, new String[]{"norwegian", "norway"}, new String[]{"omani", "oman"}, new String[]{"pakistani", "pakistan"}, new String[]{"panamanian", "panama"}, new String[]{"papuan", "papua"}, new String[]{"paraguayan", "paraguay"}, new String[]{"peruvian", "peru"}, new String[]{"portuguese", "portugal"}, new String[]{"romanian", "romania"}, new String[]{"rumania", "romania"}, new String[]{"rumanian", "romania"}, new String[]{"russian", "russia"}, new String[]{"rwandan", "rwanda"}, new String[]{"samoan", "samoa"}, new String[]{"scottish", "scotland"}, new String[]{"serb", " serbia"}, new String[]{"serbian", "serbia"}, new String[]{"siam", "thailand"}, new String[]{"siamese", "thailand"}, new String[]{"slovakia", "slovak"}, new String[]{"slovakian", "slovak"}, new String[]{"slovenian", "slovenia"}, new String[]{"somali", "somalia"}, new String[]{"somalian", "somalia"}, new String[]{"spanish", "spain"}, new String[]{"swedish", "sweden"}, new String[]{"swiss", "switzerland"}, new String[]{"syrian", "syria"}, new String[]{"taiwanese", "taiwan"}, new String[]{"tanzanian", "tanzania"}, new String[]{"texan", "texas"}, new String[]{"thai", "thailand"}, new String[]{"tunisian", "tunisia"}, new String[]{"turkish", "turkey"}, new String[]{"ugandan", "uganda"}, new String[]{"ukrainian", "ukraine"}, new 
			String[]{"uruguayan", "uruguay"}, new String[]{"uzbek", "uzbekistan"}, new String[]{"venezuelan", "venezuela"}, new String[]{"vietnamese", "viet"}, new String[]{"virginian", "virginia"}, new String[]{"yemeni", "yemen"}, new String[]{"yugoslav", "yugoslavia"}, new String[]{"yugoslavian", "yugoslavia"}, new String[]{"zambian", "zambia"}, new String[]{"zealander", "zealand"}, new String[]{"zimbabwean", "zimbabwe"}};

        private static readonly String[] supplementDict = new String[] { "aids", "applicator", "capacitor", "digitize", "electromagnet", "ellipsoid", "exosphere", "extensible", "ferromagnet", "graphics", "hydromagnet", "polygraph", "toroid", "superconduct", "backscatter", "connectionism" };
        private static readonly String[] properNouns = new String[]{"abrams", "achilles", "acropolis", "adams", "agnes", "aires", "alexander", "alexis", "alfred", "algiers", "alps", "amadeus", "ames", "amos", "andes", "angeles", "annapolis", "antilles", "aquarius", "archimedes", "arkansas", "asher", "ashly", "athens", "atkins", "atlantis", "avis", "bahamas", "bangor", "barbados", "barger", "bering", "brahms", "brandeis", "brussels", "bruxelles", "cairns", "camoros", "camus", "carlos", "celts", "chalker", "charles", "cheops", "ching", "christmas", "cocos", "collins", "columbus", "confucius", "conners", "connolly", "copernicus", "cramer", "cyclops", "cygnus", "cyprus", "dallas", "damascus", "daniels", "davies", "davis", "decker", "denning", "dennis", "descartes", "dickens", "doris", "douglas", "downs", "dreyfus", "dukakis", "dulles", "dumfries", "ecclesiastes", "edwards", "emily", "erasmus", "euphrates", "evans", "everglades", "fairbanks", "federales", "fisher", "fitzsimmons", "fleming", "forbes", "fowler", "france", "francis", "goering", "goodling", "goths", "grenadines", "guiness", "hades", "harding", "harris", "hastings", "hawkes", "hawking", "hayes", "heights", "hercules", "himalayas", "hippocrates", "hobbs", "holmes", "honduras", "hopkins", "hughes", "humphreys", "illinois", "indianapolis", "inverness", "iris", "iroquois", "irving", "isaacs", "italy", "james", "jarvis", "jeffreys", "jesus", "jones", "josephus", "judas", "julius", "kansas", "keynes", "kipling", "kiwanis", "lansing", "laos", "leeds", "levis", "leviticus", "lewis", "louis", "maccabees", "madras", "maimonides", "maldive", "massachusetts", "matthews", "mauritius", "memphis", "mercedes", "midas", "mingus", "minneapolis", "mohammed", "moines", "morris", "moses", "myers", "myknos", "nablus", "nanjing", "nantes", "naples", "neal", "netherlands", "nevis", "nostradamus", "oedipus", "olympus", "orleans", "orly", "papas", "paris", "parker", "pauling", "peking", "pershing", "peter", "peters", "philippines", "phineas", "pisces", "pryor", 
			"pythagoras", "queens", "rabelais", "ramses", "reynolds", "rhesus", "rhodes", "richards", "robins", "rodgers", "rogers", "rubens", "sagittarius", "seychelles", "socrates", "texas", "thames", "thomas", "tiberias", "tunis", "venus", "vilnius", "wales", "warner", "wilkins", "williams", "wyoming", "xmas", "yonkers", "zeus", "frances", "aarhus", "adonis", "andrews", "angus", "antares", "aquinas", "arcturus", "ares", "artemis", "augustus", "ayers", "barnabas", "barnes", "becker", "bejing", "biggs", "billings", "boeing", "boris", "borroughs", "briggs", "buenos", "calais", "caracas", "cassius", "cerberus", "ceres", "cervantes", "chantilly", "chartres", "chester", "connally", "conner", "coors", "cummings", "curtis", "daedalus", "dionysus", "dobbs", "dolores", "edmonds"};

        internal class DictEntry
        {
            internal bool bException;
            internal string strRoot;
            internal DictEntry(string strRoot, bool bIsException)
            {
                this.strRoot = strRoot;
                this.bException = bIsException;
            }
        }

        private static Hashtable dict_ht = null;
        private int nMaxCacheSize;
        private Hashtable stem_ht = null;
        private StringBuilder word;
        private int j; // index of final letter in stem (within word)
        private int k; /* INDEX of final letter in word.
		You must add 1 to k to get the current length of word.
		When you want the length of word, use the method
		wordLength, which returns (k+1). */

        private void initializeStemHash()
        {
            stem_ht = Hashtable.Synchronized(new Hashtable());
        }

        private char finalChar()
        {
            return word[k];
        }

        private char penultChar()
        {
            return word[k - 1];
        }

        private bool isVowel(int nIndex)
        {
            return !isCons(nIndex);
        }

        private bool isCons(int nIndex)
        {
            char ch;

            ch = word[nIndex];

            if ((ch == 'a') || (ch == 'e') || (ch == 'i') || (ch == 'o') || (ch == 'u'))
                return false;
            if ((ch != 'y') || (nIndex == 0))
                return true;
            else
                return (!isCons(nIndex - 1));
        }

        private bool isAlpha(char ch)
        {
            if ((ch >= 'a') && (ch <= 'z'))
                return true;
            if ((ch >= 'A') && (ch <= 'Z'))
                return true;
            return false;
        }

        // length of stem within word 
        private int stemLength()
        {
            return j + 1;
        }

        private bool endsIn(string str)
        {
            bool bMatch;
            int nSufflength = str.Length;

            int nR = word.Length - nSufflength; // length of word before this suffix 
            if (nSufflength > k)
                return false;

            bMatch = true;
            for (int r1 = nR, i = 0; (i < nSufflength) && (bMatch); i++, r1++)
            {
                if (str[i] != word[r1])
                    bMatch = false;
            }

            if (bMatch)
                j = nR - 1;
            // index of the character BEFORE the posfix
            else
                j = k;
            return bMatch;
        }

        private DictEntry wordInDict()
        {
            string strS = word.ToString();
            return (DictEntry)dict_ht[strS];
        }

        // Convert plurals to singular form, and '-ies' to 'y'
        private void plural()
        {
            if (finalChar() == 's')
            {
                if (endsIn("ies"))
                {
                    word.Length = j + 3;
                    k--;
                    if (lookup(word.ToString()))
                        // ensure calories -> calorie
                        return;
                    k++;
                    word.Append('s');
                    Suffix = "y";
                }
                else if (endsIn("es"))
                {
                    // try just removing the "s"
                    word.Length = j + 2;
                    k--;

                    /* note: don't check for exceptions here.  So, `aides' -> `aide',
                    but `aided' -> `aid'.  The exception for double s is used to prevent
                    crosses -> crosse.  This is actually correct if crosses is a plural
                    noun (a type of racket used in lacrosse), but the verb is much more
                    common */

                    if ((j > 0) && (lookup(word.ToString())) && !((word[j] == 's') && (word[j - 1] == 's')))
                        return;

                    // try removing the "es" 
                    word.Length = j + 1;
                    k--;
                    if (lookup(word.ToString()))
                        return;

                    // the default is to retain the "e"
                    word.Append('e');
                    k++;
                    return;
                }
                else
                {
                    if (word.Length > 3 && penultChar() != 's' && !endsIn("ous"))
                    {
                        // unless the word ends in "ous" or a double "s", remove the final "s"
                        word.Length = k;
                        k--;
                    }
                }
            }
        }

        // replace old suffix with str
        private void setSuff(string str, int nLen)
        {
            word.Length = j + 1;
            for (int l = 0; l < nLen; l++)
            {
                word.Append(str[l]);
            }
            k = j + nLen;
        }

        // Returns true if str is found in the dictionary
        private bool lookup(string str)
        {
            if (dict_ht.ContainsKey(str))
                return true;
            else
                return false;
        }

        // convert past tense (-ed) to present, and `-ied' to `y'
        private void pastTense()
        {
            /* Handle words less than 5 letters with a direct mapping
            This prevents (fled -> fl).  */

            if (word.Length <= 4)
                return;

            if (endsIn("ied"))
            {
                word.Length = j + 3;
                k--;
                if (lookup(word.ToString()))
                    /* we almost always want to convert -ied to -y, but
                        return ; this isn't true for short words (died->die)*/
                    k++; // I don't know any long words that this applies to, 
                word.Append('d'); // but just in case...                              
                Suffix = "y";
                return;
            }

            // the vowelInStem() is necessary so we don't stem acronyms
            if (endsIn("ed") && vowelInStem())
            {
                // see if the root ends in `e'
                word.Length = j + 2;
                k = j + 1;

                DictEntry entry = wordInDict();
                if (entry != null)
                    if (!entry.bException)
                        // if it's in the dictionary and not an exception
                        return;

                // try removing the "ed"
                word.Length = j + 1;
                k = j;
                if (lookup(word.ToString()))
                    return;


                /* try removing a doubled consonant.  if the root isn't found in
                the dictionary, the default is to leave it doubled.  This will
                correctly capture `backfilled' -> `backfill' instead of
                `backfill' -> `backfille', and seems correct most of the time  */

                if (doubleC(k))
                {
                    word.Length = k;
                    k--;
                    if (lookup(word.ToString()))
                        return;
                    word.Append(word[k]);
                    k++;
                    return;
                }

                /* if we have a `un-' prefix, then leave the word alone  
                  (this will sometimes screw up with `under-', but we   
                  will take care of that later) */

                if ((word[0] == 'u') && (word[1] == 'n'))
                {
                    word.Append('e');
                    word.Append('d');
                    k = k + 2;
                    return;
                }

                /* it wasn't found by just removing the `d' or the `ed', so prefer to
                end with an `e' (e.g., `microcoded' -> `microcode'). */

                word.Length = j + 1;
                word.Append('e');
                k = j + 1;
                return;
            }
        }

        // return TRUE if word ends with a double consonant
        private bool doubleC(int nI)
        {
            if (nI < 1)
                return false;

            if (word[nI] != word[nI - 1])
                return false;
            return (isCons(nI));
        }

        private bool vowelInStem()
        {
            for (int i = 0; i < stemLength(); i++)
            {
                if (isVowel(i))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Create a KStemmer, with the given cache size.
        /// </summary>
        /// <param name="nCacheSize"> Maximum number of words to be stored in the cache.</param>
        internal KStemmer(int nCacheSize)
        {
            if (nCacheSize >= 0)
                nMaxCacheSize = nCacheSize;
            if (dict_ht == null)
                initializeDictHash();
        }

        /// <summary>
        /// Create a KStemmer with the default cache size (20000 words).
        /// </summary>
        internal KStemmer()
        {
            nMaxCacheSize = DEFAULT_CACHE_SIZE;
            if (dict_ht == null)
                initializeDictHash();
        }

        #region Initialize Dictionary Hash

        private static void initializeDictHash()
        {
            lock (typeof(LuceneFilters.KStemmer.KStemmer))
            {
                DictEntry defaultEntry;
                DictEntry entry;

                if (dict_ht != null)
                    return;

                dict_ht = Hashtable.Synchronized(new Hashtable());
                for (int i = 0; i < exceptionWords.Length; i++)
                {
                    if (!dict_ht.ContainsKey(exceptionWords[i]))
                    {
                        entry = new DictEntry(exceptionWords[i], true);
                        dict_ht[exceptionWords[i]] = entry;
                    }
                    else
                    {
                        Console.Out.WriteLine("Warning: Entry [" + exceptionWords[i] + "] already in dictionary 1");
                    }
                }

                for (int i = 0; i < directConflations.Length; i++)
                {
                    if (!dict_ht.ContainsKey(directConflations[i][0]))
                    {
                        entry = new DictEntry(directConflations[i][1], false);
                        dict_ht[directConflations[i][0]] = entry;
                    }
                    else
                    {
                        Console.Out.WriteLine("Warning: Entry [" + directConflations[i][0] + "] already in dictionary 2");
                    }
                }

                for (int i = 0; i < countryNationality.Length; i++)
                {
                    if (!dict_ht.ContainsKey(countryNationality[i][0]))
                    {
                        entry = new DictEntry(countryNationality[i][1], false);
                        dict_ht[countryNationality[i][0]] = entry;
                    }
                    else
                    {
                        Console.Out.WriteLine("Warning: Entry [" + countryNationality[i][0] + "] already in dictionary 3");
                    }
                }

                defaultEntry = new DictEntry(null, false);

                String[] array;
                array = KStemData1.data;

                for (int i = 0; i < array.Length; i++)
                {
                    if (!dict_ht.ContainsKey(array[i]))
                    {
                        dict_ht[array[i]] = defaultEntry;
                    }
                    else
                    {
                        Console.Out.WriteLine("Warning: Entry [" + array[i] + "] already in dictionary 4");
                    }
                }

                array = KStemData2.data;
                for (int i = 0; i < array.Length; i++)
                {
                    if (!dict_ht.ContainsKey(array[i]))
                    {
                        dict_ht[array[i]] = defaultEntry;
                    }
                    else
                    {
                        Console.Out.WriteLine("Warning: Entry [" + array[i] + "] already in dictionary 4");
                    }
                }

                array = KStemData3.data;
                for (int i = 0; i < array.Length; i++)
                {
                    if (!dict_ht.ContainsKey(array[i]))
                    {
                        dict_ht[array[i]] = defaultEntry;
                    }
                    else
                    {
                        Console.Out.WriteLine("Warning: Entry [" + array[i] + "] already in dictionary 4");
                    }
                }

                array = KStemData4.data;
                for (int i = 0; i < array.Length; i++)
                {
                    if (!dict_ht.ContainsKey(array[i]))
                    {
                        dict_ht[array[i]] = defaultEntry;
                    }
                    else
                    {
                        Console.Out.WriteLine("Warning: Entry [" + array[i] + "] already in dictionary 4");
                    }
                }

                array = KStemData5.data;
                for (int i = 0; i < array.Length; i++)
                {
                    if (!dict_ht.ContainsKey(array[i]))
                    {
                        dict_ht[array[i]] = defaultEntry;
                    }
                    else
                    {
                        Console.Out.WriteLine("Warning: Entry [" + array[i] + "] already in dictionary 4");
                    }
                }

                array = KStemData6.data;
                for (int i = 0; i < array.Length; i++)
                {
                    if (!dict_ht.ContainsKey(array[i]))
                    {
                        dict_ht[array[i]] = defaultEntry;
                    }
                    else
                    {
                        Console.Out.WriteLine("Warning: Entry [" + array[i] + "] already in dictionary 4");
                    }
                }

                array = KStemData7.data;
                for (int i = 0; i < array.Length; i++)
                {
                    if (!dict_ht.ContainsKey(array[i]))
                    {
                        dict_ht[array[i]] = defaultEntry;
                    }
                    else
                    {
                        Console.Out.WriteLine("Warning: Entry [" + array[i] + "] already in dictionary 4");
                    }
                }

                for (int i = 0; i < KStemData8.data.Length; i++)
                {
                    if (!dict_ht.ContainsKey(KStemData8.data[i]))
                    {
                        dict_ht[KStemData8.data[i]] = defaultEntry;
                    }
                    else
                    {
                        Console.Out.WriteLine("Warning: Entry [" + KStemData8.data[i] + "] already in dictionary 4");
                    }
                }

                for (int i = 0; i < supplementDict.Length; i++)
                {
                    if (!dict_ht.ContainsKey(supplementDict[i]))
                    {
                        dict_ht[supplementDict[i]] = defaultEntry;
                    }
                    else
                    {
                        Console.Out.WriteLine("Warning: Entry [" + supplementDict[i] + "] already in dictionary 5");
                    }
                }

                for (int i = 0; i < properNouns.Length; i++)
                {
                    if (!dict_ht.ContainsKey(properNouns[i]))
                    {
                        dict_ht[properNouns[i]] = defaultEntry;
                    }
                    else
                    {
                        Console.Out.WriteLine("Warning: Entry [" + properNouns[i] + "] already in dictionary 6");
                    }
                }
            }
        }

        #endregion

        #region Handle derivational endings

        // handle `-ing' endings
        private void aspect()
        {
            /* handle short words (aging -> age) via a direct mapping.  This
            prevents (thing -> the) in the version of this routine that
            ignores inflectional variants that are mentioned in the dictionary
            (when the root is also present) */

            if (word.Length <= 5)
                return;

            // the vowelinstem() is necessary so we don't stem acronyms
            if (endsIn("ing") && vowelInStem())
            {

                // try adding an `e' to the stem and check against the dictionary
                word[j + 1] = 'e';
                word.Length = j + 2;
                k = j + 1;

                DictEntry entry = wordInDict();
                if (entry != null)
                {
                    if (!entry.bException)
                        // if it's in the dictionary and not an exception
                        return;
                }

                // adding on the `e' didn't work, so remove it
                word.Length = k;
                k--; // note that `ing' has also been removed 

                if (lookup(word.ToString()))
                    return;

                // if I can remove a doubled consonant and get a word, then do so 
                if (doubleC(k))
                {
                    k--;
                    word.Length = k + 1;
                    if (lookup(word.ToString()))
                        return;
                    word.Append(word[k]); // restore the doubled consonant 

                    /* the default is to leave the consonant doubled            
                      (e.g.,`fingerspelling' -> `fingerspell').  Unfortunately 
                      `bookselling' -> `booksell' and `mislabelling' -> `mislabell'). 
                      Without making the algorithm significantly more complicated, this 
                      is the best I can do */
                    k++;
                    return;
                }

                /* the word wasn't in the dictionary after removing the stem, and then
                checking with and without a final `e'.  The default is to add an `e'
                unless the word ends in two consonants, so `microcoding' -> `microcode'.
                The two consonants restriction wouldn't normally be necessary, but is
                needed because we don't try to deal with prefixes and compounds, and
                most of the time it is correct (e.g., footstamping -> footstamp, not
                footstampe; however, decoupled -> decoupl).  We can prevent almost all
                of the incorrect stems if we try to do some prefix analysis first */

                if ((j > 0) && isCons(j) && isCons(j - 1))
                {
                    k = j;
                    word.Length = k + 1;
                    return;
                }

                word.Length = j + 1;
                word.Append('e');
                k = j + 1;
                return;
            }
        }

        /* this routine deals with -ity endings.  It accepts -ability, -ibility,
        and -ality, even without checking the dictionary because they are so
        productive.  The first two are mapped to -ble, and the -ity is remove
        for the latter */
        private void ityEndings()
        {
            int old_k = k;

            if (endsIn("ity"))
            {
                word.Length = j + 1; // try just removing -ity
                k = j;
                if (lookup(word.ToString()))
                    return;
                word.Append('e'); // try removing -ity and adding -e 
                k = j + 1;
                if (lookup(word.ToString()))
                    return;
                word[j + 1] = 'i';
                word.Append("ty");
                k = old_k;
                // the -ability and -ibility endings are highly productive, so just accept them 
                if ((j > 0) && (word[j - 1] == 'i') && (word[j] == 'l'))
                {
                    word.Length = j - 1;
                    word.Append("le"); /* convert to -ble */
                    k = j;
                    return;
                }


                // ditto for -ivity
                if ((j > 0) && (word[j - 1] == 'i') && (word[j] == 'v'))
                {
                    word.Length = j + 1;
                    word.Append('e'); // convert to -ive
                    k = j + 1;
                    return;
                }
                // ditto for -ality
                if ((j > 0) && (word[j - 1] == 'a') && (word[j] == 'l'))
                {
                    word.Length = j + 1;
                    k = j;
                    return;
                }

                /* if the root isn't in the dictionary, and the variant *is*
                there, then use the variant.  This allows `immunity'->`immune',
                but prevents `capacity'->`capac'.  If neither the variant nor
                the root form are in the dictionary, then remove the ending
                as a default */

                if (lookup(word.ToString()))
                    return;

                /* the default is to remove -ity altogether */
                word.Length = j + 1;
                k = j;
                return;
            }
        }

        // handle -ence and -ance 
        private void nceEndings()
        {
            int old_k = k;
            char word_char;

            if (endsIn("nce"))
            {
                if (!((word[j] == 'e') || (word[j] == 'a')))
                    return;
                word_char = word[j];
                word.Length = j;
                word.Append('e'); // try converting -e/ance to -e (adherance/adhere) 
                k = j;
                if (lookup(word.ToString()))
                    return;
                word.Length = j; // try removing -e/ance altogether (disappearance/disappear) 
                k = j - 1;
                if (lookup(word.ToString()))
                    return;
                word.Append(word_char); // restore the original ending 
                word.Append("nce");
                k = old_k;
            }
            return;
        }

        // handle -ness
        private void nessEndings()
        {
            if (endsIn("ness"))
            {
                // this is a very productive endings, so just accept it 
                word.Length = j + 1;
                k = j;
                if (word[j] == 'i')
                    word[j] = 'y';
            }
            return;
        }

        // handle -ism
        private void ismEndings()
        {
            if (endsIn("ism"))
            {
                /* this is a very productive ending, so just accept it */
                word.Length = j + 1;
                k = j;
            }
            return;
        }

        // this routine deals with -ment endings.
        private void mentEndings()
        {
            int old_k = k;

            if (endsIn("ment"))
            {
                word.Length = j + 1;
                k = j;
                if (lookup(word.ToString()))
                    return;
                word.Append("ment");
                k = old_k;
            }
            return;
        }

        // this routine deals with -ize endings.
        private void izeEndings()
        {
            int old_k = k;

            if (endsIn("ize"))
            {
                word.Length = j + 1; // try removing -ize entirely
                k = j;
                if (lookup(word.ToString()))
                    return;
                word.Append('i');

                if (doubleC(j))
                {
                    // allow for a doubled consonant 
                    word.Length = j;
                    k = j - 1;
                    if (lookup(word.ToString()))
                        return;
                    word.Append(word[j - 1]);
                }

                word.Length = j + 1;
                word.Append('e'); // try removing -ize and adding -e 
                k = j + 1;
                if (lookup(word.ToString()))
                    return;
                word.Length = j + 1;
                word.Append("ize");
                k = old_k;
            }
            return;
        }

        // handle -ency and -ancy
        private void ncyEndings()
        {
            if (endsIn("ncy"))
            {
                if (!((word[j] == 'e') || (word[j] == 'a')))
                    return;
                word[j + 2] = 't'; // try converting -ncy to -nt
                word.Length = j + 3;
                k = j + 2;

                if (lookup(word.ToString()))
                    return;

                word[j + 2] = 'c'; // the default is to convert it to -nce 
                word.Append('e');
                k = j + 3;
            }
            return;
        }

        // handle -able and -ible
        private void bleEndings()
        {
            int old_k = k;
            char word_char;

            if (endsIn("ble"))
            {
                if (!((word[j] == 'a') || (word[j] == 'i')))
                    return;
                word_char = word[j];
                word.Length = j; // try just removing the ending
                k = j - 1;
                if (lookup(word.ToString()))
                    return;
                if (doubleC(k))
                {
                    // allow for a doubled consonant
                    word.Length = k;
                    k--;
                    if (lookup(word.ToString()))
                        return;
                    k++;
                    word.Append(word[k - 1]);
                }
                word.Length = j;
                word.Append('e'); // try removing -a/ible and adding -e
                k = j;
                if (lookup(word.ToString()))
                    return;
                word.Length = j;
                word.Append("ate"); // try removing -able and adding -ate
                // (e.g., compensable/compensate)
                k = j + 2;
                if (lookup(word.ToString()))
                    return;
                word.Length = j;
                word.Append(word_char); // restore the original values 
                word.Append("ble");
                k = old_k;
            }
            return;
        }

        /* handle -ic endings.   This is fairly straightforward, but this is
        also the only place we try *expanding* an ending, -ic -> -ical.
        This is to handle cases like `canonic' -> `canonical' */
        private void icEndings()
        {
            if (endsIn("ic"))
            {
                word.Length = j + 3;
                word.Append("al"); // try converting -ic to -ical 
                k = j + 4;
                if (lookup(word.ToString()))
                    return;

                word[j + 1] = 'y'; // try converting -ic to -y 
                word.Length = j + 2;
                k = j + 1;
                if (lookup(word.ToString()))
                    return;

                word[j + 1] = 'e'; // try converting -ic to -e 
                if (lookup(word.ToString()))
                    return;

                word.Length = j + 1; // try removing -ic altogether
                k = j;
                if (lookup(word.ToString()))
                    return;
                word.Append("ic"); // restore the original ending
                k = j + 2;
            }
            return;
        }

        // handle some derivational endings
        /* this routine deals with -ion, -ition, -ation, -ization, and -ication.  The
        -ization ending is always converted to -ize */
        private void ionEndings()
        {
            int old_k = k;

            if (endsIn("ization"))
            {
                // the -ize ending is very productive, so simply accept it as the root 
                word.Length = j + 3;
                word.Append('e');
                k = j + 3;
                return;
            }

            if (endsIn("ition"))
            {
                word.Length = j + 1;
                word.Append('e');
                k = j + 1;
                if (lookup(word.ToString()))
                    // remove -ition and add `e', and check against the dictionary 
                    return; // (e.g., definition->define, opposition->oppose) 

                // restore original values
                word.Length = j + 1;
                word.Append("ition");
                k = old_k;
            }

            if (endsIn("ation"))
            {
                word.Length = j + 3;
                word.Append('e');
                k = j + 3;
                if (lookup(word.ToString()))
                    // remove -ion and add `e', and check against the dictionary 
                    return; // (elmination -> eliminate)

                word.Length = j + 1;
                word.Append('e'); // remove -ation and add `e', and check against the dictionary 
                k = j + 1;
                if (lookup(word.ToString()))
                    return;

                word.Length = j + 1; // just remove -ation (resignation->resign) and check dictionary 
                k = j;
                if (lookup(word.ToString()))
                    return;

                // restore original values 
                word.Length = j + 1;
                word.Append("ation");
                k = old_k;
            }

            /* test -ication after -ation is attempted (e.g., `complication->complicate'
            rather than `complication->comply') */

            if (endsIn("ication"))
            {
                word.Length = j + 1;
                word.Append('y');
                k = j + 1;
                if (lookup(word.ToString()))
                    // remove -ication and add `y', and check against the dictionary
                    return; // (e.g., amplification -> amplify)

                // restore original values
                word.Length = j + 1;
                word.Append("ication");
                k = old_k;
            }


            if (endsIn("ion"))
            {
                word.Length = j + 1;
                word.Append('e');
                k = j + 1;
                if (lookup(word.ToString()))
                    // remove -ion and add `e', and check against the dictionary 
                    return;

                word.Length = j + 1;
                k = j;
                if (lookup(word.ToString()))
                    // remove -ion, and if it's found, treat that as the root 
                    return;

                // restore original values
                word.Length = j + 1;
                word.Append("ion");
                k = old_k;
            }
            return;
        }

        // this routine deals with -er, -or, -ier, and -eer.  The -izer ending is always converted to -ize
        private void erAndOrEndings()
        {
            int old_k = k;

            char word_char; // so we can remember if it was -er or -or 

            if (endsIn("izer"))
            {
                /* -ize is very productive, so accept it as the root */
                word.Length = j + 4;
                k = j + 3;
                return;
            }

            if (endsIn("er") || endsIn("or"))
            {
                word_char = word[j + 1];
                if (doubleC(j))
                {
                    word.Length = j;
                    k = j - 1;
                    if (lookup(word.ToString()))
                        return;
                    word.Append(word[j - 1]); // restore the doubled consonant 
                }

                if (word[j] == 'i')
                {
                    // do we have a -ier ending?
                    word[j] = 'y';
                    word.Length = j + 1;
                    k = j;
                    if (lookup(word.ToString()))
                        // yes, so check against the dictionary 
                        return;
                    word[j] = 'i'; // restore the endings
                    word.Append('e');
                }

                if (word[j] == 'e')
                {
                    // handle -eer
                    word.Length = j;
                    k = j - 1;
                    if (lookup(word.ToString()))
                        return;
                    word.Append('e');
                }

                word.Length = j + 2; // remove the -r ending
                k = j + 1;
                if (lookup(word.ToString()))
                    return;
                word.Length = j + 1; // try removing -er/-or
                k = j;
                if (lookup(word.ToString()))
                    return;
                word.Append('e'); // try removing -or and adding -e
                k = j + 1;
                if (lookup(word.ToString()))
                    return;
                word.Length = j + 1;
                word.Append(word_char);
                word.Append('r'); // restore the word to the way it was
                k = old_k;
            }
        }

        /* this routine deals with -ly endings.  The -ally ending is always converted to -al
        Sometimes this will temporarily leave us with a non-word (e.g., heuristically
        maps to heuristical), but then the -al is removed in the next step.  */
        private void lyEndings()
        {
            int old_k = k;

            if (endsIn("ly"))
            {
                word[j + 2] = 'e'; // try converting -ly to -le 

                if (lookup(word.ToString()))
                    return;
                word[j + 2] = 'y';

                word.Length = j + 1; // try just removing the -ly
                k = j;

                if (lookup(word.ToString()))
                    return;

                if ((j > 0) && (word[j - 1] == 'a') && (word[j] == 'l'))
                    // always convert -ally to -al
                    return;
                word.Append("ly");
                k = old_k;

                if ((j > 0) && (word[j - 1] == 'a') && (word[j] == 'b'))
                {
                    // always convert -ably to -able 
                    word[j + 2] = 'e';
                    k = j + 2;
                    return;
                }

                if (word[j] == 'i')
                {
                    // e.g., militarily -> military 
                    word.Length = j;
                    word.Append('y');
                    k = j;
                    if (lookup(word.ToString()))
                        return;
                    word.Length = j;
                    word.Append("ily");
                    k = old_k;
                }

                word.Length = j + 1; // the default is to remove -ly

                k = j;
            }
            return;
        }

        /* this routine deals with -al endings.  Some of the endings from the previous routine
        are finished up here.  */
        private void alEndings()
        {
            int old_k = k;

            if (word.Length < 4)
                return;
            if (endsIn("al"))
            {
                word.Length = j + 1;
                k = j;
                if (lookup(word.ToString()))
                    // try just removing the -al
                    return;

                if (doubleC(j))
                {
                    // allow for a doubled consonant
                    word.Length = j;
                    k = j - 1;
                    if (lookup(word.ToString()))
                        return;
                    word.Append(word[j - 1]);
                }

                word.Length = j + 1;
                word.Append('e'); // try removing the -al and adding -e
                k = j + 1;
                if (lookup(word.ToString()))
                    return;

                word.Length = j + 1;
                word.Append("um"); // try converting -al to -um
                // (e.g., optimal - > optimum )
                k = j + 2;
                if (lookup(word.ToString()))
                    return;

                word.Length = j + 1;
                word.Append("al"); // restore the ending to the way it was
                k = old_k;

                if ((j > 0) && (word[j - 1] == 'i') && (word[j] == 'c'))
                {
                    word.Length = j - 1; // try removing -ical 
                    k = j - 2;
                    if (lookup(word.ToString()))
                        return;

                    word.Length = j - 1;
                    word.Append('y'); // try turning -ical to -y (e.g., bibliographical) 
                    k = j - 1;
                    if (lookup(word.ToString()))
                        return;

                    word.Length = j - 1;
                    word.Append("ic"); // the default is to convert -ical to -ic 
                    k = j;
                    return;
                }

                if (word[j] == 'i')
                {
                    // sometimes -ial endings should be removed 
                    word.Length = j; // (sometimes it gets turned into -y, but we 
                    k = j - 1; // aren't dealing with that case for now) 
                    if (lookup(word.ToString()))
                        return;
                    word.Append("ial");
                    k = old_k;
                }
            }
            return;
        }

        /* this routine deals with -ive endings.  It normalizes some of the
        -ative endings directly, and also maps some -ive endings to -ion. */
        private void iveEndings()
        {
            int old_k = k;

            if (endsIn("ive"))
            {
                word.Length = j + 1; // try removing -ive entirely 
                k = j;
                if (lookup(word.ToString()))
                    return;

                word.Append('e'); // try removing -ive and adding -e 
                k = j + 1;
                if (lookup(word.ToString()))
                    return;
                word.Length = j + 1;
                word.Append("ive");
                if ((j > 0) && (word[j - 1] == 'a') && (word[j] == 't'))
                {
                    word[j - 1] = 'e'; // try removing -ative and adding -e 
                    word.Length = j; // (e.g., determinative -> determine) 
                    k = j - 1;
                    if (lookup(word.ToString()))
                        return;
                    word.Length = j - 1; // try just removing -ative 
                    if (lookup(word.ToString()))
                        return;

                    word.Append("ative");
                    k = old_k;
                }

                // try mapping -ive to -ion (e.g., injunctive/injunction)
                word[j + 2] = 'o';
                word[j + 3] = 'n';
                if (lookup(word.ToString()))
                    return;

                word[j + 2] = 'v'; // restore the original values 
                word[j + 3] = 'e';
                k = old_k;
            }
            return;
        }

        #endregion

        #region Get/Return Stem of word

        /// <summary>
        /// Returns the stem of a word.
        /// </summary>
        /// <param name="strTerm">The word to be stemmed.</param>
        /// <returns>The stem form of the term.</returns>
        internal virtual String stem(string strTerm)
        {
            bool bStemIt;
            string strResult;
            string strOriginal;

            if (stem_ht == null)
                initializeStemHash();

            k = strTerm.Length - 1;

            /* If the word is too long or too short, or not
            entirely alphabetic, just lowercase copy it
            into stem and return */
            bStemIt = true;
            if ((k <= 1) || (k >= nMaxWordLen - 1))
            {
                bStemIt = false;
            }
            else
            {
                word = new StringBuilder(strTerm.Length);
                for (int i = 0; i < strTerm.Length; i++)
                {
                    char ch = Char.ToLower(strTerm[i]);
                    word.Append(ch);
                    if (!isAlpha(ch))
                    {
                        bStemIt = false;
                        break;
                    }
                }
            }
            if (!bStemIt)
            {
                return strTerm.ToLower();
            }

            // Check to see if it's in the cache
            strOriginal = word.ToString();
            if (stem_ht.ContainsKey(strOriginal))
            {
                return (String)stem_ht[strOriginal];
            }

            strResult = strOriginal; // default response

            /* This while loop will never be executed more than one time;
            it is here only to allow the break statement to be used to escape
            as soon as a word is recognized */

            DictEntry entry = null;

            while (true)
            {
                entry = wordInDict();
                if (entry != null)
                    break;
                plural();
                entry = wordInDict();
                if (entry != null)
                    break;
                pastTense();
                entry = wordInDict();
                if (entry != null)
                    break;
                aspect();
                entry = wordInDict();
                if (entry != null)
                    break;
                ityEndings();
                entry = wordInDict();
                if (entry != null)
                    break;
                nessEndings();
                entry = wordInDict();
                if (entry != null)
                    break;
                ionEndings();
                entry = wordInDict();
                if (entry != null)
                    break;
                erAndOrEndings();
                entry = wordInDict();
                if (entry != null)
                    break;
                lyEndings();
                entry = wordInDict();
                if (entry != null)
                    break;
                alEndings();
                entry = wordInDict();
                if (entry != null)
                    break;
                iveEndings();
                entry = wordInDict();
                if (entry != null)
                    break;
                izeEndings();
                entry = wordInDict();
                if (entry != null)
                    break;
                mentEndings();
                entry = wordInDict();
                if (entry != null)
                    break;
                bleEndings();
                entry = wordInDict();
                if (entry != null)
                    break;
                ismEndings();
                entry = wordInDict();
                if (entry != null)
                    break;
                icEndings();
                entry = wordInDict();
                if (entry != null)
                    break;
                ncyEndings();
                entry = wordInDict();
                if (entry != null)
                    break;
                nceEndings();
                entry = wordInDict();
                break;
            }

            /* try for a direct mapping (allows for cases like `Italian'->`Italy' and
            `Italians'->`Italy')
            */
            if (entry != null)
            {
                if (entry.strRoot != null)
                    strResult = entry.strRoot;
                else
                    strResult = word.ToString();
            }
            else
                strResult = word.ToString();

            // Enter into cache, at the place not used by the last cache hit
            if (stem_ht.Count < nMaxCacheSize)
            {
                // Add term to cache
                stem_ht[strOriginal] = strResult;
            }

            return strResult;
        }

        #endregion

        #region testing

        static private void usage()
        {
            Console.Out.WriteLine("Usage:");
            Console.Out.WriteLine("  KStemmer <inputFile>");
            Console.Out.WriteLine("    or");
            Console.Out.WriteLine("  KStemmer -w <word>");
            Environment.Exit(1);
        }

        /// <summary>
        /// Testing
        /// <p>Usage:
        /// <ul>
        /// <li><code><B>KStemmer &lt;inputFile&gt;</B></code>
        /// <p> Will stem all words
        /// in <code>&lt;inputFile&gt;</code> (one word per line).
        /// <p>
        /// <li><code><B>KStemmer -w &lt;word&gt;</B></code>
        /// <p> Will stem a single
        /// <code>&lt;word&gt;</code>
        /// <p>
        /// </ul>
        /// In either case, the output is sent to <code>System.out</code>
        /// </summary>
        [STAThread]
        static public void Main(String[] args)
        {
            KStemmer stemmer = new KStemmer();
            string line = null;

            if ((args.Length == 0) || (args.Length > 2))
            {
                usage();
            }

            if (args.Length == 2)
            {
                if (!args[0].Equals("-w"))
                {
                    usage();
                }
                Console.Out.WriteLine(args[1] + " " + stemmer.stem(args[1]));
                return;
            }

            // If we get here, we are about to process a file
            try
            {
                StreamReader reader = new StreamReader(new StreamReader(args[0], Encoding.Default).BaseStream, new StreamReader(args[0], Encoding.Default).CurrentEncoding);

                line = reader.ReadLine();
                while (line != null)
                {
                    line = line.Trim();
                    Console.Out.WriteLine(line + " " + stemmer.stem(line));
                    line = reader.ReadLine();
                }
                reader.Close();
            }
            catch (Exception _ex)
            {
                Console.Out.WriteLine("Exception while processing term [" + line + "]");
                throw _ex;
                //	UtilSupport.SupportClass.WriteStackTrace(_ex, Console.Error);
            }

            //public static void WriteStackTrace(Exception throwable, TextWriter stream)
            //{
            //	stream.Write(throwable.StackTrace);
            //	stream.Flush();
            //}
        }
        #endregion
    }
}