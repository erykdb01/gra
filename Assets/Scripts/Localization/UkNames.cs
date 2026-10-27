// Ukrainian forms of the proper names used by the game (people, towns, stations), with a
// Polish -> Ukrainian transliteration for any name that is not in the table.
using System.Collections.Generic;
using System.Text;

public static class UkNames
{
    static Dictionary<string, string> map;

    static void Build()
    {
        map = new Dictionary<string, string>(256);
        string[] p =
        {
            // male first names
            "Jan|Ян", "Piotr|Пйотр", "Tadeusz|Тадеуш", "Stanisław|Станіслав", "Marek|Марек", "Andrzej|Анджей", "Henryk|Генрик", "Zbigniew|Збігнев",
            "Józef|Юзеф", "Władysław|Владислав", "Kazimierz|Казімеж", "Ryszard|Ришард", "Edward|Едвард", "Leon|Леон", "Bogdan|Богдан", "Jerzy|Єжи",
            "Witold|Вітольд", "Ludwik|Людвік", "Roman|Роман", "Feliks|Фелікс", "Adam|Адам",
            // female first names
            "Maria|Марія", "Anna|Анна", "Krystyna|Христина", "Ewa|Ева", "Halina|Галина", "Barbara|Барбара", "Zofia|Зофія", "Irena|Ірена",
            "Helena|Гелена", "Danuta|Данута", "Janina|Яніна", "Teresa|Тереза", "Wanda|Ванда", "Jadwiga|Ядвіга", "Stefania|Стефанія", "Lucyna|Луцина",
            "Alicja|Аліція", "Genowefa|Геновефа", "Bożena|Божена", "Renata|Рената", "Lena|Лена",
            // surnames (male and female forms)
            "Kowalski|Ковальський", "Kowalska|Ковальська", "Nowak|Новак", "Wiśniewski|Вишневський", "Wiśniewska|Вишневська", "Wójcik|Войцік",
            "Kamiński|Камінський", "Kamińska|Камінська", "Lewandowski|Левандовський", "Lewandowska|Левандовська", "Zieliński|Зелінський", "Zielińska|Зелінська",
            "Szymański|Шиманський", "Szymańska|Шиманська", "Dąbrowski|Домбровський", "Dąbrowska|Домбровська", "Kozłowski|Козловський", "Kozłowska|Козловська",
            "Jankowski|Янковський", "Jankowska|Янковська", "Mazur|Мазур", "Krawczyk|Кравчик", "Piotrowski|Пйотровський", "Piotrowska|Пйотровська",
            "Grabowski|Грабовський", "Grabowska|Грабовська", "Pawłowski|Павловський", "Pawłowska|Павловська", "Michalski|Міхальський", "Michalska|Міхальська",
            "Zając|Заяць", "Król|Круль", "Wieczorek|Вечорек",
            "Wolska|Вольська", "Sowa|Сова", "Kruk|Крук", "Wrona|Врона", "Ostoja|Остоя",
            // towns
            "Lublin|Люблін", "Warszawa|Варшава", "Kraków|Краків", "Gdańsk|Гданськ", "Poznań|Познань", "Łódź|Лодзь", "Radom|Радом", "Kielce|Кельці",
            "Toruń|Торунь", "Olsztyn|Ольштин", "Zamość|Замостя", "Przemyśl|Перемишль", "Białystok|Білосток", "Opole|Ополе", "Rzeszów|Жешув", "Sandomierz|Сандомир",
            // stations
            "Szara Wola|Сіра Воля", "Zimna Woda|Холодна Вода", "Dolne Mosty|Нижні Мости", "Czarny Bór|Чорний Бір", "Przełaj|Пшелай", "Ostrowiec|Островець",
            "Kamienna Góra|Кам’яна Гора", "Biała Ława|Біла Лава", "Stare Sady|Старі Сади", "Krzyżówka|Хрестівка", "Głuchy Las|Глухий Ліс", "Nowa Zagroda|Нова Загорода"
        };
        for (int i = 0; i < p.Length; i++)
        {
            int k = p[i].IndexOf('|');
            map[p[i].Substring(0, k)] = p[i].Substring(k + 1);
        }
    }

    // Ukrainian form of a name: every word is looked up, otherwise transliterated.
    public static string Get(string pl)
    {
        if (string.IsNullOrEmpty(pl)) return pl;
        if (map == null) Build();
        string whole;
        if (map.TryGetValue(pl, out whole)) return whole;
        var sb = new StringBuilder();
        string[] words = pl.Split(' ');
        for (int i = 0; i < words.Length; i++)
        {
            if (i > 0) sb.Append(' ');
            string w = words[i], uk;
            if (map.TryGetValue(w, out uk)) sb.Append(uk);
            else sb.Append(Translit(w));
        }
        return sb.ToString();
    }

    static bool IsVowel(char c) { return "aeiouyąęó".IndexOf(char.ToLowerInvariant(c)) >= 0; }

    // Rough Polish -> Ukrainian transliteration (fallback only).
    public static string Translit(string s)
    {
        var sb = new StringBuilder();
        string t = s;
        for (int i = 0; i < t.Length; i++)
        {
            char ch = t[i];
            bool up = char.IsUpper(ch);
            string low = char.ToLowerInvariant(ch).ToString();
            string next = i + 1 < t.Length ? char.ToLowerInvariant(t[i + 1]).ToString() : "";
            char prev = i > 0 ? char.ToLowerInvariant(t[i - 1]) : ' ';
            string o = null;
            int skip = 0;
            string two = low + next;
            switch (two)
            {
                case "sz": o = "ш"; skip = 1; break;
                case "cz": o = "ч"; skip = 1; break;
                case "ch": o = "х"; skip = 1; break;
                case "rz": o = "ж"; skip = 1; break;
                case "dz": o = "дз"; skip = 1; break;
                case "dż": o = "дж"; skip = 1; break;
                case "dź": o = "дзь"; skip = 1; break;
                case "ia": o = "я"; skip = 1; break;
                case "ie": o = "є"; skip = 1; break;
                case "io": o = "йо"; skip = 1; break;
                case "iu": o = "ю"; skip = 1; break;
            }
            if (o == null)
            {
                switch (low)
                {
                    case "a": o = "а"; break; case "b": o = "б"; break; case "c": o = "ц"; break; case "ć": o = "ць"; break;
                    case "d": o = "д"; break; case "e": o = "е"; break; case "ę": o = "ен"; break; case "f": o = "ф"; break;
                    case "g": o = "г"; break; case "h": o = "г"; break; case "i": o = "і"; break; case "j": o = "й"; break;
                    case "k": o = "к"; break; case "l": o = "л"; break; case "ł": o = "л"; break; case "m": o = "м"; break;
                    case "n": o = "н"; break; case "ń": o = "нь"; break; case "o": o = "о"; break; case "ó": o = "у"; break;
                    case "p": o = "п"; break; case "q": o = "к"; break; case "r": o = "р"; break; case "s": o = "с"; break;
                    case "ś": o = "сь"; break; case "t": o = "т"; break; case "u": o = "у"; break; case "v": o = "в"; break;
                    case "w": o = "в"; break; case "x": o = "кс"; break; case "y": o = "и"; break; case "z": o = "з"; break;
                    case "ź": o = "зь"; break; case "ż": o = "ж"; break; case "ą": o = "он"; break;
                    default: o = low; break;
                }
            }
            i += skip;
            if (up && o.Length > 0) o = char.ToUpperInvariant(o[0]) + o.Substring(1);
            sb.Append(o);
        }
        return sb.ToString();
    }
}
