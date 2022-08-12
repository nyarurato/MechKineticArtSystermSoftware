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

namespace MechKineticsArtSoftware
{
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
                logwriter.writeLogln($"Checking... {base_url}");

                var response = await client.GetAsync($"{http_uri}{base_url}/{url_status}?type=1");
                if (response.StatusCode.Equals(HttpStatusCode.OK))
                {
                    logwriter.writeLogln($"Check OK {base_url}");
                    return true;
                }
                else
                {
                    logwriter.writeLogln("Bad IP " + base_url + " " + response.StatusCode);
                    return false;
                }
            }
            catch (Exception e)
            {
                logwriter.writeLogln(base_url + " : " + e.Message);
                return false;
            }
        }

        public async Task<bool> Connect(string base_url)
        {
            if (!valid_url_flag)
            {
                logwriter.writeLogln($"{base_url} Stop Connecting for invalid adress");
                return false;
            }

            try
            {
                logwriter.writeLogln($"Connecting... {base_url}");

                var response = await client.GetAsync($"{http_uri}{base_url}/{url_connect}?password=");
                if (response.StatusCode.Equals(HttpStatusCode.OK))
                {
                    logwriter.writeLogln($"Connected! {base_url}");
                    board_url = base_url;
                    connected_flag = true;
                    return true;
                }
                else
                {
                    logwriter.writeLogln("Can NOT Connect " + base_url + " " + await response.Content.ReadAsStringAsync());
                    return false;
                }
            }
            catch (Exception e)
            {
                logwriter.writeLogln(base_url + " : " + e.Message);
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
                logwriter.writeLogln($"DisConnecting... {board_url}");

                var response = await client.GetAsync($"{http_uri}{board_url}/{url_disconnect}");
                if (response.StatusCode.Equals(HttpStatusCode.OK))
                {
                    logwriter.writeLogln($"Disconnected! {board_url}");

                    connected_flag = false;
                    return true;
                }
                else
                {
                    logwriter.writeLogln("CAN NOT DisConnect " + board_url + " " + response.Content.ReadAsStringAsync().Result);
                    return false;
                }
            }
            catch (Exception e)
            {
                logwriter.writeLogln(board_url + " : " + e.Message);
                return false;
            }
        }

        public async Task<string> SendGcode(string gcode)
        {
            if (!connected_flag)
            {
                logwriter.writeLogln($"{board_url}:No Connection. Do Not send {gcode}");
                return "";
            }

            try
            {
                logwriter.writeLogln($"Send {board_url} : {gcode} , {http_uri}{board_url}/{url_gcode}?gcode={gcode}");

                var response = await client.GetAsync($"{http_uri}{board_url}/{url_gcode}?gcode={gcode}");
                if (response.StatusCode.Equals(HttpStatusCode.OK))
                {
                    var resp = GetReply();
                    logwriter.writeLogln(response.Content.ReadAsStringAsync().Result);
                    logwriter.writeLogln($"{gcode} result:{await resp}");
                    return await resp;
                }
                else
                {
                    logwriter.writeLogln("CAN NOT DisConnect " + board_url + " " + response.Content.ReadAsStringAsync().Result);
                    return "";
                }
            }
            catch (Exception e)
            {
                logwriter.writeLogln(e.Message);
                return "";
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
                    logwriter.writeLogln("CAN NOT DisConnect " + board_url + " " + res);
                    return "";
                }
            }
            catch (Exception e)
            {
                logwriter.writeLogln(e.Message);
                return "";
            }
        }

        public async Task<bool> SendFile(StreamReader streamReader,string path_and_name)
        {
            if (!connected_flag)
            {
                logwriter.writeLogln($"{board_url} Do Not Send File for No Connection");
                return false;
            }

            try
            {
                logwriter.writeLogln($"Send File... To: {board_url} Path: {path_and_name}");
                HttpContent content = new StringContent(streamReader.ReadToEnd(),Encoding.UTF8);
                Debug.WriteLine(content.ReadAsStringAsync().Result);

                await Connect(board_url);
                var response = await client.PostAsync($"{http_uri}{board_url}/{url_upload}?name={path_and_name}&time={DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")}",content);
                if (response.StatusCode.Equals(HttpStatusCode.OK))
                {
                    JObject json_result = JsonRead(response.Content.ReadAsStringAsync().Result);
                    if (json_result == null)
                    {
                        logwriter.writeLogln($"Failed Send File To: {board_url}");

                        return false;
                    }

                    if(json_result["err"].ToObject<int>() == 0)
                    {
                        logwriter.writeLogln($"Success Send File To: {board_url}");
                        return true;
                    }
                    else
                    {
                        logwriter.writeLogln($"Failed Send File To: {board_url}");
                        return false;
                    }
                    

                }
                else
                {
                    logwriter.writeLogln($"Send Failed To: {board_url} Code:{response.StatusCode.ToString()}");
                    logwriter.writeLogln($"{response.Content.ReadAsStringAsync().Result}");
                    return false;
                }
            }
            catch (Exception e)
            {
                logwriter.writeLogln(e.Message);
                return false;
            }
        }

        public bool RunFile(string file_path_on_board)
        {
            if (!connected_flag)
            {
                logwriter.writeLogln($"{board_url} Do Not Run File for No Connection");
                return false;
            }

            try
            {
                SendGcode($"M32 {file_path_on_board}");
                return true;
            }
            catch (Exception e)
            {
                logwriter.writeLogln(e.Message);
                return false;
            }

        }




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
                logwriter.writeLogln($"Error: {e.Message}");
                return null;
            }
        }

        public async Task<List<float>> GetPos()
        {
            List<float> pos = new List<float>();

            if (!connected_flag)
            {
                //form1.writeLogln($"{board_url} Do Not Send File for No Connection");
                return pos;
            }

            try
            {
                var response = await client.GetAsync($"{http_uri}{board_url}/{url_model}?key=move.axes[].machinePosition&flags=f");
                if (response.StatusCode.Equals(HttpStatusCode.OK))
                {
                    JObject json_res = JsonRead(response.Content.ReadAsStringAsync().Result);
                    if(json_res == null)
                    {
                        logwriter.writeLogln($"Error: Can NOT get position data");

                        return pos;
                    }


                    var pos_list = json_res["result"].ToList();

                    foreach(var res_p in pos_list)
                    {
                        pos.Add(res_p.ToObject<float>());
                    }
                }
                

            }
            catch(Exception e)
            {
                logwriter.writeLogln($"Error: {e.Message}");
            }


            /*
            //Debug
            string str = "";
            foreach(var p in pos)
            {
                str += $"{p}, ";
            }
            logwriter.writeLogln($"[{str}]");
            */

            return pos;
        }

    }
}
