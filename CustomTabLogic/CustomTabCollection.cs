using System.Collections.Generic;
using System;
using System.Collections;
using System.Linq;

namespace DecorBlishhudModule.CustomTabLogic
{
    public class CustomTabCollection : ICollection<CustomTab>, IEnumerable<CustomTab>, IEnumerable
    {
        private List<CustomTab> _customTabs = new List<CustomTab>();

        private readonly ICustomTabOwner _owner;

        public int Count => _customTabs.Count;

        public bool IsReadOnly => false;

        public CustomTabCollection(ICustomTabOwner owner)
        {
            _owner = owner;
        }

        public IEnumerator<CustomTab> GetEnumerator()
        {
            return _customTabs.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(CustomTab customTab)
        {
            if (customTab == null)
            {
                throw new ArgumentNullException(nameof(customTab));
            }

            if (customTab.OrderPriority == 0)
            {
                customTab.OrderPriority = _customTabs.Count;
            }

            _customTabs = new List<CustomTab>(
                from t in _customTabs.Concat([customTab])
                orderby t.OrderPriority descending
                select t
            );

            if (_customTabs.Count == 1)
            {
                _owner.SelectedTabGroup1 = customTab;
            }
        }

        public void Clear()
        {
            _customTabs.Clear();
            _owner.SelectedTabGroup1 = null;
        }

        public bool Contains(CustomTab customTab)
        {
            return _customTabs.Contains(customTab);
        }

        public void CopyTo(CustomTab[] array, int arrayIndex)
        {
            _customTabs.CopyTo(array, arrayIndex);
        }

        public bool Remove(CustomTab customTab)
        {
            return _customTabs.Remove(customTab);
        }

        public int IndexOf(CustomTab customTab)
        {
            return _customTabs.IndexOf(customTab);
        }

        public CustomTab FromIndex(int tabIndex)
        {
            if (tabIndex >= 0 && tabIndex < _customTabs.Count)
            {
                return _customTabs[tabIndex];
            }
            return null;
        }
    }
}
