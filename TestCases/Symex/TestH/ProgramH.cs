using System;
using System.Collections.Generic;
using System.Text;

namespace TestCases.Symex.TestH
{
    public enum RunMode : ulong
    {
        MODE_ADD = 1000000000UL,
        MODE_SUB = 2000000000UL,
        MODE_MUL = 3000000000UL,
        MODE_DIV = 4000000000UL,
        MODE_REM = 5000000000UL,
        MODE_AND = 6000000000UL,
        MODE_OR = 7000000000UL,
        MODE_XOR = 8000000000UL,
        MODE_SHL = 9000000000UL,
        MODE_SHR = 10000000000UL
    }

    public struct ProgramConfig
    {
        public RunMode runMode;
    }

    public class ProgramH
    {
        private static bool CheckAdd(int[] operands)
        {
            return operands[0] + operands[1] == operands[2];
        }

        private static bool CheckSub(int[] operands)
        {
            return operands[0] - operands[1] == operands[2];
        }

        private static bool CheckMul(int[] operands)
        {
            return operands[0] * operands[1] == operands[2];
        }

        private static bool CheckDiv(int[] operands)
        {
            return operands[0] / operands[1] == operands[2];
        }

        private static bool CheckRem(int[] operands)
        {
            return operands[0] % operands[1] == operands[2];
        }

        private static bool CheckAnd(int[] operands)
        {
            return (operands[0] & operands[1]) == operands[2];
        }

        private static bool CheckOr(int[] operands)
        {
            return (operands[0] | operands[1]) == operands[2];
        }

        private static bool CheckXor(int[] operands)
        {
            return (operands[0] ^ operands[1]) == operands[2];
        }

        private static bool CheckShl(int[] operands)
        {
            return (operands[0] << operands[1]) == operands[2];
        }

        private static bool CheckShr(int[] operands)
        {
            return (operands[0] >> operands[1]) == operands[2];
        }

        public void Main(ProgramConfig config, int x, int y, int z)
        {
            int[] operands = new int[3];
            operands[0] = x;
            operands[1] = y;
            operands[2] = z;

            if (config.runMode == RunMode.MODE_ADD)
            {
                if (CheckAdd(operands))
                {
                    Console.WriteLine("add success");
                } else
                {
                    throw new Exception("expected add");
                }
            } else if (config.runMode == RunMode.MODE_SUB)
            {
                if (CheckSub(operands))
                {
                    Console.WriteLine("sub success");
                }
                else
                {
                    throw new Exception("expected sub");
                }
            } else if (config.runMode == RunMode.MODE_MUL)
            {
                if (CheckMul(operands))
                {
                    Console.WriteLine("mul success");
                }
                else
                {
                    throw new Exception("expected mul");
                }
            } else if (config.runMode == RunMode.MODE_DIV)
            {
                if (CheckDiv(operands))
                {
                    Console.WriteLine("div success");
                }
                else
                {
                    throw new Exception("expected div");
                }
            } else if (config.runMode == RunMode.MODE_REM)
            {
                if (CheckRem(operands))
                {
                    Console.WriteLine("rem success");
                }
                else
                {
                    throw new Exception("expected rem");
                }
            } else if (config.runMode == RunMode.MODE_AND)
            {
                if (CheckAnd(operands))
                {
                    Console.WriteLine("and success");
                }
                else
                {
                    throw new Exception("expected and");
                }
            } else if (config.runMode == RunMode.MODE_OR)
            {
                if (CheckOr(operands))
                {
                    Console.WriteLine("or success");
                }
                else
                {
                    throw new Exception("expected or");
                }
            } else if (config.runMode == RunMode.MODE_XOR)
            {
                if (CheckXor(operands))
                {
                    Console.WriteLine("xor success");
                }
                else
                {
                    throw new Exception("expected xor");
                }
            } else if (config.runMode == RunMode.MODE_SHL)
            {
                if (CheckShl(operands))
                {
                    Console.WriteLine("shl success");
                }
                else
                {
                    throw new Exception("expected shl");
                }
            } else if (config.runMode == RunMode.MODE_SHR)
            {
                if (CheckShr(operands))
                {
                    Console.WriteLine("shr success");
                }
                else
                {
                    throw new Exception("expected shr");
                }
            } else
            {
                throw new Exception("unknown mode");
            }
        }
    }
}
