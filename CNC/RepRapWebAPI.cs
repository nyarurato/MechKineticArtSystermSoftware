using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Numerics;

namespace MechKineticsArtSoftware
{
    /// <summary>
    /// Status Store Object
    /// </summary>
    public class DuetStatus
    {

        /// <summary>
        /// Status
        /// https://github.com/Duet3D/RepRapFirmware/wiki/Object-Model-Documentation#statestatus
        /// </summary>        
        public enum MACHINE_STATUS
        {
            DISCONNECTED,
            STARTING,
            UPDATING,
            OFF,
            HALTED,
            PAUSING,
            PAUSED,
            RESUMING,
            PROCESSING,
            SIMULATING,
            BUSY,
            CHANGINGTOOL,
            IDLE,
            OTHER
        }

        public DuetStatus()
        {
            status = MACHINE_STATUS.DISCONNECTED;
            switches_triggered = new List<bool>();
            probe_LastStopHeight = new List<float>();
        }

        /*
         * move 
         */

        /// <summary>
        /// Machine position XYZ
        /// </summary>
        public Vector3 machine_position;

        /// <summary>
        /// Work position XYZ
        /// </summary>
        public Vector3 user_position;

        /*
         * job 
         */

        /// <summary>
        /// job file name on running 
        /// </summary>
        public string job_filename;

        /// <summary>
        /// running time of job
        /// </summary>
        public int job_duration;

        /*
         *  network
         */


        /// <summary>
        /// hostname
        /// </summary>
        public string hostname;

        /// <summary>
        /// IP address
        /// </summary>
        public string ipaddress;

        /// <summary>
        /// MAC address
        /// </summary>
        public string macaddress;

        /*
         * state 
         */


        /// <summary>
        /// 現在のステータス
        /// </summary>
        public MACHINE_STATUS status;

        /// <summary>
        /// 取得時の時間
        /// UTC表示
        /// </summary>
        public DateTime data_time;

        /*
         * sensor 
         */

        /// <summary>
        /// センサー値
        /// </summary>
        public float sensor_lastReading;

        /// <summary>
        /// リミットスイッチトリガー状態
        /// </summary>
        public List<bool> switches_triggered;

        /// <summary>
        /// プローブの最後に検出したタッチ高さ
        /// </summary>
        public List<float> probe_LastStopHeight;

        /*
         * directories
         */
        /// <summary>
        /// Gコードファイル（NCファイル）のディレクトリ
        /// </summary>
        public string directory_gcode;
        /// <summary>
        /// systemファイルのディレクトリ
        /// </summary>
        public string directory_sys;



        public override string ToString()
        {
            string sw_result = "";
            foreach (bool b in switches_triggered)
            {
                sw_result += $"{b.ToString()}, ";
            }

            string probe_result = "[";
            foreach (float f in probe_LastStopHeight)
            {
                probe_result += $"{f.ToString()}, ";
            }

            return $"machine pos:{machine_position}, user pos:{user_position}, " +
                   $"job_fname:{job_filename}, job_time:{job_duration}, " +
                   $"hostname:{hostname}, ipaddress:{ipaddress}, mac:{macaddress}, " +
                   $"status:{status.ToString()}, time:{data_time.ToString()}, " +
                   $"sensor:{sensor_lastReading}, switch:[{sw_result}], probe:[{probe_result}]," +
                   $"dir_gcode:{directory_gcode}, dir_sys:{directory_sys}";
        }

    }

    public class RepRapWebAPI
    {
        private static HttpClient client = new HttpClient();
        const string url_connect = "rr_connect";//get
        const string url_disconnect = "rr_disconnect";//get
        const string url_status = "rr_status";//get
        const string url_gcode = "rr_gcode";//get
        const string url_reply = "rr_reply";//get
        const string url_upload = "rr_upload";//get,post
        const string url_delete = "rr_delete";//get
        const string url_mkdir = "rr_mkdir";//get
        const string url_model = "rr_model";//get
        const string http_uri = "http://";

        public const string error_response_word = "error";

        private bool valid_url_flag = false;
        private bool connected_flag = false;
        private string board_url;

        public string url { get { return board_url; } }
        public bool is_connected { get { return connected_flag; } }
        public bool is_valid_url { get { return valid_url_flag; } }

