using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace MechKineticsArtSoftware.Data
{
    public class ConfigData
    {
        public int num_board { get; set; }
        public List<string> ip_address { get; set; }

        public int num_motor { get; set; }

        public string nc_file_name { get; set; }

        public string save_path_on_board { get; set; }
        public string save_nc_path_on_pc_directory { get; set; }
        public string save_nc_name_on_pc { get; set; }
        public List<List<string>> unit_assign { get; set; }
        public float dafault_movetime { get; set; }

        public int max_keyframe { get; set; }
        public float position_update_interval { get; set; }

        public bool is_enable_ncfile_loop { get; set; }

        public ConfigData()
        {
            ip_address = new List<string>();
            unit_assign = new List<List<string>>();
        }
    }
}