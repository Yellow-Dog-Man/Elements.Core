using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elements.Data;

namespace Elements.Core
{
    /// <summary>
    /// Indicates positioning in one of the corners of a square
    /// </summary>
    [DataModelType]
    public enum Corner
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }
}
