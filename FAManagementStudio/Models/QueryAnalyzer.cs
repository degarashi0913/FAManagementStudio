using System;
using System.Collections.Generic;

namespace FAManagementStudio.Models
{
    public class QueryAnalyzer
    {
        public static IReadOnlyCollection<string> Analyze(string inputs)
        {
            var endIdx = 0;
            var result = new List<string> { };
            while (endIdx < inputs.Length)
            {
                var startIdx = endIdx;
                var word = GetWord(ref inputs, ref startIdx, out endIdx);
                switch (word)
                {
                    case var x when x == "\r\n":
                        continue;
                    //comment
                    case var x when x.StartsWith("--"):
                        new NewLineToken().StatementEnd(ref inputs, ref endIdx);
                        
                        break;
                    case var x when x.StartsWith("create", StringComparison.OrdinalIgnoreCase) ||
                                    x.StartsWith("recreate", StringComparison.OrdinalIgnoreCase) ||
                                    x.StartsWith("alter", StringComparison.OrdinalIgnoreCase):
                        {
                            var secondIdx = endIdx;
                            var next = GetWord(ref inputs, ref secondIdx, out endIdx);

                            switch (next)
                            {
                                case var y when y.StartsWith("procedure", StringComparison.OrdinalIgnoreCase):
                                    {
                                        new ExecuteBlockToken().StatementEnd(ref inputs, ref endIdx);
                                    }
                                    break;
                                default:
                                    {
                                        new BlockToken().StatementEnd(ref inputs, ref endIdx);
                                        break;
                                    }
                            }
                            break;
                        }
                    case var x when x.StartsWith("execute", StringComparison.OrdinalIgnoreCase):
                        {
                            var secondIdx = endIdx;
                            var next = GetWord(ref inputs, ref secondIdx, out endIdx);
                            //ignore space
                            if (next.StartsWith("block", StringComparison.OrdinalIgnoreCase))
                            {
                                new ExecuteBlockToken().StatementEnd(ref inputs, ref endIdx);
                            }
                            else
                            {
                                new NormalToken().StatementEnd(ref inputs, ref endIdx);
                            }
                            break;
                        }
                    default:
                        new NormalToken().StatementEnd(ref inputs, ref endIdx);
                        break;
                }
                result.Add(inputs[startIdx..endIdx]);
            }
            return [.. result];
        }

        public static string GetWord(ref string statement, ref int startIdx, out int endIdx)
        {
            var startFlg = false;
            endIdx = startIdx;
            while (endIdx < statement.Length)
            {
                var ch = statement[endIdx];
                switch (ch)
                {
                    //ignore space
                    case ' ':
                        if (startFlg) goto end;
                        startIdx++;
                        endIdx++;
                        break;
                    //ignore \r \n
                    case '\r':
                    case '\n':
                        if (startFlg) goto end;
                        else
                        {
                            endIdx++;
                            if (statement[endIdx] == '\n')
                                endIdx++;
                            return "\r\n";
                        }
                    // statement	
                    case '\'':
                        endIdx = statement.IndexOf('\'', endIdx + 1) + 1;
                        if (endIdx < startIdx) throw new ArgumentException("SQL Parse Fail: Single Quotation End is Missiong");
                        goto end;
                    // marks
                    case ',':
                    case '(':
                    case ')':
                    case ';':
                        if (!startFlg) endIdx++;
                        goto end;
                    default:
                        startFlg = true;
                        endIdx++;
                        break;
                }
            }
            return statement.Substring(startIdx);
        end:
            return statement.Substring(startIdx, endIdx - startIdx);
        }

        abstract class BaseToken
        {
            public char Delimiter => ';';
            public abstract void StatementEnd(ref string statement, ref int endIdx);
        }

        class NewLineToken : BaseToken
        {
            public override void StatementEnd(ref string statement, ref int endIdx)
            {
                while (endIdx < statement.Length)
                {
                    var ch = statement[endIdx];
                    switch (ch)
                    {
                        case '\r':
                        case '\n':
                            return;
                        default:
                            endIdx++;
                            break;
                    }
                }
            }
        }

        class NormalToken : BaseToken
        {
            public override void StatementEnd(ref string statement, ref int endIdx)
            {
                var next = statement.IndexOf(Delimiter, endIdx);
                endIdx = 0 < next ? next + 1 : statement.Length;
            }
        }
        class ExecuteBlockToken : BaseToken
        {
            public override void StatementEnd(ref string statement, ref int endIdx)
            {
                var inBlockCount = 0;
                var delimiter = Delimiter.ToString();

                endIdx = statement.IndexOf("begin", endIdx, StringComparison.OrdinalIgnoreCase);

                while (endIdx < statement.Length)
                {
                    var startIdx = endIdx;
                    var word = GetWord(ref statement, ref startIdx, out endIdx);
                    switch (word)
                    {
                        case var x when string.Compare(x, "begin", true) == 0:
                            inBlockCount++;
                            break;
                        case var x when string.Compare(x, "end", true) == 0:
                            inBlockCount--;
                            break;
                        case var x when x == delimiter:
                            if (inBlockCount == 0)
                            {
                                return;
                            }
                            break;
                    }
                }
            }
        }
        class BlockToken : BaseToken
        {
            public override void StatementEnd(ref string statement, ref int endIdx)
            {
                var inBlockCount = 0;
                var delimiter = Delimiter.ToString();
                while (endIdx < statement.Length)
                {
                    var startIdx = endIdx;
                    var word = GetWord(ref statement, ref startIdx, out endIdx);
                    switch (word)
                    {
                        case var x when string.Compare(x, "begin", true) == 0:
                            inBlockCount++;
                            break;
                        case var x when string.Compare(x, "end", true) == 0:
                            inBlockCount--;
                            break;
                        case var x when x == delimiter:
                            if (inBlockCount == 0)
                            {
                                return;
                            }
                            break;
                    }
                }
            }
        }
        class DeclarationsToken : BaseToken
        {
            public override void StatementEnd(ref string statement, ref int endIdx)
            {
                while (endIdx < statement.Length)
                {
                    var startIdx = endIdx;
                    if (string.Compare(GetWord(ref statement, ref startIdx, out endIdx), "begin", true) == 0) return;
                }
            }
        }
    }
}
