using System;
using Strumenta.Sharplasu.Model;

namespace Strumenta.Sharplasu.Transformation
{
    /// <summary>
    /// This is used to indicate that a Node represents some form of placeholders to be used in transformation.
    /// </summary>
    public class PlaceholderAstTransformation : Origin
    {
        public Origin Origin { get; }
        public string Message { get; }

        public PlaceholderAstTransformation(Origin origin, string message)
        {
            Origin = origin;
            Message = message;
        }

        public Position Position
        {
            get => Origin?.Position;
            set => throw new NotImplementedException();
        }
        
        public string SourceText => Origin?.SourceText;

        public Source Source { get; set; }
    }

    /// <summary>
    /// This is used to indicate that we do not know how to transform a certain node.
    /// </summary>
    public class MissingAstTransformation : PlaceholderAstTransformation
    {
        public object TransformationSource { get; }
        public Type ExpectedType { get; }

        public MissingAstTransformation(
            Origin origin,
            object transformationSource,
            Type expectedType = null,
            string message = null
        )
            : base(
                origin,
                message ?? $"Translation of a node is not yet implemented: " +
                          $"{(transformationSource is Node node ? node.SimpleNodeType : transformationSource)}" +
                          $"{(expectedType != null ? $" into {expectedType}" : string.Empty)}"
            )
        {
            TransformationSource = transformationSource;
            ExpectedType = expectedType;
        }

        public MissingAstTransformation(Node transformationSource, Type expectedType = null)
            : this(transformationSource, transformationSource, expectedType)
        {
        }
    }

    /// <summary>
    /// This is used to indicate that, while we had a transformation for a given node, that failed.
    /// This is typically the case because the transformation covers only certain cases and we encountered
    /// one that was not covered.
    /// </summary>
    public class FailingAstTransformation : PlaceholderAstTransformation
    {
        public FailingAstTransformation(Origin origin, string message)
            : base(origin, message)
        {
        }
    }
}