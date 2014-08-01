using System.Collections.Generic;

namespace Fybr.Server.Objects
{
    public class MultiDictionary<TK, TV>
    {
        private readonly Dictionary<TK, List<TV>> _data;

        public MultiDictionary()
        {
            _data = new Dictionary<TK, List<TV>>();
        }

        public bool Add(TK key, TV value)
        {
            List<TV> list = null;
            var r = _data.TryGetValue(key, out list);
            if (!r)
            {
                list = new List<TV>();
                _data.Add(key, list);
            }
            list.Add(value);
            return !r;
        }

        public List<TV> Get(TK key)
        {
            List<TV> list = null;
            _data.TryGetValue(key, out list);
            return list;
        }

        public bool Remove(TK key)
        {
            return _data.Remove(key);
        }

        public IEnumerable<TK> Keys
        {
            get { return _data.Keys; }
        }

        public IEnumerable<List<TV>> Values
        {
            get { return _data.Values; }
        }

    }
}