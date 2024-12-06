using System;
using System.Collections.Generic;

namespace Strumenta.Sharplasu.Model
{
    public interface IDestination
    {
    }

    [Serializable]
    public class CompositeDestination : IDestination
    {
        public List<IDestination> Elements { get; }

        public CompositeDestination(IEnumerable<IDestination> elements)
        {
            Elements = new List<IDestination>(elements);
        }

        public CompositeDestination(params IDestination[] elements)
            : this((IEnumerable<IDestination>)elements)
        {
        }
    }

    [Serializable]
    public class TextFileDestination : IDestination
    {
        public Position Position { get; }

        public TextFileDestination(Position position)
        {
            Position = position;
        }
    }
}