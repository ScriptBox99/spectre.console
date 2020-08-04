using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spectre.Console.Composition;

namespace Spectre.Console
{
    /// <summary>
    /// Represents a panel which contains another renderable item.
    /// </summary>
    public sealed class Panel : IRenderable
    {
        private readonly IRenderable _child;
        private readonly bool _fit;
        private readonly Justify _content;

        /// <summary>
        /// Initializes a new instance of the <see cref="Panel"/> class.
        /// </summary>
        /// <param name="child">The child.</param>
        /// <param name="fit">Whether or not to fit the panel to it's parent.</param>
        /// <param name="content">The justification of the panel content.</param>
        public Panel(IRenderable child, bool fit = false, Justify content = Justify.Left)
        {
            _child = child;
            _fit = fit;
            _content = content;
        }

        /// <inheritdoc/>
        public int Measure(Encoding encoding, int maxWidth)
        {
            var childWidth = _child.Measure(encoding, maxWidth);
            return childWidth + 4;
        }

        /// <inheritdoc/>
        public IEnumerable<Segment> Render(Encoding encoding, int width)
        {
            var childWidth = width - 4;
            if (!_fit)
            {
                childWidth = _child.Measure(encoding, width - 2);
            }

            var result = new List<Segment>();
            var panelWidth = childWidth + 2;

            result.Add(new Segment("┌"));
            result.Add(new Segment(new string('─', panelWidth)));
            result.Add(new Segment("┐"));
            result.Add(new Segment("\n"));

            // Render the child.
            var childSegments = _child.Render(encoding, childWidth);

            // Split the child segments into lines.
            var lines = Segment.SplitLines(childSegments, childWidth);
            foreach (var line in lines)
            {
                result.Add(new Segment("│ "));

                var content = new List<Segment>();

                var length = line.Sum(segment => segment.CellLength(encoding));
                if (length < childWidth)
                {
                    if (_content == Justify.Right)
                    {
                        var diff = childWidth - length;
                        content.Add(new Segment(new string(' ', diff)));
                    }
                    else if (_content == Justify.Center)
                    {
                        var diff = (childWidth - length) / 2;
                        content.Add(new Segment(new string(' ', diff)));
                    }
                }

                foreach (var segment in line)
                {
                    content.Add(segment.StripLineEndings());
                }

                if (length < childWidth)
                {
                    if (_content == Justify.Left)
                    {
                        var diff = childWidth - length;
                        content.Add(new Segment(new string(' ', diff)));
                    }
                    else if (_content == Justify.Center)
                    {
                        var diff = (childWidth - length) / 2;
                        content.Add(new Segment(new string(' ', diff)));

                        var remainder = (childWidth - length) % 2;
                        if (remainder != 0)
                        {
                            content.Add(new Segment(new string(' ', remainder)));
                        }
                    }
                }

                result.AddRange(content);

                result.Add(new Segment(" │"));
                result.Add(new Segment("\n"));
            }

            result.Add(new Segment("└"));
            result.Add(new Segment(new string('─', panelWidth)));
            result.Add(new Segment("┘"));
            result.Add(new Segment("\n"));

            return result;
        }
    }
}