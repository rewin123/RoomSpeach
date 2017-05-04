using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace RoomSpeach
{
    class DateSkill : Skill
    {
        public bool MakeCommand(string phrase)
        {
            bool ok = false;
            if(SkillMethods.Include(phrase,"сколько дней до"))
            {
                DateTime now = DateTime.Now;
                string result = "";
                int days = 0;

                ok = true;

                Regex daymonth = new Regex("сколько дней до (\\d+) ([йцукенгшщзхъфывапролджэячсмитьбю]+)");
                Match m = daymonth.Match(phrase);
                if(m.Value != "")
                {
                    DateTime select = new DateTime(now.Year, MonthToId(m.Groups[2].Value), int.Parse(m.Groups[1].Value));
                    days = (select - now).Days;
                }

                result = days.ToString() + " дней";
                Voice.PlayPhrase(result);
            }
            return ok;
        }

        int MonthToId(string month)
        {
            switch(month)
            {
                case "января":
                    return 1;
                case "февраля":
                    return 2;
                case "марта":
                    return 3;
                case "апреля":
                    return 4;
                case "мая":
                    return 5;
                case "июня":
                    return 6;
                case "июля":
                    return 7;
                case "августа":
                    return 8;
                case "сентября":
                    return 9;
                case "октября":
                    return 10;
                case "ноября":
                    return 11;
                case "декабря":
                    return 12;
            }

            return 1;
        }
    }
}
