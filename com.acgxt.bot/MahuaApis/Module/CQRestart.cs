using com.acgxt.bot.Utils;
using com.acgxt.cqp.cs.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace com.acgxt.bot.MahuaApis.Module {
    /// <summary>
    /// 重启酷Q模块,需要开启悬浮窗,并且使用开发者模式
    /// </summary>
    class CQRestart {
        private enum GetWindowCmd : uint {
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5,
            GW_ENABLEDPOPUP = 6
        }
        private struct Rect {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        private enum MouseEventFlag : uint {
            Move = 0x0001,
            LeftDown = 0x0002,
            LeftUp = 0x0004,
            RightDown = 0x0008,
            RightUp = 0x0010,
            MiddleDown = 0x0020,
            MiddleUp = 0x0040,
            XDown = 0x0080,
            XUp = 0x0100,
            Wheel = 0x0800,
            VirtualDesk = 0x4000,
            Absolute = 0x8000
        }
        [DllImport("user32.dll", EntryPoint = "GetDesktopWindow", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetWindow(IntPtr hWnd, GetWindowCmd uCmd);
        [DllImport("user32.dll")]
        private extern static int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll")]
        private static extern int GetWindowRect(IntPtr hwnd, out Rect lpRect);
        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);
        [DllImport("user32.dll")]
        private static extern void mouse_event(MouseEventFlag flags, int x, int y, uint data, UIntPtr extraInfo);

        public delegate void onLoggerHandler(string msg);
        public event onLoggerHandler onLogger;
        public static IntPtr livePtr;
        public static bool isFlag = false;
        public static void __init() {
            string cqpConf = Path.Combine(Directory.GetCurrentDirectory(), "conf/CQP.cfg");
            if (!File.Exists(cqpConf)) {
                Log.warning("获取酷Q配置文件丢失:" + cqpConf);
                throw new Exception("获取酷Q配置文件丢失!");
            }
            if (CQ.getIni("Dev", "Enable", cqpConf) != "1") {
                Log.warning("请打开酷Q开发者模式");
                throw new Exception("请打开酷Q开发者模式!");
            }
            if (CQ.getIni("Main", "StatusH", cqpConf) != "0") {
                Log.warning("请打开酷Q悬浮窗模式");
                throw new Exception("请打开酷Q悬浮窗模式!");
            }

            IntPtr desktopPtr = GetDesktopWindow();
            IntPtr ptr = GetWindow(desktopPtr, GetWindowCmd.GW_CHILD);
            while (ptr != IntPtr.Zero) {
                ptr = GetWindow(ptr, GetWindowCmd.GW_HWNDNEXT);
                StringBuilder s = new StringBuilder(512);
                GetWindowText(ptr, s, s.Capacity);
                if (Util.checkEmpty(s.ToString())) continue;
                if (s.ToString().Contains("酷Q")) {
                    Process[] proList = Process.GetProcesses();
                    foreach (Process p in proList) {
                        if (p.MainWindowHandle.Equals(ptr)) {
                            CQRestart.livePtr = ptr;
                            isFlag = true;
                            Log.info("获取酷Q窗口句柄成功,支持模拟重启");
                            return;
                        }
                    }
                }
            }
            throw new Exception("获取酷Q窗口失败!");
        }


        public bool run(string group,string qq) {
            if (!isFlag) {
                try {
                    __init();
                }catch(Exception e) {
                    if (onLogger != null) onLogger("无法重启:"+e.Message);
                    return false;
                }
            }
            IntPtr ptr = livePtr;
            Rect CQP_rect = new Rect();
            int size = GetWindowRect(ptr, out CQP_rect);
            int left = CQP_rect.Left + 30;
            int top = (CQP_rect.Top + CQP_rect.Bottom) / 2;

            SetCursorPos(left, top);
            Thread.Sleep(10);
            mouse_event(MouseEventFlag.RightDown, left, top, 0, UIntPtr.Zero);
            mouse_event(MouseEventFlag.RightUp, left, top, 0, UIntPtr.Zero);
            Thread.Sleep(10);
            keybd_event(0x26, 0, 0, 0);
            Thread.Sleep(100);
            keybd_event(0x26, 0, 0, 0);
            Thread.Sleep(100);
            if (onLogger != null) onLogger("正在尝试重启酷Q中...");

            Dictionary<object, object> data = new Dictionary<object, object>();
            data.Add("group", group);
            data.Add("qq", qq);
            data.Add("time", Util.getTime());
            Conf.setConfig("global.restart", data);

            keybd_event(0xD, 0, 0, 0);
            Log.addLog("尝试执行酷Q重启...");
            //开启线程处理执行失败重启
            System.Timers.Timer t = new System.Timers.Timer(1000 * 5);
            t.AutoReset = false;
            t.Elapsed += (s, e) => {
                t.Enabled = false;
                Conf.setConfig("global.restart","none");
                if (onLogger != null) onLogger("尝试重启酷Q失败...");
            };

            return true;
        }


    }
}
