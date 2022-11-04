namespace UraniumCompute.Compiler.Syntax;

public class ReturnStatementSyntax : ExpressionStatementSyntax
{
    //объединить assignment и expression и вернуть значение ?
    public ReturnStatementSyntax(ExpressionSyntax expression) : base(expression)
    {
    }
}