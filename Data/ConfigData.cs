using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Linq;

namespace MechKineticsArtSoftware.Data
{
    public class BoardInfo
    {
        public int board_group_index { get; set; }
        public string ip_address { get; set; }
        public string structure {get;set;}

        public List<int> GetStructureList() {
            return structure.Split(",").Select(x => int.Parse(x)).ToList();
        }
    }

    public class ConfigData
    {
        public int num_board_group { get; set; }
        public List<BoardInfo> boards_info { get; set; }

        public int num_motor { get; set; }

        public string nc_file_name { get; set; }

        public string nc_file_name_ext { get; set; }

        public string save_path_on_board { get; set; }
        public string save_nc_path_on_pc_directory { get; set; }
        public Dictionary<string,string> motor_assign { get; set; }

        public float dafault_movetime { get; set; }

        public int max_keyframe { get; set; }
        public int position_update_interval { get; set; }

        public bool is_enable_ncfile_loop { get; set; }

        public Dictionary<int,(int,int,int)> GetMotorAssign()
        {
            Dictionary<int, (int, int, int)> assign_dic = new Dictionary<int, (int, int, int)>();

            foreach (var key in motor_assign.Keys)
            {
                var motor_assign_info = motor_assign[key].Split(".");

                int board_group = int.Parse(motor_assign_info[0]);
                int board_index = int.Parse(motor_assign_info[1]);
                int axis_assign_num = int.Parse(motor_assign_info[2]);

                assign_dic.Add(int.Parse(key), (board_group, board_index, axis_assign_num));
            }

            return assign_dic;
        }

        public ConfigData()
        {
            boards_info = new List<BoardInfo>();
            motor_assign = new Dictionary<string, string>();
        }
    }
}