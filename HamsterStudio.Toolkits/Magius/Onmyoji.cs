using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.Toolkits.Magius
{
    internal static class Onmyoji
    {
        public static double AttackPoint(double value)
        {
            return value / 26.8;
        }

        public static double HealthPoint(double value)
        {
            return (value - 1) / 113.92;
        }

        public static double DefensePoint(double value)
        {
            return value / 4.41;
        }

        public static double HowManyPoint(double attack, double hp, double def)
        {
            return AttackPoint(attack) + HealthPoint(hp) + DefensePoint(def);
        }

        static void Test()
        {
            Console.WriteLine("Hello, World!");

            Console.WriteLine($"天照：{HowManyPoint(3886, 11962.6, 595.35)}");
            Console.WriteLine($"蛇：{HowManyPoint(4074, 12418, 481)}");
            Console.WriteLine($"SP蛇：{HowManyPoint(4153, 13216, 437)}");
            Console.WriteLine($"SP川：{HowManyPoint(3403.6, 11279.08, 383.67)}");
            Console.WriteLine($"SP玉：{HowManyPoint(3510.8, 12532.20, 388.08)}");
            Console.WriteLine($"SP茨：{HowManyPoint(3323.2, 10253.8, 379.26)}");
            Console.WriteLine($"孔雀：{HowManyPoint(3537.6, 11165.16, 441.0)}");
            Console.WriteLine($"季：{HowManyPoint(3564.4, 11165.16, 436.59)}");
        }
    }

}
