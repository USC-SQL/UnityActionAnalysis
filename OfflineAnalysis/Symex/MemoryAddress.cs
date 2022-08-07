using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.Z3;
using ICSharpCode.Decompiler.TypeSystem;
using System.Diagnostics.CodeAnalysis;

namespace UnityActionAnalysis
{
    public abstract class MemoryAddressComponent : IEquatable<MemoryAddressComponent> {
        public abstract bool Equals(MemoryAddressComponent c);
    }

    public class MemoryAddressField : MemoryAddressComponent
    {
        public readonly IField field;

        public MemoryAddressField(IField field)
        {
            this.field = field;
        }

        public override bool Equals(MemoryAddressComponent c)
        {
            if (!(c is MemoryAddressField))
            {
                return false;
            }

            MemoryAddressField f = (MemoryAddressField)c;
            return field.FullName == f.field.FullName;
        }

        public override string ToString()
        {
            return "[field:" + field.FullName + "]";
        }
    }

    public class MemoryAddressArrayElement : MemoryAddressComponent
    {
        public readonly BitVecExpr index;

        public MemoryAddressArrayElement(BitVecExpr index)
        {
            this.index = index;
        }

        public override bool Equals(MemoryAddressComponent c)
        {
            if (!(c is MemoryAddressArrayElement))
            {
                return false;
            }

            MemoryAddressArrayElement ae = (MemoryAddressArrayElement)c;
            return index.Equals(ae.index);
        }

        public override string ToString()
        {
            return "[element:" + index.ToString() + "]";
        }
    }

    public class MemoryAddressArrayElements : MemoryAddressComponent
    {
        public override bool Equals(MemoryAddressComponent c)
        {
            return c is MemoryAddressArrayElements;
        }

        public override string ToString()
        {
            return "(ArrayElements)";
        }
    }

    public class MemoryAddressArrayLength : MemoryAddressComponent
    {
        public override bool Equals(MemoryAddressComponent c)
        {
            return c is MemoryAddressArrayLength;
        }
        public override string ToString()
        {
            return "(ArrayLength)";
        }

    }

    public class MemoryAddressString : MemoryAddressComponent
    {
        public override bool Equals(MemoryAddressComponent c)
        {
            return c is MemoryAddressString;
        }

        public override string ToString()
        {
            return "(String)";
        }
    }

    public class MemoryAddressMemberToken : MemoryAddressComponent
    {
        public override bool Equals(MemoryAddressComponent c)
        {
            return c is MemoryAddressMemberToken;
        }

        public override string ToString()
        {
            return "(MemberToken)";
        }
    }

    public class MemoryAddress : IEquatable<MemoryAddress>
    {
        public readonly bool heap;
        public readonly string root;
        public readonly List<MemoryAddressComponent> components;

        public MemoryAddress(bool heap, string root, List<MemoryAddressComponent> components)
        {
            this.heap = heap;
            this.root = root;
            this.components = components;
        }

        public MemoryAddress(bool heap, string root)
        {
            this.heap = heap;
            this.root = root;
            components = new List<MemoryAddressComponent>();
        }

        public MemoryAddress WithComponent(MemoryAddressComponent component)
        {
            List<MemoryAddressComponent> newComponents = new List<MemoryAddressComponent>(components);
            newComponents.Add(component);
            return new MemoryAddress(heap, root, newComponents);
        }

        public bool Equals(MemoryAddress other)
        {
            if (root != other.root)
            {
                return false;
            }

            if (components.Count != other.components.Count)
            {
                return false;
            }

            for (int i = 0, n = components.Count; i < n; ++i)
            {
                MemoryAddressComponent c1 = components[i];
                MemoryAddressComponent c2 = other.components[i];
                if (!c1.Equals(c2))
                {
                    return false;
                }
            }

            return true;
        }

        public override string ToString()
        {
            string result = root;
            foreach (MemoryAddressComponent c in components)
            {
                result += "." + c;
            }
            return result;
        }
    }
}
