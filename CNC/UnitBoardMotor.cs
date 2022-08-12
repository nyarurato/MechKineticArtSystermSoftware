using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MechKineticsArtSoftware
{
    public class UnitBoardMotor
    {
        int unit_num;
        RepRapWebAPI board;
        string[] motor_name;
     
        public int unit_index { get { return unit_num; } }
        public RepRapWebAPI unit_board { get { return board; } }
        public string[] axis_each_name { get { return motor_name; } }

        readonly string[][] axis_name_unit = new string[][]{
            new string[]{"X","Y","Z" },
            new string[]{"A","B","C" },
            new string[]{"U","V","W" }
        };

        public UnitBoardMotor(int unit_index,RepRapWebAPI repRapWebAPI,int motor_group_index)
        {
            unit_num = unit_index;
            board = repRapWebAPI;
            motor_name = axis_name_unit[motor_group_index];
        }

    }
}
