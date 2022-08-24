#define USE_FEEDRATE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MechKineticsArtSoftware
{
    //Make this object by each Board
    class KeyFrameCAM
    {
        int board_index;
        WebAPIManage apidatas;
        LogWriter logger;

        const string setup_commands = "";
        const string end_commands = "";

        const string loop_commands = "while true\n";
        const string loop_indent = "  ";

        public KeyFrameCAM(int board_i, WebAPIManage webAPIDatas, LogWriter logwriter)
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

            ParentBoard pboard = apidatas.GetBoardFromIndex(board_index);

            List<Motor> motor_list_on_same_board = apidatas.motor_list.Where(x => (x.unit_board.parent == pboard)).ToList();

            int motor_num_on_board = motor_list_on_same_board.Count;

            float[] pre_vector = new float[motor_num_on_board];
            float[] vec = new float[motor_num_on_board];

            Keyframe previous = null;

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

                    

                    block_word += MakeAxisPostionOrder(previous,kf,motor_list_on_same_board);

                    //calc late similar move time feedrate between other board group

#if USE_MOVETIME
                    //It will support inverse feedrate moving in RRF3.5

                    double length = 0;
                    for (int j = 0; j < vec.Length; j++)
                    {
                        length += Math.Pow((vec[j] - pre_vector[j]), 2);
                    }
                    length = Math.Sqrt(length);
                    

                    int feed_rate = (int)Math.Abs(Math.Round(length / kf.move_time * 60));
#else

                    int feed_rate = kf.move_speed;
#endif
                    block_word += $" F{feed_rate}\n";

                    vec.CopyTo(pre_vector, 0);

#if USE_MOVETIME
                    //It will support inverse feedrate moving in RRF3.5
                    if (feed_rate == 0)
                    {
                        block_word = $"G4 S{kf.move_time.ToString("0.##")}\n";
                    }
                    */
#endif

                }
                else if (kf.action == Keyframe.Action.RAPIDMOVE)
                {
                    block_word += $"G0";


                    block_word += MakeAxisPostionOrder(previous,kf,motor_list_on_same_board);


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
                previous = kf;
            }

            nc_program += end_commands;

            try
            {
                File.WriteAllText(save_path, nc_program, Encoding.UTF8);
                logger.WriteLogln($"Board:{board_index} Created & Saved NC File. Path : {save_path}");
            }
            catch (Exception e)
            {
                logger.WriteLogln($"Board:{board_index} NC file output error : {e.Message}");
                return false;
            }
            
            return true;
            
        }


        string MakeAxisPostionOrder(Keyframe previous,Keyframe kf,List<Motor> motor_list_on_same_board)
        {
            string position_order = "";
            List<Motor> e_axis;
            bool is_exist_e_axis = false;

            foreach (var motor in motor_list_on_same_board)
            {
                if (motor.axis_name.Contains("E"))
                {
                    is_exist_e_axis = true;   
                }
                else
                {
                    position_order += $" {motor.axis_name}{kf.unit_motion[motor.unit_index]}";
                }

            }
            //ADD E1 E2 E3.... -> Exx:xx:xx:xx
            if (is_exist_e_axis)
            {
                e_axis = motor_list_on_same_board.FindAll(x => (x.is_e_axis_motor));
                position_order += MakeAxisPositionOrderForEaxis(previous,kf, e_axis);
            }


            return position_order;
        }

        string MakeAxisPositionOrderForEaxis(Keyframe previous,Keyframe kf,List<Motor> eaxis_motor_list_on_same_board)
        {
            string position_order = " E";

            eaxis_motor_list_on_same_board.OrderBy(x => (x.axis_name));//E0,E1...E4,E5...
            bool is_first_flag = true;

            float[] prev_position;

            if (previous == null)
            {
                prev_position = new float[kf.unit_motion.Length];
                Array.Fill(prev_position, 0);
            }
            else
            {
                prev_position = previous.unit_motion;
            }


            foreach(Motor emotor in eaxis_motor_list_on_same_board)
            {
                

                position_order += $"{(is_first_flag?string.Empty:':')}" + $"{emotor.CalcRelativePos(kf.unit_motion[emotor.unit_index],prev_position[emotor.unit_index])}";
                is_first_flag = false;
            }

            return position_order;
        }
    }
}
