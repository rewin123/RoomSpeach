using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace RoomSpeach
{
    class WeatherSkill : Skill
    {
        Regex regexState = new Regex("<span class=\"current-weather__comment\">([йцукенгшщзхъфывапролджэячсмитьбю ]+)");
        Regex regexTemp = new Regex("div class=\"current-weather__thermometer current-weather__thermometer_type_now\">(\\W\\d+)");
        Regex regexWind = new Regex("<span class=\"wind-speed\">([\\d,]+)");
        Regex regexDate = new Regex("<span class=\"forecast-brief__item-day\">(\\d+)");
        Regex regexItemState = new Regex("<div class=\"forecast-brief__item-comment\">([йцукенгшщзхъфывапролджэячсмитьбю ]+)");
        Regex regexItemMaxTemp = new Regex("title=\"Максимальная температура днём\">(\\W\\d+)");
        Regex regexItemMinTemp = new Regex("title=\"Минимальная температура ночью\">(\\W\\d+)");
        public bool MakeCommand(string phrase)
        {
            bool ok = false;
            if (SkillMethods.AnyWord(phrase, "погода", "погоду", "погоды"))
            {
                ok = true;
                //погода найдена
                //определяем дату
                if(SkillMethods.Include(phrase,"сегодня"))
                {
                    //погода на сегодня
                    WeatherNow();
                }
                else if(SkillMethods.Include(phrase,"завтра"))
                {
                    //погода на завтра
                    WeatherTomorrow();

                }
                else if(SkillMethods.Include(phrase,"прогноз") || SkillMethods.Include(phrase,"неделю"))
                {
                    //проноз погоды
                    Weather();
                }
            }
            return ok;
        }

        void WeatherNow()
        {
            HttpWebRequest request = HttpWebRequest.CreateHttp("https://yandex.ru/pogoda/malakhovka?name=%D0%9F%D1%80%D0%B8%D1%80%D0%B5%D1%87%D0%BD%D0%B0%D1%8F%20%D1%83%D0%BB%D0%B8%D1%86%D0%B0%2C%20%D0%BF%D0%BE%D1%81%D1%91%D0%BB%D0%BE%D0%BA%20%D0%B3%D0%BE%D1%80%D0%BE%D0%B4%D1%81%D0%BA%D0%BE%D0%B3%D0%BE%20%D1%82%D0%B8%D0%BF%D0%B0%20%D0%9C%D0%B0%D0%BB%D0%B0%D1%85%D0%BE%D0%B2%D0%BA%D0%B0%2C%20%D0%B3%D0%BE%D1%80%D0%BE%D0%B4%D1%81%D0%BA%D0%BE%D0%B9%20%D0%BE%D0%BA%D1%80%D1%83%D0%B3%20%D0%9B%D1%8E%D0%B1%D0%B5%D1%80%D1%86%D1%8B%2C%20%D0%9C%D0%BE%D1%81%D0%BA%D0%BE%D0%B2%D1%81%D0%BA%D0%B0%D1%8F%20%D0%BE%D0%B1%D0%BB%D0%B0%D1%81%D1%82%D1%8C&kind=street&lat=55.649117&lon=37.978866");
            string webpage = new StreamReader(request.GetResponse().GetResponseStream(), Encoding.UTF8).ReadToEnd();

            Match m = regexState.Match(webpage);
            string state = m.Groups[1].Value;
            string temperature = regexTemp.Match(webpage).Groups[1].Value;
            temperature = temperature.Replace("−", "минус ");
            string windSpeed = regexWind.Match(webpage).Groups[1].Value;
            string result = string.Format("На улице сейчас {0}. {1} градусов тепла. Ветер {2} м/с", state, temperature, windSpeed);
            Voice.PlayPhrase(result);
        }

        void Weather()
        {
            HttpWebRequest request = HttpWebRequest.CreateHttp("https://yandex.ru/pogoda/malakhovka?name=%D0%9F%D1%80%D0%B8%D1%80%D0%B5%D1%87%D0%BD%D0%B0%D1%8F%20%D1%83%D0%BB%D0%B8%D1%86%D0%B0%2C%20%D0%BF%D0%BE%D1%81%D1%91%D0%BB%D0%BE%D0%BA%20%D0%B3%D0%BE%D1%80%D0%BE%D0%B4%D1%81%D0%BA%D0%BE%D0%B3%D0%BE%20%D1%82%D0%B8%D0%BF%D0%B0%20%D0%9C%D0%B0%D0%BB%D0%B0%D1%85%D0%BE%D0%B2%D0%BA%D0%B0%2C%20%D0%B3%D0%BE%D1%80%D0%BE%D0%B4%D1%81%D0%BA%D0%BE%D0%B9%20%D0%BE%D0%BA%D1%80%D1%83%D0%B3%20%D0%9B%D1%8E%D0%B1%D0%B5%D1%80%D1%86%D1%8B%2C%20%D0%9C%D0%BE%D1%81%D0%BA%D0%BE%D0%B2%D1%81%D0%BA%D0%B0%D1%8F%20%D0%BE%D0%B1%D0%BB%D0%B0%D1%81%D1%82%D1%8C&kind=street&lat=55.649117&lon=37.978866");
            string webpage = new StreamReader(request.GetResponse().GetResponseStream(), Encoding.UTF8).ReadToEnd();

            DateTime now = DateTime.Now;
            string date = now.Day.ToString();

            MatchCollection collection = regexDate.Matches(webpage);
            MatchCollection states = regexItemState.Matches(webpage);
            MatchCollection maxTemps = regexItemMaxTemp.Matches(webpage);
            MatchCollection minTemps = regexItemMinTemp.Matches(webpage);

            string result = "";

            for (int i = 0; i < minTemps.Count; i++)
            {

                result = string.Format("Погода на {5} {0} {1}. {2}. Максимальная температура днем {3}. Минимальная температура ночью {4}.", collection[i].Groups[1].Value, GetMonth(now.Month), states[i].Groups[1].Value, maxTemps[i].Groups[1].Value.Replace("−", "минус "), minTemps[i].Groups[1].Value.Replace("−", "минус "), GetOnWeekDay(now.DayOfWeek));
                now = now.Add(TimeSpan.FromDays(1));
                Voice.PlayPhraseAsync(result);
            }

        }

        void WeatherTomorrow()
        {
            HttpWebRequest request = HttpWebRequest.CreateHttp("https://yandex.ru/pogoda/malakhovka?name=%D0%9F%D1%80%D0%B8%D1%80%D0%B5%D1%87%D0%BD%D0%B0%D1%8F%20%D1%83%D0%BB%D0%B8%D1%86%D0%B0%2C%20%D0%BF%D0%BE%D1%81%D1%91%D0%BB%D0%BE%D0%BA%20%D0%B3%D0%BE%D1%80%D0%BE%D0%B4%D1%81%D0%BA%D0%BE%D0%B3%D0%BE%20%D1%82%D0%B8%D0%BF%D0%B0%20%D0%9C%D0%B0%D0%BB%D0%B0%D1%85%D0%BE%D0%B2%D0%BA%D0%B0%2C%20%D0%B3%D0%BE%D1%80%D0%BE%D0%B4%D1%81%D0%BA%D0%BE%D0%B9%20%D0%BE%D0%BA%D1%80%D1%83%D0%B3%20%D0%9B%D1%8E%D0%B1%D0%B5%D1%80%D1%86%D1%8B%2C%20%D0%9C%D0%BE%D1%81%D0%BA%D0%BE%D0%B2%D1%81%D0%BA%D0%B0%D1%8F%20%D0%BE%D0%B1%D0%BB%D0%B0%D1%81%D1%82%D1%8C&kind=street&lat=55.649117&lon=37.978866");
            string webpage = new StreamReader(request.GetResponse().GetResponseStream(), Encoding.UTF8).ReadToEnd();

            DateTime now = DateTime.Now;
            string date = now.Day.ToString();

            MatchCollection collection = regexDate.Matches(webpage);
            MatchCollection states = regexItemState.Matches(webpage);
            MatchCollection maxTemps = regexItemMaxTemp.Matches(webpage);
            MatchCollection minTemps = regexItemMinTemp.Matches(webpage);

            string result = "";
            Task task = null;



            int i = 1;
            now = now.Add(TimeSpan.FromDays(1));
            result = string.Format("Погода на {5} {0} {1}. {2}. Максимальная температура днем {3}. Минимальная температура ночью {4}.", collection[i].Groups[1].Value, GetMonth(now.Month), states[i].Groups[1].Value, maxTemps[i].Groups[1].Value.Replace("−", "минус "), minTemps[i].Groups[1].Value.Replace("−", "минус "), GetOnWeekDay(now.DayOfWeek));

            Voice.PlayPhrase(result);
        }

        string GetOnWeekDay(DayOfWeek weekday)
        {
            if (weekday == DayOfWeek.Sunday)
                return onweekdays[6];
            else return onweekdays[(int)weekday - 1];
        }

        string GetMonth(int id)
        {
            switch (id)
            {
                case 1:
                    return "января";
                case 2:
                    return "февраля";
            }

            return "неизвестный месяц";
        }

        string[] weekdays = new string[] { "понедельник", "вторник", "среда", "четверг", "пятница", "суббота", "воскресенье" };
        string[] onweekdays = new string[] { "понедельник", "вторник", "среду", "четверг", "пятницу", "субботу", "воскресенье" };
    }
}
