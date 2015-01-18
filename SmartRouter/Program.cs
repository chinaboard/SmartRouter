using DotArgs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SmartRouter
{
    class Program
    {

        private readonly static string loginMethod = "/cgi-bin/luci";
        private readonly static string changeWANMethod = "/admin/network/network/wan";
        private readonly static string connectWANMethod = "/admin/network/iface_reconnect/wan?_=";
        private readonly static string getSysLogMethod = "/admin/status/syslog";
        private static string baseUrl = "http://";

        private static string cookie = string.Empty;

        private static string host = string.Empty;
        private static string rUser = string.Empty;
        private static string rPwd = string.Empty;
        private static string pUser = string.Empty;
        private static string pPwd = string.Empty;

        static void Main(string[] args)
        {
            CommandLineArgs cmd = new CommandLineArgs();

            cmd.RegisterArgument("h", new OptionArgument("192.168.1.1"));
            cmd.RegisterArgument("ru", new OptionArgument("root"));
            cmd.RegisterArgument("rp", new OptionArgument("root"));
            cmd.RegisterArgument("pu", new OptionArgument("", true));
            cmd.RegisterArgument("pp", new OptionArgument("", true));

            if (!cmd.Validate(args))
            {
                Console.WriteLine("exp:\r\nSmartRouter h RouterIP ru RouterUser rp RoterPwd pu PPPoEUser pp PPPoEPwd");
                return;
            }

            host = cmd.GetValue<string>("h");
            rUser = cmd.GetValue<string>("ru");
            rPwd = cmd.GetValue<string>("rp");
            pUser = cmd.GetValue<string>("pu");
            pPwd = cmd.GetValue<string>("pp");

            Console.WriteLine("{0} : {1}", "Host", host);
            Console.WriteLine("{0} : {1}", "Username", rUser);
            Console.WriteLine("{0} : {1}", "Password", rPwd);
            Console.WriteLine("{0} : {1}", "PPPoEUser", pUser);
            Console.WriteLine("{0} : {1}", "PPPoEPwd", pPwd);

            Login();

            ChangeWANConfig();

            ConnectWAN();

            PrintSysLog();

            Console.ReadLine();
        }
        private static void Login()
        {
            var loginStr = string.Format("username={0}&password={1}", rUser, rPwd);
            baseUrl = baseUrl.ConcatUrl(host);
            cookie = Helper.Login(baseUrl.ConcatUrl(loginMethod), loginStr.GetBytes());
            baseUrl = baseUrl.ConcatUrl(new Regex("path=(.*)", RegexOptions.Singleline).Match(cookie).Groups[1].Value);
        }

        private static void ChangeWANConfig()
        {
            var changeResult = Helper.ChangeWANConfig(baseUrl.ConcatUrl(changeWANMethod), cookie, pUser, pPwd);
        }
        private static void ConnectWAN()
        {
            var connResult = Helper.Get(baseUrl.ConcatUrl(connectWANMethod, DateTime.Now.Ticks.ToString()), cookie);
        }
        private static void PrintSysLog()
        {
            Thread.Sleep(1000);
            Task.Factory.StartNew(() =>
            {
                Dictionary<string, int> dict = new Dictionary<string, int>();
                CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("en-US");
                string format = "MMM d HH:mm:ss";
                while (true)
                {
                    var syslogResult = Helper.Get(baseUrl.ConcatUrl(getSysLogMethod), cookie);
                    var logList = new Regex(@"(id=""syslog"">)(.*)(</textarea>)", RegexOptions.Singleline).Match(syslogResult).Groups[2].Value.Split('\n').ToList();
                    foreach (var logStr in logList)
                    {
                        try
                        {
                            if (string.IsNullOrWhiteSpace(logStr))
                                continue;
                            if (!dict.ContainsKey(logStr))
                                dict[logStr] = 0;
                            dict[logStr]++;
                            if (dict[logStr] == 1)
                                Console.WriteLine(logStr.Replace("OpenWrt", ""));
                            var timeStr = logStr.Substring(0, logStr.IndexOf('O') - 1);
                            DateTime dateTime = DateTime.ParseExact("SEP 8 23:44:07", format, cultureInfo);
                            if (logStr.Contains("now up") && DateTime.Now.Subtract(dateTime).Seconds < 10)
                            {
                                Console.WriteLine();
                                Console.WriteLine("Command Complete!");
                                return;
                            }
                        }
                        catch(Exception)
                        {
                            continue;
                        }
                    }
                    Thread.Sleep(1000);
                }
            });
        }


    }
}
