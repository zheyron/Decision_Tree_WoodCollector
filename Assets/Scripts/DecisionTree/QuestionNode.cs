using System;

public class QuestionNode : ITreeNode
{
    private ITreeNode trueNode;
    private ITreeNode falseNode;
    private Func<bool> question;

    public QuestionNode(Func<bool> question, ITreeNode trueNode, ITreeNode falseNode)
    {
        this.question = question;
        this.trueNode = trueNode;
        this.falseNode = falseNode;
    }

    public void Execute()
    {
        if (question.Invoke())
            trueNode.Execute();
        else
            falseNode.Execute();
    }
}