using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.acgxt.bot.Utils {
    public class GroupUser {

        private string name;
        private long qqid;
        private string card;
        private int sex;
        private int age;
        private string address;
        private long joinTime;
        private long lastTime;
        private int rule;

        public void setName(string name) {
            this.name = name;
        }
        public string getName() {
            return this.name;
        }
        public void setQQId(long qqid) {
            this.qqid = qqid;
        }
        public long getQQId() {
            return this.qqid;
        }
        public void setCard(string card) {
            this.card = card;
        }
        public string getCard() {
            return this.card;
        }

        public void setSex(int sex) {
            this.sex = sex;
        }
        public int getSex() {
            return this.sex;
        }


        public void setAddress(string address) {
            this.address = address;
        }
        public string getAddress() {
            return this.address;
        }


        public void setAge(int age) {
            this.age = age;
        }
        public int getAge() {
            return this.age;
        }

        public void setJoinTime(long joinTime) {
            this.joinTime = joinTime;
        }
        public long getJoinTime() {
            return this.joinTime;
        }

        public void setLastTime(long lastTime) {
            this.lastTime = lastTime;
        }
        public long getLastTime() {
            return this.lastTime;
        }

        public void setRule(int rule) {
            this.rule = rule;
        }
        public int getRule() {
            return this.rule;
        }

    }
}
