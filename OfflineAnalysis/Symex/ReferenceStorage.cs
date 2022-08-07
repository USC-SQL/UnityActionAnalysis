using System.Collections.Generic;

namespace UnityActionAnalysis
{
    public class ReferenceStorage
    {
        private Dictionary<int, Reference> storage = new Dictionary<int, Reference>();
        private int counter = 1;

        public int Store(Reference r)
        {
            if (r.storageId != null)
            {
                return r.storageId.Value;
            } else
            {
                r.storageId = counter++;
                storage.Add(r.storageId.Value, r);
                return r.storageId.Value;
            }
        }

        public Reference Find(int storageId)
        {
            return storage[storageId];
        }
    }
}
