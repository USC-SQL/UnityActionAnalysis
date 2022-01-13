using System;
using System.Collections.Generic;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.IL;
using Microsoft.Z3;

namespace UnitySymexCrawler
{
    public class FrameStackElement
    {
        public readonly Queue<Operation> opQueue;
        public readonly int frameID;

        public FrameStackElement(Queue<Operation> opQueue, int frameID)
        {
            this.opQueue = opQueue;
            this.frameID = frameID;
        }
    }

    public enum ExecutionStatus
    {
        ACTIVE,
        ABORTED,
        HALTED
    }

    public class SymexState 
    {
        public Dictionary<string, Expr> mem;
        public Dictionary<string, Dictionary<string, Expr>> objects;
        public Dictionary<int, SymbolicMethodCall> symbolicMethodCalls;
        public Stack<FrameStackElement> frameStack;
        public Queue<Operation> opQueue;
        public List<BoolExpr> pathCondition;
        public ExecutionStatus execStatus;

        public int frameID;

        public int frameCounter;
        public int tempVarCounter;
        public Dictionary<string, int> heapCounters;
        public int symbolicMethodCounter;

        private Context z3;

        public SymexState(Context z3)
        {
            mem = new Dictionary<string, Expr>();
            objects = new Dictionary<string, Dictionary<string, Expr>>();
            symbolicMethodCalls = new Dictionary<int, SymbolicMethodCall>();
            frameStack = new Stack<FrameStackElement>();
            opQueue = new Queue<Operation>();
            pathCondition = new List<BoolExpr>();
            execStatus = ExecutionStatus.ACTIVE;

            frameID = 0;
            frameCounter = 1;
            tempVarCounter = 0;
            heapCounters = new Dictionary<string, int>();
            symbolicMethodCounter = 0;

            this.z3 = z3;
        }

        public SymexState(SymexState o)
        {
            mem = new Dictionary<string, Expr>(o.mem);
            objects = new Dictionary<string, Dictionary<string, Expr>>();
            foreach (var p in o.objects)
            {
                objects[p.Key] = new Dictionary<string, Expr>(p.Value);
            }
            symbolicMethodCalls = new Dictionary<int, SymbolicMethodCall>(o.symbolicMethodCalls);

            frameStack = new Stack<FrameStackElement>(o.frameStack.Count);
            foreach (FrameStackElement fse in o.frameStack)
            {
                frameStack.Push(new FrameStackElement(new Queue<Operation>(fse.opQueue), fse.frameID));
            }

            opQueue = new Queue<Operation>(o.opQueue);
            pathCondition = new List<BoolExpr>(o.pathCondition);
            execStatus = o.execStatus;

            frameID = o.frameID;
            frameCounter = o.frameCounter;
            tempVarCounter = o.tempVarCounter;
            heapCounters = new Dictionary<string, int>(o.heapCounters);
            symbolicMethodCounter = o.symbolicMethodCounter;

            z3 = o.z3; 
        }

