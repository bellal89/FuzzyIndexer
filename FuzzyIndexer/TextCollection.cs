using System;
using System.Collections.Generic;
using System.Linq;

namespace FuzzyIndexer
{
    public class TextCollection
    {
        private readonly IEnumerable<Tuple<long, List<string>>> collection;

        public TextCollection(IEnumerable<string> collection)
            : this(collection.Select((text, i) => Tuple.Create((long)(i + 1), text)))
        {}

        public TextCollection(IEnumerable<Tuple<int, string>> collection)
            : this(collection.Select(it => Tuple.Create((long)it.Item1, it.Item2)))
        {}

        public TextCollection(IEnumerable<Tuple<long, string>> collection)
        {
            this.collection = Normalize(collection);
        }

        private IEnumerable<Tuple<long, List<string>>> Normalize(IEnumerable<Tuple<long, string>> rawCollection)
        {
            return rawCollection.Select(idText => Tuple.Create(idText.Item1, idText.Item2.ToLower().StripHTML().Tokenize().ToList()));
        }

        public IEnumerable<string> GetWords()
        {
            return GetDocuments().SelectMany(words => words).Distinct();
        }

        public IEnumerable<List<string>> GetDocuments()
        {
            return collection.Select(it => it.Item2).ToList();
        }

        public List<Tuple<long, List<string>>> GetCollectionByFilter(Func<IEnumerable<string>, IEnumerable<string>> textLevelFilter, Func<string, bool> wordLevelFilter)
        {
            return
                collection.Select(
                    it => Tuple.Create(it.Item1, textLevelFilter(it.Item2).Where(wordLevelFilter).ToList())).ToList();
        }

        public Dictionary<string, int> GetFrequencyDistribution()
        {
            return collection
                .SelectMany(val => val.Item2)
                .GroupBy(val => val,(val, vals) => Tuple.Create(val, vals.Count()))
                .ToDictionary(it => it.Item1, it => it.Item2);
        }
    }
}