        LogWriter logwriter;

        public RepRapWebAPI(LogWriter logger)
        {
            logwriter = logger;        
        }

        ~RepRapWebAPI()
        {
            if (connected_flag)
            {
                var _ = DisConnect();
            }
        }

        public async Task<bool> Initialize(string base_url)
        {
            bool result = await CheckURL(base_url);
            valid_url_flag = result;
            board_url = base_url;
            if (valid_url_flag)
            {
                await Connect(base_url);
            }
            return true;
        }

        public async Task<bool> CheckURL(string base_url)
        {
            try
            {
                logwriter.WriteLogln($"Checking... {base_url}");

                var response = await client.GetAsync($"{http_uri}{base_url}/{url_status}?type=1");
                if (response.StatusCode.Equals(HttpStatusCode.OK))
                {
                    logwriter.WriteLogln($"Check OK {base_url}");
                    return true;
                }
                else
                {
                    logwriter.WriteLogln("Bad IP " + base_url + " " + response.StatusCode);
                    return false;
                }
            }
            catch (Exception e)
            {
                logwriter.WriteLogln(base_url + " : " + e.Message);
                return false;
            }
        }

        public async Task<bool> Connect(string base_url)
        {
            if (!valid_url_flag)
            {
                logwriter.WriteLogln($"{base_url} Stop Connecting for invalid adress");
                return false;
            }

            try
            {
                logwriter.WriteLogln($"Connecting... {base_url}");

                var response = await client.GetAsync($"{http_uri}{base_url}/{url_connect}?password=");
                if (response.StatusCode.Equals(HttpStatusCode.OK))
                {
                    logwriter.WriteLogln($"Connected! {base_url}");
                    board_url = base_url;
                    connected_flag = true;
                    return true;
                }
                else
                {
                    logwriter.WriteLogln("Can NOT Connect " + base_url + " " + await response.Content.ReadAsStringAsync());
                    return false;
                }
            }
            catch (Exception e)
            {
                logwriter.WriteLogln(base_url + " : " + e.Message);
                return false;
            }
        }

        public async Task<bool> DisConnect()
        {
            if (!connected_flag)
            {
                return true;
            }

            try
            {
                logwriter.WriteLogln($"DisConnecting... {board_url}");

                var response = await client.GetAsync($"{http_uri}{board_url}/{url_disconnect}");
                if (response.StatusCode.Equals(HttpStatusCode.OK))
                {
                    logwriter.WriteLogln($"Disconnected! {board_url}");

                    connected_flag = false;
                    return true;
                }
                else
                {
                    logwriter.WriteLogln("CAN NOT DisConnect " + board_url + " " + response.Content.ReadAsStringAsync().Result);
                    return false;
                }
            }
            catch (Exception e)
            {
                logwriter.WriteLogln(board_url + " : " + e.Message);
                return false;
            }
        }

        public async Task<string> SendGcode(string gcode)
        {
            if (!connected_flag)
            {
                logwriter.WriteLogln($"{board_url}:No Connection. Do Not send {gcode}");
                return "";
            }

            try
            {
                logwriter.WriteLogln($"Send {board_url} : {gcode} , {http_uri}{board_url}/{url_gcode}?gcode={gcode}");

                var response = await client.GetAsync($"{http_uri}{board_url}/{url_gcode}?gcode={gcode}");
                if (response.StatusCode.Equals(HttpStatusCode.OK))
                {
                    var resp = GetReply();
                    logwriter.WriteLogln(response.Content.ReadAsStringAsync().Result);
                    logwriter.WriteLogln($"{gcode} result:{await resp}");
                    return await resp;
                }
                else
                {
                    logwriter.WriteLogln("CAN NOT Send " + board_url + " " + response.Content.ReadAsStringAsync().Result);
                    return error_response_word;
                }
            }
            catch (Exception e)
            {
                logwriter.WriteLogln(e.Message);
                return error_response_word;
            }
        }

