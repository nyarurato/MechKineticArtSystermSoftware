using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using MechKineticsArtSoftware.Data;
using System.Timers;

namespace MechKineticsArtSoftware
{
    public class WebAPIManage
    {
        int board_num;
        LogWriter logwriter;
        public RepRapWebAPI[] api_list;
        public string[] urls;
        List<Motor> _motor_list;
        public List<Motor> motor_list { get { return _motor_list; } }
        public Dictionary<string, Motor> motor_axis_dictionary;
        List<ParentBoard> boards;

        ConfigManager configManager;
        ConfigData configData;

        public readonly string[] axis_names = new string[] { "X", "Y", "Z", "A", "B", "C", "U", "V", "W", "E" };

        float[] _positons;
        float[] positions { get { return _positons; } }

        Timer update_timer;

        public WebAPIManage(LogWriter logger,ConfigManager configmgr)
        {
            configManager = configmgr;
            configData = configManager.configData;
            board_num = configData.num_board_group;
            logwriter = logger;
            api_list = new RepRapWebAPI[board_num];
            urls = new string[board_num];
            boards = new List<ParentBoard>();
            _positons = new float[configData.num_motor];

            for (int i = 0; i < board_num; i++)
            {
                try
                {
                    int axis_counter = 0;

                    urls[i] = configData.boards_info[i].ip_address;
                    RepRapWebAPI webAPI = new RepRapWebAPI(logger);
                    var initialize_result = webAPI.Initialize(urls[i]);
                    api_list[i] = webAPI;

                    //set board info
                    List<int> board_structure = configData.boards_info[i].GetStructureList();

                    //make parent board
                    ParentBoard parent_board = new ParentBoard((BoardType)board_structure[0]);
                    parent_board.board_index_on_boardgroup = 0;
                    parent_board.webAPI = webAPI;
                    parent_board = SetAxisName(parent_board, new Range(0, parent_board.num_max_driver));
                    axis_counter += parent_board.num_max_driver;
                    boards.Add(parent_board);

                    //make child boards
                    for (int board_index_in_group = 1; board_index_in_group < board_structure.Count; board_index_in_group++)
                    {

                        Board board = new Board((BoardType)board_structure[board_index_in_group]);
                        board.board_index_on_boardgroup = board_index_in_group;
                        board.parent = parent_board;
                        board = SetAxisName(board, new Range(axis_counter, axis_counter+board.num_max_driver));
                        axis_counter += board.num_max_driver;
                        parent_board.board_group.Add(board);
                    }

                }catch(Exception e)
                {
                    logwriter.WriteLogln(e.Message);
                    logwriter.WriteLogln("Stop Update Process for Position Information");
                    update_timer.Stop();
                }
            }

            int motor_num = configData.num_motor;

            _motor_list = SetBoardAssign(motor_num);

            motor_axis_dictionary = GetAxisMotorDictionary();

            update_timer = new Timer(configData.position_update_interval);
            update_timer.Elapsed += (sender, e) => { _ = GetPositions(); };
            update_timer.Start();
        }

        ~WebAPIManage()
        {
            update_timer.Stop();
            update_timer.Dispose();
        }

        public List<Motor> SetBoardAssign(int unit_num)
        {
            List<Motor> motor_list = new List<Motor>();

            Dictionary<int,(int,int,int)> motor_assign_info = configData.GetMotorAssign();

            for (int i = 0; i < unit_num; i++)
            {
                try
                {
                    int board_group = motor_assign_info[i].Item1;
                    int board_index = motor_assign_info[i].Item2;
                    int axis_assign_num = motor_assign_info[i].Item3;

                    Board board = GetBoardFromIndex(board_group, board_index);
                    Motor motor = new Motor(i,board,axis_assign_num);

                    logwriter.WriteLogln($"Loaded Unit Assignment unit: {i} -> group:{board_group} , board:{board_index} , axis:{axis_assign_num}: {motor.axis_name}");

                    motor_list.Add(motor);
                }
                catch (Exception e)
                {
                    logwriter.WriteLogln($"CAN NOT Load Board Assign Setting : unit{i}\n" + e.Message);
                }

            }

            return motor_list;
        }

        public Motor GetMotorFromMotorIndex(int unit_index)
        {
            return _motor_list[unit_index];
        }

        public List<Motor> GetMotorListOnSameBoardGroup(int board_group_index)
        {
            return motor_list.Where(m => (m.unit_board.parent == boards[board_group_index])).ToList();
        }

        public Board GetBoardFromIndex(int board_group_index,int board_index_in_group)
        {
            if (board_index_in_group == 0)
                return boards[board_group_index];
            else
                return boards[board_group_index].board_group[board_index_in_group-1];
        }

        public ParentBoard GetBoardFromIndex(int board_group_index)
        {
            return boards[board_group_index];
        }


        Board SetAxisName(Board board,Range range)
        {
            for(int i = range.Start.Value; i < range.End.Value; i++)
            {
                if (i < (axis_names.Length-1))
                    board.SetAxisName(i-range.Start.Value, axis_names[i]);
                else
                {
                    board.SetAxisName(i - range.Start.Value, $"E{ i-(axis_names.Length-1) }");
                }

            }
            return board;
        }

        ParentBoard SetAxisName(ParentBoard board, Range range)
        {
            for (int i = range.Start.Value; i < range.End.Value; i++)
            {
                if (i < (axis_names.Length-1))
                    board.SetAxisName(i - range.Start.Value, axis_names[i]);
                else
                {
                    board.SetAxisName(i - range.Start.Value, $"E{ i - (axis_names.Length - 1) }");
                }

            }
            
            return board;
        }

        Dictionary<string,Motor> GetAxisMotorDictionary()
        {
            Dictionary<string, Motor> axis_and_motor = new Dictionary<string, Motor>();

            foreach(Motor motor in motor_list)
            {
                axis_and_motor.Add(motor.axis_name, motor);
            }

            return axis_and_motor;
        }

        public async Task GetPositions()
        {
            Dictionary<string,float> pos;
            for (int i=0;i < boards.Count;i++)
            {
                /*
                 * [Xpos,Ypos,Zpos,Apos,Bpos,Cpos,Upos,Vpos,Wpos,E1pos,E2pos,E3pos....]
                 */
                pos = await boards[i].webAPI.GetPos();

                foreach(var pair in pos)
                {
                   
                    motor_axis_dictionary[pair.Key].SetPosition(pair.Value); 
                    
                }
            }
        }
    }
}
