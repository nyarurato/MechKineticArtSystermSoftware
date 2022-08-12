using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MechKineticsArtSoftware
{
    class KeyFrameCAM
    {
        int board_index;
        WebAPIDatas apidatas;
        LogWriter logger;

        const string setup_commands = "";
        const string end_commands = "";

        const string loop_commands = "while true\n";
        const string loop_indent = "  ";

        const int motor_num_each_unit = 3;

        public KeyFrameCAM(int board_i, WebAPIDatas webAPIDatas, LogWriter logwriter)
        {
            board_index = board_i;
            apidatas = webAPIDatas;
            logger = logwriter;


        }

        public bool MakeNCFile(string save_path, List<Keyframe> keyframes, bool is_active_loop = false)
        {
            string nc_program = "";

            nc_program += setup_commands;

            if (is_active_loop)
            {
                nc_program += loop_commands;
            }

            var same_board_unit_list = apidatas.relation_list.Where(x => x.unit_board == apidatas.api_list[board_index]);

            float[] pre_vector = new float[same_board_unit_list.Count() * motor_num_each_unit];
            float[] vec = new float[same_board_unit_list.Count() * motor_num_each_unit];

            foreach (Keyframe kf in keyframes)
            {
                string block_word = "";

                if (is_active_loop)
                {
                    block_word += loop_indent;
                }

                if (kf.action == Keyframe.Action.NONE)
                {
                    continue;
                }
                else if (kf.action == Keyframe.Action.MOVETO)
                {

                    block_word += $"G1";

                    int index = 0;
                    foreach (var unit_motor in same_board_unit_list)
                    {
                        block_word += $" {unit_motor.axis_each_name[0]}{kf.unit_motion[unit_motor.unit_index][0]} {unit_motor.axis_each_name[1]}{kf.unit_motion[unit_motor.unit_index][1]} {unit_motor.axis_each_name[2]}{kf.unit_motion[unit_motor.unit_index][2]}";

                        vec[index] = kf.unit_motion[unit_motor.unit_index][0];
                        vec[index + 1] = kf.unit_motion[unit_motor.unit_index][1];
                        vec[index + 2] = kf.unit_motion[unit_motor.unit_index][2];

                        index += motor_num_each_unit;

                    }

                    double length = 0;
                    for (int j = 0; j < vec.Length; j++)
                    {
                        length += Math.Pow((vec[j] - pre_vector[j]), 2);
                    }
                    length = Math.Sqrt(length);

                    int feed_rate = (int)Math.Abs(Math.Round(length / kf.move_time * 60));
                    block_word += $" F{feed_rate}\n";

                    vec.CopyTo(pre_vector, 0);

                    if (feed_rate == 0)
                    {
                        block_word = $"G4 S{kf.move_time.ToString("0.##")}\n";
                    }


                }
                else if (kf.action == Keyframe.Action.RAPIDMOVE)
                {
                    block_word += $"G0";


                    foreach (var unit_motor in same_board_unit_list)
                    {
                        block_word += $" {unit_motor.axis_each_name[0]}{kf.unit_motion[unit_motor.unit_index][0]} {unit_motor.axis_each_name[1]}{kf.unit_motion[unit_motor.unit_index][1]} {unit_motor.axis_each_name[2]}{kf.unit_motion[unit_motor.unit_index][2]}";
                    }
                    block_word += $"\n";

                }
                else if (kf.action == Keyframe.Action.WAITTIME)
                {
                    block_word += $"G4 S{kf.wait_time.ToString("0.##")}\n";

                }
                else if (kf.action == Keyframe.Action.ENDOFPROGRAM)
                {
                    break;
                }

                nc_program += block_word;
            }

            nc_program += end_commands;

            try
            {
                File.WriteAllText(save_path, nc_program, Encoding.UTF8);
                logger.writeLogln($"Board:{board_index} Created & Saved NC File. Path : {save_path}");
            }
            catch (Exception e)
            {
                logger.writeLogln($"Board:{board_index} NC file output error : {e.Message}");
                return false;
            }

            return true;

        }
    }
}
