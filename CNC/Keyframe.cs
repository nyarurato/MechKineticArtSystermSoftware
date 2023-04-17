using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration;


namespace MechKineticsArtSoftware
{
    public class Keyframe
    {
        public enum Action
        {
            NONE = 0,
            MOVETO = 1,
            RAPIDMOVE = 2,
            WAITTIME = 3,
            ENDOFPROGRAM = 255
        };
        
        public int index { get; set; }
        public Action action { get; set; }
        public float wait_time { get; set; }

        public int move_speed { get; set; }
        public float[] unit_motion { get; set; }

        public Keyframe()
        {
        }

        public Keyframe(int motor_count)
        {
            unit_motion = new float[motor_count];
        }

        public void PosArrayCopy(float[] data)
        {
            if(unit_motion == null)
            {
                unit_motion = new float[data.Length];
            }
            Array.Copy(data, unit_motion, data.Length);
            
        }
    }

}
