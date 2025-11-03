using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.Barefeet
{
    public enum ShapeType
    {
        Heptadecagon = 17,
        Triangle = 3, Square, Pentagon, Hexagon, Heptagon,
        Octagon, Nonagon, Decagon, Hendecagon, Dodecagon,
        Tridecagon, Tetradecagon, Pentadecagon, Hexadecagon,
    }



    public enum ShapeStyle
    {
        BorderAndFill,
        BorderOnly,
        FillOnly,
    }

    public record MediaShape (long Width, long Height);

}
