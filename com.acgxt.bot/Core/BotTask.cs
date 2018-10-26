using com.acgxt.bot.Utils;
using com.acgxt.cqp.cs.Utils;
using Newbe.Mahua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.acgxt.bot.Core {
    class BotTask {

        public static Dictionary<int, BotTask> taskList = new Dictionary<int, BotTask>();

        public delegate void onTriggerHandler(IMahuaApi api);
        public event onTriggerHandler onTrigger;
        //mode1
        private System.Timers.Timer timer = new System.Timers.Timer(1000);
        private int y;
        private int m;
        private int d;
        private int h;
        private int i;
        private int s;
        //mode2
        private int s2;
        //mode3

        private int type = 0;

        /// <summary>
        /// 每日指定时间触发模式
        /// </summary>
        /// <param name="h"></param>
        /// <param name="i"></param>
        /// <param name="s"></param>
        public BotTask(int h,int i,int s) {
            this.h = h;
            this.i = i;
            this.s = s;
            this.type = 1;
            this.timer.AutoReset = true;
            this.timer.Enabled = true;
            this.setTask();
            this.timer.Elapsed += run1;
        }
        private int getId() {
            Random r = new Random();
            int rid = r.Next(0, 99999);
            if (BotTask.taskList.ContainsKey(rid)) {
                return getId();
            }
            return rid;
        }
        private void setTask() {
            int taskId = getId();

            BotTask.taskList.Add(taskId,this);
            //save config

            Dictionary<object, object> data = new Dictionary<object, object>();

            data.Add("type", this.type);
            data.Add("name", "task-" + taskId);
            switch (this.type) {
                case 1:
                    data.Add("h",this.h);
                    data.Add("i",this.i);
                    data.Add("s",this.s);

                    break;
                case 2:
                    data.Add("s", this.s2);
                    break;
                case 3:
                    data.Add("d", this.d);
                    data.Add("h", this.h);
                    data.Add("i", this.i);
                    data.Add("s", this.s);
                    break;
            }


            Conf.setConfig("global.task",taskId.ToString(),data);
        }
        private void run1(object sender, System.Timers.ElapsedEventArgs ev) {
            int hh = int.Parse(DateTime.Now.ToString("HH"));
            int ii = int.Parse(DateTime.Now.ToString("mm"));
            int ss = int.Parse(DateTime.Now.ToString("ss"));
            //Log.addLog(String.Format("{0}:{1}:{2}-{3}:{4}:{5}", h, i, s, hh, ii, ss));
            if (hh == h && ii == i && ss == s) {
                var robotSession = MahuaRobotManager.Instance.CreateSession();
                if (onTrigger != null) onTrigger(robotSession.MahuaApi);
            }
        }
        /// <summary>
        /// 间隔时间触发模式
        /// </summary>
        /// <param name="s"></param>
        public BotTask(int s) {
            this.timer.AutoReset = true;
            this.timer.Enabled = true;
            this.timer.Elapsed += run1;
            this.type = 2;
            this.s2 = s;

            this.timer = new System.Timers.Timer(1000*s);

            this.timer.AutoReset = true;
            this.timer.Enabled = true;
            this.setTask();

            //first event

            this.timer.Elapsed += (sender,ev)=> {
                var robotSession = MahuaRobotManager.Instance.CreateSession();
                if (onTrigger != null) onTrigger(robotSession.MahuaApi);
            };
        }


        /// <summary>
        /// 每月指定日期触发执行
        /// </summary>
        /// <param name="d"></param>
        /// <param name="h"></param>
        /// <param name="i"></param>
        /// <param name="s"></param>
        public BotTask(int d, int h, int i, int s) {
            this.d = d;
            this.h = h;
            this.i = i;
            this.s = s;
            this.type = 3;
            this.timer.AutoReset = true;
            this.timer.Enabled = true;
            this.setTask();
            this.timer.Elapsed += run3;
        }
        private void run3(object sender, System.Timers.ElapsedEventArgs ev) {
            int dd = int.Parse(DateTime.Now.ToString("dd"));
            int hh = int.Parse(DateTime.Now.ToString("HH"));
            int ii = int.Parse(DateTime.Now.ToString("mm"));
            int ss = int.Parse(DateTime.Now.ToString("ss"));
            //Log.addLog(String.Format("{0}:{1}:{2}:{3}->{4}:{5}:{6}:{7}", d,h, i, s,dd, hh, ii, ss));
            if (dd==d && hh == h && ii == i && ss == s) {
                var robotSession = MahuaRobotManager.Instance.CreateSession();
                if (onTrigger != null) onTrigger(robotSession.MahuaApi);
            }
        }

    }
}
