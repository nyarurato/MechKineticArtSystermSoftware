using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace MechKineticsArtSoftware
{
    public class WebAPIDatas
    {
        int board_num;
        LogWriter logwriter;
        public RepRapWebAPI[] api_list;
        public string[] urls;
        List<UnitBoardMotor> unit_board_motor_relation_list;
        public List<UnitBoardMotor> relation_list { get { return unit_board_motor_relation_list; } }

        ConfigManager configManager;


        public WebAPIDatas(LogWriter logger,ConfigManager configmgr)
        {
            configManager = configmgr;
            board_num = int.Parse(configManager.appsettings["num_board"]);
            logwriter = logger;
            api_list = new RepRapWebAPI[board_num];
            urls = new string[board_num];

            for (int i = 0; i < board_num; i++)
            {
                try
                {
                    urls[i] = configManager.appsettings["ip" + i.ToString()];
                    RepRapWebAPI webAPI = new RepRapWebAPI(logger);
                    var _ = webAPI.Initialize(urls[i]);
                    api_list[i] = webAPI;
                }catch(Exception e)
                {
                    logwriter.writeLog(e.Message);
                }
            }

            unit_board_motor_relation_list = SetBoardAssign(8);
        }

        public List<UnitBoardMotor> SetBoardAssign(int unit_num)
        {
            float[] board_assign_num = new float[unit_num];
            List<UnitBoardMotor> unitboardmotro_list = new List<UnitBoardMotor>();

            for(int i = 0; i < unit_num; i++)
            {
                try
                {
                    board_assign_num[i] = float.Parse(configManager.appsettings["unit" + i.ToString()]);

                    int board_index = ((int)Math.Floor(board_assign_num[i]));
                    int axis_assign_num = (int)Math.Round((board_assign_num[i] - board_index) * 10);

                    UnitBoardMotor unitMotorAssign = new UnitBoardMotor(i,api_list[board_index],axis_assign_num);

                    logwriter.writeLog($"Loaded Unit Assignment unit: {i} -> board:{board_index} , axis:{axis_assign_num}");

                    unitboardmotro_list.Add(unitMotorAssign);
                }
                catch (Exception e)
                {
                    logwriter.writeLog("CAN NOT Load Board Assign Setting :" + e.Message);
                }

            }

            return unitboardmotro_list;
        }

        public UnitBoardMotor GetBoardMotorFromUnitIndex(int unit_index)
        {
            return unit_board_motor_relation_list[unit_index];
        }
    }
}
