using kOS.Safe.Encapsulation.Suffixes;
using kOS.Safe.Exceptions;
using kOS.Safe.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace kOS.Safe.Encapsulation
{
    public class Lexicon : Structure, IDictionary<Structure, Structure>, IIndexable, IDumper
    {
        public class LexiconComparer<TI> : IEqualityComparer<TI>
        {
            public bool Equals(TI x, TI y)
            {
                if (x == null || y == null)
                {
                    return false;
                }

                if (x.GetType() != y.GetType())
                {
                    return false;
                }

                if ((x is string || x is StringValue) && (y is string || y is StringValue))
                {
                    var compare = string.Compare(x.ToString(), y.ToString(), StringComparison.InvariantCultureIgnoreCase);
                    return compare == 0;
                }

                return x.Equals(y);
            }

            public int GetHashCode(TI obj)
            {
                if (obj is string || obj is StringValue)
                {
                    return obj.ToString().ToLower().GetHashCode();
                }
                return obj.GetHashCode();
            }
        }

        private IDictionary<Structure, Structure> internalDictionary;
        private bool caseSensitive;

        public Lexicon()
        {
            internalDictionary = new Dictionary<Structure, Structure>(new LexiconComparer<Structure>());
            caseSensitive = false;
            InitalizeSuffixes();
        }

        private Lexicon(IEnumerable<KeyValuePair<Structure, Structure>> lexicon)
            : this()
        {
            foreach (var u in lexicon)
            {
                internalDictionary.Add(u);
            }
        }

        private void InitalizeSuffixes()
        {
            AddSuffix("CLEAR", new NoArgsSuffix(Clear, "Removes all items from Lexicon"));
            AddSuffix("KEYS", new Suffix<ListValue<Structure>>(GetKeys, "Returns the lexicon keys"));
            AddSuffix("HASKEY", new OneArgsSuffix<BooleanValue, Structure>(HasKey, "Returns true if a key is in the Lexicon"));
            AddSuffix("HASVALUE", new OneArgsSuffix<BooleanValue, Structure>(HasValue, "Returns true if value is in the Lexicon"));
            AddSuffix("VALUES", new Suffix<ListValue<Structure>>(GetValues, "Returns the lexicon values"));
            AddSuffix("COPY", new NoArgsSuffix<Lexicon>(() => new Lexicon(this), "Returns a copy of Lexicon"));
            AddSuffix("LENGTH", new NoArgsSuffix<ScalarValue>(() => internalDictionary.Count, "Returns the number of elements in the collection"));
            AddSuffix("REMOVE", new OneArgsSuffix<BooleanValue, Structure>(one => Remove(one), "Removes the value at the given key"));
            AddSuffix("ADD", new TwoArgsSuffix<Structure, Structure>(Add, "Adds a new item to the lexicon, will error if the key already exists"));
            AddSuffix("DUMP", new NoArgsSuffix<StringValue>(() => ToString(), "Serializes the collection to a string for printing"));
            AddSuffix(new[] { "CASESENSITIVE", "CASE" }, new SetSuffix<BooleanValue>(() => caseSensitive, SetCaseSensitivity, "Lets you get/set the case sensitivity on the collection, changing sensitivity will clear the collection"));
        }

        private void SetCaseSensitivity(BooleanValue value)
        {
            bool newCase = value.Value;
            if (newCase == caseSensitive)
            {
                return;
            }
            caseSensitive = newCase;

            internalDictionary = newCase ?
                new Dictionary<Structure, Structure>() :
            new Dictionary<Structure, Structure>(new LexiconComparer<Structure>());
        }

        private BooleanValue HasValue(Structure value)
        {
            return internalDictionary.Values.Contains(value);
        }

        private BooleanValue HasKey(Structure key)
        {
            return internalDictionary.ContainsKey(key);
        }

        public ListValue<Structure> GetValues()
        {
            return ListValue.CreateList(Values);
        }

        public ListValue<Structure> GetKeys()
        {
            return ListValue.CreateList(Keys);
        }

        public IEnumerator<KeyValuePair<Structure, Structure>> GetEnumerator()
        {
            return internalDictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<Structure, Structure> item)
        {
            if (internalDictionary.ContainsKey(item.Key))
            {
                throw new KOSDuplicateKeyException(item.Key.ToString(), caseSensitive);
            }
            internalDictionary.Add(item);
        }

        public void Clear()
        {
            internalDictionary.Clear();
        }

        public bool Contains(KeyValuePair<Structure, Structure> item)
        {
            return internalDictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<Structure, Structure>[] array, int arrayIndex)
        {
            internalDictionary.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<Structure, Structure> item)
        {
            return internalDictionary.Remove(item);
        }

        public int Count
        {
            get { return internalDictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return internalDictionary.IsReadOnly; }
        }

        public bool ContainsKey(Structure key)
        {
            return internalDictionary.ContainsKey(key);
        }

        public void Add(Structure key, Structure value)
        {
            if (internalDictionary.ContainsKey(key))
            {
                throw new KOSDuplicateKeyException(key.ToString(), caseSensitive);
            }
            internalDictionary.Add(key, value);
        }

        public bool Remove(Structure key)
        {
            return internalDictionary.Remove(key);
        }

        public bool TryGetValue(Structure key, out Structure value)
        {
            return internalDictionary.TryGetValue(key, out value);
        }

        public Structure this[Structure key]
        {
            get
            {
                if (internalDictionary.ContainsKey(key))
                {
                    return internalDictionary[key];
                }
                throw new KOSKeyNotFoundException(key.ToString(), caseSensitive);
            }
            set
            {
                internalDictionary[key] = value;
            }
        }

        public ICollection<Structure> Keys
        {
            get
            {
                return internalDictionary.Keys;
            }
        }

        public ICollection<Structure> Values
        {
            get
            {
                return internalDictionary.Values;
            }
        }

        public Structure GetIndex(Structure key)
        {
            return internalDictionary[key];
        }

        // Only needed because IIndexable demands it.  For a lexicon, none of the code is
        // actually trying to call this:
        public Structure GetIndex(int index)
        {
            return internalDictionary[FromPrimitiveWithAssert(index)];
        }

        public void SetIndex(Structure index, Structure value)
        {
            internalDictionary[index] = value;
        }
        
        // Only needed because IIndexable demands it.  For a lexicon, none of the code is
        // actually trying to call this:
        public void SetIndex(int index, Structure value)
        {
            internalDictionary[FromPrimitiveWithAssert(index)] = value;
        }

        public override string ToString()
        {
            return new SafeSerializationMgr().ToString(this);
        }

        public Dump Dump()
        {
            var result = new DumpWithHeader
            {
                Header = "LEXICON of " + internalDictionary.Count() + " items:"
            };

            List<object> list = new List<object>();

            foreach (KeyValuePair<Structure, Structure> entry in internalDictionary)
            {
                list.Add(entry.Key);
                list.Add(entry.Value);
            }

            result.Add(kOS.Safe.Dump.Entries, list);

            return result;
        }

        public void LoadDump(Dump dump)
        {
            internalDictionary.Clear();

            List<object> values = (List<object>)dump[kOS.Safe.Dump.Entries];

            for (int i = 0; 2 * i < values.Count; i++)
            {
                internalDictionary.Add(Structure.FromPrimitiveWithAssert(values[2 * i]), Structure.FromPrimitiveWithAssert(values[2 * i + 1]));
            }
        }
    }
}