        public void MemoryWrite(MemoryAddress address, Expr value)
        {
            if (address.heap)
            {
                var obj = objects[address.root];
                var c = address.components[0];
                if (c is MemoryAddressField)
                {
                    var f = (MemoryAddressField)c;
                    if (address.components.Count == 1)
                    {
                        obj[f.field.Name] = value;
                    }
                    else
                    {
                        obj[f.field.Name] = MemoryTransform(obj[f.field.Name], address.components, 1, value);
                    }
                }
                else if (c is MemoryAddressArrayElements)
                {
                    if (address.components.Count > 1)
                    {
                        throw new ArgumentException("can only have one component in address with MemoryAddressArrayElements");
                    }
                    obj["_elems"] = value;
                }
                else if (c is MemoryAddressArrayLength)
                {
                    if (address.components.Count > 1)
                    {
                        throw new ArgumentException("can only have one component in address with MemoryAddressArrayLength");
                    }
                    obj["_length"] = value;
                } else if (c is MemoryAddressString)
                {
                    if (address.components.Count > 1)
                    {
                        throw new ArgumentException("can only have one component in address with MemoryAddressString");
                    }
                    obj["_string"] = value;
                } else if (c is MemoryAddressMemberToken)
                {
                    if (address.components.Count > 1)
                    {
                        throw new ArgumentException("can only have one component in address with MemoryAddressMemberToken");
                    }
                    obj["_memberToken"] = value;
                }
                else
                {
                    var ae = (MemoryAddressArrayElement)c;
                    if (address.components.Count == 1)
                    {
                        obj["_elems"] = z3.MkStore((ArrayExpr)obj["_elems"], ae.index, value);
                    }
                    else
                    {
                        var elem = z3.MkSelect((ArrayExpr)obj["_elems"], ae.index).Simplify();
                        elem = MemoryTransform(elem, address.components, 1, value);
                        obj["_elems"] = z3.MkStore((ArrayExpr)obj["_elems"], ae.index, elem);
                    }
                }
            }
            else
            {
                if (address.components.Count == 0)
                {
                    mem[address.root] = value;
                } else
                {
                    mem[address.root] = MemoryTransform(mem[address.root], address.components, 0, value);
                }
            }
        }

        public Expr MemoryRead(MemoryAddress address, IType type)
        {
            if (address.heap)
            {
                var obj = objects[address.root];
                var c = address.components[0];
                Expr value;
                if (c is MemoryAddressField)
                {
                    MemoryAddressField f = (MemoryAddressField)c;
                    if (!obj.ContainsKey(f.field.Name))
                    {
                        obj[f.field.Name] = MakeSymbolicValue(f.field.Type, address.root + ":instancefield:" + f.field.Name);
                    }
                    value = obj[f.field.Name];
                } else if (c is MemoryAddressString)
                {
                    Expr str;
                    if (!obj.TryGetValue("_string", out str))
                    {
                        str = z3.MkConst(address.root + ":string", z3.StringSort);
                        obj["_string"] = str;
                    }
                    value = str;
                } else if (c is MemoryAddressMemberToken)
                {
                    Expr m;
                    if (!obj.TryGetValue("_memberToken", out m))
                    {
                        throw new NotSupportedException("attempted to read uninitialized member token");
                    }
                    value = m;
                } else
                {
                    IType elementType = type;
                    Expr elems;
                    Expr length;

                    if (!obj.TryGetValue("_elems", out elems))
                    {
                        elems = z3.MkConst(address.root + ":elems",
                            z3.MkArraySort(z3.MkBitVecSort(32), SymexMachine.Instance.SortPool.TypeToSort(elementType)));
                        obj["_elems"] = elems;
                    }
                    if (!obj.TryGetValue("_length", out length))
                    {
                        length = z3.MkConst(address.root + ":length", z3.MkBitVecSort(32));
                        obj["_length"] = length;
                    }

                    if (c is MemoryAddressArrayElements)
                    {
                        value = elems;
                    }
                    else if (c is MemoryAddressArrayLength)
                    {
                        value = length;
                    }
                    else
                    {
                        var ae = (MemoryAddressArrayElement)c;
                        value = z3.MkSelect((ArrayExpr)elems, ae.index).Simplify();
                    }
                }

                if (address.components.Count > 1)
                {
                    for (int i = 1, n = address.components.Count; i < n; ++i)
                    {
                        var dsort = (DatatypeSort)value.Sort;
                        var f = (MemoryAddressField)address.components[i];
                        FuncDecl accessor = Helpers.FindFieldAccessor(dsort, f.field);
                        value = accessor.Apply(value).Simplify();
                    }
                }

                return value;
            } else
            {
                Expr value;
                if (!mem.TryGetValue(address.root, out value))
                {
                    if (address.components.Count > 0)
                    {
                        throw new Exception("expected to find value at address when components.Count > 0");
                    }
                    value = MakeSymbolicValue(type, address.root);
                    mem[address.root] = value;
                }
                if (address.components.Count > 0)
                {
                    foreach (MemoryAddressComponent c in address.components)
                    {
                        var dsort = (DatatypeSort)value.Sort;
                        MemoryAddressField f = (MemoryAddressField)c;
                        FuncDecl accessor = Helpers.FindFieldAccessor(dsort, f.field);
                        value = accessor.Apply(value).Simplify();
                    }
                }
                return value;
            }
        }

