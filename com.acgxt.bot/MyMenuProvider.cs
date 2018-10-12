using Newbe.Mahua;
using System.Collections.Generic;

namespace com.acgxt.bot {
    public class MyMenuProvider : IMahuaMenuProvider {
        public IEnumerable<MahuaMenu> GetMenus() {
            return new[]
            {
                new MahuaMenu
                {
                    Id = "setConfig",
                    Text = "机器人配置"
                }
            };
        }
    }
}
