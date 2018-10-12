using Newbe.Mahua;
using Newbe.Mahua.MahuaEvents;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace com.acgxt.bot.MahuaEvents {
    /// <summary>
    /// 菜单点击事件
    /// </summary>
    public class MahuaMenuClickedMahuaEvent: IMahuaMenuClickedMahuaEvent {
        private readonly IMahuaApi api;

        public MahuaMenuClickedMahuaEvent(IMahuaApi mahuaApi) {
            this.api = mahuaApi;
        }

        public void ProcessManhuaMenuClicked(MahuaMenuClickedContext context) {

            if (context.Menu.Id=="setConfig") {

                string fileName = "xtbotManage.exe";
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
                if (!File.Exists(filePath)) {
                    MessageBox.Show("管理工具不存在,无法打开应用");
                    return;
                }
                Process.Start(filePath);










            }
            
        }
    }
}