        public Expr MakeSymbolicValue(IType type, string name)
        {
            if (type.Kind == TypeKind.Class || type.Kind == TypeKind.Interface)
            {
                MemoryAddress address = HeapAllocate(name);
                Reference r = new Reference(type, address);
                return r.ToExpr();
            } else if (type.Kind == TypeKind.Array)
            {
                MemoryAddress address = HeapAllocate(name);
                MemoryAddress elemsAddress = address.WithComponent(new MemoryAddressArrayElements());
                MemoryAddress lenAddress = address.WithComponent(new MemoryAddressArrayLength());
                ArrayType arrType = (ArrayType)type;
                MemoryWrite(elemsAddress,
                    z3.MkConst(name + ":elems",
                        z3.MkArraySort(z3.MkBitVecSort(32), SymexMachine.Instance.SortPool.TypeToSort(arrType.ElementType))));
                MemoryWrite(lenAddress,
                    z3.MkConst(name + ":length", z3.MkBitVecSort(32)));
                Reference r = new Reference(type, address);
                return r.ToExpr();
            } else if (type.Kind == TypeKind.Struct || type.Kind == TypeKind.Enum)
            {
                var sort = SymexMachine.Instance.SortPool.TypeToSort(type);
                if (sort is DatatypeSort) // struct with fields
                {
                    DatatypeSort dsort = (DatatypeSort)sort;
                    List<Expr> elems = new List<Expr>();
                    foreach (IField field in Helpers.GetInstanceFields(type))
                    {
                        elems.Add(MakeSymbolicValue(field.Type, name + ":instancefield:" + field.Name));
                    }
                    FuncDecl ctor = dsort.Constructors[0];
                    return ctor.Apply(elems.ToArray());
                } else
                {
                    return z3.MkConst(name, sort);
                }
            } else
            {
                throw new NotImplementedException("MakeSymbolicValue for type of kind " + type.Kind + " not implemented");
            }
        }

        public MemoryAddress HeapAllocate(string name)
        {
            int heapId;
            if (!heapCounters.TryGetValue(name, out heapId))
            {
                heapId = 0;
            }
            MemoryAddress address = new MemoryAddress(true, name + (heapId > 0 ? ":heapid:" + heapId : ""));
            objects[address.root] = new Dictionary<string, Expr>();
            heapCounters[name] = heapId + 1;
            return address;
        }

        public int NextFrameID()
        {
            return frameCounter;
        }

        public void NewFrame()
        {
            frameStack.Push(new FrameStackElement(opQueue, frameID));
            opQueue = new Queue<Operation>();
            frameID = frameCounter++;
        }

        public void ExitFrame()
        {
            if (frameStack.Count == 0)
            {
                if (execStatus != ExecutionStatus.ACTIVE)
                {
                    throw new Exception("unexpected ExitFrame() in inactive state");
                }
                execStatus = ExecutionStatus.HALTED;
            } else
            {
                FrameStackElement fse = frameStack.Pop();
                opQueue = fse.opQueue;
                frameID = fse.frameID;
            }
        }

        public void Fork(InstructionPointer IP, BoolExpr conditionToAdd, ILInstruction sourceInstruction)
        {
            SymexState s = new SymexState(this);
            s.pathCondition.Add(conditionToAdd);
            s.opQueue.Enqueue(new Operations.Fetch(IP, sourceInstruction));
            SymexMachine.Instance.ScheduleAddState(s);
        }

