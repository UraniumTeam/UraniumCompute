namespace UraniumCompute.Compiler.Syntax;

public abstract class SyntaxNode
{
    internal SyntaxNode? Parent { get; }
    //public string Scope { get; }  //Trivia ?
    internal List<SyntaxNode>? Descendants { get; }

    internal SyntaxTree SyntaxTree { get; }

    protected SyntaxNode(SyntaxNode? parent, List<SyntaxNode> descendants, SyntaxTree syntaxTree)
    {
        Parent = parent;
        Descendants = descendants;
        SyntaxTree = syntaxTree;
    }
    
    protected SyntaxNode(SyntaxNode? parent, SyntaxTree syntaxTree)
    {
        Parent = parent;
        Descendants = null;
        SyntaxTree = syntaxTree;
    }
}