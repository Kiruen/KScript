using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KScript.AST
{
    public class TestNative
    {
        public class Enemy
        {
            public int Level { get; set; }
            public Enemy()
            {
                Level = 1022;
            }

            public void Attack(double harm)
            {
                MessageBox.Show(GetHashCode() + "has given " + harm + " harm");
            }
        }

        public class Monster
        {
            public int Hp { get; set; }
            public Monster()
            {
                Hp = 213002;
            }

            public void Retreat(double distance)
            {
                MessageBox.Show(GetHashCode() + "has been retreated by" + distance);
            }
        }
    }
}
