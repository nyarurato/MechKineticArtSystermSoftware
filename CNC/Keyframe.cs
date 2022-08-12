using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MechKineticsArtSoftware
{
    public class Keyframe
    {
        public enum Action
        {
            NONE,
            MOVETO,
            RAPIDMOVE,
            WAITTIME,
            ENDOFPROGRAM
        };

        public int index { get; set; }
        public Action action { get; set; }
        public float wait_time { get; set; }

        public int move_time { get; set; }
        public float[][] unit_motion { get; set; }

        public Keyframe()
        {
            unit_motion = new float[][]
            {
                /*
                new float[Form1.motor_num_each_unit],
                new float[Form1.motor_num_each_unit],
                new float[Form1.motor_num_each_unit],
                new float[Form1.motor_num_each_unit],
                new float[Form1.motor_num_each_unit],
                new float[Form1.motor_num_each_unit],
                new float[Form1.motor_num_each_unit],
                new float[Form1.motor_num_each_unit]
                */
            };
        }

        public void ArrayCopy(float[][] data)
        {
            for (int i = 0; i < unit_motion.Length; i++)
            {
                Array.Copy(data[i], unit_motion[i], data[i].Length);
            }
        }
    }

}
