// 表示括号字符与语法 Kind 的组合结构体

namespace RoslynPad.Roslyn.BraceMatching;

internal readonly struct BraceCharacterAndKind
{
    /// <summary>括号字符，例如 '(' '{'</summary>
    public char Character { get; }

    /// <summary>Roslyn Syntax Token Kind 值</summary>
    public int Kind { get; }

    /// <summary>
    /// 初始化括号信息
    /// </summary>
    public BraceCharacterAndKind(char character, int kind)
        : this()
    {
        Character = character;
        Kind = kind;
    }
}
