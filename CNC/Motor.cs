using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace MechKineticsArtSoftware
{
  
    //TODO: Adding Multiple E-axis Processing
    /// <summary>
    /// Motor class
    /// </summary>
    public class Motor
    {
        int _unit_index;
        Board board;
        int _driver_num;
        bool? _is_e_axis = null;
        public bool is_e_axis_motor { get {
                if (_is_e_axis == null) {
                    _is_e_axis = board.GetMotorAxisName(_driver_num).Contains("E");
                }
                return (_is_e_axis??false);
            }  }

        public int driver_num { get { return _driver_num; } }

        /// <summary>motor index</summary> 
        public int unit_index { get { return _unit_index; } }
        public Board unit_board { get { return board; } }
        public string axis_name { get { return board.GetMotorAxisName(_driver_num); } }

        float _position = 0;
        public float position { get { return _position; } set { _position = value; } }

        public Motor(int motor_index,Board motor_on_board,int driver_index)
        {
            _unit_index = motor_index;
            board = motor_on_board;
            _driver_num = driver_index;
        }

        public async Task<string> JogMove(float distance) {

            string move_order=string.Empty;

            if (is_e_axis_motor)
            {
                if (board.is_multiple_e_axis)
                {
                    move_order = $"G0 " + GetMultipleEaxisMovePositionWord(distance);
                }
                else
                {   //single E axis
                    move_order = $"G0 E{distance.ToString("f2")}";
                }

            }
            else {
                move_order = $"G0 {board.GetMotorAxisName(_driver_num)}{distance.ToString("f2")}";

            }

            return await board.parent.webAPI.SendGcode($"G91 {move_order} G90");

        }

        public async Task<string> MoveTo(float position)
        {

            string move_order=string.Empty;


            if (is_e_axis_motor) {
                if (board.is_multiple_e_axis)
                {
                    move_order = $"G0 " + GetMultipleEaxisMovePositionWord(CalcRelativePosFromCurrentPos(position));
                }
                else
                {   //single E axis
                    move_order = $"G0 E{CalcRelativePosFromCurrentPos(position)}";
                }
            }
            else
            {
                move_order = $"G90 G0 {board.GetMotorAxisName(_driver_num)}{position.ToString("f2")}";


            }
            return await board.parent.webAPI.SendGcode(move_order);

        }

        public void SetPosition(float value)
        {
            _position = value;
        }

        public float CalcRelativePos(float target_pos_in_abs_coord,float reference_val_in_abs_coord)
        {
            return target_pos_in_abs_coord - reference_val_in_abs_coord;
        }

        public float CalcRelativePosFromCurrentPos(float target_pos_in_abs_coord)
        {
            return CalcRelativePos(target_pos_in_abs_coord,position);
        }

        Regex regex = new Regex("\\d");

        string GetMultipleEaxisMovePositionWord(float distance)
        {
            string word = string.Empty;

            Match match = regex.Match(axis_name);

            if (match.Success)
            {
                int axis_num = int.Parse(match.Value);

                word += "E";

                for(int i = 0; i < axis_num; i++)
                {
                    word += "0:";
                }
                word += $"{distance}";
            }
            else
            {
                word = $"E{distance}";
            }


            return word;
        }
    }
}
