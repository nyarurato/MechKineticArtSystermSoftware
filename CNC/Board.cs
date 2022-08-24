using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace MechKineticsArtSoftware
{
    public enum BoardType
    {
        DUET3MAINBOARD6HC = 0,
        DUET3EXPANSION3HC = 1,
        DUET3EXPANSION1HCL = 2,
        DUET3EXPANSION1LC = 3,
        DUET3MINI5PLUS = 4,
        DUET3MINI2PLUS = 5
    };

    public class Board
    {
        public int board_index_on_boardgroup { get; set; }

        public ParentBoard parent;

        BoardType _boardType;
        readonly int[] num_driver_array = { 6, 3, 1, 1, 5, 2 };

        string[] axis_names_on_board;

        public bool[] active_driver_array;

        public int num_max_driver { get { return num_driver_array[((int)_boardType)]; } }

        public bool is_multiple_e_axis { get; set; }

        public Board(BoardType boardType)
        {
            _boardType = boardType;
            axis_names_on_board = new string[num_max_driver];
            active_driver_array = new bool[6] { true, true, true, true, true, true };//temp
        }

        public string GetMotorAxisName(int driver_index_on_board)
        {
            if (driver_index_on_board < axis_names_on_board.Length)
                return axis_names_on_board[driver_index_on_board];
            else
                throw new Exception("Out of Range drive index on board");
        }

        Regex multiple_e_axis_regex = new Regex(@"E[1-9]+$");

        public void SetAxisName(int driver_num, string axis_name)
        {
            if (multiple_e_axis_regex.IsMatch(axis_name))// E1～E???
            {
                is_multiple_e_axis = true;
            }

            axis_names_on_board[driver_num] = axis_name;
        }
    }

    public class ParentBoard : Board
    {

        public RepRapWebAPI webAPI { get; set; }
        public List<Board> board_group { get; set; }

        public ParentBoard(BoardType boardType) : base(boardType)
        {
            parent = this;
            board_group = new List<Board>();

        }
    }

  
}