        async Task<string> GetReply()
        {
            if (!connected_flag)
            {
                return "";
            }

            try
            {
                var response = await client.GetAsync($"{http_uri}{board_url}/{url_reply}");
                if (response.StatusCode.Equals(HttpStatusCode.OK))
                {
                    string res = response.Content.ReadAsStringAsync().Result;
                    if (res == "")
                    {
                        return "no message";
                    }
                    else
                    {
                        return res;
                    }
                }
                else
                {string res = await response.Content.ReadAsStringAsync();
                    logwriter.WriteLogln("CAN NOT Get Reply " + board_url + " " + res);
                    return error_response_word;
                }
            }
            catch (Exception e)
            {
                logwriter.WriteLogln(e.Message);
                return error_response_word;
            }
        }

        public async Task<bool> SendFile(StreamReader streamReader,string path_and_name)
        {
            if (!connected_flag)
            {
                logwriter.WriteLogln($"{board_url} Do Not Send File for No Connection");
                return false;
            }

            try
            {
                logwriter.WriteLogln($"Send File... To: {board_url} Path: {path_and_name}");
                HttpContent content = new StringContent(streamReader.ReadToEnd(),Encoding.UTF8);
                Debug.WriteLine(content.ReadAsStringAsync().Result);

                await Connect(board_url);
                var response = await client.PostAsync($"{http_uri}{board_url}/{url_upload}?name={path_and_name}&time={DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")}",content);
                if (response.StatusCode.Equals(HttpStatusCode.OK))
                {
                    JObject json_result = JsonRead(response.Content.ReadAsStringAsync().Result);
                    if (json_result == null)
                    {
                        logwriter.WriteLogln($"Failed Send File To: {board_url}");

                        return false;
                    }

                    if(json_result["err"].ToObject<int>() == 0)
                    {
                        logwriter.WriteLogln($"Success Send File To: {board_url}");
                        return true;
                    }
                    else
                    {
                        logwriter.WriteLogln($"Failed Send File To: {board_url}");
                        return false;
                    }
                    

                }
                else
                {
                    logwriter.WriteLogln($"Send Failed To: {board_url} Code:{response.StatusCode.ToString()}");
                    logwriter.WriteLogln($"{response.Content.ReadAsStringAsync().Result}");
                    return false;
                }
            }
            catch (Exception e)
            {
                logwriter.WriteLogln(e.Message);
                return false;
            }
        }

        public async Task<bool> RunFile(string file_path_on_board)
        {
            if (!connected_flag)
            {
                logwriter.WriteLogln($"{board_url} Do Not Run File for No Connection");
                return false;
            }

            try
            {
                await SendGcode($"M32 \"{file_path_on_board}\"");
                return true;
            }
            catch (Exception e)
            {
                logwriter.WriteLogln(e.Message);
                return false;
            }

        }


        /// <summary>
        /// /machine/status
        /// </summary>
        /*
        public async Task<DuetStatus> GetStatus(string base_url, Action<JObject> callback = null)
        {
            DuetStatus duetStatus = null;

            var response = await client.GetAsync(http_uri + base_url + url_status);

            if (response.StatusCode.Equals(HttpStatusCode.OK))
            {
                duetStatus = GetStatusOBjFromJobj(JsonRead(await response.Content.ReadAsStringAsync()));

            }

            return duetStatus;
 
        }
         */

        /// <summary>
        /// 入力のJSON文字列をJObject型に変換します。
        /// </summary>
        /// <param name="json">JSON文字列</param>
        /// <returns>Jobject or null</returns>
        public JObject JsonRead(string json)
        {
            if (String.IsNullOrEmpty(json))
            {
                return null;
            }
            try
            {
                JObject jobj = JObject.Parse(json);
                
                return jobj;
            }
            catch (JsonException e)
            {
                logwriter.WriteLogln($"Error: {e.Message}");
                return null;
            }
        }

