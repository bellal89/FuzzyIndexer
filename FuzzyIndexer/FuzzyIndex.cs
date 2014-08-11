using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SpellIt;

namespace FuzzyIndexer
{
	public class FuzzyIndex
	{
		private readonly List<InvertedIndexUnit> index;
		private readonly Dictionary<string, HashSet<long>> termToIds = new Dictionary<string, HashSet<long>>();

	    public FuzzyIndex(TextCollection collection, IEnumerable<string> words)
            : this(collection, words.Select(w => new List<string> { w }).ToList())
        {}

		public FuzzyIndex(TextCollection collection, List<List<string>> words)
		{
		    var wordToId = words.SelectMany((ws, i) => ws.Select(w => Tuple.Create(w, i))).ToDictionary(it => it.Item1,
		                                                                                                it => it.Item2);

		    var stopWords =
		        new HashSet<string>(
		            collection.GetFrequencyDistribution().OrderByDescending(kv => kv.Value).Take(130).Select(kv => kv.Key));
            var idWordsList = collection.GetCollectionByFilter(tokens => tokens.Distinct(),
		                                                       w => !stopWords.Contains(w) && w.Length >= 3);

		    var mispellings = new Mispellings(idWordsList.SelectMany(idWords => idWords.Item2), wordToId.Keys);

            foreach (var idWords in idWordsList)
			{
				foreach (var word in idWords.Item2)
				{
					var dictionaryWord = mispellings.GetFixedOrNull(word);
					if(dictionaryWord == null) continue;
					if (!termToIds.ContainsKey(dictionaryWord))
						termToIds[dictionaryWord] = new HashSet<long>();
					termToIds[dictionaryWord].Add(idWords.Item1);
				}
			}
		    var synJoinedTermToId = termToIds.GroupBy(termId => words[wordToId[termId.Key]].First(),
		                                                (key, vals) => Tuple.Create(key, vals.Aggregate(new HashSet<long>(),
		                                                    (src, kv) => new HashSet<long>(src.Union(kv.Value)))));
			index = synJoinedTermToId.Select(termId => new InvertedIndexUnit(termId.Item1, termId.Item2)).ToList();
		}

		public IEnumerable<InvertedIndexUnit> GetIndex()
		{
			return index;
		}

		private static KeyValuePair<Tuple<string, string>, int> FormatStringParse(string formattedString)
		{
			var q = formattedString.Split('\t');
			return new KeyValuePair<Tuple<string, string>, int>(Tuple.Create(q[0], q[1]), Int32.Parse(q[2]));
		}

		private static string FormatStringWrite(KeyValuePair<Tuple<string, string>, int> unit)
		{
			return unit.Key.Item1 + "\t" + unit.Key.Item2 + "\t" + unit.Value;
		}
	}
}