        public string PathConditionString()
        {
            Solver s = z3.MkSolver();
            foreach (BoolExpr cond in pathCondition)
            {
                s.Assert(cond);
            }
            Helpers.AssertAssumptions(s, z3);
            string result = s.ToString();
            s.Dispose();
            return result;
        }

        public const int TYPE_STRCONST = 1;
        public const int TYPE_OBJECT = 2;
        public const int TYPE_BVCONST = 3;
        public const int TYPE_Z3EXPR = 4;

        public object SerializeExpr(Expr expr)
        {
            if (expr.Sort is IntSort)
            {
                Reference r = Reference.FromExpr(expr);
                if (r.address == null)
                {
                    return new
                    {
                        type = TYPE_OBJECT,
                        value = (object)null
                    };
                } else if (r.address.heap && r.address.components.Count == 0)
                {
                    Dictionary<string, Expr> obj = objects[r.address.root];
                    if (obj.TryGetValue("_string", out Expr strExpr) && strExpr.Sort == z3.StringSort && strExpr.FuncDecl.DeclKind == Z3_decl_kind.Z3_OP_INTERNAL)
                    {
                        var str = strExpr.ToString();
                        return new
                        {
                            type = TYPE_STRCONST,
                            value = str.Substring(1, str.Length - 2)
                        };
                    } else
                    {
                        Dictionary<string, object> result = new Dictionary<string, object>();
                        result["_address"] = new
                        {
                            type = TYPE_STRCONST,
                            value = r.address.ToString()
                        };
                        foreach (var p in obj)
                        {
                            result[p.Key] = SerializeExpr(p.Value);
                        }
                        return new
                        {
                            type = TYPE_OBJECT,
                            value = result
                        };
                    }
                }
                else
                {
                    return SerializeExpr(MemoryRead(r.address, r.type));
                }
            } 
            else if (expr.Sort is BitVecSort && expr.FuncDecl.DeclKind == Z3_decl_kind.Z3_OP_BNUM)
            {
                return new
                {
                    type = TYPE_BVCONST,
                    value = ulong.Parse(expr.ToString())
                };
            } 
            else
            {
                using (Solver s = z3.MkSolver())
                {
                    s.Assert(z3.MkEq(expr, z3.MkConst("p", expr.Sort)));
                    return new
                    {
                        type = TYPE_Z3EXPR,
                        value = s.ToString()
                    };
                }
            }
        }

        private Expr MemoryTransform(Expr input, List<MemoryAddressComponent> components, int cindex, Expr value)
        {
            if (cindex >= components.Count)
            {
                return value;
            }

            DatatypeSort dsort = (DatatypeSort)input.Sort;
            MemoryAddressField f = (MemoryAddressField)components[cindex];
            FuncDecl accessor = Helpers.FindFieldAccessor(dsort, f.field);
            Expr elem = MemoryTransform(accessor.Apply(input).Simplify(), components, cindex + 1, value);
            return UpdateField(input, f.field, elem);
        }

        private Expr UpdateField(Expr input, IField field, Expr elem)
        {
            IType declaringType = field.DeclaringType;
            DatatypeSort dsort = (DatatypeSort)SymexMachine.Instance.SortPool.TypeToSort(declaringType);
            FuncDecl ctor = dsort.Constructors[0];
            FuncDecl[] accessors = dsort.Accessors[0];
            List<Expr> elems = new List<Expr>();
            int index = 0;
            foreach (IField dtField in declaringType.GetFields(f => !f.IsStatic))
            {
                if (dtField.FullName == field.FullName)
                {
                    elems.Add(elem);
                }
                else
                {
                    elems.Add(accessors[index].Apply(input).Simplify());
                }
                ++index;
            }
            return ctor.Apply(elems.ToArray());
        }
    }
}