        public async Task<Dictionary<string,float>> GetPos()
        {
            Dictionary<string, float> pos = new Dictionary<string, float>();

            if (!connected_flag)
            {
                logwriter.WriteLogln($"{board_url}: No Connection. Can Not Get Pos");
                return pos;
            }

            try
            {
                var response = await client.GetAsync($"{http_uri}{board_url}/{url_model}?key=move.axes[]&flags=v");
                var response_ext = await client.GetAsync($"{http_uri}{board_url}/{url_model}?key=move.extruders[].position&flags=f");
                if (response.StatusCode.Equals(HttpStatusCode.OK) && response_ext.StatusCode.Equals(HttpStatusCode.OK))
                {
                    JObject json_res = JsonRead(await response.Content.ReadAsStringAsync());
                    JObject json_res2 = JsonRead(await response_ext.Content.ReadAsStringAsync());

                    if (json_res == null || json_res2 == null)
                    {
                        logwriter.WriteLogln($"Error: Can NOT get position data");

                        return pos;
                    }

                    var pos_dic = json_res["result"].ToList().ToDictionary(jtoken => jtoken["letter"].ToString(),jtoken => jtoken["machinePosition"].ToObject<float>());
                    var e_pos_dic = json_res2["result"].Select((val, index) => new { index, val })
                                                       .ToDictionary(val => $"E{val.index}", val => val.val.ToObject<float>());
                    
                    pos = pos_dic.Concat(e_pos_dic).ToDictionary(c => c.Key, c => c.Value);

                }
                

            }
            catch(Exception e)
            {
                logwriter.WriteLogln($"Error: {e.Message}");
            }

            return pos;
        }

        /// <summary>
        /// DuetStatusをJsonから生成
        /// </summary>
        /// <param name="jobj">StatusのJson</param>


        DuetStatus GetStatusOBjFromJobj(JObject jobj)
        {
            DuetStatus duetStatus = new DuetStatus();


            if (jobj == null)//何もなかった場合初期値のDuetStatusを返す
                return duetStatus;


            //move

            var xobj = jobj["move"]["axes"].Where(axis => (axis["letter"].ToString() == "X")).First();

            duetStatus.machine_position.X = xobj["machinePosition"].Value<float>();
            duetStatus.user_position.X = xobj["userPosition"].Value<float>();

            var yobj = jobj["move"]["axes"].Where(axis => (axis["letter"].ToString() == "Y")).First();

            duetStatus.machine_position.Y = yobj["machinePosition"].Value<float>();
            duetStatus.user_position.Y = yobj["userPosition"].Value<float>();


            var zobj = jobj["move"]["axes"].Where(axis => (axis["letter"].ToString() == "Z")).First();

            duetStatus.machine_position.Z = zobj["machinePosition"].Value<float>();
            duetStatus.user_position.Z = zobj["userPosition"].Value<float>();


            //Job


            if (jobj["job"]["duration"].Type == JTokenType.Null)
            {//Null
                duetStatus.job_duration = 0;
            }
            else
            {
                duetStatus.job_duration = jobj["job"]["duration"].Value<int>();
            }

            if (jobj["job"]["file"]["fileName"].Type == JTokenType.Null)
            {//Null
                duetStatus.job_filename = string.Empty;
            }
            else
            {
                duetStatus.job_filename = jobj["job"]["file"]["fileName"].ToString();
            }



            //network


            if (jobj["network"]["hostname"].Type == JTokenType.Null)
            {//Null
                duetStatus.hostname = string.Empty;
            }
            else
            {
                duetStatus.hostname = jobj["network"]["hostname"].ToString();
            }

            var interface_obj = jobj["network"]["interfaces"].Where(intf => (intf["actualIP"].Type != JTokenType.Null)).First();
            duetStatus.ipaddress = interface_obj["actualIP"].ToString();

            duetStatus.macaddress = interface_obj["mac"].ToString();


            //state


            if (jobj["state"]["time"].Type == JTokenType.Null)
            {//Null
                duetStatus.data_time = DateTime.MinValue;
            }
            else
            {
                DateTime.TryParse(jobj["state"]["time"].ToString(), out duetStatus.data_time);
            }

            string s = jobj["state"]["status"].ToString().ToUpper();
            if (Enum.IsDefined(typeof(DuetStatus.MACHINE_STATUS), s))
            {
                Enum.TryParse(s, out duetStatus.status);
            }




            //directories


            if (jobj["directories"]["gCodes"].Type == JTokenType.Null)
            {//Null
                duetStatus.directory_gcode = string.Empty;
            }
            else
            {
                duetStatus.directory_gcode = jobj["directories"]["gCodes"].ToString();
            }

            if (jobj["directories"]["system"].Type == JTokenType.Null)
            {//Null
                duetStatus.directory_gcode = string.Empty;
            }
            else
            {
                duetStatus.directory_sys = jobj["directories"]["system"].ToString();
            }

            logwriter.WriteLogln(duetStatus.ToString());
            return duetStatus;
        }



    }
}
