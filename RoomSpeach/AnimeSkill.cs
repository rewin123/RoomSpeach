using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace RoomSpeach
{
    class AnimeSkill : Skill
    {
        Regex regexAnimeList = new Regex("<title>([ЙЦУКЕНГШЩЗХЪФЫЁВАПРОЛДЖЭЯЧСМИТЬБЮйцукенгшщзхъёфывапролджэячсмитьбю ]+) \\/");
        int count = 5;
        public bool MakeCommand(string phrase)
        {
            bool ok = false;
            if (SkillMethods.AnyWord(phrase, "аниме"))
            {
                ok = true;
                string webpage = SkillMethods.GetPage("http://online.anidub.com/rss.xml");
                MatchCollection animes = regexAnimeList.Matches(webpage);
                string result = "Последние аниме на Anidub это: ";
                for (int i = 0; i < count && i < animes.Count; i++)
                {
                    result += animes[i].Groups[1].Value + ", ";
                }

                Voice.PlayPhrase(result);
            }
            else if (SkillMethods.Include(phrase, "анидаб"))
            {
                ok = true;
                Process.Start("http://online.anidub.com/");
            }
            return ok;
        }
    }
}
